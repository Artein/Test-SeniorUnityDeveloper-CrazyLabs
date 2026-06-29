using System;
using System.Collections.Generic;

namespace Game.Gameplay.Upgrades
{
    public sealed class UpgradePreviewBuilder
    {
        private readonly UpgradeDefinitionEvaluator _evaluator;
        private readonly UpgradeDefinitionValidator _validator;

        public UpgradePreviewBuilder(
            UpgradeDefinitionEvaluator evaluator,
            UpgradeDefinitionValidator validator)
        {
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public UpgradePreview Build(UpgradeDefinition definition, int currentLevel, int balance)
        {
            if (balance < 0)
                throw new ArgumentOutOfRangeException(nameof(balance), balance, "Balance must be non-negative.");

            var validationErrors = new List<UpgradeValidationError>(_validator.Validate(definition));

            if (definition == null)
                return CreateInvalidPreview(definition, currentLevel, validationErrors);

            if (currentLevel < 0 || currentLevel > definition.MaxLevel)
            {
                validationErrors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.InvalidMaxLevel,
                    definition,
                    $"Current level {currentLevel} is outside upgrade definition '{definition.StableId}' level range."));
            }

            if (validationErrors.Count > 0)
                return CreateInvalidPreview(definition, currentLevel, validationErrors);

            var currentEffect = _evaluator.GetEffectValue(definition, currentLevel);

            if (currentLevel >= definition.MaxLevel)
            {
                return new UpgradePreview(
                    UpgradePreviewState.Maxed,
                    definition,
                    currentLevel,
                    definition.MaxLevel,
                    currentEffect,
                    nextEffect: null,
                    nextCost: null,
                    isAffordable: false,
                    Array.Empty<UpgradeValidationError>());
            }

            var nextLevel = currentLevel + 1;
            var nextEffect = _evaluator.GetEffectValue(definition, nextLevel);
            var nextCost = _evaluator.GetCostValue(definition, nextLevel);
            var isAffordable = balance >= nextCost;

            return new UpgradePreview(
                isAffordable ? UpgradePreviewState.Available : UpgradePreviewState.Unaffordable,
                definition,
                currentLevel,
                definition.MaxLevel,
                currentEffect,
                nextEffect,
                nextCost,
                isAffordable,
                Array.Empty<UpgradeValidationError>());
        }

        private UpgradePreview CreateInvalidPreview(
            UpgradeDefinition definition,
            int currentLevel,
            IReadOnlyList<UpgradeValidationError> validationErrors)
        {
            return new UpgradePreview(
                UpgradePreviewState.InvalidDefinition,
                definition,
                currentLevel,
                maxLevel: definition == null ? 0 : definition.MaxLevel,
                currentEffect: 0f,
                nextEffect: null,
                nextCost: null,
                isAffordable: false,
                validationErrors);
        }
    }
}
