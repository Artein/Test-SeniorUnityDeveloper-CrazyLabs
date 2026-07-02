using System;
using Game.Foundation.Time;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Utils.Mathematics;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay.CharacterPresentation
{
    internal sealed class CharacterPresenter : IInitializable, ITickable, IDisposable
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
        private readonly GameplayStateId _runPreparationStateId;
        private readonly GameplayStateId _preLaunchStateId;
        private readonly GameplayStateId _runningStateId;

        private CharacterPresentationMode _currentMode = CharacterPresentationMode.Idle;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasAcceptedRunResult;
        private bool _acceptedRunResultSucceeded;
        private float _currentModeElapsedSeconds;
        private float _ungroundedElapsedSeconds;
        private bool _hasUngroundedStartPosition;
        private Vector3 _ungroundedStartPosition;
        private bool _previousHasLaunchPush;
        private bool _awaitingPostLaunchLanding;
        private bool _hasObservedPostLaunchUngrounded;
        private float _launchFlightNormalizedPower;
        private float _launchFlightNormalizedOffset;

        public CharacterPresenter(
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
            [Key(InjectKey.GameplayStateId.RunPreparation)]
            GameplayStateId runPreparationStateId,
            [Key(InjectKey.GameplayStateId.PreLaunch)]
            GameplayStateId preLaunchStateId,
            [Key(InjectKey.GameplayStateId.Running)]
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

            _runPreparationStateId = runPreparationStateId != null
                ? runPreparationStateId
                : throw new ArgumentNullException(nameof(runPreparationStateId));
            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(CharacterPresenter));

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
            var isRunPreparation = _gameplayStateService.IsCurrent(_runPreparationStateId);
            var isPreLaunch = _gameplayStateService.IsCurrent(_preLaunchStateId);
            var isNeutralPresentationState = isRunPreparation || isPreLaunch;
            var isRunActive = _gameplayStateService.IsCurrent(_runningStateId);
            var slingshotContext = _slingshotPresentationContextSource.Current;

            if (isNeutralPresentationState || isRunActive && !_hasAcceptedRunResult)
                ResetTerminalStateIfNeeded(isNeutralPresentationState);

            var surfaceContext = isNeutralPresentationState
                ? new RunSurfaceContext(isGrounded: true, groundNormal: Vector3.up, forwardDownhillDegrees: 0f)
                : _surfaceContextSource.Current;
            var currentPosition = _motionSource.Position;
            var linearVelocity = _motionSource.LinearVelocity;
            var coursePlanarSpeed = 0f;
            var courseForwardSpeed = 0f;
            var courseVerticalSpeed = 0f;
            var courseUpDirection = Vector3.up;

            if (_progressService.HasValidSnapshot)
            {
                var snapshot = _progressService.Snapshot;
                coursePlanarSpeed = snapshot.GetCoursePlanarSpeed(linearVelocity);
                courseForwardSpeed = snapshot.GetCourseForwardSpeed(linearVelocity);
                courseUpDirection = snapshot.UpDirection;
            }

            courseVerticalSpeed = GetCourseVerticalSpeed(linearVelocity, courseUpDirection);

            var ungroundedVerticalSeparation = UpdateUngroundedState(
                surfaceContext,
                deltaTime,
                isNeutralPresentationState,
                currentPosition,
                courseUpDirection);
            var hasLaunchFlight = UpdateLaunchFlightState(slingshotContext, surfaceContext, isNeutralPresentationState);

            var input = new CharacterPresentationClassificationInput(_currentMode, _currentModeElapsedSeconds, _ungroundedElapsedSeconds, isPreLaunch,
                isRunActive, _hasAcceptedRunResult, _acceptedRunResultSucceeded, slingshotContext.HasActivePull, slingshotContext.HasLaunchPush,
                hasLaunchFlight, slingshotContext.LaunchPushElapsedSeconds, surfaceContext, coursePlanarSpeed, courseForwardSpeed,
                courseVerticalSpeed,
                ungroundedVerticalSeparation, linearVelocity);

            var result = _classifier.Classify(input);
            var mode = NormalizeReservedPresentationMode(result.Mode);
            UpdateModeElapsed(mode, deltaTime);
            _view.ApplyFrame(CreateFrame(mode, coursePlanarSpeed, slingshotContext));
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

        private void ResetTerminalStateIfNeeded(bool isNeutralPresentationState)
        {
            if (!isNeutralPresentationState && _hasAcceptedRunResult)
                return;

            _hasAcceptedRunResult = false;
            _acceptedRunResultSucceeded = false;
        }

        private float UpdateUngroundedState(
            RunSurfaceContext surfaceContext,
            float deltaTime,
            bool isNeutralPresentationState,
            Vector3 currentPosition,
            Vector3 courseUpDirection)
        {
            if (isNeutralPresentationState || surfaceContext.IsGrounded)
            {
                _ungroundedElapsedSeconds = 0f;
                _hasUngroundedStartPosition = false;
                _ungroundedStartPosition = Vector3.zero;
                return 0f;
            }

            if (!_hasUngroundedStartPosition)
            {
                _hasUngroundedStartPosition = true;
                _ungroundedStartPosition = GetFiniteOrZero(currentPosition);
            }

            _ungroundedElapsedSeconds += deltaTime;
            var positionDelta = GetFiniteOrZero(currentPosition) - _ungroundedStartPosition;
            var separation = Vector3.Dot(positionDelta, GetSafeUpDirection(courseUpDirection));
            return float.IsFinite(separation) ? separation : 0f;
        }

        private bool UpdateLaunchFlightState(
            SlingshotPresentationContext slingshotContext,
            RunSurfaceContext surfaceContext,
            bool isNeutralPresentationState)
        {
            var hasLaunchPush = slingshotContext.HasLaunchPush;
            var isNewLaunch = hasLaunchPush && !_previousHasLaunchPush;
            _previousHasLaunchPush = hasLaunchPush;

            if (isNeutralPresentationState || _hasAcceptedRunResult)
            {
                ResetLaunchFlightState();
                return false;
            }

            if (isNewLaunch)
            {
                _awaitingPostLaunchLanding = true;
                _hasObservedPostLaunchUngrounded = false;
                _launchFlightNormalizedPower = slingshotContext.NormalizedLaunchPower;
                _launchFlightNormalizedOffset = slingshotContext.NormalizedLaunchOffset;
            }

            if (!_awaitingPostLaunchLanding)
                return false;

            if (!surfaceContext.IsGrounded)
            {
                _hasObservedPostLaunchUngrounded = true;
                return true;
            }

            if (_hasObservedPostLaunchUngrounded)
                ResetLaunchFlightState();

            return false;
        }

        private void ResetLaunchFlightState()
        {
            _awaitingPostLaunchLanding = false;
            _hasObservedPostLaunchUngrounded = false;
            _launchFlightNormalizedPower = 0f;
            _launchFlightNormalizedOffset = 0f;
        }

        private static float GetCourseVerticalSpeed(Vector3 linearVelocity, Vector3 courseUpDirection)
        {
            var speed = Vector3.Dot(linearVelocity, GetSafeUpDirection(courseUpDirection));
            return float.IsFinite(speed) ? speed : 0f;
        }

        private static Vector3 GetSafeUpDirection(Vector3 upDirection)
        {
            if (!upDirection.IsFinite() || upDirection.sqrMagnitude <= 0.000001f)
                return Vector3.up;

            var normalized = upDirection.normalized;
            return normalized.IsFinite() ? normalized : Vector3.up;
        }

        private static Vector3 GetFiniteOrZero(Vector3 position)
        {
            return position.IsFinite() ? position : Vector3.zero;
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
            var normalizedMode = NormalizeReservedPresentationMode(mode);

            if (normalizedMode != CharacterPresentationMode.Slide)
                return 1f;

            var referenceSpeed = _tuning.SlideReferenceSpeed;

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

            if (mode == CharacterPresentationMode.LaunchFlight)
            {
                return new CharacterPresentationFrame(
                    mode,
                    playbackSpeedMultiplier: 1f,
                    normalizedLaunchPower: _launchFlightNormalizedPower,
                    normalizedLaunchOffset: _launchFlightNormalizedOffset);
            }

            return new CharacterPresentationFrame(mode, CalculatePlaybackSpeed(mode, coursePlanarSpeed));
        }

        private static CharacterPresentationMode NormalizeReservedPresentationMode(CharacterPresentationMode mode)
        {
            return mode == CharacterPresentationMode.Run ? CharacterPresentationMode.Slide : mode;
        }
    }
}
