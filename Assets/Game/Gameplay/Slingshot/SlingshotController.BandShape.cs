using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public sealed partial class SlingshotController
    {
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
            if (ShouldUseSimpleBandShape(pullPoint))
            {
                FillSimpleBandShapeBuffer(_inactiveActiveBandShapeBuffer, pullPoint);
                (_currentActiveBandShapeBuffer, _inactiveActiveBandShapeBuffer) = (_inactiveActiveBandShapeBuffer, _currentActiveBandShapeBuffer);
                _hasLastValidActiveBandShape = true;
                return true;
            }

            return TryUpdateTautActiveBandShape(pullPoint);
        }

        private bool TryUpdateTautActiveBandShape(Vector3 pullPoint)
        {
            if (!TryWriteBandShape(pullPoint, _inactiveActiveBandShapeBuffer))
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
            return _view.VisibleBandRadius;
        }

        private float GetBandTargetClearanceRadius()
        {
            return _view.VisibleBandRadius + _config.BandContactPadding;
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

        // TODO: Fix this method
        private SlingshotBandShape CreateReleaseRecoilBandShape(float progress)
        {
            var recoilPullPoint = GetReleaseRecoilPullPoint(progress, out var wasDepthClamped);

            if (wasDepthClamped && TryUpdateClearSimpleReleaseRecoilBandShape(recoilPullPoint))
                return new SlingshotBandShape(_currentActiveBandShapeBuffer, true);

            if (TryUpdateTautActiveBandShape(recoilPullPoint)
                || (!wasDepthClamped && TryUpdateClearSimpleReleaseRecoilBandShape(recoilPullPoint)))
            {
                return new SlingshotBandShape(_currentActiveBandShapeBuffer, true);
            }

            return new SlingshotBandShape(_currentActiveBandShapeBuffer, true);
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
            var minimumClearDepth = Mathf.Max(0f, maximumDepth + GetBandTargetClearanceRadius());

            if (currentDepth >= minimumClearDepth)
                return recoilPullPoint;

            wasDepthClamped = true;
            return recoilPullPoint - (_geometry.LaunchFrameForward * (minimumClearDepth - currentDepth));
        }

        private bool TryUpdateClearSimpleReleaseRecoilBandShape(Vector3 pullPoint)
        {
            FillSimpleBandShapeBuffer(_inactiveActiveBandShapeBuffer, pullPoint);
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
                       _view.VisibleBandRadius,
                       out var isClear)
                   && isClear;
        }

        private bool HasTargetSilhouettePassedRestBandShape()
        {
            return _bandShapeDepthProvider.TryGetSilhouetteDepthSpan(
                       CreateBandShapeQuery(_geometry.RestPoint),
                       out _,
                       out var maximumDepth)
                   && maximumDepth <= -_view.VisibleBandRadius;
        }

        private SlingshotBandShape CreateDetachedRestBandShape()
        {
            FillBandShapeBuffer(_restBandShapeBuffer, _geometry.RestPoint);
            return new SlingshotBandShape(_restBandShapeBuffer, true);
        }

        private SlingshotBandShape CreateHeldRestBandShape()
        {
            FillBandShapeBuffer(_restBandShapeBuffer, _geometry.RestPoint);
            return new SlingshotBandShape(_restBandShapeBuffer, true);
        }

        private void FillSimpleBandShapeBuffer(Vector3[] buffer, Vector3 centerPoint)
        {
            FillBandShapeBuffer(buffer, GetSimpleBandVisualCenterPoint(centerPoint));
        }

        private Vector3 GetSimpleBandVisualCenterPoint(Vector3 centerPoint)
        {
            return centerPoint - (_geometry.LaunchFrameForward * _config.BandContactPadding);
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
