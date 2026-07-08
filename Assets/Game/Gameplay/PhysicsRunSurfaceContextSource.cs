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
        private readonly RunSupportHitValidator _supportHitValidator;
        private readonly RunSupportFootprintResolver _supportFootprintResolver;

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
            _supportHitValidator = new RunSupportHitValidator(_supportProbe, _surfaceMask, MinimumSupportNormalDot);
            _supportFootprintResolver = new RunSupportFootprintResolver(_supportProbe, _surfaceMask, _supportHitValidator);
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

            if (!TryGetBestSupportHit(frame, _supportProbeDistance, out var supportHit))
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

        private static bool AreSameDirection(Vector3 firstDirection, Vector3 secondDirection)
        {
            const float sameSuspectSupportNormalDot = 0.9999f;
            return Vector3.Dot(firstDirection, secondDirection) >= sameSuspectSupportNormalDot;
        }

        private bool TryGetBestSupportHit(
            RunProgressFrameSnapshot frame,
            float supportProbeDistance,
            out RunSupportHit bestHit)
        {
            bestHit = default;
            var upDirection = frame.UpDirection;

            if (TryGetOverlapSupport(upDirection, out bestHit))
                return true;

            if (supportProbeDistance <= 0f)
                return false;

            var direction = -upDirection;
            var castOffset = upDirection * SupportProbeSkinWidth;
            var distance = supportProbeDistance + SupportProbeSkinWidth;
            var hasContinuityNormal = Current.IsGrounded && _hasAcceptedSupportNormal;

            if (_supportFootprintResolver.TryResolve(frame, distance, SupportProbeSkinWidth, hasContinuityNormal, _acceptedSupportNormal,
                    out bestHit))
            {
                return true;
            }

            var hitCount = _supportProbe.Cast(castOffset, direction, distance, _supportHits, _surfaceMask);
            var hasHit = false;
            var bestNormalDot = float.NegativeInfinity;
            var bestDistance = float.PositiveInfinity;

            for (var hitIndex = 0; hitIndex < hitCount; hitIndex += 1)
            {
                var hit = _supportHits[hitIndex];

                if (!_supportHitValidator.IsValidSupportHit(hit)
                    || !TryGetCastSupportNormal(hit, upDirection, direction, distance, out var normal))
                {
                    continue;
                }

                var normalDot = Vector3.Dot(normal, upDirection);

                if (normalDot < bestNormalDot || (Mathf.Approximately(normalDot, bestNormalDot) && hit.distance >= bestDistance))
                    continue;

                bestNormalDot = normalDot;
                bestDistance = hit.distance;
                bestHit = new RunSupportHit(normal, hit.distance, hit.collider);
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

            if (!_supportHitValidator.IsValidSupportNormal(hit.normal, upDirection))
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

            var hitCount = Physics.RaycastNonAlloc(probeOrigin, direction, _normalProbeHits, distance, _surfaceMask, QueryTriggerInteraction.Ignore);
            var bestDistance = float.PositiveInfinity;
            var hasHit = false;

            for (var hitIndex = 0; hitIndex < hitCount; hitIndex += 1)
            {
                var hit = _normalProbeHits[hitIndex];

                if (hit.collider != targetCollider
                    || !_supportHitValidator.IsValidSupportHit(hit)
                    || !_supportHitValidator.IsValidSupportNormal(hit.normal, upDirection)
                    || hit.distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = hit.distance;
                normal = hit.normal.normalized;
                hasHit = true;
            }

            return hasHit;
        }

        private bool TryGetOverlapSupport(Vector3 upDirection, out RunSupportHit supportHit)
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

                if (!_supportHitValidator.IsValidSupportCollider(hitCollider))
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

                if (!_supportHitValidator.IsValidSupportNormal(separationDirection, upDirection))
                    continue;

                var normal = separationDirection.normalized;
                var normalDot = Vector3.Dot(normal, upDirection);

                if (normalDot < bestNormalDot || (Mathf.Approximately(normalDot, bestNormalDot) && separationDistance <= bestDistance))
                {
                    continue;
                }

                bestNormalDot = normalDot;
                bestDistance = separationDistance;
                supportHit = new RunSupportHit(normal, separationDistance, hitCollider);
                hasHit = true;
            }

            return hasHit;
        }
    }
}
