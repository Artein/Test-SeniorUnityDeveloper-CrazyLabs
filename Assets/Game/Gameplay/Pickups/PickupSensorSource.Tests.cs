#if UNITY_INCLUDE_TESTS

using System.Collections.Generic;
using Game.Foundation.Physics;

namespace Game.Gameplay.Pickups
{
    public sealed partial class PickupSensorSource
    {
        internal IReadOnlyList<TriggerNotifier> SensorEntriesForTests => _sensors;

        internal void SetSensorEntriesForTests(params TriggerNotifier[] sensors)
        {
            _sensors = sensors;
            SubscribeSensors();
        }

        internal IEnumerable<string> GetReferenceValidationErrorsForTests(string playerBodyPartLayerName, string pickupLayerName)
        {
            return GetReferenceValidationErrors(playerBodyPartLayerName, pickupLayerName);
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
