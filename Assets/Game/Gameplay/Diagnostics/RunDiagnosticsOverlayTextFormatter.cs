using System.Collections.Generic;

namespace Game.Gameplay.Diagnostics
{
    internal sealed class RunDiagnosticsOverlayTextFormatter
    {
        public string FormatMotionSummary(RunDiagnosticsOverlaySample sample)
        {
            return $"G:{(sample.IsGrounded ? 1 : 0)} fx:{sample.FixedStepsThisFrame} "
                   + $"snap:{FormatSnapReason(sample.EstimatedVisualSnapReason)} "
                   + $"rb-tgt:{sample.TargetToMotionCentimeters:0.0}cm "
                   + $"camrot:{sample.CameraRotationDeltaDegrees:0.0}";
        }

        public string FormatRunBodySpeed(RunBodySpeedDiagnosticsSnapshot snapshot)
        {
            if (snapshot.State != RunBodySpeedDiagnosticsState.Active)
                return "Run Body Speed | state:inactive";

            return $"Run Body Speed | state:active grounded:{FormatBoolean(snapshot.IsRunSurfaceGrounded)} "
                   + $"support:{(snapshot.HasValidGroundedRunSurface ? "valid" : "invalid")} "
                   + $"direction:{(snapshot.HasUsableTangentDirection ? "valid" : "unavailable")} "
                   + $"speed:{snapshot.SampledTangentSpeed:0.0}/{snapshot.EffectiveSoftMaximumSpeed:0.0}m/s "
                   + $"downhill:{snapshot.ForwardDownhillDegrees:0.0}deg "
                   + $"align:{snapshot.CourseForwardAlignment:0.00} "
                   + $"effects:{FormatContributors(snapshot.Contributors)}";
        }

        public string FormatLowSpeedAssist(RunBodySpeedDiagnosticsSnapshot snapshot)
        {
            if (snapshot.State != RunBodySpeedDiagnosticsState.Active)
                return "Low-Speed Assist | state:unavailable";

            return $"Low-Speed Assist | target:{snapshot.EffectiveLowSpeedAssistTargetSpeed:0.0}m/s "
                   + $"state:{snapshot.LowSpeedAssistAttemptState} "
                   + $"eligible:{FormatBoolean(snapshot.IsLowSpeedAssistEligible)} "
                   + $"budget:{snapshot.RemainingRequestedLowSpeedAssistVelocityBudget:0.0}m/s";
        }

        private string FormatContributors(RunBodySpeedDecisionContributors contributors)
        {
            if (contributors == RunBodySpeedDecisionContributors.None)
                return "none";

            var labels = new List<string>(4);

            AddContributor(
                labels,
                contributors,
                RunBodySpeedDecisionContributors.DownhillAcceleration,
                "downhill");

            AddContributor(
                labels,
                contributors,
                RunBodySpeedDecisionContributors.SurfaceSlowdown,
                "slowdown");

            AddContributor(
                labels,
                contributors,
                RunBodySpeedDecisionContributors.AboveEnvelopeResistance,
                "above-envelope");

            AddContributor(
                labels,
                contributors,
                RunBodySpeedDecisionContributors.LowSpeedAssist,
                "low-speed-assist");

            return labels.Count > 0 ? string.Join("+", labels) : "unknown";
        }

        private void AddContributor(
            ICollection<string> labels,
            RunBodySpeedDecisionContributors contributors,
            RunBodySpeedDecisionContributors contributor,
            string label)
        {
            if ((contributors & contributor) != 0)
                labels.Add(label);
        }

        private string FormatBoolean(bool value)
        {
            return value ? "yes" : "no";
        }

        private string FormatSnapReason(RunDiagnosticsOverlaySnapReason reason)
        {
            switch (reason)
            {
                case RunDiagnosticsOverlaySnapReason.Position:
                    return "pos";
                case RunDiagnosticsOverlaySnapReason.Rotation:
                    return "rot";
                case RunDiagnosticsOverlaySnapReason.PositionAndRotation:
                    return "pos+rot";
                default:
                    return "-";
            }
        }
    }
}
