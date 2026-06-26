using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public readonly struct SlingshotLaunchRequest
    {
        public float NormalizedPower { get; }
        public float PullDistance { get; }
        public float PullOffset { get; }
        public Vector3 FinalPullPoint { get; }
        public Vector3 LaunchDirection { get; }
        public float LaunchSpeed { get; }
        public Vector3 LaunchUpDirection { get; }
        public float LaunchUpSpeed { get; }

        public SlingshotLaunchRequest(
            float normalizedPower,
            float pullDistance,
            float pullOffset,
            Vector3 finalPullPoint,
            Vector3 launchDirection,
            float launchSpeed,
            Vector3 launchUpDirection,
            float launchUpSpeed)
        {
            NormalizedPower = normalizedPower;
            PullDistance = pullDistance;
            PullOffset = pullOffset;
            FinalPullPoint = finalPullPoint;
            LaunchDirection = launchDirection;
            LaunchSpeed = launchSpeed;
            LaunchUpDirection = launchUpDirection;
            LaunchUpSpeed = launchUpSpeed;
        }
    }
}
