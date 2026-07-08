using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    internal sealed class SphereRunSupportColliderProbe : RunSupportColliderProbe
    {
        private readonly SphereCollider _sphere;

        public SphereRunSupportColliderProbe(SphereCollider sphere)
            : base(sphere)
        {
            _sphere = sphere;
        }

        public override int Cast(
            Vector3 castOffset,
            Vector3 direction,
            float distance,
            RaycastHit[] results,
            LayerMask surfaceMask)
        {
            var sphereTransform = _sphere.transform;

            return Physics.SphereCastNonAlloc(
                sphereTransform.TransformPoint(_sphere.center) + castOffset,
                ResolveRadius(),
                direction,
                results,
                distance,
                surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        public override int Overlap(Collider[] results, LayerMask surfaceMask)
        {
            var sphereTransform = _sphere.transform;

            return Physics.OverlapSphereNonAlloc(
                sphereTransform.TransformPoint(_sphere.center),
                ResolveRadius(),
                results,
                surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        public override float GetProjectedFootprintExtent(Vector3 direction)
        {
            if (!TryNormalizeDirection(direction, out _))
                return 0f;

            return ResolveRadius();
        }

        private float ResolveRadius()
        {
            var sphereTransform = _sphere.transform;
            var scale = sphereTransform.lossyScale.Abs();

            return Mathf.Max(0f, _sphere.radius * Mathf.Max(scale.x, scale.y, scale.z));
        }
    }
}
