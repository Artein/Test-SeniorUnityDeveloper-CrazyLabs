using System.Collections.Generic;
using Unity.Mathematics;

namespace Game.Gameplay.Slingshot
{
    internal sealed partial class PullPlaneTautBandSolver
    {
        private float2 SampleLeftToCenter(int pathCount, int centerSegmentIndex, float centerSegmentProgress, float progress)
        {
            var totalLength = GetLengthFromStartToCenter(_bestPathIndices, pathCount, centerSegmentIndex, centerSegmentProgress);
            return SamplePathFromStart(_bestPathIndices, pathCount, totalLength * progress, centerSegmentIndex, centerSegmentProgress);
        }

        private float2 SampleCenterToRight(int pathCount, int centerSegmentIndex, float centerSegmentProgress, float progress)
        {
            var totalLength = GetLengthFromCenterToEnd(_bestPathIndices, pathCount, centerSegmentIndex, centerSegmentProgress);

            if (totalLength <= _epsilon)
                return _inflatedHull[_bestPathIndices[pathCount - 1]];

            var targetDistance = totalLength * progress;
            var traveled = 0f;

            var segmentStart = math.lerp(
                _inflatedHull[_bestPathIndices[centerSegmentIndex]],
                _inflatedHull[_bestPathIndices[centerSegmentIndex + 1]],
                centerSegmentProgress);

            var segmentEnd = _inflatedHull[_bestPathIndices[centerSegmentIndex + 1]];
            var firstSegmentLength = math.distance(segmentStart, segmentEnd);

            if (targetDistance <= firstSegmentLength || centerSegmentIndex == pathCount - 2)
                return math.lerp(segmentStart, segmentEnd, SafeInverseLerp(0f, firstSegmentLength, targetDistance));

            traveled += firstSegmentLength;

            for (var segmentIndex = centerSegmentIndex + 1; segmentIndex < pathCount - 1; segmentIndex += 1)
            {
                segmentStart = _inflatedHull[_bestPathIndices[segmentIndex]];
                segmentEnd = _inflatedHull[_bestPathIndices[segmentIndex + 1]];
                var segmentLength = math.distance(segmentStart, segmentEnd);

                if (targetDistance <= traveled + segmentLength || segmentIndex == pathCount - 2)
                    return math.lerp(segmentStart, segmentEnd, SafeInverseLerp(traveled, traveled + segmentLength, targetDistance));

                traveled += segmentLength;
            }

            return _inflatedHull[_bestPathIndices[pathCount - 1]];
        }

        private float2 SamplePathFromStart(int[] pathIndices, int pathCount, float targetDistance, int centerSegmentIndex,
            float centerSegmentProgress)
        {
            if (targetDistance <= _epsilon)
                return _inflatedHull[pathIndices[0]];

            var traveled = 0f;

            for (var segmentIndex = 0; segmentIndex <= centerSegmentIndex; segmentIndex += 1)
            {
                var segmentStart = _inflatedHull[pathIndices[segmentIndex]];
                var segmentEnd = _inflatedHull[pathIndices[segmentIndex + 1]];

                if (segmentIndex == centerSegmentIndex)
                    segmentEnd = math.lerp(segmentStart, segmentEnd, centerSegmentProgress);

                var segmentLength = math.distance(segmentStart, segmentEnd);

                if (targetDistance <= traveled + segmentLength || segmentIndex == centerSegmentIndex)
                    return math.lerp(segmentStart, segmentEnd, SafeInverseLerp(traveled, traveled + segmentLength, targetDistance));

                traveled += segmentLength;
            }

            return math.lerp(
                _inflatedHull[pathIndices[centerSegmentIndex]],
                _inflatedHull[pathIndices[centerSegmentIndex + 1]],
                centerSegmentProgress);
        }

        private float GetPathLengthThroughCenter(int[] pathIndices, int pathCount, int centerSegmentIndex, float centerSegmentProgress)
        {
            return GetLengthFromStartToCenter(pathIndices, pathCount, centerSegmentIndex, centerSegmentProgress)
                   + GetLengthFromCenterToEnd(pathIndices, pathCount, centerSegmentIndex, centerSegmentProgress);
        }

        private float GetLengthFromStartToCenter(int[] pathIndices, int pathCount, int centerSegmentIndex, float centerSegmentProgress)
        {
            var length = 0f;

            for (var segmentIndex = 0; segmentIndex <= centerSegmentIndex; segmentIndex += 1)
            {
                var segmentStart = _inflatedHull[pathIndices[segmentIndex]];
                var segmentEnd = _inflatedHull[pathIndices[segmentIndex + 1]];

                if (segmentIndex == centerSegmentIndex)
                    segmentEnd = math.lerp(segmentStart, segmentEnd, centerSegmentProgress);

                length += math.distance(segmentStart, segmentEnd);
            }

            return length;
        }

        private float GetLengthFromCenterToEnd(int[] pathIndices, int pathCount, int centerSegmentIndex, float centerSegmentProgress)
        {
            var length = 0f;

            var center = math.lerp(
                _inflatedHull[pathIndices[centerSegmentIndex]],
                _inflatedHull[pathIndices[centerSegmentIndex + 1]],
                centerSegmentProgress);

            length += math.distance(center, _inflatedHull[pathIndices[centerSegmentIndex + 1]]);

            for (var segmentIndex = centerSegmentIndex + 1; segmentIndex < pathCount - 1; segmentIndex += 1)
            {
                length += math.distance(_inflatedHull[pathIndices[segmentIndex]], _inflatedHull[pathIndices[segmentIndex + 1]]);
            }

            return length;
        }

        private bool TryFindPointOnPath(float2 point, int[] pathIndices, int pathCount, out int segmentIndex, out float segmentProgress)
        {
            segmentIndex = 0;
            segmentProgress = 0f;

            for (var pathIndex = 0; pathIndex < pathCount - 1; pathIndex += 1)
            {
                var segmentStart = _inflatedHull[pathIndices[pathIndex]];
                var segmentEnd = _inflatedHull[pathIndices[pathIndex + 1]];

                if (!TryGetSegmentProgress(point, segmentStart, segmentEnd, out var progress))
                    continue;

                segmentIndex = pathIndex;
                segmentProgress = progress;
                return true;
            }

            return false;
        }

        private void WritePathIndices(int startIndex, int endIndex, int direction, int hullCount, int[] outputIndices, out int pathCount)
        {
            pathCount = 0;
            var currentIndex = startIndex;

            while (true)
            {
                outputIndices[pathCount] = currentIndex;
                pathCount += 1;

                if (currentIndex == endIndex)
                    return;

                currentIndex = direction > 0 ? NextIndex(currentIndex, hullCount) : PreviousIndex(currentIndex, hullCount);
            }
        }

        private bool TryGetSegmentProgress(float2 point, float2 segmentStart, float2 segmentEnd, out float progress)
        {
            var segment = segmentEnd - segmentStart;
            var lengthSquared = math.lengthsq(segment);

            if (lengthSquared <= _epsilon * _epsilon)
            {
                progress = 0f;
                return math.distancesq(point, segmentStart) <= _epsilon * _epsilon;
            }

            progress = math.clamp(math.dot(point - segmentStart, segment) / lengthSquared, 0f, 1f);
            var closestPoint = math.lerp(segmentStart, segmentEnd, progress);
            return math.distancesq(point, closestPoint) <= _epsilon * _epsilon;
        }

        private bool TryIntersectRaySegment(float2 rayOrigin, float2 rayDirection, float2 segmentStart, float2 segmentEnd, out float distance,
            out float2 point)
        {
            distance = 0f;
            point = default;
            var segment = segmentEnd - segmentStart;
            var denominator = Cross(rayDirection, segment);

            if (math.abs(denominator) <= _epsilon)
                return false;

            var delta = segmentStart - rayOrigin;
            var rayDistance = Cross(delta, segment) / denominator;
            var segmentProgress = Cross(delta, rayDirection) / denominator;

            if (rayDistance < -_epsilon || segmentProgress < -_epsilon || segmentProgress > 1f + _epsilon)
                return false;

            distance = rayDistance;
            point = rayOrigin + (rayDirection * rayDistance);
            return true;
        }

        private bool TryIntersectLines(float2 firstStart, float2 firstEnd, float2 secondStart, float2 secondEnd, out float2 point)
        {
            point = default;
            var firstDirection = firstEnd - firstStart;
            var secondDirection = secondEnd - secondStart;
            var denominator = Cross(firstDirection, secondDirection);

            if (math.abs(denominator) <= _epsilon)
                return false;

            var progress = Cross(secondStart - firstStart, secondDirection) / denominator;
            point = firstStart + (firstDirection * progress);
            return IsFinite(point);
        }

        private bool TryIntersectSegmentsInterior(float2 firstStart, float2 firstEnd, float2 secondStart, float2 secondEnd)
        {
            var firstDirection = firstEnd - firstStart;
            var secondDirection = secondEnd - secondStart;
            var denominator = Cross(firstDirection, secondDirection);

            if (math.abs(denominator) <= _epsilon)
                return TryIntersectCollinearSegmentsInterior(firstStart, firstEnd, secondStart, secondEnd);

            var delta = secondStart - firstStart;
            var firstProgress = Cross(delta, secondDirection) / denominator;
            var secondProgress = Cross(delta, firstDirection) / denominator;

            return firstProgress > _epsilon
                   && firstProgress < 1f - _epsilon
                   && secondProgress > _epsilon
                   && secondProgress < 1f - _epsilon;
        }

        private bool TryIntersectCollinearSegmentsInterior(float2 firstStart, float2 firstEnd, float2 secondStart, float2 secondEnd)
        {
            var firstDirection = firstEnd - firstStart;
            var firstLengthSquared = math.lengthsq(firstDirection);

            if (firstLengthSquared <= _epsilon * _epsilon)
                return false;

            if (math.abs(Cross(secondStart - firstStart, firstDirection)) > _epsilon
                || math.abs(Cross(secondEnd - firstStart, firstDirection)) > _epsilon)
            {
                return false;
            }

            var secondStartProgress = math.dot(secondStart - firstStart, firstDirection) / firstLengthSquared;
            var secondEndProgress = math.dot(secondEnd - firstStart, firstDirection) / firstLengthSquared;
            var overlapStart = math.max(math.min(secondStartProgress, secondEndProgress), _epsilon);
            var overlapEnd = math.min(math.max(secondStartProgress, secondEndProgress), 1f - _epsilon);

            return overlapStart <= overlapEnd;
        }

        private bool IsPointStrictlyInsideConvexHull(float2 point, int hullCount)
        {
            for (var hullIndex = 0; hullIndex < hullCount; hullIndex += 1)
            {
                var edgeStart = _inflatedHull[hullIndex];
                var edgeEnd = _inflatedHull[NextIndex(hullIndex, hullCount)];

                if (Cross(edgeEnd - edgeStart, point - edgeStart) <= _epsilon)
                    return false;
            }

            return true;
        }

        private float2 GetOutwardNormal(float2 edge)
        {
            return math.normalizesafe(new float2(edge.y, -edge.x));
        }

        private float2 GetCentroid(float2[] points, int pointCount)
        {
            var sum = float2.zero;

            for (var pointIndex = 0; pointIndex < pointCount; pointIndex += 1)
            {
                sum += points[pointIndex];
            }

            return sum / pointCount;
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

        private float SafeInverseLerp(float start, float end, float value)
        {
            if (math.abs(end - start) <= _epsilon)
                return 1f;

            return math.clamp((value - start) / (end - start), 0f, 1f);
        }

        private int NextIndex(int index, int count)
        {
            return index + 1 < count ? index + 1 : 0;
        }

        private int PreviousIndex(int index, int count)
        {
            return index > 0 ? index - 1 : count - 1;
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

                if (xComparison != 0)
                    return xComparison;

                return left.y.CompareTo(right.y);
            }
        }
    }
}
