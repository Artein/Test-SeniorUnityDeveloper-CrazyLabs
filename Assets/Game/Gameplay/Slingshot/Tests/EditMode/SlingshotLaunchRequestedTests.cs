using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Slingshot.Tests.EditMode;
using Game.Input.UnityInput;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Internal;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class SlingshotLaunchRequestedTests
{
    private readonly List<string> _observations = new();
    private readonly List<SlingshotLaunchRequest> _launchRequests = new();
    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private FakeSlingshotConfig _config;
    private FakeUnityInput _input;
    private FakeGameplayStateService _stateService;
    private FakeSlingshotView _view;
    private FakeSlingshotInputProjector _projector;

    [SetUp]
    public void OnSetUp()
    {
        _observations.Clear();
        _launchRequests.Clear();
        _preLaunchStateId = CreateStateId("Pre-Launch");
        _runningStateId = CreateStateId("Running");
        ResetRuntimeFakes();
    }

    [TearDown]
    public void OnTearDown()
    {
        UnityEngine.Object.DestroyImmediate(_preLaunchStateId);
        UnityEngine.Object.DestroyImmediate(_runningStateId);
    }

    private void ResetRuntimeFakes()
    {
        _observations.Clear();
        _launchRequests.Clear();

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
            LaunchUpSpeed = 1.5f
        };
        _input = new FakeUnityInput(_observations);
        _stateService = new FakeGameplayStateService(_preLaunchStateId);
        _view = new FakeSlingshotView(_observations);
        _projector = new FakeSlingshotInputProjector();
        ConfigureRestBandScreenProjection();
    }

    [Test]
    public void PointerReleased_ValidPull_RaisesOneLaunchRequestWithPayloadAndRestoresIdle()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        _observations.Clear();
        var releaseScreenPosition = new Vector2(75f, 80f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(0.5f, 0f, -1.25f), new Vector2(75f, 15f));

        _input.Release(1, releaseScreenPosition);

        Assert.That(_launchRequests, Has.Count.EqualTo(1));
        Assert.That(_observations, Is.EqualTo(new[] { "view-capture-idle", "launch-requested" }));
        var request = _launchRequests[0];
        Assert.That(request.PullDistance, Is.EqualTo(1.25f));
        Assert.That(request.PullOffset, Is.EqualTo(0.5f));
        Assert.That(request.NormalizedPower, Is.EqualTo(Mathf.InverseLerp(0.25f, 2f, 1.25f)).Within(0.0001f));
        Assert.That(request.LaunchSpeed, Is.EqualTo(Mathf.Lerp(4f, 12f, request.NormalizedPower)).Within(0.0001f));
        Assert.That(request.LaunchUpDirection, Is.EqualTo(Vector3.up));
        Assert.That(request.LaunchUpSpeed, Is.EqualTo(1.5f));
        AssertDirectionEquals(request.LaunchDirection, Quaternion.AngleAxis(14f, Vector3.up) * Vector3.forward);
    }

    [Test]
    public void PointerReleased_WeakPull_RestoresIdleWithoutLaunchRequest()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        _observations.Clear();
        var releaseScreenPosition = new Vector2(50f, 40f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(0f, 0f, -0.2f), new Vector2(50f, 5f));

        _input.Release(1, releaseScreenPosition);

        Assert.That(_launchRequests, Is.Empty);
        Assert.That(_observations, Is.EqualTo(new[] { "view-capture-idle" }));
    }

    [Test]
    public void PointerReleased_ForwardOnlyPull_RestoresIdleWithoutLaunchRequest()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        _observations.Clear();
        var releaseScreenPosition = new Vector2(80f, 40f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(0.5f, 0f, 1f), new Vector2(75f, 0f));

        _input.Release(1, releaseScreenPosition);

        Assert.That(_launchRequests, Is.Empty);
        Assert.That(_observations, Is.EqualTo(new[] { "view-capture-idle" }));
    }

    [Test]
    public void PointerCanceled_ActivePull_RestoresIdleWithoutLaunchRequest()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        _observations.Clear();

        _input.Cancel(1, new Vector2(60f, 30f));

        Assert.That(_launchRequests, Is.Empty);
        Assert.That(_observations, Is.EqualTo(new[] { "view-capture-idle" }));
    }

    [Test]
    public void PointerReleased_ProjectionFailure_RestoresIdleWithoutLaunchRequest()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        _observations.Clear();
        var releaseScreenPosition = new Vector2(65f, 35f);
        _projector.SetScreenProjectionFailure(releaseScreenPosition);

        _input.Release(1, releaseScreenPosition);

        Assert.That(_launchRequests, Is.Empty);
        Assert.That(_observations, Is.EqualTo(new[] { "view-capture-idle" }));
    }

    [Test]
    public void PointerReleased_OtherPointer_DoesNotRaiseLaunchRequestOrResetActivePull()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        _observations.Clear();
        var releaseScreenPosition = new Vector2(75f, 80f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(0.5f, 0f, -1.25f), new Vector2(75f, 15f));

        _input.Release(2, releaseScreenPosition);

        Assert.That(_launchRequests, Is.Empty);
        Assert.That(_observations, Is.Empty);
        Assert.That(_view.ActivePullVisuals, Has.Count.EqualTo(1));
    }

    [Test]
    public void PointerReleased_LateralOffsetSign_RotatesLaunchDirectionWithoutChangingPower()
    {
        var positiveRequest = ReleaseAndCaptureRequest(new Vector3(0.75f, 0f, -1.25f), new Vector2(80f, 15f));
        ResetRuntimeFakes();
        var negativeRequest = ReleaseAndCaptureRequest(new Vector3(-0.75f, 0f, -1.25f), new Vector2(20f, 15f));

        Assert.That(positiveRequest.NormalizedPower, Is.EqualTo(negativeRequest.NormalizedPower).Within(0.0001f));
        Assert.That(positiveRequest.LaunchSpeed, Is.EqualTo(negativeRequest.LaunchSpeed).Within(0.0001f));
        Assert.That(positiveRequest.LaunchDirection.x, Is.GreaterThan(0f));
        Assert.That(negativeRequest.LaunchDirection.x, Is.LessThan(0f));
    }

    [Test]
    public void PointerReleased_LateralOffsetBeyondLimit_ClampsLaunchDirectionAngle()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(95f, 80f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(5f, 0f, -1f), new Vector2(95f, 15f));

        _input.Release(1, releaseScreenPosition);

        var request = _launchRequests[0];
        Assert.That(request.PullOffset, Is.EqualTo(1.25f));
        AssertDirectionEquals(request.LaunchDirection, Quaternion.AngleAxis(35f, Vector3.up) * Vector3.forward);
    }

    [Test]
    public void LaunchRequested_WhenSubscriberThrows_LogsAndNotifiesRemainingSubscribers()
    {
        using var controller = CreateInitializedController();
        var notifier = (ISlingshotLaunchNotifier)controller;
        var receivedCount = 0;
        notifier.LaunchRequested += _ => throw new InvalidOperationException("subscriber failed");
        notifier.LaunchRequested += _ => receivedCount++;
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(75f, 80f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(0.5f, 0f, -1.25f), new Vector2(75f, 15f));
        LogAssert.Expect(LogType.Exception, new Regex("subscriber failed"));

        _input.Release(1, releaseScreenPosition);

        Assert.That(receivedCount, Is.EqualTo(1));
    }

    [Test]
    public void Install_ContainerBuilt_ResolvesSlingshotContracts()
    {
        var builder = new ContainerBuilder();
        var cameraObject = new GameObject("Slingshot Test Camera");
        var camera = cameraObject.AddComponent<Camera>();
        builder.RegisterInstance(_input).As<IUnityInput>();
        builder.RegisterInstance(_stateService).As<IGameplayStateService>();
        builder.RegisterInstance(new TestLaunchTarget()).As<ILaunchTarget>();
        var installer = new SlingshotInstaller(_config, _preLaunchStateId, _view, camera);

        installer.Install(builder);

        using var container = builder.Build();
        var notifier = container.Resolve<ISlingshotLaunchNotifier>();
        var launcher = container.Resolve<ISlingshotLauncher>();
        var initializables = container.Resolve<ContainerLocal<IReadOnlyList<IInitializable>>>().Value;
        UnityEngine.Object.DestroyImmediate(cameraObject);

        Assert.That(notifier, Is.Not.Null);
        Assert.That(launcher, Is.Not.Null);
        Assert.That(initializables.Count, Is.EqualTo(2));
    }

    private SlingshotLaunchRequest ReleaseAndCaptureRequest(Vector3 rawPullPoint, Vector2 touchIndicatorScreenPosition)
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(75f, 80f);
        ConfigureProjectionForPull(releaseScreenPosition, rawPullPoint, touchIndicatorScreenPosition);

        _input.Release(1, releaseScreenPosition);

        return _launchRequests[0];
    }

    private SlingshotController CreateInitializedController()
    {
        var controller = new SlingshotController(_input, _stateService, _view, _projector, _config, _preLaunchStateId);
        ((IInitializable)controller).Initialize();
        return controller;
    }

    private void SubscribeToLaunchRequests(ISlingshotLaunchNotifier notifier)
    {
        notifier.LaunchRequested += request =>
        {
            _launchRequests.Add(request);
            _observations.Add("launch-requested");
        };
    }

    private void StartActivePull(int pointerId)
    {
        _projector.SetScreenToWorld(new Vector2(50f, 20f), _view.Geometry.RestPoint);
        _projector.SetWorldToScreen(_view.Geometry.RestPoint, new Vector2(50f, 0f));
        _input.Press(pointerId, new Vector2(50f, 20f));
    }

    private void ConfigureProjectionForPull(Vector2 screenPosition, Vector3 rawPullPoint, Vector2 touchIndicatorScreenPosition)
    {
        _projector.SetScreenToWorld(screenPosition, rawPullPoint);
        _projector.SetWorldToScreen(GetClampedPullPoint(rawPullPoint), touchIndicatorScreenPosition);
    }

    private Vector3 GetClampedPullPoint(Vector3 rawPullPoint)
    {
        var delta = rawPullPoint - _view.Geometry.RestPoint;

        var pullDistance = Mathf.Clamp(
            -Vector3.Dot(delta, _view.Geometry.LaunchFrameForward),
            0f,
            _config.MaximumPullDistance);

        var pullOffset = Mathf.Clamp(
            Vector3.Dot(delta, _view.Geometry.LaunchFrameRight),
            -_config.MaximumLateralPull,
            _config.MaximumLateralPull);

        return _view.Geometry.RestPoint
               + (_view.Geometry.LaunchFrameRight * pullOffset)
               - (_view.Geometry.LaunchFrameForward * pullDistance);
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

    private void AssertDirectionEquals(Vector3 actual, Vector3 expected)
    {
        var normalizedExpected = expected.normalized;
        Assert.That(actual.x, Is.EqualTo(normalizedExpected.x).Within(0.0001f));
        Assert.That(actual.y, Is.EqualTo(normalizedExpected.y).Within(0.0001f));
        Assert.That(actual.z, Is.EqualTo(normalizedExpected.z).Within(0.0001f));
        Assert.That(actual.magnitude, Is.EqualTo(1f).Within(0.0001f));
    }

    private sealed class TestLaunchTarget : ILaunchTarget
    {
        public void Hold()
        {
        }

        public void Launch(Vector3 velocity)
        {
        }
    }
}
