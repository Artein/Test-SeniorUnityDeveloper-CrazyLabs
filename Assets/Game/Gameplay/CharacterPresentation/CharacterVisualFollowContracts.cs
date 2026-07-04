namespace Game.Gameplay.CharacterPresentation
{
    internal interface ICharacterVisualTargetPoseSource
    {
        CharacterVisualPose CurrentPose { get; }
    }

    internal interface ICharacterVisualFollowView
    {
        CharacterVisualPose CurrentVisualPose { get; }
        void ApplyVisualPose(CharacterVisualPose pose);
    }

    internal interface ICharacterVisualFollowTuning
    {
        float VisualPositionResponseRate { get; }
        float VisualHeadingResponseRate { get; }
        float VisualUpTiltResponseRate { get; }
        float VisualMaxPositionLag { get; }
        float VisualSnapDistance { get; }
        float VisualSnapAngleDegrees { get; }
    }

    internal interface ICharacterVisualPoseSmoother
    {
        void Reset();

        CharacterVisualPose Update(
            CharacterVisualPose currentVisualPose,
            CharacterVisualPose targetPose,
            ICharacterVisualFollowTuning tuning,
            float deltaTime,
            bool snap);
    }
}
