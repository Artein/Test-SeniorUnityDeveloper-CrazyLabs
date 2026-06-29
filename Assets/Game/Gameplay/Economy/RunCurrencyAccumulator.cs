using System;
using System.Collections.Generic;

namespace Game.Gameplay.Economy
{
    public interface IRunCurrencyAccumulator
    {
        void Grant(CurrencyDefinition currencyDefinition, int amount);
        void Reset();
        RunCurrencySnapshot CreateSnapshot();
    }

    public sealed class RunCurrencyAccumulator : IRunCurrencyAccumulator
    {
        private readonly Dictionary<CurrencyDefinition, int> _amountsByCurrency = new();

        void IRunCurrencyAccumulator.Grant(CurrencyDefinition currencyDefinition, int amount)
        {
            if (currencyDefinition == null)
                throw new ArgumentNullException(nameof(currencyDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Run currency grant amount must be positive.");

            var currentAmount = _amountsByCurrency.GetValueOrDefault(currencyDefinition, 0);
            _amountsByCurrency[currencyDefinition] = checked(currentAmount + amount);
        }

        void IRunCurrencyAccumulator.Reset()
        {
            _amountsByCurrency.Clear();
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
    }
}
