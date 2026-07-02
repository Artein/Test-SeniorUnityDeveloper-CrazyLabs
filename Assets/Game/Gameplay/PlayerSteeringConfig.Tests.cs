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

        internal void SetLaunchBurstForTests(
            float graceSeconds,
            float recoverySeconds,
            float maximumPlanarSpeedMultiplier)
        {
            _launchBurstPlanarSpeedGraceSeconds = graceSeconds;
            _launchBurstPlanarSpeedRecoverySeconds = recoverySeconds;
            _launchBurstMaximumPlanarSpeedMultiplier = maximumPlanarSpeedMultiplier;
        }

        internal void SetLaunchLandingStabilizationForTests(
            float stabilizationSeconds,
            float maximumLiftSpeed)
        {
            _launchLandingStabilizationSeconds = stabilizationSeconds;
            _launchLandingMaximumLiftSpeed = maximumLiftSpeed;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
