using System;
using Game.Gameplay.Economy;
using Game.Gameplay.Pickups;
using Game.Gameplay.Upgrades;

namespace Game.Gameplay
{
    public sealed class CoinPickupCurrencyGrantResolver : IPickupCurrencyGrantResolver
    {
        private readonly IRunGameplayStatResolver _statResolver;
        private readonly CurrencyDefinition _coinCurrencyDefinition;
        private readonly GameplayStatId _coinPickupMultiplierStatId;

        private double _fractionalCarry;

        public CoinPickupCurrencyGrantResolver(
            IRunGameplayStatResolver statResolver,
            CurrencyDefinition coinCurrencyDefinition,
            GameplayStatId coinPickupMultiplierStatId)
        {
            _statResolver = statResolver ?? throw new ArgumentNullException(nameof(statResolver));

            _coinCurrencyDefinition = coinCurrencyDefinition != null
                ? coinCurrencyDefinition
                : throw new ArgumentNullException(nameof(coinCurrencyDefinition));

            _coinPickupMultiplierStatId = coinPickupMultiplierStatId != null
                ? coinPickupMultiplierStatId
                : throw new ArgumentNullException(nameof(coinPickupMultiplierStatId));
        }

        public PickupCurrencyGrantResolution Resolve(CurrencyGrant baseCurrencyGrant)
        {
            if (!ReferenceEquals(baseCurrencyGrant.CurrencyDefinition, _coinCurrencyDefinition))
                return new PickupCurrencyGrantResolution(baseCurrencyGrant, baseCurrencyGrant);

            var multiplier = GetMultiplier();
            var multipliedAmount = (baseCurrencyGrant.Amount * (double)multiplier) + _fractionalCarry;
            var finalAmount = checked((int)Math.Floor(multipliedAmount));

            if (finalAmount <= 0)
            {
                _fractionalCarry = 0d;
                finalAmount = 1;
            }
            else
            {
                _fractionalCarry = multipliedAmount - finalAmount;
            }

            return new PickupCurrencyGrantResolution(
                baseCurrencyGrant,
                new CurrencyGrant(baseCurrencyGrant.CurrencyDefinition, finalAmount));
        }

        public void Reset()
        {
            _fractionalCarry = 0d;
        }

        private float GetMultiplier()
        {
            var multiplier = _statResolver.Resolve(_coinPickupMultiplierStatId, 1f);

            if (float.IsNaN(multiplier) || float.IsInfinity(multiplier))
                return 1f;

            return Math.Max(1f, multiplier);
        }
    }
}
