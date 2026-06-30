namespace Game.Gameplay.Upgrades
{
    public enum UpgradePurchaseStatus
    {
        Purchased = 0,
        MissingDefinition = 1,
        InvalidDefinition = 2,
        MaxLevelReached = 3,
        InsufficientCurrency = 4,
        SaveFailed = 5
    }
}
