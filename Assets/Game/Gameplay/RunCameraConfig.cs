using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunCameraConfig
    {
        Vector3 AnchorOffset { get; }
        float PositionResponseRate { get; }
        float YawResponseRate { get; }
        float MinimumYawSpeed { get; }
    }

    [CreateAssetMenu(
        fileName = nameof(RunCameraConfig),
        menuName = "Game/Gameplay/Run Camera Config")]
    public sealed class RunCameraConfig : ScriptableObject, IRunCameraConfig
    {
        [SerializeField,
         Tooltip("Controls: Local meter offset applied to the followed body when positioning the Run Camera Anchor."
                 + "\n\nImpact: Higher Y raises the camera-facing target; forward or side offsets bias what the camera follows without changing gameplay movement."
                 + "\n\nTypical: Authored in Unity world meters. Keep close to the body unless the camera needs to look above or beside the character.")]
        private Vector3 _anchorOffset = new(0f, 1.2f, 0f);

        [SerializeField, Min(0f),
         Tooltip("Controls: How quickly the Run Camera Anchor position catches up to the target offset."
                 + "\n\nImpact: Lower values feel smoother and laggier; higher values feel tighter and more immediate."
                 + "\n\nTypical: Non-negative response rate. Zero holds the anchor at its current position.")]
        private float _positionResponseRate = 12f;

        [SerializeField, Min(0f),
         Tooltip("Controls: How quickly camera-facing yaw follows the run direction."
                 + "\n\nImpact: Lower values smooth direction changes; higher values make the camera-facing frame react sooner."
                 + "\n\nTypical: Non-negative response rate. Tune against position response so facing and follow motion feel consistent.")]
        private float _yawResponseRate = 12f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Minimum planar speed required before camera-facing yaw updates from run velocity."
                 + "\n\nImpact: Higher values hold the previous facing direction for longer near rest; lower values let small velocity noise steer yaw."
                 + "\n\nTypical: Meters per second. Keep low enough for slow movement, but high enough to ignore near-zero jitter.")]
        private float _minimumYawSpeed = 0.25f;

        public Vector3 AnchorOffset => _anchorOffset;
        public float PositionResponseRate => Mathf.Max(0f, _positionResponseRate);
        public float YawResponseRate => Mathf.Max(0f, _yawResponseRate);
        public float MinimumYawSpeed => Mathf.Max(0f, _minimumYawSpeed);

        public static class Serialization
        {
            public const string PositionResponseRate = nameof(_positionResponseRate);
            public const string YawResponseRate = nameof(_yawResponseRate);
            public const string MinimumYawSpeed = nameof(_minimumYawSpeed);
        }
    }
}
