using System;

namespace Game.Gameplay
{
    public readonly struct RunSteeringInputMetrics
    {
        public float ResolvedDpi { get; }
        public float RangePixels { get; }
        public float DeadzoneFraction { get; }

        public RunSteeringInputMetrics(float resolvedDpi, float rangePixels, float deadzoneFraction)
        {
            ResolvedDpi = resolvedDpi;
            RangePixels = rangePixels;
            DeadzoneFraction = deadzoneFraction;
        }
    }

    internal interface IRunSteeringInputMetricsResolver
    {
        RunSteeringInputMetrics Resolve(float rawDpi);
    }

    internal sealed class DefaultRunSteeringInputMetricsResolver : IRunSteeringInputMetricsResolver
    {
        private readonly IRunSteeringConfig _config;

        public DefaultRunSteeringInputMetricsResolver(IRunSteeringConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public RunSteeringInputMetrics Resolve(float rawDpi)
        {
            var resolvedDpi = IsAcceptedRawDpi(rawDpi) ? rawDpi : _config.FallbackDpi;
            var rangePixels = _config.RunSteeringRangeCentimeters / 2.54f * resolvedDpi;

            return new RunSteeringInputMetrics(
                resolvedDpi,
                rangePixels,
                _config.RunSteeringDeadzoneFraction);
        }

        private bool IsAcceptedRawDpi(float rawDpi)
        {
            return float.IsFinite(rawDpi)
                   && rawDpi >= _config.MinimumAcceptedDpi
                   && rawDpi <= _config.MaximumAcceptedDpi;
        }
    }
}
