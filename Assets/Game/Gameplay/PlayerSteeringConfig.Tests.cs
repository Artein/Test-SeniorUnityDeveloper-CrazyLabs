#if UNITY_INCLUDE_TESTS

namespace Game.Gameplay
{
    public sealed partial class PlayerSteeringConfig
    {
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
