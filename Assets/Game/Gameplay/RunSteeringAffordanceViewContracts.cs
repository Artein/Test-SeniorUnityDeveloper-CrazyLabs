namespace Game.Gameplay
{
    internal interface IRunSteeringAffordanceView
    {
        void Show(RunSteeringAffordancePresentationState state);
        void Update(RunSteeringAffordancePresentationState state);
        void Hide(RunSteeringAffordancePresentationState state);
        void Reset();
    }
}
