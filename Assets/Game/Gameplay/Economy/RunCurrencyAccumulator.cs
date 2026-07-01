using System;
using System.Collections.Generic;

namespace Game.Gameplay.Economy
{
    public interface IRunCurrencyAccumulator
    {
        void Grant(CurrencyDefinition currencyDefinition, int amount);
        void Grant(RunRewardSource source, CurrencyDefinition currencyDefinition, int amount);
        void Reset();
        RunCurrencySnapshot CreateSnapshot();
    }

    public sealed class RunCurrencyAccumulator : IRunCurrencyAccumulator, IRunRewardSourceLedger
    {
        private readonly Dictionary<CurrencyDefinition, int> _amountsByCurrency = new();
        private readonly Dictionary<RunRewardSourceCurrencyKey, int> _amountsBySourceCurrency = new();
        private readonly Dictionary<string, RunRewardSource> _sourcesByStableId = new(StringComparer.Ordinal);

        void IRunCurrencyAccumulator.Grant(CurrencyDefinition currencyDefinition, int amount)
        {
            ValidateGrant(currencyDefinition, amount);
            AddCurrencyAmount(currencyDefinition, amount);
        }

        void IRunCurrencyAccumulator.Grant(RunRewardSource source, CurrencyDefinition currencyDefinition, int amount)
        {
            if (string.IsNullOrWhiteSpace(source.StableId))
                throw new ArgumentException("Run currency grant source requires a non-empty Stable Id.", nameof(source));

            ValidateGrant(currencyDefinition, amount);
            AddCurrencyAmount(currencyDefinition, amount);
            TrackSource(source);

            var key = new RunRewardSourceCurrencyKey(source.StableId, currencyDefinition);
            var currentAmount = _amountsBySourceCurrency.GetValueOrDefault(key, 0);
            _amountsBySourceCurrency[key] = checked(currentAmount + amount);
        }

        void IRunCurrencyAccumulator.Reset()
        {
            _amountsByCurrency.Clear();
            _amountsBySourceCurrency.Clear();
            _sourcesByStableId.Clear();
        }

        RunCurrencySnapshot IRunCurrencyAccumulator.CreateSnapshot()
        {
            var amounts = new RunCurrencyAmount[_amountsByCurrency.Count];
            var index = 0;

            foreach (var pair in _amountsByCurrency)
            {
                amounts[index] = new RunCurrencyAmount(pair.Key, pair.Value);
                index += 1;
            }

            return new RunCurrencySnapshot(amounts);
        }

        IReadOnlyList<RunRewardSourceAmount> IRunRewardSourceLedger.CreateRewardSourceAmounts()
        {
            var amounts = new RunRewardSourceAmount[_amountsBySourceCurrency.Count];
            var index = 0;

            foreach (var pair in _amountsBySourceCurrency)
            {
                amounts[index] = new RunRewardSourceAmount(
                    _sourcesByStableId[pair.Key.SourceStableId],
                    pair.Key.CurrencyDefinition,
                    pair.Value);

                index += 1;
            }

            return amounts;
        }

        private void ValidateGrant(CurrencyDefinition currencyDefinition, int amount)
        {
            if (currencyDefinition == null)
                throw new ArgumentNullException(nameof(currencyDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Run currency grant amount must be positive.");
        }

        private void AddCurrencyAmount(CurrencyDefinition currencyDefinition, int amount)
        {
            var currentAmount = _amountsByCurrency.GetValueOrDefault(currencyDefinition, 0);
            _amountsByCurrency[currencyDefinition] = checked(currentAmount + amount);
        }

        private void TrackSource(RunRewardSource source)
        {
            if (!_sourcesByStableId.TryGetValue(source.StableId, out var existingSource))
            {
                _sourcesByStableId.Add(source.StableId, source);
                return;
            }

            if (string.Equals(existingSource.Label, source.Label, StringComparison.Ordinal)
                && existingSource.Order == source.Order
                && existingSource.ShowWhenZero == source.ShowWhenZero)
            {
                return;
            }

            throw new ArgumentException(
                "Run currency grants with the same source Stable Id must use the same label, order, and zero-display policy.",
                nameof(source));
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
}
