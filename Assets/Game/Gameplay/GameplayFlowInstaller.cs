using System;
using Game.Gameplay.GameplayState;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class GameplayFlowInstaller : IInstaller
    {
        private readonly GameplayStateId _runningStateId;

        public GameplayFlowInstaller(GameplayStateId runningStateId)
        {
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.RegisterEntryPoint<GameplayFlowController>()
                .WithParameter(_runningStateId);
        }
    }
}
