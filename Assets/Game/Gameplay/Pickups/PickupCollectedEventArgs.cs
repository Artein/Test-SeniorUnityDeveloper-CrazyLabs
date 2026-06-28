using System;
using UnityEngine;

namespace Game.Gameplay.Pickups
{
    public readonly struct PickupCollectedEventArgs
    {
        public ResourceDefinition ResourceDefinition { get; }
        public int Amount { get; }
        public Vector3 Position { get; }

        public PickupCollectedEventArgs(ResourceDefinition resourceDefinition, int amount, Vector3 position)
        {
            ResourceDefinition = resourceDefinition != null
                ? resourceDefinition
                : throw new ArgumentNullException(nameof(resourceDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Pickup collection amount must be positive.");

            Amount = amount;
            Position = position;
        }
    }
}
