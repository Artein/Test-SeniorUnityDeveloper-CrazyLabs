using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunSurfaceSlopeCalculator
    {
        float CalculateForwardDownhillDegrees(Vector3 groundNormal, RunProgressFrameSnapshot frame);
    }

    internal sealed class RunSurfaceSlopeCalculator : IRunSurfaceSlopeCalculator
    {
        private const float MinimumMagnitude = 0.000001f;

        public float CalculateForwardDownhillDegrees(Vector3 groundNormal, RunProgressFrameSnapshot frame)
        {
            if (!frame.IsValid || !TryNormalize(groundNormal, out var normalizedGroundNormal))
                return 0f;

            var forwardOnSurface = Vector3.ProjectOnPlane(frame.ForwardDirection, normalizedGroundNormal);

            if (!TryNormalize(forwardOnSurface, out var normalizedForwardOnSurface))
                return 0f;

            var verticalDrop = -Vector3.Dot(normalizedForwardOnSurface, frame.UpDirection);
            return Mathf.Asin(Mathf.Clamp(verticalDrop, -1f, 1f)) * Mathf.Rad2Deg;
        }

        private bool TryNormalize(Vector3 value, out Vector3 normalized)
        {
            normalized = default;

            if (!value.IsFinite() || value.sqrMagnitude <= MinimumMagnitude)
                return false;

            normalized = value.normalized;
            return normalized.IsFinite();
        }
    }
}
