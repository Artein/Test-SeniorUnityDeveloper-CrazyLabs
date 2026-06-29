using System;
using System.Collections.Generic;
using Game.Gameplay.Economy;

namespace Game.Gameplay.Upgrades
{
    public sealed class UpgradePurchaseService
    {
        private readonly IUpgradeCatalog _catalog;
        private readonly ICurrencyStorage _currencyStorage;
        private readonly IUpgradeProgressStorage _progressStorage;
        private readonly UpgradeDefinitionEvaluator _evaluator;
        private readonly UpgradeDefinitionValidator _definitionValidator;

        public UpgradePurchaseService(
            IUpgradeCatalog catalog,
            ICurrencyStorage currencyStorage,
            IUpgradeProgressStorage progressStorage,
            UpgradeDefinitionEvaluator evaluator,
            UpgradeDefinitionValidator definitionValidator)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _currencyStorage = currencyStorage ?? throw new ArgumentNullException(nameof(currencyStorage));
            _progressStorage = progressStorage ?? throw new ArgumentNullException(nameof(progressStorage));
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _definitionValidator = definitionValidator ?? throw new ArgumentNullException(nameof(definitionValidator));
        }

        public UpgradePurchaseResult TryPurchase(UpgradeDefinition definition)
        {
            if (!ContainsDefinition(definition))
            {
                return new UpgradePurchaseResult(
                    UpgradePurchaseStatus.MissingDefinition,
                    definition,
                    previousLevel: 0,
                    newLevel: 0,
                    cost: null,
                    Array.Empty<UpgradeValidationError>());
            }

            var currentLevel = _progressStorage.GetLevel(definition);
            var validationErrors = new List<UpgradeValidationError>(_definitionValidator.Validate(definition));

            if (currentLevel < 0 || currentLevel > definition.MaxLevel)
            {
                validationErrors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.InvalidMaxLevel,
                    definition,
                    $"Stored level {currentLevel} is outside upgrade definition '{definition.StableId}' level range."));
            }

            if (_catalog.PurchaseCurrency == null)
            {
                validationErrors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.MissingPurchaseCurrency,
                    _catalog,
                    "Upgrade catalog requires a purchase currency."));
            }

            if (validationErrors.Count > 0)
            {
                return new UpgradePurchaseResult(
                    UpgradePurchaseStatus.InvalidDefinition,
                    definition,
                    currentLevel,
                    currentLevel,
                    cost: null,
                    validationErrors);
            }

            if (currentLevel >= definition.MaxLevel)
            {
                return new UpgradePurchaseResult(
                    UpgradePurchaseStatus.MaxLevelReached,
                    definition,
                    currentLevel,
                    currentLevel,
                    cost: null,
                    Array.Empty<UpgradeValidationError>());
            }

            var nextLevel = currentLevel + 1;
            var cost = _evaluator.GetCostValue(definition, nextLevel);

            if (!_currencyStorage.TrySpend(_catalog.PurchaseCurrency, cost))
            {
                return new UpgradePurchaseResult(
                    UpgradePurchaseStatus.InsufficientCurrency,
                    definition,
                    currentLevel,
                    currentLevel,
                    cost,
                    Array.Empty<UpgradeValidationError>());
            }

            _progressStorage.SetLevel(definition, nextLevel);

            return new UpgradePurchaseResult(
                UpgradePurchaseStatus.Purchased,
                definition,
                currentLevel,
                nextLevel,
                cost,
                Array.Empty<UpgradeValidationError>());
        }

        private bool ContainsDefinition(UpgradeDefinition definition)
        {
            if (definition == null || _catalog.UpgradeDefinitions == null)
                return false;

            foreach (var catalogDefinition in _catalog.UpgradeDefinitions)
            {
                if (catalogDefinition == null)
                    continue;

                if (ReferenceEquals(catalogDefinition, definition))
                    return true;

                if (!string.IsNullOrWhiteSpace(catalogDefinition.StableId)
                    && string.Equals(catalogDefinition.StableId, definition.StableId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
