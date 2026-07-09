using UnityEngine;

namespace Game.Gameplay
{
    internal readonly struct RunSteeringAffordancePresentationState
    {
        public bool IsVisible { get; }
        public Vector2 OriginScreenPosition { get; }
        public Vector2 KnobScreenPosition { get; }
        public Vector2 LeftRangeEndScreenPosition { get; }
        public Vector2 RightRangeEndScreenPosition { get; }
        public float DeadzoneDiameterPixels { get; }

        public RunSteeringAffordancePresentationState(
            bool isVisible,
            Vector2 originScreenPosition,
            Vector2 knobScreenPosition,
            Vector2 leftRangeEndScreenPosition,
            Vector2 rightRangeEndScreenPosition,
            float deadzoneDiameterPixels)
        {
            IsVisible = isVisible;
            OriginScreenPosition = originScreenPosition;
            KnobScreenPosition = knobScreenPosition;
            LeftRangeEndScreenPosition = leftRangeEndScreenPosition;
            RightRangeEndScreenPosition = rightRangeEndScreenPosition;
            DeadzoneDiameterPixels = deadzoneDiameterPixels;
        }
    }
}
