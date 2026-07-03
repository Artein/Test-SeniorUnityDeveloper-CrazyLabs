using System;
using Game.Foundation.Input;
using Game.Foundation.Screen;
using Game.Foundation.Time;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    internal sealed class PlayerSteeringController : IInitializable, IFixedTickable, IDisposable
    {
        private readonly IUnityInput _unityInput;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly IPlayerSteeringTarget _steeringTarget;
        private readonly IRunSteeringFrameSource _steeringFrameSource;
        private readonly IRunSteeringFrameResetter _steeringFrameResetter;
        private readonly IRunSurfaceContextSource _surfaceContextSource;
        private readonly IPlayerSteeringConfig _config;
        private readonly IRunGameplayStatResolver _statResolver;
        private readonly ITime _clock;
        private readonly IScreen _screen;
        private readonly IRunSteeringGesture _runSteeringGesture;
        private readonly IRunSteeringModeSelector _steeringModeSelector = new RunSteeringModeSelector();
        private readonly IRunBodyVelocitySanityGuard _velocitySanityGuard = new RunBodyVelocitySanityGuard();
        private readonly GameplayStateId _runningStateId;
        private readonly GameplayStatId _playerSteeringResponsivenessStatId;
        private readonly float _velocityChangeSqrTolerance = 0.00000001f;
        private readonly float _minimumAirSteerMagnitude = 0.0001f;

        private IDisposable _inputEnableHandle;
        private Vector3 _steeringUp = Vector3.up;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasLaunchApplied;
        private bool _isSteeringActive;
        private bool _isLaunchLandingStabilizationArmed;
        private bool _isLaunchLandingStabilizationActive;
        private bool _hasObservedPostLaunchUngroundedSurface;
        private float _desiredSteer;
        private float _currentSteer;
        private float _launchLandingStabilizationElapsedSeconds;

        public PlayerSteeringController(
            IUnityInput unityInput,
            IGameplayStateService gameplayStateService,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            IPlayerSteeringTarget steeringTarget,
            IRunSteeringFrameSource steeringFrameSource,
            IRunSteeringFrameResetter steeringFrameResetter,
            IRunSurfaceContextSource surfaceContextSource,
            IPlayerSteeringConfig config,
            IRunGameplayStatResolver statResolver,
            ITime clock,
            IScreen screen,
            IRunSteeringGesture runSteeringGesture,
            [Key(InjectKey.GameplayStateId.Running)]
            GameplayStateId runningStateId,
            [Key(InjectKey.GameplayStatId.PlayerSteeringResponsiveness)]
            GameplayStatId playerSteeringResponsivenessStatId)
        {
            _unityInput = unityInput ?? throw new ArgumentNullException(nameof(unityInput));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _steeringTarget = steeringTarget ?? throw new ArgumentNullException(nameof(steeringTarget));
            _steeringFrameSource = steeringFrameSource ?? throw new ArgumentNullException(nameof(steeringFrameSource));
            _steeringFrameResetter = steeringFrameResetter ?? throw new ArgumentNullException(nameof(steeringFrameResetter));
            _surfaceContextSource = surfaceContextSource ?? throw new ArgumentNullException(nameof(surfaceContextSource));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _statResolver = statResolver ?? throw new ArgumentNullException(nameof(statResolver));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
            _runSteeringGesture = runSteeringGesture ?? throw new ArgumentNullException(nameof(runSteeringGesture));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));

            _playerSteeringResponsivenessStatId = playerSteeringResponsivenessStatId != null
                ? playerSteeringResponsivenessStatId
                : throw new ArgumentNullException(nameof(playerSteeringResponsivenessStatId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PlayerSteeringController));

            if (_isInitialized)
                return;

            _launchAppliedNotifier.LaunchApplied += OnSlingshotLaunchApplied;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _unityInput.PointerPressed += OnInputPointerPressed;
            _unityInput.PointerMoved += OnInputPointerMoved;
            _unityInput.PointerReleased += OnInputPointerReleased;
            _unityInput.PointerCanceled += OnInputPointerCanceled;
            _isInitialized = true;
        }

        void IFixedTickable.FixedTick()
        {
            if (_isDisposed || !_isSteeringActive)
                return;

            UpdateSmoothedSteer();
            ApplySteering();
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
                _unityInput.PointerPressed -= OnInputPointerPressed;
                _unityInput.PointerMoved -= OnInputPointerMoved;
                _unityInput.PointerReleased -= OnInputPointerReleased;
                _unityInput.PointerCanceled -= OnInputPointerCanceled;
            }

            DeactivateSteering();
        }

        private void OnSlingshotLaunchApplied(SlingshotLaunchAppliedEvent launchApplied)
        {
            if (_isDisposed)
                return;

            _hasLaunchApplied = true;
            _steeringUp = GetValidUpDirection(launchApplied.LaunchUpDirection);
            ArmLaunchLandingStabilization();

            if (_gameplayStateService.IsCurrent(_runningStateId))
                ActivateSteering();
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (nextStateId == _runningStateId)
            {
                if (_hasLaunchApplied)
                    ActivateSteering();

                return;
            }

            _hasLaunchApplied = false;
            ClearLaunchLandingStabilization();
            DeactivateSteering();
        }

        private void ActivateSteering()
        {
            if (_isSteeringActive)
                return;

            _steeringFrameResetter.Reset(_steeringUp);
            _inputEnableHandle = _unityInput.Enable();
            _isSteeringActive = true;
            ResetPointerAndSteerState();
        }

        private void DeactivateSteering()
        {
            ResetPointerAndSteerState();
            _isSteeringActive = false;

            var inputEnableHandle = _inputEnableHandle;

            if (inputEnableHandle is null)
                return;

            _inputEnableHandle = null;
            inputEnableHandle.Dispose();
        }

        private void ResetPointerAndSteerState()
        {
            _runSteeringGesture.Reset();
            _desiredSteer = 0f;
            _currentSteer = 0f;
        }

        private void OnInputPointerPressed(PointerInput pointerInput)
        {
            if (!_isSteeringActive)
                return;

            if (_runSteeringGesture.TryBegin(pointerInput, _screen.Dpi))
                _desiredSteer = _runSteeringGesture.RequestedSteering;
        }

        private void OnInputPointerMoved(PointerInput pointerInput)
        {
            if (!_isSteeringActive)
                return;

            if (_runSteeringGesture.TryMove(pointerInput))
                _desiredSteer = _runSteeringGesture.RequestedSteering;
        }

        private void OnInputPointerReleased(PointerInput pointerInput)
        {
            if (!_isSteeringActive)
                return;

            if (_runSteeringGesture.TryRelease(pointerInput))
                _desiredSteer = 0f;
        }

        private void OnInputPointerCanceled(PointerInput pointerInput)
        {
            if (!_isSteeringActive)
                return;

            if (_runSteeringGesture.TryCancel(pointerInput))
                _desiredSteer = 0f;
        }

        private void UpdateSmoothedSteer()
        {
            var responsiveness = ResolveSteeringResponsiveness();
            var fixedDeltaTime = Mathf.Max(0f, _clock.FixedDeltaTime);
            _currentSteer = Mathf.MoveTowards(_currentSteer, _desiredSteer, responsiveness * fixedDeltaTime);
        }

        private void ApplySteering()
        {
            var rawVelocity = _steeringTarget.LinearVelocity;
            var sanityResult = _velocitySanityGuard.Sanitize(rawVelocity, _config.RunBodySpeedSanityGuardMetersPerSecond);
            var velocity = sanityResult.Velocity;
            var stabilizedVelocity = ApplyLaunchLandingStabilization(velocity);
            var hasVelocityCorrection = sanityResult.WasCorrected || HasMeaningfulVelocityChange(velocity, stabilizedVelocity);
            var surfaceContext = _surfaceContextSource.Current;
            var steeringMode = _steeringModeSelector.Select(surfaceContext, stabilizedVelocity, _config.LaunchLandingMaximumLiftSpeed);

            var upDirection = GetValidUpDirection(_steeringFrameSource.GetUpDirection(_steeringUp), _steeringUp);

            var verticalVelocity = Vector3.Project(stabilizedVelocity, upDirection);
            var planarVelocity = stabilizedVelocity - verticalVelocity;
            var planarSpeed = planarVelocity.magnitude;

            if (planarSpeed < Mathf.Max(0f, _config.MinimumSteerSpeed))
            {
                if (hasVelocityCorrection)
                    _steeringTarget.ApplyVelocity(stabilizedVelocity);

                return;
            }

            if (steeringMode == RunSteeringMode.Air && !ShouldApplyAirSteering())
            {
                if (hasVelocityCorrection)
                    _steeringTarget.ApplyVelocity(stabilizedVelocity);

                return;
            }

            var fixedDeltaTime = Mathf.Max(0f, _clock.FixedDeltaTime);
            var turnAngle = _currentSteer * GetMaximumTurnDegreesPerSecond(steeringMode) * fixedDeltaTime;
            var steeredPlanarVelocity = Quaternion.AngleAxis(turnAngle, upDirection) * planarVelocity;
            var steeredVelocity = steeredPlanarVelocity + verticalVelocity;
            var targetRotation = Quaternion.LookRotation(steeredPlanarVelocity.normalized, upDirection);

            _steeringTarget.ApplySteering(steeredVelocity, targetRotation);
        }

        private bool ShouldApplyAirSteering()
        {
            return _runSteeringGesture.IsActive && Mathf.Abs(_currentSteer) > _minimumAirSteerMagnitude;
        }

        private float GetMaximumTurnDegreesPerSecond(RunSteeringMode steeringMode)
        {
            var maximumTurnDegreesPerSecond = steeringMode == RunSteeringMode.Air
                ? _config.RunAirSteeringMaximumTurnDegreesPerSecond
                : _config.MaximumTurnDegreesPerSecond;

            return Mathf.Max(0f, maximumTurnDegreesPerSecond);
        }

        private bool HasMeaningfulVelocityChange(Vector3 previousVelocity, Vector3 nextVelocity)
        {
            var deltaSqrMagnitude = (nextVelocity - previousVelocity).sqrMagnitude;

            return deltaSqrMagnitude > _velocityChangeSqrTolerance
                   && !float.IsNaN(deltaSqrMagnitude)
                   && !float.IsInfinity(deltaSqrMagnitude);
        }

        private void ArmLaunchLandingStabilization()
        {
            _isLaunchLandingStabilizationArmed = true;
            _isLaunchLandingStabilizationActive = false;
            _hasObservedPostLaunchUngroundedSurface = false;
            _launchLandingStabilizationElapsedSeconds = 0f;
        }

        private Vector3 ApplyLaunchLandingStabilization(Vector3 velocity)
        {
            var surfaceContext = _surfaceContextSource.Current;

            if (!_isLaunchLandingStabilizationArmed && !_isLaunchLandingStabilizationActive)
                return velocity;

            var startedThisTick = false;

            if (_isLaunchLandingStabilizationArmed)
            {
                if (!HasValidGroundedSurface(surfaceContext))
                {
                    _hasObservedPostLaunchUngroundedSurface = true;
                    return velocity;
                }

                if (!_hasObservedPostLaunchUngroundedSurface)
                    return velocity;

                _isLaunchLandingStabilizationArmed = false;
                _isLaunchLandingStabilizationActive = true;
                _launchLandingStabilizationElapsedSeconds = 0f;
                startedThisTick = true;
            }

            if (!_isLaunchLandingStabilizationActive)
                return velocity;

            var duration = Mathf.Max(0f, _config.LaunchLandingStabilizationSeconds);

            if (duration <= 0f)
            {
                ClearLaunchLandingStabilization();
                return velocity;
            }

            if (!startedThisTick)
            {
                _launchLandingStabilizationElapsedSeconds += Mathf.Max(0f, _clock.FixedDeltaTime);

                if (_launchLandingStabilizationElapsedSeconds > duration)
                {
                    ClearLaunchLandingStabilization();
                    return velocity;
                }
            }

            if (!HasValidGroundedSurface(surfaceContext))
                return velocity;

            var stabilizedVelocity = ClampSurfaceNormalLiftVelocity(velocity, surfaceContext.GroundNormal);

            if (startedThisTick)
                _launchLandingStabilizationElapsedSeconds += Mathf.Max(0f, _clock.FixedDeltaTime);

            if (_launchLandingStabilizationElapsedSeconds >= duration)
                ClearLaunchLandingStabilization();

            return stabilizedVelocity;
        }

        private bool HasValidGroundedSurface(RunSurfaceContext surfaceContext)
        {
            return surfaceContext.IsGrounded
                   && surfaceContext.HasValidGroundNormal
                   && IsValidDirection(surfaceContext.GroundNormal);
        }

        private Vector3 ClampSurfaceNormalLiftVelocity(Vector3 velocity, Vector3 groundNormal)
        {
            var normal = groundNormal.normalized;
            var liftSpeed = Vector3.Dot(velocity, normal);
            var maximumLiftSpeed = Mathf.Max(0f, _config.LaunchLandingMaximumLiftSpeed);

            if (liftSpeed <= maximumLiftSpeed)
                return velocity;

            return velocity - (normal * (liftSpeed - maximumLiftSpeed));
        }

        private void ClearLaunchLandingStabilization()
        {
            _isLaunchLandingStabilizationArmed = false;
            _isLaunchLandingStabilizationActive = false;
            _hasObservedPostLaunchUngroundedSurface = false;
            _launchLandingStabilizationElapsedSeconds = 0f;
        }

        private Vector3 GetValidUpDirection(Vector3 upDirection)
        {
            return GetValidUpDirection(upDirection, Vector3.up);
        }

        private Vector3 GetValidUpDirection(Vector3 upDirection, Vector3 fallbackUpDirection)
        {
            var sqrMagnitude = upDirection.sqrMagnitude;

            if (sqrMagnitude <= 0.000001f || float.IsNaN(sqrMagnitude) || float.IsInfinity(sqrMagnitude))
                return GetValidUpDirection(fallbackUpDirection);

            return upDirection.normalized;
        }

        private bool IsValidDirection(Vector3 direction)
        {
            var sqrMagnitude = direction.sqrMagnitude;
            return sqrMagnitude > 0.000001f && !float.IsNaN(sqrMagnitude) && !float.IsInfinity(sqrMagnitude);
        }

        private float ResolveSteeringResponsiveness()
        {
            return Mathf.Max(0f, _statResolver.Resolve(_playerSteeringResponsivenessStatId, _config.RunSteeringResponsiveness));
        }
    }
}
