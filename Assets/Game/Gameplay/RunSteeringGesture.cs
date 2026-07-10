using System;
using Game.Foundation.Input;
using UnityEngine;

namespace Game.Gameplay
{
    internal interface IRunSteeringGesture
    {
        bool IsActive { get; }
        float RequestedSteering { get; }
        RunSteeringAffordanceSnapshot AffordanceSnapshot { get; }

        bool TryBegin(PointerInput pointerInput, float rawDpi);
        bool TryMove(PointerInput pointerInput);
        bool TryRelease(PointerInput pointerInput);
        bool TryCancel(PointerInput pointerInput);
        void Reset();
    }

    internal sealed class RunSteeringGesture : IRunSteeringGesture
    {
        private readonly IPlayerSteeringConfig _config;

        internal bool isActive;
        internal Vector2 origin;
        private Vector2 _currentScreenPosition;
        private int _activePointerId;
        internal float capturedRangePixels;
        private float _capturedDeadzoneFraction;
        private float _requestedSteering;

        public float RequestedSteering => _requestedSteering;
        bool IRunSteeringGesture.IsActive => isActive;
        RunSteeringAffordanceSnapshot IRunSteeringGesture.AffordanceSnapshot => CreateAffordanceSnapshot();

        public RunSteeringGesture(IPlayerSteeringConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        bool IRunSteeringGesture.TryBegin(PointerInput pointerInput, float rawDpi)
        {
            if (isActive)
                return false;

            isActive = true;
            _activePointerId = pointerInput.PointerId;
            origin = pointerInput.ScreenPosition;
            _currentScreenPosition = pointerInput.ScreenPosition;
            capturedRangePixels = Mathf.Max(0.0001f, _config.ResolveRunSteeringRangePixels(rawDpi));
            _capturedDeadzoneFraction = Mathf.Clamp(_config.RunSteeringDeadzoneFraction, 0f, 0.95f);
            _requestedSteering = 0f;
            return true;
        }

        bool IRunSteeringGesture.TryMove(PointerInput pointerInput)
        {
            if (!IsActivePointer(pointerInput))
                return false;

            _currentScreenPosition = pointerInput.ScreenPosition;
            _requestedSteering = MapHorizontalDisplacement(pointerInput.ScreenPosition.x - origin.x);
            return true;
        }

        bool IRunSteeringGesture.TryRelease(PointerInput pointerInput)
        {
            if (!IsActivePointer(pointerInput))
                return false;

            ((IRunSteeringGesture)this).Reset();
            return true;
        }

        bool IRunSteeringGesture.TryCancel(PointerInput pointerInput)
        {
            if (!IsActivePointer(pointerInput))
                return false;

            ((IRunSteeringGesture)this).Reset();
            return true;
        }

        void IRunSteeringGesture.Reset()
        {
            isActive = false;
            _activePointerId = 0;
            origin = Vector2.zero;
            _currentScreenPosition = Vector2.zero;
            capturedRangePixels = 0f;
            _capturedDeadzoneFraction = 0f;
            _requestedSteering = 0f;
        }

        private RunSteeringAffordanceSnapshot CreateAffordanceSnapshot()
        {
            if (!isActive)
                return new RunSteeringAffordanceSnapshot(false, 0, Vector2.zero, Vector2.zero, 0f, 0f);

            return new RunSteeringAffordanceSnapshot(
                isActive: true,
                _activePointerId,
                origin,
                _currentScreenPosition,
                capturedRangePixels,
                _capturedDeadzoneFraction);
        }

        private bool IsActivePointer(PointerInput pointerInput)
        {
            return isActive && pointerInput.PointerId == _activePointerId;
        }

        private float MapHorizontalDisplacement(float horizontalDisplacement)
        {
            if (capturedRangePixels <= 0.0001f)
                return 0f;

            var normalized = Mathf.Clamp(horizontalDisplacement / capturedRangePixels, -1f, 1f);
            var magnitude = Mathf.Abs(normalized);

            if (magnitude <= _capturedDeadzoneFraction)
                return 0f;

            var remappedMagnitude = Mathf.InverseLerp(_capturedDeadzoneFraction, 1f, magnitude);
            return Mathf.Sign(normalized) * remappedMagnitude;
        }
    }
}
