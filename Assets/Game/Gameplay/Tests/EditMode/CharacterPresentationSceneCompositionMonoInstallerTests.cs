using Game.Gameplay.CharacterPresentation;
using NUnit.Framework;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public sealed class CharacterPresentationSceneCompositionMonoInstallerTests
{
    private GameObject _gameObject;

    [TearDown]
    public void OnTearDown()
    {
        if (_gameObject != null)
            Object.DestroyImmediate(_gameObject);
    }

    [Test]
    public void GameplayAssembly_ObsoleteCharacterPresentationInstallerType_DoesNotExist()
    {
        Assert.That(
            typeof(CharacterPresentationSceneCompositionMonoInstaller).Assembly.GetType(
                "Game.Gameplay.CharacterPresentation.CharacterPresentationInstaller"),
            Is.Null);
    }

    [Test]
    public void Install_ValidSceneReferences_RegistersCharacterPresentationComposition()
    {
        _gameObject = new GameObject(name: "Character Presentation Scene Installer Test");
        var view = _gameObject.AddComponent<CharacterPresentationView>();
        var rootRigidbody = _gameObject.AddComponent<Rigidbody>();
        rootRigidbody.isKinematic = true;
        var sensorView = _gameObject.AddComponent<AnimatedContactSensorPoseSyncView>();
        var sensorTarget = new GameObject(name: "Body Sensor").transform;
        sensorTarget.SetParent(_gameObject.transform, worldPositionStays: false);

        sensorView.SetReferencesForTests(
            rootRigidbody,
            new[] { new AnimatedContactSensorPoseBinding(_gameObject.transform, sensorTarget) });

        var installer = _gameObject.AddComponent<CharacterPresentationSceneCompositionMonoInstaller>();
        var builder = new ContainerBuilder();

        installer.SetReferencesForTests(view, _gameObject.transform, sensorView);
        installer.Install(builder);

        Assert.That(builder.Exists(typeof(ICharacterPresentationView), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterPresentationTuning), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterVisualFollowView), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterVisualFollowTuning), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterVisualTargetPoseSource), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IAnimatedContactSensorPoseSyncView), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterPresentationModeClassifier), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterPresentationSupportTracker), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterVisualPoseSmoother), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(ICharacterVisualLateStep), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IAnimatedContactSensorLateStep), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(CharacterVisualFollower)), Is.True);
        Assert.That(builder.Exists(typeof(CharacterPresenter)), Is.True);
    }

    [Test]
    public void GetReferenceValidationErrorsForTests_MissingSceneReferences_ReportsEveryReference()
    {
        _gameObject = new GameObject(name: "Character Presentation Scene Installer Test");
        var installer = _gameObject.AddComponent<CharacterPresentationSceneCompositionMonoInstaller>();

        Assert.That(
            installer.GetReferenceValidationErrorsForTests(),
            Is.EquivalentTo(
                new[]
                {
                    "CharacterPresentationSceneCompositionMonoInstaller requires a Character Presentation View reference.",
                    "CharacterPresentationSceneCompositionMonoInstaller requires a Visual Target reference.",
                    "CharacterPresentationSceneCompositionMonoInstaller requires an Animated Contact Sensor Pose Sync View reference."
                }));
    }
}
