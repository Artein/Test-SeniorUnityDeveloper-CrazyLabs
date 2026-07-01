using System;

namespace Game.Gameplay
{
    public readonly struct RunEndedViewState
    {
        public bool IsVisible { get; }
        public bool IsSuccess { get; }
        public string TitleText { get; }
        public int EarnedCoins { get; }
        public string EarnedCoinsText { get; }
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
            string bestImprovementText)
        {
            IsVisible = isVisible;
            IsSuccess = isSuccess;
            TitleText = titleText ?? string.Empty;
            EarnedCoins = Math.Max(0, earnedCoins);
            EarnedCoinsText = earnedCoinsText ?? string.Empty;
            ReachedMeters = Math.Max(0, reachedMeters);
            ReachedDistanceText = reachedDistanceText ?? string.Empty;
            HasBestImprovement = hasBestImprovement;
            BestImprovementMeters = Math.Max(0, bestImprovementMeters);
            BestImprovementText = bestImprovementText ?? string.Empty;
        }
    }
}
