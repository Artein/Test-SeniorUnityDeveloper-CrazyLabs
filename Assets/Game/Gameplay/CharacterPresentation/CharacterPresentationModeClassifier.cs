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
                return ClassifyAcceptedRunResult(input);

            if (input.HasActivePull)
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.PullAnticipation);

            if (input.IsPreLaunch || !input.IsRunActive)
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Idle);

            if (input.HasLaunchFlight)
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.LaunchFlight);

            if (input.HasLaunchPush && input.LaunchPushElapsedSeconds < Mathf.Max(0f, _tuning.LaunchPushMinimumSeconds))
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.LaunchPush);

            if (!input.SurfaceContext.IsGrounded)
                return ClassifyUngrounded(input);

            return ClassifyGrounded(input);
        }

        private CharacterPresentationClassificationResult ClassifyAcceptedRunResult(CharacterPresentationClassificationInput input)
        {
            return new CharacterPresentationClassificationResult(
                input.AcceptedRunResultSucceeded ? CharacterPresentationMode.Victory : CharacterPresentationMode.Defeat);
        }

        private CharacterPresentationClassificationResult ClassifyUngrounded(CharacterPresentationClassificationInput input)
        {
            if (input.CurrentMode == CharacterPresentationMode.Airborne)
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Airborne);

            if (HasFallIntent(input))
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Airborne);

            if (input.UngroundedElapsedSeconds < Mathf.Max(0f, _tuning.FallEnterMinimumUngroundedSeconds)
                && TryGetPreservedUngroundedLocomotion(input.CurrentMode, out var preservedMode))
            {
                return new CharacterPresentationClassificationResult(preservedMode);
            }

            if (HasMeaningfulGroundedMovement(input))
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Slide);

            return new CharacterPresentationClassificationResult(CharacterPresentationMode.Idle);
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

        private bool HasFallIntent(CharacterPresentationClassificationInput input)
        {
            var hardTimeout = Mathf.Max(0f, _tuning.FallEnterHardUngroundedSeconds);

            if (input.UngroundedElapsedSeconds >= hardTimeout)
                return true;

            if (input.UngroundedElapsedSeconds < Mathf.Max(0f, _tuning.FallEnterMinimumUngroundedSeconds))
                return false;

            if (input.CourseVerticalSpeed <= -Mathf.Max(0f, _tuning.FallEnterMinimumDownwardSpeed))
                return true;

            return input.UngroundedVerticalSeparation <= -Mathf.Max(0f, _tuning.FallEnterMinimumVerticalSeparation);
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
