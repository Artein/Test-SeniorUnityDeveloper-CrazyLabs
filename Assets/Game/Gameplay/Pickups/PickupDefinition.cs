using System;
using UnityEngine;

namespace Game.Gameplay.Pickups
{
    [CreateAssetMenu(
        fileName = nameof(PickupDefinition),
        menuName = "Game/Gameplay/Pickups/Pickup Definition")]
    public sealed partial class PickupDefinition : ScriptableObject
    {
        [SerializeField] private ResourceDefinition _resourceDefinition;
        [SerializeField] private int _amount = 1;

        public ResourceDefinition ResourceDefinition => _resourceDefinition;
        public int Amount => _amount;

        public void Validate()
        {
            if (_resourceDefinition == null)
                throw new InvalidOperationException($"Pickup Definition '{name}' requires a Resource Definition reference.");

            if (_amount <= 0)
                throw new InvalidOperationException($"Pickup Definition '{name}' amount must be positive.");
        }
    }
}
