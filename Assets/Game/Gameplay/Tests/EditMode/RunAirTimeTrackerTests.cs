using System;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunAirTimeTrackerTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private GameplayStateId _runPreparationStateId;
    private GameplayStateId _runningStateId;
    private FakeGameplayStateService _stateService;
    private FakeRunSurfaceFrameSource _surfaceFrameSource;
    private FakeTime _clock;
    private RunAirTimeTracker _tracker;

    [SetUp]
    public void OnSetUp()
    {
        _runPreparationStateId = CreateStateId("Run Preparation");
        _runningStateId = CreateStateId("Running");
        _stateService = new FakeGameplayStateService(_runPreparationStateId);

        _surfaceFrameSource = new FakeRunSurfaceFrameSource();

        _surfaceFrameSource.Publish(
            RunSupportObservationState.Supported,
            new RunSurfaceContext(isGrounded: true, Vector3.up, 0f),
            RunSurfaceTransition.SupportAcquired);
        _clock = new FakeTime { FixedDeltaTime = 0.1f };
        _tracker = new RunAirTimeTracker(_stateService, _surfaceFrameSource, _clock, _runPreparationStateId, _runningStateId);
        ((IInitializable)_tracker).Initialize();
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_tracker).Dispose();

        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void FixedTick_RunningAndUngrounded_AccumulatesFixedDeltaTime()
    {
        _stateService.ChangeTo(_runningStateId);

        _surfaceFrameSource.Publish(
            RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: false, Vector3.up, 0f),
            RunSurfaceTransition.SupportLost);

        ((IFixedTickable)_tracker).FixedTick();
        ((IFixedTickable)_tracker).FixedTick();

        Assert.That(((IRunAirTimeSource)_tracker).CurrentRunAirTimeSeconds, Is.EqualTo(0.2f).Within(0.0001f));
    }

    [Test]
    public void FixedTick_GroundedOrNotRunning_DoesNotAccumulate()
    {
        _surfaceFrameSource.Publish(
            RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: false, Vector3.up, 0f),
            RunSurfaceTransition.SupportLost);
        ((IFixedTickable)_tracker).FixedTick();

        _stateService.ChangeTo(_runningStateId);

        _surfaceFrameSource.Publish(
            RunSupportObservationState.Supported,
            new RunSurfaceContext(isGrounded: true, Vector3.up, 0f),
            RunSurfaceTransition.SupportAcquired);
        ((IFixedTickable)_tracker).FixedTick();

        Assert.That(((IRunAirTimeSource)_tracker).CurrentRunAirTimeSeconds, Is.Zero);
    }

    [Test]
    public void GameplayStateChanged_RunPreparation_ResetsCurrentRunAirTime()
    {
        _stateService.ChangeTo(_runningStateId);

        _surfaceFrameSource.Publish(
            RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: false, Vector3.up, 0f),
            RunSurfaceTransition.SupportLost);
        ((IFixedTickable)_tracker).FixedTick();

        _stateService.ChangeTo(_runPreparationStateId);

        Assert.That(((IRunAirTimeSource)_tracker).CurrentRunAirTimeSeconds, Is.Zero);
    }

    [Test]
    public void FixedTick_ObservedMissHeldAsStableSupport_DoesNotAccumulate()
    {
        _stateService.ChangeTo(_runningStateId);

        _surfaceFrameSource.Publish(
            RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: true, Vector3.up, 0f),
            RunSurfaceTransition.None,
            isMissingSupportHeld: true);

        ((IFixedTickable)_tracker).FixedTick();

        Assert.That(((IRunAirTimeSource)_tracker).CurrentRunAirTimeSeconds, Is.Zero);
    }

    [Test]
    public void FixedTick_ConfirmedSupportLossThenReacquisition_AccumulatesUntilStableSupportReturns()
    {
        _stateService.ChangeTo(_runningStateId);

        _surfaceFrameSource.Publish(
            RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: false, Vector3.up, 0f),
            RunSurfaceTransition.SupportLost);
        ((IFixedTickable)_tracker).FixedTick();

        _surfaceFrameSource.Publish(
            RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: false, Vector3.up, 0f),
            RunSurfaceTransition.None);
        ((IFixedTickable)_tracker).FixedTick();

        _surfaceFrameSource.Publish(
            RunSupportObservationState.Supported,
            new RunSurfaceContext(isGrounded: true, Vector3.up, 0f),
            RunSurfaceTransition.SupportAcquired);
        ((IFixedTickable)_tracker).FixedTick();

        Assert.That(((IRunAirTimeSource)_tracker).CurrentRunAirTimeSeconds, Is.EqualTo(0.2f).Within(0.0001f));
    }

    [Test]
    public void FixedTick_UnavailableHardReset_DoesNotAccumulate()
    {
        _stateService.ChangeTo(_runningStateId);

        _surfaceFrameSource.Publish(
            RunSupportObservationState.Unavailable,
            new RunSurfaceContext(isGrounded: false, Vector3.up, 0f),
            RunSurfaceTransition.HardReset);

        ((IFixedTickable)_tracker).FixedTick();

        Assert.That(((IRunAirTimeSource)_tracker).CurrentRunAirTimeSeconds, Is.Zero);
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

    private sealed class FakeRunSurfaceFrameSource : IRunSurfaceFrameSource
    {
        private RunSurfaceFrameSnapshot _current;

        RunSurfaceFrameSnapshot IRunSurfaceFrameSource.Current => _current;

        public void Publish(
            RunSupportObservationState observationState,
            RunSurfaceContext stableSupport,
            RunSurfaceTransition transition,
            bool isMissingSupportHeld = false)
        {
            RunProgressFrameSnapshot.TryCreate(
                Vector3.zero,
                Vector3.forward,
                Vector3.up,
                out var progressFrame,
                out _);

            var observedContext = observationState == RunSupportObservationState.Supported
                ? new RunSurfaceContext(true, Vector3.up, 0f)
                : new RunSurfaceContext(false, Vector3.up, 0f);

            var observedSupport = new RunSupportObservation(
                observationState,
                observationState == RunSupportObservationState.Unavailable ? default : progressFrame,
                observedContext,
                0f);

            _current = new RunSurfaceFrameSnapshot(
                observedSupport,
                stableSupport,
                transition,
                isMissingSupportHeld,
                false,
                default);
        }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
