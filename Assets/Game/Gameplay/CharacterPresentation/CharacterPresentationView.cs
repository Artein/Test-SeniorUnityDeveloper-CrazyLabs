using SaintsField;
using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    public sealed class CharacterPresentationView : MonoBehaviour,
        ICharacterPresentationView,
        ICharacterPresentationTuning,
        ICharacterVisualFollowView,
        ICharacterVisualFollowTuning
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _visualAnchor;

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

        [SerializeField] private float _fallEnterMinimumUngroundedSeconds = 0.3f;
        [SerializeField] private float _fallEnterMinimumDownwardSpeed = 1.5f;
        [SerializeField] private float _fallEnterMinimumVerticalSeparation = 0.18f;
        [SerializeField] private float _fallEnterHardUngroundedSeconds = 0.65f;
        [SerializeField] private float _meaningfulGroundedMovementThreshold = 0.5f;
        [SerializeField] private float _minimumLocomotionModeDuration = 0.35f;
        [SerializeField, Min(0f)] private float _launchPushMinimumSeconds = 0.25f;
        [SerializeField] private float _slideReferenceSpeed = 8f;
        [SerializeField] private float _minimumPlaybackSpeedMultiplier = 0.5f;
        [SerializeField] private float _maximumPlaybackSpeedMultiplier = 1.5f;
        [SerializeField] private float _visualPositionResponseRate = 60f;
        [SerializeField] private float _visualHeadingResponseRate = 45f;
        [SerializeField] private float _visualUpTiltResponseRate = 18f;
        [SerializeField] private float _visualMaxPositionLag = 0.06f;
        [SerializeField] private float _visualSnapDistance = 0.75f;
        [SerializeField] private float _visualSnapAngleDegrees = 45f;

        public float FallEnterMinimumUngroundedSeconds => Mathf.Max(0f, _fallEnterMinimumUngroundedSeconds);
        public float FallEnterMinimumDownwardSpeed => Mathf.Max(0f, _fallEnterMinimumDownwardSpeed);
        public float FallEnterMinimumVerticalSeparation => Mathf.Max(0f, _fallEnterMinimumVerticalSeparation);
        public float FallEnterHardUngroundedSeconds => Mathf.Max(FallEnterMinimumUngroundedSeconds, _fallEnterHardUngroundedSeconds);
        public float MeaningfulGroundedMovementThreshold => Mathf.Max(0f, _meaningfulGroundedMovementThreshold);
        public float MinimumLocomotionModeDuration => Mathf.Max(0f, _minimumLocomotionModeDuration);
        public float LaunchPushMinimumSeconds => Mathf.Max(0f, _launchPushMinimumSeconds);
        public float SlideReferenceSpeed => Mathf.Max(0.0001f, _slideReferenceSpeed);
        public float MinimumPlaybackSpeedMultiplier => Mathf.Max(0f, _minimumPlaybackSpeedMultiplier);
        public float MaximumPlaybackSpeedMultiplier => Mathf.Max(MinimumPlaybackSpeedMultiplier, _maximumPlaybackSpeedMultiplier);
        internal Transform VisualAnchorForTests => _visualAnchor;

        CharacterVisualPose ICharacterVisualFollowView.CurrentVisualPose
        {
            get
            {
                var visualAnchor = EffectiveVisualAnchor;
                return new CharacterVisualPose(visualAnchor.position, visualAnchor.rotation);
            }
        }

        float ICharacterVisualFollowTuning.VisualPositionResponseRate => Mathf.Max(0f, _visualPositionResponseRate);
        float ICharacterVisualFollowTuning.VisualHeadingResponseRate => Mathf.Max(0f, _visualHeadingResponseRate);
        float ICharacterVisualFollowTuning.VisualUpTiltResponseRate => Mathf.Max(0f, _visualUpTiltResponseRate);
        float ICharacterVisualFollowTuning.VisualMaxPositionLag => Mathf.Max(0f, _visualMaxPositionLag);
        float ICharacterVisualFollowTuning.VisualSnapDistance => Mathf.Max(0.0001f, _visualSnapDistance);
        float ICharacterVisualFollowTuning.VisualSnapAngleDegrees => Mathf.Clamp(_visualSnapAngleDegrees, 0f, 180f);

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

        void ICharacterVisualFollowView.ApplyVisualPose(CharacterVisualPose pose)
        {
            EffectiveVisualAnchor.SetPositionAndRotation(pose.Position, pose.Rotation);
        }

        private void EnsureAnimatorRootMotionDisabled()
        {
            if (_animator != null)
                _animator.applyRootMotion = false;
        }

        private Transform EffectiveVisualAnchor => _visualAnchor != null ? _visualAnchor : transform;
    }
}
