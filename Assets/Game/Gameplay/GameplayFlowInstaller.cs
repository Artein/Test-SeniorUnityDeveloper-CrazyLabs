using System;
using Game.Gameplay.GameplayState;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class GameplayFlowInstaller : IInstaller
    {
        private readonly GameplayStateId _preLaunchStateId;
        private readonly GameplayStateId _runningStateId;

        public GameplayFlowInstaller(GameplayStateId preLaunchStateId, GameplayStateId runningStateId)
        {
            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            // TODO - AI Note: Use InjectIds instead of hardcoded argument name
            builder.RegisterEntryPoint<GameplayFlowController>()
                .WithParameter("preLaunchStateId", _preLaunchStateId)
                .WithParameter("runningStateId", _runningStateId);
        }
    }
}
