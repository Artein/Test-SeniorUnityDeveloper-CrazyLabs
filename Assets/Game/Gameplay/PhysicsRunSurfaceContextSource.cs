using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunSurfaceContextSource
    {
        RunSurfaceContext Current { get; }
    }

    public sealed partial class PhysicsRunSurfaceContextSource : MonoBehaviour, IRunSurfaceContextSource
    {
        private const float SupportProbeSkinWidth = 0.02f;
        private const float MinimumSupportNormalDot = 0.0001f;
        private const int UngroundedMissThreshold = 2;

        [SerializeField] private Collider _supportCollider;
        [SerializeField] private RunProgressFrameSource _runProgressFrameSource;
        [SerializeField] private float _supportProbeDistance = 0.08f;
        [SerializeField] private LayerMask _surfaceMask = Physics.DefaultRaycastLayers;

        private readonly RaycastHit[] _supportHits = new RaycastHit[16];
        private readonly Collider[] _overlapHits = new Collider[16];
        private readonly IRunSurfaceSlopeCalculator _slopeCalculator = new RunSurfaceSlopeCalculator();

        private int _consecutiveMissedSupportSamples = UngroundedMissThreshold;

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

            if (!TryGetClosestSupportHit(frame.UpDirection, Mathf.Max(0f, _supportProbeDistance), out var supportHit))
            {
                RecordMissedSupportSample();
                return;
            }

            _consecutiveMissedSupportSamples = 0;

            var downhillDegrees = _slopeCalculator.CalculateForwardDownhillDegrees(supportHit.Normal, frame);
            Current = new RunSurfaceContext(true, supportHit.Normal, downhillDegrees);
        }

        private void SetUngrounded()
        {
            _consecutiveMissedSupportSamples = UngroundedMissThreshold;
            Current = new RunSurfaceContext(false, Vector3.up, 0f);
        }

        private void RecordMissedSupportSample()
        {
            _consecutiveMissedSupportSamples += 1;

            if (Current.IsGrounded && _consecutiveMissedSupportSamples < UngroundedMissThreshold)
                return;

            Current = new RunSurfaceContext(false, Vector3.up, 0f);
        }

        private bool TryGetClosestSupportHit(Vector3 upDirection, float supportProbeDistance, out SupportHit closestHit)
        {
            closestHit = default;

            if (TryGetOverlapSupport(upDirection, out closestHit))
                return true;

            if (supportProbeDistance <= 0f)
                return false;

            var direction = -upDirection;
            var castOffset = upDirection * SupportProbeSkinWidth;
            var distance = supportProbeDistance + SupportProbeSkinWidth;
            var hitCount = CastSupport(castOffset, direction, distance);
            var hasHit = false;
            var closestDistance = float.PositiveInfinity;

            for (var hitIndex = 0; hitIndex < hitCount; hitIndex += 1)
            {
                var hit = _supportHits[hitIndex];

                if (!IsValidSupportHit(hit))
                    continue;

                if (!IsValidSupportNormal(hit.normal, upDirection))
                    continue;

                if (hit.distance >= closestDistance)
                    continue;

                closestDistance = hit.distance;
                closestHit = new SupportHit(hit.normal.normalized);
                hasHit = true;
            }

            return hasHit;
        }

        private int CastSupport(Vector3 castOffset, Vector3 direction, float distance)
        {
            if (_supportCollider is CapsuleCollider capsule)
                return CastCapsule(capsule, castOffset, direction, distance);

            if (_supportCollider is SphereCollider sphere)
                return CastSphere(sphere, castOffset, direction, distance);

            if (_supportCollider is BoxCollider box)
                return CastBox(box, castOffset, direction, distance);

            return Physics.RaycastNonAlloc(
                _supportCollider.bounds.center + castOffset,
                direction,
                _supportHits,
                distance,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private int CastCapsule(CapsuleCollider capsule, Vector3 castOffset, Vector3 direction, float distance)
        {
            var capsuleTransform = capsule.transform;
            var axis = capsuleTransform.rotation * GetCapsuleLocalAxis(capsule.direction);
            var scale = Abs(capsuleTransform.lossyScale);
            var radius = Mathf.Max(0f, capsule.radius * GetCapsuleRadiusScale(scale, capsule.direction));
            var height = Mathf.Max(radius * 2f, capsule.height * GetCapsuleHeightScale(scale, capsule.direction));
            var center = capsuleTransform.TransformPoint(capsule.center);
            var halfSegment = Mathf.Max(0f, (height * 0.5f) - radius);

            return Physics.CapsuleCastNonAlloc(
                center + (axis * halfSegment) + castOffset,
                center - (axis * halfSegment) + castOffset,
                radius,
                direction,
                _supportHits,
                distance,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private int CastSphere(SphereCollider sphere, Vector3 castOffset, Vector3 direction, float distance)
        {
            var sphereTransform = sphere.transform;
            var scale = Abs(sphereTransform.lossyScale);
            var radius = Mathf.Max(0f, sphere.radius * Mathf.Max(scale.x, scale.y, scale.z));

            return Physics.SphereCastNonAlloc(
                sphereTransform.TransformPoint(sphere.center) + castOffset,
                radius,
                direction,
                _supportHits,
                distance,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private int CastBox(BoxCollider box, Vector3 castOffset, Vector3 direction, float distance)
        {
            var boxTransform = box.transform;
            var halfExtents = Vector3.Scale(box.size * 0.5f, Abs(boxTransform.lossyScale));

            return Physics.BoxCastNonAlloc(
                boxTransform.TransformPoint(box.center) + castOffset,
                halfExtents,
                direction,
                _supportHits,
                boxTransform.rotation,
                distance,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private bool TryGetOverlapSupport(Vector3 upDirection, out SupportHit supportHit)
        {
            supportHit = default;

            var overlapCount = OverlapSupport();
            var hasHit = false;
            var bestNormalDot = float.NegativeInfinity;
            var bestDistance = float.NegativeInfinity;

            for (var overlapIndex = 0; overlapIndex < overlapCount; overlapIndex += 1)
            {
                var hitCollider = _overlapHits[overlapIndex];

                if (!IsValidSupportCollider(hitCollider))
                    continue;

                if (!Physics.ComputePenetration(
                        _supportCollider,
                        _supportCollider.transform.position,
                        _supportCollider.transform.rotation,
                        hitCollider,
                        hitCollider.transform.position,
                        hitCollider.transform.rotation,
                        out var separationDirection,
                        out var separationDistance))
                {
                    continue;
                }

                if (!IsValidSupportNormal(separationDirection, upDirection))
                    continue;

                var normal = separationDirection.normalized;
                var normalDot = Vector3.Dot(normal, upDirection);

                if (normalDot < bestNormalDot
                    || (Mathf.Approximately(normalDot, bestNormalDot) && separationDistance <= bestDistance))
                {
                    continue;
                }

                bestNormalDot = normalDot;
                bestDistance = separationDistance;
                supportHit = new SupportHit(normal);
                hasHit = true;
            }

            return hasHit;
        }

        private int OverlapSupport()
        {
            if (_supportCollider is CapsuleCollider capsule)
                return OverlapCapsule(capsule);

            if (_supportCollider is SphereCollider sphere)
                return OverlapSphere(sphere);

            if (_supportCollider is BoxCollider box)
                return OverlapBox(box);

            return Physics.OverlapBoxNonAlloc(
                _supportCollider.bounds.center,
                _supportCollider.bounds.extents,
                _overlapHits,
                Quaternion.identity,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private int OverlapCapsule(CapsuleCollider capsule)
        {
            var capsuleTransform = capsule.transform;
            var axis = capsuleTransform.rotation * GetCapsuleLocalAxis(capsule.direction);
            var scale = Abs(capsuleTransform.lossyScale);
            var radius = Mathf.Max(0f, capsule.radius * GetCapsuleRadiusScale(scale, capsule.direction));
            var height = Mathf.Max(radius * 2f, capsule.height * GetCapsuleHeightScale(scale, capsule.direction));
            var center = capsuleTransform.TransformPoint(capsule.center);
            var halfSegment = Mathf.Max(0f, (height * 0.5f) - radius);

            return Physics.OverlapCapsuleNonAlloc(
                center + (axis * halfSegment),
                center - (axis * halfSegment),
                radius,
                _overlapHits,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private int OverlapSphere(SphereCollider sphere)
        {
            var sphereTransform = sphere.transform;
            var scale = Abs(sphereTransform.lossyScale);
            var radius = Mathf.Max(0f, sphere.radius * Mathf.Max(scale.x, scale.y, scale.z));

            return Physics.OverlapSphereNonAlloc(
                sphereTransform.TransformPoint(sphere.center),
                radius,
                _overlapHits,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private int OverlapBox(BoxCollider box)
        {
            var boxTransform = box.transform;
            var halfExtents = Vector3.Scale(box.size * 0.5f, Abs(boxTransform.lossyScale));

            return Physics.OverlapBoxNonAlloc(
                boxTransform.TransformPoint(box.center),
                halfExtents,
                _overlapHits,
                boxTransform.rotation,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
        }

        private bool IsValidSupportHit(RaycastHit hit)
        {
            return IsValidSupportCollider(hit.collider);
        }

        private bool IsValidSupportCollider(Collider hitCollider)
        {
            if (hitCollider == null || hitCollider == _supportCollider || hitCollider.isTrigger)
                return false;

            if ((_surfaceMask.value & (1 << hitCollider.gameObject.layer)) == 0)
                return false;

            var supportBody = _supportCollider.attachedRigidbody;
            return supportBody == null || hitCollider.attachedRigidbody != supportBody;
        }

        private bool IsValidSupportNormal(Vector3 normal, Vector3 upDirection)
        {
            var sqrMagnitude = normal.sqrMagnitude;

            if (sqrMagnitude <= 0.000001f || float.IsNaN(sqrMagnitude) || float.IsInfinity(sqrMagnitude))
                return false;

            return Vector3.Dot(normal.normalized, upDirection) > MinimumSupportNormalDot;
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

        private readonly struct SupportHit
        {
            public Vector3 Normal { get; }

            public SupportHit(Vector3 normal)
            {
                Normal = normal;
            }
        }
    }
}
