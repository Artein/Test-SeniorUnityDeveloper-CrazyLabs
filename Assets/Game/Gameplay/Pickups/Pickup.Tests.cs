#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay.Pickups
{
    public sealed partial class Pickup
    {
        internal void SetDefinitionForTests(PickupDefinition definition)
        {
            _definition = definition;
        }

        internal void RaiseTriggerEnteredForTests(Collider other)
        {
            OnTriggerEnter(other);
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
