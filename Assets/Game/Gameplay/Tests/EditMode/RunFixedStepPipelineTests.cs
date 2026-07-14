using System.Collections.Generic;
using Game.Gameplay;
using NUnit.Framework;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunFixedStepPipelineTests
{
    [Test]
    public void FixedTick_Always_ExecutesRunStepsInDefinedOrder()
    {
        var steps = new RecordingRunFixedSteps();

        var pipeline = new RunFixedStepPipeline(
            steps,
            steps,
            steps,
            steps,
            steps,
            steps);

        ((IFixedTickable)pipeline).FixedTick();

        Assert.That(
            steps.Calls,
            Is.EqualTo(
                new[]
                {
                    "Progress",
                    "Surface",
                    "Movement",
                    "Air Time",
                    "Run End",
                    "Lost Momentum"
                }));
    }

    private sealed class RecordingRunFixedSteps :
        IRunProgressFixedStep,
        IRunSurfaceFrameFixedStep,
        IRunBodyMovementFixedStep,
        IRunAirTimeFixedStep,
        IRunEndFixedStep,
        ILostMomentumFixedStep
    {
        private readonly List<string> _calls = new();

        public IReadOnlyList<string> Calls => _calls;

        public void SampleProgress()
        {
            _calls.Add("Progress");
        }

        public void UpdateSurfaceFrame()
        {
            _calls.Add("Surface");
        }

        public void UpdateMovement()
        {
            _calls.Add("Movement");
        }

        public void UpdateAirTime()
        {
            _calls.Add("Air Time");
        }

        public void ResolveRunEnd()
        {
            _calls.Add("Run End");
        }

        public void DetectLostMomentum()
        {
            _calls.Add("Lost Momentum");
        }
    }
}
