using System;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotPullOffsetNormalizer
    {
        float Normalize(SlingshotGeometrySnapshot geometry, float pullDistance, float pullOffset);
    }

    public sealed class SlingshotPullOffsetNormalizer : ISlingshotPullOffsetNormalizer
    {
        private readonly ISlingshotConfig _config;

        public SlingshotPullOffsetNormalizer(ISlingshotConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public float Normalize(SlingshotGeometrySnapshot geometry, float pullDistance, float pullOffset)
        {
            var validPullDistance = math.isfinite(pullDistance) ? math.max(0f, pullDistance) : 0f;
            var validPullOffset = math.isfinite(pullOffset) ? pullOffset : 0f;
            var lateralPullScale = GetLateralPullScale(validPullDistance);
            var minimumPullOffset = GetMinimumAllowedPullOffset(geometry) * lateralPullScale;
            var maximumPullOffset = GetMaximumAllowedPullOffset(geometry) * lateralPullScale;
            var clampedPullOffset = Mathf.Clamp(validPullOffset, minimumPullOffset, maximumPullOffset);

            if (clampedPullOffset < 0f)
            {
                var leftRange = -minimumPullOffset;
                return leftRange <= 0.000001f ? 0f : Mathf.Clamp(clampedPullOffset / leftRange, -1f, 0f);
            }

            if (clampedPullOffset > 0f)
            {
                var rightRange = maximumPullOffset;
                return rightRange <= 0.000001f ? 0f : Mathf.Clamp(clampedPullOffset / rightRange, 0f, 1f);
            }

            return 0f;
        }

        private float GetLateralPullScale(float pullDistance)
        {
            var fullLateralPullDistance = GetFullLateralPullDistance();
            return fullLateralPullDistance <= 0.000001f ? 1f : Mathf.Clamp01(pullDistance / fullLateralPullDistance);
        }

        private float GetFullLateralPullDistance()
        {
            return GetMinimumLateralPullRampDistance() + (_config.BandContactPadding * 2f);
        }

        private float GetMinimumLateralPullRampDistance()
        {
            var contactCushion = _config.BandContactPadding * 2f;
            return Mathf.Max(0.02f, _config.MinimumPullDistance + contactCushion);
        }

        private float GetMinimumAllowedPullOffset(SlingshotGeometrySnapshot geometry)
        {
            var leftAnchorOffset = Vector3.Dot(geometry.LeftAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
            var rightAnchorOffset = Vector3.Dot(geometry.RightAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
            var minimumAnchorOffset = Mathf.Min(leftAnchorOffset, rightAnchorOffset);
            return Mathf.Max(-_config.MaximumLateralPull, minimumAnchorOffset);
        }

        private float GetMaximumAllowedPullOffset(SlingshotGeometrySnapshot geometry)
        {
            var leftAnchorOffset = Vector3.Dot(geometry.LeftAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
            var rightAnchorOffset = Vector3.Dot(geometry.RightAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
            var maximumAnchorOffset = Mathf.Max(leftAnchorOffset, rightAnchorOffset);
            return Mathf.Min(_config.MaximumLateralPull, maximumAnchorOffset);
        }
    }
}
