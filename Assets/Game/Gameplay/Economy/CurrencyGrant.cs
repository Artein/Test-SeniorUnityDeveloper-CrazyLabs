using System;

namespace Game.Gameplay.Economy
{
    public readonly struct CurrencyGrant
    {
        public CurrencyDefinition CurrencyDefinition { get; }
        public int Amount { get; }

        public CurrencyGrant(CurrencyDefinition currencyDefinition, int amount)
        {
            CurrencyDefinition = currencyDefinition != null
                ? currencyDefinition
                : throw new ArgumentNullException(nameof(currencyDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Currency grant amount must be positive.");

            Amount = amount;
        }
    }
}
