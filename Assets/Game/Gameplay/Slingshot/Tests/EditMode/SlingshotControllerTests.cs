using System;
using System.Collections.Generic;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Slingshot.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class SlingshotControllerTests
{
    private readonly List<string> _observations = new();
    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private FakeSlingshotConfig _config;
    private FakeUnityInput _input;
    private FakeGameplayStateService _stateService;
    private FakeSlingshotView _view;
    private FakeSlingshotInputProjector _projector;
    private FakeHeldLaunchTarget _heldLaunchTarget;
    private FakeSlingshotBandShapeProvider _bandShapeProvider;
    private FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private FakeTime _clock;

    [SetUp]
    public void OnSetUp()
    {
        _observations.Clear();
        _preLaunchStateId = CreateStateId("Pre-Launch");
        _runningStateId = CreateStateId("Running");

        _config = new FakeSlingshotConfig
        {
            TouchTargetRadiusPixels = 30f,
            MinimumPullDistance = 0.25f,
            MaximumPullDistance = 2f,
            MaximumLateralPull = 1.25f,
            MaximumLaunchAngleDegrees = 35f,
            MinimumLaunchSpeed = 4f,
            MaximumLaunchSpeed = 12f,
            LaunchSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
            LaunchUpSpeed = 1.5f,
            BandContactPadding = 0.05f,
            BandSilhouetteSampleCount = 8,
            BandWrapSampleCount = 5,
            BandRecoilDuration = 0.2f,
            BandRecoilCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
        };
        _input = new FakeUnityInput(_observations);
        _stateService = new FakeGameplayStateService(_runningStateId);
        _view = new FakeSlingshotView(_observations);
        _projector = new FakeSlingshotInputProjector();
        _heldLaunchTarget = new FakeHeldLaunchTarget(_observations);
        _bandShapeProvider = new FakeSlingshotBandShapeProvider(_observations);
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();
        _clock = new FakeTime { DeltaTime = 0.1f };
        ConfigureRestBandScreenProjection();
    }

    [TearDown]
    public void OnTearDown()
    {
        UnityEngine.Object.DestroyImmediate(_preLaunchStateId);
        UnityEngine.Object.DestroyImmediate(_runningStateId);
    }

    [Test]
    public void Initialize_CurrentStateIsPreLaunch_EnablesInputBeforeCaptureIdle()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();

        Assert.That(_observations, Is.EqualTo(new[] { "input-enable", "target-position", "view-capture-idle" }));
        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(_view.Geometry.RestPoint));
        AssertRestBandShape(_view.LastBandShape);
    }

    [Test]
    public void GameplayStateChanged_EnteringPreLaunch_EnablesInputBeforeCaptureIdle()
    {
        using var controller = CreateInitializedController();
        _observations.Clear();

        _stateService.ChangeTo(_preLaunchStateId);

        Assert.That(_observations, Is.EqualTo(new[] { "input-enable", "target-position", "view-capture-idle" }));
        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
    }

    [Test]
    public void GameplayStateChanged_LeavingPreLaunch_CancelsPullAndDisablesInputAfterInactiveIdle()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();

        _stateService.ChangeTo(_runningStateId);

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-inactive-idle", "input-disable" }));
        Assert.That(_input.ActiveHandleCount, Is.Zero);
    }

    [Test]
    public void Dispose_WithActiveHandle_UnsubscribesAndDisposesHandleWithoutDrivingView()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        var controller = CreateInitializedController();
        _observations.Clear();

        ((IDisposable)controller).Dispose();
        _stateService.ChangeTo(_runningStateId);
        _input.Press(1, new Vector2(50f, 20f));

        Assert.That(_observations, Is.EqualTo(new[] { "input-disable" }));
        Assert.That(_input.ActiveHandleCount, Is.Zero);
    }

    [Test]
    public void PointerPressed_OutsideBandTouchTarget_Ignored()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        _observations.Clear();

        _input.Press(1, new Vector2(50f, 40f));

        Assert.That(_view.ActivePullVisuals, Is.Empty);
        Assert.That(_observations, Is.Empty);
    }

    [Test]
    public void PointerPressed_InsideBandTouchTarget_StartsActivePull()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        _observations.Clear();

        StartActivePull(1);

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "band-shape", "view-active-pull" }));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(_view.Geometry.RestPoint));
        Assert.That(_bandShapeProvider.Queries[^1].PullPoint, Is.EqualTo(_view.Geometry.RestPoint));
        Assert.That(_view.LastActivePullVisual.TouchIndicatorScreenPosition, Is.EqualTo(new Vector2(50f, 0f)));
    }

    [Test]
    public void PointerPressed_AtRestPointAwayFromAnchorChord_StartsActivePull()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        _projector.SetWorldToScreen(_view.Geometry.RestPoint, new Vector2(50f, 40f));
        _projector.SetScreenToWorld(new Vector2(50f, 40f), _view.Geometry.RestPoint);
        _observations.Clear();

        _input.Press(1, new Vector2(50f, 40f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "band-shape", "view-active-pull" }));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(_view.Geometry.RestPoint));
        Assert.That(_view.LastActivePullVisual.TouchIndicatorScreenPosition, Is.EqualTo(new Vector2(50f, 40f)));
    }

    [Test]
    public void PointerEvents_SecondPointerWhileActive_Ignored()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        _projector.SetScreenToWorld(new Vector2(60f, 30f), new Vector3(0f, 0f, -1f));
        _projector.SetWorldToScreen(new Vector3(0f, 0f, -1f), new Vector2(60f, 0f));
        _projector.SetScreenToWorld(new Vector2(70f, 30f), new Vector3(0.5f, 0f, -1f));
        _projector.SetWorldToScreen(new Vector3(0.5f, 0f, -1f), new Vector2(70f, 0f));

        _input.Move(2, new Vector2(60f, 30f));
        _input.Move(1, new Vector2(70f, 30f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "band-shape", "view-active-pull" }));
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.EqualTo(0.5f));
    }

    [Test]
    public void PointerMoved_ProjectionFailure_CancelsPullToCaptureIdle()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        _projector.SetScreenProjectionFailure(new Vector2(65f, 35f));

        _input.Move(1, new Vector2(65f, 35f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-capture-idle" }));
    }

    [Test]
    public void PointerMoved_ForwardDisplacement_ClampsBackwardDistanceToZero()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        _projector.SetScreenToWorld(new Vector2(50f, 60f), new Vector3(0f, 0f, 2f));
        _projector.SetWorldToScreen(_view.Geometry.RestPoint, new Vector2(50f, 0f));

        _input.Move(1, new Vector2(50f, 60f));

        Assert.That(_view.LastActivePullVisual.PullDistance, Is.Zero);
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.Zero);
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(_view.Geometry.RestPoint));
        Assert.That(_bandShapeProvider.Queries[^1].PullPoint, Is.EqualTo(_view.Geometry.RestPoint));
        Assert.That(_view.LastActivePullVisual.TouchIndicatorScreenPosition, Is.EqualTo(new Vector2(50f, 0f)));
    }

    [Test]
    public void PointerMoved_BackwardAndLateralDisplacement_ClampsIndependently()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        var rawProjectedPoint = new Vector3(4f, 0f, -5f);
        var clampedProjectedPoint = new Vector3(1.25f, 0f, -2f);
        _projector.SetScreenToWorld(new Vector2(90f, 80f), rawProjectedPoint);
        _projector.SetWorldToScreen(clampedProjectedPoint, new Vector2(80f, 15f));

        _input.Move(1, new Vector2(90f, 80f));

        Assert.That(_view.LastActivePullVisual.PullDistance, Is.EqualTo(2f));
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.EqualTo(1.25f));
        Assert.That(_view.LastActivePullVisual.NormalizedPull, Is.EqualTo(1f));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(clampedProjectedPoint));
        Assert.That(_bandShapeProvider.Queries[^1].PullPoint, Is.EqualTo(clampedProjectedPoint));

        AssertBandShapeEquals(_view.LastActivePullVisual.BandShape, _bandShapeProvider.ShapePoints);
        Assert.That(_view.LastActivePullVisual.TouchIndicatorScreenPosition, Is.EqualTo(new Vector2(80f, 15f)));
    }

    [Test]
    public void PointerMoved_BandShapeSolveFailureAfterValidShape_KeepsLastValidShapeVisible()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        var rawProjectedPoint = new Vector3(0f, 0f, -1f);
        _projector.SetScreenToWorld(new Vector2(55f, 45f), rawProjectedPoint);
        _projector.SetWorldToScreen(rawProjectedPoint, new Vector2(55f, 10f));
        _bandShapeProvider.ShouldFail = true;

        _input.Move(1, new Vector2(55f, 45f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "band-shape", "view-active-pull" }));
        AssertBandShapeEquals(_view.LastActivePullVisual.BandShape, _bandShapeProvider.ShapePoints);
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(rawProjectedPoint));
    }

    [Test]
    public void LaunchApplied_RecoilTick_UsesLiveBandShapeProviderInsteadOfInterpolatingThroughRest()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(75f, 80f);
        var finalPullPoint = new Vector3(0.5f, 0f, -1f);
        _projector.SetScreenToWorld(releaseScreenPosition, finalPullPoint);
        _projector.SetWorldToScreen(finalPullPoint, new Vector2(75f, 10f));
        SlingshotLaunchRequest launchRequest = default;
        var wasLaunchRequested = false;

        controller.LaunchRequested += request =>
        {
            launchRequest = request;
            wasLaunchRequested = true;
        };

        _input.Release(1, releaseScreenPosition);
        _observations.Clear();
        _launchAppliedNotifier.Apply(launchRequest);
        ((ITickable)controller).Tick();

        var expectedRecoilPullPoint = Vector3.Lerp(finalPullPoint, _view.Geometry.RestPoint, 0.5f);
        Assert.That(wasLaunchRequested, Is.True);
        Assert.That(_observations, Is.EqualTo(new[] { "band-shape", "view-loaded-release" }));
        Assert.That(_bandShapeProvider.Queries[^1].PullPoint, Is.EqualTo(expectedRecoilPullPoint));
        AssertBandShapeEquals(_view.LastBandShape, _bandShapeProvider.ShapePoints);
    }

    [Test]
    public void PointerReleased_ActivePull_ReturnsToCaptureIdleWithoutLaunch()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();

        _input.Release(1, new Vector2(60f, 20f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "band-shape", "target-position", "view-capture-idle" }));
    }

    [Test]
    public void PointerCanceled_ActivePull_ReturnsToCaptureIdleWithoutLaunch()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();

        _input.Cancel(1, new Vector2(60f, 20f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-capture-idle" }));
    }

    private SlingshotController CreateInitializedController()
    {
        var controller = new SlingshotController(_input, _stateService, _view, _projector, _heldLaunchTarget, _bandShapeProvider,
            _launchAppliedNotifier, _clock, _config, _preLaunchStateId);
        ((IInitializable)controller).Initialize();
        return controller;
    }

    private void StartActivePull(int pointerId)
    {
        _projector.SetScreenToWorld(new Vector2(50f, 20f), _view.Geometry.RestPoint);
        _projector.SetWorldToScreen(_view.Geometry.RestPoint, new Vector2(50f, 0f));
        _input.Press(pointerId, new Vector2(50f, 20f));
    }

    private void ConfigureRestBandScreenProjection()
    {
        _projector.SetWorldToScreen(_view.Geometry.LeftAnchorPosition, new Vector2(0f, 0f));
        _projector.SetWorldToScreen(_view.Geometry.RightAnchorPosition, new Vector2(100f, 0f));
        _projector.SetWorldToScreen(_view.Geometry.RestPoint, new Vector2(50f, 0f));
    }

    private GameplayStateId CreateStateId(string name)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = name;
        return stateId;
    }

    private void AssertBandShapeEquals(SlingshotBandShape shape, params Vector3[] expectedPoints)
    {
        Assert.That(shape.Points.Count, Is.EqualTo(expectedPoints.Length));

        for (var i = 0; i < expectedPoints.Length; i += 1)
        {
            Assert.That(shape.Points[i], Is.EqualTo(expectedPoints[i]));
        }
    }

    private void AssertRestBandShape(SlingshotBandShape shape)
    {
        Assert.That(shape.Points.Count, Is.EqualTo(_bandShapeProvider.BandShapePointCount));
        Assert.That(shape.Points[0], Is.EqualTo(_view.Geometry.LeftAnchorPosition));
        Assert.That(shape.Points[(shape.Points.Count - 1) / 2], Is.EqualTo(_view.Geometry.RestPoint));
        Assert.That(shape.Points[^1], Is.EqualTo(_view.Geometry.RightAnchorPosition));
    }
}
