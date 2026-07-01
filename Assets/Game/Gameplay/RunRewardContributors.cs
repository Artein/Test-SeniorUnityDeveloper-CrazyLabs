using System;
using System.Collections.Generic;
using System.Linq;
using Game.Gameplay.Economy;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    public readonly struct RunRewardContributorContext
    {
        public RunEndReason Reason { get; }
        public float ElapsedTime { get; }
        public float DistanceTravelled { get; }
        public Vector3 FinalPosition { get; }
        public float FinalSpeed { get; }
        public float AirTimeSeconds { get; }

        public RunRewardContributorContext(
            RunEndReason reason,
            float elapsedTime,
            float distanceTravelled,
            Vector3 finalPosition,
            float finalSpeed,
            float airTimeSeconds)
        {
            Reason = reason;
            ElapsedTime = Mathf.Max(0f, elapsedTime);
            DistanceTravelled = Mathf.Max(0f, distanceTravelled);
            FinalPosition = finalPosition;
            FinalSpeed = Mathf.Max(0f, finalSpeed);
            AirTimeSeconds = Mathf.Max(0f, airTimeSeconds);
        }
    }

    public interface IRunRewardContributor
    {
        IReadOnlyList<RunRewardSourceAmount> CreateSourceAmounts(RunRewardContributorContext context);
    }

    public sealed class AccumulatedRunRewardContributor : IRunRewardContributor
    {
        private readonly IRunRewardSourceLedger _sourceLedger;

        public AccumulatedRunRewardContributor(IRunRewardSourceLedger sourceLedger)
        {
            _sourceLedger = sourceLedger ?? throw new ArgumentNullException(nameof(sourceLedger));
        }

        public IReadOnlyList<RunRewardSourceAmount> CreateSourceAmounts(RunRewardContributorContext context)
        {
            return _sourceLedger.CreateRewardSourceAmounts();
        }
    }

    public sealed class DistanceBonusRunRewardContributor : IRunRewardContributor
    {
        private readonly CurrencyDefinition _coinCurrencyDefinition;
        private readonly RunRewardSourceCatalog _sourceCatalog;
        private readonly IRunRewardConfig _config;

        public DistanceBonusRunRewardContributor(
            [Key(GameplayState.InjectKey.CurrencyDefinition.Coin)]
            CurrencyDefinition coinCurrencyDefinition,
            RunRewardSourceCatalog sourceCatalog,
            IRunRewardConfig config)
        {
            _coinCurrencyDefinition = coinCurrencyDefinition != null
                ? coinCurrencyDefinition
                : throw new ArgumentNullException(nameof(coinCurrencyDefinition));

            _sourceCatalog = sourceCatalog ?? throw new ArgumentNullException(nameof(sourceCatalog));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IReadOnlyList<RunRewardSourceAmount> CreateSourceAmounts(RunRewardContributorContext context)
        {
            var amount = Mathf.FloorToInt(context.DistanceTravelled * Mathf.Max(0f, _config.DistanceBonusCoinsPerMeter));
            return new[] { new RunRewardSourceAmount(_sourceCatalog.DistanceBonus, _coinCurrencyDefinition, amount) };
        }
    }

    public sealed class AirTimeBonusRunRewardContributor : IRunRewardContributor
    {
        private readonly CurrencyDefinition _coinCurrencyDefinition;
        private readonly RunRewardSourceCatalog _sourceCatalog;
        private readonly IRunRewardConfig _config;

        public AirTimeBonusRunRewardContributor(
            [Key(GameplayState.InjectKey.CurrencyDefinition.Coin)]
            CurrencyDefinition coinCurrencyDefinition,
            RunRewardSourceCatalog sourceCatalog,
            IRunRewardConfig config)
        {
            _coinCurrencyDefinition = coinCurrencyDefinition != null
                ? coinCurrencyDefinition
                : throw new ArgumentNullException(nameof(coinCurrencyDefinition));

            _sourceCatalog = sourceCatalog ?? throw new ArgumentNullException(nameof(sourceCatalog));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IReadOnlyList<RunRewardSourceAmount> CreateSourceAmounts(RunRewardContributorContext context)
        {
            var amount = Mathf.FloorToInt(context.AirTimeSeconds * Mathf.Max(0f, _config.AirTimeBonusCoinsPerSecond));
            return new[] { new RunRewardSourceAmount(_sourceCatalog.AirTimeBonus, _coinCurrencyDefinition, amount) };
        }
    }

    public sealed class RunRewardBreakdownBuilder
    {
        private readonly IRunRewardContributor[] _contributors;

        public RunRewardBreakdownBuilder(IEnumerable<IRunRewardContributor> contributors)
        {
            if (contributors is null)
                throw new ArgumentNullException(nameof(contributors));

            _contributors = contributors.Where(contributor => contributor != null).ToArray();
        }

        public RunRewardBreakdown Build(RunRewardContributorContext context)
        {
            var sourceAmounts = new List<RunRewardSourceAmount>();

            for (var contributorIndex = 0; contributorIndex < _contributors.Length; contributorIndex += 1)
            {
                var contributorAmounts = _contributors[contributorIndex].CreateSourceAmounts(context);

                if (contributorAmounts == null)
                    continue;

                for (var amountIndex = 0; amountIndex < contributorAmounts.Count; amountIndex += 1)
                {
                    sourceAmounts.Add(contributorAmounts[amountIndex]);
                }
            }

            return new RunRewardBreakdown(sourceAmounts);
        }
    }
}
