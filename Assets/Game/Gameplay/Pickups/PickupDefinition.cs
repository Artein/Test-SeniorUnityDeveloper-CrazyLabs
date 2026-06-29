using System;
using Game.Gameplay.Economy;
using UnityEngine;

namespace Game.Gameplay.Pickups
{
    [CreateAssetMenu(
        fileName = nameof(PickupDefinition),
        menuName = "Game/Gameplay/Pickups/Pickup Definition")]
    public sealed partial class PickupDefinition : ScriptableObject
    {
        [SerializeField] private CurrencyDefinition _currencyDefinition;
        [SerializeField] private int _amount = 1;

        public CurrencyDefinition CurrencyDefinition => _currencyDefinition;
        public int Amount => _amount;
        public CurrencyGrant CurrencyGrant => new(_currencyDefinition, _amount);

        public void Validate()
        {
            if (_currencyDefinition == null)
                throw new InvalidOperationException($"Pickup Definition '{name}' requires a Currency Definition reference.");

            if (_amount <= 0)
                throw new InvalidOperationException($"Pickup Definition '{name}' amount must be positive.");
        }
    }
}
