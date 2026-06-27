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
            FillRestBandShapeBuffer();
        }

        private bool TryUpdateActiveBandShape(Vector3 pullPoint)
        {
            var solved = _bandShapeProvider.TryCreateBandShape(new SlingshotBandShapeQuery(
                    _geometry.LeftAnchorPosition,
                    _geometry.RightAnchorPosition,
                    _geometry.RestPoint,
                    pullPoint,
                    _geometry.LaunchFrameRight,
                    _geometry.LaunchFrameForward,
                    _geometry.LaunchFrameUp),
                _inactiveActiveBandShapeBuffer,
                out var pointCount);

            if (!solved || pointCount != _inactiveActiveBandShapeBuffer.Length)
                return false;

            (_currentActiveBandShapeBuffer, _inactiveActiveBandShapeBuffer) = (_inactiveActiveBandShapeBuffer, _currentActiveBandShapeBuffer);
            _hasLastValidActiveBandShape = true;

            return true;
        }

        private SlingshotBandShape CreateReleaseRecoilBandShape(float progress)
        {
            var recoilPullPoint = Vector3.Lerp(_pendingLaunchRequest.FinalPullPoint, _geometry.RestPoint, progress);

            if (TryUpdateActiveBandShape(recoilPullPoint))
                return new SlingshotBandShape(_currentActiveBandShapeBuffer, true);

            FillRestBandShapeBuffer();

            for (var pointIndex = 0; pointIndex < _recoilBandShapeBuffer.Length; pointIndex += 1)
            {
                _recoilBandShapeBuffer[pointIndex] = Vector3.Lerp(
                    _currentActiveBandShapeBuffer[pointIndex],
                    _restBandShapeBuffer[pointIndex],
                    progress);
            }

            return new SlingshotBandShape(_recoilBandShapeBuffer, true);
        }

        private SlingshotBandShape CreateRestBandShape()
        {
            FillRestBandShapeBuffer();
            return new SlingshotBandShape(_restBandShapeBuffer, true);
        }

        private void FillRestBandShapeBuffer()
        {
            var middleIndex = (_restBandShapeBuffer.Length - 1) / 2;
            var lastIndex = _restBandShapeBuffer.Length - 1;

            for (var pointIndex = 0; pointIndex <= middleIndex; pointIndex += 1)
            {
                var progress = middleIndex <= 0 ? 1f : (float)pointIndex / middleIndex;
                _restBandShapeBuffer[pointIndex] = Vector3.Lerp(_geometry.LeftAnchorPosition, _geometry.RestPoint, progress);
            }

            for (var pointIndex = middleIndex + 1; pointIndex <= lastIndex; pointIndex += 1)
            {
                var progress = (float)(pointIndex - middleIndex) / (lastIndex - middleIndex);
                _restBandShapeBuffer[pointIndex] = Vector3.Lerp(_geometry.RestPoint, _geometry.RightAnchorPosition, progress);
            }
        }

        private void SetHeldTargetToRest()
        {
            _heldLaunchTarget.SetHeldPosition(_geometry.RestPoint);
        }
    }
}
