using Game.Gameplay.GameplayState;
using Game.Gameplay.GameplayState.Tests.EditMode;
using NUnit.Framework;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class GameplayStateInstallerTests : GameplayStateTestFixture
{
    [Test]
    public void Install_ValidConfig_ResolvesGameplayStateService()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var config = CreateConfig(preLaunch, CreateTransition(preLaunch, running));
        var builder = new ContainerBuilder();
        var installer = new GameplayStateInstaller(config);

        installer.Install(builder);

        using var container = builder.Build();
        var service = container.Resolve<IGameplayStateService>();

        Assert.That(service.CurrentStateId, Is.SameAs(preLaunch));
    }
}
