using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotView
    {
        SlingshotGeometrySnapshot CreateGeometrySnapshot();
        void ShowInactiveIdle(SlingshotBandShape bandShape);
        void ShowCaptureIdle(SlingshotBandShape bandShape);
        void ShowLoadedRelease(SlingshotBandShape bandShape);
        void ShowActivePull(SlingshotPullVisual pullVisual);
    }

    public sealed partial class SlingshotView : MonoBehaviour, ISlingshotView
    {
        [SerializeField] private Transform _leftAnchor;
        [SerializeField] private Transform _rightAnchor;
        [SerializeField] private Transform _restPoint;
        [SerializeField] private Transform _launchFrame;
        [SerializeField] private LineRenderer _bandLineRenderer;
        [SerializeField] private GameObject _pullHintObject;
        [SerializeField] private GameObject _touchIndicatorObject;
        [SerializeField] private SlingshotConfig _gizmoConfig;
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField, Min(0.05f)] private float _gizmoFrameAxisLength = 0.75f;
        [SerializeField, Min(0.01f)] private float _gizmoTouchTargetWorldRadius = 0.25f;

        public SlingshotGeometrySnapshot CreateGeometrySnapshot()
        {
            ThrowIfInvalidReferences();
            return CreateGeometrySnapshotFromTransforms();
        }

        public void ShowInactiveIdle(SlingshotBandShape bandShape)
        {
            ThrowIfInvalidReferences();
            ApplyBandShape(bandShape);
            SetVisualObjects(false, false);
        }

        public void ShowCaptureIdle(SlingshotBandShape bandShape)
        {
            ThrowIfInvalidReferences();
            ApplyBandShape(bandShape);
            SetVisualObjects(true, false);
        }

        public void ShowLoadedRelease(SlingshotBandShape bandShape)
        {
            ThrowIfInvalidReferences();
            ApplyBandShape(bandShape);
            SetVisualObjects(false, false);
        }

        public void ShowActivePull(SlingshotPullVisual pullVisual)
        {
            ThrowIfInvalidReferences();
            ApplyBandShape(pullVisual.BandShape);
            SetTouchIndicatorScreenPosition(pullVisual.TouchIndicatorScreenPosition);
            SetVisualObjects(false, true);
        }

        private void OnValidate()
        {
            foreach (var error in GetReferenceValidationErrors())
            {
                Debug.LogWarning(error, this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos || !HasGeometryReferences())
                return;

            var geometry = CreateGeometrySnapshotFromTransforms();
            DrawBandGizmos(geometry);
            DrawLaunchFrameGizmos(geometry);
            DrawPullPlaneGizmos(geometry);
            DrawPullLimitGizmos(geometry);
            DrawLaunchAngleGizmos(geometry);
            DrawTouchTargetGizmos(geometry);
            DrawBandShapeTuningGizmos(geometry);
        }

        private void ApplyBandShape(SlingshotBandShape bandShape)
        {
            var points = bandShape.Points;
            _bandLineRenderer.positionCount = points.Count;

            for (var i = 0; i < points.Count; i += 1)
            {
                _bandLineRenderer.SetPosition(i, points[i]);
            }
        }

        private void SetVisualObjects(bool pullHintActive, bool touchIndicatorActive)
        {
            _pullHintObject.SetActive(pullHintActive);
            _touchIndicatorObject.SetActive(touchIndicatorActive);
        }

        private void SetTouchIndicatorScreenPosition(Vector2 screenPosition)
        {
            var indicatorTransform = _touchIndicatorObject.transform;
            var currentPosition = indicatorTransform.position;
            indicatorTransform.position = new Vector3(screenPosition.x, screenPosition.y, currentPosition.z);
        }

        private SlingshotGeometrySnapshot CreateGeometrySnapshotFromTransforms()
        {
            return new SlingshotGeometrySnapshot(
                _leftAnchor.position,
                _rightAnchor.position,
                _restPoint.position,
                _launchFrame.right,
                _launchFrame.forward,
                _launchFrame.up);
        }

        private void ThrowIfInvalidReferences()
        {
            var errors = GetReferenceValidationErrors().ToList();

            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(" ", errors));
        }

        private IEnumerable<string> GetReferenceValidationErrors()
        {
            if (_leftAnchor == null)
                yield return "SlingshotView requires a Left Anchor reference.";

            if (_rightAnchor == null)
                yield return "SlingshotView requires a Right Anchor reference.";

            if (_restPoint == null)
                yield return "SlingshotView requires a Rest Point reference.";

            if (_launchFrame == null)
                yield return "SlingshotView requires a Launch Frame reference.";

            if (_bandLineRenderer == null)
                yield return "SlingshotView requires a Band Line Renderer reference.";

            if (_pullHintObject == null)
                yield return "SlingshotView requires a Pull Hint object reference.";

            if (_touchIndicatorObject == null)
                yield return "SlingshotView requires a Touch Indicator object reference.";
        }

        private bool HasGeometryReferences()
        {
            return _leftAnchor != null
                   && _rightAnchor != null
                   && _restPoint != null
                   && _launchFrame != null;
        }

        // TODO - AI Note: Extract into SlingshotView.Gizmos partial class
        private void DrawBandGizmos(SlingshotGeometrySnapshot geometry)
        {
            Gizmos.color = new Color(0.95f, 0.8f, 0.35f, 1f);
            Gizmos.DrawLine(geometry.LeftAnchorPosition, geometry.RestPoint);
            Gizmos.DrawLine(geometry.RestPoint, geometry.RightAnchorPosition);
            Gizmos.DrawWireSphere(geometry.LeftAnchorPosition, 0.08f);
            Gizmos.DrawWireSphere(geometry.RightAnchorPosition, 0.08f);
            Gizmos.DrawWireSphere(geometry.RestPoint, 0.08f);
        }

        private void DrawLaunchFrameGizmos(SlingshotGeometrySnapshot geometry)
        {
            Gizmos.color = new Color(0.9f, 0.15f, 0.15f, 1f);

            Gizmos.DrawLine(
                geometry.RestPoint,
                geometry.RestPoint + (geometry.LaunchFrameRight * _gizmoFrameAxisLength));

            Gizmos.color = new Color(0.1f, 0.4f, 0.95f, 1f);

            Gizmos.DrawLine(
                geometry.RestPoint,
                geometry.RestPoint + (geometry.LaunchFrameForward * _gizmoFrameAxisLength));

            Gizmos.color = new Color(0.1f, 0.8f, 0.2f, 1f);

            Gizmos.DrawLine(
                geometry.RestPoint,
                geometry.RestPoint + (geometry.LaunchFrameUp * _gizmoFrameAxisLength));
        }

        private void DrawPullPlaneGizmos(SlingshotGeometrySnapshot geometry)
        {
            var rightExtent = geometry.LaunchFrameRight * _gizmoFrameAxisLength;
            var forwardExtent = geometry.LaunchFrameForward * _gizmoFrameAxisLength;
            var frontLeft = geometry.RestPoint + forwardExtent - rightExtent;
            var frontRight = geometry.RestPoint + forwardExtent + rightExtent;
            var backLeft = geometry.RestPoint - forwardExtent - rightExtent;
            var backRight = geometry.RestPoint - forwardExtent + rightExtent;

            Gizmos.color = new Color(0.25f, 0.95f, 0.95f, 0.5f);
            Gizmos.DrawLine(frontLeft, frontRight);
            Gizmos.DrawLine(frontRight, backRight);
            Gizmos.DrawLine(backRight, backLeft);
            Gizmos.DrawLine(backLeft, frontLeft);
            Gizmos.DrawLine(geometry.RestPoint - rightExtent, geometry.RestPoint + rightExtent);
            Gizmos.DrawLine(geometry.RestPoint - forwardExtent, geometry.RestPoint + forwardExtent);
        }

        private void DrawPullLimitGizmos(SlingshotGeometrySnapshot geometry)
        {
            if (_gizmoConfig == null)
                return;

            var rightLimit = geometry.LaunchFrameRight * _gizmoConfig.MaximumLateralPull;
            var backwardLimit = geometry.LaunchFrameForward * _gizmoConfig.MaximumPullDistance;
            var frontLeft = geometry.RestPoint - rightLimit;
            var frontRight = geometry.RestPoint + rightLimit;
            var backLeft = frontLeft - backwardLimit;
            var backRight = frontRight - backwardLimit;

            Gizmos.color = new Color(0.3f, 0.75f, 1f, 0.9f);
            Gizmos.DrawLine(frontLeft, frontRight);
            Gizmos.DrawLine(frontRight, backRight);
            Gizmos.DrawLine(backRight, backLeft);
            Gizmos.DrawLine(backLeft, frontLeft);
        }

        private void DrawLaunchAngleGizmos(SlingshotGeometrySnapshot geometry)
        {
            if (_gizmoConfig == null)
                return;

            var leftDirection = Quaternion.AngleAxis(-_gizmoConfig.MaximumLaunchAngleDegrees, geometry.LaunchFrameUp) * geometry.LaunchFrameForward;
            var rightDirection = Quaternion.AngleAxis(_gizmoConfig.MaximumLaunchAngleDegrees, geometry.LaunchFrameUp) * geometry.LaunchFrameForward;

            Gizmos.color = new Color(1f, 0.45f, 0.05f, 0.95f);
            Gizmos.DrawLine(geometry.RestPoint, geometry.RestPoint + (leftDirection * _gizmoFrameAxisLength));
            Gizmos.DrawLine(geometry.RestPoint, geometry.RestPoint + (rightDirection * _gizmoFrameAxisLength));
            DrawLaunchAngleArc(geometry);
        }

        private void DrawLaunchAngleArc(SlingshotGeometrySnapshot geometry)
        {
            var segmentCount = 12;
            var previousPoint = GetLaunchAngleArcPoint(geometry, -_gizmoConfig.MaximumLaunchAngleDegrees);

            for (var segmentIndex = 1; segmentIndex <= segmentCount; segmentIndex += 1)
            {
                var normalizedSegment = (float)segmentIndex / segmentCount;
                var angle = Mathf.Lerp(-_gizmoConfig.MaximumLaunchAngleDegrees, _gizmoConfig.MaximumLaunchAngleDegrees, normalizedSegment);
                var currentPoint = GetLaunchAngleArcPoint(geometry, angle);
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }

        private Vector3 GetLaunchAngleArcPoint(SlingshotGeometrySnapshot geometry, float angleDegrees)
        {
            var direction = Quaternion.AngleAxis(angleDegrees, geometry.LaunchFrameUp) * geometry.LaunchFrameForward;
            return geometry.RestPoint + (direction * _gizmoFrameAxisLength);
        }

        private void DrawTouchTargetGizmos(SlingshotGeometrySnapshot geometry)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.85f);
            Gizmos.DrawWireSphere(geometry.LeftAnchorPosition, _gizmoTouchTargetWorldRadius);
            Gizmos.DrawWireSphere(geometry.RestPoint, _gizmoTouchTargetWorldRadius);
            Gizmos.DrawWireSphere(geometry.RightAnchorPosition, _gizmoTouchTargetWorldRadius);
        }

        private void DrawBandShapeTuningGizmos(SlingshotGeometrySnapshot geometry)
        {
            if (_gizmoConfig == null)
                return;

            var maxPullCenter = geometry.RestPoint - (geometry.LaunchFrameForward * _gizmoConfig.MaximumPullDistance);
            var lateralLimit = geometry.LaunchFrameRight * _gizmoConfig.MaximumLateralPull;
            var leftPulledSideCenter = maxPullCenter - lateralLimit;
            var rightPulledSideCenter = maxPullCenter + lateralLimit;
            var contactPaddingRadius = Mathf.Max(0.01f, _gizmoConfig.BandContactPadding);

            Gizmos.color = new Color(0.75f, 0.25f, 1f, 0.9f);
            Gizmos.DrawLine(leftPulledSideCenter, rightPulledSideCenter);
            Gizmos.DrawWireSphere(leftPulledSideCenter, contactPaddingRadius);
            Gizmos.DrawWireSphere(maxPullCenter, contactPaddingRadius);
            Gizmos.DrawWireSphere(rightPulledSideCenter, contactPaddingRadius);

            DrawBandWrapSampleGizmos(leftPulledSideCenter, rightPulledSideCenter);
            DrawSilhouetteSampleDensityGizmos(geometry, maxPullCenter);
        }

        private void DrawBandWrapSampleGizmos(Vector3 leftPulledSideCenter, Vector3 rightPulledSideCenter)
        {
            var sampleCount = Mathf.Max(3, _gizmoConfig.BandWrapSampleCount);
            var sampleRadius = Mathf.Max(0.015f, _gizmoFrameAxisLength * 0.025f);

            Gizmos.color = new Color(0.95f, 0.35f, 1f, 0.75f);

            for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex += 1)
            {
                var progress = sampleCount == 1 ? 0.5f : (float)sampleIndex / (sampleCount - 1);
                Gizmos.DrawWireSphere(Vector3.Lerp(leftPulledSideCenter, rightPulledSideCenter, progress), sampleRadius);
            }
        }

        private void DrawSilhouetteSampleDensityGizmos(SlingshotGeometrySnapshot geometry, Vector3 center)
        {
            var sampleCount = Mathf.Max(8, _gizmoConfig.BandSilhouetteSampleCount);
            var radius = Mathf.Max(_gizmoFrameAxisLength * 0.2f, _gizmoConfig.BandContactPadding * 2f);
            var previousPoint = GetBandShapeSampleCirclePoint(geometry, center, radius, 0f);

            Gizmos.color = new Color(0.55f, 0.9f, 1f, 0.65f);

            for (var sampleIndex = 1; sampleIndex <= sampleCount; sampleIndex += 1)
            {
                var angle = (float)sampleIndex / sampleCount * Mathf.PI * 2f;
                var currentPoint = GetBandShapeSampleCirclePoint(geometry, center, radius, angle);
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }

        private Vector3 GetBandShapeSampleCirclePoint(
            SlingshotGeometrySnapshot geometry,
            Vector3 center,
            float radius,
            float angle)
        {
            var backward = -geometry.LaunchFrameForward;

            return center
                   + (geometry.LaunchFrameRight * Mathf.Cos(angle) * radius)
                   + (backward * Mathf.Sin(angle) * radius);
        }
    }
}
