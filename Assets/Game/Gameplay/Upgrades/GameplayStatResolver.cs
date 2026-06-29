using System;
using System.Collections.Generic;

namespace Game.Gameplay.Upgrades
{
    public sealed class GameplayStatResolver
    {
        private readonly IGameplayStatModifierSource[] _modifierSources;

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
            for (var sourceIndex = 0; sourceIndex < _modifierSources.Length; sourceIndex++)
            {
                var modifiers = _modifierSources[sourceIndex].Modifiers;

                if (modifiers == null)
                    continue;

                for (var modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
                {
                    var modifier = modifiers[modifierIndex];

                    if (IsMatchingModifier(modifier, statId, GameplayStatModifierOperation.FlatAdd))
                        value += modifier.Value;
                }
            }

            return value;
        }

        private float ApplyAdditivePercent(GameplayStatId statId, float value)
        {
            var additivePercent = 0f;

            for (var sourceIndex = 0; sourceIndex < _modifierSources.Length; sourceIndex++)
            {
                var modifiers = _modifierSources[sourceIndex].Modifiers;

                if (modifiers == null)
                    continue;

                for (var modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
                {
                    var modifier = modifiers[modifierIndex];

                    if (IsMatchingModifier(modifier, statId, GameplayStatModifierOperation.AdditivePercent))
                        additivePercent += modifier.Value;
                }
            }

            return value * (1f + additivePercent);
        }

        private float ApplyMultiplicativeFactor(GameplayStatId statId, float value)
        {
            for (var sourceIndex = 0; sourceIndex < _modifierSources.Length; sourceIndex++)
            {
                var modifiers = _modifierSources[sourceIndex].Modifiers;

                if (modifiers == null)
                    continue;

                for (var modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
                {
                    var modifier = modifiers[modifierIndex];

                    if (IsMatchingModifier(modifier, statId, GameplayStatModifierOperation.MultiplicativeFactor))
                        value *= modifier.Value;
                }
            }

            return value;
        }

        private float ApplyClampMin(GameplayStatId statId, float value)
        {
            for (var sourceIndex = 0; sourceIndex < _modifierSources.Length; sourceIndex++)
            {
                var modifiers = _modifierSources[sourceIndex].Modifiers;

                if (modifiers == null)
                    continue;

                for (var modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
                {
                    var modifier = modifiers[modifierIndex];

                    if (IsMatchingModifier(modifier, statId, GameplayStatModifierOperation.ClampMin))
                        value = Math.Max(value, modifier.Value);
                }
            }

            return value;
        }

        private float ApplyClampMax(GameplayStatId statId, float value)
        {
            for (var sourceIndex = 0; sourceIndex < _modifierSources.Length; sourceIndex++)
            {
                var modifiers = _modifierSources[sourceIndex].Modifiers;

                if (modifiers == null)
                    continue;

                for (var modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
                {
                    var modifier = modifiers[modifierIndex];

                    if (IsMatchingModifier(modifier, statId, GameplayStatModifierOperation.ClampMax))
                        value = Math.Min(value, modifier.Value);
                }
            }

            return value;
        }

        private bool IsMatchingModifier(
            GameplayStatModifier modifier,
            GameplayStatId statId,
            GameplayStatModifierOperation operation)
        {
            return modifier.Operation == operation && MatchesStatId(modifier.StatId, statId);
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
