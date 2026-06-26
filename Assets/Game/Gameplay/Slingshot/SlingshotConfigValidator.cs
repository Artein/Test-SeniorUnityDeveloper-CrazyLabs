using System.Collections.Generic;
using Game.Utils.Mathematics;
using Unity.Mathematics;

namespace Game.Gameplay.Slingshot
{
    internal sealed class SlingshotConfigValidator
    {
        public IEnumerable<string> Validate(ISlingshotConfig config)
        {
            if (config is null)
            {
                yield return "Slingshot config is missing.";
                yield break;
            }

            if (!config.TouchTargetRadiusPixels.IsFinitePositive())
                yield return $"Slingshot {nameof(config.TouchTargetRadiusPixels)} must be a finite positive value.";

            if (!config.MinimumPullDistance.IsFinitePositive())
                yield return $"Slingshot {nameof(config.MinimumPullDistance)} must be a finite positive value.";

            if (!config.MaximumPullDistance.IsFinitePositive())
                yield return $"Slingshot {nameof(config.MaximumPullDistance)} must be a finite positive value.";

            if (!config.MaximumLateralPull.IsFinitePositive())
                yield return $"Slingshot {nameof(config.MaximumLateralPull)} must be a finite positive value.";

            if (!config.MaximumLaunchAngleDegrees.IsFinitePositive())
                yield return $"Slingshot {nameof(config.MaximumLaunchAngleDegrees)} must be a finite positive value.";

            if (!config.MinimumLaunchSpeed.IsFinitePositive())
                yield return $"Slingshot {nameof(config.MinimumLaunchSpeed)} must be a finite positive value.";

            if (!config.MaximumLaunchSpeed.IsFinitePositive())
                yield return $"Slingshot {nameof(config.MaximumLaunchSpeed)} must be a finite positive value.";

            if (!config.LaunchUpSpeed.IsFinitePositive())
                yield return $"Slingshot {nameof(config.LaunchUpSpeed)} must be a finite positive value.";

            if (!math.isfinite(config.BandContactPadding) || config.BandContactPadding < 0f)
                yield return $"Slingshot {nameof(config.BandContactPadding)} must be a finite non-negative value.";

            if (config.BandWrapSampleCount is < 2 or > 24)
                yield return $"Slingshot {nameof(config.BandWrapSampleCount)} must be between 2 and 24.";

            if (!config.BandRecoilDuration.IsFinitePositive())
                yield return $"Slingshot {nameof(config.BandRecoilDuration)} must be a finite positive value.";

            if (config.MaximumPullDistance < config.MinimumPullDistance)
                yield return "Slingshot maximum pull distance must be greater than or equal to minimum pull distance.";

            if (config.MaximumLaunchSpeed < config.MinimumLaunchSpeed)
                yield return "Slingshot maximum launch speed must be greater than or equal to minimum launch speed.";

            if (config.LaunchSpeedCurve is not { length: > 0 })
                yield return "Slingshot launch speed curve must contain at least one key.";

            if (config.BandRecoilCurve is not { length: > 0 })
                yield return "Slingshot Band recoil curve must contain at least one key.";
        }
    }
}
