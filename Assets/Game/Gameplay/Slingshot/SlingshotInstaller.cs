using System;
using Game.Foundation.Time;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay.Slingshot
{
    public sealed class SlingshotInstaller : IInstaller
    {
        private readonly ISlingshotConfig _config;
        private readonly ISlingshotView _view;
        private readonly Camera _camera;

        public SlingshotInstaller(ISlingshotConfig config, ISlingshotView view, Camera camera)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.RegisterInstance(_config).As<ISlingshotConfig>();
            builder.RegisterInstance(_view).As<ISlingshotView>();
            builder.Register<ITime, UnityTime>(Lifetime.Singleton);

            builder.RegisterEntryPoint<SlingshotController>();
            builder.RegisterEntryPoint<SlingshotLaunchController>();
            builder.Register<ISlingshotBandShapeProvider, SlingshotBandShapeProvider>(Lifetime.Singleton);

            builder.Register<ISlingshotInputProjector, SlingshotInputProjector>(Lifetime.Singleton)
                .WithParameter(_camera);
        }
    }
}
