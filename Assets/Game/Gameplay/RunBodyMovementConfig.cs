using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunBodySpeedConfig
    {
        float DownhillAcceleration { get; }
        float SurfaceSlowdown { get; }
        float LowSpeedAssistTargetSpeed { get; }
        float LowSpeedAssistAcceleration { get; }
        float BaseSoftMaximumSpeed { get; }
        float AboveMaximumSpeedResistance { get; }
    }

    public interface IRunBodyMovementValidityConfig
    {
        float MaximumSupportedSurfaceNormalLiftSpeed { get; }
        float RunBodySpeedSanityGuardMetersPerSecond { get; }
    }

    public interface IRunLaunchLandingStabilizationConfig
    {
        float LaunchLandingStabilizationSeconds { get; }
        float LaunchLandingMaximumLiftSpeed { get; }
    }

    public interface IRunSteeringConfig
    {
        float RunSteeringRangeCentimeters { get; }
        float RunSteeringDeadzoneFraction { get; }
        float RunSteeringResponsiveness { get; }
        float FallbackDpi { get; }
        float MinimumAcceptedDpi { get; }
        float MaximumAcceptedDpi { get; }
        float MaximumTurnDegreesPerSecond { get; }
        float RunAirSteeringMaximumTurnDegreesPerSecond { get; }
        float MinimumSteerSpeed { get; }
    }

    public interface IRunSteeringFrameConfig
    {
        float RunSteeringFrameNormalSlewDegreesPerSecond { get; }
        float RunSteeringFrameSnapDegrees { get; }
        float RunSteeringFrameUngroundedGraceSeconds { get; }
        float RunSteeringFrameSuspectNormalConfirmationSeconds { get; }
    }

    [CreateAssetMenu(
        fileName = nameof(RunBodyMovementConfig),
        menuName = "Game/Gameplay/Run Body Movement Tuning")]
    public sealed partial class RunBodyMovementConfig : ScriptableObject,
        IRunBodySpeedConfig,
        IRunBodyMovementValidityConfig,
        IRunLaunchLandingStabilizationConfig,
        IRunSteeringConfig,
        IRunSteeringFrameConfig
    {
        [Header("Run Body Speed - Slope")]
        [SerializeField, Min(0f), Tooltip(
             "Controls: Speed added per second while sliding downhill in the course-forward direction."
             + "\n\nImpact: Higher values build speed faster on steep aligned descents; flat, uphill, lateral, and reversed travel receive none."
             + "\n\nUnits: Meters per second of speed added each second at full downhill contribution.")]
        private float _downhillAcceleration = 8f;

        [Header("Run Body Speed - Slowdown")]
        [SerializeField, Min(0f), Tooltip(
             "Controls: Speed removed per second from ordinary supported Run Surface motion."
             + "\n\nImpact: Higher values settle speed faster in every tangent direction; zero leaves slowdown to Rigidbody contacts and materials."
             + "\n\nUnits: Meters per second of speed removed each second.")]
        private float _surfaceSlowdown = 0.5f;

        [Header("Run Body Speed - Low-Speed Assist")]
        [SerializeField, Min(0f), Tooltip(
             "Controls: Minimum useful slide speed that eligible low-speed assistance tries to recover toward."
             + "\n\nImpact: Helps a moving, course-aligned Run Body recover from a seam or soft scrape without creating a heading from rest."
             + "\n\nUnits: Meters per second. The effective target never exceeds the resolved soft maximum speed.")]
        private float _lowSpeedAssistTargetSpeed = 5f;

        [SerializeField, Min(0f), Tooltip(
             "Controls: How quickly eligible low-speed assistance approaches its target."
             + "\n\nImpact: Higher values recover useful motion sooner, but the one-attempt velocity budget remains bounded by the initial deficit."
             + "\n\nUnits: Meters per second of speed added each second.")]
        private float _lowSpeedAssistAcceleration = 8f;

        [Header("Run Body Speed - Soft Envelope")]
        [SerializeField, Min(0.01f), Tooltip(
             "Controls: Default comfort and readability speed before Player Max Speed upgrades."
             + "\n\nImpact: Positive model acceleration stops at this soft envelope; launch, gravity, and collisions may temporarily exceed it."
             + "\n\nUnits: Meters per second.")]
        private float _baseSoftMaximumSpeed = 20f;

        [SerializeField, Min(0f), Tooltip(
             "Controls: Extra slowdown applied when supported tangent speed exceeds the resolved soft maximum."
             + "\n\nImpact: Higher values settle temporary overspeed sooner without hard-clamping launch or collision response."
             + "\n\nUnits: Meters per second of speed removed each second at twice the soft maximum.")]
        private float _aboveMaximumSpeedResistance = 12f;

        [Header("Run Body Movement Validity")]
        [SerializeField, Min(0f), Tooltip(
             "Controls: Maximum speed away from the Run Surface normal that still counts as supported movement."
             + "\n\nImpact: Motion departing the surface faster than this receives no grounded speed policy."
             + "\n\nUnits: Meters per second.")]
        private float _maximumSupportedSurfaceNormalLiftSpeed;

        [Header("Run Body Defensive Sanity")]
        [SerializeField, Min(0.0001f), Tooltip(
             "Controls: Defensive maximum Run Body speed accepted by movement calculations."
             + "\n\nImpact: Contains impossible or non-finite physics values before they contaminate movement."
             + "\n\nUnits: Meters per second. This is a sanity guard, not a player-facing speed cap.")]
        private float _runBodySpeedSanityGuardMetersPerSecond = 250f;

        [Header("Launch Landing Stabilization")]
        [SerializeField, Min(0f), Tooltip(
             "Controls: Duration after first post-launch Run Surface landing where positive surface-normal lift is suppressed."
             + "\n\nImpact: Higher values stabilize early run entry for longer; lower values restore full surface-relative velocity sooner."
             + "\n\nUnits: Seconds after landing.")]
        private float _launchLandingStabilizationSeconds = 0.3f;

        [SerializeField, Min(0f), Tooltip(
             "Controls: Maximum speed allowed away from the landed Run Surface normal during Launch Landing Stabilization."
             + "\n\nImpact: Higher values preserve more upward lift after landing; lower values produce a flatter run entry."
             + "\n\nUnits: Meters per second during the stabilization window.")]
        private float _launchLandingMaximumLiftSpeed;

        [Header("Run Steering Control")]
        [SerializeField, Min(0.0001f), Tooltip(
             "Controls: Physical horizontal touch displacement that maps to full Run Steering Control output."
             + "\n\nImpact: Higher values require wider thumb motion; lower values make steering more sensitive."
             + "\n\nUnits: Centimeters, converted through the resolved screen DPI.")]
        private float _runSteeringRangeCentimeters = 1.5f;

        [SerializeField, Range(0f, 0.95f), Tooltip(
             "Controls: Neutral portion near the Run Steering Origin as a fraction of Run Steering Range."
             + "\n\nImpact: Higher values filter more jitter before steering starts; lower values react to smaller thumb movement."
             + "\n\nUnits: Normalized fraction below full range.")]
        private float _runSteeringDeadzoneFraction = 0.15f;

        [SerializeField, Min(0f), Tooltip(
             "Controls: How quickly applied steering catches up to requested Run Steering Control output."
             + "\n\nImpact: Lower values feel smoother and heavier; higher values feel more immediate."
             + "\n\nUnits: Normalized steering response per second.")]
        private float _runSteeringResponsiveness = 8f;

        [Header("Run Steering Screen Metrics")]
        [SerializeField, Min(0.0001f), Tooltip(
             "Fallback screen DPI used when the device reports an unknown, impossible, or out-of-range raw DPI.")]
        private float _fallbackDpi = 326f;

        [SerializeField, Min(0.0001f), Tooltip("Minimum raw screen DPI accepted before fallback DPI is used.")]
        private float _minimumAcceptedDpi = 1f;

        [SerializeField, Min(0.0001f), Tooltip("Maximum raw screen DPI accepted before fallback DPI is used.")]
        private float _maximumAcceptedDpi = 1000f;

        [Header("Run Steering Direction")]
        [SerializeField, Min(0f), Tooltip(
             "Controls: Maximum grounded turn rate at full Run Steering Control output."
             + "\n\nImpact: Higher values allow sharper direction changes; this caps steering rotation, not forward speed."
             + "\n\nUnits: Degrees per second.")]
        private float _maximumTurnDegreesPerSecond = 120f;

        [SerializeField, Min(0f), Tooltip(
             "Controls: Maximum airborne turn rate at full Run Steering Control output."
             + "\n\nImpact: Higher values give more in-air agency; this remains direction-only steering."
             + "\n\nUnits: Degrees per second.")]
        private float _runAirSteeringMaximumTurnDegreesPerSecond = 45f;

        [SerializeField, Min(0f), Tooltip(
             "Controls: Minimum planar Run Body speed required before steering is applied."
             + "\n\nImpact: Higher values prevent steering while nearly stopped."
             + "\n\nUnits: Meters per second.")]
        private float _minimumSteerSpeed = 0.25f;

        [Header("Run Steering Frame Stability")]
        [SerializeField, Min(0f), Tooltip(
             "Controls: Maximum angular speed used when the Run Steering Frame follows ordinary grounded Run Surface normal changes."
             + "\n\nUnits: Degrees per second.")]
        private float _runSteeringFrameNormalSlewDegreesPerSecond = 180f;

        [SerializeField, Range(0f, 180f), Tooltip(
             "Controls: Ground-normal angle that marks a continuously grounded Run Steering Frame change as suspect."
             + "\n\nUnits: Degrees from flat to opposite normals.")]
        private float _runSteeringFrameSnapDegrees = 60f;

        [SerializeField, Min(0f), Tooltip(
             "Controls: Steering-only grace duration that keeps the last stable Run Steering Frame through short support misses."
             + "\n\nUnits: Seconds.")]
        private float _runSteeringFrameUngroundedGraceSeconds = 0.08f;

        [SerializeField, Min(0f), Tooltip(
             "Controls: Duration a large suspect Run Steering Frame normal must persist before it is accepted."
             + "\n\nUnits: Seconds.")]
        private float _runSteeringFrameSuspectNormalConfirmationSeconds = 0.04f;

        float IRunBodySpeedConfig.DownhillAcceleration => _downhillAcceleration;
        float IRunBodySpeedConfig.SurfaceSlowdown => _surfaceSlowdown;
        float IRunBodySpeedConfig.LowSpeedAssistTargetSpeed => _lowSpeedAssistTargetSpeed;
        float IRunBodySpeedConfig.LowSpeedAssistAcceleration => _lowSpeedAssistAcceleration;
        float IRunBodySpeedConfig.BaseSoftMaximumSpeed => _baseSoftMaximumSpeed;
        float IRunBodySpeedConfig.AboveMaximumSpeedResistance => _aboveMaximumSpeedResistance;
        float IRunBodyMovementValidityConfig.MaximumSupportedSurfaceNormalLiftSpeed => _maximumSupportedSurfaceNormalLiftSpeed;
        float IRunBodyMovementValidityConfig.RunBodySpeedSanityGuardMetersPerSecond => _runBodySpeedSanityGuardMetersPerSecond;
        float IRunLaunchLandingStabilizationConfig.LaunchLandingStabilizationSeconds => _launchLandingStabilizationSeconds;
        float IRunLaunchLandingStabilizationConfig.LaunchLandingMaximumLiftSpeed => _launchLandingMaximumLiftSpeed;
        float IRunSteeringConfig.RunSteeringRangeCentimeters => _runSteeringRangeCentimeters;
        float IRunSteeringConfig.RunSteeringDeadzoneFraction => _runSteeringDeadzoneFraction;
        float IRunSteeringConfig.RunSteeringResponsiveness => _runSteeringResponsiveness;
        float IRunSteeringConfig.FallbackDpi => _fallbackDpi;
        float IRunSteeringConfig.MinimumAcceptedDpi => _minimumAcceptedDpi;
        float IRunSteeringConfig.MaximumAcceptedDpi => _maximumAcceptedDpi;
        float IRunSteeringConfig.MaximumTurnDegreesPerSecond => _maximumTurnDegreesPerSecond;
        float IRunSteeringConfig.RunAirSteeringMaximumTurnDegreesPerSecond => _runAirSteeringMaximumTurnDegreesPerSecond;
        float IRunSteeringConfig.MinimumSteerSpeed => _minimumSteerSpeed;
        float IRunSteeringFrameConfig.RunSteeringFrameNormalSlewDegreesPerSecond => _runSteeringFrameNormalSlewDegreesPerSecond;
        float IRunSteeringFrameConfig.RunSteeringFrameSnapDegrees => _runSteeringFrameSnapDegrees;
        float IRunSteeringFrameConfig.RunSteeringFrameUngroundedGraceSeconds => _runSteeringFrameUngroundedGraceSeconds;

        float IRunSteeringFrameConfig.RunSteeringFrameSuspectNormalConfirmationSeconds =>
            _runSteeringFrameSuspectNormalConfirmationSeconds;

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
