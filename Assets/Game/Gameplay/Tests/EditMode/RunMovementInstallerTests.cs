using System;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public sealed class RunMovementInstallerTests
{
    private RunBodyMovementConfig _config;

    [TearDown]
    public void OnTearDown()
    {
        if (_config != null)
            Object.DestroyImmediate(_config);
    }

    [Test]
    public void Install_ValidDependencies_RegistersMovementCompositionWithoutResolvingExternalGraph()
    {
        _config = ScriptableObject.CreateInstance<RunBodyMovementConfig>();
        var movementTarget = new FakeMovementTarget();
        var motionSource = new FakeMotionSource();
        var progressFrameSource = new FakeProgressFrameSource();
        var contactNotifier = new FakeContactNotifier();
        var builder = new ContainerBuilder();

        new RunMovementInstaller(
                _config,
                movementTarget,
                motionSource,
                progressFrameSource,
                contactNotifier)
            .Install(builder);

        Assert.That(builder.Exists(typeof(IRunBodyMovementTarget), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunMotionSource), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunProgressFrameSource), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRigidbodyContactNotifier), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunBodySpeedConfig), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(RunSupportAttachmentConfig)), Is.True);
        Assert.That(builder.Exists(typeof(RunSurfaceStabilityConfig)), Is.True);
        Assert.That(builder.Exists(typeof(RunSteeringFrameConfig)), Is.True);
        Assert.That(builder.Exists(typeof(IRunBodySpeedEvaluator), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(IRunSurfaceFrameSource), includeInterfaceTypes: true), Is.True);
        Assert.That(builder.Exists(typeof(RunBodyMovementController)), Is.True);
    }

    private sealed class FakeMovementTarget : IRunBodyMovementTarget
    {
        public Vector3 LinearVelocity => Vector3.zero;

        public void ApplyTargetState(RunBodyMovementTargetState targetState)
        {
        }
    }

    private sealed class FakeMotionSource : IRunMotionSource
    {
        public Vector3 LinearVelocity => Vector3.zero;
        public Vector3 Position => Vector3.zero;
    }

    private sealed class FakeProgressFrameSource : IRunProgressFrameSource
    {
        public bool TryCreateSnapshot(Vector3 origin, out RunProgressFrameSnapshot snapshot, out string error)
        {
            snapshot = default;
            error = string.Empty;
            return false;
        }
    }

    private sealed class FakeContactNotifier : IRigidbodyContactNotifier
    {
        public event Action<RigidbodyCollisionNotification> CollisionEntered;
        public event Action<RigidbodyTriggerNotification> TriggerEntered;
    }
}
