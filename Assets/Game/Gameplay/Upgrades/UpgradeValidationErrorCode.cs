namespace Game.Gameplay.Upgrades
{
    public enum UpgradeValidationErrorCode
    {
        NullUpgradeDefinition = 0,
        MissingUpgradeId = 1,
        MissingDisplayName = 2,
        MissingIcon = 3,
        MissingTargetStatId = 4,
        InvalidMaxLevel = 5,
        MissingCostProgression = 6,
        MissingEffectProgression = 7,
        MissingProgressionCurve = 8,
        InvalidProgressionCurve = 9,
        InvalidProgressionRange = 10,
        InvalidProgressionStep = 11,
        DuplicateExactOverrideLevel = 12,
        ExactOverrideLevelOutOfRange = 13,
        InvalidExactOverrideValue = 14,
        InvalidCostValue = 15,
        InvalidEffectValue = 16,
        MissingPurchaseCurrency = 17,
        NullUpgradeDefinitions = 18,
        DuplicateUpgradeId = 19
    }
}
