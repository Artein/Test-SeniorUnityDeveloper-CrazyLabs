using System;
using Unity.Mathematics;

namespace Game.Gameplay.Slingshot
{
    internal sealed partial class PullPlaneTautBandSolver
    {
        private readonly float2[] _sortedSamples;
        private readonly float2[] _hull;
        private readonly float2[] _inflatedHull;
        private readonly int[] _leftTangentCandidates;
        private readonly int[] _rightTangentCandidates;
        private readonly int[] _candidatePathIndices;
        private readonly int[] _bestPathIndices;
        private readonly Float2Comparer _float2Comparer = new();
        private readonly int _maxBandWrapSampleCount;
        private readonly float _epsilon = 0.0001f;

        public PullPlaneTautBandSolver(int maxSilhouetteSampleCount, int maxBandWrapSampleCount)
        {
            if (maxSilhouetteSampleCount < 3)
                throw new ArgumentOutOfRangeException(nameof(maxSilhouetteSampleCount));

            if (maxBandWrapSampleCount < 3)
                throw new ArgumentOutOfRangeException(nameof(maxBandWrapSampleCount));

            _sortedSamples = new float2[maxSilhouetteSampleCount];
            _hull = new float2[maxSilhouetteSampleCount * 2];
            _inflatedHull = new float2[maxSilhouetteSampleCount];
            _leftTangentCandidates = new int[maxSilhouetteSampleCount];
            _rightTangentCandidates = new int[maxSilhouetteSampleCount];
            _candidatePathIndices = new int[maxSilhouetteSampleCount + 1];
            _bestPathIndices = new int[maxSilhouetteSampleCount + 1];
            _maxBandWrapSampleCount = maxBandWrapSampleCount;
        }

        public bool TrySolve(
            float2 leftAnchor,
            float2 rightAnchor,
            float2 pullPoint,
            float2[] silhouetteSamples,
            int silhouetteSampleCount,
            float contactPadding,
            int wrapSampleCount,
            float2[] outputPoints,
            out int pointCount)
        {
            pointCount = 0;

            if (outputPoints is null)
                throw new ArgumentNullException(nameof(outputPoints));

            var requiredPointCount = wrapSampleCount + 4;

            if (outputPoints.Length < requiredPointCount)
                throw new ArgumentException("Output buffer is too small for the requested Band Shape.", nameof(outputPoints));

            if (!CanAttemptSolve(leftAnchor, rightAnchor, pullPoint, silhouetteSamples, silhouetteSampleCount, contactPadding, wrapSampleCount))
                return false;

            if (!TryBuildConvexHull(silhouetteSamples, silhouetteSampleCount, out var hullCount))
                return false;

            if (!TryInflateHull(hullCount, contactPadding, out var inflatedCount))
                return false;

            if (!TryFindPulledSideCenter(pullPoint, inflatedCount, out var pulledSideCenter))
                return false;

            if (!TryFindTangentCandidates(leftAnchor, inflatedCount, _leftTangentCandidates, out var leftCandidateCount)
                || !TryFindTangentCandidates(rightAnchor, inflatedCount, _rightTangentCandidates, out var rightCandidateCount))
            {
                return false;
            }

            if (!TrySelectPath(
                    leftAnchor,
                    rightAnchor,
                    pulledSideCenter,
                    inflatedCount,
                    leftCandidateCount,
                    rightCandidateCount,
                    out var bestPathCount,
                    out var centerSegmentIndex,
                    out var centerSegmentProgress))
            {
                return false;
            }

            WriteOutput(
                leftAnchor,
                rightAnchor,
                pulledSideCenter,
                wrapSampleCount,
                outputPoints,
                bestPathCount,
                centerSegmentIndex,
                centerSegmentProgress);

            pointCount = requiredPointCount;
            return true;
        }

        private bool CanAttemptSolve(
            float2 leftAnchor,
            float2 rightAnchor,
            float2 pullPoint,
            float2[] silhouetteSamples,
            int silhouetteSampleCount,
            float contactPadding,
            int wrapSampleCount)
        {
            if (!IsFinite(leftAnchor)
                || !IsFinite(rightAnchor)
                || !IsFinite(pullPoint)
                || silhouetteSamples is null
                || silhouetteSampleCount < 3
                || silhouetteSampleCount > silhouetteSamples.Length
                || silhouetteSampleCount > _sortedSamples.Length
                || !math.isfinite(contactPadding)
                || contactPadding < 0f
                || wrapSampleCount < 3
                || wrapSampleCount > _maxBandWrapSampleCount
                || wrapSampleCount % 2 == 0)
            {
                return false;
            }

            for (var sampleIndex = 0; sampleIndex < silhouetteSampleCount; sampleIndex += 1)
            {
                if (!IsFinite(silhouetteSamples[sampleIndex]))
                    return false;
            }

            return true;
        }

        private bool TryBuildConvexHull(float2[] samples, int sampleCount, out int hullCount)
        {
            hullCount = 0;

            for (var i = 0; i < sampleCount; i += 1)
            {
                _sortedSamples[i] = samples[i];
            }

            Array.Sort(_sortedSamples, 0, sampleCount, _float2Comparer);

            var uniqueCount = 0;

            for (var i = 0; i < sampleCount; i += 1)
            {
                var sample = _sortedSamples[i];

                if (uniqueCount > 0 && math.distancesq(sample, _sortedSamples[uniqueCount - 1]) <= _epsilon * _epsilon)
                    continue;

                _sortedSamples[uniqueCount] = sample;
                uniqueCount += 1;
            }

            if (uniqueCount < 3)
                return false;

            for (var i = 0; i < uniqueCount; i += 1)
            {
                while (hullCount >= 2
                       && Cross(_hull[hullCount - 1] - _hull[hullCount - 2], _sortedSamples[i] - _hull[hullCount - 1]) <= _epsilon)
                {
                    hullCount -= 1;
                }

                _hull[hullCount] = _sortedSamples[i];
                hullCount += 1;
            }

            var lowerHullCount = hullCount;

            for (var i = uniqueCount - 2; i >= 0; i -= 1)
            {
                while (hullCount > lowerHullCount
                       && Cross(_hull[hullCount - 1] - _hull[hullCount - 2], _sortedSamples[i] - _hull[hullCount - 1]) <= _epsilon)
                {
                    hullCount -= 1;
                }

                _hull[hullCount] = _sortedSamples[i];
                hullCount += 1;
            }

            hullCount -= 1;
            return hullCount >= 3 && GetSignedArea(_hull, hullCount) > _epsilon;
        }

        private bool TryInflateHull(int hullCount, float contactPadding, out int inflatedCount)
        {
            inflatedCount = hullCount;

            if (contactPadding <= _epsilon)
            {
                for (var i = 0; i < hullCount; i += 1)
                {
                    _inflatedHull[i] = _hull[i];
                }

                return true;
            }

            var center = GetCentroid(_hull, hullCount);

            for (var i = 0; i < hullCount; i += 1)
            {
                var previousIndex = PreviousIndex(i, hullCount);
                var current = _hull[i];
                var previousEdgeStart = _hull[previousIndex];
                var previousEdgeEnd = current;
                var currentEdgeStart = current;
                var currentEdgeEnd = _hull[NextIndex(i, hullCount)];
                var previousOffsetStart = previousEdgeStart + (GetOutwardNormal(previousEdgeEnd - previousEdgeStart) * contactPadding);
                var previousOffsetEnd = previousEdgeEnd + (GetOutwardNormal(previousEdgeEnd - previousEdgeStart) * contactPadding);
                var currentOffsetStart = currentEdgeStart + (GetOutwardNormal(currentEdgeEnd - currentEdgeStart) * contactPadding);
                var currentOffsetEnd = currentEdgeEnd + (GetOutwardNormal(currentEdgeEnd - currentEdgeStart) * contactPadding);

                if (TryIntersectLines(previousOffsetStart, previousOffsetEnd, currentOffsetStart, currentOffsetEnd, out var inflatedVertex))
                {
                    _inflatedHull[i] = inflatedVertex;
                    continue;
                }

                var fallbackDirection = math.normalizesafe(current - center, new float2(0f, 1f));
                _inflatedHull[i] = current + (fallbackDirection * contactPadding);
            }

            return GetSignedArea(_inflatedHull, inflatedCount) > _epsilon;
        }

        private bool TryFindPulledSideCenter(float2 pullPoint, int hullCount, out float2 pulledSideCenter)
        {
            pulledSideCenter = default;
            var center = GetCentroid(_inflatedHull, hullCount);
            var direction = math.normalizesafe(pullPoint, new float2(0f, 1f));
            var bestDistance = float.MaxValue;
            var found = false;

            for (var i = 0; i < hullCount; i += 1)
            {
                var edgeStart = _inflatedHull[i];
                var edgeEnd = _inflatedHull[NextIndex(i, hullCount)];

                if (!TryIntersectRaySegment(center, direction, edgeStart, edgeEnd, out var distance, out var point))
                    continue;

                if (distance < -_epsilon || distance >= bestDistance)
                    continue;

                bestDistance = distance;
                pulledSideCenter = point;
                found = true;
            }

            if (found)
                return true;

            var bestDot = float.MinValue;

            for (var i = 0; i < hullCount; i += 1)
            {
                var dot = math.dot(_inflatedHull[i] - center, direction);

                if (dot <= bestDot)
                    continue;

                bestDot = dot;
                pulledSideCenter = _inflatedHull[i];
                found = true;
            }

            return found;
        }

        private bool TryFindTangentCandidates(float2 anchor, int hullCount, int[] outputIndices, out int candidateCount)
        {
            candidateCount = 0;

            for (var vertexIndex = 0; vertexIndex < hullCount; vertexIndex += 1)
            {
                var hasPositive = false;
                var hasNegative = false;
                var anchorToVertex = _inflatedHull[vertexIndex] - anchor;

                if (math.lengthsq(anchorToVertex) <= _epsilon * _epsilon)
                    continue;

                for (var otherIndex = 0; otherIndex < hullCount; otherIndex += 1)
                {
                    if (otherIndex == vertexIndex)
                        continue;

                    var side = Cross(anchorToVertex, _inflatedHull[otherIndex] - anchor);

                    if (side > _epsilon)
                        hasPositive = true;
                    else if (side < -_epsilon)
                        hasNegative = true;

                    if (hasPositive && hasNegative)
                        break;
                }

                if (hasPositive && hasNegative)
                    continue;

                outputIndices[candidateCount] = vertexIndex;
                candidateCount += 1;
            }

            return candidateCount > 0;
        }

        private bool TrySelectPath(
            float2 leftAnchor,
            float2 rightAnchor,
            float2 pulledSideCenter,
            int hullCount,
            int leftCandidateCount,
            int rightCandidateCount,
            out int bestPathCount,
            out int bestCenterSegmentIndex,
            out float bestCenterSegmentProgress)
        {
            bestPathCount = 0;
            bestCenterSegmentIndex = 0;
            bestCenterSegmentProgress = 0f;
            var bestLength = float.MaxValue;

            for (var leftCandidateIndex = 0; leftCandidateIndex < leftCandidateCount; leftCandidateIndex += 1)
            {
                for (var rightCandidateIndex = 0; rightCandidateIndex < rightCandidateCount; rightCandidateIndex += 1)
                {
                    var leftIndex = _leftTangentCandidates[leftCandidateIndex];
                    var rightIndex = _rightTangentCandidates[rightCandidateIndex];

                    EvaluateCandidatePath(leftAnchor, rightAnchor, pulledSideCenter, hullCount, leftIndex, rightIndex, 1, ref bestLength,
                        ref bestPathCount, ref bestCenterSegmentIndex, ref bestCenterSegmentProgress);

                    EvaluateCandidatePath(leftAnchor, rightAnchor, pulledSideCenter, hullCount, leftIndex, rightIndex, -1, ref bestLength,
                        ref bestPathCount, ref bestCenterSegmentIndex, ref bestCenterSegmentProgress);
                }
            }

            return bestPathCount >= 2;
        }

        private void EvaluateCandidatePath(
            float2 leftAnchor,
            float2 rightAnchor,
            float2 pulledSideCenter,
            int hullCount,
            int leftIndex,
            int rightIndex,
            int direction,
            ref float bestLength,
            ref int bestPathCount,
            ref int bestCenterSegmentIndex,
            ref float bestCenterSegmentProgress)
        {
            WritePathIndices(leftIndex, rightIndex, direction, hullCount, _candidatePathIndices, out var candidatePathCount);

            if (!TryFindPointOnPath(pulledSideCenter, _candidatePathIndices, candidatePathCount, out var centerSegmentIndex,
                    out var centerSegmentProgress))
                return;

            var leftContact = _inflatedHull[leftIndex];
            var rightContact = _inflatedHull[rightIndex];

            if (FreeSpanCrossesHull(leftAnchor, leftContact, leftIndex, hullCount)
                || FreeSpanCrossesHull(rightAnchor, rightContact, rightIndex, hullCount))
            {
                return;
            }

            var pathLength = math.distance(leftAnchor, leftContact)
                             + GetPathLengthThroughCenter(_candidatePathIndices, candidatePathCount, centerSegmentIndex, centerSegmentProgress)
                             + math.distance(rightContact, rightAnchor);

            if (pathLength >= bestLength)
                return;

            bestLength = pathLength;
            bestPathCount = candidatePathCount;
            bestCenterSegmentIndex = centerSegmentIndex;
            bestCenterSegmentProgress = centerSegmentProgress;

            for (var i = 0; i < candidatePathCount; i += 1)
            {
                _bestPathIndices[i] = _candidatePathIndices[i];
            }
        }

        private void WriteOutput(
            float2 leftAnchor,
            float2 rightAnchor,
            float2 pulledSideCenter,
            int wrapSampleCount,
            float2[] outputPoints,
            int pathCount,
            int centerSegmentIndex,
            float centerSegmentProgress)
        {
            var halfWrapSampleCount = wrapSampleCount / 2;
            outputPoints[0] = leftAnchor;
            outputPoints[1] = _inflatedHull[_bestPathIndices[0]];

            for (var i = 0; i < halfWrapSampleCount; i += 1)
            {
                var progress = (i + 1f) / (halfWrapSampleCount + 1f);
                outputPoints[2 + i] = SampleLeftToCenter(pathCount, centerSegmentIndex, centerSegmentProgress, progress);
            }

            outputPoints[2 + halfWrapSampleCount] = pulledSideCenter;

            for (var i = 0; i < halfWrapSampleCount; i += 1)
            {
                var progress = (i + 1f) / (halfWrapSampleCount + 1f);

                outputPoints[3 + halfWrapSampleCount + i] =
                    SampleCenterToRight(pathCount, centerSegmentIndex, centerSegmentProgress, progress);
            }

            outputPoints[wrapSampleCount + 2] = _inflatedHull[_bestPathIndices[pathCount - 1]];
            outputPoints[wrapSampleCount + 3] = rightAnchor;
        }

        private bool FreeSpanCrossesHull(float2 spanStart, float2 spanEnd, int contactIndex, int hullCount)
        {
            if (IsPointStrictlyInsideConvexHull(spanStart, hullCount)
                || IsPointStrictlyInsideConvexHull(spanEnd, hullCount))
            {
                return true;
            }

            for (var i = 0; i < hullCount; i += 1)
            {
                var nextIndex = NextIndex(i, hullCount);

                if (i == contactIndex || nextIndex == contactIndex)
                    continue;

                if (TryIntersectSegmentsInterior(spanStart, spanEnd, _inflatedHull[i], _inflatedHull[nextIndex]))
                    return true;
            }

            return false;
        }

#if UNITY_INCLUDE_TESTS
        internal bool FreeSpanCrossesHullForTests(float2 spanStart, float2 spanEnd, int contactIndex, float2[] hull, int hullCount)
        {
            if (hull is null)
                throw new ArgumentNullException(nameof(hull));

            if (hullCount < 3 || hullCount > hull.Length || hullCount > _inflatedHull.Length)
                throw new ArgumentOutOfRangeException(nameof(hullCount));

            for (var i = 0; i < hullCount; i += 1)
            {
                _inflatedHull[i] = hull[i];
            }

            return FreeSpanCrossesHull(spanStart, spanEnd, contactIndex, hullCount);
        }
#endif
    }
}
