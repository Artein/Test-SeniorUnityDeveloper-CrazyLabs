#if UNITY_INCLUDE_TESTS

using Game.Gameplay.Economy;

namespace Game.Gameplay.Pickups
{
    public sealed partial class PickupDefinition
    {
        internal void SetValuesForTests(CurrencyDefinition currencyDefinition, int amount)
        {
            _currencyDefinition = currencyDefinition;
            _amount = amount;
        }

        internal void SetCurrencyGrantForTests(CurrencyGrant currencyGrant)
        {
            _currencyDefinition = currencyGrant.CurrencyDefinition;
            _amount = currencyGrant.Amount;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
