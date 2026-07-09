using System;
using Game.Foundation.Time;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay
{
    internal sealed class RunSteeringAffordancePresenter : IInitializable, ITickable, IRunSteeringAffordanceView
    {
        private readonly IRunSteeringAffordancePresentationView _view;
        private readonly IRunSteeringAffordanceTuning _tuning;
        private readonly ITime _clock;

        private float _animationElapsedSeconds;
        private float _animationDurationSeconds;
        private float _animationStartAlpha;
        private float _animationEndAlpha;
        private float _animationStartScale;
        private float _animationEndScale;
        private float _currentAlpha;
        private float _currentScale;
        private bool _isAnimating;
        private bool _deactivateWhenAnimationCompletes;

        public RunSteeringAffordancePresenter(
            IRunSteeringAffordancePresentationView view,
            IRunSteeringAffordanceTuning tuning,
            ITime clock)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _currentScale = HiddenScale;
        }

        void IInitializable.Initialize()
        {
            Reset();
        }

        void ITickable.Tick()
        {
            if (!_isAnimating)
                return;

            _animationElapsedSeconds += Mathf.Max(0f, _clock.DeltaTime);

            var progress = _animationDurationSeconds > 0f
                ? Mathf.Clamp01(_animationElapsedSeconds / _animationDurationSeconds)
                : 1f;

            ApplyAnimation(
                Mathf.Lerp(_animationStartAlpha, _animationEndAlpha, progress),
                Mathf.Lerp(_animationStartScale, _animationEndScale, progress));

            if (progress >= 1f)
                CompleteAnimation();
        }

        void IRunSteeringAffordanceView.Show(RunSteeringAffordancePresentationState state)
        {
            if (!state.IsVisible)
            {
                Reset();
                return;
            }

            _view.Present(state);
            BeginAnimation(1f, 1f, HiddenScale, 1f, ShowDurationSeconds, false);
        }

        void IRunSteeringAffordanceView.Update(RunSteeringAffordancePresentationState state)
        {
            if (!state.IsVisible)
            {
                Reset();
                return;
            }

            _view.Present(state);
        }

        void IRunSteeringAffordanceView.Hide(RunSteeringAffordancePresentationState state)
        {
            if (state.IsVisible)
                _view.Present(state);

            BeginAnimation(
                _currentAlpha,
                0f,
                _currentScale,
                HiddenScale,
                HideDurationSeconds,
                true);
        }

        void IRunSteeringAffordanceView.Reset()
        {
            Reset();
        }

        private float HiddenScale => Mathf.Clamp01(_tuning.HiddenScale);
        private float ShowDurationSeconds => Mathf.Max(0f, _tuning.ShowDurationSeconds);
        private float HideDurationSeconds => Mathf.Max(0f, _tuning.HideDurationSeconds);

        private void Reset()
        {
            _isAnimating = false;
            _deactivateWhenAnimationCompletes = false;
            _animationElapsedSeconds = 0f;
            _animationDurationSeconds = 0f;
            ApplyAnimation(0f, HiddenScale);
            _view.Deactivate();
        }

        private void BeginAnimation(
            float startAlpha,
            float endAlpha,
            float startScale,
            float endScale,
            float durationSeconds,
            bool deactivateWhenAnimationCompletes)
        {
            _animationElapsedSeconds = 0f;
            _animationDurationSeconds = durationSeconds;
            _animationStartAlpha = startAlpha;
            _animationEndAlpha = endAlpha;
            _animationStartScale = startScale;
            _animationEndScale = endScale;
            _deactivateWhenAnimationCompletes = deactivateWhenAnimationCompletes;

            ApplyAnimation(startAlpha, startScale);

            if (durationSeconds <= 0f)
            {
                CompleteAnimation();
                return;
            }

            _isAnimating = true;
        }

        private void CompleteAnimation()
        {
            _isAnimating = false;
            ApplyAnimation(_animationEndAlpha, _animationEndScale);

            if (_deactivateWhenAnimationCompletes)
                _view.Deactivate();

            _deactivateWhenAnimationCompletes = false;
        }

        private void ApplyAnimation(float alpha, float scale)
        {
            _currentAlpha = Mathf.Clamp01(alpha);
            _currentScale = Mathf.Max(0f, scale);
            _view.ApplyAnimation(_currentAlpha, _currentScale);
        }
    }
}
