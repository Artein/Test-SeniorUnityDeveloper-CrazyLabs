using UnityEngine;

namespace Game.Gameplay
{
    public interface IPlayerSteeringConfig
    {
        float SteeringDeadzone { get; }
        float SteeringSensitivity { get; }
        float SteeringResponseRate { get; }
        float MaximumTurnDegreesPerSecond { get; }
        float MinimumSteerSpeed { get; }
        float MaximumPlanarSpeed { get; }
    }

    [CreateAssetMenu(
        fileName = nameof(PlayerSteeringConfig),
        menuName = "Game/Gameplay/Player Steering Config")]
    public sealed class PlayerSteeringConfig : ScriptableObject, IPlayerSteeringConfig
    {
        [Header("Input Mapping")]
        [SerializeField, Range(0f, 0.95f)]
        [Tooltip("Ignored center-screen input as a fraction of half-screen width. 0.10 means small touches near center do not steer. Common range: 0.08-0.15.")]
        private float _steeringDeadzone = 0.1f;

        [SerializeField, Min(0f)]
        [Tooltip("Multiplier for horizontal pointer position after the deadzone. Lower values make left/right steering less sensitive. Common range: 0.65-0.80.")]
        private float _steeringSensitivity = 0.7f;

        [Header("Movement Response")]
        [SerializeField, Min(0f)]
        [Tooltip("How quickly current steering catches up to requested steering. Lower feels smoother and heavier; higher feels snappier. Common range: 5-8.")]
        private float _steeringResponseRate = 8f;

        [SerializeField, Min(0f)]
        [Tooltip("Maximum turn rate at full steer, in degrees per second. Caps how sharply velocity can rotate. Common range: 80-120.")]
        private float _maximumTurnDegreesPerSecond = 120f;

        [SerializeField, Min(0f)]
        [Tooltip("Minimum planar speed required before steering is applied. Prevents steering while nearly stopped. Common range: 0.20-0.50.")]
        private float _minimumSteerSpeed = 0.25f;

        [SerializeField, Min(0f)]
        [Tooltip("Maximum planar speed before steering clamps movement speed. This base value can be raised by max-speed upgrades. Common base range: 8-10.")]
        private float _maximumPlanarSpeed = 10f;

        public float SteeringDeadzone => _steeringDeadzone;
        public float SteeringSensitivity => _steeringSensitivity;
        public float SteeringResponseRate => _steeringResponseRate;
        public float MaximumTurnDegreesPerSecond => _maximumTurnDegreesPerSecond;
        public float MinimumSteerSpeed => _minimumSteerSpeed;
        public float MaximumPlanarSpeed => _maximumPlanarSpeed;
    }
}
