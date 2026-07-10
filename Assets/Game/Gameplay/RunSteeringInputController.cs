using System;
using Game.Foundation.Input;
using Game.Foundation.Screen;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    internal readonly struct RunSteeringInputState
    {
        public bool IsGestureActive { get; }
        public float DesiredSteer { get; }
        public float SmoothedSteer { get; }
        public bool HasCapturedMetrics { get; }
        public RunSteeringInputMetrics CapturedMetrics { get; }

        public RunSteeringInputState(
            bool isGestureActive,
            float desiredSteer,
            float smoothedSteer,
            bool hasCapturedMetrics,
            RunSteeringInputMetrics capturedMetrics)
        {
            IsGestureActive = isGestureActive;
            DesiredSteer = desiredSteer;
            SmoothedSteer = smoothedSteer;
            HasCapturedMetrics = hasCapturedMetrics;
            CapturedMetrics = capturedMetrics;
        }
    }

    internal interface IRunSteeringInputSource
    {
        RunSteeringInputState AdvanceAndRead(float fixedDeltaTime);
    }

    internal sealed class RunSteeringInputController : IRunSteeringInputSource, IInitializable, IDisposable
    {
        private readonly IUnityInput _unityInput;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly IRunSteeringConfig _steeringConfig;
        private readonly IRunGameplayStatResolver _statResolver;
        private readonly IScreen _screen;
        private readonly IRunSteeringGesture _gesture;
        private readonly IRunSteeringAffordanceLayout _affordanceLayout;
        private readonly IRunSteeringAffordancePresenter _affordancePresenter;
        private readonly IRunSteeringPointerPressGuard _pointerPressGuard;
        private readonly GameplayStateId _runningStateId;
        private readonly GameplayStatId _steeringResponsivenessStatId;

        private IDisposable _inputEnableHandle;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasLaunchApplied;
        private bool _isInputActive;
        private float _desiredSteer;
        private float _smoothedSteer;

        public RunSteeringInputController(
            IUnityInput unityInput,
            IGameplayStateService gameplayStateService,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            IRunSteeringConfig steeringConfig,
            IRunGameplayStatResolver statResolver,
            IScreen screen,
            IRunSteeringGesture gesture,
            IRunSteeringAffordanceLayout affordanceLayout,
            IRunSteeringAffordancePresenter affordancePresenter,
            IRunSteeringPointerPressGuard pointerPressGuard,
            [Key(InjectKey.GameplayStateId.Running)]
            GameplayStateId runningStateId,
            [Key(InjectKey.GameplayStatId.PlayerSteeringResponsiveness)]
            GameplayStatId steeringResponsivenessStatId)
        {
            _unityInput = unityInput ?? throw new ArgumentNullException(nameof(unityInput));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _steeringConfig = steeringConfig ?? throw new ArgumentNullException(nameof(steeringConfig));
            _statResolver = statResolver ?? throw new ArgumentNullException(nameof(statResolver));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
            _gesture = gesture ?? throw new ArgumentNullException(nameof(gesture));
            _affordanceLayout = affordanceLayout ?? throw new ArgumentNullException(nameof(affordanceLayout));
            _affordancePresenter = affordancePresenter ?? throw new ArgumentNullException(nameof(affordancePresenter));
            _pointerPressGuard = pointerPressGuard ?? throw new ArgumentNullException(nameof(pointerPressGuard));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));

            _steeringResponsivenessStatId = steeringResponsivenessStatId != null
                ? steeringResponsivenessStatId
                : throw new ArgumentNullException(nameof(steeringResponsivenessStatId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunSteeringInputController));

            if (_isInitialized)
                return;

            _launchAppliedNotifier.LaunchApplied += OnLaunchApplied;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _unityInput.PointerPressed += OnPointerPressed;
            _unityInput.PointerMoved += OnPointerMoved;
            _unityInput.PointerReleased += OnPointerReleased;
            _unityInput.PointerCanceled += OnPointerCanceled;
            _isInitialized = true;
        }

        public RunSteeringInputState AdvanceAndRead(float fixedDeltaTime)
        {
            if (_isDisposed || !_isInputActive)
                return default;

            var responsiveness = Mathf.Max(
                0f,
                _statResolver.Resolve(
                    _steeringResponsivenessStatId,
                    _steeringConfig.RunSteeringResponsiveness));

            _smoothedSteer = Mathf.MoveTowards(
                _smoothedSteer,
                _desiredSteer,
                responsiveness * Mathf.Max(0f, fixedDeltaTime));

            return new RunSteeringInputState(
                _gesture.IsActive,
                _desiredSteer,
                _smoothedSteer,
                _gesture.HasCapturedMetrics,
                _gesture.CapturedMetrics);
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _launchAppliedNotifier.LaunchApplied -= OnLaunchApplied;
            _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
            _unityInput.PointerPressed -= OnPointerPressed;
            _unityInput.PointerMoved -= OnPointerMoved;
            _unityInput.PointerReleased -= OnPointerReleased;
            _unityInput.PointerCanceled -= OnPointerCanceled;
            DeactivateInput();
        }

        private void OnLaunchApplied(SlingshotLaunchAppliedEvent launchApplied)
        {
            if (_isDisposed)
                return;

            _hasLaunchApplied = true;

            if (!_gameplayStateService.IsCurrent(_runningStateId))
                return;

            if (_isInputActive)
                ResetPointerAndSteerState();
            else
                ActivateInput();
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (nextStateId == _runningStateId)
            {
                if (_hasLaunchApplied)
                    ActivateInput();

                return;
            }

            _hasLaunchApplied = false;
            DeactivateInput();
        }

        private void ActivateInput()
        {
            if (_isInputActive)
                return;

            ResetPointerAndSteerState();
            _inputEnableHandle = _unityInput.Enable();
            _isInputActive = true;
        }

        private void DeactivateInput()
        {
            ResetPointerAndSteerState();
            _isInputActive = false;

            var inputEnableHandle = _inputEnableHandle;
            _inputEnableHandle = null;
            inputEnableHandle?.Dispose();
        }

        private void ResetPointerAndSteerState()
        {
            _affordancePresenter.Reset();
            _gesture.Reset();
            _desiredSteer = 0f;
            _smoothedSteer = 0f;
        }

        private void OnPointerPressed(PointerInput pointerInput)
        {
            if (!_isInputActive
                || !_pointerPressGuard.CanBeginRunSteering(pointerInput)
                || !_gesture.TryBegin(pointerInput, _screen.Dpi))
            {
                return;
            }

            _desiredSteer = _gesture.RequestedSteering;
            _affordancePresenter.Show(_affordanceLayout.Create(_gesture.AffordanceSnapshot));
        }

        private void OnPointerMoved(PointerInput pointerInput)
        {
            if (!_isInputActive || !_gesture.TryMove(pointerInput))
                return;

            _desiredSteer = _gesture.RequestedSteering;
            _affordancePresenter.Update(_affordanceLayout.Create(_gesture.AffordanceSnapshot));
        }

        private void OnPointerReleased(PointerInput pointerInput)
        {
            if (!_isInputActive)
                return;

            var finalSnapshot = _gesture.AffordanceSnapshot.WithCurrentScreenPosition(pointerInput.ScreenPosition);

            if (!_gesture.TryRelease(pointerInput))
                return;

            _desiredSteer = 0f;
            _affordancePresenter.Hide(_affordanceLayout.Create(finalSnapshot));
        }

        private void OnPointerCanceled(PointerInput pointerInput)
        {
            if (!_isInputActive)
                return;

            var finalSnapshot = _gesture.AffordanceSnapshot.WithCurrentScreenPosition(pointerInput.ScreenPosition);

            if (!_gesture.TryCancel(pointerInput))
                return;

            _desiredSteer = 0f;
            _affordancePresenter.Hide(_affordanceLayout.Create(finalSnapshot));
        }
    }
}
