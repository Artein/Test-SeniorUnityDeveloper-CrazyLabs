using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Upgrades
{
    public sealed class UpgradeDefinitionValidator
    {
        private readonly UpgradeDefinitionEvaluator _evaluator;

        public UpgradeDefinitionValidator(UpgradeDefinitionEvaluator evaluator)
        {
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public IReadOnlyList<UpgradeValidationError> Validate(IUpgradeDefinition definition)
        {
            var errors = new List<UpgradeValidationError>();

            if (definition is null)
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.NullUpgradeDefinition,
                    null,
                    "Upgrade definition is missing."));
                return errors;
            }

            ValidateDefinitionFields(definition, errors);
            ValidateProgression(definition, definition.CostProgression, 1, definition.MaxLevel, true, errors);
            ValidateProgression(definition, definition.EffectProgression, 0, Math.Max(0, definition.MaxLevel), false, errors);

            return errors;
        }

        private void ValidateDefinitionFields(
            IUpgradeDefinition definition,
            List<UpgradeValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(definition.StableId))
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.MissingUpgradeId,
                    definition,
                    "Upgrade definition requires a stable id."));
            }

            if (string.IsNullOrWhiteSpace(definition.DisplayName))
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.MissingDisplayName,
                    definition,
                    "Upgrade definition requires a display name."));
            }

            if (definition.Icon == null)
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.MissingIcon,
                    definition,
                    $"Upgrade definition '{definition.StableId}' requires an icon."));
            }

            if (definition.TargetStatId == null)
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.MissingTargetStatId,
                    definition,
                    $"Upgrade definition '{definition.StableId}' requires a target stat id."));
            }

            if (definition.MaxLevel <= 0)
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.InvalidMaxLevel,
                    definition,
                    $"Upgrade definition '{definition.StableId}' requires a positive max level."));
            }

            if (definition.DisplayDecimalPlaces < 0)
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.InvalidEffectValue,
                    definition,
                    $"Upgrade definition '{definition.StableId}' display decimal places must be non-negative."));
            }
        }

        private void ValidateProgression(
            IUpgradeDefinition definition,
            UpgradeProgression progression,
            int minimumLevel,
            int maximumLevel,
            bool isCostProgression,
            List<UpgradeValidationError> errors)
        {
            if (progression == null)
            {
                errors.Add(new UpgradeValidationError(
                    isCostProgression
                        ? UpgradeValidationErrorCode.MissingCostProgression
                        : UpgradeValidationErrorCode.MissingEffectProgression,
                    definition,
                    $"Upgrade definition '{definition.StableId}' is missing a {(isCostProgression ? "cost" : "effect")} progression."));
                return;
            }

            ValidateProgressionShape(definition, progression, isCostProgression, errors);
            ValidateExactOverrides(definition, progression, minimumLevel, maximumLevel, errors);

            if (definition.MaxLevel <= 0 || progression.Curve is not { length: > 0 })
                return;

            ValidateEvaluatedValues(definition, minimumLevel, maximumLevel, isCostProgression, errors);
        }

        private void ValidateProgressionShape(
            IUpgradeDefinition definition,
            UpgradeProgression progression,
            bool isCostProgression,
            List<UpgradeValidationError> errors)
        {
            var progressionName = isCostProgression ? "cost" : "effect";

            if (!math.isfinite(progression.MinimumValue) || !math.isfinite(progression.MaximumValue))
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.InvalidProgressionRange,
                    definition,
                    $"Upgrade definition '{definition.StableId}' {progressionName} progression range must be finite."));
            }

            if (progression.MaximumValue < progression.MinimumValue)
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.InvalidProgressionRange,
                    definition,
                    $"Upgrade definition '{definition.StableId}' {progressionName} progression max must be greater than or equal to min."));
            }

            if (!math.isfinite(progression.StepSize) || progression.StepSize < 0f)
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.InvalidProgressionStep,
                    definition,
                    $"Upgrade definition '{definition.StableId}' {progressionName} progression step must be finite and non-negative."));
            }

            ValidateCurve(definition, progression.Curve, progressionName, errors);
        }

        private void ValidateCurve(
            IUpgradeDefinition definition,
            AnimationCurve curve,
            string progressionName,
            List<UpgradeValidationError> errors)
        {
            if (curve is not { length: > 0 })
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.MissingProgressionCurve,
                    definition,
                    $"Upgrade definition '{definition.StableId}' {progressionName} progression curve requires at least one key."));
                return;
            }

            foreach (var key in curve.keys)
            {
                if (math.isfinite(key.time) && math.isfinite(key.value) && key.time is >= 0f and <= 1f)
                    continue;

                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.InvalidProgressionCurve,
                    definition,
                    $"Upgrade definition '{definition.StableId}' {progressionName} progression curve keys must be finite and normalized."));
                return;
            }
        }

        private void ValidateExactOverrides(
            IUpgradeDefinition definition,
            UpgradeProgression progression,
            int minimumLevel,
            int maximumLevel,
            List<UpgradeValidationError> errors)
        {
            var seenLevels = new HashSet<int>();

            foreach (var exactOverride in progression.ExactOverrides)
            {
                if (!seenLevels.Add(exactOverride.Level))
                {
                    errors.Add(new UpgradeValidationError(
                        UpgradeValidationErrorCode.DuplicateExactOverrideLevel,
                        definition,
                        $"Upgrade definition '{definition.StableId}' has duplicate exact override level {exactOverride.Level}."));
                }

                if (exactOverride.Level < minimumLevel || exactOverride.Level > maximumLevel)
                {
                    errors.Add(new UpgradeValidationError(
                        UpgradeValidationErrorCode.ExactOverrideLevelOutOfRange,
                        definition,
                        $"Upgrade definition '{definition.StableId}' exact override level {exactOverride.Level} is outside the valid range."));
                }

                if (!math.isfinite(exactOverride.Value))
                {
                    errors.Add(new UpgradeValidationError(
                        UpgradeValidationErrorCode.InvalidExactOverrideValue,
                        definition,
                        $"Upgrade definition '{definition.StableId}' exact override value for level {exactOverride.Level} must be finite."));
                }
            }
        }

        private void ValidateEvaluatedValues(
            IUpgradeDefinition definition,
            int minimumLevel,
            int maximumLevel,
            bool isCostProgression,
            List<UpgradeValidationError> errors)
        {
            for (var level = minimumLevel; level <= maximumLevel; level++)
            {
                try
                {
                    if (isCostProgression)
                    {
                        _evaluator.GetCostValue(definition, level);
                    }
                    else
                    {
                        _evaluator.GetEffectValue(definition, level);
                    }
                }
                catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
                {
                    errors.Add(new UpgradeValidationError(
                        isCostProgression
                            ? UpgradeValidationErrorCode.InvalidCostValue
                            : UpgradeValidationErrorCode.InvalidEffectValue,
                        definition,
                        $"Upgrade definition '{definition.StableId}' has an invalid {(isCostProgression ? "cost" : "effect")} value at level {level}: {exception.Message}"));
                    return;
                }
            }
        }
    }
}
