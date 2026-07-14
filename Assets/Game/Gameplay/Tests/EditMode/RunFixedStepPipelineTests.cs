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
                    "Run End",
                    "Progress",
                    "Surface",
                    "Movement",
                    "Air Time",
                    "Lost Momentum"
                }));
    }

    [Test]
    public void FixedTick_RunEndBlocks_ExecutesNoRemainingRunSteps()
    {
        var steps = new RecordingRunFixedSteps
        {
            RunEndResult = RunEndFixedStepResult.BlockRemainingRunSteps
        };

        var pipeline = new RunFixedStepPipeline(steps, steps, steps, steps, steps, steps);

        ((IFixedTickable)pipeline).FixedTick();

        Assert.That(steps.Calls, Is.EqualTo(new[] { "Run End" }));
        Assert.That(steps.MovementWriteCount, Is.Zero);
    }

    [Test]
    public void FixedTick_ConsecutiveCatchUpTicksBecomeBlocked_PerformsNoSecondMovementWrite()
    {
        var steps = new RecordingRunFixedSteps();
        var pipeline = new RunFixedStepPipeline(steps, steps, steps, steps, steps, steps);

        ((IFixedTickable)pipeline).FixedTick();
        steps.RunEndResult = RunEndFixedStepResult.BlockRemainingRunSteps;
        ((IFixedTickable)pipeline).FixedTick();

        Assert.That(steps.MovementWriteCount, Is.EqualTo(1));
        Assert.That(steps.Calls[^1], Is.EqualTo("Run End"));
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
        public int MovementWriteCount { get; private set; }
        public RunEndFixedStepResult RunEndResult { get; set; }

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
            MovementWriteCount += 1;
        }

        public void UpdateAirTime()
        {
            _calls.Add("Air Time");
        }

        public RunEndFixedStepResult ResolveRunEnd()
        {
            _calls.Add("Run End");
            return RunEndResult;
        }

        public void DetectLostMomentum()
        {
            _calls.Add("Lost Momentum");
        }
    }
}
