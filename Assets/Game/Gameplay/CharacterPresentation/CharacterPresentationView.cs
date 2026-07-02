using SaintsField;
using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    public sealed class CharacterPresentationView : MonoBehaviour, ICharacterPresentationView, ICharacterPresentationTuning
    {
        [SerializeField] private Animator _animator;

        [SerializeField, AnimatorParam(nameof(_animator), AnimatorControllerParameterType.Int)]
        private string _modeParameterName = "PresentationMode";

        [SerializeField, AnimatorParam(nameof(_animator), AnimatorControllerParameterType.Float)]
        private string _playbackSpeedParameterName = "PlaybackSpeedMultiplier";

        [SerializeField, AnimatorParam(nameof(_animator), AnimatorControllerParameterType.Float)]
        private string _normalizedPullParameterName = "NormalizedPull";

        [SerializeField, AnimatorParam(nameof(_animator), AnimatorControllerParameterType.Float)]
        private string _normalizedLaunchPowerParameterName = "NormalizedLaunchPower";

        [SerializeField, AnimatorParam(nameof(_animator), AnimatorControllerParameterType.Float)]
        private string _normalizedPullOffsetParameterName = "NormalizedPullOffset";

        [SerializeField, AnimatorParam(nameof(_animator), AnimatorControllerParameterType.Float)]
        private string _normalizedLaunchOffsetParameterName = "NormalizedLaunchOffset";

        [SerializeField] private float _airborneDelaySeconds = 0.12f;
        [SerializeField] private float _meaningfulGroundedMovementThreshold = 0.5f;
        [SerializeField] private float _minimumLocomotionModeDuration = 0.35f;
        [SerializeField, Min(0f)] private float _launchPushMinimumSeconds = 0.25f;
        [SerializeField] private float _slideReferenceSpeed = 8f;
        [SerializeField] private float _minimumPlaybackSpeedMultiplier = 0.5f;
        [SerializeField] private float _maximumPlaybackSpeedMultiplier = 1.5f;

        public float AirborneDelaySeconds => Mathf.Max(0f, _airborneDelaySeconds);
        public float MeaningfulGroundedMovementThreshold => Mathf.Max(0f, _meaningfulGroundedMovementThreshold);
        public float MinimumLocomotionModeDuration => Mathf.Max(0f, _minimumLocomotionModeDuration);
        public float LaunchPushMinimumSeconds => Mathf.Max(0f, _launchPushMinimumSeconds);
        public float SlideReferenceSpeed => Mathf.Max(0.0001f, _slideReferenceSpeed);
        public float MinimumPlaybackSpeedMultiplier => Mathf.Max(0f, _minimumPlaybackSpeedMultiplier);
        public float MaximumPlaybackSpeedMultiplier => Mathf.Max(MinimumPlaybackSpeedMultiplier, _maximumPlaybackSpeedMultiplier);

        private void Awake()
        {
            EnsureAnimatorRootMotionDisabled();
        }

        private void OnValidate()
        {
            EnsureAnimatorRootMotionDisabled();
        }

        public void ApplyFrame(CharacterPresentationFrame frame)
        {
            if (_animator == null)
                return;

            EnsureAnimatorRootMotionDisabled();
            _animator.SetInteger(_modeParameterName, (int)frame.Mode);

            _animator.SetFloat(_playbackSpeedParameterName, Mathf.Clamp(
                frame.PlaybackSpeedMultiplier,
                MinimumPlaybackSpeedMultiplier,
                MaximumPlaybackSpeedMultiplier));
            _animator.SetFloat(_normalizedPullParameterName, frame.NormalizedPull);
            _animator.SetFloat(_normalizedLaunchPowerParameterName, frame.NormalizedLaunchPower);
            _animator.SetFloat(_normalizedPullOffsetParameterName, frame.NormalizedPullOffset);
            _animator.SetFloat(_normalizedLaunchOffsetParameterName, frame.NormalizedLaunchOffset);
        }

        private void EnsureAnimatorRootMotionDisabled()
        {
            if (_animator != null)
                _animator.applyRootMotion = false;
        }
    }
}
