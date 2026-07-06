using System;
using UnityEngine;

namespace Game.Gameplay
{
    internal abstract class RunSupportColliderProbe : IRunSupportColliderProbe
    {
        public Collider Collider { get; }

        protected RunSupportColliderProbe(Collider collider)
        {
            Collider = collider != null ? collider : throw new ArgumentNullException(nameof(collider));
        }

        public abstract int Cast(
            Vector3 castOffset,
            Vector3 direction,
            float distance,
            RaycastHit[] results,
            LayerMask surfaceMask);

        public abstract int Overlap(
            Collider[] results, 
            LayerMask surfaceMask);

        public Vector3 GetSupportProbeOrigin(Vector3 upDirection, float skinWidth)
        {
            var bounds = Collider.bounds;

            var projectedExtent =
                bounds.extents.x * Mathf.Abs(upDirection.x)
                + bounds.extents.y * Mathf.Abs(upDirection.y)
                + bounds.extents.z * Mathf.Abs(upDirection.z);

            return bounds.center - (upDirection * projectedExtent) + (upDirection * skinWidth);
        }
    }
}
