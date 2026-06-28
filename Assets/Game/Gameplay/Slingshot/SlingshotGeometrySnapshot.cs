using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public readonly struct SlingshotGeometrySnapshot
    {
        public Vector3 LeftAnchorPosition { get; }
        public Vector3 RightAnchorPosition { get; }
        public Vector3 RestPoint { get; }
        public Vector3 LaunchFrameRight { get; }
        public Vector3 LaunchFrameForward { get; }
        public Vector3 LaunchFrameUp { get; }

        public SlingshotGeometrySnapshot(
            Vector3 leftAnchorPosition,
            Vector3 rightAnchorPosition,
            Vector3 restPoint,
            Vector3 launchFrameRight,
            Vector3 launchFrameForward,
            Vector3 launchFrameUp)
        {
            var validator = new LaunchFrameValidator();

            validator.ThrowIfInvalidAndNormalize(
                launchFrameRight,
                launchFrameForward,
                launchFrameUp,
                out var normalizedRight,
                out var normalizedForward,
                out var normalizedUp);

            LeftAnchorPosition = leftAnchorPosition;
            RightAnchorPosition = rightAnchorPosition;
            RestPoint = restPoint;
            LaunchFrameRight = normalizedRight;
            LaunchFrameForward = normalizedForward;
            LaunchFrameUp = normalizedUp;
        }
    }
}
