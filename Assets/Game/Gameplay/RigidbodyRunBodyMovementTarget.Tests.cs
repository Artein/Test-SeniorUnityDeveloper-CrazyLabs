#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class RigidbodyRunBodyMovementTarget
    {
        private int _successfulTargetWriteCountForTests;

        internal int SuccessfulTargetWriteCountForTests => _successfulTargetWriteCountForTests;

        internal void SetRigidbodyForTests(Rigidbody body)
        {
            _rigidbody = body;
        }

        partial void RecordSuccessfulTargetWriteForTests()
        {
            _successfulTargetWriteCountForTests += 1;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
