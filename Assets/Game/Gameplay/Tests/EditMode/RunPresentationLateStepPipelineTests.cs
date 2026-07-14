using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.CharacterPresentation;
using NUnit.Framework;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunPresentationLateStepPipelineTests
{
    [Test]
    public void LateTick_Always_ExecutesPresentationStepsInDefinedOrder()
    {
        var steps = new RecordingPresentationLateSteps();

        var pipeline = new RunPresentationLateStepPipeline(
            steps,
            steps,
            steps);

        ((ILateTickable)pipeline).LateTick();

        Assert.That(
            steps.Calls,
            Is.EqualTo(
                new[]
                {
                    "Camera",
                    "Character Visual",
                    "Animated Contact Sensors"
                }));
    }

    private sealed class RecordingPresentationLateSteps :
        IRunCameraLateStep,
        ICharacterVisualLateStep,
        IAnimatedContactSensorLateStep
    {
        private readonly List<string> _calls = new();

        public IReadOnlyList<string> Calls => _calls;

        public void UpdateCamera()
        {
            _calls.Add("Camera");
        }

        public void UpdateVisual()
        {
            _calls.Add("Character Visual");
        }

        public void SynchronizeSensors()
        {
            _calls.Add("Animated Contact Sensors");
        }
    }
}
