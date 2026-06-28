#if UNITY_INCLUDE_TESTS

namespace Game.Gameplay
{
    public sealed partial class RunEndConfig
    {
        internal void SetValuesForTests(
            float obstacleImpactSpeedThreshold,
            float lostMomentumLaunchGraceDuration,
            float lostMomentumDuration,
            float lostMomentumPlanarSpeedThreshold,
            float lostMomentumProgressThreshold,
            float runEndedDelay)
        {
            _obstacleImpactSpeedThreshold = obstacleImpactSpeedThreshold;
            _lostMomentumLaunchGraceDuration = lostMomentumLaunchGraceDuration;
            _lostMomentumDuration = lostMomentumDuration;
            _lostMomentumPlanarSpeedThreshold = lostMomentumPlanarSpeedThreshold;
            _lostMomentumProgressThreshold = lostMomentumProgressThreshold;
            _runEndedDelay = runEndedDelay;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
