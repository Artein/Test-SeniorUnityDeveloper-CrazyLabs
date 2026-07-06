#if UNITY_INCLUDE_TESTS

using System.Collections.Generic;
using Game.Gameplay.Pickups;

namespace Game.Gameplay
{
    public sealed partial class GameplayPickupsSceneCompositionMonoInstaller
    {
        internal IReadOnlyList<Pickup> LevelPickupsForTests => LevelPickups;

        internal void SetReferencesForTests(
            Pickup[] levelPickups,
            PickupSensorSource pickupSensorSource,
            string pickupLayerName,
            string playerBodyPartLayerName)
        {
            _levelPickups = levelPickups;
            _pickupSensorSource = pickupSensorSource;
            _pickupLayerName = pickupLayerName;
            _playerBodyPartLayerName = playerBodyPartLayerName;
        }

        internal void SetLayerCollisionIgnoredForTests(bool ignored)
        {
            _layerCollisionMatrix = new FixedLayerCollisionMatrix(ignored);
        }

        internal IEnumerable<string> GetReferenceValidationErrorsForTests()
        {
            return GetReferenceValidationErrors();
        }

        private sealed class FixedLayerCollisionMatrix : ILayerCollisionMatrix
        {
            private readonly bool _ignored;

            public FixedLayerCollisionMatrix(bool ignored)
            {
                _ignored = ignored;
            }

            public bool GetIgnoreLayerCollision(int firstLayer, int secondLayer)
            {
                return _ignored;
            }
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
