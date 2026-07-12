using UnityEngine;

namespace Game.Gameplay.Diagnostics
{
    internal readonly struct RunDiagnosticsOverlaySample
    {
        public float SpeedMetersPerSecond { get; }
        public float MotionStepMetersPerSecond { get; }
        public float VisualTargetStepMetersPerSecond { get; }
        public float VisualTargetStepMeters { get; }
        public float ObservedGroundNormalDeltaDegrees { get; }
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
        public RunSupportObservationState ObservedSupportState { get; }
        public bool HasObservedGroundNormal { get; }
        public Vector3 ObservedGroundNormal { get; }
        public bool IsStableGrounded { get; }
        public bool HasStableGroundNormal { get; }
        public Vector3 StableGroundNormal { get; }
        public RunSurfaceTransition SurfaceTransition { get; }
        public RunSupportAttachmentTransition AttachmentTransition { get; }
        public bool IsMissingSupportHeld { get; }
        public bool IsConfirmingDiscontinuity { get; }
        public bool IsSteeringFrameValid { get; }
        public Vector3 SteeringUpDirection { get; }
        public RunBodySpeedDiagnosticsSnapshot SpeedDiagnostics { get; }

        public RunDiagnosticsOverlaySample(
            float speedMetersPerSecond,
            float motionStepMetersPerSecond,
            float visualTargetStepMetersPerSecond,
            float visualTargetStepMeters,
            float observedGroundNormalDeltaDegrees,
            float steeringUpDeltaDegrees,
            float visualLagCentimeters,
            float cameraStepMetersPerSecond,
            float targetToMotionCentimeters,
            float visualTargetRotationDeltaDegrees,
            float visualRotationDeltaDegrees,
            float cameraRotationDeltaDegrees,
            RunDiagnosticsOverlaySnapReason estimatedVisualSnapReason,
            int fixedStepsThisFrame,
            RunSurfaceFrameSnapshot surfaceFrame,
            RunBodySpeedDiagnosticsSnapshot speedDiagnostics)
        {
            SpeedMetersPerSecond = float.IsFinite(speedMetersPerSecond) ? Mathf.Max(a: 0f, speedMetersPerSecond) : 0f;
            MotionStepMetersPerSecond = float.IsFinite(motionStepMetersPerSecond) ? Mathf.Max(a: 0f, motionStepMetersPerSecond) : 0f;

            VisualTargetStepMetersPerSecond =
                float.IsFinite(visualTargetStepMetersPerSecond) ? Mathf.Max(a: 0f, visualTargetStepMetersPerSecond) : 0f;

            VisualTargetStepMeters = float.IsFinite(visualTargetStepMeters) ? Mathf.Max(a: 0f, visualTargetStepMeters) : 0f;

            ObservedGroundNormalDeltaDegrees = float.IsFinite(observedGroundNormalDeltaDegrees)
                ? Mathf.Clamp(observedGroundNormalDeltaDegrees, min: 0f, max: 180f)
                : 0f;

            SteeringUpDeltaDegrees = float.IsFinite(steeringUpDeltaDegrees) ? Mathf.Clamp(steeringUpDeltaDegrees, min: 0f, max: 180f) : 0f;
            VisualLagCentimeters = float.IsFinite(visualLagCentimeters) ? Mathf.Max(a: 0f, visualLagCentimeters) : 0f;
            CameraStepMetersPerSecond = float.IsFinite(cameraStepMetersPerSecond) ? Mathf.Max(a: 0f, cameraStepMetersPerSecond) : 0f;
            TargetToMotionCentimeters = float.IsFinite(targetToMotionCentimeters) ? Mathf.Max(a: 0f, targetToMotionCentimeters) : 0f;

            VisualTargetRotationDeltaDegrees =
                float.IsFinite(visualTargetRotationDeltaDegrees) ? Mathf.Clamp(visualTargetRotationDeltaDegrees, min: 0f, max: 180f) : 0f;

            VisualRotationDeltaDegrees = float.IsFinite(visualRotationDeltaDegrees)
                ? Mathf.Clamp(visualRotationDeltaDegrees, min: 0f, max: 180f)
                : 0f;

            CameraRotationDeltaDegrees = float.IsFinite(cameraRotationDeltaDegrees)
                ? Mathf.Clamp(cameraRotationDeltaDegrees, min: 0f, max: 180f)
                : 0f;

            EstimatedVisualSnapReason = estimatedVisualSnapReason;
            FixedStepsThisFrame = Mathf.Max(a: 0, fixedStepsThisFrame);
            ObservedSupportState = surfaceFrame.ObservedSupport.State;

            HasObservedGroundNormal = ObservedSupportState == RunSupportObservationState.Supported
                                      && surfaceFrame.ObservedSupport.SurfaceContext.HasValidGroundNormal;

            ObservedGroundNormal = HasObservedGroundNormal
                ? surfaceFrame.ObservedSupport.SurfaceContext.GroundNormal
                : Vector3.up;

            IsStableGrounded = surfaceFrame.StableSupport.IsGrounded;
            HasStableGroundNormal = IsStableGrounded && surfaceFrame.StableSupport.HasValidGroundNormal;
            StableGroundNormal = HasStableGroundNormal ? surfaceFrame.StableSupport.GroundNormal : Vector3.up;
            SurfaceTransition = surfaceFrame.Transition;
            AttachmentTransition = surfaceFrame.AttachmentTransition;
            IsMissingSupportHeld = surfaceFrame.IsMissingSupportHeld;
            IsConfirmingDiscontinuity = surfaceFrame.IsConfirmingDiscontinuity;
            IsSteeringFrameValid = surfaceFrame.SteeringFrame.IsValid;
            SteeringUpDirection = IsSteeringFrameValid ? surfaceFrame.SteeringFrame.UpDirection : Vector3.up;
            SpeedDiagnostics = speedDiagnostics;
        }

        public float SelectMetric(int metricIndex)
        {
            return metricIndex switch
            {
                0 => SpeedMetersPerSecond,
                1 => MotionStepMetersPerSecond,
                2 => VisualTargetStepMetersPerSecond,
                3 => ObservedGroundNormalDeltaDegrees,
                4 => SteeringUpDeltaDegrees,
                5 => HasEstimatedVisualSnap ? 1f : 0f,
                6 => VisualTargetRotationDeltaDegrees,
                7 => VisualRotationDeltaDegrees,
                8 => VisualLagCentimeters,
                9 => CameraStepMetersPerSecond,
                _ => 0f
            };
        }
    }
}
