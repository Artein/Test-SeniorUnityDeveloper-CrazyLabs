using System;
using UnityEngine;

namespace Game.Gameplay
{
    internal abstract class RunSupportColliderProbe : IRunSupportColliderProbe
    {
        private const float MinimumDirectionSqrMagnitude = 0.000001f;
        private const float MinimumOriginPadding = 0.05f;

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

        public virtual float GetProjectedFootprintExtent(Vector3 direction)
        {
            if (!TryNormalizeDirection(direction, out var normalizedDirection))
                return 0f;

            return CalculateProjectedExtent(Collider.bounds, normalizedDirection);
        }

        public bool TryGetSupportSampleOrigin(
            Vector3 upDirection,
            Vector3 lateralOffset,
            float skinWidth,
            out Vector3 origin)
        {
            origin = default;

            if (!TryNormalizeDirection(upDirection, out var normalizedUpDirection))
                return false;

            var bounds = Collider.bounds;
            var projectedExtent = GetProjectedFootprintExtent(normalizedUpDirection);
            var padding = Mathf.Max(MinimumOriginPadding, Mathf.Max(0f, skinWidth) * 2f);
            var rayStart = bounds.center + lateralOffset - (normalizedUpDirection * (projectedExtent + padding));
            var rayDistance = (projectedExtent + padding) * 2f;

            if (!Collider.Raycast(new Ray(rayStart, normalizedUpDirection), out var hit, rayDistance))
                return false;

            origin = hit.point + (normalizedUpDirection * Mathf.Max(0f, skinWidth));
            return true;
        }

        protected bool TryNormalizeDirection(Vector3 direction, out Vector3 normalizedDirection)
        {
            normalizedDirection = default;

            if (direction.sqrMagnitude <= MinimumDirectionSqrMagnitude)
                return false;

            normalizedDirection = direction.normalized;
            return true;
        }

        protected float CalculateProjectedExtent(Bounds bounds, Vector3 normalizedDirection)
        {
            return (bounds.extents.x * Mathf.Abs(normalizedDirection.x))
                   + (bounds.extents.y * Mathf.Abs(normalizedDirection.y))
                   + (bounds.extents.z * Mathf.Abs(normalizedDirection.z));
        }
    }
}
