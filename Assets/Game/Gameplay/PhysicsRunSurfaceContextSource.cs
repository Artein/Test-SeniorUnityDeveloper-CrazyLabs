using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunSurfaceContextSource
    {
        RunSurfaceContext Current { get; }
    }

    public sealed partial class PhysicsRunSurfaceContextSource : MonoBehaviour, IRunSurfaceContextSource
    {
        [SerializeField] private Collider _supportCollider;
        [SerializeField] private RunProgressFrameSource _runProgressFrameSource;
        [SerializeField] private float _supportProbeDistance = 0.08f;
        [SerializeField] private LayerMask _surfaceMask = Physics.DefaultRaycastLayers;

        private readonly RaycastHit[] _supportHits = new RaycastHit[16];
        private readonly IRunSurfaceSlopeCalculator _slopeCalculator = new RunSurfaceSlopeCalculator();

        public RunSurfaceContext Current { get; private set; } = new(false, Vector3.up, 0f);

        private void Reset()
        {
            _supportCollider = GetComponentInChildren<Collider>();
            _runProgressFrameSource = GetComponentInParent<RunProgressFrameSource>();
        }

        private void FixedUpdate()
        {
            Sample();
        }

        private void Sample()
        {
            if (_supportCollider == null || _runProgressFrameSource == null)
            {
                SetUngrounded();
                return;
            }

            if (!_runProgressFrameSource.TryCreateSnapshot(_supportCollider.bounds.center, out var frame, out _))
            {
                SetUngrounded();
                return;
            }

            if (!TryGetClosestSupportHit(-frame.UpDirection, Mathf.Max(0f, _supportProbeDistance), out var supportHit))
            {
                SetUngrounded();
                return;
            }

            var downhillDegrees = _slopeCalculator.CalculateForwardDownhillDegrees(supportHit.normal, frame);
            Current = new RunSurfaceContext(true, supportHit.normal, downhillDegrees);
        }

        private void SetUngrounded()
        {
            Current = new RunSurfaceContext(false, Vector3.up, 0f);
        }

        private bool TryGetClosestSupportHit(Vector3 direction, float distance, out RaycastHit closestHit)
        {
            closestHit = default;

            if (distance <= 0f)
                return false;

            var hitCount = CastSupport(direction, distance);
            var hasHit = false;
            var closestDistance = float.PositiveInfinity;

            for (var hitIndex = 0; hitIndex < hitCount; hitIndex += 1)
            {
                var hit = _supportHits[hitIndex];

                if (!IsValidSupportHit(hit))
                    continue;

                if (hit.distance >= closestDistance)
                    continue;

                closestDistance = hit.distance;
                closestHit = hit;
                hasHit = true;
            }

            return hasHit;
        }

        private int CastSupport(Vector3 direction, float distance)
        {
            if (_supportCollider is CapsuleCollider capsule)
                return CastCapsule(capsule, direction, distance);

            if (_supportCollider is SphereCollider sphere)
                return CastSphere(sphere, direction, distance);

            if (_supportCollider is BoxCollider box)
                return CastBox(box, direction, distance);

            return Physics.RaycastNonAlloc(
                _supportCollider.bounds.center,
                direction,
                _supportHits,
                distance,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private int CastCapsule(CapsuleCollider capsule, Vector3 direction, float distance)
        {
            var capsuleTransform = capsule.transform;
            var axis = capsuleTransform.rotation * GetCapsuleLocalAxis(capsule.direction);
            var scale = Abs(capsuleTransform.lossyScale);
            var radius = Mathf.Max(0f, capsule.radius * GetCapsuleRadiusScale(scale, capsule.direction));
            var height = Mathf.Max(radius * 2f, capsule.height * GetCapsuleHeightScale(scale, capsule.direction));
            var center = capsuleTransform.TransformPoint(capsule.center);
            var halfSegment = Mathf.Max(0f, (height * 0.5f) - radius);

            return Physics.CapsuleCastNonAlloc(
                center + (axis * halfSegment),
                center - (axis * halfSegment),
                radius,
                direction,
                _supportHits,
                distance,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private int CastSphere(SphereCollider sphere, Vector3 direction, float distance)
        {
            var sphereTransform = sphere.transform;
            var scale = Abs(sphereTransform.lossyScale);
            var radius = Mathf.Max(0f, sphere.radius * Mathf.Max(scale.x, scale.y, scale.z));

            return Physics.SphereCastNonAlloc(
                sphereTransform.TransformPoint(sphere.center),
                radius,
                direction,
                _supportHits,
                distance,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private int CastBox(BoxCollider box, Vector3 direction, float distance)
        {
            var boxTransform = box.transform;
            var halfExtents = Vector3.Scale(box.size * 0.5f, Abs(boxTransform.lossyScale));

            return Physics.BoxCastNonAlloc(
                boxTransform.TransformPoint(box.center),
                halfExtents,
                direction,
                _supportHits,
                boxTransform.rotation,
                distance,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private bool IsValidSupportHit(RaycastHit hit)
        {
            var hitCollider = hit.collider;

            if (hitCollider == null || hitCollider == _supportCollider || hitCollider.isTrigger)
                return false;

            if ((_surfaceMask.value & (1 << hitCollider.gameObject.layer)) == 0)
                return false;

            var supportBody = _supportCollider.attachedRigidbody;
            return supportBody == null || hitCollider.attachedRigidbody != supportBody;
        }

        private Vector3 GetCapsuleLocalAxis(int direction)
        {
            if (direction == 0)
                return Vector3.right;

            if (direction == 1)
                return Vector3.up;

            return Vector3.forward;
        }

        private float GetCapsuleRadiusScale(Vector3 scale, int direction)
        {
            if (direction == 0)
                return Mathf.Max(scale.y, scale.z);

            if (direction == 1)
                return Mathf.Max(scale.x, scale.z);

            return Mathf.Max(scale.x, scale.y);
        }

        private float GetCapsuleHeightScale(Vector3 scale, int direction)
        {
            if (direction == 0)
                return scale.x;

            if (direction == 1)
                return scale.y;

            return scale.z;
        }

        private Vector3 Abs(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }
    }
}
