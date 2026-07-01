using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunEndPoseLockControllerTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private GameplayStateId _runPreparationStateId;
    private GameplayStateId _runEndedStateId;
    private FakeRunEndPoseLockTarget _target;
    private FakeRunResultNotifier _runResultNotifier;
    private FakeGameplayStateService _stateService;
    private RunEndPoseLockController _controller;

    [SetUp]
    public void OnSetUp()
    {
        _runPreparationStateId = CreateStateId("Run Preparation");
        _runEndedStateId = CreateStateId("Run Ended");
        _target = new FakeRunEndPoseLockTarget();
        _runResultNotifier = new FakeRunResultNotifier();
        _stateService = new FakeGameplayStateService(_runEndedStateId);
        _controller = new RunEndPoseLockController(_target, _runResultNotifier, _stateService, _runPreparationStateId);
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_controller).Dispose();

        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void RunResultAccepted_HoldsAcceptedFinalPosition()
    {
        ((IInitializable)_controller).Initialize();
        var finalPosition = new Vector3(1f, 2f, 3f);

        _runResultNotifier.Raise(CreateRunResult(finalPosition));

        Assert.That(_target.HeldPositions, Is.EqualTo(new[] { finalPosition }));
        Assert.That(_target.ReleaseCount, Is.Zero);
    }

    [Test]
    public void GameplayStateChanged_ToRunPreparation_ReleasesHeldPose()
    {
        ((IInitializable)_controller).Initialize();
        _runResultNotifier.Raise(CreateRunResult(new Vector3(1f, 2f, 3f)));

        _stateService.ChangeTo(_runPreparationStateId);

        Assert.That(_target.ReleaseCount, Is.EqualTo(1));
    }

    [Test]
    public void RunResultAccepted_WhileAlreadyLocked_IgnoresDuplicateResult()
    {
        ((IInitializable)_controller).Initialize();
        var finalPosition = new Vector3(1f, 2f, 3f);

        _runResultNotifier.Raise(CreateRunResult(finalPosition));
        _runResultNotifier.Raise(CreateRunResult(finalPosition));

        Assert.That(_target.HeldPositions, Is.EqualTo(new[] { finalPosition }));
        Assert.That(_target.ReleaseCount, Is.Zero);
    }

    [Test]
    public void GameplayStateChanged_ToRunPreparationWithoutAcceptedResult_DoesNotRelease()
    {
        ((IInitializable)_controller).Initialize();

        _stateService.ChangeTo(_runPreparationStateId);

        Assert.That(_target.ReleaseCount, Is.Zero);
    }

    [Test]
    public void Dispose_AfterInitialize_UnsubscribesFromNotifierAndStateService()
    {
        ((IInitializable)_controller).Initialize();
        ((IDisposable)_controller).Dispose();

        _runResultNotifier.Raise(CreateRunResult(new Vector3(1f, 2f, 3f)));
        _stateService.ChangeTo(_runPreparationStateId);

        Assert.That(_target.HeldPositions, Is.Empty);
        Assert.That(_target.ReleaseCount, Is.Zero);
    }

    private RunResult CreateRunResult(Vector3 finalPosition)
    {
        return new RunResult(
            reason: RunEndReason.ObstacleHit,
            elapsedTime: 1f,
            distanceTravelled: 2f,
            finalPosition: finalPosition,
            finalSpeed: 0f,
            currencySnapshot: new RunCurrencySnapshot(Array.Empty<RunCurrencyAmount>()));
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = stateName;
        _objects.Add(stateId);
        return stateId;
    }

    private sealed class FakeRunEndPoseLockTarget : IRunEndPoseLockTarget
    {
        public List<Vector3> HeldPositions { get; } = new();
        public int ReleaseCount { get; private set; }

        public void HoldRunEndPose(Vector3 position)
        {
            HeldPositions.Add(position);
        }

        public void ReleaseRunEndPose()
        {
            ReleaseCount += 1;
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
}
