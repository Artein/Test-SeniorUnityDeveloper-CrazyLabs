using System;
using Game.Foundation.Time;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay.Slingshot
{
    public sealed class SlingshotPresentationContextSource : IInitializable, ITickable, IDisposable, ISlingshotPresentationContextSource
    {
        private readonly ISlingshotActivePullNotifier _activePullNotifier;
        private readonly ISlingshotCaptureLifecycleNotifier _captureLifecycleNotifier;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly ISlingshotGeometrySnapshotSource _geometrySnapshotSource;
        private readonly ISlingshotPullOffsetNormalizer _pullOffsetNormalizer;
        private readonly ITime _clock;

        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasActivePull;
        private float _normalizedPull;
        private float _normalizedPullOffset;
        private bool _hasLaunchPush;
        private float _launchPushElapsedSeconds;
        private float _normalizedLaunchPower;
        private float _normalizedLaunchOffset;

        public SlingshotPresentationContext Current { get; private set; }

        public SlingshotPresentationContextSource(
            ISlingshotActivePullNotifier activePullNotifier,
            ISlingshotCaptureLifecycleNotifier captureLifecycleNotifier,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            ISlingshotGeometrySnapshotSource geometrySnapshotSource,
            ISlingshotPullOffsetNormalizer pullOffsetNormalizer,
            ITime clock)
        {
            _activePullNotifier = activePullNotifier ?? throw new ArgumentNullException(nameof(activePullNotifier));
            _captureLifecycleNotifier = captureLifecycleNotifier ?? throw new ArgumentNullException(nameof(captureLifecycleNotifier));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _geometrySnapshotSource = geometrySnapshotSource ?? throw new ArgumentNullException(nameof(geometrySnapshotSource));
            _pullOffsetNormalizer = pullOffsetNormalizer ?? throw new ArgumentNullException(nameof(pullOffsetNormalizer));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SlingshotPresentationContextSource));

            if (_isInitialized)
                return;

            _activePullNotifier.ActivePullChanged += OnActivePullChanged;
            _activePullNotifier.ActivePullCleared += OnActivePullCleared;
            _captureLifecycleNotifier.CaptureEnabled += OnCaptureEnabled;
            _captureLifecycleNotifier.CaptureDisabled += OnCaptureDisabled;
            _launchAppliedNotifier.LaunchApplied += OnLaunchApplied;
            _isInitialized = true;
            UpdateCurrent();
        }

        void ITickable.Tick()
        {
            if (_isDisposed || !_hasLaunchPush)
                return;

            _launchPushElapsedSeconds += Mathf.Max(0f, _clock.DeltaTime);
            UpdateCurrent();
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _activePullNotifier.ActivePullChanged -= OnActivePullChanged;
            _activePullNotifier.ActivePullCleared -= OnActivePullCleared;
            _captureLifecycleNotifier.CaptureEnabled -= OnCaptureEnabled;
            _captureLifecycleNotifier.CaptureDisabled -= OnCaptureDisabled;
            _launchAppliedNotifier.LaunchApplied -= OnLaunchApplied;
        }

        private void OnActivePullChanged(SlingshotActivePullContext context)
        {
            _hasActivePull = true;
            _normalizedPull = context.NormalizedPull;
            _normalizedPullOffset = context.NormalizedPullOffset;
            UpdateCurrent();
        }

        private void OnActivePullCleared()
        {
            ClearActivePull();
            UpdateCurrent();
        }

        private void OnCaptureEnabled()
        {
            ClearActivePull();
            ClearLaunchPush();
            UpdateCurrent();
        }

        private void OnCaptureDisabled()
        {
            ClearActivePull();
            UpdateCurrent();
        }

        private void OnLaunchApplied(SlingshotLaunchAppliedEvent launchApplied)
        {
            var request = launchApplied.Request;

            ClearActivePull();
            _hasLaunchPush = true;
            _launchPushElapsedSeconds = 0f;
            _normalizedLaunchPower = request.PullStrength;

            _normalizedLaunchOffset =
                _pullOffsetNormalizer.Normalize(_geometrySnapshotSource.CurrentGeometry, request.PullDistance, request.PullOffset);
            UpdateCurrent();
        }

        private void ClearActivePull()
        {
            _hasActivePull = false;
            _normalizedPull = 0f;
            _normalizedPullOffset = 0f;
        }

        private void ClearLaunchPush()
        {
            _hasLaunchPush = false;
            _launchPushElapsedSeconds = 0f;
            _normalizedLaunchPower = 0f;
            _normalizedLaunchOffset = 0f;
        }

        private void UpdateCurrent()
        {
            Current = new SlingshotPresentationContext(
                _hasActivePull,
                _normalizedPull,
                _normalizedPullOffset,
                _hasLaunchPush,
                _launchPushElapsedSeconds,
                _normalizedLaunchPower,
                _normalizedLaunchOffset);
        }
    }
}
