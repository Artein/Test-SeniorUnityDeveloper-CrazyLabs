using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Gameplay
{
    public interface IRunBodySpeedConfig
    {
         float AboveMaximumSpeedResistance { get; }
         float BaseSoftMaximumSpeed { get; }
         float DownhillAcceleration { get; }
         float LowSpeedAssistAcceleration { get; }
         float LowSpeedAssistTargetSpeed { get; }
         float SurfaceSlowdown { get; }
    }

    public interface IRunBodyMovementValidityConfig
    {
         float MaximumSupportedSurfaceNormalLiftSpeed { get; }
         float RunBodySpeedSanityGuardMetersPerSecond { get; }
    }

    public interface IRunLaunchLandingStabilizationConfig
    {
         float LaunchLandingMaximumLiftSpeed { get; }
         float LaunchLandingStabilizationSeconds { get; }
    }

    public interface IRunSteeringConfig
    {
         float FallbackDpi { get; }
         float MaximumAcceptedDpi { get; }
         float MaximumTurnDegreesPerSecond { get; }
         float MinimumAcceptedDpi { get; }
         float MinimumSteerSpeed { get; }
         float RunAirSteeringMaximumTurnDegreesPerSecond { get; }
         float RunSteeringDeadzoneFraction { get; }
         float RunSteeringRangeCentimeters { get; }
         float RunSteeringResponsiveness { get; }
    }

    public interface IRunSurfaceStabilityAuthoringConfig
    {
         float CandidateCoherenceDegrees { get; }
         float DiscontinuousNormalConfirmationSeconds { get; }
         float DiscontinuousNormalThresholdDegrees { get; }
         float SupportLossConfirmationSeconds { get; }
    }

    public interface IRunSteeringFrameAuthoringConfig
    {
         float AirborneUpRetentionSeconds { get; }
         float NormalSlewDegreesPerSecond { get; }
    }

    public interface IRunSupportAttachmentAuthoringConfig
    {
         float MaximumAttachedSurfaceNormalLiftSpeed { get; }
         float MinimumReattachmentNormalChangeDegrees { get; }
         float SameSurfaceReattachmentSeparationMeters { get; }
         float TransitionConfirmationSeconds { get; }
    }

    [CreateAssetMenu(
        fileName = nameof(RunBodyMovementConfig),
        menuName = "Game/Gameplay/Run Body Movement Tuning")]
    public sealed partial class RunBodyMovementConfig : ScriptableObject,
        IRunBodySpeedConfig,
        IRunBodyMovementValidityConfig,
        IRunLaunchLandingStabilizationConfig,
        IRunSteeringConfig,
        IRunSurfaceStabilityAuthoringConfig,
        IRunSupportAttachmentAuthoringConfig,
        IRunSteeringFrameAuthoringConfig
    {
         [Header(header: "Run Body Speed - Slope"), SerializeField, Min(min: 0f), Tooltip(
               "Controls: Speed added per second while sliding downhill in the course-forward direction."
               + "\n\nImpact: Higher values build speed faster on steep aligned descents; flat, uphill, lateral, and reversed travel receive none."
               + "\n\nUnits: Meters per second of speed added each second at full downhill contribution.")]
        private float _downhillAcceleration = 8f;

         [Header(header: "Run Body Speed - Slowdown"), SerializeField, Min(min: 0f), Tooltip(
               "Controls: Speed removed per second from ordinary supported Run Surface motion."
               + "\n\nImpact: Higher values settle speed faster in every tangent direction; zero leaves slowdown to Rigidbody contacts and materials."
               + "\n\nUnits: Meters per second of speed removed each second.")]
        private float _surfaceSlowdown = 0.5f;

         [Header(header: "Run Body Speed - Low-Speed Assist"), SerializeField, Min(min: 0f), Tooltip(
               "Controls: Minimum useful slide speed that eligible low-speed assistance tries to recover toward."
               + "\n\nImpact: Helps a moving, course-aligned Run Body recover from a seam or soft scrape without creating a heading from rest."
               + "\n\nUnits: Meters per second. The effective target never exceeds the resolved soft maximum speed.")]
        private float _lowSpeedAssistTargetSpeed = 5f;

         [SerializeField, Min(min: 0f), Tooltip(
               "Controls: How quickly eligible low-speed assistance approaches its target."
               + "\n\nImpact: Higher values recover useful motion sooner, but the one-attempt velocity budget remains bounded by the initial deficit."
               + "\n\nUnits: Meters per second of speed added each second.")]
        private float _lowSpeedAssistAcceleration = 8f;

         [Header(header: "Run Body Speed - Soft Envelope"), SerializeField, Min(min: 0.01f), Tooltip(
               "Controls: Default comfort and readability speed before Player Max Speed upgrades."
               + "\n\nImpact: Positive model acceleration stops at this soft envelope; launch, gravity, and collisions may temporarily exceed it."
               + "\n\nUnits: Meters per second.")]
        private float _baseSoftMaximumSpeed = 20f;

         [SerializeField, Min(min: 0f), Tooltip(
               "Controls: Extra slowdown applied when supported tangent speed exceeds the resolved soft maximum."
               + "\n\nImpact: Higher values settle temporary overspeed sooner without hard-clamping launch or collision response."
               + "\n\nUnits: Meters per second of speed removed each second at twice the soft maximum.")]
        private float _aboveMaximumSpeedResistance = 12f;

         [Header(header: "Run Body Movement Validity"), SerializeField, Min(min: 0f), Tooltip(
               "Controls: Maximum speed away from the Run Surface normal that still counts as supported movement."
               + "\n\nImpact: Motion departing the surface faster than this receives no grounded speed policy."
               + "\n\nUnits: Meters per second.")]
        private float _maximumSupportedSurfaceNormalLiftSpeed;

         [Header(header: "Run Body Defensive Sanity"), SerializeField, Min(min: 0.0001f), Tooltip(
               "Controls: Defensive maximum Run Body speed accepted by movement calculations."
               + "\n\nImpact: Contains impossible or non-finite physics values before they contaminate movement."
               + "\n\nUnits: Meters per second. This is a sanity guard, not a player-facing speed cap.")]
        private float _runBodySpeedSanityGuardMetersPerSecond = 250f;

         [Header(header: "Launch Landing Stabilization"), SerializeField, Min(min: 0f), Tooltip(
               "Controls: Duration after first post-launch Run Surface landing where positive surface-normal lift is suppressed."
               + "\n\nImpact: Higher values stabilize early run entry for longer; lower values restore full surface-relative velocity sooner."
               + "\n\nUnits: Seconds after landing.")]
        private float _launchLandingStabilizationSeconds = 0.3f;

         [SerializeField, Min(min: 0f), Tooltip(
               "Controls: Maximum speed allowed away from the landed Run Surface normal during Launch Landing Stabilization."
               + "\n\nImpact: Higher values preserve more upward lift after landing; lower values produce a flatter run entry."
               + "\n\nUnits: Meters per second during the stabilization window.")]
        private float _launchLandingMaximumLiftSpeed;

         [Header(header: "Run Steering Control"), SerializeField, Min(min: 0.0001f), Tooltip(
               "Controls: Physical horizontal touch displacement that maps to full Run Steering Control output."
               + "\n\nImpact: Higher values require wider thumb motion; lower values make steering more sensitive."
               + "\n\nUnits: Centimeters, converted through the resolved screen DPI.")]
        private float _runSteeringRangeCentimeters = 1.5f;

         [SerializeField, Range(min: 0f, max: 0.95f), Tooltip(
               "Controls: Neutral portion near the Run Steering Origin as a fraction of Run Steering Range."
               + "\n\nImpact: Higher values filter more jitter before steering starts; lower values react to smaller thumb movement."
               + "\n\nUnits: Normalized fraction below full range.")]
        private float _runSteeringDeadzoneFraction = 0.15f;

         [SerializeField, Min(min: 0f), Tooltip(
               "Controls: How quickly applied steering catches up to requested Run Steering Control output."
               + "\n\nImpact: Lower values feel smoother and heavier; higher values feel more immediate."
               + "\n\nUnits: Normalized steering response per second.")]
        private float _runSteeringResponsiveness = 8f;

         [Header(header: "Run Steering Screen Metrics"), SerializeField, Min(min: 0.0001f), Tooltip(
               tooltip: "Fallback screen DPI used when the device reports an unknown, impossible, or out-of-range raw DPI.")]
        private float _fallbackDpi = 326f;

         [SerializeField, Min(min: 0.0001f), Tooltip(tooltip: "Minimum raw screen DPI accepted before fallback DPI is used.")]
        private float _minimumAcceptedDpi = 1f;

         [SerializeField, Min(min: 0.0001f), Tooltip(tooltip: "Maximum raw screen DPI accepted before fallback DPI is used.")]
        private float _maximumAcceptedDpi = 1000f;

         [Header(header: "Run Steering Direction"), SerializeField, Min(min: 0f), Tooltip(
               "Controls: Maximum grounded turn rate at full Run Steering Control output."
               + "\n\nImpact: Higher values allow sharper direction changes; this caps steering rotation, not forward speed."
               + "\n\nUnits: Degrees per second.")]
        private float _maximumTurnDegreesPerSecond = 120f;

         [SerializeField, Min(min: 0f), Tooltip(
               "Controls: Maximum airborne turn rate at full Run Steering Control output."
               + "\n\nImpact: Higher values give more in-air agency; this remains direction-only steering."
               + "\n\nUnits: Degrees per second.")]
        private float _runAirSteeringMaximumTurnDegreesPerSecond = 45f;

         [SerializeField, Min(min: 0f), Tooltip(
               "Controls: Minimum planar Run Body speed required before steering is applied."
               + "\n\nImpact: Higher values prevent steering while nearly stopped."
               + "\n\nUnits: Meters per second.")]
        private float _minimumSteerSpeed = 0.25f;

         [Header(header: "Run Steering Frame Stability"), SerializeField, Min(min: 0f), Tooltip(
               "Controls: Maximum angular speed used when the Run Steering Frame follows ordinary grounded Run Surface normal changes."
               + "\n\nUnits: Degrees per second.")]
        private float _runSteeringFrameNormalSlewDegreesPerSecond = 180f;

         [FormerlySerializedAs(oldName: "_runSteeringFrameSnapDegrees"), SerializeField, Range(min: 0f, max: 180f), Tooltip(
               "Controls: Ground-normal angle that marks Observed Support as discontinuous from Stable Support."
               + "\n\nUnits: Degrees from flat to opposite normals.")]
        private float _runSurfaceDiscontinuousNormalThresholdDegrees = 60f;

         [FormerlySerializedAs(oldName: "_runSteeringFrameUngroundedGraceSeconds"), SerializeField, Min(min: 0f), Tooltip(
               "Controls: Duration Stable Support is retained through consecutive Missing observations."
               + "\n\nUnits: Seconds.")]
        private float _runSurfaceSupportLossConfirmationSeconds = 0.08f;

         [FormerlySerializedAs(oldName: "_runSteeringFrameSuspectNormalConfirmationSeconds"), SerializeField, Min(min: 0f), Tooltip(
               "Controls: Duration a coherent discontinuous normal must persist before Stable Support accepts it."
               + "\n\nUnits: Seconds.")]
        private float _runSurfaceDiscontinuousNormalConfirmationSeconds = 0.04f;

         [SerializeField, Range(min: 0f, max: 180f), Tooltip(
               "Controls: Maximum angular difference between discontinuous candidate normals that may confirm together."
               + "\n\nUnits: Degrees.")]
        private float _runSurfaceCandidateCoherenceDegrees = 1f;

         [Header(header: "Run Support Attachment"), SerializeField, Min(min: 0f), Tooltip(
               "Controls: Outward speed along Observed Support normal that starts a physical detachment episode."
               + "\n\nUnits: Meters per second.")]
        private float _runSupportMaximumAttachedSurfaceNormalLiftSpeed = 0.35f;

         [SerializeField, Min(min: 0f), Tooltip(
               "Controls: Distance from the detachment plane that must be crossed before the same surface can be reattached."
               + "\n\nUnits: Meters.")]
        private float _runSupportSameSurfaceReattachmentSeparationMeters = 0.08f;

         [SerializeField, Range(min: 0f, max: 180f), Tooltip(
               "Controls: Normal change that identifies a distinct landing surface without requiring return to the detachment plane."
               + "\n\nUnits: Degrees.")]
        private float _runSupportMinimumReattachmentNormalChangeDegrees = 30f;

         [SerializeField, Min(min: 0f), Tooltip(
               "Controls: Duration a detachment or reattachment condition must remain coherent before transition."
               + "\n\nUnits: Seconds.")]
        private float _runSupportAttachmentTransitionConfirmationSeconds = 0.04f;

         [SerializeField, Min(min: 0f), Tooltip(
               "Controls: Duration the last valid steering up frame remains available after Stable Support is lost."
               + "\n\nUnits: Seconds.")]
        private float _runSteeringFrameAirborneUpRetentionSeconds = 0.12f;

         float IRunBodySpeedConfig.AboveMaximumSpeedResistance => _aboveMaximumSpeedResistance;
         float IRunSteeringFrameAuthoringConfig.AirborneUpRetentionSeconds => _runSteeringFrameAirborneUpRetentionSeconds;
         float IRunBodySpeedConfig.BaseSoftMaximumSpeed => _baseSoftMaximumSpeed;

         float IRunSurfaceStabilityAuthoringConfig.CandidateCoherenceDegrees => _runSurfaceCandidateCoherenceDegrees;

         float IRunSurfaceStabilityAuthoringConfig.DiscontinuousNormalConfirmationSeconds =>
            _runSurfaceDiscontinuousNormalConfirmationSeconds;

         float IRunSurfaceStabilityAuthoringConfig.DiscontinuousNormalThresholdDegrees => _runSurfaceDiscontinuousNormalThresholdDegrees;

         float IRunBodySpeedConfig.DownhillAcceleration => _downhillAcceleration;
         float IRunSteeringConfig.FallbackDpi => _fallbackDpi;
         float IRunLaunchLandingStabilizationConfig.LaunchLandingMaximumLiftSpeed => _launchLandingMaximumLiftSpeed;

         float IRunLaunchLandingStabilizationConfig.LaunchLandingStabilizationSeconds => _launchLandingStabilizationSeconds;
         float IRunBodySpeedConfig.LowSpeedAssistAcceleration => _lowSpeedAssistAcceleration;
         float IRunBodySpeedConfig.LowSpeedAssistTargetSpeed => _lowSpeedAssistTargetSpeed;
         float IRunSteeringConfig.MaximumAcceptedDpi => _maximumAcceptedDpi;

         float IRunSupportAttachmentAuthoringConfig.MaximumAttachedSurfaceNormalLiftSpeed =>
            _runSupportMaximumAttachedSurfaceNormalLiftSpeed;

         float IRunBodyMovementValidityConfig.MaximumSupportedSurfaceNormalLiftSpeed => _maximumSupportedSurfaceNormalLiftSpeed;
         float IRunSteeringConfig.MaximumTurnDegreesPerSecond => _maximumTurnDegreesPerSecond;
         float IRunSteeringConfig.MinimumAcceptedDpi => _minimumAcceptedDpi;

         float IRunSupportAttachmentAuthoringConfig.MinimumReattachmentNormalChangeDegrees =>
            _runSupportMinimumReattachmentNormalChangeDegrees;

         float IRunSteeringConfig.MinimumSteerSpeed => _minimumSteerSpeed;

         float IRunSteeringFrameAuthoringConfig.NormalSlewDegreesPerSecond => _runSteeringFrameNormalSlewDegreesPerSecond;
         float IRunSteeringConfig.RunAirSteeringMaximumTurnDegreesPerSecond => _runAirSteeringMaximumTurnDegreesPerSecond;
         float IRunBodyMovementValidityConfig.RunBodySpeedSanityGuardMetersPerSecond => _runBodySpeedSanityGuardMetersPerSecond;
         float IRunSteeringConfig.RunSteeringDeadzoneFraction => _runSteeringDeadzoneFraction;

         float IRunSteeringConfig.RunSteeringRangeCentimeters => _runSteeringRangeCentimeters;
         float IRunSteeringConfig.RunSteeringResponsiveness => _runSteeringResponsiveness;

         float IRunSupportAttachmentAuthoringConfig.SameSurfaceReattachmentSeparationMeters =>
            _runSupportSameSurfaceReattachmentSeparationMeters;

         float IRunSurfaceStabilityAuthoringConfig.SupportLossConfirmationSeconds => _runSurfaceSupportLossConfirmationSeconds;
         float IRunBodySpeedConfig.SurfaceSlowdown => _surfaceSlowdown;

         float IRunSupportAttachmentAuthoringConfig.TransitionConfirmationSeconds =>
            _runSupportAttachmentTransitionConfirmationSeconds;

         private void OnValidate()
        {
            var validator = new RunBodyMovementConfigValidator();

            foreach (var error in validator.Validate(this))
            {
                Debug.LogWarning(error, this);
            }
        }
    }
}
