using System;
using System.Linq;
using Game.Foundation.Time;
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

    public sealed partial class SlingshotController : IInitializable, ITickable, IDisposable, ISlingshotLaunchNotifier
    {
        private readonly IUnityInput _unityInput;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotView _view;
        private readonly ISlingshotInputProjector _inputProjector;
        private readonly IHeldLaunchTarget _heldLaunchTarget;
        private readonly ISlingshotBandShapeProvider _bandShapeProvider;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly ITime _clock;
        private readonly ISlingshotConfig _config;
        private readonly GameplayStateId _preLaunchStateId;

        private IDisposable _inputEnableHandle;
        private Vector3[] _firstActiveBandShapeBuffer;
        private Vector3[] _secondActiveBandShapeBuffer;
        private Vector3[] _currentActiveBandShapeBuffer;
        private Vector3[] _inactiveActiveBandShapeBuffer;
        private Vector3[] _restBandShapeBuffer;
        private Vector3[] _recoilBandShapeBuffer;
        private SlingshotGeometrySnapshot _geometry;
        private SlingshotLaunchRequest _pendingLaunchRequest;
        private float _releaseRecoilElapsed;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _isCaptureEnabled;
        private bool _hasActivePointer;
        private bool _hasLastValidActiveBandShape;
        private bool _isCurrentPullBandShapeValid;
        private bool _isLaunchHandoffPending;
        private bool _isReleaseRecoilActive;
        private int _activePointerId;

        public event Action<SlingshotLaunchRequest> LaunchRequested;

        public SlingshotController(
            IUnityInput unityInput,
            IGameplayStateService gameplayStateService,
            ISlingshotView view,
            ISlingshotInputProjector inputProjector,
            IHeldLaunchTarget heldLaunchTarget,
            ISlingshotBandShapeProvider bandShapeProvider,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            ITime clock,
            ISlingshotConfig config,
            GameplayStateId preLaunchStateId)
        {
            _unityInput = unityInput ?? throw new ArgumentNullException(nameof(unityInput));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _inputProjector = inputProjector ?? throw new ArgumentNullException(nameof(inputProjector));
            _heldLaunchTarget = heldLaunchTarget ?? throw new ArgumentNullException(nameof(heldLaunchTarget));
            _bandShapeProvider = bandShapeProvider ?? throw new ArgumentNullException(nameof(bandShapeProvider));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
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
            InitializeBandShapeBuffers();
            _unityInput.PointerPressed += HandlePointerPressed;
            _unityInput.PointerMoved += HandlePointerMoved;
            _unityInput.PointerReleased += HandlePointerReleased;
            _unityInput.PointerCanceled += HandlePointerCanceled;
            _gameplayStateService.GameplayStateChanged += HandleGameplayStateChanged;
            _launchAppliedNotifier.LaunchApplied += HandleLaunchApplied;
            _isInitialized = true;

            if (_gameplayStateService.IsCurrent(_preLaunchStateId))
            {
                EnableCapture();
                return;
            }

            _view.ShowInactiveIdle(CreateRestBandShape());
        }

        void ITickable.Tick()
        {
            if (!_isReleaseRecoilActive || _isDisposed)
                return;

            TickReleaseRecoil();
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
            _launchAppliedNotifier.LaunchApplied -= HandleLaunchApplied;

            _hasActivePointer = false;
            _isCaptureEnabled = false;
            _isLaunchHandoffPending = false;
            _isReleaseRecoilActive = false;
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
            _isLaunchHandoffPending = false;
            _isReleaseRecoilActive = false;
            _hasLastValidActiveBandShape = false;
            _isCurrentPullBandShapeValid = false;
            _inputEnableHandle = _unityInput.Enable();
            _isCaptureEnabled = true;
            SetHeldTargetToRest();
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
                if (_isLaunchHandoffPending || _isReleaseRecoilActive)
                    return;

                SetHeldTargetToRest();
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
            if (!_isCaptureEnabled
                || _isLaunchHandoffPending
                || _isReleaseRecoilActive
                || _hasActivePointer
                || !IsInsideBandTouchTarget(pointerInput.ScreenPosition))
            {
                return;
            }

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

            if (!_isCurrentPullBandShapeValid)
            {
                CancelActivePullToCaptureIdle();
                return;
            }

            if (!TryCreateLaunchRequest(pullVisual, out var launchRequest))
            {
                CancelActivePullToCaptureIdle();
                return;
            }

            _hasActivePointer = false;
            _isLaunchHandoffPending = true;
            _isReleaseRecoilActive = false;
            _pendingLaunchRequest = launchRequest;
            _view.ShowLoadedRelease(pullVisual.BandShape);
            LaunchRequested?.InvokeSafely(launchRequest);
        }

        private void HandlePointerCanceled(PointerInput pointerInput)
        {
            if (!IsActivePointer(pointerInput))
                return;

            CancelActivePullToCaptureIdle();
        }

        private void HandleLaunchApplied(SlingshotLaunchRequest launchRequest)
        {
            if (!_isLaunchHandoffPending)
                return;

            _pendingLaunchRequest = launchRequest;
            _isLaunchHandoffPending = false;
            _isReleaseRecoilActive = true;
            _releaseRecoilElapsed = 0f;
        }

        private void TickReleaseRecoil()
        {
            _releaseRecoilElapsed += Mathf.Max(0f, _clock.DeltaTime);
            var normalizedTime = Mathf.Clamp01(_releaseRecoilElapsed / _config.BandRecoilDuration);

            if (normalizedTime >= 1f)
            {
                _isReleaseRecoilActive = false;
                _view.ShowInactiveIdle(CreateRestBandShape());
                return;
            }

            var progress = Mathf.Clamp01(_config.BandRecoilCurve.Evaluate(normalizedTime));
            _view.ShowLoadedRelease(CreateReleaseRecoilBandShape(progress));
        }

        private bool IsActivePointer(PointerInput pointerInput)
        {
            return _isCaptureEnabled && _hasActivePointer && pointerInput.PointerId == _activePointerId;
        }

        private void CancelActivePullToCaptureIdle()
        {
            _hasActivePointer = false;
            _isLaunchHandoffPending = false;
            _isReleaseRecoilActive = false;
            _hasLastValidActiveBandShape = false;
            _isCurrentPullBandShapeValid = false;

            if (!_isCaptureEnabled)
                return;

            SetHeldTargetToRest();
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
            var finalPullPoint = GetPullPoint(pullVisual.PullDistance, pullVisual.PullOffset);

            launchRequest = new SlingshotLaunchRequest(
                normalizedPower,
                pullVisual.PullDistance,
                pullVisual.PullOffset,
                finalPullPoint,
                launchDirection,
                launchSpeed,
                _geometry.LaunchFrameUp,
                _config.LaunchUpSpeed);

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
                steering = Mathf.Clamp(-pullOffset / _config.MaximumLateralPull, -1f, 1f);

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

            var clampedPullPoint = GetPullPoint(pullDistance, pullOffset);

            if (!_inputProjector.TryProjectWorldToScreen(clampedPullPoint, out var touchIndicatorScreenPosition))
            {
                pullVisual = default;
                return false;
            }

            _heldLaunchTarget.SetHeldPosition(clampedPullPoint);

            var normalizedPull = Mathf.Clamp01(pullDistance / _config.MaximumPullDistance);
            _isCurrentPullBandShapeValid = TryUpdateActiveBandShape(clampedPullPoint);

            if (!_isCurrentPullBandShapeValid && !_hasLastValidActiveBandShape)
            {
                pullVisual = default;
                return false;
            }

            var shape = new SlingshotBandShape(_currentActiveBandShapeBuffer, true);
            pullVisual = new SlingshotPullVisual(shape, touchIndicatorScreenPosition, pullDistance, pullOffset, normalizedPull);
            return true;
        }

        private Vector3 GetPullPoint(float pullDistance, float pullOffset)
        {
            return _geometry.RestPoint
                   + (_geometry.LaunchFrameRight * pullOffset)
                   - (_geometry.LaunchFrameForward * pullDistance);
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
    }
}
