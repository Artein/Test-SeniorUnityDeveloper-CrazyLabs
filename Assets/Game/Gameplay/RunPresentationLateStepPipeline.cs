using System;
using Game.Gameplay.CharacterPresentation;
using VContainer.Unity;

namespace Game.Gameplay
{
    internal sealed class RunPresentationLateStepPipeline : ILateTickable
    {
        private readonly IRunCameraLateStep _cameraStep;
        private readonly ICharacterVisualLateStep _characterVisualStep;
        private readonly IAnimatedContactSensorLateStep _animatedContactSensorStep;

        public RunPresentationLateStepPipeline(
            IRunCameraLateStep cameraStep,
            ICharacterVisualLateStep characterVisualStep,
            IAnimatedContactSensorLateStep animatedContactSensorStep)
        {
            _cameraStep = cameraStep ?? throw new ArgumentNullException(nameof(cameraStep));
            _characterVisualStep = characterVisualStep ?? throw new ArgumentNullException(nameof(characterVisualStep));

            if (animatedContactSensorStep is null)
                throw new ArgumentNullException(nameof(animatedContactSensorStep));

            _animatedContactSensorStep = animatedContactSensorStep;
        }

        void ILateTickable.LateTick()
        {
            _cameraStep.UpdateCamera();
            _characterVisualStep.UpdateVisual();
            _animatedContactSensorStep.SynchronizeSensors();
        }
    }
}
