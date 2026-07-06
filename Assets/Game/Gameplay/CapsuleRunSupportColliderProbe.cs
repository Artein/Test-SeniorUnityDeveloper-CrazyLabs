using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    internal sealed class CapsuleRunSupportColliderProbe : RunSupportColliderProbe
    {
        private readonly CapsuleCollider _capsule;

        public CapsuleRunSupportColliderProbe(CapsuleCollider capsule)
            : base(capsule)
        {
            _capsule = capsule;
        }

        public override int Cast(
            Vector3 castOffset,
            Vector3 direction,
            float distance,
            RaycastHit[] results,
            LayerMask surfaceMask)
        {
            var capsuleTransform = _capsule.transform;
            var axis = capsuleTransform.rotation * GetCapsuleLocalAxis(_capsule.direction);
            var scale = capsuleTransform.lossyScale.Abs();
            var radius = Mathf.Max(0f, _capsule.radius * GetCapsuleRadiusScale(scale, _capsule.direction));
            var height = Mathf.Max(radius * 2f, _capsule.height * GetCapsuleHeightScale(scale, _capsule.direction));
            var center = capsuleTransform.TransformPoint(_capsule.center);
            var halfSegment = Mathf.Max(0f, (height * 0.5f) - radius);

            return Physics.CapsuleCastNonAlloc(
                center + (axis * halfSegment) + castOffset,
                center - (axis * halfSegment) + castOffset,
                radius,
                direction,
                results,
                distance,
                surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        public override int Overlap(Collider[] results, LayerMask surfaceMask)
        {
            var capsuleTransform = _capsule.transform;
            var axis = capsuleTransform.rotation * GetCapsuleLocalAxis(_capsule.direction);
            var scale = capsuleTransform.lossyScale.Abs();
            var radius = Mathf.Max(0f, _capsule.radius * GetCapsuleRadiusScale(scale, _capsule.direction));
            var height = Mathf.Max(radius * 2f, _capsule.height * GetCapsuleHeightScale(scale, _capsule.direction));
            var center = capsuleTransform.TransformPoint(_capsule.center);
            var halfSegment = Mathf.Max(0f, (height * 0.5f) - radius);

            return Physics.OverlapCapsuleNonAlloc(
                center + (axis * halfSegment),
                center - (axis * halfSegment),
                radius,
                results,
                surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private static Vector3 GetCapsuleLocalAxis(int direction)
        {
            return direction switch
            {
                0 => Vector3.right,
                1 => Vector3.up,
                _ => Vector3.forward
            };
        }

        private static float GetCapsuleRadiusScale(Vector3 scale, int direction)
        {
            return direction switch
            {
                0 => Mathf.Max(scale.y, scale.z),
                1 => Mathf.Max(scale.x, scale.z),
                _ => Mathf.Max(scale.x, scale.y)
            };
        }

        private static float GetCapsuleHeightScale(Vector3 scale, int direction)
        {
            return direction switch
            {
                0 => scale.x,
                1 => scale.y,
                _ => scale.z
            };
        }
    }
}
