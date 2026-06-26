using System;
using System.Linq;
using Game.Gameplay.GameplayState;
using Game.Input.UnityInput;
using Game.Utils.Invocation;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotLaunchNotifier
    {
        event Action<SlingshotLaunchRequest> LaunchRequested;
    }

    public sealed class SlingshotController : IInitializable, IDisposable, ISlingshotLaunchNotifier
    {
        private readonly IUnityInput _unityInput;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotView _view;
        private readonly ISlingshotInputProjector _inputProjector;
        private readonly ISlingshotConfig _config;
        private readonly GameplayStateId _preLaunchStateId;

        private IDisposable _inputEnableHandle;
        private SlingshotGeometrySnapshot _geometry;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _isCaptureEnabled;
        private bool _hasActivePointer;
        private int _activePointerId;

        public event Action<SlingshotLaunchRequest> LaunchRequested;

        public SlingshotController(
            IUnityInput unityInput,
            IGameplayStateService gameplayStateService,
            ISlingshotView view,
            ISlingshotInputProjector inputProjector,
            ISlingshotConfig config,
            GameplayStateId preLaunchStateId)
        {
            _unityInput = unityInput ?? throw new ArgumentNullException(nameof(unityInput));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _inputProjector = inputProjector ?? throw new ArgumentNullException(nameof(inputProjector));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));

            ValidateConfig();
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SlingshotController));

            if (_isInitialized)
                return;

            _geometry = _view.CreateGeometrySnapshot();
            _unityInput.PointerPressed += HandlePointerPressed;
            _unityInput.PointerMoved += HandlePointerMoved;
            _unityInput.PointerReleased += HandlePointerReleased;
            _unityInput.PointerCanceled += HandlePointerCanceled;
            _gameplayStateService.GameplayStateChanged += HandleGameplayStateChanged;
            _isInitialized = true;

            if (_gameplayStateService.IsCurrent(_preLaunchStateId))
            {
                EnableCapture();
                return;
            }

            _view.ShowInactiveIdle(CreateRestBandShape());
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _unityInput.PointerPressed -= HandlePointerPressed;
            _unityInput.PointerMoved -= HandlePointerMoved;
            _unityInput.PointerReleased -= HandlePointerReleased;
            _unityInput.PointerCanceled -= HandlePointerCanceled;
            _gameplayStateService.GameplayStateChanged -= HandleGameplayStateChanged;

            _hasActivePointer = false;
            _isCaptureEnabled = false;
            DisposeInputHandle();
        }

        private void ValidateConfig()
        {
            var validator = new SlingshotConfigValidator();
            var errors = validator.Validate(_config).ToList();

            if (errors.Count <= 0)
                return;

            throw new ArgumentException("Invalid Slingshot config: " + string.Join(" ", errors), nameof(_config));
        }

        private void HandleGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (ReferenceEquals(nextStateId, _preLaunchStateId))
            {
                EnableCapture();
                return;
            }

            if (_isCaptureEnabled || ReferenceEquals(previousStateId, _preLaunchStateId))
                DisableCapture();
        }

        private void EnableCapture()
        {
            if (_isCaptureEnabled)
                return;

            _hasActivePointer = false;
            _inputEnableHandle = _unityInput.Enable();
            _isCaptureEnabled = true;
            _view.ShowCaptureIdle(CreateRestBandShape());
        }

        private void DisableCapture()
        {
            if (!_isCaptureEnabled && _inputEnableHandle is null)
                return;

            _hasActivePointer = false;
            _isCaptureEnabled = false;

            try
            {
                _view.ShowInactiveIdle(CreateRestBandShape());
            }
            finally
            {
                DisposeInputHandle();
            }
        }

        private void DisposeInputHandle()
        {
            var inputEnableHandle = _inputEnableHandle;

            if (inputEnableHandle is null)
                return;

            _inputEnableHandle = null;
            inputEnableHandle.Dispose();
        }

        private void HandlePointerPressed(PointerInput pointerInput)
        {
            if (!_isCaptureEnabled || _hasActivePointer || !IsInsideBandTouchTarget(pointerInput.ScreenPosition))
                return;

            if (!TryCreatePullVisual(pointerInput.ScreenPosition, out var pullVisual))
                return;

            _activePointerId = pointerInput.PointerId;
            _hasActivePointer = true;
            _view.ShowActivePull(pullVisual);
        }

        private void HandlePointerMoved(PointerInput pointerInput)
        {
            if (!IsActivePointer(pointerInput))
                return;

            if (!TryCreatePullVisual(pointerInput.ScreenPosition, out var pullVisual))
            {
                CancelActivePullToCaptureIdle();
                return;
            }

            _view.ShowActivePull(pullVisual);
        }

        private void HandlePointerReleased(PointerInput pointerInput)
        {
            if (!IsActivePointer(pointerInput))
                return;

            if (!TryCreatePullVisual(pointerInput.ScreenPosition, out var pullVisual))
            {
                CancelActivePullToCaptureIdle();
                return;
            }

            var hasLaunchRequest = TryCreateLaunchRequest(pullVisual, out var launchRequest);
            CancelActivePullToCaptureIdle();

            if (hasLaunchRequest)
                LaunchRequested?.InvokeSafely(launchRequest);
        }

        private void HandlePointerCanceled(PointerInput pointerInput)
        {
            if (!IsActivePointer(pointerInput))
                return;

            CancelActivePullToCaptureIdle();
        }

        private bool IsActivePointer(PointerInput pointerInput)
        {
            return _isCaptureEnabled && _hasActivePointer && pointerInput.PointerId == _activePointerId;
        }

        private void CancelActivePullToCaptureIdle()
        {
            _hasActivePointer = false;

            if (_isCaptureEnabled)
                _view.ShowCaptureIdle(CreateRestBandShape());
        }

        private bool TryCreateLaunchRequest(SlingshotPullVisual pullVisual, out SlingshotLaunchRequest launchRequest)
        {
            if (pullVisual.PullDistance < _config.MinimumPullDistance)
            {
                launchRequest = default;
                return false;
            }

            var normalizedPower = GetNormalizedLaunchPower(pullVisual.PullDistance);
            var launchSpeedCurveValue = Mathf.Clamp01(_config.LaunchSpeedCurve.Evaluate(normalizedPower));
            var launchSpeed = Mathf.Lerp(_config.MinimumLaunchSpeed, _config.MaximumLaunchSpeed, launchSpeedCurveValue);
            var launchDirection = GetLaunchDirection(pullVisual.PullOffset);

            launchRequest = new SlingshotLaunchRequest(normalizedPower, pullVisual.PullDistance, pullVisual.PullOffset, launchDirection,
                launchSpeed, _geometry.LaunchFrameUp, _config.LaunchUpSpeed);

            return true;
        }

        private float GetNormalizedLaunchPower(float pullDistance)
        {
            var pullRange = _config.MaximumPullDistance - _config.MinimumPullDistance;

            if (pullRange <= 0.000001f)
                return 1f;

            return Mathf.Clamp01(Mathf.InverseLerp(_config.MinimumPullDistance, _config.MaximumPullDistance, pullDistance));
        }

        private Vector3 GetLaunchDirection(float pullOffset)
        {
            var steering = 0f;

            if (_config.MaximumLateralPull > 0.000001f)
                steering = Mathf.Clamp(pullOffset / _config.MaximumLateralPull, -1f, 1f);

            return (Quaternion.AngleAxis(steering * _config.MaximumLaunchAngleDegrees, _geometry.LaunchFrameUp) * _geometry.LaunchFrameForward)
                .normalized;
        }

        private bool TryCreatePullVisual(Vector2 screenPosition, out SlingshotPullVisual pullVisual)
        {
            if (!_inputProjector.TryProjectScreenToPullPlane(screenPosition, _geometry, out var rawPullPoint))
            {
                pullVisual = default;
                return false;
            }

            var delta = rawPullPoint - _geometry.RestPoint;

            var pullDistance = Mathf.Clamp(
                -Vector3.Dot(delta, _geometry.LaunchFrameForward),
                0f,
                _config.MaximumPullDistance);

            var pullOffset = Mathf.Clamp(
                Vector3.Dot(delta, _geometry.LaunchFrameRight),
                -_config.MaximumLateralPull,
                _config.MaximumLateralPull);

            var clampedPullPoint = _geometry.RestPoint
                                   + (_geometry.LaunchFrameRight * pullOffset)
                                   - (_geometry.LaunchFrameForward * pullDistance);

            if (!_inputProjector.TryProjectWorldToScreen(clampedPullPoint, out var touchIndicatorScreenPosition))
            {
                pullVisual = default;
                return false;
            }

            var normalizedPull = Mathf.Clamp01(pullDistance / _config.MaximumPullDistance);
            var shape = new SlingshotBandShape(_geometry.LeftAnchorPosition, clampedPullPoint, _geometry.RightAnchorPosition);
            pullVisual = new SlingshotPullVisual(shape, touchIndicatorScreenPosition, pullDistance, pullOffset, normalizedPull);
            return true;
        }

        private bool IsInsideBandTouchTarget(Vector2 screenPosition)
        {
            if (!_inputProjector.TryProjectWorldToScreen(_geometry.LeftAnchorPosition, out var leftAnchorScreenPosition)
                || !_inputProjector.TryProjectWorldToScreen(_geometry.RestPoint, out var restPointScreenPosition)
                || !_inputProjector.TryProjectWorldToScreen(_geometry.RightAnchorPosition, out var rightAnchorScreenPosition))
            {
                return false;
            }

            var distanceToLeftBand = GetDistanceToSegment(screenPosition, leftAnchorScreenPosition, restPointScreenPosition);
            var distanceToRightBand = GetDistanceToSegment(screenPosition, restPointScreenPosition, rightAnchorScreenPosition);
            var distanceToBand = Mathf.Min(distanceToLeftBand, distanceToRightBand);
            return distanceToBand <= _config.TouchTargetRadiusPixels;
        }

        private float GetDistanceToSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
        {
            var segment = segmentEnd - segmentStart;
            var segmentLengthSquared = segment.sqrMagnitude;

            if (segmentLengthSquared <= 0.000001f)
                return Vector2.Distance(point, segmentStart);

            var segmentProgress = Mathf.Clamp01(Vector2.Dot(point - segmentStart, segment) / segmentLengthSquared);
            var closestPoint = segmentStart + (segment * segmentProgress);
            return Vector2.Distance(point, closestPoint);
        }

        private SlingshotBandShape CreateRestBandShape()
        {
            return new SlingshotBandShape(_geometry.LeftAnchorPosition, _geometry.RestPoint, _geometry.RightAnchorPosition);
        }
    }
}
