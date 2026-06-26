using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public readonly struct SlingshotBandContactQuery
    {
        public Vector3 LeftAnchorPosition { get; }
        public Vector3 RightAnchorPosition { get; }
        public Vector3 PullPoint { get; }
        public Vector3 LaunchFrameRight { get; }
        public Vector3 LaunchFrameForward { get; }
        public Vector3 LaunchFrameUp { get; }
        public float ContactPadding { get; }
        public int WrapSampleCount { get; }

        public SlingshotBandContactQuery(
            Vector3 leftAnchorPosition,
            Vector3 rightAnchorPosition,
            Vector3 pullPoint,
            Vector3 launchFrameRight,
            Vector3 launchFrameForward,
            Vector3 launchFrameUp,
            float contactPadding,
            int wrapSampleCount)
        {
            LeftAnchorPosition = leftAnchorPosition;
            RightAnchorPosition = rightAnchorPosition;
            PullPoint = pullPoint;
            LaunchFrameRight = launchFrameRight;
            LaunchFrameForward = launchFrameForward;
            LaunchFrameUp = launchFrameUp;
            ContactPadding = contactPadding;
            WrapSampleCount = wrapSampleCount;
        }
    }
}
