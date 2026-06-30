namespace Game.Gameplay.GameplayState
{
    public static class InjectKey
    {
        public static class GameplayStateId
        {
            public const string RunPreparation = nameof(GameplayStateId) + ":" + nameof(RunPreparation);
            public const string PreLaunch = nameof(GameplayStateId) + ":" + nameof(PreLaunch);
            public const string Running = nameof(GameplayStateId) + ":" + nameof(Running);
            public const string RunEnded = nameof(GameplayStateId) + ":" + nameof(RunEnded);
        }

        public static class GameplayStatId
        {
            public const string SlingshotLaunchPower = nameof(GameplayStatId) + ":" + nameof(SlingshotLaunchPower);
            public const string PlayerMaxSpeed = nameof(GameplayStatId) + ":" + nameof(PlayerMaxSpeed);
            public const string PlayerSteeringResponsiveness = nameof(GameplayStatId) + ":" + nameof(PlayerSteeringResponsiveness);
            public const string CoinPickupMultiplier = nameof(GameplayStatId) + ":" + nameof(CoinPickupMultiplier);
        }

        public static class CurrencyDefinition
        {
            public const string Coin = nameof(CurrencyDefinition) + ":" + nameof(Coin);
        }

        public static class Pickups
        {
            public const string LevelPickups = nameof(Pickups) + ":" + nameof(LevelPickups);
        }

        public static class Tags
        {
            public const string Player = nameof(Tags) + ":" + nameof(Player);
        }
    }
}
