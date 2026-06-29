using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public readonly struct SlingshotPullVisual
    {
        public SlingshotBandShape BandShape { get; }
        public Vector2 TouchIndicatorScreenPosition { get; }
        public float PullDistance { get; }
        public float PullOffset { get; }
        public float NormalizedPull { get; }
        public float NormalizedPullOffset { get; }

        public SlingshotPullVisual(
            SlingshotBandShape bandShape,
            Vector2 touchIndicatorScreenPosition,
            float pullDistance,
            float pullOffset,
            float normalizedPull,
            float normalizedPullOffset)
        {
            BandShape = bandShape;
            TouchIndicatorScreenPosition = touchIndicatorScreenPosition;
            PullDistance = pullDistance;
            PullOffset = pullOffset;
            NormalizedPull = normalizedPull;
            NormalizedPullOffset = normalizedPullOffset;
        }
    }
}
