using System;
using System.Collections.Generic;

namespace Game.Gameplay.Upgrades
{
    public readonly struct UpgradePurchaseResult
    {
        public UpgradePurchaseStatus Status { get; }
        public UpgradeDefinition Definition { get; }
        public int PreviousLevel { get; }
        public int NewLevel { get; }
        public int? Cost { get; }
        public IReadOnlyList<UpgradeValidationError> ValidationErrors { get; }
        public bool IsSuccess => Status == UpgradePurchaseStatus.Purchased;
        
        public UpgradePurchaseResult(
            UpgradePurchaseStatus status,
            UpgradeDefinition definition,
            int previousLevel,
            int newLevel,
            int? cost,
            IReadOnlyList<UpgradeValidationError> validationErrors)
        {
            Status = status;
            Definition = definition;
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
            Cost = cost;
            ValidationErrors = validationErrors ?? Array.Empty<UpgradeValidationError>();
        }
    }
}
