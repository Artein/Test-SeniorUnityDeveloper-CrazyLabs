using System;
using System.Collections.Generic;
using Game.Gameplay.Upgrades;
using UnityEngine;

namespace Game.Gameplay
{
    public readonly struct RunPreparationViewState
    {
        public bool IsVisible { get; }
        public int CoinBalance { get; }
        public string CoinBalanceText { get; }
        public Sprite CurrencyIcon { get; }
        public IReadOnlyList<RunPreparationUpgradeViewState> Upgrades { get; }
        
        public RunPreparationViewState(
            bool isVisible,
            int coinBalance,
            string coinBalanceText,
            Sprite currencyIcon,
            IReadOnlyList<RunPreparationUpgradeViewState> upgrades)
        {
            IsVisible = isVisible;
            CoinBalance = coinBalance;
            CoinBalanceText = coinBalanceText ?? string.Empty;
            CurrencyIcon = currencyIcon;
            Upgrades = upgrades ?? Array.Empty<RunPreparationUpgradeViewState>();
        }
    }

    public readonly struct RunPreparationUpgradeViewState
    {
        public UpgradeDefinition Definition { get; }
        public string StableId { get; }
        public string DisplayName { get; }
        public string CardTitle { get; }
        public Sprite Icon { get; }
        public Sprite CostCurrencyIcon { get; }
        public int OwnedLevel { get; }
        public int MaxLevel { get; }
        public string OfferLevelText { get; }
        public string CurrentEffectText { get; }
        public string NextEffectText { get; }
        public string EffectPreviewText { get; }
        public int? NextCost { get; }
        public string NextCostText { get; }
        public bool CanBuy { get; }
        public bool IsMaxed { get; }
        public string StatusText { get; }
        public string ButtonText { get; }
        
        public RunPreparationUpgradeViewState(
            UpgradeDefinition definition,
            string stableId,
            string displayName,
            string cardTitle,
            Sprite icon,
            Sprite costCurrencyIcon,
            int ownedLevel,
            int maxLevel,
            string offerLevelText,
            string currentEffectText,
            string nextEffectText,
            string effectPreviewText,
            int? nextCost,
            string nextCostText,
            bool canBuy,
            bool isMaxed,
            string statusText,
            string buttonText)
        {
            Definition = definition;
            StableId = stableId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            CardTitle = cardTitle ?? string.Empty;
            Icon = icon;
            CostCurrencyIcon = costCurrencyIcon;
            OwnedLevel = ownedLevel;
            MaxLevel = maxLevel;
            OfferLevelText = offerLevelText ?? string.Empty;
            CurrentEffectText = currentEffectText ?? string.Empty;
            NextEffectText = nextEffectText ?? string.Empty;
            EffectPreviewText = effectPreviewText ?? string.Empty;
            NextCost = nextCost;
            NextCostText = nextCostText ?? string.Empty;
            CanBuy = canBuy;
            IsMaxed = isMaxed;
            StatusText = statusText ?? string.Empty;
            ButtonText = buttonText ?? string.Empty;
        }
    }

    public interface IRunPreparationView
    {
        event Action<UpgradeDefinition> BuyRequested;
        event Action ContinueRequested;

        void Render(RunPreparationViewState state);
    }
}
