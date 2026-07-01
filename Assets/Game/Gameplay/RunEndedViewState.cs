using System;
using System.Collections.Generic;

namespace Game.Gameplay
{
    public readonly struct RunEndedRewardSourceRowViewState
    {
        public string LabelText { get; }
        public int Amount { get; }
        public string AmountText { get; }

        public RunEndedRewardSourceRowViewState(string labelText, int amount, string amountText)
        {
            LabelText = labelText ?? string.Empty;
            Amount = Math.Max(0, amount);
            AmountText = amountText ?? string.Empty;
        }
    }

    public readonly struct RunEndedViewState
    {
        private static readonly RunEndedRewardSourceRowViewState[] EmptyRewardSourceRows = Array.Empty<RunEndedRewardSourceRowViewState>();

        public bool IsVisible { get; }
        public bool IsSuccess { get; }
        public string TitleText { get; }
        public int EarnedCoins { get; }
        public string EarnedCoinsText { get; }
        public IReadOnlyList<RunEndedRewardSourceRowViewState> RewardSourceRows { get; }
        public int ReachedMeters { get; }
        public string ReachedDistanceText { get; }
        public bool HasBestImprovement { get; }
        public int BestImprovementMeters { get; }
        public string BestImprovementText { get; }

        public RunEndedViewState(
            bool isVisible,
            bool isSuccess,
            string titleText,
            int earnedCoins,
            string earnedCoinsText,
            int reachedMeters,
            string reachedDistanceText,
            bool hasBestImprovement,
            int bestImprovementMeters,
            string bestImprovementText,
            IEnumerable<RunEndedRewardSourceRowViewState> rewardSourceRows = null)
        {
            IsVisible = isVisible;
            IsSuccess = isSuccess;
            TitleText = titleText ?? string.Empty;
            EarnedCoins = Math.Max(0, earnedCoins);
            EarnedCoinsText = earnedCoinsText ?? string.Empty;
            RewardSourceRows = CopyRewardSourceRows(rewardSourceRows);
            ReachedMeters = Math.Max(0, reachedMeters);
            ReachedDistanceText = reachedDistanceText ?? string.Empty;
            HasBestImprovement = hasBestImprovement;
            BestImprovementMeters = Math.Max(0, bestImprovementMeters);
            BestImprovementText = bestImprovementText ?? string.Empty;
        }

        private static IReadOnlyList<RunEndedRewardSourceRowViewState> CopyRewardSourceRows(
            IEnumerable<RunEndedRewardSourceRowViewState> rewardSourceRows)
        {
            if (rewardSourceRows == null)
                return EmptyRewardSourceRows;

            if (rewardSourceRows is RunEndedRewardSourceRowViewState[] rowArray)
                return (RunEndedRewardSourceRowViewState[])rowArray.Clone();

            var copiedRows = new List<RunEndedRewardSourceRowViewState>();

            foreach (var rewardSourceRow in rewardSourceRows)
            {
                copiedRows.Add(rewardSourceRow);
            }

            return copiedRows.Count > 0 ? copiedRows.ToArray() : EmptyRewardSourceRows;
        }
    }
}
