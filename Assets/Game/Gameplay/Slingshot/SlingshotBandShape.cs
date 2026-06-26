using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public readonly struct SlingshotBandShape
    {
        public Vector3 LeftAnchorPosition { get; }
        public Vector3 MiddlePosition { get; }
        public Vector3 RightAnchorPosition { get; }

        public SlingshotBandShape(Vector3 leftAnchorPosition, Vector3 middlePosition, Vector3 rightAnchorPosition)
        {
            LeftAnchorPosition = leftAnchorPosition;
            MiddlePosition = middlePosition;
            RightAnchorPosition = rightAnchorPosition;
        }
    }
}
