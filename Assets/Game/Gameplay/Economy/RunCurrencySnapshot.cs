using System;
using System.Collections.Generic;

namespace Game.Gameplay.Economy
{
    public readonly struct RunCurrencyAmount
    {
        public CurrencyDefinition CurrencyDefinition { get; }
        public int Amount { get; }

        public RunCurrencyAmount(CurrencyDefinition currencyDefinition, int amount)
        {
            CurrencyDefinition = currencyDefinition != null
                ? currencyDefinition
                : throw new ArgumentNullException(nameof(currencyDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Run currency snapshot amount must be positive.");

            Amount = amount;
        }
    }

    public sealed class RunCurrencySnapshot
    {
        private readonly Dictionary<CurrencyDefinition, int> _amountsByCurrency;
        private readonly RunCurrencyAmount[] _amounts;

        public IReadOnlyList<RunCurrencyAmount> Amounts => _amounts;

        public RunCurrencySnapshot(IEnumerable<RunCurrencyAmount> amounts)
        {
            if (amounts is null)
                throw new ArgumentNullException(nameof(amounts));

            _amountsByCurrency = new Dictionary<CurrencyDefinition, int>();

            foreach (var amount in amounts)
            {
                if (_amountsByCurrency.ContainsKey(amount.CurrencyDefinition))
                    throw new ArgumentException("Run currency snapshot cannot contain duplicate Currency Definition entries.", nameof(amounts));

                _amountsByCurrency.Add(amount.CurrencyDefinition, amount.Amount);
            }

            _amounts = new RunCurrencyAmount[_amountsByCurrency.Count];
            var index = 0;

            foreach (var pair in _amountsByCurrency)
            {
                _amounts[index] = new RunCurrencyAmount(pair.Key, pair.Value);
                index += 1;
            }
        }

        public int GetAmount(CurrencyDefinition currencyDefinition)
        {
            return currencyDefinition == null ? 0 : _amountsByCurrency.GetValueOrDefault(currencyDefinition, 0);
        }
    }
}
