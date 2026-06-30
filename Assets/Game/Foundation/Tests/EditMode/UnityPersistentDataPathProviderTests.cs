using Game.Foundation.Persistence;
using NUnit.Framework;
using UnityEngine;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class UnityPersistentDataPathProviderTests
{
    [Test]
    public void PersistentDataPath_ReturnsUnityPersistentDataPath()
    {
        var provider = new UnityPersistentDataPathProvider();

        Assert.That(provider.PersistentDataPath, Is.EqualTo(Application.persistentDataPath));
    }

    [Test]
    public void VContainerRegistration_ResolvesPersistentDataPathProvider()
    {
        var builder = new ContainerBuilder();

        builder.Register<IPersistentDataPathProvider, UnityPersistentDataPathProvider>(Lifetime.Singleton);

        using var container = builder.Build();
        var provider = container.Resolve<IPersistentDataPathProvider>();

        Assert.That(provider, Is.TypeOf<UnityPersistentDataPathProvider>());
    }
}
