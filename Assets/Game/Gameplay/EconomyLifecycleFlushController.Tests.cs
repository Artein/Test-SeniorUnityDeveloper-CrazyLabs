#if UNITY_INCLUDE_TESTS

namespace Game.Gameplay
{
    public sealed partial class EconomyLifecycleFlushController
    {
        internal void NotifyApplicationFocusChangedForTests(bool hasFocus)
        {
            OnApplicationFocusChanged(hasFocus);
        }

        internal void NotifyApplicationQuittingForTests()
        {
            OnApplicationQuitting();
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
