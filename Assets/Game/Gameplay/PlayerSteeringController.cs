using System;
using Game.Foundation.Input;
using Game.Foundation.Screen;
using Game.Foundation.Time;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay
{
    internal sealed class PlayerSteeringController : IInitializable, IFixedTickable, IDisposable
    {
        private readonly IUnityInput _unityInput;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly IPlayerSteeringTarget _steeringTarget;
        private readonly IPlayerSteeringConfig _config;
        private readonly ITime _clock;
        private readonly IScreen _screen;
        private readonly GameplayStateId _runningStateId;

        private IDisposable _inputEnableHandle;
        private Vector3 _steeringUp = Vector3.up;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasLaunchApplied;
        private bool _isSteeringActive;
        private bool _hasActivePointer;
        private int _activePointerId;
        private float _desiredSteer;
        private float _currentSteer;

        public PlayerSteeringController(
            IUnityInput unityInput,
            IGameplayStateService gameplayStateService,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            IPlayerSteeringTarget steeringTarget,
            IPlayerSteeringConfig config,
            ITime clock,
            IScreen screen,
            GameplayStateId runningStateId)
        {
            _unityInput = unityInput ?? throw new ArgumentNullException(nameof(unityInput));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _steeringTarget = steeringTarget ?? throw new ArgumentNullException(nameof(steeringTarget));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PlayerSteeringController));

            if (_isInitialized)
                return;

            _launchAppliedNotifier.LaunchApplied += HandleLaunchApplied;
            _gameplayStateService.GameplayStateChanged += HandleGameplayStateChanged;
            _unityInput.PointerPressed += HandlePointerPressed;
            _unityInput.PointerMoved += HandlePointerMoved;
            _unityInput.PointerReleased += HandlePointerReleased;
            _unityInput.PointerCanceled += HandlePointerCanceled;
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
                _launchAppliedNotifier.LaunchApplied -= HandleLaunchApplied;
                _gameplayStateService.GameplayStateChanged -= HandleGameplayStateChanged;
                _unityInput.PointerPressed -= HandlePointerPressed;
                _unityInput.PointerMoved -= HandlePointerMoved;
                _unityInput.PointerReleased -= HandlePointerReleased;
                _unityInput.PointerCanceled -= HandlePointerCanceled;
            }

            DeactivateSteering();
        }

        private void HandleLaunchApplied(SlingshotLaunchRequest launchRequest)
        {
            if (_isDisposed)
                return;

            _hasLaunchApplied = true;
            _steeringUp = GetValidUpDirection(launchRequest.LaunchUpDirection);

            if (_gameplayStateService.IsCurrent(_runningStateId))
                ActivateSteering();
        }

        private void HandleGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (ReferenceEquals(nextStateId, _runningStateId))
            {
                if (_hasLaunchApplied)
                    ActivateSteering();

                return;
            }

            _hasLaunchApplied = false;
            DeactivateSteering();
        }

        private void ActivateSteering()
        {
            if (_isSteeringActive)
                return;

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
            _hasActivePointer = false;
            _desiredSteer = 0f;
            _currentSteer = 0f;
        }

        private void HandlePointerPressed(PointerInput pointerInput)
        {
            if (!_isSteeringActive || _hasActivePointer)
                return;

            _activePointerId = pointerInput.PointerId;
            _hasActivePointer = true;
            _desiredSteer = GetSteerFromScreenPosition(pointerInput.ScreenPosition);
        }

        private void HandlePointerMoved(PointerInput pointerInput)
        {
            if (!IsActivePointer(pointerInput))
                return;

            _desiredSteer = GetSteerFromScreenPosition(pointerInput.ScreenPosition);
        }

        private void HandlePointerReleased(PointerInput pointerInput)
        {
            if (!IsActivePointer(pointerInput))
                return;

            _hasActivePointer = false;
            _desiredSteer = 0f;
        }

        private void HandlePointerCanceled(PointerInput pointerInput)
        {
            if (!IsActivePointer(pointerInput))
                return;

            _hasActivePointer = false;
            _desiredSteer = 0f;
        }

        private bool IsActivePointer(PointerInput pointerInput)
        {
            return _isSteeringActive && _hasActivePointer && pointerInput.PointerId == _activePointerId;
        }

        private float GetSteerFromScreenPosition(Vector2 screenPosition)
        {
            var screenWidth = _screen.Width;
            var halfWidth = screenWidth * 0.5f;

            if (halfWidth <= 0.0001f)
                return 0f;

            var rawSteer = Mathf.Clamp((screenPosition.x - halfWidth) / halfWidth, -1f, 1f);
            var deadzone = Mathf.Clamp(_config.SteeringDeadzone, 0f, 0.95f);

            if (Mathf.Abs(rawSteer) <= deadzone)
                return 0f;

            var sensitivity = Mathf.Max(0f, _config.SteeringSensitivity);
            return Mathf.Clamp(rawSteer * sensitivity, -1f, 1f);
        }

        private void UpdateSmoothedSteer()
        {
            var responseRate = Mathf.Max(0f, _config.SteeringResponseRate);
            var fixedDeltaTime = Mathf.Max(0f, _clock.FixedDeltaTime);
            _currentSteer = Mathf.MoveTowards(_currentSteer, _desiredSteer, responseRate * fixedDeltaTime);
        }

        private void ApplySteering()
        {
            var velocity = _steeringTarget.LinearVelocity;
            var upDirection = _steeringUp.normalized;
            var verticalVelocity = Vector3.Project(velocity, upDirection);
            var planarVelocity = velocity - verticalVelocity;
            var planarSpeed = planarVelocity.magnitude;

            if (planarSpeed < Mathf.Max(0f, _config.MinimumSteerSpeed))
                return;

            var fixedDeltaTime = Mathf.Max(0f, _clock.FixedDeltaTime);
            var turnAngle = _currentSteer * Mathf.Max(0f, _config.MaximumTurnDegreesPerSecond) * fixedDeltaTime;
            var steeredPlanarVelocity = Quaternion.AngleAxis(turnAngle, upDirection) * planarVelocity;
            var steeredVelocity = steeredPlanarVelocity + verticalVelocity;
            var targetRotation = Quaternion.LookRotation(steeredPlanarVelocity.normalized, upDirection);

            _steeringTarget.ApplySteering(steeredVelocity, targetRotation);
        }

        private Vector3 GetValidUpDirection(Vector3 upDirection)
        {
            var sqrMagnitude = upDirection.sqrMagnitude;

            if (sqrMagnitude <= 0.000001f || float.IsNaN(sqrMagnitude) || float.IsInfinity(sqrMagnitude))
                return Vector3.up;

            return upDirection.normalized;
        }
    }
}
