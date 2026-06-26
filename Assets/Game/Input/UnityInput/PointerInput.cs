using UnityEngine;

namespace Game.Input.UnityInput
{
    public readonly struct PointerInput
    {
        public int PointerId { get; }
        public Vector2 ScreenPosition { get; }

        public PointerInput(int pointerId, Vector2 screenPosition)
        {
            PointerId = pointerId;
            ScreenPosition = screenPosition;
        }
    }
}
