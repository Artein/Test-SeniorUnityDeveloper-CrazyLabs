using System;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public sealed class RunAirTimeTrackerTests
{
    private readonly List<Object> _objects = new();
    private FakeTime _clock;
    private GameplayStateId _runningStateId;
    private GameplayStateId _runPreparationStateId;
    private FakeGameplayStateService _stateService;
    private FakeRunSurfaceFrameSource _surfaceFrameSource;
    private RunAirTimeTracker _tracker;

    [SetUp]
    public void OnSetUp()
    {
        _runPreparationStateId = CreateStateId(stateName: "Run Preparation");
        _runningStateId = CreateStateId(stateName: "Running");
        _stateService = new FakeGameplayStateService(currentStateId: _runPreparationStateId);
        _surfaceFrameSource = new FakeRunSurfaceFrameSource();

        _surfaceFrameSource.Publish(
            observationState: RunSupportObservationState.Supported,
            new RunSurfaceContext(isGrounded: true, groundNormal: Vector3.up, forwardDownhillDegrees: 0f),
            transition: RunSurfaceTransition.SupportAcquired);

        _clock = new FakeTime { FixedDeltaTime = 0.1f };

        _tracker = new RunAirTimeTracker(
            gameplayStateService: _stateService,
            surfaceFrameSource: _surfaceFrameSource,
            clock: _clock,
            runPreparationStateId: _runPreparationStateId,
            runningStateId: _runningStateId);

        ((IInitializable)_tracker).Initialize();
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_tracker).Dispose();

        foreach (var unityObject in _objects)
        {
            Object.DestroyImmediate(obj: unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void FixedTick_RunningAndUngrounded_AccumulatesFixedDeltaTime()
    {
        _stateService.ChangeTo(nextStateId: _runningStateId);

        _surfaceFrameSource.Publish(
            observationState: RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: false, groundNormal: Vector3.up, forwardDownhillDegrees: 0f),
            transition: RunSurfaceTransition.SupportLost);

        ((IFixedTickable)_tracker).FixedTick();
        ((IFixedTickable)_tracker).FixedTick();

        Assert.That(actual: _tracker.CurrentRunAirTimeSeconds, Is.EqualTo(expected: 0.2f).Within(amount: 0.0001f));
    }

    [Test]
    public void FixedTick_GroundedOrNotRunning_DoesNotAccumulate()
    {
        _surfaceFrameSource.Publish(
            observationState: RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: false, groundNormal: Vector3.up, forwardDownhillDegrees: 0f),
            transition: RunSurfaceTransition.SupportLost);

        ((IFixedTickable)_tracker).FixedTick();

        _stateService.ChangeTo(nextStateId: _runningStateId);

        _surfaceFrameSource.Publish(
            observationState: RunSupportObservationState.Supported,
            new RunSurfaceContext(isGrounded: true, groundNormal: Vector3.up, forwardDownhillDegrees: 0f),
            transition: RunSurfaceTransition.SupportAcquired);

        ((IFixedTickable)_tracker).FixedTick();

        Assert.That(actual: _tracker.CurrentRunAirTimeSeconds, expression: Is.Zero);
    }

    [Test]
    public void GameplayStateChanged_RunPreparation_ResetsCurrentRunAirTime()
    {
        _stateService.ChangeTo(nextStateId: _runningStateId);

        _surfaceFrameSource.Publish(
            observationState: RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: false, groundNormal: Vector3.up, forwardDownhillDegrees: 0f),
            transition: RunSurfaceTransition.SupportLost);

        ((IFixedTickable)_tracker).FixedTick();

        _stateService.ChangeTo(nextStateId: _runPreparationStateId);

        Assert.That(actual: _tracker.CurrentRunAirTimeSeconds, expression: Is.Zero);
    }

    [Test]
    public void FixedTick_ObservedMissHeldAsStableSupport_DoesNotAccumulate()
    {
        _stateService.ChangeTo(nextStateId: _runningStateId);

        _surfaceFrameSource.Publish(
            observationState: RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: true, groundNormal: Vector3.up, forwardDownhillDegrees: 0f),
            transition: RunSurfaceTransition.None,
            isMissingSupportHeld: true);

        ((IFixedTickable)_tracker).FixedTick();

        Assert.That(actual: _tracker.CurrentRunAirTimeSeconds, expression: Is.Zero);
    }

    [Test]
    public void FixedTick_ConfirmedSupportLossThenReacquisition_AccumulatesUntilStableSupportReturns()
    {
        _stateService.ChangeTo(nextStateId: _runningStateId);

        _surfaceFrameSource.Publish(
            observationState: RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: false, groundNormal: Vector3.up, forwardDownhillDegrees: 0f),
            transition: RunSurfaceTransition.SupportLost);

        ((IFixedTickable)_tracker).FixedTick();

        _surfaceFrameSource.Publish(
            observationState: RunSupportObservationState.Missing,
            new RunSurfaceContext(isGrounded: false, groundNormal: Vector3.up, forwardDownhillDegrees: 0f),
            transition: RunSurfaceTransition.None);

        ((IFixedTickable)_tracker).FixedTick();

        _surfaceFrameSource.Publish(
            observationState: RunSupportObservationState.Supported,
            new RunSurfaceContext(isGrounded: true, groundNormal: Vector3.up, forwardDownhillDegrees: 0f),
            transition: RunSurfaceTransition.SupportAcquired);

        ((IFixedTickable)_tracker).FixedTick();

        Assert.That(actual: _tracker.CurrentRunAirTimeSeconds, Is.EqualTo(expected: 0.2f).Within(amount: 0.0001f));
    }

    [Test]
    public void FixedTick_UnavailableHardReset_DoesNotAccumulate()
    {
        _stateService.ChangeTo(nextStateId: _runningStateId);

        _surfaceFrameSource.Publish(
            observationState: RunSupportObservationState.Unavailable,
            new RunSurfaceContext(isGrounded: false, groundNormal: Vector3.up, forwardDownhillDegrees: 0f),
            transition: RunSurfaceTransition.HardReset);

        ((IFixedTickable)_tracker).FixedTick();

        Assert.That(actual: _tracker.CurrentRunAirTimeSeconds, expression: Is.Zero);
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = stateName;
        _objects.Add(item: stateId);
        return stateId;
    }

    private sealed class FakeGameplayStateService : IGameplayStateService
    {
        public GameplayStateId CurrentStateId { get; private set; }

        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanged;
        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanging;

        public FakeGameplayStateService(GameplayStateId currentStateId)
        {
            CurrentStateId = currentStateId;
        }

        public bool IsCurrent(GameplayStateId stateId)
        {
            return ReferenceEquals(objA: CurrentStateId, objB: stateId);
        }

        public bool TryTransitionTo(GameplayStateId nextStateId)
        {
            ChangeTo(nextStateId: nextStateId);
            return true;
        }

        public void ChangeTo(GameplayStateId nextStateId)
        {
            var previousStateId = CurrentStateId;
            GameplayStateChanging?.Invoke(arg1: nextStateId, arg2: previousStateId);
            CurrentStateId = nextStateId;
            GameplayStateChanged?.Invoke(arg1: nextStateId, arg2: previousStateId);
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
                origin: Vector3.zero,
                forwardDirection: Vector3.forward,
                upDirection: Vector3.up,
                out var progressFrame,
                error: out _);

            var observedContext = observationState == RunSupportObservationState.Supported
                ? new RunSurfaceContext(isGrounded: true, groundNormal: Vector3.up, forwardDownhillDegrees: 0f)
                : new RunSurfaceContext(isGrounded: false, groundNormal: Vector3.up, forwardDownhillDegrees: 0f);

            var observedSupport = new RunSupportObservation(
                state: observationState,
                observationState == RunSupportObservationState.Unavailable ? default : progressFrame,
                surfaceContext: observedContext,
                supportDistance: 0f);

            _current = new RunSurfaceFrameSnapshot(
                observedSupport: observedSupport,
                stableSupport: stableSupport,
                transition: transition,
                isMissingSupportHeld: isMissingSupportHeld,
                isConfirmingDiscontinuity: false,
                steeringFrame: default);
        }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
