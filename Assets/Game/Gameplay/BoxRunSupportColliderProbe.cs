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
            var halfExtents = Vector3.Scale(_box.size * 0.5f, boxTransform.lossyScale.Abs());

            return Physics.BoxCastNonAlloc(
                boxTransform.TransformPoint(_box.center) + castOffset,
                halfExtents,
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
            var halfExtents = Vector3.Scale(_box.size * 0.5f, boxTransform.lossyScale.Abs());

            return Physics.OverlapBoxNonAlloc(
                boxTransform.TransformPoint(_box.center),
                halfExtents,
                results,
                boxTransform.rotation,
                surfaceMask,
                QueryTriggerInteraction.Ignore);
        }
    }
}
