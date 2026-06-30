using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Foundation.Input;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Slingshot.Tests.EditMode;
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
    private FakeSlingshotConfig _config;
    private FakeUnityInput _input;
    private FakeSlingshotView _view;
    private FakeSlingshotInputProjector _projector;
    private FakeLaunchTarget _launchTarget;
    private FakeHeldLaunchTarget _heldLaunchTarget;
    private FakeSlingshotBandShapeProvider _bandShapeProvider;
    private SlingshotPullOffsetNormalizer _pullOffsetNormalizer;
    private FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private FakeTime _clock;

    [SetUp]
    public void OnSetUp()
    {
        _observations.Clear();
        _launchRequests.Clear();
        ResetRuntimeFakes();
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
        _pullOffsetNormalizer = new SlingshotPullOffsetNormalizer(_config);
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();
        _clock = new FakeTime { DeltaTime = 0.1f };
        ConfigureRestBandScreenProjection();
    }

    [Test]
    public void PointerReleased_ValidPull_RaisesOneLaunchRequestWithPayloadAndKeepsLoadedBandUntilLaunchApplied()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        _observations.Clear();
        var releaseScreenPosition = new Vector2(75f, 80f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(0.5f, 0f, -1.25f), new Vector2(75f, 15f));

        _input.Release(1, releaseScreenPosition);

        Assert.That(_launchRequests, Has.Count.EqualTo(1));
        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "band-shape", "view-loaded-release", "launch-requested" }));
        var request = _launchRequests[0];
        Assert.That(request.PullDistance, Is.EqualTo(1.25f));
        Assert.That(request.PullOffset, Is.EqualTo(0.5f));
        Assert.That(request.FinalPullPoint, Is.EqualTo(new Vector3(0.5f, 0f, -1.25f)));
        Assert.That(request.PullStrength, Is.EqualTo(Mathf.InverseLerp(0.25f, 2f, 1.25f)).Within(0.0001f));
        Assert.That(request.NormalizedLateralPull, Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(request.LaunchFrameForward, Is.EqualTo(Vector3.forward));
        Assert.That(request.LaunchFrameUp, Is.EqualTo(Vector3.up));
    }

    [Test]
    public void LaunchApplied_RecoilCompleteAndClearanceProven_DetachesAtRest()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(75f, 80f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(0.5f, 0f, -1.25f), new Vector2(75f, 15f));
        _input.Release(1, releaseScreenPosition);
        _clock.DeltaTime = _config.BandRecoilDuration;
        _observations.Clear();

        _launchAppliedNotifier.Apply(_launchRequests[0]);
        ((ITickable)controller).Tick();

        Assert.That(_observations, Is.EqualTo(new[] { "band-clearance", "band-depth-span", "view-inactive-idle" }));
        Assert.That(_bandShapeProvider.ClearanceQueries, Has.Count.EqualTo(1));
        Assert.That(_bandShapeProvider.DepthSpanQueries, Has.Count.EqualTo(1));
        Assert.That(_bandShapeProvider.ClearanceRadii[^1], Is.EqualTo(_view.VisibleBandRadius));
        _observations.Clear();

        ((ITickable)controller).Tick();

        Assert.That(_observations, Is.Empty);
    }

    [Test]
    public void LaunchApplied_ClearanceProvenBeforeRecoilDuration_KeepsLoadedReleaseUntilDurationCompletes()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(75f, 80f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(0.5f, 0f, -1.25f), new Vector2(75f, 15f));
        _input.Release(1, releaseScreenPosition);
        _clock.DeltaTime = _config.BandRecoilDuration * 0.5f;
        _observations.Clear();

        _launchAppliedNotifier.Apply(_launchRequests[0]);
        ((ITickable)controller).Tick();

        Assert.That(_observations, Is.EqualTo(new[] { "band-depth-span", "band-shape", "view-loaded-release" }));
        AssertBandShapeEquals(_view.LastBandShape, _bandShapeProvider.ShapePoints);
        _clock.DeltaTime = _config.BandRecoilDuration;
        _observations.Clear();

        ((ITickable)controller).Tick();

        Assert.That(_observations, Is.EqualTo(new[] { "band-clearance", "band-depth-span", "view-inactive-idle" }));
        AssertBandShapeEquals(_view.LastBandShape, CreateRestBandShape());
    }

    [Test]
    public void LaunchApplied_ClearanceProvenBeforeTargetPassesRestBand_KeepsLoadedReleaseUntilTargetPasses()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(75f, 80f);
        var finalPullPoint = new Vector3(0.5f, 0f, -1.25f);
        ConfigureProjectionForPull(releaseScreenPosition, finalPullPoint, new Vector2(75f, 15f));
        _input.Release(1, releaseScreenPosition);
        _launchAppliedNotifier.Apply(_launchRequests[0]);
        _bandShapeProvider.SilhouetteMaximumDepth = 0.1f;
        _clock.DeltaTime = _config.BandRecoilDuration;
        _observations.Clear();

        ((ITickable)controller).Tick();

        Assert.That(_observations,
            Is.EqualTo(new[] { "band-clearance", "band-depth-span", "band-depth-span", "band-clearance", "view-loaded-release" }));
        Assert.That(_bandShapeProvider.ClearanceQueries, Has.Count.EqualTo(2));
        Assert.That(_bandShapeProvider.DepthSpanQueries, Has.Count.EqualTo(2));
        _bandShapeProvider.SilhouetteMaximumDepth = -_view.VisibleBandRadius;
        _observations.Clear();

        ((ITickable)controller).Tick();

        Assert.That(_observations, Is.EqualTo(new[] { "band-clearance", "band-depth-span", "view-inactive-idle" }));
        Assert.That(_bandShapeProvider.ClearanceQueries, Has.Count.EqualTo(3));
        Assert.That(_bandShapeProvider.DepthSpanQueries, Has.Count.EqualTo(3));
        AssertBandShapeEquals(_view.LastBandShape, CreateRestBandShape());
    }

    [Test]
    public void LaunchApplied_RecoilReadyButClearanceBlocked_KeepsLoadedReleaseAndRetriesLiveProvider()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(75f, 80f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(0.5f, 0f, -1.25f), new Vector2(75f, 15f));
        _input.Release(1, releaseScreenPosition);
        _launchAppliedNotifier.Apply(_launchRequests[0]);
        _bandShapeProvider.IsBandShapeClear = false;
        _clock.DeltaTime = _config.BandRecoilDuration;
        _observations.Clear();

        ((ITickable)controller).Tick();

        Assert.That(_observations, Is.EqualTo(new[] { "band-clearance", "band-depth-span", "band-shape", "view-loaded-release" }));
        Assert.That(_bandShapeProvider.ClearanceQueries, Has.Count.EqualTo(1));
        Assert.That(_bandShapeProvider.Queries[^1].PullPoint, Is.EqualTo(_view.Geometry.RestPoint));
        AssertBandShapeEquals(_view.LastBandShape, _bandShapeProvider.ShapePoints);
    }

    [Test]
    public void LaunchApplied_WithoutPendingValidRelease_DoesNotStartRecoil()
    {
        using var controller = CreateInitializedController();

        var request = new SlingshotLaunchRequest(1f, 1f, 0f, 0f, new Vector3(0f, 0f, -1f), Vector3.forward, Vector3.up);
        _observations.Clear();

        _launchAppliedNotifier.Apply(request);
        ((ITickable)controller).Tick();

        Assert.That(_observations, Is.Empty);
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
        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "target-position", "view-capture-idle" }));
        Assert.That(_bandShapeProvider.Queries, Is.Empty);
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
        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "target-position", "view-capture-idle" }));
        Assert.That(_bandShapeProvider.Queries, Is.Empty);
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
        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-capture-idle" }));
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
        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "view-capture-idle" }));
    }

    [Test]
    public void PointerReleased_BandShapeSolveFailureAfterLastValidShape_BlocksLaunchAndRestoresIdle()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        _observations.Clear();
        var releaseScreenPosition = new Vector2(75f, 80f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(0.5f, 0f, -1.25f), new Vector2(75f, 15f));
        _bandShapeProvider.ShouldFail = true;
        _bandShapeProvider.ShouldFailActivePullOnly = true;

        _input.Release(1, releaseScreenPosition);

        Assert.That(_launchRequests, Is.Empty);
        Assert.That(_observations, Is.EqualTo(new[] { "target-position", "band-shape", "target-position", "view-capture-idle" }));
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
    public void PointerReleased_LateralOffsetSign_ReportsSignedPullOffsetWithoutChangingPullStrength()
    {
        var positiveRequest = ReleaseAndCaptureRequest(new Vector3(0.75f, 0f, -1.25f), new Vector2(80f, 15f));
        ResetRuntimeFakes();
        var negativeRequest = ReleaseAndCaptureRequest(new Vector3(-0.75f, 0f, -1.25f), new Vector2(20f, 15f));

        Assert.That(positiveRequest.PullStrength, Is.EqualTo(negativeRequest.PullStrength).Within(0.0001f));
        Assert.That(positiveRequest.PullOffset, Is.GreaterThan(0f));
        Assert.That(negativeRequest.PullOffset, Is.LessThan(0f));
        Assert.That(positiveRequest.NormalizedLateralPull, Is.GreaterThan(0f));
        Assert.That(negativeRequest.NormalizedLateralPull, Is.LessThan(0f));
    }

    [Test]
    public void PointerReleased_LateralOffsetBeyondAnchorSpan_ClampsPullOffsetAndNormalizedLateralPull()
    {
        using var controller = CreateInitializedController();
        SubscribeToLaunchRequests(controller);
        StartActivePull(1);
        var releaseScreenPosition = new Vector2(95f, 80f);
        ConfigureProjectionForPull(releaseScreenPosition, new Vector3(5f, 0f, -1f), new Vector2(95f, 15f));

        _input.Release(1, releaseScreenPosition);

        var request = _launchRequests[0];
        Assert.That(request.PullOffset, Is.EqualTo(1f));
        Assert.That(request.NormalizedLateralPull, Is.EqualTo(1f).Within(0.0001f));
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
        builder.RegisterInstance<IUnityInput>(_input);
        var launchTarget = new TestLaunchTarget();
        builder.RegisterInstance<ILaunchTarget, IHeldLaunchTarget, ILaunchTargetSilhouetteSource>(launchTarget);
        builder.RegisterInstance<IPullHintView, IPullHintTuning>(new TestPullHintView());
        var installer = new SlingshotInstaller(_config, _view, camera);

        installer.Install(builder);

        using var container = builder.Build();
        var notifier = container.Resolve<ISlingshotLaunchNotifier>();
        var capture = container.Resolve<ISlingshotCapture>();
        var activePullNotifier = container.Resolve<ISlingshotActivePullNotifier>();
        var captureLifecycleNotifier = container.Resolve<ISlingshotCaptureLifecycleNotifier>();
        var runPreparationReset = container.Resolve<ISlingshotRunPreparationReset>();
        var presentationContextSource = container.Resolve<ISlingshotPresentationContextSource>();
        var pullOffsetNormalizer = container.Resolve<ISlingshotPullOffsetNormalizer>();
        var appliedNotifier = container.Resolve<ISlingshotLaunchAppliedNotifier>();
        var appliedPublisher = container.Resolve<ISlingshotLaunchAppliedPublisher>();
        var bandShapeProvider = container.Resolve<ISlingshotBandShapeProvider>();
        var initializables = container.Resolve<ContainerLocal<IReadOnlyList<IInitializable>>>().Value;
        var tickables = container.Resolve<ContainerLocal<IReadOnlyList<ITickable>>>().Value;
        UnityEngine.Object.DestroyImmediate(cameraObject);

        Assert.That(notifier, Is.Not.Null);
        Assert.That(capture, Is.Not.Null);
        Assert.That(activePullNotifier, Is.Not.Null);
        Assert.That(captureLifecycleNotifier, Is.Not.Null);
        Assert.That(runPreparationReset, Is.Not.Null);
        Assert.That(presentationContextSource, Is.Not.Null);
        Assert.That(pullOffsetNormalizer, Is.Not.Null);
        Assert.That(appliedNotifier, Is.Not.Null);
        Assert.That(appliedPublisher, Is.Not.Null);
        Assert.That(bandShapeProvider, Is.Not.Null);
        Assert.That(initializables.Count, Is.EqualTo(3));
        Assert.That(tickables.Count, Is.EqualTo(3));
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
        var controller = new SlingshotController(_input, _view, _projector, _launchTarget, _heldLaunchTarget, _bandShapeProvider,
            _pullOffsetNormalizer, _launchAppliedNotifier, _clock, _config);
        ((IInitializable)controller).Initialize();
        ((ISlingshotCapture)controller).EnableCapture();
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
        var pullDistance = Mathf.Clamp(-Vector3.Dot(delta, _view.Geometry.LaunchFrameForward), 0f, _config.MaximumPullDistance);
        var anchorOffsetA = Vector3.Dot(_view.Geometry.LeftAnchorPosition - _view.Geometry.RestPoint, _view.Geometry.LaunchFrameRight);
        var anchorOffsetB = Vector3.Dot(_view.Geometry.RightAnchorPosition - _view.Geometry.RestPoint, _view.Geometry.LaunchFrameRight);
        var minimumAnchorOffset = Mathf.Min(anchorOffsetA, anchorOffsetB);
        var maximumAnchorOffset = Mathf.Max(anchorOffsetA, anchorOffsetB);

        var fullLateralPullDistance = Mathf.Max(0.02f, _config.MinimumPullDistance + (_config.BandContactPadding * 2f))
                                      + (_config.BandContactPadding * 2f);

        var lateralPullScale = fullLateralPullDistance <= 0.000001f ? 1f : Mathf.Clamp01(pullDistance / fullLateralPullDistance);

        var pullOffset = Mathf.Clamp(
            Vector3.Dot(delta, _view.Geometry.LaunchFrameRight),
            Mathf.Max(-_config.MaximumLateralPull, minimumAnchorOffset) * lateralPullScale,
            Mathf.Min(_config.MaximumLateralPull, maximumAnchorOffset) * lateralPullScale);

        return _view.Geometry.RestPoint + (_view.Geometry.LaunchFrameRight * pullOffset) - (_view.Geometry.LaunchFrameForward * pullDistance);
    }

    private void ConfigureRestBandScreenProjection()
    {
        _projector.SetWorldToScreen(_view.Geometry.LeftAnchorPosition, new Vector2(0f, 0f));
        _projector.SetWorldToScreen(_view.Geometry.RightAnchorPosition, new Vector2(100f, 0f));
        _projector.SetWorldToScreen(_view.Geometry.RestPoint, new Vector2(50f, 0f));
    }

    private Vector3[] CreateRestBandShape()
    {
        var points = new Vector3[_bandShapeProvider.BandShapePointCount];
        var middleIndex = (points.Length - 1) / 2;
        var lastIndex = points.Length - 1;

        for (var pointIndex = 0; pointIndex <= middleIndex; pointIndex += 1)
        {
            var progress = middleIndex <= 0 ? 1f : (float)pointIndex / middleIndex;
            points[pointIndex] = Vector3.Lerp(_view.Geometry.LeftAnchorPosition, _view.Geometry.RestPoint, progress);
        }

        for (var pointIndex = middleIndex + 1; pointIndex <= lastIndex; pointIndex += 1)
        {
            var progress = (float)(pointIndex - middleIndex) / (lastIndex - middleIndex);
            points[pointIndex] = Vector3.Lerp(_view.Geometry.RestPoint, _view.Geometry.RightAnchorPosition, progress);
        }

        return points;
    }

    private void AssertBandShapeEquals(SlingshotBandShape shape, params Vector3[] expectedPoints)
    {
        Assert.That(shape.Points.Count, Is.EqualTo(expectedPoints.Length));

        for (var i = 0; i < expectedPoints.Length; i += 1)
        {
            Assert.That(shape.Points[i], Is.EqualTo(expectedPoints[i]));
        }
    }

    private sealed class TestLaunchTarget : ILaunchTarget, IHeldLaunchTarget, ILaunchTargetSilhouetteSource
    {
        public void Hold()
        {
        }

        public void Launch(Vector3 velocity)
        {
        }

        public void SetHeldPosition(Vector3 heldPosition)
        {
        }

        public bool TryWriteSilhouetteSamples(LaunchTargetSilhouetteQuery query, Vector3[] outputSamples, out int sampleCount)
        {
            if (outputSamples is null)
                throw new ArgumentNullException(nameof(outputSamples));

            if (outputSamples.Length < 4)
                throw new ArgumentException("Output buffer is too small.", nameof(outputSamples));

            outputSamples[0] = new Vector3(-0.5f, 0f, 0f);
            outputSamples[1] = new Vector3(0.5f, 0f, 0f);
            outputSamples[2] = new Vector3(0.5f, 0f, -1f);
            outputSamples[3] = new Vector3(-0.5f, 0f, -1f);
            sampleCount = 4;
            return true;
        }
    }

    private sealed class TestPullHintView : IPullHintView, IPullHintTuning
    {
        public float InitialIdleDelaySeconds => 2f;
        public float PlaybackDurationSeconds => 1.25f;
        public float RepeatCooldownSeconds => 4f;

        public void ShowAt(Vector2 screenPosition)
        {
        }

        public void Play()
        {
        }

        public void Hide()
        {
        }
    }
}
