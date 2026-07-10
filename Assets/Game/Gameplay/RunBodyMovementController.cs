using System;
using Game.Foundation.Time;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    internal sealed class RunBodyMovementController : IInitializable, IFixedTickable, IDisposable
    {
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly IRunBodyMovementTarget _movementTarget;
        private readonly IRunSteeringInputSource _steeringInputSource;
        private readonly IRunBodySpeedEvaluator _speedEvaluator;
        private readonly IRunSteeringEvaluator _steeringEvaluator;
        private readonly IRunLaunchLandingStabilizer _launchLandingStabilizer;
        private readonly IRunSteeringFrameSource _steeringFrameSource;
        private readonly IRunSteeringFrameResetter _steeringFrameResetter;
        private readonly IRunSurfaceContextSource _surfaceContextSource;
        private readonly IRunProgressService _runProgressService;
        private readonly IRunGameplayStatResolver _runGameplayStatResolver;
        private readonly IRunBodySpeedDiagnosticsSink _speedDiagnosticsSink;
        private readonly IRunBodySpeedConfig _speedConfig;
        private readonly IRunBodyMovementValidityConfig _movementValidityConfig;
        private readonly IRunSteeringConfig _steeringConfig;
        private readonly RunBodySpeedEnvelopeValidator _speedEnvelopeValidator;
        private readonly ITime _clock;
        private readonly GameplayStatId _playerMaxSpeedStatId;
        private readonly GameplayStateId _runningStateId;
        private readonly IRunSteeringModeSelector _steeringModeSelector = new RunSteeringModeSelector();
        private readonly IRunBodyVelocitySanityGuard _velocitySanityGuard = new RunBodyVelocitySanityGuard();
        private readonly RunBodyLowSpeedAssistAttempt _lowSpeedAssistAttempt = new();
        private readonly float _minimumDirectionSqrMagnitude = 0.000001f;
        private readonly float _surfaceNormalLiftEpsilon = 0.0001f;

        private Vector3 _launchUpDirection = Vector3.up;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasLaunchApplied;
        private bool _isMovementActive;

        public RunBodyMovementController(
            IGameplayStateService gameplayStateService,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            IRunBodyMovementTarget movementTarget,
            IRunSteeringInputSource steeringInputSource,
            IRunBodySpeedEvaluator speedEvaluator,
            IRunSteeringEvaluator steeringEvaluator,
            IRunLaunchLandingStabilizer launchLandingStabilizer,
            IRunSteeringFrameSource steeringFrameSource,
            IRunSteeringFrameResetter steeringFrameResetter,
            IRunSurfaceContextSource surfaceContextSource,
            IRunProgressService runProgressService,
            IRunGameplayStatResolver runGameplayStatResolver,
            IRunBodySpeedDiagnosticsSink speedDiagnosticsSink,
            IRunBodySpeedConfig speedConfig,
            IRunBodyMovementValidityConfig movementValidityConfig,
            IRunSteeringConfig steeringConfig,
            RunBodySpeedEnvelopeValidator speedEnvelopeValidator,
            ITime clock,
            [Key(InjectKey.GameplayStatId.PlayerMaxSpeed)]
            GameplayStatId playerMaxSpeedStatId,
            [Key(InjectKey.GameplayStateId.Running)]
            GameplayStateId runningStateId)
        {
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _movementTarget = movementTarget ?? throw new ArgumentNullException(nameof(movementTarget));
            _steeringInputSource = steeringInputSource ?? throw new ArgumentNullException(nameof(steeringInputSource));
            _speedEvaluator = speedEvaluator ?? throw new ArgumentNullException(nameof(speedEvaluator));
            _steeringEvaluator = steeringEvaluator ?? throw new ArgumentNullException(nameof(steeringEvaluator));
            _launchLandingStabilizer = launchLandingStabilizer ?? throw new ArgumentNullException(nameof(launchLandingStabilizer));
            _steeringFrameSource = steeringFrameSource ?? throw new ArgumentNullException(nameof(steeringFrameSource));
            _steeringFrameResetter = steeringFrameResetter ?? throw new ArgumentNullException(nameof(steeringFrameResetter));
            _surfaceContextSource = surfaceContextSource ?? throw new ArgumentNullException(nameof(surfaceContextSource));
            _runProgressService = runProgressService ?? throw new ArgumentNullException(nameof(runProgressService));
            _runGameplayStatResolver = runGameplayStatResolver ?? throw new ArgumentNullException(nameof(runGameplayStatResolver));
            _speedDiagnosticsSink = speedDiagnosticsSink ?? throw new ArgumentNullException(nameof(speedDiagnosticsSink));
            _speedConfig = speedConfig ?? throw new ArgumentNullException(nameof(speedConfig));
            _movementValidityConfig = movementValidityConfig ?? throw new ArgumentNullException(nameof(movementValidityConfig));
            _steeringConfig = steeringConfig ?? throw new ArgumentNullException(nameof(steeringConfig));
            _speedEnvelopeValidator = speedEnvelopeValidator ?? throw new ArgumentNullException(nameof(speedEnvelopeValidator));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            _playerMaxSpeedStatId = playerMaxSpeedStatId != null
                ? playerMaxSpeedStatId
                : throw new ArgumentNullException(nameof(playerMaxSpeedStatId));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunBodyMovementController));

            if (_isInitialized)
                return;

            _launchAppliedNotifier.LaunchApplied += OnLaunchApplied;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;
        }

        void IFixedTickable.FixedTick()
        {
            if (_isDisposed || !_isMovementActive)
                return;

            var resolvedSoftMaximumSpeed = _runGameplayStatResolver.Resolve(
                _playerMaxSpeedStatId,
                _speedConfig.BaseSoftMaximumSpeed);
            _speedEnvelopeValidator.ValidateOrThrow(resolvedSoftMaximumSpeed);

            var fixedDeltaTime = Mathf.Max(0f, _clock.FixedDeltaTime);
            var inputState = _steeringInputSource.AdvanceAndRead(fixedDeltaTime);
            var rawVelocity = _movementTarget.LinearVelocity;

            var sanityResult = _velocitySanityGuard.Sanitize(
                rawVelocity,
                _movementValidityConfig.RunBodySpeedSanityGuardMetersPerSecond);
            var surfaceContext = _surfaceContextSource.Current;

            var correctedVelocity = _launchLandingStabilizer.Stabilize(
                new RunLaunchLandingStabilizationContext(
                    sanityResult.Velocity,
                    surfaceContext,
                    fixedDeltaTime));

            var speedContext = CreateSpeedContext(
                correctedVelocity,
                surfaceContext,
                resolvedSoftMaximumSpeed);
            var speedDecision = _speedEvaluator.Evaluate(speedContext);

            var steeringMode = _steeringModeSelector.Select(
                surfaceContext,
                correctedVelocity,
                ResolveMaximumSupportedSurfaceNormalLiftSpeed());
            var steeringUp = ResolveSteeringUp();

            var steeringDecision = _steeringEvaluator.Evaluate(
                new RunSteeringContext(
                    correctedVelocity,
                    steeringUp,
                    steeringMode,
                    inputState.SmoothedSteer,
                    ResolveMaximumTurnDegreesPerSecond(steeringMode),
                    _steeringConfig.MinimumSteerSpeed,
                    fixedDeltaTime,
                    inputState.IsGestureActive));

            var targetState = ComposeTargetState(
                correctedVelocity,
                surfaceContext,
                steeringMode,
                steeringUp,
                steeringDecision,
                speedContext,
                speedDecision,
                fixedDeltaTime,
                out var sampledTangentSpeed,
                out var hasUsableTangentDirection);

            _movementTarget.ApplyTargetState(targetState);

            PublishSpeedDiagnostics(
                surfaceContext,
                speedContext,
                speedDecision,
                sampledTangentSpeed,
                hasUsableTangentDirection);
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _launchAppliedNotifier.LaunchApplied -= OnLaunchApplied;
            _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
            ClearMovementLifecycle();
        }

        private void OnLaunchApplied(SlingshotLaunchAppliedEvent launchApplied)
        {
            if (_isDisposed)
                return;

            _hasLaunchApplied = true;
            _launchUpDirection = GetValidDirection(launchApplied.LaunchUpDirection, Vector3.up);
            _launchLandingStabilizer.ArmForLaunch();

            if (_gameplayStateService.IsCurrent(_runningStateId))
                ActivateMovement();
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (nextStateId == _runningStateId)
            {
                if (_hasLaunchApplied)
                    ActivateMovement();

                return;
            }

            _hasLaunchApplied = false;
            ClearMovementLifecycle();
        }

        private void ActivateMovement()
        {
            _steeringFrameResetter.Reset(_launchUpDirection);
            _lowSpeedAssistAttempt.RearmForNewRun();
            _speedDiagnosticsSink.Clear();
            _isMovementActive = true;
        }

        private void ClearMovementLifecycle()
        {
            _isMovementActive = false;
            _lowSpeedAssistAttempt.Clear();
            _speedDiagnosticsSink.Clear();
            _launchLandingStabilizer.Reset();
            _steeringFrameResetter.Clear();
        }

        private RunBodyMovementTargetState ComposeTargetState(
            Vector3 correctedVelocity,
            RunSurfaceContext surfaceContext,
            RunSteeringMode steeringMode,
            Vector3 steeringUp,
            RunSteeringDecision steeringDecision,
            RunBodySpeedContext speedContext,
            RunBodySpeedDecision speedDecision,
            float fixedDeltaTime,
            out float sampledTangentSpeed,
            out bool hasUsableTangentDirection)
        {
            var movementPlaneNormal = ResolveMovementPlaneNormal(
                surfaceContext,
                steeringMode,
                steeringUp);
            var normalVelocity = Vector3.Project(correctedVelocity, movementPlaneNormal);
            var tangentVelocity = correctedVelocity - normalVelocity;
            sampledTangentSpeed = tangentVelocity.magnitude;
            hasUsableTangentDirection = TryNormalize(tangentVelocity, out _);

            var tangentSpeed = ResolveTangentSpeed(
                sampledTangentSpeed,
                speedContext.HasValidGroundedRunSurface,
                hasUsableTangentDirection,
                speedDecision,
                fixedDeltaTime);

            var selectedIntent = steeringDecision.ShouldApplySteering
                ? steeringDecision.SteeringIntentDirection
                : tangentVelocity;

            var hasFinalDirection = TryProjectAndNormalize(
                selectedIntent,
                movementPlaneNormal,
                out var finalTangentDirection);

            if (!hasFinalDirection)
            {
                hasFinalDirection = TryProjectAndNormalize(
                    tangentVelocity,
                    movementPlaneNormal,
                    out finalTangentDirection);
            }

            var finalTangentVelocity = hasFinalDirection && float.IsFinite(tangentSpeed)
                ? finalTangentDirection * tangentSpeed
                : Vector3.zero;
            var finalVelocity = finalTangentVelocity + normalVelocity;
            var rotation = Quaternion.identity;

            var hasRotation = steeringDecision.ShouldApplySteering
                              && hasFinalDirection
                              && TryCreateFacing(finalTangentDirection, steeringUp, out rotation);

            return new RunBodyMovementTargetState(
                finalVelocity,
                hasRotation,
                rotation);
        }

        private void PublishSpeedDiagnostics(
            RunSurfaceContext surfaceContext,
            RunBodySpeedContext speedContext,
            RunBodySpeedDecision speedDecision,
            float sampledTangentSpeed,
            bool hasUsableTangentDirection)
        {
            var assistAttempt = _lowSpeedAssistAttempt.Snapshot;

            _speedDiagnosticsSink.Publish(new RunBodySpeedDiagnosticsSnapshot(
                RunBodySpeedDiagnosticsState.Active,
                surfaceContext.IsGrounded,
                speedContext.HasValidGroundedRunSurface,
                hasUsableTangentDirection,
                sampledTangentSpeed,
                speedDecision.SoftMaximumSpeed,
                speedContext.ForwardDownhillDegrees,
                speedContext.CourseForwardAlignment,
                speedDecision.Contributors,
                speedDecision.LowSpeedAssistTargetSpeed,
                assistAttempt.State,
                assistAttempt.IsEligible,
                assistAttempt.RemainingRequestedVelocityBudget));
        }

        private RunBodySpeedContext CreateSpeedContext(
            Vector3 correctedVelocity,
            RunSurfaceContext surfaceContext,
            float resolvedSoftMaximumSpeed)
        {
            var hasValidGroundedRunSurface = surfaceContext.IsGrounded
                                             && surfaceContext.HasValidGroundNormal
                                             && Vector3.Dot(correctedVelocity, surfaceContext.GroundNormal)
                                             <= ResolveMaximumSupportedSurfaceNormalLiftSpeed();

            var courseForwardAlignment = hasValidGroundedRunSurface
                ? ResolveCourseForwardAlignment(correctedVelocity, surfaceContext.GroundNormal)
                : 0f;

            return new RunBodySpeedContext(
                correctedVelocity,
                hasValidGroundedRunSurface,
                surfaceContext.GroundNormal,
                surfaceContext.ForwardDownhillDegrees,
                courseForwardAlignment,
                resolvedSoftMaximumSpeed);
        }

        private float ResolveMaximumSupportedSurfaceNormalLiftSpeed()
        {
            return _movementValidityConfig.MaximumSupportedSurfaceNormalLiftSpeed
                   + _surfaceNormalLiftEpsilon;
        }

        private float ResolveCourseForwardAlignment(
            Vector3 correctedVelocity,
            Vector3 surfaceNormal)
        {
            if (!_runProgressService.HasValidSnapshot
                || !TryProjectAndNormalize(correctedVelocity, surfaceNormal, out var tangentDirection)
                || !TryProjectAndNormalize(
                    _runProgressService.Snapshot.ForwardDirection,
                    surfaceNormal,
                    out var courseForwardDirection))
            {
                return 0f;
            }

            return Mathf.Clamp(Vector3.Dot(tangentDirection, courseForwardDirection), -1f, 1f);
        }

        private float ResolveTangentSpeed(
            float currentTangentSpeed,
            bool hasValidGroundedRunSurface,
            bool hasUsableTangentDirection,
            RunBodySpeedDecision speedDecision,
            float fixedDeltaTime)
        {
            if (!float.IsFinite(currentTangentSpeed))
                return 0f;

            var resolvedSpeed = currentTangentSpeed;
            var accelerationDelta = Mathf.Max(0f, speedDecision.TangentAcceleration) * fixedDeltaTime;

            if (resolvedSpeed < speedDecision.SoftMaximumSpeed)
            {
                resolvedSpeed = Mathf.Min(
                    resolvedSpeed + accelerationDelta,
                    speedDecision.SoftMaximumSpeed);
            }

            var dragDelta = Mathf.Max(0f, speedDecision.TangentDrag) * fixedDeltaTime;
            var naturallyIntegratedSpeed = Mathf.Max(0f, resolvedSpeed - dragDelta);

            var requestedAssistDelta = _lowSpeedAssistAttempt.Advance(
                new RunBodyLowSpeedAssistAttemptContext(
                    currentTangentSpeed,
                    naturallyIntegratedSpeed,
                    hasValidGroundedRunSurface,
                    hasUsableTangentDirection,
                    speedDecision.LowSpeedAssistTargetSpeed,
                    speedDecision.LowSpeedAssistAcceleration,
                    fixedDeltaTime));

            return naturallyIntegratedSpeed + requestedAssistDelta;
        }

        private Vector3 ResolveMovementPlaneNormal(
            RunSurfaceContext surfaceContext,
            RunSteeringMode steeringMode,
            Vector3 steeringUp)
        {
            if (steeringMode == RunSteeringMode.Grounded
                && surfaceContext.IsGrounded
                && surfaceContext.HasValidGroundNormal)
            {
                return GetValidDirection(surfaceContext.GroundNormal, steeringUp);
            }

            return steeringUp;
        }

        private Vector3 ResolveSteeringUp()
        {
            return GetValidDirection(
                _steeringFrameSource.GetUpDirection(_launchUpDirection),
                _launchUpDirection);
        }

        private float ResolveMaximumTurnDegreesPerSecond(RunSteeringMode steeringMode)
        {
            return steeringMode == RunSteeringMode.Air
                ? _steeringConfig.RunAirSteeringMaximumTurnDegreesPerSecond
                : _steeringConfig.MaximumTurnDegreesPerSecond;
        }

        private bool TryProjectAndNormalize(
            Vector3 direction,
            Vector3 planeNormal,
            out Vector3 projectedDirection)
        {
            return TryNormalize(Vector3.ProjectOnPlane(direction, planeNormal), out projectedDirection);
        }

        private bool TryCreateFacing(
            Vector3 forward,
            Vector3 up,
            out Quaternion rotation)
        {
            rotation = Quaternion.identity;

            if (!TryNormalize(forward, out var normalizedForward)
                || !TryNormalize(up, out var normalizedUp)
                || Vector3.Cross(normalizedForward, normalizedUp).sqrMagnitude <= _minimumDirectionSqrMagnitude)
            {
                return false;
            }

            rotation = Quaternion.LookRotation(normalizedForward, normalizedUp);

            return float.IsFinite(rotation.x)
                   && float.IsFinite(rotation.y)
                   && float.IsFinite(rotation.z)
                   && float.IsFinite(rotation.w);
        }

        private Vector3 GetValidDirection(Vector3 direction, Vector3 fallback)
        {
            return TryNormalize(direction, out var normalizedDirection)
                ? normalizedDirection
                : TryNormalize(fallback, out var normalizedFallback)
                    ? normalizedFallback
                    : Vector3.up;
        }

        private bool TryNormalize(Vector3 direction, out Vector3 normalizedDirection)
        {
            normalizedDirection = Vector3.zero;
            var sqrMagnitude = direction.sqrMagnitude;

            if (!float.IsFinite(sqrMagnitude) || sqrMagnitude <= _minimumDirectionSqrMagnitude)
                return false;

            var normalized = direction.normalized;

            if (!float.IsFinite(normalized.x)
                || !float.IsFinite(normalized.y)
                || !float.IsFinite(normalized.z))
            {
                return false;
            }

            normalizedDirection = normalized;
            return true;
        }
    }
}
