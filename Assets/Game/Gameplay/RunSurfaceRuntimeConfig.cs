using System;
using UnityEngine;

namespace Game.Gameplay
{
    internal readonly struct RunSurfaceProbeConfig
    {
        public float Distance { get; }
        public float SkinWidth { get; }
        public LayerMask SurfaceMask { get; }
        public float MinimumSupportNormalDot { get; }
        public float FootprintSampleOffsetScale { get; }
        public float FootprintNormalClusterAngleDegrees { get; }

        public RunSurfaceProbeConfig(
            float distance,
            float skinWidth,
            LayerMask surfaceMask,
            float minimumSupportNormalDot,
            float footprintSampleOffsetScale,
            float footprintNormalClusterAngleDegrees)
        {
            if (!float.IsFinite(distance) || distance < 0f)
                throw new ArgumentOutOfRangeException(nameof(distance));

            if (!float.IsFinite(skinWidth) || skinWidth < 0f)
                throw new ArgumentOutOfRangeException(nameof(skinWidth));

            if (surfaceMask.value == 0)
                throw new ArgumentOutOfRangeException(nameof(surfaceMask));

            if (!float.IsFinite(minimumSupportNormalDot)
                || minimumSupportNormalDot < -1f
                || minimumSupportNormalDot > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumSupportNormalDot));
            }

            if (!float.IsFinite(footprintSampleOffsetScale)
                || footprintSampleOffsetScale < 0f
                || footprintSampleOffsetScale > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(footprintSampleOffsetScale));
            }

            if (!float.IsFinite(footprintNormalClusterAngleDegrees)
                || footprintNormalClusterAngleDegrees < 0f
                || footprintNormalClusterAngleDegrees > 180f)
            {
                throw new ArgumentOutOfRangeException(nameof(footprintNormalClusterAngleDegrees));
            }

            Distance = distance;
            SkinWidth = skinWidth;
            SurfaceMask = surfaceMask;
            MinimumSupportNormalDot = minimumSupportNormalDot;
            FootprintSampleOffsetScale = footprintSampleOffsetScale;
            FootprintNormalClusterAngleDegrees = footprintNormalClusterAngleDegrees;
        }
    }

    internal readonly struct RunSurfaceStabilityConfig
    {
        public float SupportLossConfirmationSeconds { get; }
        public float DiscontinuousNormalThresholdDegrees { get; }
        public float DiscontinuousNormalConfirmationSeconds { get; }
        public float CandidateCoherenceDegrees { get; }

        public RunSurfaceStabilityConfig(
            float supportLossConfirmationSeconds,
            float discontinuousNormalThresholdDegrees,
            float discontinuousNormalConfirmationSeconds,
            float candidateCoherenceDegrees)
        {
            if (!float.IsFinite(supportLossConfirmationSeconds) || supportLossConfirmationSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(supportLossConfirmationSeconds));

            if (!float.IsFinite(discontinuousNormalThresholdDegrees)
                || discontinuousNormalThresholdDegrees < 0f
                || discontinuousNormalThresholdDegrees > 180f)
            {
                throw new ArgumentOutOfRangeException(nameof(discontinuousNormalThresholdDegrees));
            }

            if (!float.IsFinite(discontinuousNormalConfirmationSeconds) || discontinuousNormalConfirmationSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(discontinuousNormalConfirmationSeconds));

            if (!float.IsFinite(candidateCoherenceDegrees)
                || candidateCoherenceDegrees < 0f
                || candidateCoherenceDegrees > 180f)
            {
                throw new ArgumentOutOfRangeException(nameof(candidateCoherenceDegrees));
            }

            SupportLossConfirmationSeconds = supportLossConfirmationSeconds;
            DiscontinuousNormalThresholdDegrees = discontinuousNormalThresholdDegrees;
            DiscontinuousNormalConfirmationSeconds = discontinuousNormalConfirmationSeconds;
            CandidateCoherenceDegrees = candidateCoherenceDegrees;
        }
    }

    internal readonly struct RunSupportAttachmentConfig
    {
        public float MaximumAttachedSurfaceNormalLiftSpeed { get; }
        public float SameSurfaceReattachmentSeparationMeters { get; }
        public float MinimumReattachmentNormalChangeDegrees { get; }
        public float TransitionConfirmationSeconds { get; }

        public RunSupportAttachmentConfig(
            float maximumAttachedSurfaceNormalLiftSpeed,
            float sameSurfaceReattachmentSeparationMeters,
            float minimumReattachmentNormalChangeDegrees,
            float transitionConfirmationSeconds)
        {
            if (!float.IsFinite(maximumAttachedSurfaceNormalLiftSpeed) || maximumAttachedSurfaceNormalLiftSpeed < 0f)
                throw new ArgumentOutOfRangeException(nameof(maximumAttachedSurfaceNormalLiftSpeed));

            if (!float.IsFinite(sameSurfaceReattachmentSeparationMeters) || sameSurfaceReattachmentSeparationMeters < 0f)
                throw new ArgumentOutOfRangeException(nameof(sameSurfaceReattachmentSeparationMeters));

            if (!float.IsFinite(minimumReattachmentNormalChangeDegrees)
                || minimumReattachmentNormalChangeDegrees < 0f
                || minimumReattachmentNormalChangeDegrees > 180f)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumReattachmentNormalChangeDegrees));
            }

            if (!float.IsFinite(transitionConfirmationSeconds) || transitionConfirmationSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(transitionConfirmationSeconds));

            MaximumAttachedSurfaceNormalLiftSpeed = maximumAttachedSurfaceNormalLiftSpeed;
            SameSurfaceReattachmentSeparationMeters = sameSurfaceReattachmentSeparationMeters;
            MinimumReattachmentNormalChangeDegrees = minimumReattachmentNormalChangeDegrees;
            TransitionConfirmationSeconds = transitionConfirmationSeconds;
        }
    }

    internal readonly struct RunSteeringFrameConfig
    {
        public float NormalSlewDegreesPerSecond { get; }
        public float AirborneUpRetentionSeconds { get; }

        public RunSteeringFrameConfig(
            float normalSlewDegreesPerSecond,
            float airborneUpRetentionSeconds)
        {
            if (!float.IsFinite(normalSlewDegreesPerSecond) || normalSlewDegreesPerSecond < 0f)
                throw new ArgumentOutOfRangeException(nameof(normalSlewDegreesPerSecond));

            if (!float.IsFinite(airborneUpRetentionSeconds) || airborneUpRetentionSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(airborneUpRetentionSeconds));

            NormalSlewDegreesPerSecond = normalSlewDegreesPerSecond;
            AirborneUpRetentionSeconds = airborneUpRetentionSeconds;
        }
    }
}
