using System;
using UnityEngine;

namespace Game.Gameplay
{
    internal sealed class PhysicsRunSupportProbe : IRunSupportProbe
    {
        private const int HitCapacity = 16;

        private readonly RunSurfaceProbeConfig _config;
        private readonly IRunSupportColliderProbe _supportProbe;
        private readonly RaycastHit[] _supportHits = new RaycastHit[HitCapacity];
        private readonly RaycastHit[] _normalProbeHits = new RaycastHit[HitCapacity];
        private readonly Collider[] _overlapHits = new Collider[HitCapacity];
        private readonly IRunSurfaceSlopeCalculator _slopeCalculator;
        private readonly RunSupportHitValidator _supportHitValidator;
        private readonly RunSupportFootprintResolver _supportFootprintResolver;

        public Vector3 SampleOrigin => _supportProbe.Collider.bounds.center;

        public PhysicsRunSupportProbe(
            Collider supportCollider,
            IRunSupportColliderProbeFactory runSupportColliderProbeFactory,
            RunSurfaceProbeConfig config,
            IRunSurfaceSlopeCalculator slopeCalculator)
        {
            if (supportCollider == null)
                throw new ArgumentNullException(nameof(supportCollider));

            var supportProbeFactory = runSupportColliderProbeFactory
                                      ?? throw new ArgumentNullException(nameof(runSupportColliderProbeFactory));

            _slopeCalculator = slopeCalculator ?? throw new ArgumentNullException(nameof(slopeCalculator));
            _config = config;
            _supportProbe = supportProbeFactory.Create(supportCollider);

            _supportHitValidator = new RunSupportHitValidator(
                _supportProbe,
                _config.SurfaceMask,
                _config.MinimumSupportNormalDot);

            _supportFootprintResolver = new RunSupportFootprintResolver(
                _supportProbe,
                _config.SurfaceMask,
                _supportHitValidator,
                _config.FootprintSampleOffsetScale,
                _config.FootprintNormalClusterAngleDegrees);
        }

        public RunSupportObservation Observe(
            RunProgressFrameSnapshot frame,
            bool hasContinuityNormal,
            Vector3 continuityNormal)
        {
            if (!frame.IsValid)
                throw new ArgumentException("A valid Run Progress Frame is required.", nameof(frame));

            if (!TryGetBestSupportHit(frame, hasContinuityNormal, continuityNormal, out var supportHit))
            {
                return new RunSupportObservation(
                    RunSupportObservationState.Missing,
                    frame,
                    new RunSurfaceContext(false, Vector3.up, 0f),
                    0f);
            }

            var downhillDegrees = _slopeCalculator.CalculateForwardDownhillDegrees(supportHit.Normal, frame);
            var context = new RunSurfaceContext(true, supportHit.Normal, downhillDegrees);

            return new RunSupportObservation(
                RunSupportObservationState.Supported,
                frame,
                context,
                Mathf.Max(0f, supportHit.Distance));
        }

        private bool TryGetBestSupportHit(
            RunProgressFrameSnapshot frame,
            bool hasContinuityNormal,
            Vector3 continuityNormal,
            out RunSupportHit bestHit)
        {
            bestHit = default;
            var upDirection = frame.UpDirection;

            if (TryGetOverlapSupport(upDirection, out bestHit))
                return true;

            if (_config.Distance <= 0f)
                return false;

            var direction = -upDirection;
            var castOffset = upDirection * _config.SkinWidth;
            var distance = _config.Distance + _config.SkinWidth;

            if (_supportFootprintResolver.TryResolve(
                    frame,
                    distance,
                    _config.SkinWidth,
                    hasContinuityNormal,
                    continuityNormal,
                    out bestHit))
            {
                return true;
            }

            var hitCount = _supportProbe.Cast(castOffset, direction, distance, _supportHits, _config.SurfaceMask);
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

                if (normalDot < bestNormalDot
                    || (Mathf.Approximately(normalDot, bestNormalDot) && hit.distance >= bestDistance))
                {
                    continue;
                }

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

            if (!_supportProbe.TryGetSupportSampleOrigin(
                    upDirection,
                    Vector3.zero,
                    _config.SkinWidth,
                    out var probeOrigin))
            {
                return false;
            }

            var hitCount = Physics.RaycastNonAlloc(
                probeOrigin,
                direction,
                _normalProbeHits,
                distance,
                _config.SurfaceMask,
                QueryTriggerInteraction.Ignore);

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

            var overlapCount = _supportProbe.Overlap(_overlapHits, _config.SurfaceMask);
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

                if (normalDot < bestNormalDot
                    || (Mathf.Approximately(normalDot, bestNormalDot) && separationDistance <= bestDistance))
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
