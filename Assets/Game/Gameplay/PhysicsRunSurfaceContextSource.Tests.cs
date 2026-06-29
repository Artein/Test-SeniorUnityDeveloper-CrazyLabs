#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class PhysicsRunSurfaceContextSource
    {
        internal Collider SupportColliderForTests => _supportCollider;
        internal float SupportProbeDistanceForTests => _supportProbeDistance;

        internal void SetReferencesForTests(
            Collider supportCollider,
            RunProgressFrameSource runProgressFrameSource,
            float supportProbeDistance,
            LayerMask surfaceMask)
        {
            _supportCollider = supportCollider;
            _runProgressFrameSource = runProgressFrameSource;
            _supportProbeDistance = supportProbeDistance;
            _surfaceMask = surfaceMask;
        }

        internal void SampleForTests()
        {
            Sample();
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
