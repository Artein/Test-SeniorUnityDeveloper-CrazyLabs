using System;
using System.Collections.Generic;

namespace Game.Gameplay.Economy
{
    public interface ICurrencyStorage
    {
        void Grant(CurrencyDefinition currencyDefinition, int amount);
        bool TrySpend(CurrencyDefinition currencyDefinition, int amount);
        int GetAmount(CurrencyDefinition currencyDefinition);
    }

    public sealed class CurrencyStorage : ICurrencyStorage
    {
        private readonly Dictionary<CurrencyDefinition, int> _amountsByCurrency = new();

        void ICurrencyStorage.Grant(CurrencyDefinition currencyDefinition, int amount)
        {
            if (currencyDefinition == null)
                throw new ArgumentNullException(nameof(currencyDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Currency grant amount must be positive.");

            var currentAmount = ((ICurrencyStorage)this).GetAmount(currencyDefinition);
            _amountsByCurrency[currencyDefinition] = checked(currentAmount + amount);
        }

        bool ICurrencyStorage.TrySpend(CurrencyDefinition currencyDefinition, int amount)
        {
            if (currencyDefinition == null)
                throw new ArgumentNullException(nameof(currencyDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Currency spend amount must be positive.");

            var currentAmount = ((ICurrencyStorage)this).GetAmount(currencyDefinition);

            if (currentAmount < amount)
                return false;

            _amountsByCurrency[currencyDefinition] = currentAmount - amount;
            return true;
        }

        int ICurrencyStorage.GetAmount(CurrencyDefinition currencyDefinition)
        {
            return currencyDefinition == null ? 0 : _amountsByCurrency.GetValueOrDefault(currencyDefinition, 0);
        }
    }
}
