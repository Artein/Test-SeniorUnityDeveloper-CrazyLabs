using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Game.Gameplay.Slingshot
{
    internal sealed class PullPlaneBandShapeClearance
    {
        private const float Epsilon = 0.0001f;

        private readonly float2[] _sortedSamples;
        private readonly float2[] _hull;
        private readonly Float2Comparer _float2Comparer = new();

        public PullPlaneBandShapeClearance(int maxSilhouetteSampleCount)
        {
            if (maxSilhouetteSampleCount < 3)
                throw new ArgumentOutOfRangeException(nameof(maxSilhouetteSampleCount));

            _sortedSamples = new float2[maxSilhouetteSampleCount];
            _hull = new float2[maxSilhouetteSampleCount * 2];
        }

        public bool IsClear(
            float2[] bandShapePoints,
            int bandShapePointCount,
            float2[] silhouetteSamples,
            int silhouetteSampleCount,
            float clearanceRadius)
        {
            if (!CanCheck(bandShapePoints, bandShapePointCount, silhouetteSamples, silhouetteSampleCount, clearanceRadius))
                return false;

            if (!TryBuildConvexHull(silhouetteSamples, silhouetteSampleCount, out var hullCount))
                return false;

            var blockedDistance = math.max(0f, clearanceRadius) + Epsilon;
            var blockedDistanceSquared = blockedDistance * blockedDistance;

            for (var pointIndex = 0; pointIndex < bandShapePointCount - 1; pointIndex += 1)
            {
                var segmentStart = bandShapePoints[pointIndex];
                var segmentEnd = bandShapePoints[pointIndex + 1];

                if (IsPointInsideOrOnConvexHull(segmentStart, hullCount)
                    || IsPointInsideOrOnConvexHull(segmentEnd, hullCount))
                {
                    return false;
                }

                for (var hullIndex = 0; hullIndex < hullCount; hullIndex += 1)
                {
                    var edgeStart = _hull[hullIndex];
                    var edgeEnd = _hull[NextIndex(hullIndex, hullCount)];

                    if (GetSegmentDistanceSquared(segmentStart, segmentEnd, edgeStart, edgeEnd) <= blockedDistanceSquared)
                        return false;
                }
            }

            return true;
        }

        private bool CanCheck(
            float2[] bandShapePoints,
            int bandShapePointCount,
            float2[] silhouetteSamples,
            int silhouetteSampleCount,
            float clearanceRadius)
        {
            if (bandShapePoints is null
                || silhouetteSamples is null
                || bandShapePointCount < 2
                || bandShapePointCount > bandShapePoints.Length
                || silhouetteSampleCount < 3
                || silhouetteSampleCount > silhouetteSamples.Length
                || silhouetteSampleCount > _sortedSamples.Length
                || !math.isfinite(clearanceRadius)
                || clearanceRadius < 0f)
            {
                return false;
            }

            for (var pointIndex = 0; pointIndex < bandShapePointCount; pointIndex += 1)
            {
                if (!IsFinite(bandShapePoints[pointIndex]))
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

                if (uniqueCount > 0 && math.distancesq(sample, _sortedSamples[uniqueCount - 1]) <= Epsilon * Epsilon)
                    continue;

                _sortedSamples[uniqueCount] = sample;
                uniqueCount += 1;
            }

            if (uniqueCount < 3)
                return false;

            for (var i = 0; i < uniqueCount; i += 1)
            {
                while (hullCount >= 2
                       && Cross(_hull[hullCount - 1] - _hull[hullCount - 2], _sortedSamples[i] - _hull[hullCount - 1]) <= Epsilon)
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
                       && Cross(_hull[hullCount - 1] - _hull[hullCount - 2], _sortedSamples[i] - _hull[hullCount - 1]) <= Epsilon)
                {
                    hullCount -= 1;
                }

                _hull[hullCount] = _sortedSamples[i];
                hullCount += 1;
            }

            hullCount -= 1;
            return hullCount >= 3 && GetSignedArea(_hull, hullCount) > Epsilon;
        }

        private bool IsPointInsideOrOnConvexHull(float2 point, int hullCount)
        {
            for (var hullIndex = 0; hullIndex < hullCount; hullIndex += 1)
            {
                var edgeStart = _hull[hullIndex];
                var edgeEnd = _hull[NextIndex(hullIndex, hullCount)];

                if (Cross(edgeEnd - edgeStart, point - edgeStart) < -Epsilon)
                    return false;
            }

            return true;
        }

        private float GetSegmentDistanceSquared(float2 firstStart, float2 firstEnd, float2 secondStart, float2 secondEnd)
        {
            if (TryIntersectSegments(firstStart, firstEnd, secondStart, secondEnd))
                return 0f;

            return math.min(
                math.min(GetPointSegmentDistanceSquared(firstStart, secondStart, secondEnd),
                    GetPointSegmentDistanceSquared(firstEnd, secondStart, secondEnd)),
                math.min(GetPointSegmentDistanceSquared(secondStart, firstStart, firstEnd),
                    GetPointSegmentDistanceSquared(secondEnd, firstStart, firstEnd)));
        }

        private bool TryIntersectSegments(float2 firstStart, float2 firstEnd, float2 secondStart, float2 secondEnd)
        {
            var firstDirection = firstEnd - firstStart;
            var secondDirection = secondEnd - secondStart;
            var denominator = Cross(firstDirection, secondDirection);

            if (math.abs(denominator) <= Epsilon)
                return TryIntersectCollinearSegments(firstStart, firstEnd, secondStart, secondEnd);

            var delta = secondStart - firstStart;
            var firstProgress = Cross(delta, secondDirection) / denominator;
            var secondProgress = Cross(delta, firstDirection) / denominator;

            return firstProgress is >= -Epsilon and <= 1f + Epsilon
                   && secondProgress is >= -Epsilon and <= 1f + Epsilon;
        }

        private bool TryIntersectCollinearSegments(float2 firstStart, float2 firstEnd, float2 secondStart, float2 secondEnd)
        {
            var firstDirection = firstEnd - firstStart;
            var firstLengthSquared = math.lengthsq(firstDirection);

            if (firstLengthSquared <= Epsilon * Epsilon)
                return math.distancesq(firstStart, secondStart) <= Epsilon * Epsilon
                       || math.distancesq(firstStart, secondEnd) <= Epsilon * Epsilon;

            if (math.abs(Cross(secondStart - firstStart, firstDirection)) > Epsilon
                || math.abs(Cross(secondEnd - firstStart, firstDirection)) > Epsilon)
            {
                return false;
            }

            var secondStartProgress = math.dot(secondStart - firstStart, firstDirection) / firstLengthSquared;
            var secondEndProgress = math.dot(secondEnd - firstStart, firstDirection) / firstLengthSquared;
            var overlapStart = math.max(math.min(secondStartProgress, secondEndProgress), 0f);
            var overlapEnd = math.min(math.max(secondStartProgress, secondEndProgress), 1f);
            return overlapStart <= overlapEnd + Epsilon;
        }

        private float GetPointSegmentDistanceSquared(float2 point, float2 segmentStart, float2 segmentEnd)
        {
            var segment = segmentEnd - segmentStart;
            var lengthSquared = math.lengthsq(segment);

            if (lengthSquared <= Epsilon * Epsilon)
                return math.distancesq(point, segmentStart);

            var progress = math.clamp(math.dot(point - segmentStart, segment) / lengthSquared, 0f, 1f);
            var closestPoint = math.lerp(segmentStart, segmentEnd, progress);
            return math.distancesq(point, closestPoint);
        }

        private float GetSignedArea(float2[] points, int pointCount)
        {
            var area = 0f;

            for (var pointIndex = 0; pointIndex < pointCount; pointIndex += 1)
            {
                area += Cross(points[pointIndex], points[NextIndex(pointIndex, pointCount)]);
            }

            return area * 0.5f;
        }

        private int NextIndex(int index, int count)
        {
            return index + 1 < count ? index + 1 : 0;
        }

        private bool IsFinite(float2 value)
        {
            return math.all(math.isfinite(value));
        }

        private float Cross(float2 left, float2 right)
        {
            return (left.x * right.y) - (left.y * right.x);
        }

        private sealed class Float2Comparer : IComparer<float2>
        {
            public int Compare(float2 left, float2 right)
            {
                var xComparison = left.x.CompareTo(right.x);
                return xComparison != 0 ? xComparison : left.y.CompareTo(right.y);
            }
        }
    }
}
