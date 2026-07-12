using System;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.CharacterPresentation;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class CharacterPresenterTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private GameplayStateId _runPreparationStateId;
    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private FakeGameplayStateService _stateService;
    private FakeRunMotionSource _motionSource;
    private FakeRunProgressService _progressService;
    private FakeRunSurfaceFrameSource _surfaceContextSource;
    private FakeSlingshotPresentationContextSource _slingshotPresentationContextSource;
    private FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private FakeRunResultNotifier _runResultNotifier;
    private FakeCharacterPresentationModeClassifier _classifier;
    private ICharacterPresentationSupportTracker _supportTracker;
    private FakeCharacterPresentationView _view;
    private FakeCharacterPresentationTuning _tuning;
    private FakeTime _clock;
    private CharacterPresenter _presenter;

    [SetUp]
    public void OnSetUp()
    {
        _runPreparationStateId = CreateStateId("RunPreparation");
        _preLaunchStateId = CreateStateId("Pre-Launch");
        _runningStateId = CreateStateId("Running");
        _stateService = new FakeGameplayStateService(_runningStateId);

        _motionSource = new FakeRunMotionSource
        {
            Position = Vector3.zero,
            LinearVelocity = new Vector3(0f, 0f, 8f)
        };

        _progressService = new FakeRunProgressService
        {
            HasValidSnapshot = true,
            Snapshot = CreateSnapshot()
        };

        _surfaceContextSource = new FakeRunSurfaceFrameSource
        {
            Current = new RunSurfaceContext(true, Vector3.up, 12f)
        };
        _slingshotPresentationContextSource = new FakeSlingshotPresentationContextSource();
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();
        _runResultNotifier = new FakeRunResultNotifier();

        _classifier = new FakeCharacterPresentationModeClassifier
        {
            NextMode = CharacterPresentationMode.Slide
        };
        _supportTracker = new CharacterPresentationSupportTracker();
        _view = new FakeCharacterPresentationView();
        _tuning = new FakeCharacterPresentationTuning();
        _clock = new FakeTime { DeltaTime = 0.1f };
        _presenter = CreatePresenter();
        ((IInitializable)_presenter).Initialize();
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_presenter).Dispose();

        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void Tick_SlingshotContext_ForwardsModeSelectionFactsToClassifier()
    {
        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: true,
            normalizedPull: 0.4f,
            normalizedPullOffset: -0.25f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: 0.12f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasActivePull, Is.True);
        Assert.That(_classifier.LastInput.HasLaunchPush, Is.True);
        Assert.That(_classifier.LastInput.LaunchPushElapsedSeconds, Is.EqualTo(0.12f).Within(0.0001f));
    }

    [Test]
    public void Tick_PullAnticipation_CopiesPullValuesAndZerosLaunchValues()
    {
        _classifier.NextMode = CharacterPresentationMode.PullAnticipation;

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: true,
            normalizedPull: 0.4f,
            normalizedPullOffset: -0.25f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: 0.12f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        ((ITickable)_presenter).Tick();

        var frame = _view.AppliedFrames[0];
        Assert.That(frame.Mode, Is.EqualTo(CharacterPresentationMode.PullAnticipation));
        Assert.That(frame.NormalizedPull, Is.EqualTo(0.4f).Within(0.0001f));
        Assert.That(frame.NormalizedPullOffset, Is.EqualTo(-0.25f).Within(0.0001f));
        Assert.That(frame.NormalizedLaunchPower, Is.Zero);
        Assert.That(frame.NormalizedLaunchOffset, Is.Zero);
    }

    [Test]
    public void Tick_LaunchPushFromLaunchContext_AppliesLaunchFlight()
    {
        _classifier.NextMode = CharacterPresentationMode.LaunchPush;

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: true,
            normalizedPull: 0.4f,
            normalizedPullOffset: -0.25f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: 0.12f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        ((ITickable)_presenter).Tick();

        var frame = _view.AppliedFrames[0];
        Assert.That(frame.Mode, Is.EqualTo(CharacterPresentationMode.LaunchFlight));
        Assert.That(frame.NormalizedPull, Is.Zero);
        Assert.That(frame.NormalizedPullOffset, Is.Zero);
        Assert.That(frame.NormalizedLaunchPower, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(frame.NormalizedLaunchOffset, Is.EqualTo(0.5f).Within(0.0001f));
    }

    [Test]
    public void Tick_LaunchPushWhileLaunchFlightQueued_AppliesLaunchFlight()
    {
        _classifier.NextMode = CharacterPresentationMode.LaunchPush;

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: true,
            normalizedPull: 0.8f,
            normalizedPullOffset: -0.4f,
            hasLaunchPush: false,
            launchPushElapsedSeconds: 0f,
            normalizedLaunchPower: 0f,
            normalizedLaunchOffset: 0f);

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent());
        _view.AppliedFrames.Clear();

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: 0f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: -0.4f);

        ((ITickable)_presenter).Tick();

        var frame = _view.AppliedFrames[0];
        Assert.That(frame.Mode, Is.EqualTo(CharacterPresentationMode.LaunchFlight));
        Assert.That(frame.NormalizedPull, Is.Zero);
        Assert.That(frame.NormalizedPullOffset, Is.Zero);
        Assert.That(frame.NormalizedLaunchPower, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(frame.NormalizedLaunchOffset, Is.EqualTo(-0.4f).Within(0.0001f));
    }

    [Test]
    public void Tick_LaunchFlight_CopiesLaunchValuesAndZerosPullValues()
    {
        _classifier.NextMode = CharacterPresentationMode.LaunchFlight;

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: true,
            normalizedPull: 0.4f,
            normalizedPullOffset: -0.25f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: 0.3f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        ((ITickable)_presenter).Tick();

        var frame = _view.AppliedFrames[0];
        Assert.That(frame.Mode, Is.EqualTo(CharacterPresentationMode.LaunchFlight));
        Assert.That(frame.NormalizedPull, Is.Zero);
        Assert.That(frame.NormalizedPullOffset, Is.Zero);
        Assert.That(frame.NormalizedLaunchPower, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(frame.NormalizedLaunchOffset, Is.EqualTo(0.5f).Within(0.0001f));
    }

    [Test]
    public void Tick_ReservedRunMode_ZerosPullAndLaunchValues()
    {
        _classifier.NextMode = CharacterPresentationMode.Run;

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: true,
            normalizedPull: 0.4f,
            normalizedPullOffset: -0.25f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: 0.12f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        ((ITickable)_presenter).Tick();

        var frame = _view.AppliedFrames[0];
        Assert.That(frame.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
        Assert.That(frame.NormalizedPull, Is.Zero);
        Assert.That(frame.NormalizedPullOffset, Is.Zero);
        Assert.That(frame.NormalizedLaunchPower, Is.Zero);
        Assert.That(frame.NormalizedLaunchOffset, Is.Zero);
    }

    [Test]
    public void Tick_RunningWithValidSnapshot_BuildsClassifierInputFromPhysicsFacts()
    {
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.IsRunActive, Is.True);
        Assert.That(_classifier.LastInput.IsPreLaunch, Is.False);
        Assert.That(_classifier.LastInput.SurfaceContext.IsGrounded, Is.True);
        Assert.That(_classifier.LastInput.SurfaceContext.ForwardDownhillDegrees, Is.EqualTo(12f));
        Assert.That(_classifier.LastInput.CoursePlanarSpeed, Is.EqualTo(8f).Within(0.0001f));
        Assert.That(_classifier.LastInput.CourseForwardSpeed, Is.EqualTo(8f).Within(0.0001f));
        Assert.That(_classifier.LastInput.CourseVerticalSpeed, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(_classifier.LastInput.UngroundedVerticalSeparation, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void Tick_RunningWithValidSnapshot_ForwardsCourseVerticalSpeed()
    {
        _motionSource.LinearVelocity = new Vector3(0f, -1.5f, 8f);

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.CourseVerticalSpeed, Is.EqualTo(-1.5f).Within(0.0001f));
    }

    [Test]
    public void Tick_ClassifierReturnsSlide_AppliesSlideFrameWithSpeedMatchedPlayback()
    {
        ((ITickable)_presenter).Tick();

        Assert.That(_view.AppliedFrames, Has.Count.EqualTo(1));
        Assert.That(_view.AppliedFrames[0].Mode, Is.EqualTo(CharacterPresentationMode.Slide));
        Assert.That(_view.AppliedFrames[0].PlaybackSpeedMultiplier, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void Tick_ClassifierReturnsSlide_UsesSlideReferenceSpeed()
    {
        _tuning.SlideReferenceSpeed = 4f;
        _motionSource.LinearVelocity = new Vector3(0f, 0f, 6f);

        ((ITickable)_presenter).Tick();

        Assert.That(_view.AppliedFrames[0].PlaybackSpeedMultiplier, Is.EqualTo(1.5f).Within(0.0001f));
    }

    [Test]
    public void Tick_ClassifierReturnsRun_NormalizesToSlideFrameWithClampedPlayback()
    {
        _classifier.NextMode = CharacterPresentationMode.Run;
        _motionSource.LinearVelocity = new Vector3(0f, 0f, 20f);

        ((ITickable)_presenter).Tick();

        Assert.That(_view.AppliedFrames[0].Mode, Is.EqualTo(CharacterPresentationMode.Slide));
        Assert.That(_view.AppliedFrames[0].PlaybackSpeedMultiplier, Is.EqualTo(_tuning.MaximumPlaybackSpeedMultiplier));
    }

    [Test]
    public void Tick_ClassifierReturnsRun_StoresSlideAsCurrentMode()
    {
        _classifier.NextMode = CharacterPresentationMode.Run;

        ((ITickable)_presenter).Tick();

        _classifier.NextMode = CharacterPresentationMode.Idle;
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.CurrentMode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Tick_ClassifierReturnsIdle_AppliesNeutralPlayback()
    {
        _classifier.NextMode = CharacterPresentationMode.Idle;
        _motionSource.LinearVelocity = new Vector3(0f, 0f, 20f);

        ((ITickable)_presenter).Tick();

        Assert.That(_view.AppliedFrames[0].Mode, Is.EqualTo(CharacterPresentationMode.Idle));
        Assert.That(_view.AppliedFrames[0].PlaybackSpeedMultiplier, Is.EqualTo(1f));
    }

    [Test]
    public void Tick_Ungrounded_AccumulatesUngroundedElapsedTime()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.UngroundedElapsedSeconds, Is.EqualTo(_clock.DeltaTime).Within(0.0001f));
    }

    [Test]
    public void Tick_Ungrounded_CapturesStartPositionOncePerUngroundedInterval()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        _motionSource.Position = new Vector3(0f, 3f, 0f);

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.UngroundedVerticalSeparation, Is.EqualTo(0f).Within(0.0001f));

        _motionSource.Position = new Vector3(0f, 2.6f, 0f);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.UngroundedVerticalSeparation, Is.EqualTo(-0.4f).Within(0.0001f));
    }

    [Test]
    public void Tick_Ungrounded_UsesCourseUpForVerticalSeparation()
    {
        _progressService.Snapshot = CreateSnapshot(Vector3.right);
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        _motionSource.Position = new Vector3(3f, 0f, 0f);
        _motionSource.LinearVelocity = new Vector3(-1.5f, 0f, 8f);

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.CourseVerticalSpeed, Is.EqualTo(-1.5f).Within(0.0001f));

        _motionSource.Position = new Vector3(2.75f, 0f, 0f);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.UngroundedVerticalSeparation, Is.EqualTo(-0.25f).Within(0.0001f));
    }

    [Test]
    public void Tick_GroundedAfterUngrounded_ResetsUngroundedSeparation()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        _motionSource.Position = new Vector3(0f, 3f, 0f);
        ((ITickable)_presenter).Tick();

        _motionSource.Position = new Vector3(0f, 2f, 0f);
        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.UngroundedVerticalSeparation, Is.EqualTo(-1f).Within(0.0001f));

        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);
        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.UngroundedElapsedSeconds, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(_classifier.LastInput.UngroundedVerticalSeparation, Is.EqualTo(0f).Within(0.0001f));

        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        _motionSource.Position = new Vector3(0f, 1f, 0f);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.UngroundedVerticalSeparation, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void Tick_GroundedWithSurfaceLiftAbovePresentationThreshold_ForwardsUnsupportedSurface()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);
        _motionSource.LinearVelocity = new Vector3(0f, 0.5f, 8f);

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.SurfaceContext.IsGrounded, Is.False);
        Assert.That(_classifier.LastInput.UngroundedElapsedSeconds, Is.EqualTo(_clock.DeltaTime).Within(0.0001f));
    }

    [Test]
    public void Tick_GroundedFlickerBeforePresentationReacquire_DoesNotResetUngroundedTiming()
    {
        _clock.DeltaTime = 0.02f;
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        _motionSource.Position = new Vector3(0f, 3f, 0f);
        ((ITickable)_presenter).Tick();

        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);
        _motionSource.Position = new Vector3(0f, 2.9f, 0f);
        _motionSource.LinearVelocity = new Vector3(0f, -0.1f, 8f);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.SurfaceContext.IsGrounded, Is.False);
        Assert.That(_classifier.LastInput.UngroundedElapsedSeconds, Is.EqualTo(0.04f).Within(0.0001f));
        Assert.That(_classifier.LastInput.UngroundedVerticalSeparation, Is.EqualTo(-0.1f).Within(0.0001f));
    }

    [Test]
    public void Tick_NeutralPresentationState_ResetsUngroundedSeparation()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        _motionSource.Position = new Vector3(0f, 3f, 0f);
        ((ITickable)_presenter).Tick();

        _motionSource.Position = new Vector3(0f, 2f, 0f);
        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.UngroundedVerticalSeparation, Is.EqualTo(-1f).Within(0.0001f));

        _stateService.ChangeTo(_preLaunchStateId);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.UngroundedElapsedSeconds, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(_classifier.LastInput.UngroundedVerticalSeparation, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void Tick_PostLaunchGroundedBeforeObservedUngrounded_ForwardsLaunchFlightImmediately()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);
    }

    [Test]
    public void Tick_PostLaunchGroundedWithLargeDelta_ForwardsLaunchFlightOnStartTick()
    {
        _clock.DeltaTime = _tuning.LaunchFlightMaximumGroundedWaitSeconds + 1f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);
    }

    [Test]
    public void LaunchApplied_InPreLaunch_StartsLaunchFlightWhenRunningBegins()
    {
        _stateService.ChangeTo(_preLaunchStateId);
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);
        _slingshotPresentationContextSource.Current = default;

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent());
        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.False);

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: 0f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        _stateService.ChangeTo(_runningStateId);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);
    }

    [Test]
    public void Tick_LaunchPushObservedInPreLaunchWithoutLaunchApplied_StartsLaunchFlightWhenRunningBegins()
    {
        _stateService.ChangeTo(_preLaunchStateId);
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: 0f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.False);

        _stateService.ChangeTo(_runningStateId);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);
    }

    [Test]
    public void LaunchApplied_BeforePresentationContextAdvances_StartsLaunchFlightImmediately()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);
        _slingshotPresentationContextSource.Current = default;

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent());
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);
    }

    [Test]
    public void LaunchApplied_BeforeNextTick_AppliesLaunchFlightFrameImmediately()
    {
        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: true,
            normalizedPull: 0.8f,
            normalizedPullOffset: -0.4f,
            hasLaunchPush: false,
            launchPushElapsedSeconds: 0f,
            normalizedLaunchPower: 0f,
            normalizedLaunchOffset: 0f);

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent());

        Assert.That(_view.AppliedFrames, Has.Count.EqualTo(1));

        var frame = _view.AppliedFrames[0];
        Assert.That(frame.Mode, Is.EqualTo(CharacterPresentationMode.LaunchFlight));
        Assert.That(frame.PlaybackSpeedMultiplier, Is.EqualTo(1f));
        Assert.That(frame.NormalizedPull, Is.Zero);
        Assert.That(frame.NormalizedPullOffset, Is.Zero);
        Assert.That(frame.NormalizedLaunchPower, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(frame.NormalizedLaunchOffset, Is.EqualTo(-0.4f).Within(0.0001f));
    }

    [Test]
    public void Tick_PostLaunchGroundedPastMaximumWait_ClearsLaunchFlightAndDoesNotResurrectOnLaterUngrounded()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: 0f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);

        _clock.DeltaTime = _tuning.LaunchFlightMaximumGroundedWaitSeconds;
        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.False);
        Assert.That(_classifier.LastInput.HasLaunchPush, Is.False);

        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.False);
    }

    [Test]
    public void Tick_PostLaunchUngroundedAfterLaunch_ForwardsLaunchFlight()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);
        ((ITickable)_presenter).Tick();

        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);
    }

    [Test]
    public void Tick_PostLaunchGroundedWithLiftAfterObservedUngrounded_PreservesLaunchFlight()
    {
        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);

        _slingshotPresentationContextSource.Current = default;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);
        _motionSource.LinearVelocity = new Vector3(0f, 0.5f, 8f);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);
    }

    [Test]
    public void Tick_LaunchPushFlagDropsBeforeLanding_PreservesLaunchFlightAndLaunchValues()
    {
        _classifier.NextMode = CharacterPresentationMode.LaunchFlight;
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);
        ((ITickable)_presenter).Tick();

        _slingshotPresentationContextSource.Current = default;
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);

        var frame = _view.AppliedFrames[^1];
        Assert.That(frame.Mode, Is.EqualTo(CharacterPresentationMode.LaunchFlight));
        Assert.That(frame.NormalizedLaunchPower, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(frame.NormalizedLaunchOffset, Is.EqualTo(0.5f).Within(0.0001f));
    }

    [Test]
    public void Tick_PostLaunchGroundedAfterObservedUngrounded_ClearsLaunchFlight()
    {
        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);

        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);

        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.False);
        Assert.That(_classifier.LastInput.HasLaunchPush, Is.False);
        Assert.That(_view.AppliedFrames[^1].Mode, Is.Not.EqualTo(CharacterPresentationMode.LaunchPush));
    }

    [Test]
    public void Tick_PreLaunch_ResetsLaunchFlight()
    {
        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);

        _stateService.ChangeTo(_preLaunchStateId);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.False);
    }

    [Test]
    public void Tick_AcceptedRunResult_ResetsLaunchFlight()
    {
        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);

        _runResultNotifier.Raise(CreateSuccessfulRunResult());
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.False);
    }

    [Test]
    public void Tick_NewLaunchAfterPreviousLaunchEnded_ResetsObservedUngroundedAndStartsLaunchFlightImmediately()
    {
        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            normalizedLaunchPower: 0.8f,
            normalizedLaunchOffset: 0.5f);
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        ((ITickable)_presenter).Tick();
        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);

        _slingshotPresentationContextSource.Current = default;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);
        ((ITickable)_presenter).Tick();

        _slingshotPresentationContextSource.Current = new SlingshotPresentationContext(
            hasActivePull: false,
            normalizedPull: 0f,
            normalizedPullOffset: 0f,
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            normalizedLaunchPower: 0.9f,
            normalizedLaunchOffset: -0.25f);
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);
        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasLaunchFlight, Is.True);
    }

    [Test]
    public void Tick_PreLaunch_ResetsAcceptedRunResult()
    {
        _runResultNotifier.Raise(CreateSuccessfulRunResult());
        _stateService.ChangeTo(_preLaunchStateId);

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.IsPreLaunch, Is.True);
        Assert.That(_classifier.LastInput.HasAcceptedRunResult, Is.False);
    }

    [Test]
    public void Tick_RunPreparation_ResetsAcceptedRunResult()
    {
        _runResultNotifier.Raise(CreateSuccessfulRunResult());
        _stateService.ChangeTo(_runPreparationStateId);

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.IsPreLaunch, Is.False);
        Assert.That(_classifier.LastInput.IsRunActive, Is.False);
        Assert.That(_classifier.LastInput.HasAcceptedRunResult, Is.False);
        Assert.That(_classifier.LastInput.SurfaceContext.IsGrounded, Is.True);
        Assert.That(_classifier.LastInput.SurfaceContext.ForwardDownhillDegrees, Is.Zero);
    }

    [Test]
    public void RunResultAccepted_SuccessfulResult_IsPassedToClassifierUntilPreLaunch()
    {
        _runResultNotifier.Raise(CreateSuccessfulRunResult());

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasAcceptedRunResult, Is.True);
        Assert.That(_classifier.LastInput.AcceptedRunResultSucceeded, Is.True);
    }

    [Test]
    public void Dispose_AfterInitialize_UnsubscribesFromRunResultNotifier()
    {
        ((IDisposable)_presenter).Dispose();
        _runResultNotifier.Raise(CreateSuccessfulRunResult());

        ((ITickable)_presenter).Tick();

        Assert.That(_classifier.LastInput.HasAcceptedRunResult, Is.False);
    }

    private CharacterPresenter CreatePresenter()
    {
        return new CharacterPresenter(
            _stateService,
            _motionSource,
            _progressService,
            _surfaceContextSource,
            _slingshotPresentationContextSource,
            _launchAppliedNotifier,
            _runResultNotifier,
            _classifier,
            _supportTracker,
            _view,
            _tuning,
            _clock,
            _runPreparationStateId,
            _preLaunchStateId,
            _runningStateId);
    }

    private RunResult CreateSuccessfulRunResult()
    {
        return new RunResult(
            RunEndReason.Finished,
            1f,
            5f,
            Vector3.zero,
            3f,
            new RunRewardBreakdown(Array.Empty<RunRewardSourceAmount>()));
    }

    private SlingshotLaunchAppliedEvent CreateLaunchAppliedEvent()
    {
        var request = new SlingshotLaunchRequest(
            pullStrength: 0.8f,
            pullDistance: 1f,
            pullOffset: 0.25f,
            normalizedLateralPull: 0.25f,
            finalPullPoint: Vector3.zero,
            launchFrameForward: Vector3.forward,
            launchFrameUp: Vector3.up);

        return new SlingshotLaunchAppliedEvent(
            request,
            velocityChange: Vector3.forward,
            launchDirection: Vector3.forward,
            launchUpDirection: Vector3.up);
    }

    private RunProgressFrameSnapshot CreateSnapshot()
    {
        return CreateSnapshot(Vector3.up);
    }

    private RunProgressFrameSnapshot CreateSnapshot(Vector3 upDirection)
    {
        Assert.That(RunProgressFrameSnapshot.TryCreate(Vector3.zero, Vector3.forward, upDirection, out var snapshot, out var error), Is.True, error);
        return snapshot;
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = stateName;
        _objects.Add(stateId);
        return stateId;
    }

    private sealed class FakeGameplayStateService : IGameplayStateService
    {
        public GameplayStateId CurrentStateId { get; private set; }

        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanging;
        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanged;

        public FakeGameplayStateService(GameplayStateId currentStateId)
        {
            CurrentStateId = currentStateId;
        }

        public bool IsCurrent(GameplayStateId stateId)
        {
            return CurrentStateId == stateId;
        }

        public bool TryTransitionTo(GameplayStateId nextStateId)
        {
            ChangeTo(nextStateId);
            return true;
        }

        public void ChangeTo(GameplayStateId nextStateId)
        {
            var previousStateId = CurrentStateId;
            GameplayStateChanging?.Invoke(nextStateId, previousStateId);
            CurrentStateId = nextStateId;
            GameplayStateChanged?.Invoke(nextStateId, previousStateId);
        }
    }

    private sealed class FakeRunMotionSource : IRunMotionSource
    {
        public Vector3 Position { get; set; }
        public Vector3 LinearVelocity { get; set; }
    }

    private sealed class FakeRunProgressService : IRunProgressService
    {
        public bool HasValidSnapshot { get; set; }
        public string SnapshotError { get; set; } = string.Empty;
        public RunProgressFrameSnapshot Snapshot { get; set; }
        public float CurrentForwardProgress { get; set; }
        public float MaximumForwardProgress { get; set; }

        public RunProgressSample CurrentSample => new(
            HasValidSnapshot,
            SnapshotError,
            Snapshot,
            CurrentForwardProgress,
            MaximumForwardProgress);

        public bool TryBeginRun(Vector3 origin, out string error)
        {
            error = string.Empty;
            return HasValidSnapshot;
        }

        public void SamplePosition(Vector3 position)
        {
        }

        public void Reset()
        {
            HasValidSnapshot = false;
        }
    }

    private sealed class FakeRunSurfaceFrameSource : IRunSurfaceFrameSource
    {
        public RunSurfaceContext Current { get; set; }

        RunSurfaceFrameSnapshot IRunSurfaceFrameSource.Current
        {
            get
            {
                RunProgressFrameSnapshot.TryCreate(
                    Vector3.zero,
                    Vector3.forward,
                    Vector3.up,
                    out var progressFrame,
                    out _);

                var observationState = Current.IsGrounded
                    ? RunSupportObservationState.Supported
                    : RunSupportObservationState.Missing;

                var observedSupport = new RunSupportObservation(
                    observationState,
                    progressFrame,
                    Current,
                    0f);

                return new RunSurfaceFrameSnapshot(
                    observedSupport,
                    Current,
                    RunSurfaceTransition.None,
                    false,
                    false,
                    default);
            }
        }
    }

    private sealed class FakeSlingshotPresentationContextSource : ISlingshotPresentationContextSource
    {
        public SlingshotPresentationContext Current { get; set; }
    }

    private sealed class FakeSlingshotLaunchAppliedNotifier : ISlingshotLaunchAppliedNotifier
    {
        public event Action<SlingshotLaunchAppliedEvent> LaunchApplied;

        public void Apply(SlingshotLaunchAppliedEvent launchApplied)
        {
            LaunchApplied?.Invoke(launchApplied);
        }
    }

    private sealed class FakeRunResultNotifier : IRunResultNotifier
    {
        public event Action<RunResult> RunResultAccepted;

        public void Raise(RunResult result)
        {
            RunResultAccepted?.Invoke(result);
        }
    }

    private sealed class FakeCharacterPresentationModeClassifier : ICharacterPresentationModeClassifier
    {
        public CharacterPresentationMode NextMode { get; set; }
        public CharacterPresentationClassificationInput LastInput { get; private set; }

        public CharacterPresentationClassificationResult Classify(CharacterPresentationClassificationInput input)
        {
            LastInput = input;
            return new CharacterPresentationClassificationResult(NextMode);
        }
    }

    private sealed class FakeCharacterPresentationView : ICharacterPresentationView
    {
        public List<CharacterPresentationFrame> AppliedFrames { get; } = new();

        public void ApplyFrame(CharacterPresentationFrame frame)
        {
            AppliedFrames.Add(frame);
        }
    }

    private sealed class FakeCharacterPresentationTuning : ICharacterPresentationTuning
    {
        public float FallEnterMinimumUngroundedSeconds { get; set; } = 0.3f;
        public float FallEnterMinimumDownwardSpeed { get; set; } = 1.5f;
        public float FallEnterMinimumVerticalSeparation { get; set; } = 0.18f;
        public float FallEnterHardUngroundedSeconds { get; set; } = 0.65f;
        public float MeaningfulGroundedMovementThreshold { get; set; } = 0.5f;
        public float MinimumLocomotionModeDuration { get; set; } = 0.35f;
        public float LaunchPushMinimumSeconds { get; set; } = 0.25f;
        public float LaunchFlightMaximumGroundedWaitSeconds { get; set; } = 0.35f;
        public float PresentationSupportMaximumSurfaceLiftSpeed { get; set; } = 0.35f;
        public float PresentationSupportReacquireSeconds { get; set; } = 0.08f;
        public float SlideReferenceSpeed { get; set; } = 8f;
        public float MinimumPlaybackSpeedMultiplier { get; set; } = 0.5f;
        public float MaximumPlaybackSpeedMultiplier { get; set; } = 1.5f;
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
