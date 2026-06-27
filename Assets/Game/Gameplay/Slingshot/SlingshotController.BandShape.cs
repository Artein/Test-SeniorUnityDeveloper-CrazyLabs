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
            _recoilBandShapeBuffer = new Vector3[pointCount];
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
            var solved = _bandShapeProvider.TryCreateBandShape(new SlingshotBandShapeQuery(
                    _geometry.LeftAnchorPosition,
                    _geometry.RightAnchorPosition,
                    _geometry.RestPoint,
                    pullPoint,
                    _geometry.LaunchFrameRight,
                    _geometry.LaunchFrameForward,
                    _geometry.LaunchFrameUp),
                buffer,
                out var pointCount);

            return solved && pointCount == buffer.Length;
        }

        private SlingshotBandShape CreateReleaseRecoilBandShape(float progress)
        {
            var recoilPullPoint = Vector3.Lerp(_pendingLaunchRequest.FinalPullPoint, _geometry.RestPoint, progress);

            if (ShouldUseSimpleBandShape(recoilPullPoint))
            {
                FillSimpleBandShapeBuffer(_recoilBandShapeBuffer, recoilPullPoint);
                return new SlingshotBandShape(_recoilBandShapeBuffer, true);
            }

            if (TryUpdateTautActiveBandShape(recoilPullPoint))
                return new SlingshotBandShape(_currentActiveBandShapeBuffer, true);

            FillBandShapeBuffer(_restBandShapeBuffer, _geometry.RestPoint);

            for (var pointIndex = 0; pointIndex < _recoilBandShapeBuffer.Length; pointIndex += 1)
            {
                _recoilBandShapeBuffer[pointIndex] = Vector3.Lerp(
                    _currentActiveBandShapeBuffer[pointIndex],
                    _restBandShapeBuffer[pointIndex],
                    progress);
            }

            return new SlingshotBandShape(_recoilBandShapeBuffer, true);
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

        private void SetHeldTargetToRest()
        {
            _heldLaunchTarget.SetHeldPosition(_geometry.RestPoint);
        }
    }
}
