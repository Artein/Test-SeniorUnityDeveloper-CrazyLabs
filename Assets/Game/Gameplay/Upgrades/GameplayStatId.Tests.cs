#if UNITY_INCLUDE_TESTS

namespace Game.Gameplay.Upgrades
{
    public sealed partial class GameplayStatId
    {
        internal void SetValuesForTests(string id)
        {
            _id = id;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
