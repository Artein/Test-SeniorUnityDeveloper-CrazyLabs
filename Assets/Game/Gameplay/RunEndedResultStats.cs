using System;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    public readonly struct RunEndedResultStats
    {
        public int EarnedCoins { get; }
        public int ReachedMeters { get; }
        public bool HasBestImprovement { get; }
        public int BestImprovementMeters { get; }

        public RunEndedResultStats(
            int earnedCoins,
            int reachedMeters,
            bool hasBestImprovement,
            int bestImprovementMeters)
        {
            EarnedCoins = Math.Max(0, earnedCoins);
            ReachedMeters = Math.Max(0, reachedMeters);
            HasBestImprovement = hasBestImprovement;
            BestImprovementMeters = Math.Max(0, bestImprovementMeters);
        }
    }

    public sealed class RunSessionBestDistanceTracker
    {
        public float BestDistance { get; private set; }

        public void Record(float distance)
        {
            BestDistance = Mathf.Max(BestDistance, Mathf.Max(0f, distance));
        }
    }

    public sealed class RunEndedResultStatsBuilder
    {
        private readonly CurrencyDefinition _coinCurrencyDefinition;
        private readonly RunSessionBestDistanceTracker _bestDistanceTracker;

        public RunEndedResultStatsBuilder(
            [Key(InjectKey.CurrencyDefinition.Coin)]
            CurrencyDefinition coinCurrencyDefinition,
            RunSessionBestDistanceTracker bestDistanceTracker)
        {
            _coinCurrencyDefinition = coinCurrencyDefinition;
            _bestDistanceTracker = bestDistanceTracker ?? throw new ArgumentNullException(nameof(bestDistanceTracker));
        }

        public RunEndedResultStats Build(RunResult result)
        {
            var previousBestDistance = _bestDistanceTracker.BestDistance;
            var distance = Mathf.Max(0f, result.DistanceTravelled);
            var reachedMeters = Mathf.FloorToInt(distance);
            var previousBestMeters = Mathf.FloorToInt(Mathf.Max(0f, previousBestDistance));
            var improvementMeters = Mathf.Max(0, reachedMeters - previousBestMeters);

            _bestDistanceTracker.Record(distance);

            return new RunEndedResultStats(
                earnedCoins: result.CurrencySnapshot.GetAmount(_coinCurrencyDefinition),
                reachedMeters: reachedMeters,
                hasBestImprovement: improvementMeters > 0,
                bestImprovementMeters: improvementMeters);
        }
    }
}
