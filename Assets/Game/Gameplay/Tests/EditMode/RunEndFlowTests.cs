using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Economy;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunEndFlowTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private GameplayStateId _runPreparationStateId;
    private GameplayStateId _runningStateId;
    private GameplayStateId _runEndedStateId;
    private FakeGameplayStateService _stateService;
    private FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private FakeContactNotifier _contactNotifier;
    private FakeRunContactClassifier _contactClassifier;
    private FakeRunProgressService _progressService;
    private FakeRunMotionSource _motionSource;
    private RunCurrencyAccumulator _runCurrencyAccumulator;
    private FakeRunEndConfig _config;
    private FakeTime _clock;
    private RunEndFlow _flow;
    private CurrencyDefinition _coins;

    [SetUp]
    public void OnSetUp()
    {
        _runPreparationStateId = CreateStateId("Run Preparation");
        _runningStateId = CreateStateId("Running");
        _runEndedStateId = CreateStateId("Run Ended");
        _stateService = new FakeGameplayStateService(_runPreparationStateId);
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();
        _contactNotifier = new FakeContactNotifier();
        _contactClassifier = new FakeRunContactClassifier();

        _progressService = new FakeRunProgressService
        {
            HasValidSnapshot = true,
            MaximumForwardProgress = 12.5f
        };

        _motionSource = new FakeRunMotionSource
        {
            Position = new Vector3(1f, 2f, 3f),
            LinearVelocity = new Vector3(0f, 0f, 4f)
        };
        _runCurrencyAccumulator = new RunCurrencyAccumulator();
        _config = new FakeRunEndConfig { RunEndedDelay = 0.2f };
        _clock = new FakeTime { FixedDeltaTime = 0.1f };
        _coins = CreateCurrencyDefinition("Coins");
        _flow = CreateFlow();
        ((IInitializable)_flow).Initialize();
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_flow).Dispose();

        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void FixedTick_BeforeLaunchApplied_IgnoresCandidate()
    {
        _stateService.ChangeTo(_runningStateId);

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IFixedTickable)_flow).FixedTick();

        Assert.That(_stateService.RequestedStateIds, Is.Empty);
    }

    [Test]
    public void FixedTick_FinishCandidate_LogsSuccessfulResultAndTransitionsToRunEnded()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished, IsSuccess=True, ElapsedTime=0.1, DistanceTravelled=12.5"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IFixedTickable)_flow).FixedTick();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runEndedStateId));
        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId }));
    }

    [Test]
    public void FixedTick_FinishCandidate_PublishesRunResultWithCurrencySnapshot()
    {
        ActivateRun();
        ((IRunCurrencyAccumulator)_runCurrencyAccumulator).Grant(_coins, 7);
        RunResult? acceptedResult = null;
        ((IRunResultNotifier)_flow).RunResultAccepted += result => acceptedResult = result;
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IFixedTickable)_flow).FixedTick();

        Assert.That(acceptedResult.HasValue, Is.True);
        Assert.That(acceptedResult.Value.CurrencySnapshot.GetAmount(_coins), Is.EqualTo(7));
    }

    [Test]
    public void GameplayStateChanged_EnteringRunPreparation_ResetsAccumulatorWithoutMutatingAcceptedResult()
    {
        ActivateRun();
        ((IRunCurrencyAccumulator)_runCurrencyAccumulator).Grant(_coins, 7);
        RunResult? acceptedResult = null;
        ((IRunResultNotifier)_flow).RunResultAccepted += result => acceptedResult = result;
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IFixedTickable)_flow).FixedTick();
        ((IFixedTickable)_flow).FixedTick();
        ((IFixedTickable)_flow).FixedTick();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runPreparationStateId));
        Assert.That(((IRunCurrencyAccumulator)_runCurrencyAccumulator).CreateSnapshot().GetAmount(_coins), Is.Zero);
        Assert.That(acceptedResult.HasValue, Is.True);
        Assert.That(acceptedResult.Value.CurrencySnapshot.GetAmount(_coins), Is.EqualTo(7));
    }

    [Test]
    public void FixedTick_RunEndedDelayElapsed_TransitionsBackToRunPreparation()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=OutOfBounds"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.OutOfBounds));
        ((IFixedTickable)_flow).FixedTick();
        ((IFixedTickable)_flow).FixedTick();
        ((IFixedTickable)_flow).FixedTick();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runPreparationStateId));
        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId, _runPreparationStateId }));
    }

    [Test]
    public void FixedTick_SameTickCandidates_ResolveByPriority()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.LostMomentum));
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.OutOfBounds));
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IFixedTickable)_flow).FixedTick();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runEndedStateId));
    }

    [Test]
    public void FixedTick_AfterAcceptedResult_IgnoresLaterCandidatesUntilNextLaunch()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IFixedTickable)_flow).FixedTick();
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));
        ((IFixedTickable)_flow).FixedTick();

        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId }));
    }

    [Test]
    public void LaunchApplied_AfterPreviousResult_AllowsNewResult()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished"));
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IFixedTickable)_flow).FixedTick();
        _stateService.ChangeTo(_runPreparationStateId);

        _stateService.ChangeTo(_runningStateId);
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent());
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=ObstacleHit"));
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));
        ((IFixedTickable)_flow).FixedTick();

        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId, _runEndedStateId }));
    }

    [Test]
    public void FixedTick_InvalidProgressSnapshot_TransitionsButSuppressesRunResult()
    {
        ActivateRun();
        _progressService.HasValidSnapshot = false;
        _progressService.SnapshotError = "bad frame";
        LogAssert.Expect(LogType.Error, new Regex("Run End Flow skipped Run Result.*bad frame"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IFixedTickable)_flow).FixedTick();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runEndedStateId));
    }

    [Test]
    public void TriggerNotification_UsesClassifierAndQueuesCandidate()
    {
        ActivateRun();
        _contactClassifier.TriggerCandidate = new RunEndCandidate(RunEndReason.OutOfBounds);
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=OutOfBounds"));

        _contactNotifier.RaiseTrigger(new RigidbodyTriggerNotification(null));
        ((IFixedTickable)_flow).FixedTick();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runEndedStateId));
    }

    private RunEndFlow CreateFlow()
    {
        return new RunEndFlow(_stateService, _launchAppliedNotifier, _contactNotifier, _contactClassifier, _progressService, _motionSource,
            _runCurrencyAccumulator, _config, _clock, _runPreparationStateId, _runningStateId, _runEndedStateId);
    }

    private void ActivateRun()
    {
        _stateService.ChangeTo(_runningStateId);
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent());
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = stateName;
        _objects.Add(stateId);

        return stateId;
    }

    private CurrencyDefinition CreateCurrencyDefinition(string resourceName)
    {
        var currencyDefinition = ScriptableObject.CreateInstance<CurrencyDefinition>();
        currencyDefinition.name = resourceName;
        _objects.Add(currencyDefinition);

        return currencyDefinition;
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
        public List<GameplayStateId> RequestedStateIds { get; } = new();

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
            RequestedStateIds.Add(nextStateId);
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

    private sealed class FakeContactNotifier : IRigidbodyContactNotifier
    {
        public event Action<RigidbodyCollisionNotification> CollisionEntered;
        public event Action<RigidbodyTriggerNotification> TriggerEntered;

        public void RaiseTrigger(RigidbodyTriggerNotification notification)
        {
            TriggerEntered?.Invoke(notification);
        }
    }

    private sealed class FakeRunContactClassifier : IRunContactClassifier
    {
        public RunEndCandidate? CollisionCandidate { get; set; }
        public RunEndCandidate? TriggerCandidate { get; set; }

        public bool TryClassify(RigidbodyCollisionNotification notification, out RunEndCandidate candidate)
        {
            candidate = CollisionCandidate.GetValueOrDefault();
            return CollisionCandidate.HasValue;
        }

        public bool TryClassify(RigidbodyTriggerNotification notification, out RunEndCandidate candidate)
        {
            candidate = TriggerCandidate.GetValueOrDefault();
            return TriggerCandidate.HasValue;
        }
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

    private sealed class FakeRunEndConfig : IRunEndConfig
    {
        public float ObstacleImpactSpeedThreshold { get; set; }
        public float LostMomentumLaunchGraceDuration { get; set; }
        public float LostMomentumDuration { get; set; }
        public float LostMomentumPlanarSpeedThreshold { get; set; }
        public float LostMomentumProgressThreshold { get; set; }
        public float RunEndedDelay { get; set; }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
