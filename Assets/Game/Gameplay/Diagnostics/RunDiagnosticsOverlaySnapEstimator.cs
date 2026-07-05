using Game.Gameplay.CharacterPresentation;
using UnityEngine;

namespace Game.Gameplay.Diagnostics
{
    internal sealed class RunDiagnosticsOverlaySnapEstimator
    {
        private readonly float _visualLagSnapEpsilonCentimeters = 0.5f;

        public RunDiagnosticsOverlaySnapReason Estimate(
            float visualTargetStepMeters,
            float visualTargetRotationDeltaDegrees,
            float visualLagCentimeters,
            ICharacterVisualFollowTuning tuning)
        {
            if (tuning == null || visualLagCentimeters > _visualLagSnapEpsilonCentimeters)
                return RunDiagnosticsOverlaySnapReason.None;

            var hasPositionSnap = visualTargetStepMeters >= Mathf.Max(0.0001f, tuning.VisualSnapDistance);
            var hasRotationSnap = visualTargetRotationDeltaDegrees >= Mathf.Clamp(tuning.VisualSnapAngleDegrees, 0f, 180f);

            if (hasPositionSnap && hasRotationSnap)
                return RunDiagnosticsOverlaySnapReason.PositionAndRotation;

            if (hasPositionSnap)
                return RunDiagnosticsOverlaySnapReason.Position;

            return hasRotationSnap ? RunDiagnosticsOverlaySnapReason.Rotation : RunDiagnosticsOverlaySnapReason.None;
        }
    }
}
