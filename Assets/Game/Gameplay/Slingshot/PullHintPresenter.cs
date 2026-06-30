using System;
using Game.Foundation.Time;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay.Slingshot
{
    public interface IPullHintView
    {
        void ShowAt(Vector2 screenPosition);
        void Play();
        void Hide();
    }

    public interface IPullHintTuning
    {
        float InitialIdleDelaySeconds { get; }
        float PlaybackDurationSeconds { get; }
        float RepeatCooldownSeconds { get; }
    }

    public sealed class PullHintPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly IPullHintView _view;
        private readonly IPullHintTuning _tuning;
        private readonly ITime _clock;
        private readonly ISlingshotCaptureLifecycleNotifier _captureLifecycleNotifier;
        private readonly ISlingshotActivePullNotifier _activePullNotifier;
        private readonly ISlingshotGeometrySnapshotSource _geometrySnapshotSource;
        private readonly ISlingshotInputProjector _inputProjector;

        private PullHintPlaybackPhase _phase = PullHintPlaybackPhase.WaitingForInitialDelay;
        private bool _captureEnabled;
        private bool _suppressedForCurrentCapture;
        private float _elapsedSeconds;
        private float InitialIdleDelaySeconds => Mathf.Max(0f, _tuning.InitialIdleDelaySeconds);
        private float PlaybackDurationSeconds => Mathf.Max(0f, _tuning.PlaybackDurationSeconds);
        private float RepeatCooldownSeconds => Mathf.Max(0f, _tuning.RepeatCooldownSeconds);

        public PullHintPresenter(
            IPullHintView view,
            IPullHintTuning tuning,
            ITime clock,
            ISlingshotCaptureLifecycleNotifier captureLifecycleNotifier,
            ISlingshotActivePullNotifier activePullNotifier,
            ISlingshotGeometrySnapshotSource geometrySnapshotSource,
            ISlingshotInputProjector inputProjector)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _captureLifecycleNotifier = captureLifecycleNotifier ?? throw new ArgumentNullException(nameof(captureLifecycleNotifier));
            _activePullNotifier = activePullNotifier ?? throw new ArgumentNullException(nameof(activePullNotifier));
            _geometrySnapshotSource = geometrySnapshotSource ?? throw new ArgumentNullException(nameof(geometrySnapshotSource));
            _inputProjector = inputProjector ?? throw new ArgumentNullException(nameof(inputProjector));
        }

        void IInitializable.Initialize()
        {
            _captureLifecycleNotifier.CaptureEnabled += OnCaptureEnabled;
            _captureLifecycleNotifier.CaptureDisabled += OnCaptureDisabled;
            _activePullNotifier.ActivePullChanged += OnActivePullChanged;
            _activePullNotifier.ActivePullCleared += OnActivePullCleared;

            _view.Hide();
        }

        void ITickable.Tick()
        {
            if (!_captureEnabled || _suppressedForCurrentCapture)
                return;

            _elapsedSeconds += Mathf.Max(0f, _clock.DeltaTime);

            switch (_phase)
            {
                case PullHintPlaybackPhase.WaitingForInitialDelay:
                    TryStartAfterDelay(InitialIdleDelaySeconds);
                    break;
                case PullHintPlaybackPhase.Playing:
                    CompletePlaybackAfterDuration();
                    break;
                case PullHintPlaybackPhase.WaitingForRepeatCooldown:
                    TryStartAfterDelay(RepeatCooldownSeconds);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported Pull Hint playback phase '{_phase}'.");
            }
        }

        void IDisposable.Dispose()
        {
            _captureLifecycleNotifier.CaptureEnabled -= OnCaptureEnabled;
            _captureLifecycleNotifier.CaptureDisabled -= OnCaptureDisabled;
            _activePullNotifier.ActivePullChanged -= OnActivePullChanged;
            _activePullNotifier.ActivePullCleared -= OnActivePullCleared;
        }

        private void TryStartAfterDelay(float requiredDelaySeconds)
        {
            if (_elapsedSeconds < requiredDelaySeconds)
                return;

            if (!TryStartPlayback())
                return;

            _phase = PullHintPlaybackPhase.Playing;
            _elapsedSeconds = 0f;
        }

        private bool TryStartPlayback()
        {
            var geometry = _geometrySnapshotSource.CurrentGeometry;

            if (!_inputProjector.TryProjectWorldToScreen(geometry.RestPoint, out var screenPosition))
                return false;

            _view.ShowAt(screenPosition);
            _view.Play();
            return true;
        }

        private void CompletePlaybackAfterDuration()
        {
            if (_elapsedSeconds < PlaybackDurationSeconds)
                return;

            _view.Hide();
            _phase = PullHintPlaybackPhase.WaitingForRepeatCooldown;
            _elapsedSeconds = 0f;
        }

        private void OnCaptureEnabled()
        {
            _captureEnabled = true;
            _suppressedForCurrentCapture = false;
            ResetPlayback(PullHintPlaybackPhase.WaitingForInitialDelay);
            _view.Hide();
        }

        private void OnCaptureDisabled()
        {
            _captureEnabled = false;
            _suppressedForCurrentCapture = false;
            ResetPlayback(PullHintPlaybackPhase.WaitingForInitialDelay);
            _view.Hide();
        }

        private void OnActivePullChanged(SlingshotActivePullContext context)
        {
            if (!_captureEnabled)
                return;

            _suppressedForCurrentCapture = true;
            ResetPlayback(PullHintPlaybackPhase.WaitingForInitialDelay);
            _view.Hide();
        }

        private void OnActivePullCleared()
        {
            if (_suppressedForCurrentCapture)
                _view.Hide();
        }

        private void ResetPlayback(PullHintPlaybackPhase phase)
        {
            _phase = phase;
            _elapsedSeconds = 0f;
        }

        private enum PullHintPlaybackPhase
        {
            WaitingForInitialDelay,
            Playing,
            WaitingForRepeatCooldown
        }
    }
}
