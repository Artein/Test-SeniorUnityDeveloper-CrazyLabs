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
        [SerializeField, Min(0f),
         Tooltip("Controls: Contact-normal impact speed needed for Obstacle Impact contact to end the Run."
                 + "\n\nImpact: Higher values ignore softer obstacle bumps; lower values end the Run on gentler contact."
                 + "\n\nTypical: Meters per second along the collision normal. This is an Obstacle Impact threshold, not a movement speed cap.")]
        private float _obstacleImpactSpeedThreshold = 2.5f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Time after launch before Lost Momentum detection can end the Run."
                 + "\n\nImpact: Higher values protect early launch recovery for longer; lower values allow stall detection sooner."
                 + "\n\nTypical: Seconds. Use to avoid ending a Run during normal launch-to-run transition.")]
        private float _lostMomentumLaunchGraceDuration = 1.25f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Duration that low speed and low progress must persist before Lost Momentum ends the Run."
                 + "\n\nImpact: Higher values tolerate longer slowdowns; lower values end stalled runs faster."
                 + "\n\nTypical: Seconds. This distinguishes sustained Lost Momentum from momentary slowdown.")]
        private float _lostMomentumDuration = 1f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Planar speed threshold used by Lost Momentum detection."
                 + "\n\nImpact: Higher values classify more slow movement as stalled; lower values require the Run Body to be closer to stopped."
                 + "\n\nTypical: Meters per second. Evaluated together with progress threshold and duration.")]
        private float _lostMomentumPlanarSpeedThreshold = 0.35f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Minimum forward progress expected during Lost Momentum detection."
                 + "\n\nImpact: Higher values require more progress to stay alive; lower values tolerate slower forward gain."
                 + "\n\nTypical: Unity world meters of progress over the detection window.")]
        private float _lostMomentumProgressThreshold = 0.05f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Delay before post-result input can acknowledge a Run Ended state."
                 + "\n\nImpact: Higher values prevent accidental immediate acknowledgement for longer; lower values let the player dismiss results sooner."
                 + "\n\nTypical: Seconds after result presentation.")]
        private float _runEndedAcknowledgeGuardDuration = 0.25f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Coin reward rate earned from completed Run distance."
                 + "\n\nImpact: Higher values increase distance-driven currency rewards; lower values make distance less valuable."
                 + "\n\nTypical: Coins per Unity world meter. This affects Distance Bonus, not Run Distance Display.")]
        private float _distanceBonusCoinsPerMeter = 0.1f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Coin reward rate earned from unsupported Run air time."
                 + "\n\nImpact: Higher values reward ramps and airborne travel more; lower values make air time less valuable."
                 + "\n\nTypical: Coins per second of air time. This affects Air Time Bonus, not character presentation.")]
        private float _airTimeBonusCoinsPerSecond = 1f;

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
