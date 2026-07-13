using System;
using Game.Gameplay.Slingshot;
using Game.Utils.Mathematics;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay
{
    public interface IRunProgressService
    {
        bool HasValidSnapshot { get; }
        string SnapshotError { get; }
        RunProgressFrameSnapshot Snapshot { get; }
        float CurrentForwardProgress { get; }
        float MaximumForwardProgress { get; }
        RunProgressSample CurrentSample { get; }
        bool TryBeginRun(Vector3 origin, out string error);
        void SamplePosition(Vector3 position);
        void Reset();
    }

    internal interface IRunProgressFixedStep
    {
        void SampleProgress();
    }

    internal sealed class RunProgressService : IRunProgressService, IRunProgressFixedStep, IInitializable, IDisposable
    {
        private readonly IRunProgressFrameSource _frameSource;
        private readonly IRunMotionSource _motionSource;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;

        private bool _isInitialized;
        private bool _isDisposed;

        public bool HasValidSnapshot { get; private set; }
        public string SnapshotError { get; private set; } = string.Empty;
        public RunProgressFrameSnapshot Snapshot { get; private set; }
        public float CurrentForwardProgress { get; private set; }
        public float MaximumForwardProgress { get; private set; }

        public RunProgressSample CurrentSample => new(
            HasValidSnapshot,
            SnapshotError,
            Snapshot,
            CurrentForwardProgress,
            MaximumForwardProgress);

        public RunProgressService(
            IRunProgressFrameSource frameSource,
            IRunMotionSource motionSource,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier)
        {
            _frameSource = frameSource ?? throw new ArgumentNullException(nameof(frameSource));
            _motionSource = motionSource ?? throw new ArgumentNullException(nameof(motionSource));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunProgressService));

            if (_isInitialized)
                return;

            _launchAppliedNotifier.LaunchApplied += OnSlingshotLaunchApplied;
            _isInitialized = true;
        }

        void IRunProgressFixedStep.SampleProgress()
        {
            if (_isDisposed || !HasValidSnapshot)
                return;

            SamplePosition(_motionSource.Position);
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_isInitialized)
                _launchAppliedNotifier.LaunchApplied -= OnSlingshotLaunchApplied;

            Reset();
        }

        public bool TryBeginRun(Vector3 origin, out string error)
        {
            Reset();

            if (!_frameSource.TryCreateSnapshot(origin, out var snapshot, out error))
            {
                SnapshotError = error;
                Debug.LogError("Invalid Run Progress Frame. " + error);
                return false;
            }

            Snapshot = snapshot;
            HasValidSnapshot = true;
            SamplePosition(origin);
            return true;
        }

        public void SamplePosition(Vector3 position)
        {
            if (!HasValidSnapshot)
                return;

            if (!position.IsFinite())
                return;

            CurrentForwardProgress = Snapshot.GetForwardProgress(position);
            MaximumForwardProgress = Mathf.Max(MaximumForwardProgress, CurrentForwardProgress);
        }

        public void Reset()
        {
            HasValidSnapshot = false;
            SnapshotError = string.Empty;
            Snapshot = default;
            CurrentForwardProgress = 0f;
            MaximumForwardProgress = 0f;
        }

        private void OnSlingshotLaunchApplied(SlingshotLaunchAppliedEvent launchApplied)
        {
            if (_isDisposed)
                return;

            TryBeginRun(_motionSource.Position, out _);
        }
    }
}
