using System;
using VContainer.Unity;

namespace Game.Gameplay
{
    internal sealed class RunFixedStepPipeline : IFixedTickable
    {
        private readonly IRunProgressFixedStep _progressStep;
        private readonly IRunSurfaceFrameFixedStep _surfaceFrameStep;
        private readonly IRunBodyMovementFixedStep _movementStep;
        private readonly IRunAirTimeFixedStep _airTimeStep;
        private readonly IRunEndFixedStep _runEndStep;
        private readonly ILostMomentumFixedStep _lostMomentumStep;

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
            _progressStep.SampleProgress();
            _surfaceFrameStep.UpdateSurfaceFrame();
            _movementStep.UpdateMovement();
            _airTimeStep.UpdateAirTime();
            _runEndStep.ResolveRunEnd();
            _lostMomentumStep.DetectLostMomentum();
        }
    }
}
