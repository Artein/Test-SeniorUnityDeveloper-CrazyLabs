using System.Collections.Generic;

namespace Game.Gameplay
{
    public sealed class RunBodyMovementConfigValidator
    {
        public IEnumerable<string> Validate(RunBodyMovementConfig config)
        {
            if (config == null)
            {
                yield return "Run Body Movement Tuning config is missing.";
                yield break;
            }

            var speedConfig = (IRunBodySpeedConfig)config;
            var validityConfig = (IRunBodyMovementValidityConfig)config;
            var landingConfig = (IRunLaunchLandingStabilizationConfig)config;
            var steeringConfig = (IRunSteeringConfig)config;
            var stabilityConfig = (IRunSurfaceStabilityAuthoringConfig)config;
            var attachmentConfig = (IRunSupportAttachmentAuthoringConfig)config;
            var frameConfig = (IRunSteeringFrameAuthoringConfig)config;

            foreach (var error in ValidateSpeed(speedConfig))
                yield return error;

            foreach (var error in ValidateMovementValidity(validityConfig))
                yield return error;

            foreach (var error in ValidateLaunchLanding(landingConfig))
                yield return error;

            foreach (var error in ValidateSteering(steeringConfig))
                yield return error;

            foreach (var error in ValidateSurfaceStability(stabilityConfig))
                yield return error;

            foreach (var error in ValidateSupportAttachment(attachmentConfig))
                yield return error;

            foreach (var error in ValidateSteeringFrame(frameConfig))
                yield return error;

            if (IsFinitePositive(speedConfig.BaseSoftMaximumSpeed)
                && IsFinitePositive(validityConfig.RunBodySpeedSanityGuardMetersPerSecond)
                && speedConfig.BaseSoftMaximumSpeed >= validityConfig.RunBodySpeedSanityGuardMetersPerSecond)
            {
                yield return "Run Body base soft maximum speed must be below the Run Body Speed Sanity Guard.";
            }
        }

        private IEnumerable<string> ValidateSpeed(IRunBodySpeedConfig config)
        {
            if (!IsFiniteNonNegative(config.DownhillAcceleration))
                yield return $"{nameof(IRunBodySpeedConfig.DownhillAcceleration)} must be a finite non-negative value.";

            if (!IsFiniteNonNegative(config.SurfaceSlowdown))
                yield return $"{nameof(IRunBodySpeedConfig.SurfaceSlowdown)} must be a finite non-negative value.";

            if (!IsFiniteNonNegative(config.LowSpeedAssistTargetSpeed))
                yield return $"{nameof(IRunBodySpeedConfig.LowSpeedAssistTargetSpeed)} must be a finite non-negative value.";

            if (!IsFiniteNonNegative(config.LowSpeedAssistAcceleration))
                yield return $"{nameof(IRunBodySpeedConfig.LowSpeedAssistAcceleration)} must be a finite non-negative value.";

            if (!IsFinitePositive(config.BaseSoftMaximumSpeed))
                yield return $"{nameof(IRunBodySpeedConfig.BaseSoftMaximumSpeed)} must be a finite positive value.";

            if (!IsFiniteNonNegative(config.AboveMaximumSpeedResistance))
                yield return $"{nameof(IRunBodySpeedConfig.AboveMaximumSpeedResistance)} must be a finite non-negative value.";
        }

        private IEnumerable<string> ValidateMovementValidity(IRunBodyMovementValidityConfig config)
        {
            if (!IsFiniteNonNegative(config.MaximumSupportedSurfaceNormalLiftSpeed))
            {
                yield return
                    $"{nameof(IRunBodyMovementValidityConfig.MaximumSupportedSurfaceNormalLiftSpeed)} must be a finite non-negative value.";
            }

            if (!IsFinitePositive(config.RunBodySpeedSanityGuardMetersPerSecond))
            {
                yield return
                    $"{nameof(IRunBodyMovementValidityConfig.RunBodySpeedSanityGuardMetersPerSecond)} must be a finite positive value.";
            }
        }

        private IEnumerable<string> ValidateLaunchLanding(IRunLaunchLandingStabilizationConfig config)
        {
            if (!IsFiniteNonNegative(config.LaunchLandingStabilizationSeconds))
            {
                yield return
                    $"{nameof(IRunLaunchLandingStabilizationConfig.LaunchLandingStabilizationSeconds)} must be a finite non-negative value.";
            }

            if (!IsFiniteNonNegative(config.LaunchLandingMaximumLiftSpeed))
            {
                yield return
                    $"{nameof(IRunLaunchLandingStabilizationConfig.LaunchLandingMaximumLiftSpeed)} must be a finite non-negative value.";
            }
        }

        private IEnumerable<string> ValidateSteering(IRunSteeringConfig config)
        {
            if (!IsFinitePositive(config.RunSteeringRangeCentimeters))
                yield return $"{nameof(IRunSteeringConfig.RunSteeringRangeCentimeters)} must be a finite positive value.";

            if (!IsFiniteInRange(config.RunSteeringDeadzoneFraction, 0f, 0.95f))
            {
                yield return
                    $"{nameof(IRunSteeringConfig.RunSteeringDeadzoneFraction)} must be finite and between 0 and 0.95.";
            }

            if (!IsFiniteNonNegative(config.RunSteeringResponsiveness))
                yield return $"{nameof(IRunSteeringConfig.RunSteeringResponsiveness)} must be a finite non-negative value.";

            if (!IsFinitePositive(config.FallbackDpi))
                yield return $"{nameof(IRunSteeringConfig.FallbackDpi)} must be a finite positive value.";

            if (!IsFinitePositive(config.MinimumAcceptedDpi))
                yield return $"{nameof(IRunSteeringConfig.MinimumAcceptedDpi)} must be a finite positive value.";

            if (!IsFinitePositive(config.MaximumAcceptedDpi))
                yield return $"{nameof(IRunSteeringConfig.MaximumAcceptedDpi)} must be a finite positive value.";

            if (!IsFiniteNonNegative(config.MaximumTurnDegreesPerSecond))
            {
                yield return
                    $"{nameof(IRunSteeringConfig.MaximumTurnDegreesPerSecond)} must be a finite non-negative value.";
            }

            if (!IsFiniteNonNegative(config.RunAirSteeringMaximumTurnDegreesPerSecond))
            {
                yield return
                    $"{nameof(IRunSteeringConfig.RunAirSteeringMaximumTurnDegreesPerSecond)} must be a finite non-negative value.";
            }

            if (!IsFiniteNonNegative(config.MinimumSteerSpeed))
                yield return $"{nameof(IRunSteeringConfig.MinimumSteerSpeed)} must be a finite non-negative value.";

            if (IsFinitePositive(config.MinimumAcceptedDpi)
                && IsFinitePositive(config.MaximumAcceptedDpi)
                && config.MinimumAcceptedDpi > config.MaximumAcceptedDpi)
            {
                yield return "Run Steering minimum accepted DPI must not exceed maximum accepted DPI.";
            }

            if (IsFinitePositive(config.FallbackDpi)
                && IsFinitePositive(config.MinimumAcceptedDpi)
                && IsFinitePositive(config.MaximumAcceptedDpi)
                && (config.FallbackDpi < config.MinimumAcceptedDpi || config.FallbackDpi > config.MaximumAcceptedDpi))
            {
                yield return "Run Steering fallback DPI must be within the accepted DPI range.";
            }
        }

        private IEnumerable<string> ValidateSurfaceStability(IRunSurfaceStabilityAuthoringConfig config)
        {
            if (!IsFiniteNonNegative(config.SupportLossConfirmationSeconds))
            {
                yield return
                    $"{nameof(IRunSurfaceStabilityAuthoringConfig.SupportLossConfirmationSeconds)} must be a finite non-negative value.";
            }

            if (!IsFiniteInRange(config.DiscontinuousNormalThresholdDegrees, 0f, 180f))
            {
                yield return
                    $"{nameof(IRunSurfaceStabilityAuthoringConfig.DiscontinuousNormalThresholdDegrees)} must be finite and between 0 and 180 degrees.";
            }

            if (!IsFiniteNonNegative(config.DiscontinuousNormalConfirmationSeconds))
            {
                yield return
                    $"{nameof(IRunSurfaceStabilityAuthoringConfig.DiscontinuousNormalConfirmationSeconds)} must be a finite non-negative value.";
            }

            if (!IsFiniteInRange(config.CandidateCoherenceDegrees, 0f, 180f))
            {
                yield return
                    $"{nameof(IRunSurfaceStabilityAuthoringConfig.CandidateCoherenceDegrees)} must be finite and between 0 and 180 degrees.";
            }
        }

        private IEnumerable<string> ValidateSteeringFrame(IRunSteeringFrameAuthoringConfig config)
        {
            if (!IsFiniteNonNegative(config.NormalSlewDegreesPerSecond))
            {
                yield return
                    $"{nameof(IRunSteeringFrameAuthoringConfig.NormalSlewDegreesPerSecond)} must be a finite non-negative value.";
            }

            if (!IsFiniteNonNegative(config.AirborneUpRetentionSeconds))
            {
                yield return
                    $"{nameof(IRunSteeringFrameAuthoringConfig.AirborneUpRetentionSeconds)} must be a finite non-negative value.";
            }
        }

        private IEnumerable<string> ValidateSupportAttachment(IRunSupportAttachmentAuthoringConfig config)
        {
            if (!IsFiniteNonNegative(config.MaximumAttachedSurfaceNormalLiftSpeed))
            {
                yield return
                    $"{nameof(IRunSupportAttachmentAuthoringConfig.MaximumAttachedSurfaceNormalLiftSpeed)} must be a finite non-negative value.";
            }

            if (!IsFiniteNonNegative(config.SameSurfaceReattachmentSeparationMeters))
            {
                yield return
                    $"{nameof(IRunSupportAttachmentAuthoringConfig.SameSurfaceReattachmentSeparationMeters)} must be a finite non-negative value.";
            }

            if (!IsFiniteInRange(config.MinimumReattachmentNormalChangeDegrees, 0f, 180f))
            {
                yield return
                    $"{nameof(IRunSupportAttachmentAuthoringConfig.MinimumReattachmentNormalChangeDegrees)} must be finite and between 0 and 180.";
            }

            if (!IsFiniteNonNegative(config.TransitionConfirmationSeconds))
            {
                yield return
                    $"{nameof(IRunSupportAttachmentAuthoringConfig.TransitionConfirmationSeconds)} must be a finite non-negative value.";
            }
        }

        private bool IsFinitePositive(float value)
        {
            return float.IsFinite(value) && value > 0f;
        }

        private bool IsFiniteNonNegative(float value)
        {
            return float.IsFinite(value) && value >= 0f;
        }

        private bool IsFiniteInRange(float value, float minimum, float maximum)
        {
            return float.IsFinite(value) && value >= minimum && value <= maximum;
        }
    }
}
