using System;
using VContainer;
using VContainer.Unity;

namespace Game.Foundation.Input
{
    public sealed class UnityInputInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.Register<UnityInputBackend>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<UnityInput>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
