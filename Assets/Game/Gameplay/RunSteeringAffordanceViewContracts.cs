namespace Game.Gameplay
{
    internal interface IRunSteeringAffordanceView
    {
        void Show(RunSteeringAffordancePresentationState state);
        void Update(RunSteeringAffordancePresentationState state);
        void Hide(RunSteeringAffordancePresentationState state);
        void Reset();
    }

    internal sealed class NullRunSteeringAffordanceView : IRunSteeringAffordanceView
    {
        public void Show(RunSteeringAffordancePresentationState state)
        {
        }

        public void Update(RunSteeringAffordancePresentationState state)
        {
        }

        public void Hide(RunSteeringAffordancePresentationState state)
        {
        }

        public void Reset()
        {
        }
    }
}
