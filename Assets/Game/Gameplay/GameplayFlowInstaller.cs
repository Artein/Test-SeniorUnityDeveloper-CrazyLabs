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
        private readonly GameplayStateId _runEndedStateId;

        public GameplayFlowInstaller(
            GameplayStateId runPreparationStateId,
            GameplayStateId preLaunchStateId,
            GameplayStateId runningStateId,
            GameplayStateId runEndedStateId)
        {
            _runPreparationStateId = runPreparationStateId != null
                ? runPreparationStateId
                : throw new ArgumentNullException(nameof(runPreparationStateId));

            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
            _runEndedStateId = runEndedStateId != null ? runEndedStateId : throw new ArgumentNullException(nameof(runEndedStateId));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.RegisterInstance(_runPreparationStateId).Keyed(InjectKey.GameplayStateId.RunPreparation);
            builder.RegisterInstance(_preLaunchStateId).Keyed(InjectKey.GameplayStateId.PreLaunch);
            builder.RegisterInstance(_runningStateId).Keyed(InjectKey.GameplayStateId.Running);
            builder.RegisterInstance(_runEndedStateId).Keyed(InjectKey.GameplayStateId.RunEnded);
            builder.RegisterEntryPoint<GameplayFlowController>();
        }
    }
}
