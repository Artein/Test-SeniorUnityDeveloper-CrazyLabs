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
        [SerializeField] private Vector3 _anchorOffset = new(0f, 1.2f, 0f);
        [SerializeField, Min(0f)] private float _positionResponseRate = 12f;
        [SerializeField, Min(0f)] private float _yawResponseRate = 12f;
        [SerializeField, Min(0f)] private float _minimumYawSpeed = 0.25f;

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
