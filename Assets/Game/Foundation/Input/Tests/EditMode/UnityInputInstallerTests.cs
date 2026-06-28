using Game.Foundation.Input;
using NUnit.Framework;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class UnityInputInstallerTests
{
    [Test]
    public void Install_ContainerBuilt_ResolvesInputInterfacesAsSameUnityInputInstance()
    {
        var builder = new ContainerBuilder();
        var installer = new UnityInputInstaller();

        installer.Install(builder);

        using var container = builder.Build();
        var unityInput = container.Resolve<IUnityInput>();
        var supportApi = container.Resolve<IEnhancedTouchSupportApi>();
        var pointerInput = container.Resolve<IEnhancedTouchPointerInput>();

        Assert.That(supportApi, Is.SameAs(unityInput));
        Assert.That(pointerInput, Is.SameAs(unityInput));
    }
}
