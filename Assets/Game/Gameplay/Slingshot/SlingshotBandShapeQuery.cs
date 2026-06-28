using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public readonly struct SlingshotBandShapeQuery
    {
        public Vector3 LeftAnchorPosition { get; }
        public Vector3 RightAnchorPosition { get; }
        public Vector3 RestPoint { get; }
        public Vector3 PullPoint { get; }
        public Vector3 LaunchFrameRight { get; }
        public Vector3 LaunchFrameForward { get; }
        public Vector3 LaunchFrameUp { get; }

        public SlingshotBandShapeQuery(
            Vector3 leftAnchorPosition,
            Vector3 rightAnchorPosition,
            Vector3 restPoint,
            Vector3 pullPoint,
            Vector3 launchFrameRight,
            Vector3 launchFrameForward,
            Vector3 launchFrameUp)
        {
            LeftAnchorPosition = leftAnchorPosition;
            RightAnchorPosition = rightAnchorPosition;
            RestPoint = restPoint;
            PullPoint = pullPoint;
            LaunchFrameRight = launchFrameRight;
            LaunchFrameForward = launchFrameForward;
            LaunchFrameUp = launchFrameUp;
        }
    }
}
