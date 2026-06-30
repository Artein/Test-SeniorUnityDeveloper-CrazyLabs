using System;
using Game.Gameplay.Economy;
using Game.Gameplay.Upgrades;
using JetBrains.Annotations;

namespace Game.Gameplay
{
    [UsedImplicitly]
    public sealed class GameplayEconomyContentIndex : IPlayerEconomyContentIndex
    {
        private readonly IUpgradeCatalog _upgradeCatalog;
        private readonly CurrencyDefinition _coinCurrencyDefinition;

        public GameplayEconomyContentIndex(IUpgradeCatalog upgradeCatalog, CurrencyDefinition coinCurrencyDefinition)
        {
            _upgradeCatalog = upgradeCatalog ?? throw new ArgumentNullException(nameof(upgradeCatalog));
            _coinCurrencyDefinition = coinCurrencyDefinition != null
                ? coinCurrencyDefinition
                : throw new ArgumentNullException(nameof(coinCurrencyDefinition));
        }

        public bool IsKnownCurrencyId(string currencySaveId)
        {
            if (string.IsNullOrWhiteSpace(currencySaveId))
                return false;

            if (string.Equals(_coinCurrencyDefinition.SaveId, currencySaveId, StringComparison.Ordinal))
                return true;

            return _upgradeCatalog.PurchaseCurrency != null
                   && string.Equals(_upgradeCatalog.PurchaseCurrency.SaveId, currencySaveId, StringComparison.Ordinal);
        }

        public bool TryGetUpgradeMaxLevel(string upgradeStableId, out int maxLevel)
        {
            maxLevel = 0;

            if (string.IsNullOrWhiteSpace(upgradeStableId))
                return false;

            foreach (var upgradeDefinition in _upgradeCatalog.UpgradeDefinitions)
            {
                if (upgradeDefinition == null
                    || !string.Equals(upgradeDefinition.StableId, upgradeStableId, StringComparison.Ordinal))
                {
                    continue;
                }

                maxLevel = upgradeDefinition.MaxLevel;
                return true;
            }

            return false;
        }
    }
}
