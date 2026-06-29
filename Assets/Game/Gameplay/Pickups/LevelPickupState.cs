using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Gameplay.Pickups
{
    public interface ILevelPickupState
    {
        bool TryConsume(Pickup pickup);
        void ResetForLevelSession();
        bool IsAvailable(Pickup pickup);
    }

    public sealed class LevelPickupState : ILevelPickupState
    {
        private readonly Pickup[] _pickups;
        private readonly HashSet<Pickup> _availablePickups = new();

        public LevelPickupState(IReadOnlyList<Pickup> pickups)
        {
            if (pickups is null)
                throw new ArgumentNullException(nameof(pickups));

            var uniquePickups = new HashSet<Pickup>(pickups.Count);

            foreach (var pickup in pickups)
            {
                if (pickup == null)
                    throw new ArgumentException("Level Pickup State cannot contain a null Pickup reference.", nameof(pickups));

                if (!uniquePickups.Add(pickup))
                    throw new ArgumentException($"Level Pickup State contains duplicate Pickup reference '{pickup.name}'.", nameof(pickups));

                pickup.Validate();
            }

            _pickups = uniquePickups.ToArray();
            ResetForLevelSession();
        }

        bool ILevelPickupState.IsAvailable(Pickup pickup)
        {
            return pickup != null && _availablePickups.Contains(pickup);
        }

        public bool TryConsume(Pickup pickup)
        {
            if (pickup == null)
                return false;

            return _availablePickups.Remove(pickup);
        }

        public void ResetForLevelSession()
        {
            _availablePickups.Clear();

            foreach (var pickup in _pickups)
            {
                _availablePickups.Add(pickup);
                pickup.SetAvailable(true);
            }
        }
    }
}
