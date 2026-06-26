using System;
using Game.Gameplay.GameplayState;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay.Slingshot
{
    public sealed class SlingshotInstaller : IInstaller
    {
        private readonly ISlingshotConfig _config;
        private readonly GameplayStateId _preLaunchStateId;
        private readonly ISlingshotView _view;
        private readonly Camera _camera;

        public SlingshotInstaller(ISlingshotConfig config, GameplayStateId preLaunchStateId, ISlingshotView view, Camera camera)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.RegisterInstance(_config).As<ISlingshotConfig>();
            builder.RegisterInstance(_view).As<ISlingshotView>();

            builder.Register<SlingshotInputProjector>(Lifetime.Singleton)
                .WithParameter(_camera)
                .As<ISlingshotInputProjector>();

            builder.RegisterEntryPoint<SlingshotController>()
                .WithParameter(_preLaunchStateId);

            builder.RegisterEntryPoint<SlingshotLaunchController>()
                .WithParameter(_preLaunchStateId);
        }
    }
}
