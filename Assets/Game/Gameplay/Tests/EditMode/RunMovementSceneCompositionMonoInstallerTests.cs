using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public sealed class RunMovementSceneCompositionMonoInstallerTests
{
    private RunBodyMovementConfig _config;
    private GameObject _gameObject;

    [TearDown]
    public void OnTearDown()
    {
        if (_gameObject != null)
            Object.DestroyImmediate(_gameObject);

        if (_config != null)
            Object.DestroyImmediate(_config);
    }

    [Test]
    public void GameplayAssembly_ObsoleteRunMovementInstallerType_DoesNotExist()
    {
        Assert.That(
            typeof(RunMovementSceneCompositionMonoInstaller).Assembly.GetType("Game.Gameplay.RunMovementInstaller"),
            Is.Null);
    }

    [Test]
    public void Install_ValidSceneReferences_RegistersCameraAndMovementComposition()
    {
        _config = ScriptableObject.CreateInstance<RunBodyMovementConfig>();
        _gameObject = new GameObject(name: "Run Movement Scene Installer Test");
        var movementTarget = _gameObject.AddComponent<RigidbodyRunBodyMovementTarget>();
        var motionSource = _gameObject.AddComponent<RigidbodyRunCameraSource>();
        var progressFrameSource = _gameObject.AddComponent<RunProgressFrameSource>();
        var contactNotifier = _gameObject.AddComponent<RigidbodyContactNotifier>();
        var installer = _gameObject.AddComponent<RunMovementSceneCompositionMonoInstaller>();
        var builder = new ContainerBuilder();

        installer.SetReferencesForTests(
            _config,
            movementTarget,
            motionSource,
            progressFrameSource,
            contactNotifier);

        installer.Install(builder);

        Assert.That(builder.Exists(typeof(IRunCameraSource), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunMotionSource), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunBodyMovementTarget), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunProgressFrameSource), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRigidbodyContactNotifier), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunBodySpeedConfig), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunBodyMovementValidityConfig), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunLaunchLandingStabilizationConfig), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunSteeringConfig), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(RunSupportAttachmentConfig)), Is.True);
        Assert.That(builder.Exists(typeof(RunSurfaceStabilityConfig)), Is.True);
        Assert.That(builder.Exists(typeof(RunSteeringFrameConfig)), Is.True);
        Assert.That(builder.Exists(typeof(IRunBodySpeedEvaluator), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunBodySpeedDiagnosticsSource), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunBodySpeedDiagnosticsSink), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunSteeringEvaluator), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunLaunchLandingStabilizer), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunContactClassifier), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunSurfaceSlopeCalculator), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(RunSupportAttachmentPolicy)), Is.True);
        Assert.That(builder.Exists(typeof(RunSurfaceStabilityPolicy)), Is.True);
        Assert.That(builder.Exists(typeof(RunSteeringFramePolicy)), Is.True);
        Assert.That(builder.Exists(typeof(IRunSurfaceFrameSource), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunSteeringFrameSource), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunSteeringFrameResetter), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunSurfaceFrameFixedStep), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(RunBodySpeedEnvelopeValidator)), Is.True);
        Assert.That(builder.Exists(typeof(IRunBodyMovementFixedStep), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IInitializable), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(RunBodyMovementController)), Is.True);
    }

    [Test]
    public void GetReferenceValidationErrorsForTests_MissingSceneReferences_ReportsEveryReference()
    {
        _gameObject = new GameObject(name: "Run Movement Scene Installer Test");
        var installer = _gameObject.AddComponent<RunMovementSceneCompositionMonoInstaller>();

        Assert.That(
            installer.GetReferenceValidationErrorsForTests(),
            Is.EquivalentTo(
                new[]
                {
                    "RunMovementSceneCompositionMonoInstaller requires a Run Body Movement Config reference.",
                    "RunMovementSceneCompositionMonoInstaller requires a Run Body Movement Target reference.",
                    "RunMovementSceneCompositionMonoInstaller requires a Run Camera Source reference.",
                    "RunMovementSceneCompositionMonoInstaller requires a Run Progress Frame Source reference.",
                    "RunMovementSceneCompositionMonoInstaller requires a Rigidbody Contact Notifier reference."
                }));
    }
}
