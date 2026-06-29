using System;
using System.Linq;
using Game.Foundation.Input;
using Game.Foundation.Time;
using Game.Utils.Invocation;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotLaunchNotifier
    {
        event Action<SlingshotLaunchRequest> LaunchRequested;
    }

    public interface ISlingshotCapture
    {
        void EnableCapture();
        void DisableCapture();
    }

    public sealed partial class SlingshotController : IInitializable, ITickable, IDisposable, ISlingshotCapture, ISlingshotLaunchNotifier
    {
        private readonly IUnityInput _unityInput;
        private readonly ISlingshotView _view;
        private readonly ISlingshotInputProjector _inputProjector;
        private readonly ILaunchTarget _launchTarget;
        private readonly IHeldLaunchTarget _heldLaunchTarget;
        private readonly ISlingshotBandShapeProvider _bandShapeProvider;
        private readonly ISlingshotBandShapeDepthProvider _bandShapeDepthProvider;
        private readonly ISlingshotBandShapeOffsetProvider _bandShapeOffsetProvider;
        private readonly ISlingshotRenderedBandShapeProvider _renderedBandShapeProvider;
        private readonly ISlingshotBandVisibilityRayProvider _bandVisibilityRayProvider;
        private readonly ILaunchTargetBandOcclusionSource _launchTargetBandOcclusionSource;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly ITime _clock;
        private readonly ISlingshotConfig _config;

        private IDisposable _inputEnableHandle;
        private Vector3[] _firstActiveBandShapeBuffer;
        private Vector3[] _secondActiveBandShapeBuffer;
        private Vector3[] _currentActiveBandShapeBuffer;
        private Vector3[] _inactiveActiveBandShapeBuffer;
        private Vector3[] _restBandShapeBuffer;
        private Vector3[] _visibilityBandShapeBuffer;
        private SlingshotGeometrySnapshot _geometry;
        private SlingshotLaunchRequest _pendingLaunchRequest;
        private Vector3 _committedHeldTargetPosition;
        private float _releaseRecoilElapsed;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _isCaptureEnabled;
        private bool _hasActivePointer;
        private bool _hasCommittedHeldTargetPosition;
        private bool _hasLastValidActiveBandShape;
        private bool _isCurrentPullBandShapeValid;
        private bool _isLaunchHandoffPending;
        private bool _isReleaseRecoilActive;
        private int _activePointerId;

        public event Action<SlingshotLaunchRequest> LaunchRequested;

        public SlingshotController(
            IUnityInput unityInput,
            ISlingshotView view,
            ISlingshotInputProjector inputProjector,
            ILaunchTarget launchTarget,
            IHeldLaunchTarget heldLaunchTarget,
            ISlingshotBandShapeProvider bandShapeProvider,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            ITime clock,
            ISlingshotConfig config)
        {
            _unityInput = unityInput ?? throw new ArgumentNullException(nameof(unityInput));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _inputProjector = inputProjector ?? throw new ArgumentNullException(nameof(inputProjector));
            _launchTarget = launchTarget ?? throw new ArgumentNullException(nameof(launchTarget));
            _heldLaunchTarget = heldLaunchTarget ?? throw new ArgumentNullException(nameof(heldLaunchTarget));
            _bandShapeProvider = bandShapeProvider ?? throw new ArgumentNullException(nameof(bandShapeProvider));

            _bandShapeDepthProvider = bandShapeProvider as ISlingshotBandShapeDepthProvider
                                      ?? throw new ArgumentException(
                                          "Slingshot Band Shape provider must support silhouette depth spans.",
                                          nameof(bandShapeProvider));

            _bandShapeOffsetProvider = bandShapeProvider as ISlingshotBandShapeOffsetProvider
                                       ?? throw new ArgumentException(
                                           "Slingshot Band Shape provider must support silhouette offset spans.",
                                           nameof(bandShapeProvider));

            _renderedBandShapeProvider = bandShapeProvider as ISlingshotRenderedBandShapeProvider
                                         ?? throw new ArgumentException(
                                             "Slingshot Band Shape provider must support rendered Band Shape solves.",
                                             nameof(bandShapeProvider));

            _bandVisibilityRayProvider = inputProjector as ISlingshotBandVisibilityRayProvider;
            _launchTargetBandOcclusionSource = launchTarget as ILaunchTargetBandOcclusionSource;
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _config = config ?? throw new ArgumentNullException(nameof(config));

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
            _unityInput.PointerPressed += OnInputPointerPressed;
            _unityInput.PointerMoved += OnInputPointerMoved;
            _unityInput.PointerReleased += OnInputPointerReleased;
            _unityInput.PointerCanceled += OnInputPointerCanceled;
            _launchAppliedNotifier.LaunchApplied += OnSlingshotLaunchApplied;
            _isInitialized = true;
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

            _unityInput.PointerPressed -= OnInputPointerPressed;
            _unityInput.PointerMoved -= OnInputPointerMoved;
            _unityInput.PointerReleased -= OnInputPointerReleased;
            _unityInput.PointerCanceled -= OnInputPointerCanceled;
            _launchAppliedNotifier.LaunchApplied -= OnSlingshotLaunchApplied;

            _hasActivePointer = false;
            _isCaptureEnabled = false;
            _isLaunchHandoffPending = false;
            _isReleaseRecoilActive = false;
            DisposeInputHandle();
        }

        void ISlingshotCapture.EnableCapture()
        {
            ThrowIfCaptureCallInvalid();

            if (_isCaptureEnabled)
                return;

            _geometry = _view.CreateGeometrySnapshot();
            _hasActivePointer = false;
            _isLaunchHandoffPending = false;
            _isReleaseRecoilActive = false;
            _hasLastValidActiveBandShape = false;
            _isCurrentPullBandShapeValid = false;
            _inputEnableHandle = _unityInput.Enable();
            _isCaptureEnabled = true;
            _launchTarget.Hold();
            SetHeldTargetToRest();
            _view.ShowCaptureIdle(CreateHeldRestBandShape());
        }

        void ISlingshotCapture.DisableCapture()
        {
            ThrowIfCaptureCallInvalid();

            if (!_isCaptureEnabled && _inputEnableHandle is null)
                return;

            _hasActivePointer = false;
            _isCaptureEnabled = false;

            try
            {
                if (_isLaunchHandoffPending || _isReleaseRecoilActive)
                    return;

                SetHeldTargetToRest();
                _view.ShowInactiveIdle(CreateDetachedRestBandShape());
            }
            finally
            {
                DisposeInputHandle();
            }
        }

        private void ValidateConfig()
        {
            var validator = new SlingshotConfigValidator();
            var errors = validator.Validate(_config).ToList();

            if (errors.Count <= 0)
                return;

            throw new ArgumentException("Invalid Slingshot config: " + string.Join(" ", errors), nameof(_config));
        }

        private void ThrowIfCaptureCallInvalid()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SlingshotController));

            if (!_isInitialized)
                throw new InvalidOperationException("Slingshot capture cannot be changed before initialization.");
        }

        private void DisposeInputHandle()
        {
            var inputEnableHandle = _inputEnableHandle;

            if (inputEnableHandle is null)
                return;

            _inputEnableHandle = null;
            inputEnableHandle.Dispose();
        }

        private void OnInputPointerPressed(PointerInput pointerInput)
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

        private void OnInputPointerMoved(PointerInput pointerInput)
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

        private void OnInputPointerReleased(PointerInput pointerInput)
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

        private void OnInputPointerCanceled(PointerInput pointerInput)
        {
            if (!IsActivePointer(pointerInput))
                return;

            CancelActivePullToCaptureIdle();
        }

        private void OnSlingshotLaunchApplied(SlingshotLaunchRequest launchRequest)
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

            if (IsDetachedRestBandShapeClear() && HasTargetSilhouettePassedRestBandShape())
            {
                _isReleaseRecoilActive = false;
                _view.ShowInactiveIdle(CreateDetachedRestBandShape());
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
            _view.ShowCaptureIdle(CreateHeldRestBandShape());
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
            var maximumSteeringOffset = GetMaximumSteeringOffset(pullOffset);

            if (maximumSteeringOffset > 0.000001f)
                steering = Mathf.Clamp(-pullOffset / maximumSteeringOffset, -1f, 1f);

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

            var pullOffset = ClampPullOffset(Vector3.Dot(delta, _geometry.LaunchFrameRight), pullDistance);

            var clampedPullPoint = GetPullPoint(pullDistance, pullOffset);
            PoseHeldTargetForPullSolve(clampedPullPoint);

            var silhouetteClampedPullOffset = ClampPullOffsetToRenderedSilhouetteCorridor(pullOffset, clampedPullPoint);

            if (Mathf.Abs(silhouetteClampedPullOffset - pullOffset) > 0.0001f)
            {
                pullOffset = silhouetteClampedPullOffset;
                clampedPullPoint = GetPullPoint(pullDistance, pullOffset);
                PoseHeldTargetForPullSolve(clampedPullPoint);
            }

            var visibleClampedPullOffset = ClampPullOffsetToVisibleBandShape(pullDistance, pullOffset);

            if (Mathf.Abs(visibleClampedPullOffset - pullOffset) > 0.0001f)
            {
                pullOffset = visibleClampedPullOffset;
                clampedPullPoint = GetPullPoint(pullDistance, pullOffset);
                PoseHeldTargetForPullSolve(clampedPullPoint);
            }

            if (!_inputProjector.TryProjectWorldToScreen(clampedPullPoint, out var touchIndicatorScreenPosition))
            {
                RestoreCommittedHeldTargetPosition();
                pullVisual = default;
                return false;
            }

            var normalizedPull = Mathf.Clamp01(pullDistance / _config.MaximumPullDistance);
            _isCurrentPullBandShapeValid = TryUpdateActiveBandShape(clampedPullPoint);

            if (!_isCurrentPullBandShapeValid && !_hasLastValidActiveBandShape)
            {
                RestoreCommittedHeldTargetPosition();
                pullVisual = default;
                return false;
            }

            var shape = new SlingshotBandShape(_currentActiveBandShapeBuffer, true);
            pullVisual = new SlingshotPullVisual(shape, touchIndicatorScreenPosition, pullDistance, pullOffset, normalizedPull);
            CommitHeldTargetPosition(clampedPullPoint);
            return true;
        }

        private void PoseHeldTargetForPullSolve(Vector3 position)
        {
            _heldLaunchTarget.SetHeldPosition(position);
        }

        private void SetCommittedHeldTargetPosition(Vector3 position)
        {
            _heldLaunchTarget.SetHeldPosition(position);
            CommitHeldTargetPosition(position);
        }

        private void CommitHeldTargetPosition(Vector3 position)
        {
            _committedHeldTargetPosition = position;
            _hasCommittedHeldTargetPosition = true;
        }

        private void RestoreCommittedHeldTargetPosition()
        {
            _heldLaunchTarget.SetHeldPosition(
                _hasCommittedHeldTargetPosition
                    ? _committedHeldTargetPosition
                    : _geometry.RestPoint);
        }

        private Vector3 GetPullPoint(float pullDistance, float pullOffset)
        {
            return _geometry.RestPoint
                   + (_geometry.LaunchFrameRight * pullOffset)
                   - (_geometry.LaunchFrameForward * pullDistance);
        }

        private float ClampPullOffset(float rawPullOffset, float pullDistance)
        {
            var lateralPullScale = GetLateralPullScale(pullDistance);

            return Mathf.Clamp(
                rawPullOffset,
                GetMinimumAllowedPullOffset() * lateralPullScale,
                GetMaximumAllowedPullOffset() * lateralPullScale);
        }

        private float ClampPullOffsetToRenderedSilhouetteCorridor(float pullOffset, Vector3 posedPullPoint)
        {
            if (!_bandShapeOffsetProvider.TryGetSilhouetteOffsetSpan(
                    CreateBandShapeQuery(posedPullPoint),
                    out var minimumSilhouetteOffset,
                    out var maximumSilhouetteOffset))
            {
                return pullOffset;
            }

            var targetLeftExtent = Mathf.Max(0f, pullOffset - minimumSilhouetteOffset);
            var targetRightExtent = Mathf.Max(0f, maximumSilhouetteOffset - pullOffset);
            var renderedCorridorClearance = GetRenderedAnchorCorridorClearance();
            var minimumPullOffset = GetMinimumAnchorOffset() + targetLeftExtent + renderedCorridorClearance;
            var maximumPullOffset = GetMaximumAnchorOffset() - targetRightExtent - renderedCorridorClearance;

            if (minimumPullOffset > maximumPullOffset)
                return pullOffset;

            return Mathf.Clamp(pullOffset, minimumPullOffset, maximumPullOffset);
        }

        private float GetRenderedAnchorCorridorClearance()
        {
            return GetBandTargetClearanceRadius() * 3f;
        }

        private float GetLateralPullScale(float pullDistance)
        {
            var fullLateralPullDistance = GetFullLateralPullDistance();

            if (fullLateralPullDistance <= 0.000001f)
                return 1f;

            return Mathf.Clamp01(pullDistance / fullLateralPullDistance);
        }

        private float GetFullLateralPullDistance()
        {
            return GetMinimumLateralPullRampDistance() + (_config.BandContactPadding * 2f);
        }

        private float GetMaximumSteeringOffset(float pullOffset)
        {
            if (pullOffset < 0f)
                return -GetMinimumAllowedPullOffset();

            return GetMaximumAllowedPullOffset();
        }

        private float GetMinimumAllowedPullOffset()
        {
            return Mathf.Max(-_config.MaximumLateralPull, GetMinimumAnchorOffset());
        }

        private float GetMaximumAllowedPullOffset()
        {
            return Mathf.Min(_config.MaximumLateralPull, GetMaximumAnchorOffset());
        }

        private float GetMinimumAnchorOffset()
        {
            var leftAnchorOffset = Vector3.Dot(_geometry.LeftAnchorPosition - _geometry.RestPoint, _geometry.LaunchFrameRight);
            var rightAnchorOffset = Vector3.Dot(_geometry.RightAnchorPosition - _geometry.RestPoint, _geometry.LaunchFrameRight);
            return Mathf.Min(leftAnchorOffset, rightAnchorOffset);
        }

        private float GetMaximumAnchorOffset()
        {
            var leftAnchorOffset = Vector3.Dot(_geometry.LeftAnchorPosition - _geometry.RestPoint, _geometry.LaunchFrameRight);
            var rightAnchorOffset = Vector3.Dot(_geometry.RightAnchorPosition - _geometry.RestPoint, _geometry.LaunchFrameRight);
            return Mathf.Max(leftAnchorOffset, rightAnchorOffset);
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
