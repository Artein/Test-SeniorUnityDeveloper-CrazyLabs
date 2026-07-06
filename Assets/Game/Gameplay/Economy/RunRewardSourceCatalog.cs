namespace Game.Gameplay.Economy
{
    public sealed class RunRewardSourceCatalog
    {
        public RunRewardSource PickedUpCoins { get; } = new(stableId: "picked-up-coins", label: "Picked-Up Coins", order: 10, showWhenZero: false);
        public RunRewardSource DistanceBonus { get; } = new(stableId: "distance-bonus", label: "Distance Bonus", order: 20, showWhenZero: false);
        public RunRewardSource AirTimeBonus { get; } = new(stableId: "air-time-bonus", label: "Air Time Bonus", order: 30, showWhenZero: false);
    }
}
