using System;
using System.Collections.Generic;

namespace Game.Gameplay.Upgrades
{
    public sealed class GameplayStatResolver
    {
        private readonly IReadOnlyList<IGameplayStatModifierSource> _modifierSources;

        public GameplayStatResolver(IReadOnlyList<IGameplayStatModifierSource> modifierSources)
        {
            if (modifierSources is null)
                throw new ArgumentNullException(nameof(modifierSources));

            var modifierSourceCopy = new IGameplayStatModifierSource[modifierSources.Count];

            for (var index = 0; index < modifierSources.Count; index++)
            {
                modifierSourceCopy[index] = modifierSources[index]
                                            ?? throw new ArgumentException("Gameplay stat modifier sources cannot contain null entries.",
                                                nameof(modifierSources));
            }

            _modifierSources = modifierSourceCopy;
        }

        public float Resolve(GameplayStatId statId, float baseValue)
        {
            if (statId == null)
                throw new ArgumentNullException(nameof(statId));

            if (float.IsNaN(baseValue) || float.IsInfinity(baseValue))
                throw new ArgumentException("Base gameplay stat value must be finite.", nameof(baseValue));

            var value = ApplyFlatAdd(statId, baseValue);
            value = ApplyAdditivePercent(statId, value);
            value = ApplyMultiplicativeFactor(statId, value);
            value = ApplyClampMin(statId, value);
            return ApplyClampMax(statId, value);
        }

        private float ApplyFlatAdd(GameplayStatId statId, float value)
        {
            foreach (var modifier in EnumerateMatchingModifiers(statId, GameplayStatModifierOperation.FlatAdd))
            {
                value += modifier.Value;
            }

            return value;
        }

        private float ApplyAdditivePercent(GameplayStatId statId, float value)
        {
            var additivePercent = 0f;

            foreach (var modifier in EnumerateMatchingModifiers(statId, GameplayStatModifierOperation.AdditivePercent))
            {
                additivePercent += modifier.Value;
            }

            return value * (1f + additivePercent);
        }

        private float ApplyMultiplicativeFactor(GameplayStatId statId, float value)
        {
            foreach (var modifier in EnumerateMatchingModifiers(statId, GameplayStatModifierOperation.MultiplicativeFactor))
            {
                value *= modifier.Value;
            }

            return value;
        }

        private float ApplyClampMin(GameplayStatId statId, float value)
        {
            foreach (var modifier in EnumerateMatchingModifiers(statId, GameplayStatModifierOperation.ClampMin))
            {
                value = Math.Max(value, modifier.Value);
            }

            return value;
        }

        private float ApplyClampMax(GameplayStatId statId, float value)
        {
            foreach (var modifier in EnumerateMatchingModifiers(statId, GameplayStatModifierOperation.ClampMax))
            {
                value = Math.Min(value, modifier.Value);
            }

            return value;
        }

        private IEnumerable<GameplayStatModifier> EnumerateMatchingModifiers(
            GameplayStatId statId,
            GameplayStatModifierOperation operation)
        {
            foreach (var modifierSource in _modifierSources)
            {
                var modifiers = modifierSource.Modifiers;

                if (modifiers == null)
                    continue;

                foreach (var modifier in modifiers)
                {
                    if (modifier.Operation == operation && MatchesStatId(modifier.StatId, statId))
                        yield return modifier;
                }
            }
        }

        private bool MatchesStatId(GameplayStatId candidate, GameplayStatId requested)
        {
            if (candidate == requested)
                return true;

            if (candidate == null || requested == null)
                return false;

            return !string.IsNullOrWhiteSpace(candidate.Id)
                   && string.Equals(candidate.Id, requested.Id, StringComparison.Ordinal);
        }
    }
}
