using System;
using Game.Foundation.Time;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    internal sealed class LostMomentumDetector : IInitializable, IFixedTickable, IDisposable
    {
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly IRunProgressService _progressService;
        private readonly IRunMotionSource _motionSource;
        private readonly IRunEndCandidateReceiver _candidateReceiver;
        private readonly IRunEndConfig _config;
        private readonly ITime _clock;
        private readonly GameplayStateId _runningStateId;

        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasLaunchApplied;
        private bool _isActive;
        private bool _hasSubmittedCandidate;
        private float _elapsedSinceLaunch;
        private float _lowMomentumElapsed;
        private float _windowStartMaximumProgress;

        public LostMomentumDetector(
            IGameplayStateService gameplayStateService,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            IRunProgressService progressService,
            IRunMotionSource motionSource,
            IRunEndCandidateReceiver candidateReceiver,
            IRunEndConfig config,
            ITime clock,
            [Key(InjectKey.GameplayStateId.Running)]
            GameplayStateId runningStateId)
        {
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            _motionSource = motionSource ?? throw new ArgumentNullException(nameof(motionSource));
            _candidateReceiver = candidateReceiver ?? throw new ArgumentNullException(nameof(candidateReceiver));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(LostMomentumDetector));

            if (_isInitialized)
                return;

            _launchAppliedNotifier.LaunchApplied += OnSlingshotLaunchApplied;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;
        }

        void IFixedTickable.FixedTick()
        {
            if (_isDisposed || !_isActive || _hasSubmittedCandidate)
                return;

            var fixedDeltaTime = Math.Max(0f, _clock.FixedDeltaTime);
            _elapsedSinceLaunch += fixedDeltaTime;
            var progressSample = _progressService.CurrentSample;

            if (!progressSample.HasValidSnapshot || _elapsedSinceLaunch < _config.LostMomentumLaunchGraceDuration)
            {
                ResetLowMomentumWindow();
                return;
            }

            if (!IsLowMomentum(progressSample))
            {
                ResetLowMomentumWindow();
                return;
            }

            _lowMomentumElapsed += fixedDeltaTime;

            if (_lowMomentumElapsed < _config.LostMomentumDuration)
                return;

            _hasSubmittedCandidate = true;
            _candidateReceiver.SubmitCandidate(new RunEndCandidate(RunEndReason.LostMomentum));
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_isInitialized)
            {
                _launchAppliedNotifier.LaunchApplied -= OnSlingshotLaunchApplied;
                _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
            }

            Deactivate();
        }

        private void OnSlingshotLaunchApplied(SlingshotLaunchAppliedEvent launchApplied)
        {
            if (_isDisposed)
                return;

            _hasLaunchApplied = true;
            _elapsedSinceLaunch = 0f;
            _hasSubmittedCandidate = false;
            ResetLowMomentumWindow();

            if (_gameplayStateService.IsCurrent(_runningStateId))
                Activate();
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (ReferenceEquals(nextStateId, _runningStateId))
            {
                if (_hasLaunchApplied)
                    Activate();

                return;
            }

            _hasLaunchApplied = false;
            Deactivate();
        }

        private void Activate()
        {
            _isActive = true;
            ResetLowMomentumWindow();
        }

        private void Deactivate()
        {
            _isActive = false;
            _elapsedSinceLaunch = 0f;
            _lowMomentumElapsed = 0f;
            _windowStartMaximumProgress = 0f;
        }

        private bool IsLowMomentum(RunProgressSample progressSample)
        {
            var planarSpeed = progressSample.Snapshot.GetCoursePlanarSpeed(_motionSource.LinearVelocity);
            var progressDelta = progressSample.MaximumForwardProgress - _windowStartMaximumProgress;

            return planarSpeed <= _config.LostMomentumPlanarSpeedThreshold
                   && progressDelta <= _config.LostMomentumProgressThreshold;
        }

        private void ResetLowMomentumWindow()
        {
            _lowMomentumElapsed = 0f;
            _windowStartMaximumProgress = _progressService.CurrentSample.MaximumForwardProgress;
        }
    }
}
