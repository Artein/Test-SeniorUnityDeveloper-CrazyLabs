#if UNITY_INCLUDE_TESTS

using UnityEngine;
using Game.Foundation.Physics;

namespace Game.Gameplay.Pickups
{
    public sealed partial class Pickup
    {
        internal TriggerNotifier TriggerNotifierForTests => _triggerNotifier;

        internal void SetDefinitionForTests(PickupDefinition definition)
        {
            _definition = definition;
        }

        internal void SetTriggerNotifierForTests(TriggerNotifier triggerNotifier)
        {
            _triggerNotifier = triggerNotifier;
        }

        internal void RaiseTriggerEnteredForTests(Collider other)
        {
            HandleTriggerEntered(other);
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
