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

        [SerializeField] private float _airborneDelaySeconds = 0.12f;
        [SerializeField] private float _slideEnterDownhillDegrees = 6f;
        [SerializeField] private float _slideExitDownhillDegrees = 3f;
        [SerializeField] private float _runFlatMaximumAbsSlopeDegrees = 4f;
        [SerializeField] private float _runMinimumForwardSpeed = 0.5f;
        [SerializeField] private float _minimumLocomotionModeDuration = 0.15f;
        [SerializeField] private float _slideReferenceSpeed = 8f;
        [SerializeField] private float _runReferenceSpeed = 8f;
        [SerializeField] private float _minimumPlaybackSpeedMultiplier = 0.5f;
        [SerializeField] private float _maximumPlaybackSpeedMultiplier = 1.5f;

        public float AirborneDelaySeconds => Mathf.Max(0f, _airborneDelaySeconds);
        public float SlideEnterDownhillDegrees => Mathf.Max(0f, _slideEnterDownhillDegrees);
        public float SlideExitDownhillDegrees => Mathf.Clamp(_slideExitDownhillDegrees, 0f, SlideEnterDownhillDegrees);
        public float RunFlatMaximumAbsSlopeDegrees => Mathf.Max(0f, _runFlatMaximumAbsSlopeDegrees);
        public float RunMinimumForwardSpeed => Mathf.Max(0f, _runMinimumForwardSpeed);
        public float MinimumLocomotionModeDuration => Mathf.Max(0f, _minimumLocomotionModeDuration);
        public float SlideReferenceSpeed => Mathf.Max(0.0001f, _slideReferenceSpeed);
        public float RunReferenceSpeed => Mathf.Max(0.0001f, _runReferenceSpeed);
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
        }

        private void EnsureAnimatorRootMotionDisabled()
        {
            if (_animator != null)
                _animator.applyRootMotion = false;
        }
    }
}
