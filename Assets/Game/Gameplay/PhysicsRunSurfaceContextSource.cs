using System;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay
{
    public interface IRunSurfaceContextSource
    {
        RunSurfaceContext Current { get; }
    }

    internal sealed partial class PhysicsRunSurfaceContextSource : IRunSurfaceContextSource, IFixedTickable
    {
        private const float SupportProbeSkinWidth = 0.02f;
        private const float MinimumSupportNormalDot = 0.17f;
        private const float SupportNormalSnapDegrees = 45f;
        private const float SameSuspectSupportNormalDot = 0.9999f;
        private const int UngroundedMissThreshold = 2;
        private const int SuspectSupportNormalConfirmationSampleCount = 2;

        private readonly IRunSupportColliderProbe _supportProbe;
        private readonly IRunProgressFrameSource _runProgressFrameSource;
        private readonly float _supportProbeDistance;
        private readonly LayerMask _surfaceMask;
        private readonly RaycastHit[] _supportHits = new RaycastHit[16];
        private readonly RaycastHit[] _normalProbeHits = new RaycastHit[16];
        private readonly Collider[] _overlapHits = new Collider[16];
        private readonly IRunSurfaceSlopeCalculator _slopeCalculator = new RunSurfaceSlopeCalculator();

        private int _consecutiveMissedSupportSamples = UngroundedMissThreshold;
        private Vector3 _acceptedSupportNormal = Vector3.up;
        private Vector3 _suspectSupportNormal = Vector3.up;
        private bool _hasAcceptedSupportNormal;
        private bool _hasSuspectSupportNormal;
        private int _suspectSupportNormalSampleCount;

        public RunSurfaceContext Current { get; private set; } = new(isGrounded: false, Vector3.up, forwardDownhillDegrees: 0f);

        public PhysicsRunSurfaceContextSource(
            Collider supportCollider,
            IRunProgressFrameSource runProgressFrameSource,
            IRunSupportColliderProbeFactory runSupportColliderProbeFactory,
            float supportProbeDistance,
            LayerMask surfaceMask)
        {
            if (supportCollider == null)
                throw new ArgumentNullException(nameof(supportCollider));

            _runProgressFrameSource = runProgressFrameSource ?? throw new ArgumentNullException(nameof(runProgressFrameSource));
            var supportProbeFactory = runSupportColliderProbeFactory ?? throw new ArgumentNullException(nameof(runSupportColliderProbeFactory));
            _supportProbe = supportProbeFactory.Create(supportCollider);
            _supportProbeDistance = Mathf.Max(0f, supportProbeDistance);
            _surfaceMask = surfaceMask;
        }

        void IFixedTickable.FixedTick()
        {
            Sample();
        }

        private void Sample()
        {
            if (!_runProgressFrameSource.TryCreateSnapshot(_supportProbe.Collider.bounds.center, out var frame, out _))
            {
                SetUngrounded();
                return;
            }

            if (!TryGetBestSupportHit(frame.UpDirection, _supportProbeDistance, out var supportHit))
            {
                RecordMissedSupportSample();
                return;
            }

            _consecutiveMissedSupportSamples = 0;

            var supportNormal = AcceptOrKeepSupportNormal(supportHit.Normal);
            var downhillDegrees = _slopeCalculator.CalculateForwardDownhillDegrees(supportNormal, frame);
            Current = new RunSurfaceContext(true, supportNormal, downhillDegrees);
        }

        private void SetUngrounded()
        {
            _consecutiveMissedSupportSamples = UngroundedMissThreshold;
            ClearAcceptedSupportNormal();
            Current = new RunSurfaceContext(false, Vector3.up, 0f);
        }

        private void RecordMissedSupportSample()
        {
            _consecutiveMissedSupportSamples += 1;

            if (Current.IsGrounded && _consecutiveMissedSupportSamples < UngroundedMissThreshold)
                return;

            ClearAcceptedSupportNormal();
            Current = new RunSurfaceContext(false, Vector3.up, 0f);
        }

        private Vector3 AcceptOrKeepSupportNormal(Vector3 sampledSupportNormal)
        {
            var supportNormal = sampledSupportNormal.normalized;

            if (!Current.IsGrounded || !_hasAcceptedSupportNormal)
            {
                AcceptSupportNormal(supportNormal);
                return _acceptedSupportNormal;
            }

            var angleToSample = Vector3.Angle(_acceptedSupportNormal, supportNormal);

            if (angleToSample <= 0.0001f)
            {
                AcceptSupportNormal(supportNormal);
                return _acceptedSupportNormal;
            }

            if (angleToSample > SupportNormalSnapDegrees)
                return ProcessSuspectSupportNormal(supportNormal);

            AcceptSupportNormal(supportNormal);
            return _acceptedSupportNormal;
        }

        private Vector3 ProcessSuspectSupportNormal(Vector3 supportNormal)
        {
            if (!_hasSuspectSupportNormal || !AreSameDirection(_suspectSupportNormal, supportNormal))
            {
                _suspectSupportNormal = supportNormal;
                _suspectSupportNormalSampleCount = 1;
                _hasSuspectSupportNormal = true;
            }
            else
            {
                _suspectSupportNormalSampleCount += 1;
            }

            if (_suspectSupportNormalSampleCount < SuspectSupportNormalConfirmationSampleCount)
                return _acceptedSupportNormal;

            AcceptSupportNormal(supportNormal);
            return _acceptedSupportNormal;
        }

        private void AcceptSupportNormal(Vector3 supportNormal)
        {
            _acceptedSupportNormal = supportNormal.normalized;
            _hasAcceptedSupportNormal = true;
            ClearSuspectSupportNormal();
        }

        private void ClearAcceptedSupportNormal()
        {
            _acceptedSupportNormal = Vector3.up;
            _hasAcceptedSupportNormal = false;
            ClearSuspectSupportNormal();
        }

        private void ClearSuspectSupportNormal()
        {
            _suspectSupportNormal = Vector3.up;
            _hasSuspectSupportNormal = false;
            _suspectSupportNormalSampleCount = 0;
        }

        private bool AreSameDirection(Vector3 firstDirection, Vector3 secondDirection)
        {
            return Vector3.Dot(firstDirection, secondDirection) >= SameSuspectSupportNormalDot;
        }

        private bool TryGetBestSupportHit(Vector3 upDirection, float supportProbeDistance, out SupportHit bestHit)
        {
            bestHit = default;

            if (TryGetOverlapSupport(upDirection, out bestHit))
                return true;

            if (supportProbeDistance <= 0f)
                return false;

            var direction = -upDirection;
            var castOffset = upDirection * SupportProbeSkinWidth;
            var distance = supportProbeDistance + SupportProbeSkinWidth;
            var hitCount = _supportProbe.Cast(castOffset, direction, distance, _supportHits, _surfaceMask);
            var hasHit = false;
            var bestNormalDot = float.NegativeInfinity;
            var bestDistance = float.PositiveInfinity;

            for (var hitIndex = 0; hitIndex < hitCount; hitIndex += 1)
            {
                var hit = _supportHits[hitIndex];

                if (!IsValidSupportHit(hit))
                    continue;

                if (!TryGetCastSupportNormal(hit, upDirection, direction, distance, out var normal))
                    continue;

                var normalDot = Vector3.Dot(normal, upDirection);

                if (normalDot < bestNormalDot
                    || (Mathf.Approximately(normalDot, bestNormalDot) && hit.distance >= bestDistance))
                {
                    continue;
                }

                bestNormalDot = normalDot;
                bestDistance = hit.distance;
                bestHit = new SupportHit(normal);
                hasHit = true;
            }

            return hasHit;
        }

        private bool TryGetCastSupportNormal(
            RaycastHit hit,
            Vector3 upDirection,
            Vector3 direction,
            float distance,
            out Vector3 normal)
        {
            normal = default;

            if (!IsValidSupportNormal(hit.normal, upDirection))
                return false;

            normal = hit.normal.normalized;

            if (!TryProbeSupportSurfaceNormal(hit.collider, upDirection, direction, distance, out var probedNormal))
                return true;

            normal = probedNormal;
            return true;
        }

        private bool TryProbeSupportSurfaceNormal(
            Collider targetCollider,
            Vector3 upDirection,
            Vector3 direction,
            float distance,
            out Vector3 normal)
        {
            normal = default;

            var probeOrigin = _supportProbe.GetSupportProbeOrigin(upDirection, SupportProbeSkinWidth);

            var hitCount = Physics.RaycastNonAlloc(
                probeOrigin,
                direction,
                _normalProbeHits,
                distance,
                _surfaceMask,
                QueryTriggerInteraction.Ignore);
            var bestDistance = float.PositiveInfinity;
            var hasHit = false;

            for (var hitIndex = 0; hitIndex < hitCount; hitIndex += 1)
            {
                var hit = _normalProbeHits[hitIndex];

                if (hit.collider != targetCollider || !IsValidSupportHit(hit) || !IsValidSupportNormal(hit.normal, upDirection))
                    continue;

                if (hit.distance >= bestDistance)
                    continue;

                bestDistance = hit.distance;
                normal = hit.normal.normalized;
                hasHit = true;
            }

            return hasHit;
        }

        private bool TryGetOverlapSupport(Vector3 upDirection, out SupportHit supportHit)
        {
            supportHit = default;

            var overlapCount = _supportProbe.Overlap(_overlapHits, _surfaceMask);
            var hasHit = false;
            var bestNormalDot = float.NegativeInfinity;
            var bestDistance = float.NegativeInfinity;
            var supportCollider = _supportProbe.Collider;

            for (var overlapIndex = 0; overlapIndex < overlapCount; overlapIndex += 1)
            {
                var hitCollider = _overlapHits[overlapIndex];

                if (!IsValidSupportCollider(hitCollider))
                    continue;

                if (!Physics.ComputePenetration(
                        supportCollider,
                        supportCollider.transform.position,
                        supportCollider.transform.rotation,
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

        private bool IsValidSupportHit(RaycastHit hit)
        {
            return IsValidSupportCollider(hit.collider);
        }

        private bool IsValidSupportCollider(Collider hitCollider)
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

        private bool IsValidSupportNormal(Vector3 normal, Vector3 upDirection)
        {
            var sqrMagnitude = normal.sqrMagnitude;

            if (sqrMagnitude <= 0.000001f || float.IsNaN(sqrMagnitude) || float.IsInfinity(sqrMagnitude))
                return false;

            return Vector3.Dot(normal.normalized, upDirection) > MinimumSupportNormalDot;
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
