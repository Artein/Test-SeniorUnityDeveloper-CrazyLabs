using UnityEngine;

namespace Game.Gameplay
{
    internal sealed class BoundsRunSupportColliderProbe : RunSupportColliderProbe
    {
        public BoundsRunSupportColliderProbe(Collider collider)
            : base(collider)
        {
        }

        public override int Cast(
            Vector3 castOffset,
            Vector3 direction,
            float distance,
            RaycastHit[] results,
            LayerMask surfaceMask)
        {
            return Physics.RaycastNonAlloc(
                Collider.bounds.center + castOffset,
                direction,
                results,
                distance,
                surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        public override int Overlap(Collider[] results, LayerMask surfaceMask)
        {
            return Physics.OverlapBoxNonAlloc(
                Collider.bounds.center,
                Collider.bounds.extents,
                results,
                Quaternion.identity,
                surfaceMask,
                QueryTriggerInteraction.Ignore);
        }
    }
}
