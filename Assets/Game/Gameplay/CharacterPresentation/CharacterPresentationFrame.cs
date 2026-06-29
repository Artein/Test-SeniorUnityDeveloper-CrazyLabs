namespace Game.Gameplay.CharacterPresentation
{
    public readonly struct CharacterPresentationFrame
    {
        public CharacterPresentationMode Mode { get; }
        public float PlaybackSpeedMultiplier { get; }

        public CharacterPresentationFrame(CharacterPresentationMode mode, float playbackSpeedMultiplier)
        {
            Mode = mode;
            PlaybackSpeedMultiplier = playbackSpeedMultiplier;
        }
    }

    public interface ICharacterPresentationView
    {
        void ApplyFrame(CharacterPresentationFrame frame);
    }

    public interface ICharacterPresentationTuning
    {
        float AirborneDelaySeconds { get; }
        float SlideEnterDownhillDegrees { get; }
        float SlideExitDownhillDegrees { get; }
        float RunFlatMaximumAbsSlopeDegrees { get; }
        float RunMinimumForwardSpeed { get; }
        float MinimumLocomotionModeDuration { get; }
        float SlideReferenceSpeed { get; }
        float RunReferenceSpeed { get; }
        float MinimumPlaybackSpeedMultiplier { get; }
        float MaximumPlaybackSpeedMultiplier { get; }
    }
}
