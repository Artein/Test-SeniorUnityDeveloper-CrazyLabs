using System;
using Game.Foundation.Time;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay.CharacterPresentation
{
    internal sealed class CharacterPresentationPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly IGameplayStateService _gameplayStateService;
        private readonly IRunMotionSource _motionSource;
        private readonly IRunProgressService _progressService;
        private readonly IRunSurfaceContextSource _surfaceContextSource;
        private readonly ISlingshotPresentationContextSource _slingshotPresentationContextSource;
        private readonly IRunResultNotifier _runResultNotifier;
        private readonly ICharacterPresentationModeClassifier _classifier;
        private readonly ICharacterPresentationView _view;
        private readonly ICharacterPresentationTuning _tuning;
        private readonly ITime _clock;
        private readonly GameplayStateId _preLaunchStateId;
        private readonly GameplayStateId _runningStateId;

        private CharacterPresentationMode _currentMode = CharacterPresentationMode.Idle;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasAcceptedRunResult;
        private bool _acceptedRunResultSucceeded;
        private float _currentModeElapsedSeconds;
        private float _ungroundedElapsedSeconds;

        public CharacterPresentationPresenter(
            IGameplayStateService gameplayStateService,
            IRunMotionSource motionSource,
            IRunProgressService progressService,
            IRunSurfaceContextSource surfaceContextSource,
            ISlingshotPresentationContextSource slingshotPresentationContextSource,
            IRunResultNotifier runResultNotifier,
            ICharacterPresentationModeClassifier classifier,
            ICharacterPresentationView view,
            ICharacterPresentationTuning tuning,
            ITime clock,
            GameplayStateId preLaunchStateId,
            GameplayStateId runningStateId)
        {
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _motionSource = motionSource ?? throw new ArgumentNullException(nameof(motionSource));
            _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            _surfaceContextSource = surfaceContextSource ?? throw new ArgumentNullException(nameof(surfaceContextSource));

            _slingshotPresentationContextSource = slingshotPresentationContextSource
                                                  ?? throw new ArgumentNullException(nameof(slingshotPresentationContextSource));
            _runResultNotifier = runResultNotifier ?? throw new ArgumentNullException(nameof(runResultNotifier));
            _classifier = classifier ?? throw new ArgumentNullException(nameof(classifier));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(CharacterPresentationPresenter));

            if (_isInitialized)
                return;

            _runResultNotifier.RunResultAccepted += OnRunResultAccepted;
            _isInitialized = true;
        }

        void ITickable.Tick()
        {
            if (_isDisposed)
                return;

            var deltaTime = Mathf.Max(0f, _clock.DeltaTime);
            var isPreLaunch = _gameplayStateService.IsCurrent(_preLaunchStateId);
            var isRunActive = _gameplayStateService.IsCurrent(_runningStateId);
            var slingshotContext = _slingshotPresentationContextSource.Current;

            if (isPreLaunch || isRunActive && !_hasAcceptedRunResult)
                ResetTerminalStateIfNeeded(isPreLaunch);

            var surfaceContext = isPreLaunch
                ? new RunSurfaceContext(isGrounded: true, groundNormal: Vector3.up, forwardDownhillDegrees: 0f)
                : _surfaceContextSource.Current;
            UpdateUngroundedTime(surfaceContext, deltaTime, isPreLaunch);

            var linearVelocity = _motionSource.LinearVelocity;
            var coursePlanarSpeed = 0f;
            var courseForwardSpeed = 0f;

            if (_progressService.HasValidSnapshot)
            {
                coursePlanarSpeed = _progressService.Snapshot.GetCoursePlanarSpeed(linearVelocity);
                courseForwardSpeed = _progressService.Snapshot.GetCourseForwardSpeed(linearVelocity);
            }

            var input = new CharacterPresentationClassificationInput(_currentMode, _currentModeElapsedSeconds, _ungroundedElapsedSeconds, isPreLaunch,
                isRunActive, _hasAcceptedRunResult, _acceptedRunResultSucceeded, slingshotContext.HasActivePull, slingshotContext.HasLaunchPush,
                slingshotContext.LaunchPushElapsedSeconds, surfaceContext, coursePlanarSpeed, courseForwardSpeed, linearVelocity);

            var result = _classifier.Classify(input);
            UpdateModeElapsed(result.Mode, deltaTime);
            _view.ApplyFrame(CreateFrame(result.Mode, coursePlanarSpeed, slingshotContext));
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_isInitialized)
                _runResultNotifier.RunResultAccepted -= OnRunResultAccepted;
        }

        private void OnRunResultAccepted(RunResult result)
        {
            if (_isDisposed)
                return;

            _hasAcceptedRunResult = true;
            _acceptedRunResultSucceeded = result.IsSuccess;
        }

        private void ResetTerminalStateIfNeeded(bool isPreLaunch)
        {
            if (!isPreLaunch && _hasAcceptedRunResult)
                return;

            _hasAcceptedRunResult = false;
            _acceptedRunResultSucceeded = false;
        }

        private void UpdateUngroundedTime(RunSurfaceContext surfaceContext, float deltaTime, bool isPreLaunch)
        {
            if (isPreLaunch || surfaceContext.IsGrounded)
            {
                _ungroundedElapsedSeconds = 0f;
                return;
            }

            _ungroundedElapsedSeconds += deltaTime;
        }

        private void UpdateModeElapsed(CharacterPresentationMode nextMode, float deltaTime)
        {
            if (nextMode == _currentMode)
            {
                _currentModeElapsedSeconds += deltaTime;
                return;
            }

            _currentMode = nextMode;
            _currentModeElapsedSeconds = 0f;
        }

        private float CalculatePlaybackSpeed(CharacterPresentationMode mode, float coursePlanarSpeed)
        {
            if (mode != CharacterPresentationMode.Slide && mode != CharacterPresentationMode.Run)
                return 1f;

            var referenceSpeed = mode == CharacterPresentationMode.Slide
                ? _tuning.SlideReferenceSpeed
                : _tuning.RunReferenceSpeed;

            if (referenceSpeed <= 0f)
                return 1f;

            return Mathf.Clamp(
                coursePlanarSpeed / referenceSpeed,
                _tuning.MinimumPlaybackSpeedMultiplier,
                _tuning.MaximumPlaybackSpeedMultiplier);
        }

        private CharacterPresentationFrame CreateFrame(
            CharacterPresentationMode mode,
            float coursePlanarSpeed,
            SlingshotPresentationContext slingshotContext)
        {
            if (mode == CharacterPresentationMode.PullAnticipation)
            {
                return new CharacterPresentationFrame(
                    mode,
                    playbackSpeedMultiplier: 1f,
                    slingshotContext.NormalizedPull,
                    normalizedPullOffset: slingshotContext.NormalizedPullOffset);
            }

            if (mode == CharacterPresentationMode.LaunchPush)
            {
                return new CharacterPresentationFrame(
                    mode,
                    playbackSpeedMultiplier: 1f,
                    normalizedLaunchPower: slingshotContext.NormalizedLaunchPower,
                    normalizedLaunchOffset: slingshotContext.NormalizedLaunchOffset);
            }

            return new CharacterPresentationFrame(mode, CalculatePlaybackSpeed(mode, coursePlanarSpeed));
        }
    }
}
