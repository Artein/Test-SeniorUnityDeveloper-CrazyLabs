namespace Game.Gameplay.GameplayState
{
    public readonly struct GameplayStateValidationError
    {
        public GameplayStateValidationErrorCode Code { get; }
        public GameplayStateTransition Transition { get; }
        public string Message { get; }

        public GameplayStateValidationError(
            GameplayStateValidationErrorCode code,
            GameplayStateTransition transition,
            string message)
        {
            Code = code;
            Transition = transition;
            Message = message;
        }
    }
}
