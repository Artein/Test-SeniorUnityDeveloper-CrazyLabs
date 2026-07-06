#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public sealed partial class RigidbodyLaunchTarget
    {
        internal bool HasPreviousStateForTests => _hasPreviousState;
        internal RigidbodyInterpolation PreviousInterpolationForTests => _previousInterpolation;
        internal Collider BandContactColliderForTests => _bandContactCollider;

        internal void SetReferencesForTests(Rigidbody body, Collider bandContactCollider, Transform bandCenter)
        {
            _rigidbody = body;
            _bandContactCollider = bandContactCollider;
            _bandCenter = bandCenter;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
