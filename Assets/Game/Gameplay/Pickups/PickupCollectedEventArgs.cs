using Game.Gameplay.Economy;
using UnityEngine;

namespace Game.Gameplay.Pickups
{
    public readonly struct PickupCollectedEventArgs
    {
        public CurrencyGrant CurrencyGrant => FinalCurrencyGrant;
        public CurrencyDefinition CurrencyDefinition => FinalCurrencyGrant.CurrencyDefinition;
        public int Amount => FinalAmount;
        public CurrencyGrant BaseCurrencyGrant { get; }
        public CurrencyGrant FinalCurrencyGrant { get; }
        public int BaseAmount => BaseCurrencyGrant.Amount;
        public int FinalAmount => FinalCurrencyGrant.Amount;
        public Vector3 Position { get; }

        public PickupCollectedEventArgs(CurrencyGrant baseCurrencyGrant, CurrencyGrant finalCurrencyGrant, Vector3 position)
        {
            var resolution = new PickupCurrencyGrantResolution(baseCurrencyGrant, finalCurrencyGrant);
            BaseCurrencyGrant = resolution.BaseCurrencyGrant;
            FinalCurrencyGrant = resolution.FinalCurrencyGrant;
            Position = position;
        }

        public PickupCollectedEventArgs(CurrencyDefinition currencyDefinition, int amount, Vector3 position)
            : this(new CurrencyGrant(currencyDefinition, amount), position)
        {
        }

        private PickupCollectedEventArgs(CurrencyGrant currencyGrant, Vector3 position)
            : this(currencyGrant, currencyGrant, position)
        {
        }
    }
}
