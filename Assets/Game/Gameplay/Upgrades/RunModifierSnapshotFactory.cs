using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Gameplay.Upgrades
{
    public interface IRunModifierSnapshotFactory
    {
        RunModifierSnapshot CreateSnapshot();
    }

    public sealed class RunModifierSnapshotFactory : IRunModifierSnapshotFactory
    {
        private readonly IUpgradeCatalog _catalog;
        private readonly IUpgradeProgressStorage _progressStorage;
        private readonly UpgradeDefinitionEvaluator _evaluator;
        private readonly UpgradeCatalogValidator _catalogValidator;

        public RunModifierSnapshotFactory(
            IUpgradeCatalog catalog,
            IUpgradeProgressStorage progressStorage,
            UpgradeDefinitionEvaluator evaluator,
            UpgradeCatalogValidator catalogValidator)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _progressStorage = progressStorage ?? throw new ArgumentNullException(nameof(progressStorage));
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _catalogValidator = catalogValidator ?? throw new ArgumentNullException(nameof(catalogValidator));
        }

        public RunModifierSnapshot CreateSnapshot()
        {
            var validationErrors = _catalogValidator.Validate(_catalog);

            if (validationErrors.Count > 0)
                throw new InvalidOperationException(CreateValidationMessage(validationErrors));

            var modifiers = new List<GameplayStatModifier>();

            foreach (var definition in _catalog.UpgradeDefinitions)
            {
                var level = _progressStorage.GetLevel(definition);

                if (level <= 0)
                    continue;

                modifiers.Add(new GameplayStatModifier(
                    definition.TargetStatId,
                    ToModifierOperation(definition.OperationType),
                    _evaluator.GetEffectValue(definition, level)));
            }

            return new RunModifierSnapshot(modifiers);
        }

        private GameplayStatModifierOperation ToModifierOperation(UpgradeOperationType operationType)
        {
            return operationType switch
            {
                UpgradeOperationType.FlatAdd => GameplayStatModifierOperation.FlatAdd,
                UpgradeOperationType.AdditivePercent => GameplayStatModifierOperation.AdditivePercent,
                UpgradeOperationType.MultiplicativeFactor => GameplayStatModifierOperation.MultiplicativeFactor,
                UpgradeOperationType.ClampMin => GameplayStatModifierOperation.ClampMin,
                UpgradeOperationType.ClampMax => GameplayStatModifierOperation.ClampMax,
                _ => throw new InvalidOperationException($"Unsupported upgrade operation type '{operationType}'.")
            };
        }

        private string CreateValidationMessage(IReadOnlyList<UpgradeValidationError> validationErrors)
        {
            var messages = validationErrors.Select(error => error.Message);
            return $"Upgrade catalog is invalid for run modifier snapshot creation: {string.Join("; ", messages)}";
        }
    }
}
