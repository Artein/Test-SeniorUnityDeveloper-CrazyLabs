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
        private readonly ICharacterPresentationModeClassifier _classifier;
        private readonly ITime _clock;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly IRunMotionSource _motionSource;
        private readonly GameplayStateId _preLaunchStateId;
        private readonly IRunProgressService _progressService;
        private readonly GameplayStateId _runningStateId;
        private readonly GameplayStateId _runPreparationStateId;
        private readonly IRunResultNotifier _runResultNotifier;
        private readonly ISlingshotPresentationContextSource _slingshotPresentationContextSource;
        private readonly ICharacterPresentationSupportTracker _supportTracker;
        private readonly IRunSurfaceFrameSource _surfaceFrameSource;
        private readonly ICharacterPresentationTuning _tuning;
        private readonly ICharacterPresentationView _view;
        private readonly ICharacterVisualTargetPoseSource _visualTargetPoseSource;
        private bool _acceptedRunResultSucceeded;

        private CharacterPresentationMode _currentMode = CharacterPresentationMode.Idle;
        private float _currentModeElapsedSeconds;
        private bool _hasAcceptedRunResult;
        private bool _hasActiveLaunchFlight;
        private bool _hasConsumedLaunchPushForFlight;
        private bool _hasObservedPostLaunchUngrounded;
        private bool _hasPendingLaunchFlight;
        private bool _isDisposed;
        private bool _isInitialized;
        private float _launchFlightElapsedSeconds;
        private float _launchFlightNormalizedOffset;
        private float _launchFlightNormalizedPower;
        private bool _previousHasLaunchPush;
        private bool _suppressConsumedLaunchPushForClassification;

        public CharacterPresenter(
            IGameplayStateService gameplayStateService,
            IRunMotionSource motionSource,
            ICharacterVisualTargetPoseSource visualTargetPoseSource,
            IRunProgressService progressService,
            IRunSurfaceFrameSource surfaceFrameSource,
            ISlingshotPresentationContextSource slingshotPresentationContextSource,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            IRunResultNotifier runResultNotifier,
            ICharacterPresentationModeClassifier classifier,
            ICharacterPresentationSupportTracker supportTracker,
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
            _visualTargetPoseSource = visualTargetPoseSource ?? throw new ArgumentNullException(nameof(visualTargetPoseSource));
            _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            _surfaceFrameSource = surfaceFrameSource ?? throw new ArgumentNullException(nameof(surfaceFrameSource));

            _slingshotPresentationContextSource = slingshotPresentationContextSource
                                                  ?? throw new ArgumentNullException(nameof(slingshotPresentationContextSource));

            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _runResultNotifier = runResultNotifier ?? throw new ArgumentNullException(nameof(runResultNotifier));
            _classifier = classifier ?? throw new ArgumentNullException(nameof(classifier));
            _supportTracker = supportTracker ?? throw new ArgumentNullException(nameof(supportTracker));
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
            _launchAppliedNotifier.LaunchApplied += OnLaunchApplied;
            _isInitialized = true;
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _runResultNotifier.RunResultAccepted -= OnRunResultAccepted;
            _launchAppliedNotifier.LaunchApplied -= OnLaunchApplied;
        }

        void ITickable.Tick()
        {
            if (_isDisposed)
                return;

            var deltaTime = Mathf.Max(a: 0f, _clock.DeltaTime);
            var isRunPreparation = _gameplayStateService.IsCurrent(_runPreparationStateId);
            var isPreLaunch = _gameplayStateService.IsCurrent(_preLaunchStateId);
            var isNeutralPresentationState = isRunPreparation || isPreLaunch;
            var isRunActive = _gameplayStateService.IsCurrent(_runningStateId);
            var slingshotContext = _slingshotPresentationContextSource.Current;

            if (isNeutralPresentationState || isRunActive && !_hasAcceptedRunResult)
                ResetTerminalStateIfNeeded(isNeutralPresentationState);

            var surfaceContext = isNeutralPresentationState
                ? new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f)
                : ResolveObservedSurfaceContext(_surfaceFrameSource.Current.ObservedSupport);

            var currentPosition = _visualTargetPoseSource.CurrentPose.Position;
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

            var supportSample = _supportTracker.Update(
                surfaceContext,
                currentPosition,
                linearVelocity,
                courseUpDirection,
                _tuning,
                deltaTime,
                isNeutralPresentationState);

            var presentationSurfaceContext = supportSample.SurfaceContext;

            var hasLaunchFlight = UpdateLaunchFlightState(
                slingshotContext,
                presentationSurfaceContext,
                isRunPreparation,
                isPreLaunch,
                deltaTime);

            var hasLaunchPushForClassification = slingshotContext.HasLaunchPush && !_suppressConsumedLaunchPushForClassification;
            var launchPushElapsedSeconds = hasLaunchPushForClassification ? slingshotContext.LaunchPushElapsedSeconds : 0f;

            var input = new CharacterPresentationClassificationInput(
                _currentMode,
                _currentModeElapsedSeconds,
                supportSample.UngroundedElapsedSeconds,
                isPreLaunch,
                isRunActive,
                _hasAcceptedRunResult,
                _acceptedRunResultSucceeded,
                slingshotContext.HasActivePull,
                hasLaunchPushForClassification,
                hasLaunchFlight,
                launchPushElapsedSeconds,
                presentationSurfaceContext,
                coursePlanarSpeed,
                courseForwardSpeed,
                courseVerticalSpeed,
                supportSample.UngroundedVerticalSeparation,
                linearVelocity);

            var result = _classifier.Classify(input);
            var mode = NormalizePresentationMode(result.Mode);
            UpdateModeElapsed(mode, deltaTime);
            _view.ApplyFrame(CreateFrame(mode, coursePlanarSpeed, slingshotContext));
        }

        private void OnRunResultAccepted(RunResult result)
        {
            if (_isDisposed)
                return;

            ResetLaunchFlightState();
            _hasAcceptedRunResult = true;
            _acceptedRunResultSucceeded = result.IsSuccess;
        }

        private void OnLaunchApplied(SlingshotLaunchAppliedEvent launchApplied)
        {
            if (_isDisposed)
                return;

            QueueLaunchFlight(_slingshotPresentationContextSource.Current, launchApplied);
            ApplyImmediateLaunchFlightFrame();
        }

        private void ResetTerminalStateIfNeeded(bool isNeutralPresentationState)
        {
            if (!isNeutralPresentationState && _hasAcceptedRunResult)
                return;

            _hasAcceptedRunResult = false;
            _acceptedRunResultSucceeded = false;
        }

        private bool UpdateLaunchFlightState(
            SlingshotPresentationContext slingshotContext,
            RunSurfaceContext surfaceContext,
            bool isRunPreparation,
            bool isPreLaunch,
            float deltaTime)
        {
            var hasLaunchPush = slingshotContext.HasLaunchPush;

            if (isRunPreparation || _hasAcceptedRunResult)
            {
                ResetLaunchFlightState();
                _hasConsumedLaunchPushForFlight = false;
                _suppressConsumedLaunchPushForClassification = false;
                _previousHasLaunchPush = false;
                return false;
            }

            if (isPreLaunch)
            {
                if (!_hasPendingLaunchFlight)
                    ResetLaunchFlightState();

                _hasConsumedLaunchPushForFlight = false;
                _suppressConsumedLaunchPushForClassification = false;

                if (_hasPendingLaunchFlight && hasLaunchPush)
                    CacheLaunchValues(slingshotContext);

                _previousHasLaunchPush = hasLaunchPush;
                return false;
            }

            if (!hasLaunchPush)
            {
                _hasConsumedLaunchPushForFlight = false;
                _suppressConsumedLaunchPushForClassification = false;
            }

            var isNewLaunch = hasLaunchPush && !_previousHasLaunchPush;
            _previousHasLaunchPush = hasLaunchPush;

            var shouldStartLaunchFlight =
                _hasPendingLaunchFlight
                || isNewLaunch
                || hasLaunchPush && !_hasActiveLaunchFlight && !_hasConsumedLaunchPushForFlight;

            var startedLaunchFlight = false;

            if (shouldStartLaunchFlight)
            {
                StartLaunchFlight(slingshotContext);
                startedLaunchFlight = true;
            }

            if (_hasActiveLaunchFlight && hasLaunchPush)
                CacheLaunchValues(slingshotContext);

            if (!_hasActiveLaunchFlight)
                return false;

            if (!surfaceContext.IsGrounded)
            {
                _hasObservedPostLaunchUngrounded = true;
                return true;
            }

            if (startedLaunchFlight)
                return true;

            _launchFlightElapsedSeconds += Mathf.Max(a: 0f, deltaTime);

            if (_hasObservedPostLaunchUngrounded
                || _launchFlightElapsedSeconds >= Mathf.Max(a: 0f, _tuning.LaunchFlightMaximumGroundedWaitSeconds))
            {
                ResetLaunchFlightState(resetLaunchPushSuppression: false);
                _suppressConsumedLaunchPushForClassification = hasLaunchPush;
                return false;
            }

            return true;
        }

        private void QueueLaunchFlight(SlingshotPresentationContext slingshotContext, SlingshotLaunchAppliedEvent launchApplied)
        {
            _hasPendingLaunchFlight = true;
            _hasActiveLaunchFlight = false;
            _hasConsumedLaunchPushForFlight = false;
            _suppressConsumedLaunchPushForClassification = false;
            _hasObservedPostLaunchUngrounded = false;
            _launchFlightElapsedSeconds = 0f;
            CacheLaunchValues(slingshotContext, launchApplied);
        }

        private void ApplyImmediateLaunchFlightFrame()
        {
            UpdateModeElapsed(CharacterPresentationMode.LaunchFlight, deltaTime: 0f);

            _view.ApplyFrame(
                new CharacterPresentationFrame(
                    CharacterPresentationMode.LaunchFlight,
                    playbackSpeedMultiplier: 1f,
                    normalizedLaunchPower: _launchFlightNormalizedPower,
                    normalizedLaunchOffset: _launchFlightNormalizedOffset));
        }

        private void StartLaunchFlight(SlingshotPresentationContext slingshotContext)
        {
            _hasPendingLaunchFlight = false;
            _hasActiveLaunchFlight = true;
            _hasConsumedLaunchPushForFlight |= slingshotContext.HasLaunchPush;
            _suppressConsumedLaunchPushForClassification = false;
            _hasObservedPostLaunchUngrounded = false;
            _launchFlightElapsedSeconds = 0f;
            CacheLaunchValues(slingshotContext);
        }

        private void CacheLaunchValues(SlingshotPresentationContext slingshotContext)
        {
            _ = TryCacheLaunchValues(slingshotContext);
        }

        private void CacheLaunchValues(SlingshotPresentationContext slingshotContext, SlingshotLaunchAppliedEvent launchApplied)
        {
            if (TryCacheLaunchValues(slingshotContext))
                return;

            var request = launchApplied.Request;
            _launchFlightNormalizedPower = float.IsFinite(request.PullStrength) ? Mathf.Clamp01(request.PullStrength) : 0f;

            _launchFlightNormalizedOffset = float.IsFinite(request.NormalizedLateralPull)
                ? Mathf.Clamp(request.NormalizedLateralPull, min: -1f, max: 1f)
                : 0f;
        }

        private bool TryCacheLaunchValues(SlingshotPresentationContext slingshotContext)
        {
            if (slingshotContext.HasLaunchPush)
            {
                _launchFlightNormalizedPower = slingshotContext.NormalizedLaunchPower;
                _launchFlightNormalizedOffset = slingshotContext.NormalizedLaunchOffset;
                return true;
            }

            if (slingshotContext.HasActivePull)
            {
                _launchFlightNormalizedPower = slingshotContext.NormalizedPull;
                _launchFlightNormalizedOffset = slingshotContext.NormalizedPullOffset;
                return true;
            }

            return false;
        }

        private void ResetLaunchFlightState(bool resetLaunchPushSuppression = true)
        {
            _hasPendingLaunchFlight = false;
            _hasActiveLaunchFlight = false;
            _hasObservedPostLaunchUngrounded = false;
            _launchFlightElapsedSeconds = 0f;
            _launchFlightNormalizedPower = 0f;
            _launchFlightNormalizedOffset = 0f;

            if (resetLaunchPushSuppression)
                _suppressConsumedLaunchPushForClassification = false;
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

        private RunSurfaceContext ResolveObservedSurfaceContext(RunSupportObservation observation)
        {
            return observation.State == RunSupportObservationState.Supported
                ? observation.SurfaceContext
                : new RunSurfaceContext(isGrounded: false, Vector3.up, forwardDownhillDegrees: 0f);
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

        private CharacterPresentationMode NormalizePresentationMode(CharacterPresentationMode mode)
        {
            if (mode == CharacterPresentationMode.LaunchPush && (_hasPendingLaunchFlight || _hasActiveLaunchFlight))
                return CharacterPresentationMode.LaunchFlight;

            if (mode == CharacterPresentationMode.LaunchPush && _suppressConsumedLaunchPushForClassification)
                return CharacterPresentationMode.Slide;

            return NormalizeReservedPresentationMode(mode);
        }

        private static CharacterPresentationMode NormalizeReservedPresentationMode(CharacterPresentationMode mode)
        {
            return mode == CharacterPresentationMode.Run ? CharacterPresentationMode.Slide : mode;
        }
    }
}
