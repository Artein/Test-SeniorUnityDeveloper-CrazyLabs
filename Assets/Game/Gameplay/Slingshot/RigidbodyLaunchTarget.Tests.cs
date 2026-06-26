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

        internal void SetReferencesForTests(Rigidbody body, Collider bandContactCollider)
        {
            _rigidbody = body;
            _bandContactCollider = bandContactCollider;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
