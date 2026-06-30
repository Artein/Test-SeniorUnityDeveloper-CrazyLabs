using System;
using System.Collections.Generic;

namespace Game.Gameplay.Upgrades
{
    public sealed class UpgradeCatalogValidator
    {
        private readonly UpgradeDefinitionValidator _definitionValidator;

        public UpgradeCatalogValidator(UpgradeDefinitionValidator definitionValidator)
        {
            _definitionValidator = definitionValidator ?? throw new ArgumentNullException(nameof(definitionValidator));
        }

        public IReadOnlyList<UpgradeValidationError> Validate(IUpgradeCatalog catalog)
        {
            var errors = new List<UpgradeValidationError>();

            if (catalog is null)
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.NullUpgradeDefinitions,
                    null,
                    "Upgrade catalog is missing."));
                return errors;
            }

            if (catalog.PurchaseCurrency == null)
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.MissingPurchaseCurrency,
                    catalog,
                    "Upgrade catalog requires a purchase currency."));
            }
            else if (string.IsNullOrWhiteSpace(catalog.PurchaseCurrency.SaveId))
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.MissingPurchaseCurrency,
                    catalog,
                    "Upgrade catalog purchase currency requires a stable save id."));
            }

            ValidateDefinitions(catalog, errors);
            return errors;
        }

        private void ValidateDefinitions(
            IUpgradeCatalog catalog,
            List<UpgradeValidationError> errors)
        {
            if (catalog.UpgradeDefinitions == null)
            {
                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.NullUpgradeDefinitions,
                    catalog,
                    "Upgrade catalog has a missing upgrade definition list."));
                return;
            }

            var acceptedStableIds = new HashSet<string>();

            foreach (var definition in catalog.UpgradeDefinitions)
            {
                if (definition == null)
                {
                    errors.Add(new UpgradeValidationError(
                        UpgradeValidationErrorCode.NullUpgradeDefinition,
                        catalog,
                        "Upgrade catalog contains a null upgrade definition."));
                    continue;
                }

                foreach (var definitionError in _definitionValidator.Validate(definition))
                {
                    errors.Add(definitionError);
                }

                if (string.IsNullOrWhiteSpace(definition.StableId))
                    continue;

                if (acceptedStableIds.Add(definition.StableId))
                    continue;

                errors.Add(new UpgradeValidationError(
                    UpgradeValidationErrorCode.DuplicateUpgradeId,
                    definition,
                    $"Upgrade catalog contains duplicate upgrade id '{definition.StableId}'."));
            }
        }
    }
}
