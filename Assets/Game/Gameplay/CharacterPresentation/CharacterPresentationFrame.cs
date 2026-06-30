using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    public readonly struct CharacterPresentationFrame
    {
        public CharacterPresentationMode Mode { get; }
        public float PlaybackSpeedMultiplier { get; }
        public float NormalizedPull { get; }
        public float NormalizedLaunchPower { get; }
        public float NormalizedPullOffset { get; }
        public float NormalizedLaunchOffset { get; }

        public CharacterPresentationFrame(
            CharacterPresentationMode mode,
            float playbackSpeedMultiplier,
            float normalizedPull = 0f,
            float normalizedLaunchPower = 0f,
            float normalizedPullOffset = 0f,
            float normalizedLaunchOffset = 0f)
        {
            Mode = mode;
            PlaybackSpeedMultiplier = float.IsFinite(playbackSpeedMultiplier) ? Mathf.Max(0f, playbackSpeedMultiplier) : 1f;
            NormalizedPull = float.IsFinite(normalizedPull) ? Mathf.Clamp01(normalizedPull) : 0f;
            NormalizedLaunchPower = float.IsFinite(normalizedLaunchPower) ? Mathf.Clamp01(normalizedLaunchPower) : 0f;
            NormalizedPullOffset = float.IsFinite(normalizedPullOffset) ? Mathf.Clamp(normalizedPullOffset, -1f, 1f) : 0f;
            NormalizedLaunchOffset = float.IsFinite(normalizedLaunchOffset) ? Mathf.Clamp(normalizedLaunchOffset, -1f, 1f) : 0f;
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
        float LaunchPushMinimumSeconds { get; }
        float SlideReferenceSpeed { get; }
        float RunReferenceSpeed { get; }
        float MinimumPlaybackSpeedMultiplier { get; }
        float MaximumPlaybackSpeedMultiplier { get; }
    }
}
