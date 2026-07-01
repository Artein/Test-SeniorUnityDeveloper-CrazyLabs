using System;
using System.Collections.Generic;
using System.Globalization;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class RunEndedPresenter : IInitializable, IDisposable
    {
        private readonly IRunEndedView _view;
        private readonly IRunResultNotifier _runResultNotifier;
        private readonly IRunResultAcknowledgeCommand _acknowledgeCommand;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly RunEndedResultStatsBuilder _statsBuilder;
        private readonly GameplayStateId _runEndedStateId;

        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasAcceptedResultViewState;
        private RunEndedViewState _acceptedResultViewState;

        public RunEndedPresenter(
            IRunEndedView view,
            IRunResultNotifier runResultNotifier,
            IRunResultAcknowledgeCommand acknowledgeCommand,
            IGameplayStateService gameplayStateService,
            RunEndedResultStatsBuilder statsBuilder,
            [Key(InjectKey.GameplayStateId.RunEnded)]
            GameplayStateId runEndedStateId)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _runResultNotifier = runResultNotifier ?? throw new ArgumentNullException(nameof(runResultNotifier));
            _acknowledgeCommand = acknowledgeCommand ?? throw new ArgumentNullException(nameof(acknowledgeCommand));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _statsBuilder = statsBuilder ?? throw new ArgumentNullException(nameof(statsBuilder));

            _runEndedStateId = runEndedStateId != null
                ? runEndedStateId
                : throw new ArgumentNullException(nameof(runEndedStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunEndedPresenter));

            if (_isInitialized)
                return;

            _view.AcknowledgeRequested += OnAcknowledgeRequested;
            _runResultNotifier.RunResultAccepted += OnRunResultAccepted;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;

            Refresh();
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (!_isInitialized)
                return;

            _view.AcknowledgeRequested -= OnAcknowledgeRequested;
            _runResultNotifier.RunResultAccepted -= OnRunResultAccepted;
            _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
        }

        private void OnRunResultAccepted(RunResult result)
        {
            if (_isDisposed)
                return;

            _acceptedResultViewState = BuildVisibleViewState(result, _statsBuilder.Build(result));
            _hasAcceptedResultViewState = true;
            Refresh();
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (!ReferenceEquals(nextStateId, _runEndedStateId))
                _hasAcceptedResultViewState = false;

            Refresh();
        }

        private void OnAcknowledgeRequested()
        {
            if (_isDisposed)
                return;

            if (_acknowledgeCommand.TryAcknowledge())
                Refresh();
        }

        private void Refresh()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunEndedPresenter));

            _view.Apply(BuildViewState());
        }

        private RunEndedViewState BuildViewState()
        {
            if (!_gameplayStateService.IsCurrent(_runEndedStateId) || !_hasAcceptedResultViewState)
                return new RunEndedViewState(
                    isVisible: false,
                    isSuccess: false,
                    titleText: string.Empty,
                    earnedCoins: 0,
                    earnedCoinsText: string.Empty,
                    reachedMeters: 0,
                    reachedDistanceText: string.Empty,
                    hasBestImprovement: false,
                    bestImprovementMeters: 0,
                    bestImprovementText: string.Empty);

            return _acceptedResultViewState;
        }

        private static RunEndedViewState BuildVisibleViewState(RunResult result, RunEndedResultStats stats)
        {
            return new RunEndedViewState(
                isVisible: true,
                isSuccess: result.IsSuccess,
                titleText: FormatTitleText(result.IsSuccess),
                earnedCoins: stats.EarnedCoins,
                earnedCoinsText: FormatEarnedCoinsText(stats.EarnedCoins),
                reachedMeters: stats.ReachedMeters,
                reachedDistanceText: FormatReachedDistanceText(stats.ReachedMeters),
                hasBestImprovement: stats.HasBestImprovement,
                bestImprovementMeters: stats.BestImprovementMeters,
                bestImprovementText: FormatBestImprovementText(stats.HasBestImprovement, stats.BestImprovementMeters),
                rewardSourceRows: BuildRewardSourceRows(result.RewardBreakdown.SourceAmounts));
        }

        private static string FormatEarnedCoinsText(int earnedCoins)
        {
            return Math.Max(0, earnedCoins).ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatTitleText(bool isSuccess)
        {
            return isSuccess ? "VICTORY" : "DEFEAT";
        }

        private static string FormatReachedDistanceText(int reachedMeters)
        {
            return "DISTANCE " + reachedMeters.ToString(CultureInfo.InvariantCulture) + " m";
        }

        private static string FormatBestImprovementText(bool hasBestImprovement, int bestImprovementMeters)
        {
            return hasBestImprovement
                ? "NEW BEST +" + bestImprovementMeters.ToString(CultureInfo.InvariantCulture) + " m"
                : string.Empty;
        }

        private static IReadOnlyList<RunEndedRewardSourceRowViewState> BuildRewardSourceRows(
            IReadOnlyList<RunRewardSourceAmount> sourceAmounts)
        {
            if (sourceAmounts is not { Count: > 0 })
                return Array.Empty<RunEndedRewardSourceRowViewState>();

            var rows = new RunEndedRewardSourceRowViewState[sourceAmounts.Count];

            for (var sourceAmountIndex = 0; sourceAmountIndex < sourceAmounts.Count; sourceAmountIndex += 1)
            {
                var sourceAmount = sourceAmounts[sourceAmountIndex];

                rows[sourceAmountIndex] = new RunEndedRewardSourceRowViewState(
                    sourceAmount.Source.Label,
                    sourceAmount.Amount,
                    FormatRewardSourceAmountText(sourceAmount.Amount));
            }

            return rows;
        }

        private static string FormatRewardSourceAmountText(int amount)
        {
            return Math.Max(0, amount).ToString(CultureInfo.InvariantCulture);
        }
    }
}
