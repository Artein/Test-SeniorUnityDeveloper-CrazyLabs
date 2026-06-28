using System;
using System.Collections.Generic;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Slingshot.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class SlingshotControllerTests
{
    private readonly List<string> _observations = new();
    private FakeSlingshotConfig _config;
    private FakeUnityInput _input;
    private FakeSlingshotView _view;
    private FakeSlingshotInputProjector _projector;
    private FakeLaunchTarget _launchTarget;
    private FakeHeldLaunchTarget _heldLaunchTarget;
    private FakeSlingshotBandShapeProvider _bandShapeProvider;
    private FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private FakeTime _clock;

    [SetUp]
    public void OnSetUp()
    {
        _observations.Clear();

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
        _view = new FakeSlingshotView(_observations);
        _projector = new FakeSlingshotInputProjector();
        _launchTarget = new FakeLaunchTarget(_observations);
        _heldLaunchTarget = new FakeHeldLaunchTarget(_observations);
        _bandShapeProvider = new FakeSlingshotBandShapeProvider(_observations);
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();
        _clock = new FakeTime { DeltaTime = 0.1f };
        ConfigureRestBandScreenProjection();
    }

    [Test]
    public void EnableCapture_AfterInitialize_HoldsTargetAndEnablesInputBeforeCaptureIdle()
    {
        using var controller = CreateInitializedController();

        Assert.That(_observations, Is.EqualTo(new[] { "input-enable", "target-hold", "target-position", "view-capture-idle" }));
        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
        Assert.That(_launchTarget.HoldCallCount, Is.EqualTo(1));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(_view.Geometry.RestPoint));
        Assert.That(_bandShapeProvider.Queries, Is.Empty);
        AssertBandShapeEqualsRawTwoSpan(_view.LastBandShape, _view.Geometry.RestPoint);
    }

    [Test]
    public void EnableCapture_WhenAlreadyEnabled_DoesNothing()
    {
        using var controller = CreateInitializedController();
        _observations.Clear();

        ((ISlingshotCapture)controller).EnableCapture();

        Assert.That(_observations, Is.Empty);
        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
        Assert.That(_launchTarget.HoldCallCount, Is.EqualTo(1));
    }

    [Test]
    public void DisableCapture_WhenEnabled_CancelsPullAndDisablesInputAfterInactiveIdle()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();

        ((ISlingshotCapture)controller).DisableCapture();

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-inactive-idle", "input-disable" }));
        Assert.That(_input.ActiveHandleCount, Is.Zero);
    }

    [Test]
    public void Dispose_WithActiveHandle_UnsubscribesAndDisposesHandleWithoutDrivingView()
    {
        var controller = CreateInitializedController();
        _observations.Clear();

        ((IDisposable)controller).Dispose();
        _input.Press(1, new Vector2(50f, 20f));

        Assert.That(_observations, Is.EqualTo(new[] { "input-disable" }));
        Assert.That(_input.ActiveHandleCount, Is.Zero);
    }

    [Test]
    public void EnableCapture_BeforeInitialize_ThrowsInvalidOperationException()
    {
        var controller = CreateController();

        Assert.That(
            () => ((ISlingshotCapture)controller).EnableCapture(),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void DisableCapture_BeforeInitialize_ThrowsInvalidOperationException()
    {
        var controller = CreateController();

        Assert.That(
            () => ((ISlingshotCapture)controller).DisableCapture(),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void EnableCapture_AfterDispose_ThrowsObjectDisposedException()
    {
        var controller = CreateInitializedController();

        ((IDisposable)controller).Dispose();

        Assert.That(
            () => ((ISlingshotCapture)controller).EnableCapture(),
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void DisableCapture_AfterDispose_ThrowsObjectDisposedException()
    {
        var controller = CreateInitializedController();

        ((IDisposable)controller).Dispose();

        Assert.That(
            () => ((ISlingshotCapture)controller).DisableCapture(),
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void PointerPressed_OutsideBandTouchTarget_Ignored()
    {
        using var controller = CreateInitializedController();
        _observations.Clear();

        _input.Press(1, new Vector2(50f, 40f));

        Assert.That(_view.ActivePullVisuals, Is.Empty);
        Assert.That(_observations, Is.Empty);
    }

    [Test]
    public void PointerPressed_InsideBandTouchTarget_StartsActivePull()
    {
        using var controller = CreateInitializedController();
        _observations.Clear();

        StartActivePull(1);

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-active-pull" }));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(_view.Geometry.RestPoint));
        Assert.That(_bandShapeProvider.Queries, Is.Empty);

        AssertBandShapeEqualsRawTwoSpan(
            _view.LastActivePullVisual.BandShape,
            GetExpectedSimpleBandVisualCenterPoint(_view.Geometry.RestPoint));
        Assert.That(_view.LastActivePullVisual.TouchIndicatorScreenPosition, Is.EqualTo(new Vector2(50f, 0f)));
    }

    [Test]
    public void PointerPressed_AtRestPointAwayFromAnchorChord_StartsActivePull()
    {
        using var controller = CreateInitializedController();
        _projector.SetWorldToScreen(_view.Geometry.RestPoint, new Vector2(50f, 40f));
        _projector.SetScreenToWorld(new Vector2(50f, 40f), _view.Geometry.RestPoint);
        _observations.Clear();

        _input.Press(1, new Vector2(50f, 40f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-active-pull" }));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(_view.Geometry.RestPoint));
        Assert.That(_bandShapeProvider.Queries, Is.Empty);

        AssertBandShapeEqualsRawTwoSpan(
            _view.LastActivePullVisual.BandShape,
            GetExpectedSimpleBandVisualCenterPoint(_view.Geometry.RestPoint));
        Assert.That(_view.LastActivePullVisual.TouchIndicatorScreenPosition, Is.EqualTo(new Vector2(50f, 40f)));
    }

    [Test]
    public void PointerEvents_SecondPointerWhileActive_Ignored()
    {
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
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        _projector.SetScreenToWorld(new Vector2(50f, 60f), new Vector3(0f, 0f, 2f));
        _projector.SetWorldToScreen(_view.Geometry.RestPoint, new Vector2(50f, 0f));

        _input.Move(1, new Vector2(50f, 60f));

        Assert.That(_view.LastActivePullVisual.PullDistance, Is.Zero);
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.Zero);
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(_view.Geometry.RestPoint));
        Assert.That(_bandShapeProvider.Queries, Is.Empty);

        AssertBandShapeEqualsRawTwoSpan(
            _view.LastActivePullVisual.BandShape,
            GetExpectedSimpleBandVisualCenterPoint(_view.Geometry.RestPoint));
        Assert.That(_view.LastActivePullVisual.TouchIndicatorScreenPosition, Is.EqualTo(new Vector2(50f, 0f)));
    }

    [Test]
    public void PointerMoved_NearRestPull_UsesCleanTwoSpanBandShape()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        var nearRestPullPoint = new Vector3(0.02f, 0f, -0.02f);
        _projector.SetScreenToWorld(new Vector2(52f, 22f), nearRestPullPoint);
        _projector.SetWorldToScreen(nearRestPullPoint, new Vector2(52f, 2f));

        _input.Move(1, new Vector2(52f, 22f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-active-pull" }));
        Assert.That(_bandShapeProvider.Queries, Is.Empty);
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(nearRestPullPoint));
        AssertBandShapeEqualsRawTwoSpan(_view.LastActivePullVisual.BandShape, GetExpectedSimpleBandVisualCenterPoint(nearRestPullPoint));
    }

    [Test]
    public void PointerMoved_TinyLateralPullBelowTautMagnitude_UsesCleanTwoSpanBandShape()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        var tinyLateralPullPoint = new Vector3(0.03f, 0f, -0.02f);
        var expectedPullPoint = GetExpectedClampedPullPoint(tinyLateralPullPoint);
        _projector.SetScreenToWorld(new Vector2(53f, 22f), tinyLateralPullPoint);
        _projector.SetWorldToScreen(expectedPullPoint, new Vector2(53f, 2f));

        _input.Move(1, new Vector2(53f, 22f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-active-pull" }));
        Assert.That(_bandShapeProvider.Queries, Is.Empty);
        Assert.That(_view.LastActivePullVisual.PullDistance, Is.EqualTo(0.02f).Within(0.0001f));
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.EqualTo(expectedPullPoint.x).Within(0.0001f));
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.GreaterThan(0f));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(expectedPullPoint));
        AssertBandShapeEqualsRawTwoSpan(_view.LastActivePullVisual.BandShape, GetExpectedSimpleBandVisualCenterPoint(expectedPullPoint));
    }

    [Test]
    public void PointerMoved_ShallowBackwardLateralPullBelowMinimumTautDistance_UsesCleanTwoSpanBandShape()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        var shallowLateralPullPoint = new Vector3(1f, 0f, -0.2f);
        var expectedPullPoint = GetExpectedClampedPullPoint(shallowLateralPullPoint);
        _projector.SetScreenToWorld(new Vector2(85f, 35f), shallowLateralPullPoint);
        _projector.SetWorldToScreen(expectedPullPoint, new Vector2(65f, 5f));

        _input.Move(1, new Vector2(85f, 35f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-active-pull" }));
        Assert.That(_bandShapeProvider.Queries, Is.Empty);
        Assert.That(_view.LastActivePullVisual.PullDistance, Is.EqualTo(0.2f).Within(0.0001f));
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.EqualTo(expectedPullPoint.x).Within(0.0001f));
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.GreaterThan(0.05f));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(expectedPullPoint));
        AssertBandShapeEqualsRawTwoSpan(_view.LastActivePullVisual.BandShape, GetExpectedSimpleBandVisualCenterPoint(expectedPullPoint));
    }

    [Test]
    public void PointerMoved_ShallowBackwardMaxLateralPullAtRampThreshold_ClampsLateralAndUsesCleanTwoSpanBandShape()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        var shallowLateralPullPoint = new Vector3(1f, 0f, -0.35f);
        var expectedPullPoint = GetExpectedClampedPullPoint(shallowLateralPullPoint);
        _projector.SetScreenToWorld(new Vector2(100f, 35f), shallowLateralPullPoint);
        _projector.SetWorldToScreen(expectedPullPoint, new Vector2(75f, 5f));

        _input.Move(1, new Vector2(100f, 35f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-active-pull" }));
        Assert.That(_bandShapeProvider.Queries, Is.Empty);
        Assert.That(_view.LastActivePullVisual.PullDistance, Is.EqualTo(0.35f).Within(0.0001f));
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.EqualTo(expectedPullPoint.x).Within(0.0001f));
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.LessThan(1f));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(expectedPullPoint));
        AssertBandShapeEqualsRawTwoSpan(_view.LastActivePullVisual.BandShape, GetExpectedSimpleBandVisualCenterPoint(expectedPullPoint));
    }

    [Test]
    public void PointerMoved_BackwardLateralPullBeyondRampThreshold_UsesTautBandShape()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        var lateralPullPoint = new Vector3(1f, 0f, -0.5f);
        var expectedPullPoint = GetExpectedClampedPullPoint(lateralPullPoint);
        _projector.SetScreenToWorld(new Vector2(100f, 50f), lateralPullPoint);
        _projector.SetWorldToScreen(expectedPullPoint, new Vector2(75f, 8f));

        _input.Move(1, new Vector2(100f, 50f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "band-shape", "view-active-pull" }));
        Assert.That(_bandShapeProvider.Queries, Has.Count.EqualTo(1));
        Assert.That(_bandShapeProvider.Queries[^1].PullPoint, Is.EqualTo(expectedPullPoint));
        Assert.That(_view.LastActivePullVisual.PullDistance, Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.EqualTo(expectedPullPoint.x).Within(0.0001f));
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(expectedPullPoint));
        AssertBandShapeEquals(_view.LastActivePullVisual.BandShape, _bandShapeProvider.ShapePoints);
    }

    [Test]
    public void PointerMoved_BackwardAndLateralDisplacement_ClampsLateralOffsetToAnchorSpan()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        var rawProjectedPoint = new Vector3(4f, 0f, -5f);
        var clampedProjectedPoint = new Vector3(1f, 0f, -2f);
        _projector.SetScreenToWorld(new Vector2(90f, 80f), rawProjectedPoint);
        _projector.SetWorldToScreen(clampedProjectedPoint, new Vector2(80f, 15f));

        _input.Move(1, new Vector2(90f, 80f));

        Assert.That(_view.LastActivePullVisual.PullDistance, Is.EqualTo(2f));
        Assert.That(_view.LastActivePullVisual.PullOffset, Is.EqualTo(1f));
        Assert.That(_view.LastActivePullVisual.NormalizedPull, Is.EqualTo(1f));
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(clampedProjectedPoint));
        Assert.That(_bandShapeProvider.Queries[^1].PullPoint, Is.EqualTo(clampedProjectedPoint));

        AssertBandShapeEquals(_view.LastActivePullVisual.BandShape, _bandShapeProvider.ShapePoints);
        Assert.That(_view.LastActivePullVisual.TouchIndicatorScreenPosition, Is.EqualTo(new Vector2(80f, 15f)));
    }

    [Test]
    public void PointerMoved_BandShapeSolveFailureAfterValidShape_KeepsLastValidShapeVisible()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();
        var validProjectedPoint = new Vector3(0f, 0f, -1f);
        _projector.SetScreenToWorld(new Vector2(55f, 45f), validProjectedPoint);
        _projector.SetWorldToScreen(validProjectedPoint, new Vector2(55f, 10f));

        _input.Move(1, new Vector2(55f, 45f));

        var expectedLastValidShape = (Vector3[])_bandShapeProvider.ShapePoints.Clone();
        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "band-shape", "view-active-pull" }));
        AssertBandShapeEquals(_view.LastActivePullVisual.BandShape, expectedLastValidShape);
        _observations.Clear();

        var failedProjectedPoint = new Vector3(0.25f, 0f, -1.25f);
        _projector.SetScreenToWorld(new Vector2(65f, 55f), failedProjectedPoint);
        _projector.SetWorldToScreen(failedProjectedPoint, new Vector2(65f, 12f));
        _bandShapeProvider.ShouldFail = true;

        _input.Move(1, new Vector2(65f, 55f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "band-shape", "view-active-pull" }));
        AssertBandShapeEquals(_view.LastActivePullVisual.BandShape, expectedLastValidShape);
        Assert.That(_heldLaunchTarget.HeldPositions[^1], Is.EqualTo(failedProjectedPoint));
    }

    [Test]
    public void LaunchApplied_RecoilClearanceBlocked_UsesLiveBandShapeProviderInsteadOfInterpolatingThroughRest()
    {
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
        _bandShapeProvider.IsBandShapeClear = false;
        _launchAppliedNotifier.Apply(launchRequest);
        ((ITickable)controller).Tick();

        var expectedRecoilPullPoint = Vector3.Lerp(finalPullPoint, _view.Geometry.RestPoint, 0.5f);
        Assert.That(wasLaunchRequested, Is.True);
        Assert.That(_observations, Is.EqualTo(new[] { "band-clearance", "band-depth-span", "band-shape", "view-loaded-release" }));
        Assert.That(_bandShapeProvider.Queries[^1].PullPoint, Is.EqualTo(expectedRecoilPullPoint));
        AssertBandShapeEquals(_view.LastBandShape, _bandShapeProvider.ShapePoints);
    }

    [Test]
    public void LaunchApplied_RecoilClearanceBlocked_WhenRecoilPullPointIsNearRest_UsesLiveBandShapeProvider()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(55f, 45f);
        var finalPullPoint = new Vector3(0.15f, 0f, -0.3f);
        _projector.SetScreenToWorld(releaseScreenPosition, finalPullPoint);
        _projector.SetWorldToScreen(finalPullPoint, new Vector2(55f, 5f));
        SlingshotLaunchRequest launchRequest = default;

        controller.LaunchRequested += request => launchRequest = request;

        _input.Release(1, releaseScreenPosition);
        _observations.Clear();
        _bandShapeProvider.IsBandShapeClear = false;
        _launchAppliedNotifier.Apply(launchRequest);
        ((ITickable)controller).Tick();

        var expectedRecoilPullPoint = Vector3.Lerp(finalPullPoint, _view.Geometry.RestPoint, 0.5f);
        Assert.That(_observations, Is.EqualTo(new[] { "band-clearance", "band-depth-span", "band-shape", "view-loaded-release" }));
        Assert.That(_bandShapeProvider.Queries[^1].PullPoint, Is.EqualTo(expectedRecoilPullPoint));
        AssertBandShapeEquals(_view.LastBandShape, _bandShapeProvider.ShapePoints);
    }

    [Test]
    public void LaunchApplied_RecoilSolveFailsBeforeClearance_KeepsLastValidLiveBandShape()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(75f, 80f);
        var finalPullPoint = new Vector3(0.5f, 0f, -1f);
        _projector.SetScreenToWorld(releaseScreenPosition, finalPullPoint);
        _projector.SetWorldToScreen(finalPullPoint, new Vector2(75f, 10f));
        SlingshotLaunchRequest launchRequest = default;

        controller.LaunchRequested += request => launchRequest = request;

        _input.Release(1, releaseScreenPosition);
        _bandShapeProvider.IsBandShapeClear = false;
        _launchAppliedNotifier.Apply(launchRequest);
        ((ITickable)controller).Tick();

        var expectedLastValidShape = (Vector3[])_bandShapeProvider.ShapePoints.Clone();
        AssertBandShapeEquals(_view.LastBandShape, expectedLastValidShape);
        _observations.Clear();

        _bandShapeProvider.IsBandShapeClear = false;
        _bandShapeProvider.ShouldFail = true;

        ((ITickable)controller).Tick();

        Assert.That(_observations, Is.EqualTo(new[] { "band-clearance", "band-depth-span", "band-shape", "band-clearance", "view-loaded-release" }));
        AssertBandShapeEquals(_view.LastBandShape, expectedLastValidShape);
    }

    [Test]
    public void LaunchApplied_RecoilSolveFailsBeforeClearance_UsesClearSimpleRecoilFallback()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(75f, 80f);
        var finalPullPoint = new Vector3(0.5f, 0f, -1f);
        _projector.SetScreenToWorld(releaseScreenPosition, finalPullPoint);
        _projector.SetWorldToScreen(finalPullPoint, new Vector2(75f, 10f));
        SlingshotLaunchRequest launchRequest = default;

        controller.LaunchRequested += request => launchRequest = request;

        _input.Release(1, releaseScreenPosition);
        _launchAppliedNotifier.Apply(launchRequest);
        _bandShapeProvider.ShouldFail = true;
        _bandShapeProvider.ClearanceResults.Enqueue(false);
        _bandShapeProvider.ClearanceResults.Enqueue(true);
        _observations.Clear();

        ((ITickable)controller).Tick();

        var expectedRecoilPullPoint = Vector3.Lerp(finalPullPoint, _view.Geometry.RestPoint, 0.5f);
        Assert.That(_observations, Is.EqualTo(new[] { "band-clearance", "band-depth-span", "band-shape", "band-clearance", "view-loaded-release" }));
        Assert.That(_bandShapeProvider.Queries[^1].PullPoint, Is.EqualTo(expectedRecoilPullPoint));
        Assert.That(_bandShapeProvider.ClearanceQueries[^1].PullPoint, Is.EqualTo(expectedRecoilPullPoint));
        AssertBandShapeEqualsRawTwoSpan(_view.LastBandShape, GetExpectedSimpleBandVisualCenterPoint(expectedRecoilPullPoint));
    }

    [Test]
    public void LaunchApplied_RecoilDepthClampedAndSimpleBandClear_UsesClearSimpleRecoilBeforeTautWrap()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(65f, 45f);
        var finalPullPoint = new Vector3(0.2f, 0f, -0.3f);
        _projector.SetScreenToWorld(releaseScreenPosition, finalPullPoint);
        _projector.SetWorldToScreen(finalPullPoint, new Vector2(65f, 5f));
        SlingshotLaunchRequest launchRequest = default;

        controller.LaunchRequested += request => launchRequest = request;

        _input.Release(1, releaseScreenPosition);
        _launchAppliedNotifier.Apply(launchRequest);
        _bandShapeProvider.SilhouetteMaximumDepth = 0.2f;
        _bandShapeProvider.ClearanceResults.Enqueue(false);
        _bandShapeProvider.ClearanceResults.Enqueue(true);
        var queryCountBeforeRecoil = _bandShapeProvider.Queries.Count;
        _observations.Clear();

        ((ITickable)controller).Tick();

        var rawRecoilPullPoint = Vector3.Lerp(finalPullPoint, _view.Geometry.RestPoint, 0.5f);
        var minimumClearDepth = _bandShapeProvider.SilhouetteMaximumDepth + _view.VisibleBandRadius + _config.BandContactPadding;
        var currentDepth = -Vector3.Dot(rawRecoilPullPoint - _view.Geometry.RestPoint, _view.Geometry.LaunchFrameForward);

        var expectedRecoilPullPoint = rawRecoilPullPoint
                                      - (_view.Geometry.LaunchFrameForward
                                         * (minimumClearDepth - currentDepth));
        Assert.That(_observations, Is.EqualTo(new[] { "band-clearance", "band-depth-span", "band-clearance", "view-loaded-release" }));
        Assert.That(_bandShapeProvider.Queries, Has.Count.EqualTo(queryCountBeforeRecoil));
        Assert.That(_bandShapeProvider.ClearanceQueries[^1].PullPoint, Is.EqualTo(expectedRecoilPullPoint));
        AssertBandShapeEqualsRawTwoSpan(_view.LastBandShape, GetExpectedSimpleBandVisualCenterPoint(expectedRecoilPullPoint));
    }

    [Test]
    public void PointerReleased_ActivePull_ReturnsToCaptureIdleWithoutLaunch()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();

        _input.Release(1, new Vector2(60f, 20f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "target-position", "view-capture-idle" }));
    }

    [Test]
    public void PointerCanceled_ActivePull_ReturnsToCaptureIdleWithoutLaunch()
    {
        using var controller = CreateInitializedController();
        StartActivePull(1);
        _observations.Clear();

        _input.Cancel(1, new Vector2(60f, 20f));

        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-capture-idle" }));
    }

    private SlingshotController CreateInitializedController()
    {
        var controller = CreateController();
        ((IInitializable)controller).Initialize();
        ((ISlingshotCapture)controller).EnableCapture();
        return controller;
    }

    private SlingshotController CreateController()
    {
        return new SlingshotController(_input, _view, _projector, _launchTarget, _heldLaunchTarget, _bandShapeProvider,
            _launchAppliedNotifier, _clock, _config);
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

    private Vector3 GetExpectedClampedPullPoint(Vector3 rawPullPoint)
    {
        var delta = rawPullPoint - _view.Geometry.RestPoint;

        var pullDistance = Mathf.Clamp(
            -Vector3.Dot(delta, _view.Geometry.LaunchFrameForward),
            0f,
            _config.MaximumPullDistance);

        var lateralPullScale = GetExpectedLateralPullScale(pullDistance);

        var pullOffset = Mathf.Clamp(
            Vector3.Dot(delta, _view.Geometry.LaunchFrameRight),
            GetMinimumAllowedPullOffset() * lateralPullScale,
            GetMaximumAllowedPullOffset() * lateralPullScale);

        return _view.Geometry.RestPoint
               + (_view.Geometry.LaunchFrameRight * pullOffset)
               - (_view.Geometry.LaunchFrameForward * pullDistance);
    }

    private float GetExpectedLateralPullScale(float pullDistance)
    {
        var fullLateralPullDistance = Mathf.Max(0.02f, _config.MinimumPullDistance + (_config.BandContactPadding * 2f))
                                      + (_config.BandContactPadding * 2f);

        if (fullLateralPullDistance <= 0.000001f)
            return 1f;

        return Mathf.Clamp01(pullDistance / fullLateralPullDistance);
    }

    private Vector3 GetExpectedSimpleBandVisualCenterPoint(Vector3 pullPoint)
    {
        return pullPoint - (_view.Geometry.LaunchFrameForward * _config.BandContactPadding);
    }

    private float GetMinimumAllowedPullOffset()
    {
        var leftAnchorOffset = Vector3.Dot(_view.Geometry.LeftAnchorPosition - _view.Geometry.RestPoint, _view.Geometry.LaunchFrameRight);
        var rightAnchorOffset = Vector3.Dot(_view.Geometry.RightAnchorPosition - _view.Geometry.RestPoint, _view.Geometry.LaunchFrameRight);
        var minimumAnchorOffset = Mathf.Min(leftAnchorOffset, rightAnchorOffset);
        return Mathf.Max(-_config.MaximumLateralPull, minimumAnchorOffset);
    }

    private float GetMaximumAllowedPullOffset()
    {
        var leftAnchorOffset = Vector3.Dot(_view.Geometry.LeftAnchorPosition - _view.Geometry.RestPoint, _view.Geometry.LaunchFrameRight);
        var rightAnchorOffset = Vector3.Dot(_view.Geometry.RightAnchorPosition - _view.Geometry.RestPoint, _view.Geometry.LaunchFrameRight);
        var maximumAnchorOffset = Mathf.Max(leftAnchorOffset, rightAnchorOffset);
        return Mathf.Min(_config.MaximumLateralPull, maximumAnchorOffset);
    }

    private void AssertBandShapeEquals(SlingshotBandShape shape, params Vector3[] expectedPoints)
    {
        Assert.That(shape.Points.Count, Is.EqualTo(expectedPoints.Length));

        for (var i = 0; i < expectedPoints.Length; i += 1)
        {
            Assert.That(shape.Points[i], Is.EqualTo(expectedPoints[i]));
        }
    }

    private void AssertBandShapeEqualsRawTwoSpan(SlingshotBandShape shape, Vector3 centerPoint)
    {
        Assert.That(shape.Points.Count, Is.EqualTo(_bandShapeProvider.BandShapePointCount));

        var middleIndex = (shape.Points.Count - 1) / 2;
        var lastIndex = shape.Points.Count - 1;

        for (var pointIndex = 0; pointIndex <= middleIndex; pointIndex += 1)
        {
            var progress = middleIndex <= 0 ? 1f : (float)pointIndex / middleIndex;
            var expectedPoint = Vector3.Lerp(_view.Geometry.LeftAnchorPosition, centerPoint, progress);
            Assert.That(shape.Points[pointIndex], Is.EqualTo(expectedPoint));
        }

        for (var pointIndex = middleIndex + 1; pointIndex <= lastIndex; pointIndex += 1)
        {
            var progress = (float)(pointIndex - middleIndex) / (lastIndex - middleIndex);
            var expectedPoint = Vector3.Lerp(centerPoint, _view.Geometry.RightAnchorPosition, progress);
            Assert.That(shape.Points[pointIndex], Is.EqualTo(expectedPoint));
        }
    }
}
