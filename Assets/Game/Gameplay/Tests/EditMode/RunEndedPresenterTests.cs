using System;
using System.Collections.Generic;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunEndedPresenterTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private GameplayStateId _runPreparationStateId;
    private GameplayStateId _runEndedStateId;
    private CurrencyDefinition _coins;
    private FakeGameplayStateService _stateService;
    private FakeRunResultNotifier _runResultNotifier;
    private FakeRunResultAcknowledgeCommand _acknowledgeCommand;
    private FakeRunEndedView _view;
    private RunEndedPresenter _presenter;

    [SetUp]
    public void OnSetUp()
    {
        _runPreparationStateId = CreateStateId("Run Preparation");
        _runEndedStateId = CreateStateId("Run Ended");
        _coins = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _coins.SetSaveIdForTests("currency-coins");
        _stateService = new FakeGameplayStateService(_runPreparationStateId);
        _runResultNotifier = new FakeRunResultNotifier();
        _acknowledgeCommand = new FakeRunResultAcknowledgeCommand();
        _view = new FakeRunEndedView();
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
    public void Initialize_NotRunEnded_RendersHiddenState()
    {
        ((IInitializable)_presenter).Initialize();

        Assert.That(_view.RenderedStates, Has.Count.EqualTo(1));
        Assert.That(_view.RenderedStates[^1].IsVisible, Is.False);
        Assert.That(_view.RenderedStates[^1].TitleText, Is.Empty);
        Assert.That(_view.RenderedStates[^1].RewardSourceRows, Is.Empty);
    }

    [Test]
    public void RunResultAccepted_CurrentRunEnded_RendersAcceptedResultStats()
    {
        ((IInitializable)_presenter).Initialize();
        _stateService.ChangeTo(_runEndedStateId);

        _runResultNotifier.Raise(CreateRunResult(RunEndReason.Finished, 17.9f, 5));

        var state = _view.RenderedStates[^1];
        Assert.That(state.IsVisible, Is.True);
        Assert.That(state.IsSuccess, Is.True);
        Assert.That(state.TitleText, Is.EqualTo("VICTORY"));
        Assert.That(state.EarnedCoins, Is.EqualTo(5));
        Assert.That(state.EarnedCoinsText, Is.EqualTo("5"));
        var rewardSourceRows = state.RewardSourceRows.ToArray();
        Assert.That(rewardSourceRows, Has.Length.EqualTo(1));
        Assert.That(rewardSourceRows[0].LabelText, Is.EqualTo("Test Reward"));
        Assert.That(rewardSourceRows[0].Amount, Is.EqualTo(5));
        Assert.That(rewardSourceRows[0].AmountText, Is.EqualTo("5"));
        Assert.That(state.ReachedMeters, Is.EqualTo(17));
        Assert.That(state.ReachedDistanceText, Is.EqualTo("DISTANCE 17 m"));
        Assert.That(state.HasBestImprovement, Is.True);
        Assert.That(state.BestImprovementMeters, Is.EqualTo(17));
        Assert.That(state.BestImprovementText, Is.EqualTo("NEW BEST +17 m"));
    }

    [Test]
    public void RunResultAccepted_FailedRun_RendersFailureState()
    {
        ((IInitializable)_presenter).Initialize();
        _stateService.ChangeTo(_runEndedStateId);

        _runResultNotifier.Raise(CreateRunResult(RunEndReason.ObstacleHit, 4.2f, 1));

        var state = _view.RenderedStates[^1];
        Assert.That(state.IsVisible, Is.True);
        Assert.That(state.IsSuccess, Is.False);
        Assert.That(state.TitleText, Is.EqualTo("DEFEAT"));
    }

    [Test]
    public void RunResultAccepted_NotLongerThanSessionBest_HidesBestImprovementText()
    {
        ((IInitializable)_presenter).Initialize();
        _stateService.ChangeTo(_runEndedStateId);
        _runResultNotifier.Raise(CreateRunResult(RunEndReason.ObstacleHit, 20.4f, 1));

        _runResultNotifier.Raise(CreateRunResult(RunEndReason.ObstacleHit, 12.1f, 2));

        var state = _view.RenderedStates[^1];
        Assert.That(state.IsVisible, Is.True);
        Assert.That(state.HasBestImprovement, Is.False);
        Assert.That(state.BestImprovementMeters, Is.Zero);
        Assert.That(state.BestImprovementText, Is.Empty);
    }

    [Test]
    public void GameplayStateChanged_LeavingRunEnded_HidesViewWithoutDroppingSessionBest()
    {
        ((IInitializable)_presenter).Initialize();
        _stateService.ChangeTo(_runEndedStateId);
        _runResultNotifier.Raise(CreateRunResult(RunEndReason.ObstacleHit, 20.4f, 1));

        _stateService.ChangeTo(_runPreparationStateId);
        _stateService.ChangeTo(_runEndedStateId);
        _runResultNotifier.Raise(CreateRunResult(RunEndReason.ObstacleHit, 22.1f, 1));

        Assert.That(_view.RenderedStates[^2].IsVisible, Is.False);
        Assert.That(_view.RenderedStates[^1].HasBestImprovement, Is.True);
        Assert.That(_view.RenderedStates[^1].BestImprovementMeters, Is.EqualTo(2));
        Assert.That(_view.RenderedStates[^1].BestImprovementText, Is.EqualTo("NEW BEST +2 m"));
    }

    [Test]
    public void AcknowledgeRequested_DelegatesToAcknowledgeCommandAndRefreshesView()
    {
        ((IInitializable)_presenter).Initialize();
        _stateService.ChangeTo(_runEndedStateId);
        _runResultNotifier.Raise(CreateRunResult(RunEndReason.ObstacleHit, 7.1f, 3));

        _view.RequestAcknowledge();

        Assert.That(_acknowledgeCommand.CallCount, Is.EqualTo(1));
        Assert.That(_view.RenderedStates[^1].IsVisible, Is.True);
    }

    [Test]
    public void Dispose_AfterInitialize_UnsubscribesFromViewAndNotifier()
    {
        ((IInitializable)_presenter).Initialize();
        ((IDisposable)_presenter).Dispose();

        _stateService.ChangeTo(_runEndedStateId);
        _runResultNotifier.Raise(CreateRunResult(RunEndReason.Finished, 5f, 1));
        _view.RequestAcknowledge();

        Assert.That(_view.RenderedStates, Has.Count.EqualTo(1));
        Assert.That(_acknowledgeCommand.CallCount, Is.Zero);
    }

    private RunEndedPresenter CreatePresenter()
    {
        return new RunEndedPresenter(
            _view,
            _runResultNotifier,
            _acknowledgeCommand,
            _stateService,
            new RunEndedResultStatsBuilder(_coins, new RunSessionBestDistanceTracker()),
            _runEndedStateId);
    }

    private RunResult CreateRunResult(RunEndReason reason, float distance, int coins)
    {
        return new RunResult(
            reason,
            1f,
            distance,
            Vector3.zero,
            0f,
            new RunRewardBreakdown(new[]
            {
                new RunRewardSourceAmount(new RunRewardSource("test-reward", "Test Reward", 0, showWhenZero: false), _coins, coins)
            }));
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = Track(ScriptableObject.CreateInstance<GameplayStateId>());
        stateId.name = stateName;
        return stateId;
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }

    private sealed class FakeRunEndedView : IRunEndedView
    {
        public event Action AcknowledgeRequested;

        public List<RunEndedViewState> RenderedStates { get; } = new();

        public void Apply(RunEndedViewState state)
        {
            RenderedStates.Add(state);
        }

        public void RequestAcknowledge()
        {
            AcknowledgeRequested?.Invoke();
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

    private sealed class FakeRunResultAcknowledgeCommand : IRunResultAcknowledgeCommand
    {
        public int CallCount { get; private set; }

        public bool TryAcknowledge()
        {
            CallCount += 1;
            return true;
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
