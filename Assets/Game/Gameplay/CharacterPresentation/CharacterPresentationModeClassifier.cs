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

            if (input.IsPreLaunch || !input.IsRunActive)
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Idle);

            if (!input.SurfaceContext.IsGrounded)
                return ClassifyUngrounded(input);

            return ClassifyGrounded(input);
        }

        private CharacterPresentationClassificationResult ClassifyUngrounded(CharacterPresentationClassificationInput input)
        {
            if (input.UngroundedElapsedSeconds < Mathf.Max(0f, _tuning.AirborneDelaySeconds)
                && IsLocomotionMode(input.CurrentMode))
            {
                return new CharacterPresentationClassificationResult(input.CurrentMode);
            }

            return new CharacterPresentationClassificationResult(CharacterPresentationMode.Airborne);
        }

        private CharacterPresentationClassificationResult ClassifyGrounded(CharacterPresentationClassificationInput input)
        {
            if (IsLocomotionMode(input.CurrentMode)
                && input.CurrentModeElapsedSeconds < Mathf.Max(0f, _tuning.MinimumLocomotionModeDuration))
            {
                return new CharacterPresentationClassificationResult(input.CurrentMode);
            }

            var downhillDegrees = input.SurfaceContext.ForwardDownhillDegrees;
            var slideEnterDownhillDegrees = Mathf.Max(0f, _tuning.SlideEnterDownhillDegrees);
            var slideExitDownhillDegrees = Mathf.Clamp(_tuning.SlideExitDownhillDegrees, 0f, slideEnterDownhillDegrees);
            var runFlatMaximumAbsSlopeDegrees = Mathf.Max(0f, _tuning.RunFlatMaximumAbsSlopeDegrees);

            if (downhillDegrees >= slideEnterDownhillDegrees)
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Slide);

            if (input.CurrentMode == CharacterPresentationMode.Run
                && downhillDegrees < slideEnterDownhillDegrees
                && HasForwardRunSpeed(input))
            {
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Run);
            }

            if (input.CurrentMode == CharacterPresentationMode.Slide
                && downhillDegrees > slideExitDownhillDegrees)
            {
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Slide);
            }

            if (Mathf.Abs(downhillDegrees) <= runFlatMaximumAbsSlopeDegrees
                && HasForwardRunSpeed(input))
            {
                return new CharacterPresentationClassificationResult(CharacterPresentationMode.Run);
            }

            return new CharacterPresentationClassificationResult(CharacterPresentationMode.Slide);
        }

        private bool HasForwardRunSpeed(CharacterPresentationClassificationInput input)
        {
            return input.CourseForwardSpeed >= Mathf.Max(0f, _tuning.RunMinimumForwardSpeed);
        }

        private bool IsLocomotionMode(CharacterPresentationMode mode)
        {
            return mode is CharacterPresentationMode.Slide or CharacterPresentationMode.Run;
        }
    }
}
