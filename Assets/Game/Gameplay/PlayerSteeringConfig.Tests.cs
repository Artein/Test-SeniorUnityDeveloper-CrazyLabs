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
    }
}

#endif // UNITY_INCLUDE_TESTS
