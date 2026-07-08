using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    internal sealed class BoxRunSupportColliderProbe : RunSupportColliderProbe
    {
        private readonly BoxCollider _box;

        public BoxRunSupportColliderProbe(BoxCollider box)
            : base(box)
        {
            _box = box;
        }

        public override int Cast(
            Vector3 castOffset,
            Vector3 direction,
            float distance,
            RaycastHit[] results,
            LayerMask surfaceMask)
        {
            var boxTransform = _box.transform;

            return Physics.BoxCastNonAlloc(
                boxTransform.TransformPoint(_box.center) + castOffset,
                ResolveHalfExtents(),
                direction,
                results,
                boxTransform.rotation,
                distance,
                surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        public override int Overlap(Collider[] results, LayerMask surfaceMask)
        {
            var boxTransform = _box.transform;

            return Physics.OverlapBoxNonAlloc(
                boxTransform.TransformPoint(_box.center),
                ResolveHalfExtents(),
                results,
                boxTransform.rotation,
                surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        public override float GetProjectedFootprintExtent(Vector3 direction)
        {
            if (!TryNormalizeDirection(direction, out var normalizedDirection))
                return 0f;

            var boxTransform = _box.transform;
            var halfExtents = ResolveHalfExtents();

            return (halfExtents.x * Mathf.Abs(Vector3.Dot(boxTransform.right, normalizedDirection)))
                   + (halfExtents.y * Mathf.Abs(Vector3.Dot(boxTransform.up, normalizedDirection)))
                   + (halfExtents.z * Mathf.Abs(Vector3.Dot(boxTransform.forward, normalizedDirection)));
        }

        private Vector3 ResolveHalfExtents()
        {
            return Vector3.Scale(_box.size * 0.5f, _box.transform.lossyScale.Abs());
        }
    }
}
