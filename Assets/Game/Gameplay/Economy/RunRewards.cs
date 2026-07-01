using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Gameplay.Economy
{
    public readonly struct RunRewardSource
    {
        public string StableId { get; }
        public string Label { get; }
        public int Order { get; }
        public bool ShowWhenZero { get; }

        public RunRewardSource(string stableId, string label, int order, bool showWhenZero)
        {
            if (string.IsNullOrWhiteSpace(stableId))
                throw new ArgumentException("Run reward source requires a non-empty Stable Id.", nameof(stableId));

            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Run reward source requires a non-empty Label.", nameof(label));

            StableId = stableId;
            Label = label;
            Order = order;
            ShowWhenZero = showWhenZero;
        }
    }

    public readonly struct RunRewardSourceAmount
    {
        public RunRewardSource Source { get; }
        public CurrencyDefinition CurrencyDefinition { get; }
        public int Amount { get; }

        public RunRewardSourceAmount(RunRewardSource source, CurrencyDefinition currencyDefinition, int amount)
        {
            if (string.IsNullOrWhiteSpace(source.StableId))
                throw new ArgumentException("Run reward source amount requires a source with a non-empty Stable Id.", nameof(source));

            Source = source;

            CurrencyDefinition = currencyDefinition != null
                ? currencyDefinition
                : throw new ArgumentNullException(nameof(currencyDefinition));

            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Run reward source amount cannot be negative.");

            Amount = amount;
        }
    }

    public sealed class RunRewardBreakdown
    {
        private readonly RunRewardSourceAmount[] _sourceAmounts;

        public IReadOnlyList<RunRewardSourceAmount> SourceAmounts => _sourceAmounts;
        public RunCurrencySnapshot CurrencySnapshot { get; }

        public RunRewardBreakdown(IEnumerable<RunRewardSourceAmount> sourceAmounts)
        {
            if (sourceAmounts is null)
                throw new ArgumentNullException(nameof(sourceAmounts));

            _sourceAmounts = BuildSourceAmounts(sourceAmounts);
            CurrencySnapshot = BuildCurrencySnapshot(_sourceAmounts);
        }

        private RunRewardSourceAmount[] BuildSourceAmounts(IEnumerable<RunRewardSourceAmount> sourceAmounts)
        {
            var sourcesByStableId = new Dictionary<string, RunRewardSource>(StringComparer.Ordinal);
            var amountsBySourceCurrency = new Dictionary<RunRewardSourceCurrencyKey, int>();

            foreach (var sourceAmount in sourceAmounts)
            {
                ValidateSourceIdentity(sourceAmount.Source, sourcesByStableId);
                var key = new RunRewardSourceCurrencyKey(sourceAmount.Source.StableId, sourceAmount.CurrencyDefinition);
                var currentAmount = amountsBySourceCurrency.GetValueOrDefault(key, 0);
                amountsBySourceCurrency[key] = checked(currentAmount + sourceAmount.Amount);
            }

            return amountsBySourceCurrency
                .Select(pair => new RunRewardSourceAmount(
                    sourcesByStableId[pair.Key.SourceStableId],
                    pair.Key.CurrencyDefinition,
                    pair.Value))
                .Where(sourceAmount => sourceAmount.Amount > 0 || sourceAmount.Source.ShowWhenZero)
                .OrderBy(sourceAmount => sourceAmount.Source.Order)
                .ThenBy(sourceAmount => sourceAmount.Source.StableId, StringComparer.Ordinal)
                .ThenBy(sourceAmount => GetCurrencySortKey(sourceAmount.CurrencyDefinition), StringComparer.Ordinal)
                .ToArray();
        }

        private void ValidateSourceIdentity(RunRewardSource source, IDictionary<string, RunRewardSource> sourcesByStableId)
        {
            if (!sourcesByStableId.TryGetValue(source.StableId, out var existingSource))
            {
                sourcesByStableId.Add(source.StableId, source);
                return;
            }

            if (string.Equals(existingSource.Label, source.Label, StringComparison.Ordinal)
                && existingSource.Order == source.Order
                && existingSource.ShowWhenZero == source.ShowWhenZero)
            {
                return;
            }

            throw new ArgumentException(
                "Run reward sources with the same Stable Id must use the same label, order, and zero-display policy.",
                nameof(source));
        }

        private RunCurrencySnapshot BuildCurrencySnapshot(IEnumerable<RunRewardSourceAmount> sourceAmounts)
        {
            var amountsByCurrency = new Dictionary<CurrencyDefinition, int>();

            foreach (var sourceAmount in sourceAmounts)
            {
                if (sourceAmount.Amount <= 0)
                    continue;

                var currentAmount = amountsByCurrency.GetValueOrDefault(sourceAmount.CurrencyDefinition, 0);
                amountsByCurrency[sourceAmount.CurrencyDefinition] = checked(currentAmount + sourceAmount.Amount);
            }

            return new RunCurrencySnapshot(amountsByCurrency.Select(pair => new RunCurrencyAmount(pair.Key, pair.Value)));
        }

        private string GetCurrencySortKey(CurrencyDefinition currencyDefinition)
        {
            if (currencyDefinition == null)
                return string.Empty;

            return string.IsNullOrWhiteSpace(currencyDefinition.SaveId)
                ? currencyDefinition.name ?? string.Empty
                : currencyDefinition.SaveId;
        }

        private readonly struct RunRewardSourceCurrencyKey : IEquatable<RunRewardSourceCurrencyKey>
        {
            public string SourceStableId { get; }
            public CurrencyDefinition CurrencyDefinition { get; }

            public RunRewardSourceCurrencyKey(string sourceStableId, CurrencyDefinition currencyDefinition)
            {
                SourceStableId = sourceStableId ?? string.Empty;
                CurrencyDefinition = currencyDefinition;
            }

            public bool Equals(RunRewardSourceCurrencyKey other)
            {
                return string.Equals(SourceStableId, other.SourceStableId, StringComparison.Ordinal)
                       && ReferenceEquals(CurrencyDefinition, other.CurrencyDefinition);
            }

            public override bool Equals(object obj)
            {
                return obj is RunRewardSourceCurrencyKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((SourceStableId != null ? StringComparer.Ordinal.GetHashCode(SourceStableId) : 0) * 397)
                           ^ (CurrencyDefinition != null ? CurrencyDefinition.GetHashCode() : 0);
                }
            }
        }
    }

    public interface IRunRewardSourceLedger
    {
        IReadOnlyList<RunRewardSourceAmount> CreateRewardSourceAmounts();
    }
}
