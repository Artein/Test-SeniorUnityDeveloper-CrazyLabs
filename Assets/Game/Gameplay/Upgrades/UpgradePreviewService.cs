using System;
using System.Collections.Generic;
using Game.Gameplay.Economy;

namespace Game.Gameplay.Upgrades
{
    public sealed class UpgradePreviewService
    {
        private readonly IUpgradeCatalog _catalog;
        private readonly IUpgradeProgressStorage _progressStorage;
        private readonly ICurrencyStorage _currencyStorage;
        private readonly UpgradePreviewBuilder _previewBuilder;

        public UpgradePreviewService(
            IUpgradeCatalog catalog,
            IUpgradeProgressStorage progressStorage,
            ICurrencyStorage currencyStorage,
            UpgradePreviewBuilder previewBuilder)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _progressStorage = progressStorage ?? throw new ArgumentNullException(nameof(progressStorage));
            _currencyStorage = currencyStorage ?? throw new ArgumentNullException(nameof(currencyStorage));
            _previewBuilder = previewBuilder ?? throw new ArgumentNullException(nameof(previewBuilder));
        }

        public UpgradePreview Build(UpgradeDefinition definition)
        {
            var currentLevel = _progressStorage.GetLevel(definition);
            var balance = _catalog.PurchaseCurrency == null ? 0 : _currencyStorage.GetAmount(_catalog.PurchaseCurrency);
            return _previewBuilder.Build(definition, currentLevel, balance);
        }

        public IReadOnlyList<UpgradePreview> BuildAll()
        {
            var previews = new List<UpgradePreview>();

            if (_catalog.UpgradeDefinitions == null)
                return previews;

            foreach (var definition in _catalog.UpgradeDefinitions)
            {
                if (definition == null)
                    continue;

                previews.Add(Build(definition));
            }

            return previews;
        }
    }
}
