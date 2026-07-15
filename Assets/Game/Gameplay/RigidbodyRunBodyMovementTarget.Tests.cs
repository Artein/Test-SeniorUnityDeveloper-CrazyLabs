#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class RigidbodyRunBodyMovementTarget
    {
        internal int SuccessfulTargetWriteCountForTests { get; private set; }

        internal void SetRigidbodyForTests(Rigidbody body)
        {
            _rigidbody = body;
        }

        partial void RecordSuccessfulTargetWriteForTests()
        {
            SuccessfulTargetWriteCountForTests += 1;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
