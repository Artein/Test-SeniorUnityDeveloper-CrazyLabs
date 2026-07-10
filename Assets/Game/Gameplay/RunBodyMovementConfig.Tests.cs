#if UNITY_INCLUDE_TESTS

namespace Game.Gameplay
{
    public sealed partial class RunBodyMovementConfig
    {
        internal void SetSpeedValuesForTests(
            float downhillAcceleration,
            float surfaceSlowdown,
            float lowSpeedAssistTargetSpeed,
            float lowSpeedAssistAcceleration,
            float baseSoftMaximumSpeed,
            float aboveMaximumSpeedResistance)
        {
            _downhillAcceleration = downhillAcceleration;
            _surfaceSlowdown = surfaceSlowdown;
            _lowSpeedAssistTargetSpeed = lowSpeedAssistTargetSpeed;
            _lowSpeedAssistAcceleration = lowSpeedAssistAcceleration;
            _baseSoftMaximumSpeed = baseSoftMaximumSpeed;
            _aboveMaximumSpeedResistance = aboveMaximumSpeedResistance;
        }

        internal void SetMovementValidityValuesForTests(
            float maximumSupportedSurfaceNormalLiftSpeed,
            float runBodySpeedSanityGuardMetersPerSecond)
        {
            _maximumSupportedSurfaceNormalLiftSpeed = maximumSupportedSurfaceNormalLiftSpeed;
            _runBodySpeedSanityGuardMetersPerSecond = runBodySpeedSanityGuardMetersPerSecond;
        }

        internal void SetSteeringInputValuesForTests(
            float rangeCentimeters,
            float deadzoneFraction,
            float responsiveness,
            float fallbackDpi,
            float minimumAcceptedDpi,
            float maximumAcceptedDpi)
        {
            _runSteeringRangeCentimeters = rangeCentimeters;
            _runSteeringDeadzoneFraction = deadzoneFraction;
            _runSteeringResponsiveness = responsiveness;
            _fallbackDpi = fallbackDpi;
            _minimumAcceptedDpi = minimumAcceptedDpi;
            _maximumAcceptedDpi = maximumAcceptedDpi;
        }

        internal void SetRunSteeringFrameStabilityForTests(
            float normalSlewDegreesPerSecond,
            float snapDegrees,
            float ungroundedGraceSeconds,
            float suspectNormalConfirmationSeconds)
        {
            _runSteeringFrameNormalSlewDegreesPerSecond = normalSlewDegreesPerSecond;
            _runSteeringFrameSnapDegrees = snapDegrees;
            _runSteeringFrameUngroundedGraceSeconds = ungroundedGraceSeconds;
            _runSteeringFrameSuspectNormalConfirmationSeconds = suspectNormalConfirmationSeconds;
        }

        internal void SetLaunchLandingStabilizationForTests(
            float stabilizationSeconds,
            float maximumLiftSpeed)
        {
            _launchLandingStabilizationSeconds = stabilizationSeconds;
            _launchLandingMaximumLiftSpeed = maximumLiftSpeed;
        }

        internal void SetRunBodySpeedSanityGuardForTests(float metersPerSecond)
        {
            _runBodySpeedSanityGuardMetersPerSecond = metersPerSecond;
        }

        internal void SetRunAirSteeringMaximumTurnDegreesPerSecondForTests(float degreesPerSecond)
        {
            _runAirSteeringMaximumTurnDegreesPerSecond = degreesPerSecond;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
