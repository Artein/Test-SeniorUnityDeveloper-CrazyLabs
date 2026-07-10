using UnityEngine;

namespace Game.Gameplay
{
    internal readonly struct RunSteeringAffordanceSnapshot
    {
        public bool IsActive { get; }
        public int PointerId { get; }
        public Vector2 OriginScreenPosition { get; }
        public Vector2 CurrentScreenPosition { get; }
        public float CapturedRangePixels { get; }
        public float CapturedDeadzoneFraction { get; }

        public RunSteeringAffordanceSnapshot(
            bool isActive,
            int pointerId,
            Vector2 originScreenPosition,
            Vector2 currentScreenPosition,
            float capturedRangePixels,
            float capturedDeadzoneFraction)
        {
            IsActive = isActive;
            PointerId = pointerId;
            OriginScreenPosition = originScreenPosition;
            CurrentScreenPosition = currentScreenPosition;
            CapturedRangePixels = capturedRangePixels;
            CapturedDeadzoneFraction = capturedDeadzoneFraction;
        }

        public RunSteeringAffordanceSnapshot WithCurrentScreenPosition(Vector2 currentScreenPosition)
        {
            return new RunSteeringAffordanceSnapshot(
                IsActive,
                PointerId,
                OriginScreenPosition,
                currentScreenPosition,
                CapturedRangePixels,
                CapturedDeadzoneFraction);
        }
    }
}
