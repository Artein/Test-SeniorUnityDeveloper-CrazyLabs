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
    private FakeRunSurfaceContextSource _surfaceContextSource;
    private FakeSlingshotPresentationContextSource _slingshotPresentationContextSource;
    private FakeRunResultNotifier _runResultNotifier;
    private FakeCharacterPresentationModeClassifier _classifier;
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

        _surfaceContextSource = new FakeRunSurfaceContextSource
        {
            Current = new RunSurfaceContext(true, Vector3.up, 12f)
        };
        _slingshotPresentationContextSource = new FakeSlingshotPresentationContextSource();
        _runResultNotifier = new FakeRunResultNotifier();

        _classifier = new FakeCharacterPresentationModeClassifier
        {
            NextMode = CharacterPresentationMode.Slide
        };
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
    public void Tick_LaunchPush_CopiesLaunchValuesAndZerosPullValues()
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
        Assert.That(frame.Mode, Is.EqualTo(CharacterPresentationMode.LaunchPush));
        Assert.That(frame.NormalizedPull, Is.Zero);
        Assert.That(frame.NormalizedPullOffset, Is.Zero);
        Assert.That(frame.NormalizedLaunchPower, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(frame.NormalizedLaunchOffset, Is.EqualTo(0.5f).Within(0.0001f));
    }

    [Test]
    public void Tick_NonSlingshotMode_ZerosPullAndLaunchValues()
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
        Assert.That(frame.Mode, Is.EqualTo(CharacterPresentationMode.Run));
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
    public void Tick_ClassifierReturnsRun_AppliesRunFrameWithClampedPlayback()
    {
        _classifier.NextMode = CharacterPresentationMode.Run;
        _motionSource.LinearVelocity = new Vector3(0f, 0f, 20f);

        ((ITickable)_presenter).Tick();

        Assert.That(_view.AppliedFrames[0].Mode, Is.EqualTo(CharacterPresentationMode.Run));
        Assert.That(_view.AppliedFrames[0].PlaybackSpeedMultiplier, Is.EqualTo(_tuning.MaximumPlaybackSpeedMultiplier));
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
            _runResultNotifier,
            _classifier,
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

    private RunProgressFrameSnapshot CreateSnapshot()
    {
        Assert.That(RunProgressFrameSnapshot.TryCreate(Vector3.zero, Vector3.forward, Vector3.up, out var snapshot, out var error), Is.True, error);
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
            return ReferenceEquals(CurrentStateId, stateId);
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

    private sealed class FakeRunSurfaceContextSource : IRunSurfaceContextSource
    {
        public RunSurfaceContext Current { get; set; }
    }

    private sealed class FakeSlingshotPresentationContextSource : ISlingshotPresentationContextSource
    {
        public SlingshotPresentationContext Current { get; set; }
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
        public float AirborneDelaySeconds { get; set; } = 0.12f;
        public float SlideEnterDownhillDegrees { get; set; } = 6f;
        public float SlideExitDownhillDegrees { get; set; } = 3f;
        public float RunFlatMaximumAbsSlopeDegrees { get; set; } = 4f;
        public float RunMinimumForwardSpeed { get; set; } = 0.5f;
        public float MinimumLocomotionModeDuration { get; set; } = 0.15f;
        public float LaunchPushMinimumSeconds { get; set; } = 0.25f;
        public float SlideReferenceSpeed { get; set; } = 8f;
        public float RunReferenceSpeed { get; set; } = 8f;
        public float MinimumPlaybackSpeedMultiplier { get; set; } = 0.5f;
        public float MaximumPlaybackSpeedMultiplier { get; set; } = 1.5f;
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
