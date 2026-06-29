using System.Collections.Generic;

namespace Game.Gameplay.Upgrades
{
    public readonly struct UpgradePreview
    {
        public UpgradePreviewState State { get; }
        public UpgradeDefinition Definition { get; }
        public int CurrentLevel { get; }
        public int MaxLevel { get; }
        public float CurrentEffect { get; }
        public float? NextEffect { get; }
        public int? NextCost { get; }
        public bool IsAffordable { get; }
        public bool IsMaxed => State == UpgradePreviewState.Maxed;
        public bool IsValid => State != UpgradePreviewState.InvalidDefinition;
        public IReadOnlyList<UpgradeValidationError> ValidationErrors { get; }
        
        public UpgradePreview(
            UpgradePreviewState state,
            UpgradeDefinition definition,
            int currentLevel,
            int maxLevel,
            float currentEffect,
            float? nextEffect,
            int? nextCost,
            bool isAffordable,
            IReadOnlyList<UpgradeValidationError> validationErrors)
        {
            State = state;
            Definition = definition;
            CurrentLevel = currentLevel;
            MaxLevel = maxLevel;
            CurrentEffect = currentEffect;
            NextEffect = nextEffect;
            NextCost = nextCost;
            IsAffordable = isAffordable;
            ValidationErrors = validationErrors;
        }
    }
}
