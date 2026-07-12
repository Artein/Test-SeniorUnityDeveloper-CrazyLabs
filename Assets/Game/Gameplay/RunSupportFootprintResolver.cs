using UnityEngine;

namespace Game.Gameplay
{
    internal sealed class RunSupportFootprintResolver
    {
        private const int SampleCount = 5;
        private const int HitCapacity = 16;
        private const float ComparisonEpsilon = 0.0001f;

        private readonly IRunSupportColliderProbe _supportProbe;
        private readonly LayerMask _surfaceMask;
        private readonly RunSupportHitValidator _supportHitValidator;
        private readonly float _sampleOffsetScale;
        private readonly float _normalClusterMinimumDot;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[HitCapacity];
        private readonly Candidate[] _candidates = new Candidate[SampleCount];
        private readonly Cluster[] _clusters = new Cluster[SampleCount];

        public RunSupportFootprintResolver(
            IRunSupportColliderProbe supportProbe,
            LayerMask surfaceMask,
            RunSupportHitValidator supportHitValidator,
            float sampleOffsetScale = 0.6f,
            float normalClusterAngleDegrees = 8f)
        {
            _supportProbe = supportProbe;
            _surfaceMask = surfaceMask;
            _supportHitValidator = supportHitValidator;
            _sampleOffsetScale = sampleOffsetScale;
            _normalClusterMinimumDot = Mathf.Cos(normalClusterAngleDegrees * Mathf.Deg2Rad);
        }

        public bool TryResolve(
            RunProgressFrameSnapshot frame,
            float distance,
            float skinWidth,
            bool hasContinuityNormal,
            Vector3 continuityNormal,
            out RunSupportHit supportHit)
        {
            supportHit = default;

            if (distance <= 0f)
                return false;

            var rightOffset =
                frame.RightDirection * (_supportProbe.GetProjectedFootprintExtent(frame.RightDirection) * _sampleOffsetScale);

            var forwardOffset =
                frame.ForwardDirection * (_supportProbe.GetProjectedFootprintExtent(frame.ForwardDirection) * _sampleOffsetScale);
            var candidateCount = 0;

            TryAddCandidate(Vector3.zero, frame.UpDirection, distance, skinWidth, ref candidateCount);
            TryAddCandidate(rightOffset, frame.UpDirection, distance, skinWidth, ref candidateCount);
            TryAddCandidate(-rightOffset, frame.UpDirection, distance, skinWidth, ref candidateCount);
            TryAddCandidate(forwardOffset, frame.UpDirection, distance, skinWidth, ref candidateCount);
            TryAddCandidate(-forwardOffset, frame.UpDirection, distance, skinWidth, ref candidateCount);

            if (candidateCount <= 0)
                return false;

            var clusterCount = CreateClusters(candidateCount);

            if (clusterCount <= 0)
                return false;

            var clusterIndex = FindBestClusterIndex(clusterCount, frame.UpDirection, hasContinuityNormal, continuityNormal);
            var bestCluster = _clusters[clusterIndex];
            supportHit = new RunSupportHit(bestCluster.Normal, bestCluster.Distance, bestCluster.Collider);
            return true;
        }

        private void TryAddCandidate(
            Vector3 lateralOffset,
            Vector3 upDirection,
            float distance,
            float skinWidth,
            ref int candidateCount)
        {
            if (candidateCount >= SampleCount
                || !_supportProbe.TryGetSupportSampleOrigin(upDirection, lateralOffset, skinWidth, out var origin)
                || !TryCollectCandidate(origin, upDirection, distance, out var candidate))
            {
                return;
            }

            _candidates[candidateCount] = candidate;
            candidateCount += 1;
        }

        private bool TryCollectCandidate(
            Vector3 origin,
            Vector3 upDirection,
            float distance,
            out Candidate candidate)
        {
            candidate = default;

            var hitCount = Physics.RaycastNonAlloc(origin, -upDirection, _raycastHits, distance, _surfaceMask, QueryTriggerInteraction.Ignore);
            var hasHit = false;
            var bestNormalDot = float.NegativeInfinity;
            var bestDistance = float.PositiveInfinity;
            var bestNormal = Vector3.up;
            Collider bestCollider = null;

            for (var hitIndex = 0; hitIndex < hitCount; hitIndex += 1)
            {
                var hit = _raycastHits[hitIndex];

                if (!_supportHitValidator.IsValidSupportHit(hit) || !_supportHitValidator.IsValidSupportNormal(hit.normal, upDirection))
                    continue;

                var normal = hit.normal.normalized;
                var normalDot = Vector3.Dot(normal, upDirection);

                if (normalDot < bestNormalDot || (Mathf.Approximately(normalDot, bestNormalDot) && hit.distance >= bestDistance))
                    continue;

                bestNormalDot = normalDot;
                bestDistance = hit.distance;
                bestNormal = normal;
                bestCollider = hit.collider;
                hasHit = true;
            }

            if (!hasHit)
                return false;

            candidate = new Candidate(bestNormal, bestDistance, bestCollider);
            return true;
        }

        private int CreateClusters(int candidateCount)
        {
            var clusterCount = 0;

            for (var candidateIndex = 0; candidateIndex < candidateCount; candidateIndex += 1)
            {
                var candidate = _candidates[candidateIndex];
                var clusterIndex = FindClusterIndex(candidate.Normal, clusterCount);

                if (clusterIndex < 0)
                {
                    _clusters[clusterCount] = new Cluster(candidate.Normal, candidate.Distance, candidate.Collider);
                    clusterCount += 1;
                    continue;
                }

                var cluster = _clusters[clusterIndex];
                cluster.Add(candidate);
                _clusters[clusterIndex] = cluster;
            }

            return clusterCount;
        }

        private int FindClusterIndex(Vector3 normal, int clusterCount)
        {
            for (var clusterIndex = 0; clusterIndex < clusterCount; clusterIndex += 1)
            {
                if (Vector3.Dot(_clusters[clusterIndex].Normal, normal) >= _normalClusterMinimumDot)
                    return clusterIndex;
            }

            return -1;
        }

        private int FindBestClusterIndex(
            int clusterCount,
            Vector3 upDirection,
            bool hasContinuityNormal,
            Vector3 continuityNormal)
        {
            var bestClusterIndex = 0;

            for (var clusterIndex = 1; clusterIndex < clusterCount; clusterIndex += 1)
            {
                var cluster = _clusters[clusterIndex];
                var currentBestCluster = _clusters[bestClusterIndex];

                if (IsBetterCluster(cluster, currentBestCluster, upDirection, hasContinuityNormal, continuityNormal))
                    bestClusterIndex = clusterIndex;
            }

            return bestClusterIndex;
        }

        private bool IsBetterCluster(
            Cluster candidate,
            Cluster currentBest,
            Vector3 upDirection,
            bool hasContinuityNormal,
            Vector3 continuityNormal)
        {
            if (candidate.Count != currentBest.Count)
                return candidate.Count > currentBest.Count;

            if (hasContinuityNormal)
            {
                var candidateContinuityDot = Vector3.Dot(candidate.Normal, continuityNormal);
                var bestContinuityDot = Vector3.Dot(currentBest.Normal, continuityNormal);

                if (HasMeaningfulDelta(candidateContinuityDot, bestContinuityDot))
                    return candidateContinuityDot > bestContinuityDot;
            }

            var candidateUpDot = Vector3.Dot(candidate.Normal, upDirection);
            var bestUpDot = Vector3.Dot(currentBest.Normal, upDirection);

            if (HasMeaningfulDelta(candidateUpDot, bestUpDot))
                return candidateUpDot > bestUpDot;

            if (HasMeaningfulDelta(candidate.Distance, currentBest.Distance))
                return candidate.Distance < currentBest.Distance;

            return false;
        }

        private bool HasMeaningfulDelta(float firstValue, float secondValue)
        {
            return Mathf.Abs(firstValue - secondValue) > ComparisonEpsilon;
        }

        private readonly struct Candidate
        {
            public Vector3 Normal { get; }

            public float Distance { get; }

            public Collider Collider { get; }

            public Candidate(Vector3 normal, float distance, Collider collider)
            {
                Normal = normal;
                Distance = distance;
                Collider = collider;
            }
        }

        private struct Cluster
        {
            public Vector3 Normal { get; private set; }

            public float Distance { get; private set; }

            public Collider Collider { get; private set; }

            public int Count { get; private set; }

            public Cluster(Vector3 normal, float distance, Collider collider)
            {
                Normal = normal;
                Distance = distance;
                Collider = collider;
                Count = 1;
            }

            public void Add(Candidate candidate)
            {
                var normal = (Normal * Count) + candidate.Normal;
                Normal = normal.normalized;
                Count += 1;

                if (candidate.Distance >= Distance)
                    return;

                Distance = candidate.Distance;
                Collider = candidate.Collider;
            }
        }
    }
}
