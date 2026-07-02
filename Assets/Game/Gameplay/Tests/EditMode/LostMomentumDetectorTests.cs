using System;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class LostMomentumDetectorTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private FakeGameplayStateService _stateService;
    private FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private FakeRunProgressService _progressService;
    private FakeRunMotionSource _motionSource;
    private FakeRunEndCandidateReceiver _candidateReceiver;
    private FakeRunEndConfig _config;
    private FakeTime _clock;
    private LostMomentumDetector _detector;

    [SetUp]
    public void OnSetUp()
    {
        _preLaunchStateId = CreateStateId("Pre-Launch");
        _runningStateId = CreateStateId("Running");
        _stateService = new FakeGameplayStateService(_preLaunchStateId);
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();
        _progressService = new FakeRunProgressService();
        _motionSource = new FakeRunMotionSource();
        _candidateReceiver = new FakeRunEndCandidateReceiver();

        _config = new FakeRunEndConfig
        {
            LostMomentumLaunchGraceDuration = 0.1f,
            LostMomentumDuration = 0.2f,
            LostMomentumPlanarSpeedThreshold = 0.5f,
            LostMomentumProgressThreshold = 0.05f
        };
        _clock = new FakeTime { FixedDeltaTime = 0.1f };
        _detector = CreateDetector();
        ((IInitializable)_detector).Initialize();
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_detector).Dispose();

        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void FixedTick_BeforeLaunch_DoesNotSubmitCandidate()
    {
        _stateService.ChangeTo(_runningStateId);

        Tick(5);

        Assert.That(_candidateReceiver.Candidates, Is.Empty);
    }

    [Test]
    public void FixedTick_DuringLaunchGrace_DoesNotSubmitCandidate()
    {
        ActivateDetector();

        ((IFixedTickable)_detector).FixedTick();

        Assert.That(_candidateReceiver.Candidates, Is.Empty);
    }

    [Test]
    public void FixedTick_Running_DoesNotSampleProgressService()
    {
        ActivateDetector();

        ((IFixedTickable)_detector).FixedTick();

        Assert.That(_progressService.SamplePositionCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_HighPlanarSpeed_DoesNotSubmitCandidate()
    {
        ActivateDetector();
        _motionSource.LinearVelocity = Vector3.forward;

        Tick(5);

        Assert.That(_candidateReceiver.Candidates, Is.Empty);
    }

    [Test]
    public void FixedTick_ContinuingForwardProgress_DoesNotSubmitCandidate()
    {
        ActivateDetector();

        for (var tick = 0; tick < 5; tick += 1)
        {
            _motionSource.Position += Vector3.forward * 0.1f;
            SampleProgressService();
            ((IFixedTickable)_detector).FixedTick();
        }

        Assert.That(_candidateReceiver.Candidates, Is.Empty);
    }

    [Test]
    public void FixedTick_LowSpeedAndLowProgressForDuration_SubmitsLostMomentum()
    {
        ActivateDetector();

        Tick(4);

        Assert.That(_candidateReceiver.Candidates, Has.Count.EqualTo(1));
        Assert.That(_candidateReceiver.Candidates[0].Reason, Is.EqualTo(RunEndReason.LostMomentum));
    }

    private LostMomentumDetector CreateDetector()
    {
        return new LostMomentumDetector(
            _stateService,
            _launchAppliedNotifier,
            _progressService,
            _motionSource,
            _candidateReceiver,
            _config,
            _clock,
            _runningStateId);
    }

    private void ActivateDetector()
    {
        _stateService.ChangeTo(_runningStateId);
        _progressService.BeginValidRun(Vector3.zero);
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent());
    }

    private void Tick(int count)
    {
        for (var tick = 0; tick < count; tick += 1)
        {
            SampleProgressService();
            ((IFixedTickable)_detector).FixedTick();
        }
    }

    private void SampleProgressService()
    {
        if (_progressService.HasValidSnapshot)
            _progressService.SamplePosition(_motionSource.Position);
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = stateName;
        _objects.Add(stateId);

        return stateId;
    }

    private SlingshotLaunchAppliedEvent CreateLaunchAppliedEvent()
    {
        var request = new SlingshotLaunchRequest(
            1f,
            1f,
            0f,
            0f,
            Vector3.zero,
            Vector3.forward,
            Vector3.up);

        return new SlingshotLaunchAppliedEvent(
            request,
            Vector3.forward,
            Vector3.forward,
            Vector3.up);
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

    private sealed class FakeSlingshotLaunchAppliedNotifier : ISlingshotLaunchAppliedNotifier
    {
        public event Action<SlingshotLaunchAppliedEvent> LaunchApplied;

        public void Apply(SlingshotLaunchAppliedEvent launchApplied)
        {
            LaunchApplied?.Invoke(launchApplied);
        }
    }

    private sealed class FakeRunProgressService : IRunProgressService
    {
        public bool HasValidSnapshot { get; private set; }
        public string SnapshotError { get; private set; } = string.Empty;
        public RunProgressFrameSnapshot Snapshot { get; private set; }
        public float CurrentForwardProgress { get; private set; }
        public float MaximumForwardProgress { get; private set; }
        public int SamplePositionCallCount { get; private set; }

        public RunProgressSample CurrentSample => new(
            HasValidSnapshot,
            SnapshotError,
            Snapshot,
            CurrentForwardProgress,
            MaximumForwardProgress);

        public bool TryBeginRun(Vector3 origin, out string error)
        {
            BeginValidRun(origin);
            error = string.Empty;
            return true;
        }

        public void BeginValidRun(Vector3 origin)
        {
            RunProgressFrameSnapshot.TryCreate(origin, Vector3.forward, Vector3.up, out var snapshot, out _);
            Snapshot = snapshot;
            HasValidSnapshot = true;
        }

        public void SamplePosition(Vector3 position)
        {
            SamplePositionCallCount += 1;

            if (!HasValidSnapshot)
                return;

            CurrentForwardProgress = Snapshot.GetForwardProgress(position);
            MaximumForwardProgress = Mathf.Max(MaximumForwardProgress, CurrentForwardProgress);
        }

        public void Reset()
        {
            HasValidSnapshot = false;
            Snapshot = default;
            SnapshotError = string.Empty;
            CurrentForwardProgress = 0f;
            MaximumForwardProgress = 0f;
        }
    }

    private sealed class FakeRunMotionSource : IRunMotionSource
    {
        public Vector3 Position { get; set; }
        public Vector3 LinearVelocity { get; set; }
    }

    private sealed class FakeRunEndCandidateReceiver : IRunEndCandidateReceiver
    {
        public List<RunEndCandidate> Candidates { get; } = new();

        public void SubmitCandidate(RunEndCandidate candidate)
        {
            Candidates.Add(candidate);
        }
    }

    private sealed class FakeRunEndConfig : IRunEndConfig
    {
        public float ObstacleImpactSpeedThreshold { get; set; }
        public float LostMomentumLaunchGraceDuration { get; set; }
        public float LostMomentumDuration { get; set; }
        public float LostMomentumPlanarSpeedThreshold { get; set; }
        public float LostMomentumProgressThreshold { get; set; }
        public float RunEndedAcknowledgeGuardDuration { get; set; }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
