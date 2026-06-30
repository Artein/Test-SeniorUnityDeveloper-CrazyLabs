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
        private readonly ILevelPickupSource _pickupSource;
        private readonly List<Pickup> _pickups = new();
        private readonly HashSet<Pickup> _knownPickups = new();
        private readonly HashSet<Pickup> _availablePickups = new();

#if UNITY_INCLUDE_TESTS
        internal IReadOnlyList<Pickup> PickupsForTests
        {
            get
            {
                SynchronizePickupSource();
                return _pickups.ToArray();
            }
        }
#endif

        public LevelPickupState(ILevelPickupSource pickupSource)
        {
            _pickupSource = pickupSource ?? throw new ArgumentNullException(nameof(pickupSource));
            ResetForLevelSession();
        }

        bool ILevelPickupState.IsAvailable(Pickup pickup)
        {
            SynchronizePickupSource();
            return pickup != null && _availablePickups.Contains(pickup);
        }

        public bool TryConsume(Pickup pickup)
        {
            SynchronizePickupSource();

            if (pickup == null)
                return false;

            return _availablePickups.Remove(pickup);
        }

        public void ResetForLevelSession()
        {
            SynchronizePickupSource();
            _availablePickups.Clear();

            foreach (var pickup in _pickups)
            {
                _availablePickups.Add(pickup);
                pickup.SetAvailable(true);
            }
        }

        private void SynchronizePickupSource()
        {
            var sourcePickups = _pickupSource.GetLevelPickups();

            if (sourcePickups == null)
                throw new InvalidOperationException("Level Pickup Source cannot return a null Pickup list.");

            var uniquePickups = new HashSet<Pickup>(sourcePickups.Count);

            for (var pickupIndex = 0; pickupIndex < sourcePickups.Count; pickupIndex += 1)
            {
                var pickup = sourcePickups[pickupIndex];

                if (pickup == null)
                    throw new InvalidOperationException("Level Pickup State cannot contain a null Pickup reference.");

                if (!uniquePickups.Add(pickup))
                    throw new InvalidOperationException($"Level Pickup State contains duplicate Pickup reference '{pickup.name}'.");

                if (!_knownPickups.Add(pickup))
                    continue;

                pickup.Validate();
                _pickups.Add(pickup);
                _availablePickups.Add(pickup);
                pickup.SetAvailable(true);
            }
        }
    }
}
