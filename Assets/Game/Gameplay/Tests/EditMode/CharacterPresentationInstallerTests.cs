using Game.Gameplay.CharacterPresentation;
using NUnit.Framework;
using UnityEngine;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class CharacterPresentationInstallerTests
{
    private GameObject _gameObject;

    [TearDown]
    public void OnTearDown()
    {
        if (_gameObject != null)
            Object.DestroyImmediate(_gameObject);
    }

    [Test]
    public void Install_ValidDependencies_RegistersCharacterPresentationCompositionWithoutResolvingExternalGraph()
    {
        _gameObject = new GameObject(name: "Character Presentation Installer Test");
        var view = _gameObject.AddComponent<CharacterPresentationView>();
        var sensorView = _gameObject.AddComponent<AnimatedContactSensorPoseSyncView>();
        var builder = new ContainerBuilder();

        new CharacterPresentationInstaller(view, _gameObject.transform, sensorView).Install(builder);

        Assert.That(builder.Exists(typeof(ICharacterPresentationView), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterPresentationTuning), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterVisualFollowView), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterVisualFollowTuning), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IAnimatedContactSensorPoseSyncView), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterPresentationModeClassifier), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterPresentationSupportTracker), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterVisualPoseSmoother), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(CharacterVisualFollower)), Is.True);
        Assert.That(builder.Exists(typeof(AnimatedContactSensorPoseSync)), Is.True);
        Assert.That(builder.Exists(typeof(CharacterPresenter)), Is.True);
    }
}
