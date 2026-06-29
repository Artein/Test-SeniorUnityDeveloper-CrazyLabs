namespace Game.Gameplay.CharacterPresentation
{
    public readonly struct CharacterPresentationClassificationResult
    {
        public CharacterPresentationMode Mode { get; }

        public CharacterPresentationClassificationResult(CharacterPresentationMode mode)
        {
            Mode = mode;
        }
    }
}
