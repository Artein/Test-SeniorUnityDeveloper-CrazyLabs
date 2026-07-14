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
public sealed partial class RunEndFlowTests
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
    private RunRewardSourceCatalog _runRewardSourceCatalog;
    private RunRewardBreakdownBuilder _runRewardBreakdownBuilder;
    private CapturingRunRewardContributor _capturingRewardContributor;
    private FakeRunAirTimeSource _runAirTimeSource;
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
        RunProgressFrameSnapshot.TryCreate(Vector3.zero, Vector3.forward, Vector3.up, out var progressSnapshot, out _);

        _progressService = new FakeRunProgressService
        {
            HasValidSnapshot = true,
            Snapshot = progressSnapshot,
            MaximumForwardProgress = 12.5f
        };

        _motionSource = new FakeRunMotionSource
        {
            Position = new Vector3(1f, 2f, 3f),
            LinearVelocity = new Vector3(0f, 0f, 4f)
        };

        _runCurrencyAccumulator = new RunCurrencyAccumulator();
        _runRewardSourceCatalog = new RunRewardSourceCatalog();
        _capturingRewardContributor = new CapturingRunRewardContributor();

        _runRewardBreakdownBuilder = new RunRewardBreakdownBuilder(
            new IRunRewardContributor[]
            {
                new AccumulatedRunRewardContributor(_runCurrencyAccumulator),
                _capturingRewardContributor
            });

        _runAirTimeSource = new FakeRunAirTimeSource();
        _config = new FakeRunEndConfig { RunEndedAcknowledgeGuardDuration = 0.2f };
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
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(_stateService.RequestedStateIds, Is.Empty);
    }

    [Test]
    public void FixedTick_RunningWithoutCandidates_DoesNotSampleProgressService()
    {
        ActivateRun();

        var fixedStepResult = ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(_progressService.SamplePositionCallCount, Is.Zero);
        Assert.That(fixedStepResult, Is.EqualTo(RunEndFixedStepResult.ContinueRunSteps));
    }

    [Test]
    public void FixedTick_RunEndedGuardElapsedWithoutAcknowledge_StaysRunEnded()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=OutOfBounds"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.OutOfBounds));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runEndedStateId));
        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId }));
    }

    [Test]
    public void TryAcknowledge_BeforeGuardElapsed_ReturnsFalseAndStaysRunEnded()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=OutOfBounds"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.OutOfBounds));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        var acknowledged = ((IRunResultAcknowledgeCommand)_flow).TryAcknowledge();

        Assert.That(acknowledged, Is.False);
        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runEndedStateId));
        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId }));
    }

    [Test]
    public void TryAcknowledge_AfterGuardElapsed_TransitionsBackToRunPreparation()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=OutOfBounds"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.OutOfBounds));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        var acknowledged = ((IRunResultAcknowledgeCommand)_flow).TryAcknowledge();

        Assert.That(acknowledged, Is.True);
        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runPreparationStateId));
        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId, _runPreparationStateId }));
    }

    [Test]
    public void TryAcknowledge_NotAwaitingRunEnded_ReturnsFalse()
    {
        var acknowledged = ((IRunResultAcknowledgeCommand)_flow).TryAcknowledge();

        Assert.That(acknowledged, Is.False);
        Assert.That(_stateService.RequestedStateIds, Is.Empty);
    }

    private RunEndFlow CreateFlow()
    {
        return new RunEndFlow(
            _stateService,
            _launchAppliedNotifier,
            _contactNotifier,
            _contactClassifier,
            _progressService,
            _motionSource,
            _runCurrencyAccumulator,
            _runRewardBreakdownBuilder,
            _runAirTimeSource,
            _config,
            _clock,
            _runPreparationStateId,
            _runningStateId,
            _runEndedStateId);
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
        public bool TryTransitionResult { get; set; } = true;

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

            if (!TryTransitionResult)
                return false;

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

        public RunProgressSample CurrentSample => new(
            HasValidSnapshot,
            SnapshotError,
            Snapshot,
            CurrentForwardProgress,
            MaximumForwardProgress);

        public int SamplePositionCallCount { get; private set; }

        public bool TryBeginRun(Vector3 origin, out string error)
        {
            error = string.Empty;
            return HasValidSnapshot;
        }

        public void SamplePosition(Vector3 position)
        {
            SamplePositionCallCount += 1;
        }

        public void Reset()
        {
            HasValidSnapshot = false;
            SnapshotError = string.Empty;
            Snapshot = default;
            CurrentForwardProgress = 0f;
            MaximumForwardProgress = 0f;
        }
    }

    private sealed class FakeRunMotionSource : IRunMotionSource
    {
        public Vector3 Position { get; set; }
        public Vector3 LinearVelocity { get; set; }
    }

    private sealed class FakeRunAirTimeSource : IRunAirTimeSource
    {
        public float CurrentRunAirTimeSeconds { get; set; }
    }

    private sealed class CapturingRunRewardContributor : IRunRewardContributor
    {
        public RunRewardContributorContext? LastContext { get; private set; }

        public IReadOnlyList<RunRewardSourceAmount> CreateSourceAmounts(RunRewardContributorContext context)
        {
            LastContext = context;
            return Array.Empty<RunRewardSourceAmount>();
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
