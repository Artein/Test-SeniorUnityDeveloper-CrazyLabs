#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public sealed partial class RigidbodyLaunchTarget
    {
        internal void SetRigidbodyForTests(Rigidbody body)
        {
            _rigidbody = body;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
