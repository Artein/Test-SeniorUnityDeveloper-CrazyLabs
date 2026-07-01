namespace Game.Gameplay.Economy
{
    public sealed class RunRewardSourceCatalog
    {
        public RunRewardSource PickedUpCoins { get; } = new("picked-up-coins", "Picked-Up Coins", 10, showWhenZero: false);
        public RunRewardSource DistanceBonus { get; } = new("distance-bonus", "Distance Bonus", 20, showWhenZero: false);
        public RunRewardSource AirTimeBonus { get; } = new("air-time-bonus", "Air Time Bonus", 30, showWhenZero: false);
    }
}
