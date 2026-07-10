#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class RigidbodyRunBodyMovementTarget
    {
        internal void SetRigidbodyForTests(Rigidbody body)
        {
            _rigidbody = body;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
