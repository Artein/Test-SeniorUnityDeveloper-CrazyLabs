using Game.Utils.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public interface IPlayerSteeringConfig
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
        float RunBodySpeedSanityGuardMetersPerSecond { get; }
        float LaunchLandingStabilizationSeconds { get; }
        float LaunchLandingMaximumLiftSpeed { get; }
        float RunSteeringFrameNormalSlewDegreesPerSecond { get; }
        float RunSteeringFrameSnapDegrees { get; }
        float RunSteeringFrameUngroundedGraceSeconds { get; }
        float RunSteeringFrameSuspectNormalConfirmationSeconds { get; }
        float ResolveRunSteeringDpi(float rawDpi);
        float ResolveRunSteeringRangePixels(float rawDpi);
    }

    [CreateAssetMenu(
        fileName = nameof(PlayerSteeringConfig),
        menuName = "Game/Gameplay/Player Steering Config")]
    public sealed partial class PlayerSteeringConfig : ScriptableObject, IPlayerSteeringConfig
    {
        [Header("Run Steering Control")]
        [SerializeField, Min(0.0001f),
         Tooltip("Controls: Physical horizontal touch displacement that maps to full Run Steering Control output."
                 + "\n\nImpact: Higher values require a wider thumb motion for full steer; lower values make steering more sensitive."
                 + "\n\nTypical: Centimeters, converted through the resolved screen DPI.")]
        private float _runSteeringRangeCentimeters = 1.5f;

        [SerializeField, Range(0f, 0.95f),
         Tooltip("Controls: Neutral portion near the Run Steering Origin as a fraction of Run Steering Range."
                 + "\n\nImpact: Higher values filter more jitter before steering starts; lower values react to smaller thumb movement."
                 + "\n\nTypical: Normalized fraction constrained below full range.")]
        private float _runSteeringDeadzoneFraction = 0.15f;

        [SerializeField, Min(0f), Tooltip(
             "Controls: How quickly applied steering catches up to requested Run Steering Control output."
             + "\n\nImpact: Lower values feel smoother and heavier; higher values feel snappier and more immediate."
             + "\n\nTypical: Non-negative response rate. Tune against turn rate so steering does not feel either delayed or twitchy.")]
        private float _runSteeringResponsiveness = 8f;

        [Header("Screen Metrics")]
        [SerializeField, Min(0.0001f),
         Tooltip(
             "Fallback screen DPI used when Unity reports an unknown, impossible, or extreme raw DPI for physical Run Steering Control range conversion.")]
        private float _fallbackDpi = 326f;

        [SerializeField, Min(0.0001f),
         Tooltip(
             "Minimum raw screen DPI accepted before using fallback DPI. Protects physical steering range conversion from invalid device reports.")]
        private float _minimumAcceptedDpi = 1f;

        [SerializeField, Min(0.0001f),
         Tooltip(
             "Maximum raw screen DPI accepted before using fallback DPI. Protects physical steering range conversion from extreme device reports.")]
        private float _maximumAcceptedDpi = 1000f;

        [Header("Run Steering Direction")]
        [SerializeField, Min(0f),
         Tooltip("Controls: Maximum grounded turn rate at full Run Steering Control output."
                 + "\n\nImpact: Higher values allow sharper direction changes; lower values make grounded steering broader and calmer."
                 + "\n\nTypical: Degrees per second. This caps steering rotation, not forward speed.")]
        private float _maximumTurnDegreesPerSecond = 120f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Maximum airborne turn rate at full Run Steering Control output."
                 + "\n\nImpact: Higher values give more in-air agency; lower values keep airborne steering gentle compared with grounded steering."
                 + "\n\nTypical: Degrees per second. Usually lower than grounded turn rate.")]
        private float _runAirSteeringMaximumTurnDegreesPerSecond = 45f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Minimum planar Run Body speed required before steering is applied."
                 + "\n\nImpact: Higher values prevent steering while nearly stopped; lower values allow steering at slower movement speeds."
                 + "\n\nTypical: Meters per second. Keep above near-zero physics jitter.")]
        private float _minimumSteerSpeed = 0.25f;

        [SerializeField, Min(0.0001f),
         Tooltip(
             "Controls: Defensive maximum Run Body speed accepted by steering calculations."
             + "\n\nImpact: Contains impossible or non-finite physics values before they contaminate steering."
             + "\n\nTypical: Meters per second. This is a sanity guard, not a player-facing movement speed cap.")]
        private float _runBodySpeedSanityGuardMetersPerSecond = 250f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Duration after first post-launch Run Surface landing where positive surface-normal lift is suppressed."
                 + "\n\nImpact: Higher values stabilize early run entry for longer; lower values restore full surface-relative velocity sooner."
                 + "\n\nTypical: Seconds after landing. Keep short enough that normal run steering returns quickly.")]
        private float _launchLandingStabilizationSeconds = 0.3f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Maximum speed allowed away from the landed Run Surface normal during Launch Landing Stabilization."
                 + "\n\nImpact: Higher values preserve more upward lift after landing; lower values suppress more lift for a flatter run entry."
                 + "\n\nTypical: Meters per second during the stabilization window.")]
        private float _launchLandingMaximumLiftSpeed = 0f;

        [Header("Run Steering Frame Stability")]
        [SerializeField, Min(0f),
         Tooltip(
             "Controls: Maximum angular speed used when the Run Steering Frame follows ordinary grounded Run Surface normal changes."
             + "\n\nImpact: Higher values track surface bends faster; lower values smooth support-normal changes more visibly."
             + "\n\nTypical: Degrees per second. Used for continuous grounded frame changes.")]
        private float _runSteeringFrameNormalSlewDegreesPerSecond = 180f;

        [SerializeField, Range(0f, 180f),
         Tooltip("Controls: Ground-normal angle that marks a continuously grounded Run Steering Frame change as suspect."
                 + "\n\nImpact: Lower values require confirmation for smaller support-normal changes; higher values accept larger changes immediately."
                 + "\n\nTypical: Degrees, constrained from flat to fully opposite normals.")]
        private float _runSteeringFrameSnapDegrees = 60f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Steering-only grace duration that keeps the last stable Run Steering Frame through short raw support misses."
                 + "\n\nImpact: Higher values bridge longer contact gaps; lower values react sooner to ungrounded support."
                 + "\n\nTypical: Seconds. Used for frame stability, not airborne steering power.")]
        private float _runSteeringFrameUngroundedGraceSeconds = 0.08f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Duration a large suspect Run Steering Frame normal must persist before it is accepted as real support."
                 + "\n\nImpact: Higher values reject transient normal spikes for longer; lower values accept sharp support changes sooner."
                 + "\n\nTypical: Seconds. Pairs with the suspect normal angle threshold.")]
        private float _runSteeringFrameSuspectNormalConfirmationSeconds = 0.04f;

        float IPlayerSteeringConfig.RunSteeringRangeCentimeters => _runSteeringRangeCentimeters.GetPositiveOrDefault(1.5f);
        float IPlayerSteeringConfig.RunSteeringDeadzoneFraction => Mathf.Clamp(_runSteeringDeadzoneFraction, 0f, 0.95f);
        float IPlayerSteeringConfig.RunSteeringResponsiveness => _runSteeringResponsiveness.GetNonNegativeOrDefault(8f);
        float IPlayerSteeringConfig.FallbackDpi => _fallbackDpi.GetPositiveOrDefault(326f);
        float IPlayerSteeringConfig.MinimumAcceptedDpi => _minimumAcceptedDpi.GetPositiveOrDefault(1f);

        float IPlayerSteeringConfig.MaximumAcceptedDpi =>
            Mathf.Max(((IPlayerSteeringConfig)this).MinimumAcceptedDpi, _maximumAcceptedDpi.GetPositiveOrDefault(1000f));

        float IPlayerSteeringConfig.MaximumTurnDegreesPerSecond => _maximumTurnDegreesPerSecond;

        float IPlayerSteeringConfig.RunAirSteeringMaximumTurnDegreesPerSecond =>
            _runAirSteeringMaximumTurnDegreesPerSecond.GetNonNegativeOrDefault(45f);

        float IPlayerSteeringConfig.MinimumSteerSpeed => _minimumSteerSpeed;
        float IPlayerSteeringConfig.RunBodySpeedSanityGuardMetersPerSecond => _runBodySpeedSanityGuardMetersPerSecond.GetPositiveOrDefault(250f);
        float IPlayerSteeringConfig.LaunchLandingStabilizationSeconds => _launchLandingStabilizationSeconds.GetNonNegativeOrDefault(0.3f);
        float IPlayerSteeringConfig.LaunchLandingMaximumLiftSpeed => _launchLandingMaximumLiftSpeed.GetNonNegativeOrDefault(0f);

        float IPlayerSteeringConfig.RunSteeringFrameNormalSlewDegreesPerSecond =>
            _runSteeringFrameNormalSlewDegreesPerSecond.GetNonNegativeOrDefault(180f);

        float IPlayerSteeringConfig.RunSteeringFrameSnapDegrees => Mathf.Clamp(_runSteeringFrameSnapDegrees.GetNonNegativeOrDefault(60f), 0f, 180f);
        float IPlayerSteeringConfig.RunSteeringFrameUngroundedGraceSeconds => _runSteeringFrameUngroundedGraceSeconds.GetNonNegativeOrDefault(0.08f);

        float IPlayerSteeringConfig.RunSteeringFrameSuspectNormalConfirmationSeconds =>
            _runSteeringFrameSuspectNormalConfirmationSeconds.GetNonNegativeOrDefault(0.04f);

        float IPlayerSteeringConfig.ResolveRunSteeringDpi(float rawDpi)
        {
            if (!math.isfinite(rawDpi)
                || rawDpi < ((IPlayerSteeringConfig)this).MinimumAcceptedDpi
                || rawDpi > ((IPlayerSteeringConfig)this).MaximumAcceptedDpi)
            {
                return ((IPlayerSteeringConfig)this).FallbackDpi;
            }

            return rawDpi;
        }

        float IPlayerSteeringConfig.ResolveRunSteeringRangePixels(float rawDpi)
        {
            return ((IPlayerSteeringConfig)this).RunSteeringRangeCentimeters / 2.54f * ((IPlayerSteeringConfig)this).ResolveRunSteeringDpi(rawDpi);
        }
    }
}
