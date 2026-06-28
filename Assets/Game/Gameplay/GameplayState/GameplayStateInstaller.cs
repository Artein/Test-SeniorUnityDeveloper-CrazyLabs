using System;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay.GameplayState
{
    public sealed class GameplayStateInstaller : IInstaller
    {
        private readonly IGameplayStateConfig _config;

        public GameplayStateInstaller(IGameplayStateConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.RegisterInstance(_config);
            builder.Register<IGameplayStateValidator, GameplayStateValidator>(Lifetime.Singleton);
            builder.Register<IGameplayStateModel, GameplayStateModel>(Lifetime.Singleton);
            builder.Register<IGameplayStateService, GameplayStateService>(Lifetime.Singleton);
        }
    }
}
