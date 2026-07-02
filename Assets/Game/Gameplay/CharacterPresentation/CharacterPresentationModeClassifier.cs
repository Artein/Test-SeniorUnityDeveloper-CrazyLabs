using System;
using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    public interface ICharacterPresentationModeClassifier
    {
        CharacterPresentationClassificationResult Classify(CharacterPresentationClassificationInput input);
    }

    internal sealed class CharacterPresentationModeClassifier : ICharacterPresentationModeClassifier
    {
        private readonly ICharacterPresentationTuning _tuning;

        public CharacterPresentationModeClassifier(ICharacterPresentationTuning tuning)
        {
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
        }

        public CharacterPresentationClassificationResult Classify(CharacterPresentationClassificationInput input)
        {
            if (input.HasAcceptedRunResult)
            {
                return new CharacterPresentationClassificationResult(
                    input.AcceptedRunResultSucceeded ? CharacterPresentationMode.Victory : CharacterPresentationMode.Defeat);
            }

            if (input.HasActivePull)
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.PullAnticipation);

            if (input.HasLaunchPush && input.LaunchPushElapsedSeconds < Mathf.Max(0f, _tuning.LaunchPushMinimumSeconds))
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.LaunchPush);

            if (input.IsPreLaunch || !input.IsRunActive)
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Idle);

            if (!input.SurfaceContext.IsGrounded)
                return ClassifyUngrounded(input);

            return ClassifyGrounded(input);
        }

        private CharacterPresentationClassificationResult ClassifyUngrounded(CharacterPresentationClassificationInput input)
        {
            if (input.UngroundedElapsedSeconds < Mathf.Max(0f, _tuning.AirborneDelaySeconds)
                && TryGetPreservedUngroundedLocomotion(input.CurrentMode, out var preservedMode))
            {
                return new CharacterPresentationClassificationResult(preservedMode);
            }

            return new CharacterPresentationClassificationResult(CharacterPresentationMode.Airborne);
        }

        private CharacterPresentationClassificationResult ClassifyGrounded(CharacterPresentationClassificationInput input)
        {
            if (input.CurrentMode == CharacterPresentationMode.Slide
                && input.CurrentModeElapsedSeconds < Mathf.Max(0f, _tuning.MinimumLocomotionModeDuration))
            {
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Slide);
            }

            if (HasMeaningfulGroundedMovement(input))
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Slide);

            return new CharacterPresentationClassificationResult(CharacterPresentationMode.Idle);
        }

        private bool HasMeaningfulGroundedMovement(CharacterPresentationClassificationInput input)
        {
            return Mathf.Max(0f, input.CoursePlanarSpeed) >= Mathf.Max(0f, _tuning.MeaningfulGroundedMovementThreshold);
        }

        private bool TryGetPreservedUngroundedLocomotion(CharacterPresentationMode mode, out CharacterPresentationMode preservedMode)
        {
            if (mode is CharacterPresentationMode.Slide or CharacterPresentationMode.Run)
            {
                preservedMode = CharacterPresentationMode.Slide;
                return true;
            }

            preservedMode = CharacterPresentationMode.Idle;
            return false;
        }
    }
}
