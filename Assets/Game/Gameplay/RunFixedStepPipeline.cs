using System;
using VContainer.Unity;

namespace Game.Gameplay
{
    internal sealed class RunFixedStepPipeline : IFixedTickable
    {
        private readonly IRunAirTimeFixedStep _airTimeStep;
        private readonly ILostMomentumFixedStep _lostMomentumStep;
        private readonly IRunBodyMovementFixedStep _movementStep;
        private readonly IRunProgressFixedStep _progressStep;
        private readonly IRunEndFixedStep _runEndStep;
        private readonly IRunSurfaceFrameFixedStep _surfaceFrameStep;

        public RunFixedStepPipeline(
            IRunProgressFixedStep progressStep,
            IRunSurfaceFrameFixedStep surfaceFrameStep,
            IRunBodyMovementFixedStep movementStep,
            IRunAirTimeFixedStep airTimeStep,
            IRunEndFixedStep runEndStep,
            ILostMomentumFixedStep lostMomentumStep)
        {
            _progressStep = progressStep ?? throw new ArgumentNullException(nameof(progressStep));
            _surfaceFrameStep = surfaceFrameStep ?? throw new ArgumentNullException(nameof(surfaceFrameStep));
            _movementStep = movementStep ?? throw new ArgumentNullException(nameof(movementStep));
            _airTimeStep = airTimeStep ?? throw new ArgumentNullException(nameof(airTimeStep));
            _runEndStep = runEndStep ?? throw new ArgumentNullException(nameof(runEndStep));
            _lostMomentumStep = lostMomentumStep ?? throw new ArgumentNullException(nameof(lostMomentumStep));
        }

        void IFixedTickable.FixedTick()
        {
            if (_runEndStep.ResolveRunEnd() == RunEndFixedStepResult.BlockRemainingRunSteps)
                return;

            _progressStep.SampleProgress();
            _surfaceFrameStep.UpdateSurfaceFrame();
            _movementStep.UpdateMovement();
            _airTimeStep.UpdateAirTime();
            _lostMomentumStep.DetectLostMomentum();
        }
    }
}
