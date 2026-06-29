using System;
using Game.Gameplay.Economy;

namespace Game.Gameplay.Pickups
{
    public readonly struct PickupCurrencyGrantResolution
    {
        public CurrencyGrant BaseCurrencyGrant { get; }
        public CurrencyGrant FinalCurrencyGrant { get; }
        public CurrencyDefinition CurrencyDefinition => FinalCurrencyGrant.CurrencyDefinition;
        public int BaseAmount => BaseCurrencyGrant.Amount;
        public int FinalAmount => FinalCurrencyGrant.Amount;

        public PickupCurrencyGrantResolution(CurrencyGrant baseCurrencyGrant, CurrencyGrant finalCurrencyGrant)
        {
            if (!ReferenceEquals(baseCurrencyGrant.CurrencyDefinition, finalCurrencyGrant.CurrencyDefinition))
            {
                throw new ArgumentException(
                    "Pickup currency grant resolution requires base and final grants to use the same Currency Definition.",
                    nameof(finalCurrencyGrant));
            }

            BaseCurrencyGrant = baseCurrencyGrant;
            FinalCurrencyGrant = finalCurrencyGrant;
        }
    }
}
