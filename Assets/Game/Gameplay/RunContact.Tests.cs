#if UNITY_INCLUDE_TESTS

namespace Game.Gameplay
{
    public sealed partial class RunContact
    {
        internal void SetCategoryForTests(RunContactCategory category)
        {
            _category = category;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
