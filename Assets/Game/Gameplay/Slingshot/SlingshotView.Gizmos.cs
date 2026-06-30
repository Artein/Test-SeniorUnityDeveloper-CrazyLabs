using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public sealed partial class SlingshotView
    {
        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos || !HasGeometryReferences())
                return;

            var geometry = CreateGeometrySnapshotFromTransforms();
            DrawBandGizmos(geometry);
            DrawLaunchFrameGizmos(geometry);
            DrawPullPlaneGizmos(geometry);
            DrawPullLimitGizmos(geometry);
            DrawTouchTargetGizmos(geometry);
            DrawBandShapeTuningGizmos(geometry);
        }

        private bool HasGeometryReferences()
        {
            return _leftAnchor != null
                   && _rightAnchor != null
                   && _restPoint != null
                   && _launchFrame != null;
        }

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
