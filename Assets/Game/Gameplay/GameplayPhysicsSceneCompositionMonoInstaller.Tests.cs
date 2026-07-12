#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class GameplayPhysicsSceneCompositionMonoInstaller
    {
        internal Collider SupportColliderForTests => _supportCollider;
        internal float SupportProbeDistanceForTests => _supportProbeDistance;
        internal float SupportProbeSkinWidthForTests => _supportProbeSkinWidth;
        internal LayerMask SurfaceMaskForTests => _surfaceMask;
        internal float MinimumSupportNormalDotForTests => _minimumSupportNormalDot;
        internal float FootprintSampleOffsetScaleForTests => _footprintSampleOffsetScale;
        internal float FootprintNormalClusterAngleDegreesForTests => _footprintNormalClusterAngleDegrees;

        internal void SetReferencesForTests(
            Collider supportCollider,
            float supportProbeDistance,
            LayerMask surfaceMask)
        {
            _supportCollider = supportCollider;
            _supportProbeDistance = supportProbeDistance;
            _surfaceMask = surfaceMask;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
