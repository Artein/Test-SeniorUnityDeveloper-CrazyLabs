using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.Diagnostics
{
    internal static class RunDiagnosticsOverlayTextFormatter
    {
        public static string FormatMotionSummary(RunDiagnosticsOverlaySample sample)
        {
            return $"observed:{sample.ObservedSupportState} "
                   + $"normal:{FormatDirection(sample.HasObservedGroundNormal, sample.ObservedGroundNormal)} "
                   + $"stable:{(sample.IsStableGrounded ? "grounded" : "unsupported")} "
                   + $"normal:{FormatDirection(sample.HasStableGroundNormal, sample.StableGroundNormal)} "
                   + $"transition:{sample.SurfaceTransition} "
                   + $"attachment:{sample.AttachmentTransition} "
                   + $"held:{FormatBoolean(sample.IsMissingSupportHeld)} "
                   + $"confirming:{FormatBoolean(sample.IsConfirmingDiscontinuity)} "
                   + $"steering:{(sample.IsSteeringFrameValid ? "valid" : "unavailable")} "
                   + $"up:{FormatDirection(sample.IsSteeringFrameValid, sample.SteeringUpDirection)} "
                   + $"fx:{sample.FixedStepsThisFrame} "
                   + $"snap:{FormatSnapReason(sample.EstimatedVisualSnapReason)} "
                   + $"rb-tgt:{sample.TargetToMotionCentimeters:0.0}cm "
                   + $"camrot:{sample.CameraRotationDeltaDegrees:0.0}";
        }

        public static string FormatRunBodySpeed(RunBodySpeedDiagnosticsSnapshot snapshot)
        {
            if (snapshot.State != RunBodySpeedDiagnosticsState.Active)
                return "Run Body Speed | state:inactive";

            return $"Run Body Speed | state:active grounded:{FormatBoolean(snapshot.IsRunSurfaceGrounded)} "
                   + $"support:{(snapshot.HasValidGroundedRunSurface ? "valid" : "invalid")} "
                   + $"direction:{(snapshot.HasUsableTangentDirection ? "valid" : "unavailable")} "
                   + $"speed:{snapshot.SampledTangentSpeed:0.0}/{snapshot.EffectiveSoftMaximumSpeed:0.0}m/s "
                   + $"downhill:{snapshot.ForwardDownhillDegrees:0.0}deg "
                   + $"align:{snapshot.CourseForwardAlignment:0.00} "
                   + $"policy:{FormatContributors(snapshot.PolicyContributors)} "
                   + $"requested:{FormatContributors(snapshot.RequestedContributors)}";
        }

        public static string FormatLowSpeedAssist(RunBodySpeedDiagnosticsSnapshot snapshot)
        {
            if (snapshot.State != RunBodySpeedDiagnosticsState.Active)
                return "Low-Speed Assist | state:unavailable";

            return $"Low-Speed Assist | target:{snapshot.EffectiveLowSpeedAssistTargetSpeed:0.0}m/s "
                   + $"state:{snapshot.LowSpeedAssistAttemptState} "
                   + $"conditions:{FormatBoolean(snapshot.MeetsLowSpeedAssistPolicyConditions)} "
                   + $"request:{snapshot.RequestedLowSpeedAssistVelocityDelta:+0.0;-0.0;0.0}m/s "
                   + $"budget:{snapshot.RemainingRequestedLowSpeedAssistVelocityBudget:0.0}m/s";
        }

        private static string FormatContributors(RunBodySpeedDecisionContributors contributors)
        {
            if (contributors == RunBodySpeedDecisionContributors.None)
                return "none";

            var labels = new List<string>(capacity: 4);

            AddContributor(
                labels,
                contributors,
                RunBodySpeedDecisionContributors.DownhillAcceleration,
                label: "downhill");

            AddContributor(
                labels,
                contributors,
                RunBodySpeedDecisionContributors.SurfaceSlowdown,
                label: "slowdown");

            AddContributor(
                labels,
                contributors,
                RunBodySpeedDecisionContributors.AboveEnvelopeResistance,
                label: "above-envelope");

            AddContributor(
                labels,
                contributors,
                RunBodySpeedDecisionContributors.LowSpeedAssist,
                label: "low-speed-assist");

            return labels.Count > 0 ? string.Join(separator: "+", labels) : "unknown";
        }

        private static void AddContributor(
            ICollection<string> labels,
            RunBodySpeedDecisionContributors contributors,
            RunBodySpeedDecisionContributors contributor,
            string label)
        {
            if ((contributors & contributor) != 0)
                labels.Add(label);
        }

        private static string FormatBoolean(bool value)
        {
            return value ? "yes" : "no";
        }

        private static string FormatDirection(bool isValid, Vector3 direction)
        {
            return isValid ? $"({direction.x:0.000},{direction.y:0.000},{direction.z:0.000})" : "n/a";
        }

        private static string FormatSnapReason(RunDiagnosticsOverlaySnapReason reason)
        {
            return reason switch
            {
                RunDiagnosticsOverlaySnapReason.Position => "pos",
                RunDiagnosticsOverlaySnapReason.Rotation => "rot",
                RunDiagnosticsOverlaySnapReason.PositionAndRotation => "pos+rot",
                _ => "-"
            };
        }
    }
}
