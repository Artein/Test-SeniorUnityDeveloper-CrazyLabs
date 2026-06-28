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
    }

    [CreateAssetMenu(
        fileName = nameof(PlayerSteeringConfig),
        menuName = "Game/Gameplay/Player Steering Config")]
    public sealed class PlayerSteeringConfig : ScriptableObject, IPlayerSteeringConfig
    {
        [SerializeField, Range(0f, 0.95f)] private float _steeringDeadzone = 0.1f;
        [SerializeField, Min(0f)] private float _steeringSensitivity = 1f;
        [SerializeField, Min(0f)] private float _steeringResponseRate = 8f;
        [SerializeField, Min(0f)] private float _maximumTurnDegreesPerSecond = 120f;
        [SerializeField, Min(0f)] private float _minimumSteerSpeed = 0.25f;

        public float SteeringDeadzone => _steeringDeadzone;
        public float SteeringSensitivity => _steeringSensitivity;
        public float SteeringResponseRate => _steeringResponseRate;
        public float MaximumTurnDegreesPerSecond => _maximumTurnDegreesPerSecond;
        public float MinimumSteerSpeed => _minimumSteerSpeed;
    }
}
