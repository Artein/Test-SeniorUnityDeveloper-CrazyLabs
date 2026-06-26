namespace Game.Gameplay.GameplayState
{
    internal interface IGameplayStateModel
    {
        GameplayStateId CurrentStateId { get; set; }
    }

    internal sealed class GameplayStateModel : IGameplayStateModel
    {
        public GameplayStateId CurrentStateId { get; set; }
    }
}
