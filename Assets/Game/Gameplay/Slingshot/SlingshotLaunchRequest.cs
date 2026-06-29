using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public readonly struct SlingshotLaunchRequest
    {
        public float PullStrength { get; }
        public float PullDistance { get; }
        public float PullOffset { get; }
        public float NormalizedLateralPull { get; }
        public Vector3 FinalPullPoint { get; }
        public Vector3 LaunchFrameForward { get; }
        public Vector3 LaunchFrameUp { get; }

        public SlingshotLaunchRequest(
            float pullStrength,
            float pullDistance,
            float pullOffset,
            float normalizedLateralPull,
            Vector3 finalPullPoint,
            Vector3 launchFrameForward,
            Vector3 launchFrameUp)
        {
            PullStrength = pullStrength;
            PullDistance = pullDistance;
            PullOffset = pullOffset;
            NormalizedLateralPull = normalizedLateralPull;
            FinalPullPoint = finalPullPoint;
            LaunchFrameForward = launchFrameForward;
            LaunchFrameUp = launchFrameUp;
        }
    }
}
