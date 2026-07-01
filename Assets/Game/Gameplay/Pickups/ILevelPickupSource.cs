using System;
using System.Collections.Generic;

namespace Game.Gameplay.Pickups
{
    public interface ILevelPickupSource
    {
        IReadOnlyList<Pickup> GetLevelPickups();
    }

    internal sealed class FixedLevelPickupSource : ILevelPickupSource
    {
        private readonly IReadOnlyList<Pickup> _pickups;

        public FixedLevelPickupSource(IReadOnlyList<Pickup> pickups)
        {
            ValidateFixedPickups(pickups);
            _pickups = pickups ?? throw new ArgumentNullException(nameof(pickups));
        }

        public IReadOnlyList<Pickup> GetLevelPickups()
        {
            return _pickups;
        }

        private static IReadOnlyList<Pickup> ValidateFixedPickups(IReadOnlyList<Pickup> pickups)
        {
            if (pickups == null)
                throw new ArgumentNullException(nameof(pickups));

            var uniquePickups = new HashSet<Pickup>(pickups.Count);

            for (var pickupIndex = 0; pickupIndex < pickups.Count; pickupIndex += 1)
            {
                var pickup = pickups[pickupIndex];

                if (pickup == null)
                    throw new ArgumentException(
                        "Level Pickup State cannot contain a null Pickup reference.",
                        nameof(pickups));

                if (!uniquePickups.Add(pickup))
                    throw new ArgumentException(
                        $"Level Pickup State contains duplicate Pickup reference '{pickup.name}'.",
                        nameof(pickups));
            }

            return pickups;
        }
    }
}
