using System;
using System.Collections.Generic;
using System.Globalization;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Upgrades;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class RunPreparationPresenter : IInitializable, IDisposable
    {
        private readonly IRunPreparationView _view;
        private readonly ICurrencyStorage _currencyStorage;
        private readonly IUpgradeCatalog _catalog;
        private readonly UpgradePreviewService _previewService;
        private readonly UpgradePurchaseService _purchaseService;
        private readonly IRunPreparationContinueCommand _continueCommand;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly GameplayStateId _runPreparationStateId;

        private bool _isInitialized;
        private bool _isDisposed;

        public RunPreparationPresenter(
            IRunPreparationView view,
            ICurrencyStorage currencyStorage,
            IUpgradeCatalog catalog,
            UpgradePreviewService previewService,
            UpgradePurchaseService purchaseService,
            IRunPreparationContinueCommand continueCommand,
            IGameplayStateService gameplayStateService,
            GameplayStateId runPreparationStateId)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _currencyStorage = currencyStorage ?? throw new ArgumentNullException(nameof(currencyStorage));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _previewService = previewService ?? throw new ArgumentNullException(nameof(previewService));
            _purchaseService = purchaseService ?? throw new ArgumentNullException(nameof(purchaseService));
            _continueCommand = continueCommand ?? throw new ArgumentNullException(nameof(continueCommand));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));

            _runPreparationStateId = runPreparationStateId != null
                ? runPreparationStateId
                : throw new ArgumentNullException(nameof(runPreparationStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunPreparationPresenter));

            if (_isInitialized)
                return;

            _view.BuyRequested += OnBuyRequested;
            _view.ContinueRequested += OnContinueRequested;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;

            Refresh();
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (!_isInitialized)
                return;

            _view.BuyRequested -= OnBuyRequested;
            _view.ContinueRequested -= OnContinueRequested;
            _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
        }

        private void Refresh()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunPreparationPresenter));

            _view.Render(BuildViewState());
        }

        private void OnBuyRequested(UpgradeDefinition definition)
        {
            if (_isDisposed)
                return;

            _purchaseService.TryPurchase(definition);
            Refresh();
        }

        private void OnContinueRequested()
        {
            if (_isDisposed)
                return;

            _continueCommand.TryContinue();
            Refresh();
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            Refresh();
        }

        private RunPreparationViewState BuildViewState()
        {
            var coinBalance = _catalog.PurchaseCurrency == null ? 0 : _currencyStorage.GetAmount(_catalog.PurchaseCurrency);
            var upgrades = new List<RunPreparationUpgradeViewState>();

            foreach (var preview in _previewService.BuildAll())
            {
                upgrades.Add(BuildUpgradeViewState(preview));
            }

            return new RunPreparationViewState(
                _gameplayStateService.IsCurrent(_runPreparationStateId),
                coinBalance,
                coinBalance.ToString(CultureInfo.InvariantCulture),
                _catalog.PurchaseCurrency == null ? null : _catalog.PurchaseCurrency.Icon,
                upgrades);
        }

        private RunPreparationUpgradeViewState BuildUpgradeViewState(UpgradePreview preview)
        {
            var definition = preview.Definition;
            var canBuy = preview.State == UpgradePreviewState.Available && preview.IsAffordable;
            var offerEffectText = FormatOfferEffectText(definition, preview);

            return new RunPreparationUpgradeViewState(
                definition,
                definition == null ? string.Empty : definition.StableId,
                definition == null ? string.Empty : definition.DisplayName,
                definition == null ? string.Empty : definition.ShortDisplayName,
                definition == null ? null : definition.Icon,
                _catalog.PurchaseCurrency == null ? null : _catalog.PurchaseCurrency.Icon,
                preview.CurrentLevel,
                preview.MaxLevel,
                FormatOfferLevelText(preview),
                offerEffectText,
                preview.NextCost,
                preview.NextCost.HasValue ? preview.NextCost.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                canBuy,
                preview.IsMaxed,
                FormatStatusText(preview),
                preview.IsMaxed ? "MAX" : "UPGRADE");
        }

        private string FormatOfferEffectText(UpgradeDefinition definition, UpgradePreview preview)
        {
            var offerEffect = preview.NextEffect ?? preview.CurrentEffect;

            return FormatEffect(definition, offerEffect);
        }

        private string FormatOfferLevelText(UpgradePreview preview)
        {
            return preview.IsMaxed
                ? "MAX"
                : (preview.CurrentLevel + 1).ToString(CultureInfo.InvariantCulture);
        }

        private string FormatStatusText(UpgradePreview preview)
        {
            if (preview.State == UpgradePreviewState.Available && preview.IsAffordable)
                return "Available";

            if (preview.State == UpgradePreviewState.Unaffordable && preview.NextCost.HasValue)
                return $"Need {preview.NextCost.Value.ToString(CultureInfo.InvariantCulture)}";

            if (preview.State == UpgradePreviewState.Maxed)
                return "Maxed";

            return "Invalid";
        }

        private string FormatEffect(UpgradeDefinition definition, float value)
        {
            if (definition == null)
                return string.Empty;

            var decimalPlaces = Math.Max(0, definition.DisplayDecimalPlaces);
            var format = "F" + decimalPlaces.ToString(CultureInfo.InvariantCulture);

            return definition.ValueFormat switch
            {
                UpgradeValueFormat.Multiplier => "x" + value.ToString(format, CultureInfo.InvariantCulture),
                UpgradeValueFormat.Percent => value.ToString("P" + decimalPlaces.ToString(CultureInfo.InvariantCulture),
                    CultureInfo.InvariantCulture),
                _ => value.ToString(format, CultureInfo.InvariantCulture)
            };
        }
    }
}
