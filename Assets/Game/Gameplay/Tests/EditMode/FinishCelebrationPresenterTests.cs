using System;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class FinishCelebrationPresenterTests
{
    private readonly System.Collections.Generic.List<UnityEngine.Object> _objects = new();
    private GameplayStateId _runPreparationStateId;
    private GameplayStateId _runningStateId;
    private FakeGameplayStateService _stateService;
    private FakeRunResultNotifier _notifier;
    private FakeFinishPresentationView _view;
    private FinishCelebrationPresenter _presenter;

    [SetUp]
    public void OnSetUp()
    {
        _runPreparationStateId = CreateStateId("RunPreparation");
        _runningStateId = CreateStateId("Running");
        _stateService = new FakeGameplayStateService(_runningStateId);
        _notifier = new FakeRunResultNotifier();
        _view = new FakeFinishPresentationView();
        _presenter = CreatePresenter();
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
    public void Initialize_CurrentRunPreparation_ResetsPresentationView()
    {
        _stateService.ChangeTo(_runPreparationStateId);

        ((IInitializable)_presenter).Initialize();

        Assert.That(_view.ResetCount, Is.EqualTo(1));
        Assert.That(_view.PlaySuccessCelebrationCount, Is.EqualTo(0));
    }

    [Test]
    public void RunResultAccepted_SuccessfulResult_PlaysSuccessCelebrationOnce()
    {
        ((IInitializable)_presenter).Initialize();

        _notifier.Raise(CreateResult(RunEndReason.Finished));
        _notifier.Raise(CreateResult(RunEndReason.Finished));

        Assert.That(_view.PlaySuccessCelebrationCount, Is.EqualTo(1));
        Assert.That(_view.ResetCount, Is.EqualTo(0));
    }

    [Test]
    public void RunResultAccepted_FailedResult_DoesNotPlaySuccessCelebration()
    {
        ((IInitializable)_presenter).Initialize();

        _notifier.Raise(CreateResult(RunEndReason.ObstacleHit));
        _notifier.Raise(CreateResult(RunEndReason.OutOfBounds));
        _notifier.Raise(CreateResult(RunEndReason.LostMomentum));

        Assert.That(_view.PlaySuccessCelebrationCount, Is.EqualTo(0));
    }

    [Test]
    public void GameplayStateChanged_ToRunPreparation_ResetsPresentationAndAllowsNextSuccess()
    {
        ((IInitializable)_presenter).Initialize();
        _notifier.Raise(CreateResult(RunEndReason.Finished));

        _stateService.ChangeTo(_runPreparationStateId);
        _stateService.ChangeTo(_runningStateId);
        _notifier.Raise(CreateResult(RunEndReason.Finished));

        Assert.That(_view.ResetCount, Is.EqualTo(1));
        Assert.That(_view.PlaySuccessCelebrationCount, Is.EqualTo(2));
    }

    [Test]
    public void Dispose_UnsubscribesFromAcceptedResultsAndStateChanges()
    {
        ((IInitializable)_presenter).Initialize();
        ((IDisposable)_presenter).Dispose();

        _notifier.Raise(CreateResult(RunEndReason.Finished));
        _stateService.ChangeTo(_runPreparationStateId);

        Assert.That(_view.PlaySuccessCelebrationCount, Is.EqualTo(0));
        Assert.That(_view.ResetCount, Is.EqualTo(0));
    }

    private FinishCelebrationPresenter CreatePresenter()
    {
        return new FinishCelebrationPresenter(_stateService, _notifier, _view, _runPreparationStateId);
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = stateName;
        _objects.Add(stateId);
        return stateId;
    }

    private static RunResult CreateResult(RunEndReason reason)
    {
        return new RunResult(
            reason,
            elapsedTime: 1f,
            distanceTravelled: 10f,
            finalPosition: Vector3.forward,
            finalSpeed: 2f,
            rewardBreakdown: new RunRewardBreakdown(Array.Empty<RunRewardSourceAmount>()));
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

    private sealed class FakeRunResultNotifier : IRunResultNotifier
    {
        public event Action<RunResult> RunResultAccepted;

        public void Raise(RunResult result)
        {
            RunResultAccepted?.Invoke(result);
        }
    }

    private sealed class FakeFinishPresentationView : IFinishPresentationView
    {
        public int PlaySuccessCelebrationCount { get; private set; }
        public int ResetCount { get; private set; }

        public void PlaySuccessCelebration()
        {
            PlaySuccessCelebrationCount += 1;
        }

        public void ResetForRunPreparation()
        {
            ResetCount += 1;
        }
    }
}
