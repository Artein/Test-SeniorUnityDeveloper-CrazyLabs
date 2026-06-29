using System.Collections.Generic;
using Game.Gameplay.Economy;
using UnityEngine;

namespace Game.Gameplay.Upgrades
{
    public interface IUpgradeCatalog
    {
        CurrencyDefinition PurchaseCurrency { get; }
        IReadOnlyList<UpgradeDefinition> UpgradeDefinitions { get; }
    }

    [CreateAssetMenu(
        fileName = nameof(UpgradeCatalog),
        menuName = "Game/Gameplay/Upgrades/Upgrade Catalog")]
    public sealed partial class UpgradeCatalog : ScriptableObject, IUpgradeCatalog
    {
        [SerializeField] private CurrencyDefinition _purchaseCurrency;
        [SerializeField] private List<UpgradeDefinition> _upgradeDefinitions = new();

        public CurrencyDefinition PurchaseCurrency => _purchaseCurrency;
        public IReadOnlyList<UpgradeDefinition> UpgradeDefinitions => _upgradeDefinitions;

        private void OnValidate()
        {
            var validator = new UpgradeCatalogValidator(new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator()));
            var errors = validator.Validate(this);

            foreach (var error in errors)
            {
                Debug.LogWarning(error.Message);
            }
        }
    }
}
