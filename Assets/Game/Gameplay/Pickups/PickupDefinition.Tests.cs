#if UNITY_INCLUDE_TESTS

namespace Game.Gameplay.Pickups
{
    public sealed partial class PickupDefinition
    {
        internal void SetValuesForTests(ResourceDefinition resourceDefinition, int amount)
        {
            _resourceDefinition = resourceDefinition;
            _amount = amount;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
