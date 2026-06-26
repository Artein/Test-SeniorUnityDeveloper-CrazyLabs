namespace Game.Gameplay.GameplayState
{
    public enum GameplayStateValidationErrorCode
    {
        NullConfig,
        MissingInitialState,
        NullTransition,
        MissingTransitionFromState,
        MissingTransitionToState,
        SelfTransition,
        DuplicateTransition
    }
}
