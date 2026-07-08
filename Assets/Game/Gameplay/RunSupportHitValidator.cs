using UnityEngine;

namespace Game.Gameplay
{
    internal sealed class RunSupportHitValidator
    {
        private const float MinimumSqrMagnitude = 0.000001f;

        private readonly IRunSupportColliderProbe _supportProbe;
        private readonly LayerMask _surfaceMask;
        private readonly float _minimumSupportNormalDot;

        public RunSupportHitValidator(
            IRunSupportColliderProbe supportProbe,
            LayerMask surfaceMask,
            float minimumSupportNormalDot)
        {
            _supportProbe = supportProbe;
            _surfaceMask = surfaceMask;
            _minimumSupportNormalDot = minimumSupportNormalDot;
        }

        public bool IsValidSupportHit(RaycastHit hit)
        {
            return IsValidSupportCollider(hit.collider);
        }

        public bool IsValidSupportCollider(Collider hitCollider)
        {
            var supportCollider = _supportProbe.Collider;

            if (hitCollider == null || hitCollider == supportCollider || hitCollider.isTrigger)
                return false;

            if ((_surfaceMask.value & (1 << hitCollider.gameObject.layer)) == 0)
                return false;

            var supportBody = supportCollider.attachedRigidbody;

            if (supportBody != null && hitCollider.attachedRigidbody == supportBody)
                return false;

            return hitCollider.TryGetComponent(out RunContact runContact)
                   && runContact.Category == RunContactCategory.Surface;
        }

        public bool IsValidSupportNormal(Vector3 normal, Vector3 upDirection)
        {
            var sqrMagnitude = normal.sqrMagnitude;

            if (sqrMagnitude <= MinimumSqrMagnitude || float.IsNaN(sqrMagnitude) || float.IsInfinity(sqrMagnitude))
                return false;

            return Vector3.Dot(normal.normalized, upDirection) > _minimumSupportNormalDot;
        }
    }
}
