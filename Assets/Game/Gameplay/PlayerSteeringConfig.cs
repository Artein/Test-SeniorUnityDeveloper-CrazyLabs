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
         Tooltip("Physical horizontal touch displacement, in centimeters, that maps to full Run Steering Control output. Initial baseline: 1.5cm.")]
        private float _runSteeringRangeCentimeters = 1.5f;

        [SerializeField, Range(0f, 0.95f),
         Tooltip("Neutral portion near the Run Steering Origin as a fraction of Run Steering Range. Values beyond this are remapped to full range.")]
        private float _runSteeringDeadzoneFraction = 0.15f;

        [SerializeField, Min(0f), Tooltip(
             "How quickly applied steering catches up to requested Run Steering Control output. Lower feels smoother and heavier; higher feels snappier.")]
        private float _runSteeringResponsiveness = 8f;

        [Header("Screen Metrics")]
        [SerializeField, Min(0.0001f), Tooltip("DPI used when Unity reports an unknown, impossible, or extreme raw screen DPI.")]
        private float _fallbackDpi = 326f;

        [SerializeField, Min(0.0001f), Tooltip("Minimum raw screen DPI accepted before falling back.")]
        private float _minimumAcceptedDpi = 1f;

        [SerializeField, Min(0.0001f), Tooltip("Maximum raw screen DPI accepted before falling back.")]
        private float _maximumAcceptedDpi = 1000f;

        [Header("Run Steering Direction")]
        [SerializeField, Min(0f),
         Tooltip("Maximum turn rate at full steer, in degrees per second. Caps how sharply velocity can rotate. Common range: 80-120.")]
        private float _maximumTurnDegreesPerSecond = 120f;

        [SerializeField, Min(0f),
         Tooltip("Maximum in-air turn rate at full steer, in degrees per second. Lower than grounded steering to keep airborne agency gentle.")]
        private float _runAirSteeringMaximumTurnDegreesPerSecond = 45f;

        [SerializeField, Min(0f),
         Tooltip("Minimum planar speed required before steering is applied. Prevents steering while nearly stopped. Common range: 0.20-0.50.")]
        private float _minimumSteerSpeed = 0.25f;

        [SerializeField, Min(0.0001f),
         Tooltip(
             "Defensive maximum Run Body velocity used only to contain impossible or non-finite physics values. This is not a gameplay speed cap.")]
        private float _runBodySpeedSanityGuardMetersPerSecond = 250f;

        [SerializeField, Min(0f),
         Tooltip("Seconds after first post-launch Run Surface landing where positive surface-normal lift is suppressed.")]
        private float _launchLandingStabilizationSeconds = 0.3f;

        [SerializeField, Min(0f),
         Tooltip("Maximum allowed speed away from the landed Run Surface normal during post-launch landing stabilization.")]
        private float _launchLandingMaximumLiftSpeed = 0f;

        [Header("Run Steering Frame Stability")]
        [SerializeField, Min(0f),
         Tooltip(
             "Maximum angular speed, in degrees per second, used when the Run Steering Frame follows ordinary grounded Run Surface normal changes.")]
        private float _runSteeringFrameNormalSlewDegreesPerSecond = 180f;

        [SerializeField, Range(0f, 180f),
         Tooltip("Ground-normal angle above which a continuously grounded Run Steering Frame change is treated as suspect before it is accepted.")]
        private float _runSteeringFrameSnapDegrees = 60f;

        [SerializeField, Min(0f),
         Tooltip("Steering-only duration, in seconds, to keep the last stable Run Steering Frame through short raw support misses.")]
        private float _runSteeringFrameUngroundedGraceSeconds = 0.08f;

        [SerializeField, Min(0f),
         Tooltip("Duration, in seconds, a large suspect Run Steering Frame normal must persist before it is accepted as real support.")]
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
