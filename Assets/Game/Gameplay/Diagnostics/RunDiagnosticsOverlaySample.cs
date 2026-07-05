using UnityEngine;

namespace Game.Gameplay.Diagnostics
{
    internal readonly struct RunDiagnosticsOverlaySample
    {
        public float SpeedMetersPerSecond { get; }
        public float MotionStepMetersPerSecond { get; }
        public float VisualTargetStepMetersPerSecond { get; }
        public float VisualTargetStepMeters { get; }
        public float RawGroundNormalDeltaDegrees { get; }
        public float SteeringUpDeltaDegrees { get; }
        public float VisualLagCentimeters { get; }
        public float CameraStepMetersPerSecond { get; }
        public float TargetToMotionCentimeters { get; }
        public float VisualTargetRotationDeltaDegrees { get; }
        public float VisualRotationDeltaDegrees { get; }
        public float CameraRotationDeltaDegrees { get; }
        public RunDiagnosticsOverlaySnapReason EstimatedVisualSnapReason { get; }
        public bool HasEstimatedVisualSnap => EstimatedVisualSnapReason != RunDiagnosticsOverlaySnapReason.None;
        public int FixedStepsThisFrame { get; }
        public bool IsGrounded { get; }

        public RunDiagnosticsOverlaySample(
            float speedMetersPerSecond,
            float motionStepMetersPerSecond,
            float visualTargetStepMetersPerSecond,
            float visualTargetStepMeters,
            float rawGroundNormalDeltaDegrees,
            float steeringUpDeltaDegrees,
            float visualLagCentimeters,
            float cameraStepMetersPerSecond,
            float targetToMotionCentimeters,
            float visualTargetRotationDeltaDegrees,
            float visualRotationDeltaDegrees,
            float cameraRotationDeltaDegrees,
            RunDiagnosticsOverlaySnapReason estimatedVisualSnapReason,
            int fixedStepsThisFrame,
            bool isGrounded)
        {
            SpeedMetersPerSecond = float.IsFinite(speedMetersPerSecond) ? Mathf.Max(0f, speedMetersPerSecond) : 0f;
            MotionStepMetersPerSecond = float.IsFinite(motionStepMetersPerSecond) ? Mathf.Max(0f, motionStepMetersPerSecond) : 0f;
            VisualTargetStepMetersPerSecond = float.IsFinite(visualTargetStepMetersPerSecond) ? Mathf.Max(0f, visualTargetStepMetersPerSecond) : 0f;
            VisualTargetStepMeters = float.IsFinite(visualTargetStepMeters) ? Mathf.Max(0f, visualTargetStepMeters) : 0f;
            RawGroundNormalDeltaDegrees = float.IsFinite(rawGroundNormalDeltaDegrees) ? Mathf.Clamp(rawGroundNormalDeltaDegrees, 0f, 180f) : 0f;
            SteeringUpDeltaDegrees = float.IsFinite(steeringUpDeltaDegrees) ? Mathf.Clamp(steeringUpDeltaDegrees, 0f, 180f) : 0f;
            VisualLagCentimeters = float.IsFinite(visualLagCentimeters) ? Mathf.Max(0f, visualLagCentimeters) : 0f;
            CameraStepMetersPerSecond = float.IsFinite(cameraStepMetersPerSecond) ? Mathf.Max(0f, cameraStepMetersPerSecond) : 0f;
            TargetToMotionCentimeters = float.IsFinite(targetToMotionCentimeters) ? Mathf.Max(0f, targetToMotionCentimeters) : 0f;
            VisualTargetRotationDeltaDegrees = float.IsFinite(visualTargetRotationDeltaDegrees) ? Mathf.Clamp(visualTargetRotationDeltaDegrees, 0f, 180f) : 0f;
            VisualRotationDeltaDegrees = float.IsFinite(visualRotationDeltaDegrees) ? Mathf.Clamp(visualRotationDeltaDegrees, 0f, 180f) : 0f;
            CameraRotationDeltaDegrees = float.IsFinite(cameraRotationDeltaDegrees) ? Mathf.Clamp(cameraRotationDeltaDegrees, 0f, 180f) : 0f;
            EstimatedVisualSnapReason = estimatedVisualSnapReason;
            FixedStepsThisFrame = Mathf.Max(0, fixedStepsThisFrame);
            IsGrounded = isGrounded;
        }
    }
}
