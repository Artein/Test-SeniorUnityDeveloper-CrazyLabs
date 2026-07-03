using System.Text;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using static GameplaySceneBandShapePlayModeTestUtils;

// ReSharper disable once CheckNamespace
internal static class GameplaySceneDirectMaxPullBandShapeAssertions
{
    public static void AssertDirectMaxPullBandUsesCenteredTautWrap(
        GameplaySceneBandShapePlayModeTestContext context,
        Vector3[] bandPositions,
        string phase,
        string diagnostics)
    {
        AssertMaximumPullBandUsesTautClearVisibleWrap(context, bandPositions, phase, diagnostics);
    }

    public static void AssertMaximumPullBandUsesTautClearVisibleWrap(
        GameplaySceneBandShapePlayModeTestContext context,
        Vector3[] bandPositions,
        string phase,
        string diagnostics)
    {
        AssertBandShapeDoesNotMatchRawTwoSpan(bandPositions, context.Geometry, phase, diagnostics);
        AssertMiddleWrapTracksBandCenter(bandPositions, context, phase, diagnostics);
        AssertBandPathOffsetsAreMonotonic(bandPositions, context, phase, diagnostics);
        AssertBandPathHasNoSharpFolds(bandPositions, context.Geometry, phase, diagnostics);
        AssertBandSamplesStayOutsideCollider(bandPositions, context, phase, diagnostics);
        AssertRenderedBandWidthVisibleFromCamera(bandPositions, context, phase, diagnostics);
    }

    public static void AssertBandShapeIsSymmetricAroundBandCenter(
        Vector3[] bandPositions,
        GameplaySceneBandShapePlayModeTestContext context,
        string phase,
        string diagnostics)
    {
        var middleIndex = (bandPositions.Length - 1) / 2;
        var lastIndex = bandPositions.Length - 1;
        var bandCenterOffset = ProjectOffset(context.BandCenter.transform.position, context.Geometry);
        var renderedRadius = GetMaximumRenderedBandRadius(context.BandLineRenderer);
        var lateralTolerance = Mathf.Max(0.12f, renderedRadius + 0.06f);
        const float depthTolerance = 0.12f;

        for (var pointIndex = 0; pointIndex <= middleIndex; pointIndex += 1)
        {
            var mirroredIndex = lastIndex - pointIndex;
            var leftOffset = ProjectOffset(bandPositions[pointIndex], context.Geometry) - bandCenterOffset;
            var rightOffset = ProjectOffset(bandPositions[mirroredIndex], context.Geometry) - bandCenterOffset;
            var leftDepth = ProjectDepth(bandPositions[pointIndex], context.Geometry);
            var rightDepth = ProjectDepth(bandPositions[mirroredIndex], context.Geometry);

            Assert.That(
                leftOffset,
                Is.EqualTo(-rightOffset).Within(lateralTolerance),
                $"{phase} Band point {pointIndex} should mirror point {mirroredIndex} around the centered held target.\n{diagnostics}");

            Assert.That(
                leftDepth,
                Is.EqualTo(rightDepth).Within(depthTolerance),
                $"{phase} Band point {pointIndex} depth should mirror point {mirroredIndex} depth.\n{diagnostics}");
        }
    }

    public static void AssertBandShapeStableFromPreviousFrame(
        GameplaySceneBandShapePlayModeTestContext context,
        Vector3[] previousFrameBandPositions,
        Vector3[] currentFrameBandPositions,
        float requestedPullOffsetDelta,
        string phase,
        string diagnostics)
    {
        Assert.That(
            currentFrameBandPositions,
            Has.Length.EqualTo(previousFrameBandPositions.Length),
            $"{phase} Band Shape point count should be stable across repeated direct max-pull frames.\n{diagnostics}");

        var renderedBandRadius = GetMaximumRenderedBandRadius(context.BandLineRenderer);

        if (Mathf.Abs(requestedPullOffsetDelta) <= 0.0001f)
        {
            var maximumExpectedPointMovement = Mathf.Max(0.03f, renderedBandRadius);

            for (var pointIndex = 0; pointIndex < currentFrameBandPositions.Length; pointIndex += 1)
            {
                Assert.That(
                    Vector3.Distance(currentFrameBandPositions[pointIndex], previousFrameBandPositions[pointIndex]),
                    Is.LessThanOrEqualTo(maximumExpectedPointMovement),
                    $"{phase} Band point {pointIndex} should not jump between repeated direct max-pull frames.\n{diagnostics}");
            }

            return;
        }

        var maximumExpectedPathDrift = Mathf.Max(0.03f, Mathf.Abs(requestedPullOffsetDelta) * 3f, renderedBandRadius);
        var previousToCurrentPathDrift = GetMaximumDistanceToPolyline(previousFrameBandPositions, currentFrameBandPositions);
        var currentToPreviousPathDrift = GetMaximumDistanceToPolyline(currentFrameBandPositions, previousFrameBandPositions);
        var maximumPathDrift = Mathf.Max(previousToCurrentPathDrift, currentToPreviousPathDrift);

        Assert.That(
            maximumPathDrift,
            Is.LessThanOrEqualTo(maximumExpectedPathDrift),
            $"{phase} Band path should remain continuous between near-direct max-pull jitter frames.\n{diagnostics}");
    }

    public static void AssertRawProjectedPullReachedMaximumDepth(
        GameplaySceneBandShapePlayModeTestContext context,
        Vector3 rawProjectedPullPoint,
        string phase,
        string diagnostics)
    {
        Assert.That(
            ProjectDepth(rawProjectedPullPoint, context.Geometry),
            Is.GreaterThanOrEqualTo(context.SlingshotConfig.MaximumPullDistance - 0.02f),
            $"{phase} raw screen-projected input should reach maximum pull depth before controller clamping.\n{diagnostics}");
    }

    public static string CreateFrameDiagnostics(
        GameplaySceneBandShapePlayModeTestContext context,
        Vector3[] bandPositions,
        int frameIndex,
        float requestedPullOffset,
        string screenshotPath)
    {
        return CreateDiagnostics(
            context,
            bandPositions,
            frameIndex,
            $"requestedPullOffset={requestedPullOffset:0.###} ",
            screenshotPath);
    }

    public static string CreateScreenDragFrameDiagnostics(
        GameplaySceneBandShapePlayModeTestContext context,
        Vector3[] bandPositions,
        int frameIndex,
        Vector2 screenPosition,
        Vector3 rawProjectedPullPoint,
        string screenshotPath)
    {
        return CreateDiagnostics(
            context,
            bandPositions,
            frameIndex,
            $"screen=({screenPosition.x:0.#}, {screenPosition.y:0.#}) rawProjectedPull={DescribePoint(rawProjectedPullPoint, context.Geometry)} ",
            screenshotPath);
    }

    private static void AssertBandShapeDoesNotMatchRawTwoSpan(
        Vector3[] bandPositions,
        SlingshotGeometrySnapshot geometry,
        string phase,
        string diagnostics)
    {
        var middleIndex = (bandPositions.Length - 1) / 2;
        var centerPoint = bandPositions[middleIndex];
        var maximumDistanceFromRawTwoSpan = GetMaximumDistanceFromRawTwoSpan(bandPositions, geometry, centerPoint);

        Assert.That(
            maximumDistanceFromRawTwoSpan,
            Is.GreaterThan(0.01f),
            $"{phase} Band Shape should use taut target wrap instead of raw two-span geometry.\n{diagnostics}");
    }

    private static float GetMaximumDistanceToPolyline(Vector3[] sourcePoints, Vector3[] polylinePoints)
    {
        var maximumDistance = 0f;

        for (var pointIndex = 0; pointIndex < sourcePoints.Length; pointIndex += 1)
        {
            maximumDistance = Mathf.Max(maximumDistance, GetDistanceToPolyline(sourcePoints[pointIndex], polylinePoints));
        }

        return maximumDistance;
    }

    private static float GetDistanceToPolyline(Vector3 point, Vector3[] polylinePoints)
    {
        var minimumDistanceSquared = float.PositiveInfinity;

        for (var pointIndex = 0; pointIndex < polylinePoints.Length - 1; pointIndex += 1)
        {
            minimumDistanceSquared = Mathf.Min(
                minimumDistanceSquared,
                GetDistanceSquaredToSegment(point, polylinePoints[pointIndex], polylinePoints[pointIndex + 1]));
        }

        return Mathf.Sqrt(minimumDistanceSquared);
    }

    private static float GetDistanceSquaredToSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
    {
        var segment = segmentEnd - segmentStart;
        var segmentLengthSquared = segment.sqrMagnitude;

        if (segmentLengthSquared <= 0.000001f)
            return (point - segmentStart).sqrMagnitude;

        var progress = Mathf.Clamp01(Vector3.Dot(point - segmentStart, segment) / segmentLengthSquared);
        var closestPoint = segmentStart + (segment * progress);
        return (point - closestPoint).sqrMagnitude;
    }

    private static void AssertMiddleWrapTracksBandCenter(
        Vector3[] bandPositions,
        GameplaySceneBandShapePlayModeTestContext context,
        string phase,
        string diagnostics)
    {
        var middleWrapPoint = bandPositions[(bandPositions.Length - 1) / 2];
        var middleWrapOffset = ProjectOffset(middleWrapPoint, context.Geometry);
        var middleWrapDepth = ProjectDepth(middleWrapPoint, context.Geometry);
        var bandCenterOffset = ProjectOffset(context.BandCenter.transform.position, context.Geometry);
        var directPullSideContactDepth = GetDirectPullSideRenderedContactDepth(context);
        var lateralTolerance = GetMaximumRenderedBandRadius(context.BandLineRenderer) + 0.04f;

        var depthTolerance = Mathf.Max(0.15f,
            GetMaximumRenderedBandRadius(context.BandLineRenderer) + context.SlingshotConfig.BandContactPadding + 0.04f);

        Assert.That(
            Mathf.Abs(middleWrapOffset - bandCenterOffset),
            Is.LessThanOrEqualTo(lateralTolerance),
            $"{phase} middle wrap should stay laterally aligned with the held Band Center.\n{diagnostics}");

        Assert.That(
            Mathf.Abs(middleWrapDepth - directPullSideContactDepth),
            Is.LessThanOrEqualTo(depthTolerance),
            $"{phase} middle wrap should stay on the direct-pull side near the rendered contact depth.\n{diagnostics}");
    }

    private static float GetDirectPullSideRenderedContactDepth(GameplaySceneBandShapePlayModeTestContext context)
    {
        var renderedClearance = context.SlingshotConfig.BandContactPadding + GetMaximumRenderedBandRadius(context.BandLineRenderer);

        return GetMaximumProjectedBoundsDepth(context.TargetCollider.bounds, context.Geometry) + renderedClearance;
    }

    private static float GetMaximumProjectedBoundsDepth(Bounds bounds, SlingshotGeometrySnapshot geometry)
    {
        var maximumDepth = float.NegativeInfinity;

        for (var xSign = -1; xSign <= 1; xSign += 2)
        {
            for (var ySign = -1; ySign <= 1; ySign += 2)
            {
                for (var zSign = -1; zSign <= 1; zSign += 2)
                {
                    var corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(xSign, ySign, zSign));
                    maximumDepth = Mathf.Max(maximumDepth, ProjectDepth(corner, geometry));
                }
            }
        }

        return maximumDepth;
    }

    private static void AssertBandPathOffsetsAreMonotonic(
        Vector3[] bandPositions,
        GameplaySceneBandShapePlayModeTestContext context,
        string phase,
        string diagnostics)
    {
        var tolerance = GetMaximumRenderedBandRadius(context.BandLineRenderer) + 0.01f;
        var previousOffset = ProjectOffset(bandPositions[0], context.Geometry);

        for (var pointIndex = 1; pointIndex < bandPositions.Length; pointIndex += 1)
        {
            var offset = ProjectOffset(bandPositions[pointIndex], context.Geometry);

            Assert.That(
                offset,
                Is.GreaterThanOrEqualTo(previousOffset - tolerance),
                $"{phase} Band path should not laterally backtrack across the centered target.\n{diagnostics}");

            previousOffset = Mathf.Max(previousOffset, offset);
        }
    }

    private static void AssertBandPathHasNoSharpFolds(
        Vector3[] bandPositions,
        SlingshotGeometrySnapshot geometry,
        string phase,
        string diagnostics)
    {
        const float minimumAdjacentDirectionDot = 0.2f;
        const float minimumSegmentLengthSquared = 0.000001f;

        for (var pointIndex = 1; pointIndex < bandPositions.Length - 1; pointIndex += 1)
        {
            var previousSegment = ProjectToPullPlane(bandPositions[pointIndex] - bandPositions[pointIndex - 1], geometry);
            var nextSegment = ProjectToPullPlane(bandPositions[pointIndex + 1] - bandPositions[pointIndex], geometry);

            if (previousSegment.sqrMagnitude <= minimumSegmentLengthSquared
                || nextSegment.sqrMagnitude <= minimumSegmentLengthSquared)
            {
                continue;
            }

            var directionDot = Vector2.Dot(previousSegment.normalized, nextSegment.normalized);

            Assert.That(
                directionDot,
                Is.GreaterThanOrEqualTo(minimumAdjacentDirectionDot),
                $"{phase} Band path should not form a sharp inward fold at point {pointIndex}.\n{diagnostics}");
        }
    }

    private static void AssertBandSamplesStayOutsideCollider(
        Vector3[] bandPositions,
        GameplaySceneBandShapePlayModeTestContext context,
        string phase,
        string diagnostics)
    {
        const float samplingSafetyMargin = 0.002f;
        var requiredClearance = GetMaximumRenderedBandRadius(context.BandLineRenderer) + samplingSafetyMargin;

        for (var pointIndex = 0; pointIndex < bandPositions.Length - 1; pointIndex += 1)
        {
            for (var sampleIndex = 0; sampleIndex <= 24; sampleIndex += 1)
            {
                var samplePoint = Vector3.Lerp(bandPositions[pointIndex], bandPositions[pointIndex + 1], sampleIndex / 24f);
                var closestPoint = context.TargetCollider.ClosestPoint(samplePoint);
                var surfaceDistance = Vector3.Distance(samplePoint, closestPoint);

                Assert.That(
                    surfaceDistance,
                    Is.GreaterThan(requiredClearance),
                    $"{phase} Band rendered radius intersects the Launch Target Collider at segment {pointIndex} sample {sampleIndex}.\n{diagnostics}");
            }
        }
    }

    private static void AssertRenderedBandWidthVisibleFromCamera(
        Vector3[] bandPositions,
        GameplaySceneBandShapePlayModeTestContext context,
        string phase,
        string diagnostics)
    {
        var cameraPosition = context.InputCamera.transform.position;
        var visibleBandRadius = GetMaximumRenderedBandRadius(context.BandLineRenderer);

        for (var pointIndex = 0; pointIndex < bandPositions.Length - 1; pointIndex += 1)
        {
            for (var sampleIndex = 0; sampleIndex <= 24; sampleIndex += 1)
            {
                var samplePoint = Vector3.Lerp(bandPositions[pointIndex], bandPositions[pointIndex + 1], sampleIndex / 24f);

                var widthAxis = GetCameraFacingBandWidthAxis(
                    bandPositions[pointIndex],
                    bandPositions[pointIndex + 1],
                    cameraPosition,
                    samplePoint);

                AssertBandRenderPointVisibleFromCamera(samplePoint, context, phase, pointIndex, sampleIndex, diagnostics);

                AssertBandRenderPointVisibleFromCamera(
                    samplePoint + (widthAxis * visibleBandRadius),
                    context,
                    phase,
                    pointIndex,
                    sampleIndex,
                    diagnostics);

                AssertBandRenderPointVisibleFromCamera(
                    samplePoint - (widthAxis * visibleBandRadius),
                    context,
                    phase,
                    pointIndex,
                    sampleIndex,
                    diagnostics);
            }
        }
    }

    private static void AssertBandRenderPointVisibleFromCamera(
        Vector3 renderPoint,
        GameplaySceneBandShapePlayModeTestContext context,
        string phase,
        int pointIndex,
        int sampleIndex,
        string diagnostics)
    {
        var cameraPosition = context.InputCamera.transform.position;
        var rayDirection = renderPoint - cameraPosition;
        var sampleDistance = rayDirection.magnitude;

        if (sampleDistance <= 0.0001f)
            return;

        var ray = new Ray(cameraPosition, rayDirection / sampleDistance);

        Assert.That(
            context.TargetCollider.Raycast(ray, out _, sampleDistance - 0.002f),
            Is.False,
            $"{phase} Band segment {pointIndex} sample {sampleIndex} rendered width is hidden behind the Launch Target from the gameplay camera.\n{diagnostics}");
    }

    private static Vector3 GetCameraFacingBandWidthAxis(
        Vector3 segmentStart,
        Vector3 segmentEnd,
        Vector3 cameraPosition,
        Vector3 samplePoint)
    {
        const float minimumMagnitude = 0.000001f;
        var segmentDirection = segmentEnd - segmentStart;
        var viewDirection = samplePoint - cameraPosition;

        if (segmentDirection.sqrMagnitude <= minimumMagnitude || viewDirection.sqrMagnitude <= minimumMagnitude)
            return Vector3.up;

        var widthAxis = Vector3.Cross(viewDirection.normalized, segmentDirection.normalized);

        return widthAxis.sqrMagnitude > minimumMagnitude ? widthAxis.normalized : Vector3.up;
    }

    private static string CreateDiagnostics(
        GameplaySceneBandShapePlayModeTestContext context,
        Vector3[] bandPositions,
        int frameIndex,
        string frameDetails,
        string screenshotPath)
    {
        var message = new StringBuilder();
        message.Append("[SlingshotMaxDirectPull] ");
        message.Append($"frame={frameIndex} ");
        message.Append(frameDetails);
        message.Append($"screenshot={screenshotPath} ");
        message.Append($"BandCenter={DescribePoint(context.BandCenter.transform.position, context.Geometry)} ");
        message.Append($"ColliderCenter={DescribePoint(context.TargetCollider.bounds.center, context.Geometry)}");

        for (var pointIndex = 0; pointIndex < bandPositions.Length; pointIndex += 1)
        {
            message.AppendLine();
            message.Append($"[SlingshotMaxDirectPull] point[{pointIndex}]={DescribePoint(bandPositions[pointIndex], context.Geometry)}");
        }

        return message.ToString();
    }

    private static float GetMaximumDistanceFromRawTwoSpan(Vector3[] bandPositions, SlingshotGeometrySnapshot geometry, Vector3 centerPoint)
    {
        var middleIndex = (bandPositions.Length - 1) / 2;
        var lastIndex = bandPositions.Length - 1;
        var maximumDistance = 0f;

        for (var pointIndex = 0; pointIndex <= middleIndex; pointIndex += 1)
        {
            var progress = middleIndex <= 0 ? 1f : (float)pointIndex / middleIndex;
            var expectedPoint = Vector3.Lerp(geometry.LeftAnchorPosition, centerPoint, progress);
            maximumDistance = Mathf.Max(maximumDistance, Vector3.Distance(bandPositions[pointIndex], expectedPoint));
        }

        for (var pointIndex = middleIndex + 1; pointIndex <= lastIndex; pointIndex += 1)
        {
            var progress = (float)(pointIndex - middleIndex) / (lastIndex - middleIndex);
            var expectedPoint = Vector3.Lerp(centerPoint, geometry.RightAnchorPosition, progress);
            maximumDistance = Mathf.Max(maximumDistance, Vector3.Distance(bandPositions[pointIndex], expectedPoint));
        }

        return maximumDistance;
    }
}
