using System;
using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunSteeringFrameSource
    {
        Vector3 GetUpDirection(Vector3 fallbackUpDirection);
    }

    internal sealed class RunSurfaceSteeringFrameSource : IRunSteeringFrameSource
    {
        private const float MinimumUpSqrMagnitude = 0.000001f;

        private readonly IRunSurfaceContextSource _surfaceContextSource;

        public RunSurfaceSteeringFrameSource(IRunSurfaceContextSource surfaceContextSource)
        {
            _surfaceContextSource = surfaceContextSource ?? throw new ArgumentNullException(nameof(surfaceContextSource));
        }

        public Vector3 GetUpDirection(Vector3 fallbackUpDirection)
        {
            var fallbackUp = GetValidUpDirection(fallbackUpDirection, Vector3.up);
            var surfaceContext = _surfaceContextSource.Current;

            if (!surfaceContext.IsGrounded)
                return fallbackUp;

            if (!surfaceContext.HasValidGroundNormal)
                return fallbackUp;

            return GetValidUpDirection(surfaceContext.GroundNormal, fallbackUp);
        }

        private Vector3 GetValidUpDirection(Vector3 upDirection, Vector3 fallbackUpDirection)
        {
            var sqrMagnitude = upDirection.sqrMagnitude;

            if (sqrMagnitude <= MinimumUpSqrMagnitude || float.IsNaN(sqrMagnitude) || float.IsInfinity(sqrMagnitude))
                return fallbackUpDirection;

            var normalized = upDirection.normalized;

            return float.IsNaN(normalized.sqrMagnitude) || float.IsInfinity(normalized.sqrMagnitude)
                ? fallbackUpDirection
                : normalized;
        }
    }
}
