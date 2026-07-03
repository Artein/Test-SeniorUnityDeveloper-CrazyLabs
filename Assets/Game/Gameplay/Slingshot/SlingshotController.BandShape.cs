using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public sealed partial class SlingshotController
    {
        private const float BandTargetClearanceProbeMargin = 0.02f;
        private const int SimpleBandVisualStandOffSearchStepCount = 8;
        private const float SimpleBandVisualStandOffComparisonTolerance = 0.0001f;

        private void InitializeBandShapeBuffers()
        {
            var pointCount = _bandShapeProvider.BandShapePointCount;
            _firstActiveBandShapeBuffer = new Vector3[pointCount];
            _secondActiveBandShapeBuffer = new Vector3[pointCount];
            _currentActiveBandShapeBuffer = _firstActiveBandShapeBuffer;
            _inactiveActiveBandShapeBuffer = _secondActiveBandShapeBuffer;
            _restBandShapeBuffer = new Vector3[pointCount];
            _visibilityBandShapeBuffer = new Vector3[pointCount];
            FillBandShapeBuffer(_restBandShapeBuffer, _geometry.RestPoint);
        }

        private bool TryUpdateActiveBandShape(Vector3 pullPoint)
        {
            if (ShouldUseSimpleBandShape(pullPoint) && TryUpdateClearSimpleBandShape(pullPoint))
                return true;

            return TryUpdateClearTautBandShape(pullPoint);
        }

        private bool TryUpdateClearTautBandShape(Vector3 pullPoint)
        {
            if (!TryWriteBandShape(pullPoint, _inactiveActiveBandShapeBuffer))
                return false;

            var candidateBandShape = new SlingshotBandShape(_inactiveActiveBandShapeBuffer, true);

            if (!IsBandShapeClear(candidateBandShape, pullPoint))
                return false;

            (_currentActiveBandShapeBuffer, _inactiveActiveBandShapeBuffer) = (_inactiveActiveBandShapeBuffer, _currentActiveBandShapeBuffer);
            _hasLastValidActiveBandShape = true;
            return true;
        }

        private bool TryWriteBandShape(Vector3 pullPoint, Vector3[] buffer)
        {
            var shapeQuery = CreateBandShapeQuery(pullPoint);
            var renderedBandRadius = GetRenderedBandRadius();
            var solved = _renderedBandShapeProvider.TryCreateRenderedBandShape(shapeQuery, renderedBandRadius, buffer, out var pointCount);
            return solved && pointCount == buffer.Length;
        }

        private float GetRenderedBandRadius()
        {
            return _view.VisibleBandRadius + BandTargetClearanceProbeMargin;
        }

        private float GetBandTargetClearanceRadius()
        {
            return _view.VisibleBandRadius + _config.BandContactPadding;
        }

        private float GetBandTargetClearanceProbeRadius()
        {
            return GetBandTargetClearanceRadius() + BandTargetClearanceProbeMargin;
        }

        private float GetBandShapeClearanceProbeRadius()
        {
            return _view.VisibleBandRadius + BandTargetClearanceProbeMargin;
        }

        private float ClampPullOffsetToVisibleBandShape(float pullDistance, float pullOffset)
        {
            if (_bandVisibilityRayProvider is null || _launchTargetBandOcclusionSource is null || Mathf.Abs(pullOffset) <= 0.0001f ||
                IsBandShapeVisibleAtPullOffset(pullDistance, pullOffset))
                return pullOffset;

            if (!IsBandShapeVisibleAtPullOffset(pullDistance, 0f))
            {
                PoseHeldTargetForPullSolve(GetPullPoint(pullDistance, pullOffset));
                return pullOffset;
            }

            var pullOffsetSign = Mathf.Sign(pullOffset);
            var minimumVisibleMagnitude = 0f;
            var maximumHiddenMagnitude = Mathf.Abs(pullOffset);

            for (var iteration = 0; iteration < 8; iteration += 1)
            {
                var candidateMagnitude = (minimumVisibleMagnitude + maximumHiddenMagnitude) * 0.5f;
                var candidatePullOffset = pullOffsetSign * candidateMagnitude;

                if (IsBandShapeVisibleAtPullOffset(pullDistance, candidatePullOffset))
                {
                    minimumVisibleMagnitude = candidateMagnitude;
                    continue;
                }

                maximumHiddenMagnitude = candidateMagnitude;
            }

            var visiblePullOffset = pullOffsetSign * minimumVisibleMagnitude;
            PoseHeldTargetForPullSolve(GetPullPoint(pullDistance, visiblePullOffset));
            return visiblePullOffset;
        }

        private bool IsBandShapeVisibleAtPullOffset(float pullDistance, float pullOffset)
        {
            var pullPoint = GetPullPoint(pullDistance, pullOffset);
            PoseHeldTargetForPullSolve(pullPoint);

            if (ShouldUseSimpleBandShape(pullPoint))
            {
                FillSimpleBandShapeBuffer(_visibilityBandShapeBuffer, pullPoint);
            }
            else if (!TryWriteBandShape(pullPoint, _visibilityBandShapeBuffer))
            {
                return false;
            }

            return IsBandShapeVisibleFromCamera(_visibilityBandShapeBuffer);
        }

        private bool IsBandShapeVisibleFromCamera(Vector3[] bandShapePoints)
        {
            const int segmentSampleCount = 24;
            const float raycastPadding = 0.002f;
            var visibleBandRadius = _view.VisibleBandRadius;

            for (var pointIndex = 0; pointIndex < bandShapePoints.Length - 1; pointIndex += 1)
            {
                var segmentStart = bandShapePoints[pointIndex];
                var segmentEnd = bandShapePoints[pointIndex + 1];

                for (var sampleIndex = 0; sampleIndex <= segmentSampleCount; sampleIndex += 1)
                {
                    var progress = (float)sampleIndex / segmentSampleCount;
                    var samplePoint = Vector3.Lerp(segmentStart, segmentEnd, progress);

                    if (!_bandVisibilityRayProvider.TryCreateRayToWorldPoint(samplePoint, out var centerRay, out var centerDistance))
                        return false;

                    if (!IsBandVisibilityRayClear(centerRay, centerDistance, raycastPadding))
                        return false;

                    if (visibleBandRadius <= 0.0001f)
                        continue;

                    var widthAxis = GetCameraFacingBandWidthAxis(segmentStart, segmentEnd, centerRay.direction);

                    if (!IsBandRenderPointVisibleFromCamera(samplePoint + (widthAxis * visibleBandRadius), raycastPadding)
                        || !IsBandRenderPointVisibleFromCamera(samplePoint - (widthAxis * visibleBandRadius), raycastPadding))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsBandRenderPointVisibleFromCamera(Vector3 renderPoint, float raycastPadding)
        {
            return _bandVisibilityRayProvider.TryCreateRayToWorldPoint(renderPoint, out var ray, out var distance)
                   && IsBandVisibilityRayClear(ray, distance, raycastPadding);
        }

        private bool IsBandVisibilityRayClear(Ray ray, float distance, float raycastPadding)
        {
            var occlusionDistance = distance - raycastPadding;
            return occlusionDistance <= 0f || !_launchTargetBandOcclusionSource.IsBandPointHiddenFrom(ray, occlusionDistance);
        }

        private Vector3 GetCameraFacingBandWidthAxis(Vector3 segmentStart, Vector3 segmentEnd, Vector3 viewDirection)
        {
            const float minimumMagnitude = 0.000001f;
            var segmentDirection = segmentEnd - segmentStart;

            if (segmentDirection.sqrMagnitude <= minimumMagnitude || viewDirection.sqrMagnitude <= minimumMagnitude)
                return _geometry.LaunchFrameUp;

            var widthAxis = Vector3.Cross(viewDirection.normalized, segmentDirection.normalized);

            return widthAxis.sqrMagnitude > minimumMagnitude ? widthAxis.normalized : _geometry.LaunchFrameUp;
        }

        private SlingshotBandShape CreateReleaseRecoilBandShape(float progress)
        {
            var recoilPullPoint = GetReleaseRecoilPullPoint(progress, out var wasDepthClamped);
            TryUpdateReleaseRecoilBandShape(recoilPullPoint, wasDepthClamped);

            return CreateCurrentActiveBandShape();
        }

        private bool TryUpdateReleaseRecoilBandShape(Vector3 recoilPullPoint, bool wasDepthClamped)
        {
            if (wasDepthClamped && TryUpdateClearSimpleBandShape(recoilPullPoint))
                return true;

            if (TryUpdateClearTautBandShape(recoilPullPoint))
                return true;

            if (!wasDepthClamped && TryUpdateClearSimpleBandShape(recoilPullPoint))
                return true;

            // Keep the last valid active shape if recoil cannot solve a clear intermediate shape.
            return false;
        }

        private Vector3 GetReleaseRecoilPullPoint(float progress, out bool wasDepthClamped)
        {
            wasDepthClamped = false;
            var recoilPullPoint = Vector3.Lerp(_pendingLaunchRequest.FinalPullPoint, _geometry.RestPoint, progress);

            if (!_bandShapeDepthProvider.TryGetSilhouetteDepthSpan(
                    CreateBandShapeQuery(_geometry.RestPoint),
                    out _,
                    out var maximumDepth))
            {
                return recoilPullPoint;
            }

            var currentDepth = GetPullDistance(recoilPullPoint);
            var minimumClearDepth = Mathf.Max(0f, maximumDepth + GetBandShapeClearanceProbeRadius());

            if (currentDepth >= minimumClearDepth)
                return recoilPullPoint;

            wasDepthClamped = true;
            return recoilPullPoint - (_geometry.LaunchFrameForward * (minimumClearDepth - currentDepth));
        }

        private bool TryUpdateClearSimpleBandShape(Vector3 pullPoint)
        {
            var visualCenterPoint = GetSimpleBandVisualCenterPoint(pullPoint);

            if (TryUpdateClearSimpleBandShape(pullPoint, visualCenterPoint))
                return true;

            return TryUpdateAdaptiveClearSimpleBandShape(pullPoint, visualCenterPoint);
        }

        private bool TryUpdateAdaptiveClearSimpleBandShape(Vector3 pullPoint, Vector3 initialVisualCenterPoint)
        {
            if (!_bandShapeDepthProvider.TryGetSilhouetteDepthSpan(
                    CreateBandShapeQuery(pullPoint),
                    out _,
                    out var maximumDepth)
                || maximumDepth <= 0f)
            {
                return false;
            }

            var initialDepth = GetPullDistance(initialVisualCenterPoint);

            var minimumSearchDepth = Mathf.Max(
                initialDepth + SimpleBandVisualStandOffComparisonTolerance,
                maximumDepth + GetBandShapeClearanceProbeRadius());

            var maximumExtraStandOff = Mathf.Max(
                _config.BandContactPadding * 4f,
                GetBandShapeClearanceProbeRadius() * 4f);
            var maximumSearchDepth = initialDepth + maximumExtraStandOff;

            if (minimumSearchDepth > maximumSearchDepth)
                maximumSearchDepth = minimumSearchDepth;

            for (var stepIndex = 0; stepIndex < SimpleBandVisualStandOffSearchStepCount; stepIndex += 1)
            {
                var progress = SimpleBandVisualStandOffSearchStepCount <= 1
                    ? 1f
                    : (float)stepIndex / (SimpleBandVisualStandOffSearchStepCount - 1);
                var candidateDepth = Mathf.Lerp(minimumSearchDepth, maximumSearchDepth, progress);
                var visualCenterPoint = GetSimpleBandVisualCenterPointAtDepth(pullPoint, candidateDepth);

                if (TryUpdateClearSimpleBandShape(pullPoint, visualCenterPoint))
                    return true;
            }

            return false;
        }

        private bool TryUpdateClearSimpleBandShape(Vector3 pullPoint, Vector3 visualCenterPoint)
        {
            FillBandShapeBuffer(_inactiveActiveBandShapeBuffer, visualCenterPoint);
            var candidateBandShape = new SlingshotBandShape(_inactiveActiveBandShapeBuffer, true);

            if (!IsBandShapeClear(candidateBandShape, pullPoint))
                return false;

            (_currentActiveBandShapeBuffer, _inactiveActiveBandShapeBuffer) = (_inactiveActiveBandShapeBuffer, _currentActiveBandShapeBuffer);
            _hasLastValidActiveBandShape = true;
            return true;
        }

        private bool IsDetachedRestBandShapeClear()
        {
            return IsBandShapeClear(CreateDetachedRestBandShape(), _geometry.RestPoint);
        }

        private bool IsBandShapeClear(SlingshotBandShape bandShape, Vector3 pullPoint)
        {
            return _bandShapeProvider.TryCheckBandShapeClearance(
                       CreateBandShapeQuery(pullPoint),
                       bandShape.Points,
                       GetBandShapeClearanceProbeRadius(),
                       out var isClear)
                   && isClear;
        }

        private SlingshotBandShape CreateDetachedRestBandShape()
        {
            FillBandShapeBuffer(_restBandShapeBuffer, _geometry.RestPoint);
            return new SlingshotBandShape(_restBandShapeBuffer);
        }

        private SlingshotBandShape CreateHeldRestBandShape()
        {
            FillBandShapeBuffer(_restBandShapeBuffer, _geometry.RestPoint);
            return new SlingshotBandShape(_restBandShapeBuffer);
        }

        private SlingshotBandShape CreateCurrentActiveBandShape()
        {
            return new SlingshotBandShape(_currentActiveBandShapeBuffer);
        }

        private void FillSimpleBandShapeBuffer(Vector3[] buffer, Vector3 centerPoint)
        {
            FillBandShapeBuffer(buffer, GetSimpleBandVisualCenterPoint(centerPoint));
        }

        private Vector3 GetSimpleBandVisualCenterPoint(Vector3 centerPoint)
        {
            return centerPoint - (_geometry.LaunchFrameForward * _config.BandContactPadding);
        }

        private Vector3 GetSimpleBandVisualCenterPointAtDepth(Vector3 pullPoint, float visualCenterDepth)
        {
            var pullOffset = Vector3.Dot(pullPoint - _geometry.RestPoint, _geometry.LaunchFrameRight);
            var safeDepth = Mathf.Max(0f, visualCenterDepth);

            return _geometry.RestPoint
                   + (_geometry.LaunchFrameRight * pullOffset)
                   - (_geometry.LaunchFrameForward * safeDepth);
        }

        private void FillBandShapeBuffer(Vector3[] buffer, Vector3 centerPoint)
        {
            var middleIndex = (buffer.Length - 1) / 2;
            var lastIndex = buffer.Length - 1;

            for (var pointIndex = 0; pointIndex <= middleIndex; pointIndex += 1)
            {
                var progress = middleIndex <= 0 ? 1f : (float)pointIndex / middleIndex;
                buffer[pointIndex] = Vector3.Lerp(_geometry.LeftAnchorPosition, centerPoint, progress);
            }

            for (var pointIndex = middleIndex + 1; pointIndex <= lastIndex; pointIndex += 1)
            {
                var progress = (float)(pointIndex - middleIndex) / (lastIndex - middleIndex);
                buffer[pointIndex] = Vector3.Lerp(centerPoint, _geometry.RightAnchorPosition, progress);
            }
        }

        private bool ShouldUseSimpleBandShape(Vector3 pullPoint)
        {
            const float tolerance = 0.0001f;
            return GetPullDistance(pullPoint) <= GetSimpleBandShapePullDistanceThreshold() + tolerance;
        }

        private float GetSimpleBandShapePullDistanceThreshold()
        {
            return GetFullLateralPullDistance();
        }

        private float GetMinimumLateralPullRampDistance()
        {
            var contactCushion = _config.BandContactPadding * 2f;
            return Mathf.Max(0.02f, _config.MinimumPullDistance + contactCushion);
        }

        private float GetPullDistance(Vector3 pullPoint)
        {
            var delta = pullPoint - _geometry.RestPoint;
            return Mathf.Max(0f, -Vector3.Dot(delta, _geometry.LaunchFrameForward));
        }

        private SlingshotBandShapeQuery CreateBandShapeQuery(Vector3 pullPoint)
        {
            return new SlingshotBandShapeQuery(
                _geometry.LeftAnchorPosition,
                _geometry.RightAnchorPosition,
                _geometry.RestPoint,
                pullPoint,
                _geometry.LaunchFrameRight,
                _geometry.LaunchFrameForward,
                _geometry.LaunchFrameUp);
        }

        private void SetHeldTargetToRest()
        {
            SetCommittedHeldTargetPosition(_geometry.RestPoint);
        }
    }
}
