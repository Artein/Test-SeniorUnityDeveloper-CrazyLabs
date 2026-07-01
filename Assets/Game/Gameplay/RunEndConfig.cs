using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunEndConfig
    {
        float ObstacleImpactSpeedThreshold { get; }
        float LostMomentumLaunchGraceDuration { get; }
        float LostMomentumDuration { get; }
        float LostMomentumPlanarSpeedThreshold { get; }
        float LostMomentumProgressThreshold { get; }
        float RunEndedAcknowledgeGuardDuration { get; }
    }

    public interface IRunRewardConfig
    {
        float DistanceBonusCoinsPerMeter { get; }
        float AirTimeBonusCoinsPerSecond { get; }
    }

    [CreateAssetMenu(
        fileName = nameof(RunEndConfig),
        menuName = "Game/Gameplay/Run End Config")]
    public sealed partial class RunEndConfig : ScriptableObject, IRunEndConfig, IRunRewardConfig
    {
        [SerializeField, Min(0f)] private float _obstacleImpactSpeedThreshold = 2.5f;
        [SerializeField, Min(0f)] private float _lostMomentumLaunchGraceDuration = 1.25f;
        [SerializeField, Min(0f)] private float _lostMomentumDuration = 1f;
        [SerializeField, Min(0f)] private float _lostMomentumPlanarSpeedThreshold = 0.35f;
        [SerializeField, Min(0f)] private float _lostMomentumProgressThreshold = 0.05f;
        [SerializeField, Min(0f)] private float _runEndedAcknowledgeGuardDuration = 0.25f;
        [SerializeField, Min(0f)] private float _distanceBonusCoinsPerMeter = 0.1f;
        [SerializeField, Min(0f)] private float _airTimeBonusCoinsPerSecond = 1f;

        public float ObstacleImpactSpeedThreshold => Mathf.Max(0f, _obstacleImpactSpeedThreshold);
        public float LostMomentumLaunchGraceDuration => Mathf.Max(0f, _lostMomentumLaunchGraceDuration);
        public float LostMomentumDuration => Mathf.Max(0f, _lostMomentumDuration);
        public float LostMomentumPlanarSpeedThreshold => Mathf.Max(0f, _lostMomentumPlanarSpeedThreshold);
        public float LostMomentumProgressThreshold => Mathf.Max(0f, _lostMomentumProgressThreshold);
        public float RunEndedAcknowledgeGuardDuration => Mathf.Max(0f, _runEndedAcknowledgeGuardDuration);
        public float DistanceBonusCoinsPerMeter => Mathf.Max(0f, _distanceBonusCoinsPerMeter);
        public float AirTimeBonusCoinsPerSecond => Mathf.Max(0f, _airTimeBonusCoinsPerSecond);
    }
}
