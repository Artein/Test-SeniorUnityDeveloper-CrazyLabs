using System;
using Game.Gameplay.GameplayState;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class GameplayFlowInstaller : IInstaller
    {
        private readonly GameplayStateId _runPreparationStateId;
        private readonly GameplayStateId _preLaunchStateId;
        private readonly GameplayStateId _runningStateId;

        public GameplayFlowInstaller(
            GameplayStateId runPreparationStateId,
            GameplayStateId preLaunchStateId,
            GameplayStateId runningStateId)
        {
            _runPreparationStateId = runPreparationStateId != null
                ? runPreparationStateId
                : throw new ArgumentNullException(nameof(runPreparationStateId));

            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            // TODO - AI Note: Use InjectIds instead of hardcoded argument name
            builder.RegisterEntryPoint<GameplayFlowController>()
                .WithParameter("runPreparationStateId", _runPreparationStateId)
                .WithParameter("preLaunchStateId", _preLaunchStateId)
                .WithParameter("runningStateId", _runningStateId);
        }
    }
}
