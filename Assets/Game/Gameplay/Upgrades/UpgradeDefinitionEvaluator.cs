using System;
using Unity.Mathematics;

namespace Game.Gameplay.Upgrades
{
    public sealed class UpgradeDefinitionEvaluator
    {
        public int GetCostValue(IUpgradeDefinition definition, int level)
        {
            if (definition is null)
                throw new ArgumentNullException(nameof(definition));

            if (level < 1 || level > definition.MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Cost level must be between 1 and max level.");

            if (definition.CostProgression == null)
                throw new InvalidOperationException("Upgrade cost progression is missing.");

            var value = definition.CostProgression.Evaluate(level, 1, definition.MaxLevel);

            if (!math.isfinite(value))
                throw new InvalidOperationException("Upgrade cost progression returned a non-finite value.");

            if (value <= 0f)
                throw new InvalidOperationException("Upgrade cost progression must return a positive value.");

            return checked((int)value);
        }

        public float GetEffectValue(IUpgradeDefinition definition, int level)
        {
            if (definition is null)
                throw new ArgumentNullException(nameof(definition));

            if (level < 0 || level > definition.MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Effect level must be between 0 and max level.");

            if (definition.EffectProgression == null)
                throw new InvalidOperationException("Upgrade effect progression is missing.");

            var value = definition.EffectProgression.Evaluate(level, 0, definition.MaxLevel);

            if (!math.isfinite(value))
                throw new InvalidOperationException("Upgrade effect progression returned a non-finite value.");

            return value;
        }
    }
}
