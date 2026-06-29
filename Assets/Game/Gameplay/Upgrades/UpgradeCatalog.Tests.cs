#if UNITY_INCLUDE_TESTS

using System.Collections.Generic;
using Game.Gameplay.Economy;

namespace Game.Gameplay.Upgrades
{
    public sealed partial class UpgradeCatalog
    {
        internal void SetValuesForTests(
            CurrencyDefinition purchaseCurrency,
            IReadOnlyList<UpgradeDefinition> upgradeDefinitions)
        {
            _purchaseCurrency = purchaseCurrency;
            _upgradeDefinitions.Clear();

            if (upgradeDefinitions is null)
                return;

            foreach (var upgradeDefinition in upgradeDefinitions)
            {
                _upgradeDefinitions.Add(upgradeDefinition);
            }
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
