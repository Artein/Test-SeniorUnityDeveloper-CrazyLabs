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
        private readonly IPlayerSteeringConfig _config;
        private readonly IRunGameplayStatResolver _statResolver;
        private readonly ITime _clock;
        private readonly IScreen _screen;
        private readonly IRunSteeringGesture _runSteeringGesture;
        private readonly GameplayStateId _runningStateId;
        private readonly GameplayStatId _playerMaxSpeedStatId;
        private readonly GameplayStatId _playerSteeringResponsivenessStatId;

        private IDisposable _inputEnableHandle;
        private Vector3 _steeringUp = Vector3.up;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasLaunchApplied;
        private bool _isSteeringActive;
        private float _desiredSteer;
        private float _currentSteer;

        public PlayerSteeringController(
            IUnityInput unityInput,
            IGameplayStateService gameplayStateService,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            IPlayerSteeringTarget steeringTarget,
            IPlayerSteeringConfig config,
            IRunGameplayStatResolver statResolver,
            ITime clock,
            IScreen screen,
            IRunSteeringGesture runSteeringGesture,
            [Key(InjectKey.GameplayStateId.Running)]
            GameplayStateId runningStateId,
            [Key(InjectKey.GameplayStatId.PlayerMaxSpeed)]
            GameplayStatId playerMaxSpeedStatId,
            [Key(InjectKey.GameplayStatId.PlayerSteeringResponsiveness)]
            GameplayStatId playerSteeringResponsivenessStatId)
        {
            _unityInput = unityInput ?? throw new ArgumentNullException(nameof(unityInput));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _steeringTarget = steeringTarget ?? throw new ArgumentNullException(nameof(steeringTarget));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _statResolver = statResolver ?? throw new ArgumentNullException(nameof(statResolver));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
            _runSteeringGesture = runSteeringGesture ?? throw new ArgumentNullException(nameof(runSteeringGesture));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));

            _playerMaxSpeedStatId = playerMaxSpeedStatId != null
                ? playerMaxSpeedStatId
                : throw new ArgumentNullException(nameof(playerMaxSpeedStatId));

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

            if (_gameplayStateService.IsCurrent(_runningStateId))
                ActivateSteering();
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
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
            var velocity = _steeringTarget.LinearVelocity;
            var upDirection = _steeringUp.normalized;
            var verticalVelocity = Vector3.Project(velocity, upDirection);
            var planarVelocity = velocity - verticalVelocity;
            var planarSpeed = planarVelocity.magnitude;

            if (planarSpeed < Mathf.Max(0f, _config.MinimumSteerSpeed))
                return;

            var maximumPlanarSpeed = ResolveMaximumPlanarSpeed();

            if (planarSpeed > maximumPlanarSpeed)
                planarVelocity = planarVelocity.normalized * maximumPlanarSpeed;

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

        private float ResolveSteeringResponsiveness()
        {
            return Mathf.Max(0f, _statResolver.Resolve(_playerSteeringResponsivenessStatId, _config.RunSteeringResponsiveness));
        }

        private float ResolveMaximumPlanarSpeed()
        {
            return Mathf.Max(0.0001f, _statResolver.Resolve(_playerMaxSpeedStatId, _config.MaximumPlanarSpeed));
        }
    }
}
