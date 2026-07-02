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
        float MinimumSteerSpeed { get; }
        float MaximumPlanarSpeed { get; }
        float ResolveRunSteeringDpi(float rawDpi);
        float ResolveRunSteeringRangePixels(float rawDpi);
    }

    [CreateAssetMenu(
        fileName = nameof(PlayerSteeringConfig),
        menuName = "Game/Gameplay/Player Steering Config")]
    public sealed class PlayerSteeringConfig : ScriptableObject, IPlayerSteeringConfig
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

        [Header("Movement Limits")]
        [SerializeField, Min(0f),
         Tooltip("Maximum turn rate at full steer, in degrees per second. Caps how sharply velocity can rotate. Common range: 80-120.")]
        private float _maximumTurnDegreesPerSecond = 120f;

        [SerializeField, Min(0f),
         Tooltip("Minimum planar speed required before steering is applied. Prevents steering while nearly stopped. Common range: 0.20-0.50.")]
        private float _minimumSteerSpeed = 0.25f;

        [SerializeField, Min(0f),
         Tooltip(
             "Maximum planar speed before steering clamps movement speed. This base value can be raised by max-speed upgrades. Common base range: 8-10.")]
        private float _maximumPlanarSpeed = 10f;

        float IPlayerSteeringConfig.RunSteeringRangeCentimeters => _runSteeringRangeCentimeters.GetPositiveOrDefault(1.5f);
        float IPlayerSteeringConfig.RunSteeringDeadzoneFraction => Mathf.Clamp(_runSteeringDeadzoneFraction, 0f, 0.95f);
        float IPlayerSteeringConfig.RunSteeringResponsiveness => _runSteeringResponsiveness.GetNonNegativeOrDefault(8f);
        float IPlayerSteeringConfig.FallbackDpi => _fallbackDpi.GetPositiveOrDefault(326f);
        float IPlayerSteeringConfig.MinimumAcceptedDpi => _minimumAcceptedDpi.GetPositiveOrDefault(1f);

        float IPlayerSteeringConfig.MaximumAcceptedDpi =>
            Mathf.Max(((IPlayerSteeringConfig)this).MinimumAcceptedDpi, _maximumAcceptedDpi.GetPositiveOrDefault(1000f));

        float IPlayerSteeringConfig.MaximumTurnDegreesPerSecond => _maximumTurnDegreesPerSecond;
        float IPlayerSteeringConfig.MinimumSteerSpeed => _minimumSteerSpeed;
        float IPlayerSteeringConfig.MaximumPlanarSpeed => _maximumPlanarSpeed;

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
