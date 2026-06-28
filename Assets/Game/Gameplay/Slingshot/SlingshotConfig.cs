using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotConfig
    {
        float TouchTargetRadiusPixels { get; }
        float MinimumPullDistance { get; }
        float MaximumPullDistance { get; }
        float MaximumLateralPull { get; }
        float MaximumLaunchAngleDegrees { get; }
        float MinimumLaunchSpeed { get; }
        float MaximumLaunchSpeed { get; }
        AnimationCurve LaunchSpeedCurve { get; }
        float LaunchUpSpeed { get; }
        float BandContactPadding { get; }
        int BandSilhouetteSampleCount { get; }
        int BandWrapSampleCount { get; }
        float BandRecoilDuration { get; }
        AnimationCurve BandRecoilCurve { get; }
    }

    [CreateAssetMenu(
        fileName = nameof(SlingshotConfig),
        menuName = "Game/Gameplay/Slingshot Config")]
    public sealed class SlingshotConfig : ScriptableObject, ISlingshotConfig
    {
        [SerializeField, Min(1f)] private float _touchTargetRadiusPixels = 80f;
        [SerializeField, Min(0f)] private float _minimumPullDistance = 0.25f;
        [SerializeField, Min(0.01f)] private float _maximumPullDistance = 3f;
        [SerializeField, Min(0f)] private float _maximumLateralPull = 1.5f;
        [SerializeField, Range(0f, 85f)] private float _maximumLaunchAngleDegrees = 35f;
        [SerializeField, Min(0f)] private float _minimumLaunchSpeed = 4f;
        [SerializeField, Min(0f)] private float _maximumLaunchSpeed = 12f;
        [SerializeField] private AnimationCurve _launchSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField, Min(0f)] private float _launchUpSpeed = 1.5f;
        [SerializeField, Min(0f)] private float _bandContactPadding = 0.05f;
        [SerializeField, Range(8, 64)] private int _bandSilhouetteSampleCount = 32;
        [SerializeField, Range(3, 31)] private int _bandWrapSampleCount = 13;
        [SerializeField, Min(0.01f)] private float _bandRecoilDuration = 0.18f;
        [SerializeField] private AnimationCurve _bandRecoilCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public float TouchTargetRadiusPixels => _touchTargetRadiusPixels;
        public float MinimumPullDistance => _minimumPullDistance;
        public float MaximumPullDistance => _maximumPullDistance;
        public float MaximumLateralPull => _maximumLateralPull;
        public float MaximumLaunchAngleDegrees => _maximumLaunchAngleDegrees;
        public float MinimumLaunchSpeed => _minimumLaunchSpeed;
        public float MaximumLaunchSpeed => _maximumLaunchSpeed;
        public AnimationCurve LaunchSpeedCurve => _launchSpeedCurve;
        public float LaunchUpSpeed => _launchUpSpeed;
        public float BandContactPadding => _bandContactPadding;
        public int BandSilhouetteSampleCount => _bandSilhouetteSampleCount;
        public int BandWrapSampleCount => _bandWrapSampleCount;
        public float BandRecoilDuration => _bandRecoilDuration;
        public AnimationCurve BandRecoilCurve => _bandRecoilCurve;

        private void OnValidate()
        {
            var validator = new SlingshotConfigValidator();
            var errors = validator.Validate(this);

            foreach (var error in errors)
            {
                Debug.LogWarning(error);
            }
        }
    }
}
