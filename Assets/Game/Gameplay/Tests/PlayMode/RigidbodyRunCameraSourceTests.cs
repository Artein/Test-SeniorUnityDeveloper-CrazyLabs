using System.Collections;
using System.Reflection;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RigidbodyRunCameraSourceTests
{
    private GameObject _gameObject;
    private LifetimeScope _lifetimeScope;
    private Rigidbody _rigidbody;
    private RigidbodyRunCameraSource _source;

    [SetUp]
    public void OnSetUp()
    {
        _gameObject = new GameObject(name: "Rigidbody Run Camera Source Test");
        _rigidbody = _gameObject.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _source = _gameObject.AddComponent<RigidbodyRunCameraSource>();
        _source.SetRigidbodyForTests(_rigidbody);
    }

    [TearDown]
    public void OnTearDown()
    {
        if (_lifetimeScope != null)
            Object.DestroyImmediate(_lifetimeScope.gameObject);

        Object.DestroyImmediate(_gameObject);
    }

    [Test]
    public void given_InterfaceOnlyAdapter_when_DeclaredPublicPropertiesInspected_then_NoneAreExposed()
    {
        var publicProperties = typeof(RigidbodyRunCameraSource).GetProperties(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.That(publicProperties, Is.Empty);
    }

    [Test]
    public void given_NoInterpolationPoseSplit_when_SourceReadThroughBothContracts_then_EachReturnsExpectedPose()
    {
        var sharedPosition = new Vector3(x: 1f, y: 2f, z: 3f);
        _rigidbody.transform.position = sharedPosition;
        _rigidbody.position = sharedPosition;
        _rigidbody.linearVelocity = new Vector3(x: 4f, y: 5f, z: 6f);

        var cameraSource = (IRunCameraSource)_source;
        var motionSource = (IRunMotionSource)_source;

        Assert.That(cameraSource.Position, Is.EqualTo(_rigidbody.transform.position));
        Assert.That(cameraSource.LinearVelocity, Is.EqualTo(_rigidbody.linearVelocity));
        Assert.That(motionSource.Position, Is.EqualTo(_rigidbody.position));
        Assert.That(motionSource.LinearVelocity, Is.EqualTo(_rigidbody.linearVelocity));
    }

    [UnityTest]
    public IEnumerator given_InterpolationPoseSplit_when_SourceReadThroughBothContracts_then_EachReturnsExpectedTimeDomainPose()
    {
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.position = Vector3.zero;
        _rigidbody.linearVelocity = Vector3.right * 30f;

        yield return new WaitForFixedUpdate();
        yield return null;

        var renderPoseDistance = Vector3.Distance(_rigidbody.position, _rigidbody.transform.position);
        Assume.That(renderPoseDistance, Is.GreaterThan(expected: 0.0001f));

        var cameraSource = (IRunCameraSource)_source;
        var motionSource = (IRunMotionSource)_source;

        Assert.That(cameraSource.Position, Is.EqualTo(_rigidbody.transform.position));
        Assert.That(motionSource.Position, Is.EqualTo(_rigidbody.position));
        Assert.That(cameraSource.Position, Is.Not.EqualTo(motionSource.Position));
        Assert.That(cameraSource.LinearVelocity, Is.EqualTo(_rigidbody.linearVelocity));
        Assert.That(motionSource.LinearVelocity, Is.EqualTo(_rigidbody.linearVelocity));
    }

    [UnityTest]
    public IEnumerator given_InterpolatedRigidbody_when_VContainerFixedTickRuns_then_MotionSourceUsesPhysicsPose()
    {
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.position = Vector3.zero;
        _rigidbody.linearVelocity = Vector3.right * 30f;
        var observation = new FixedPoseObservation();
        var motionSource = (IRunMotionSource)_source;

        _lifetimeScope = LifetimeScope.Create(
            builder =>
            {
                builder.RegisterInstance(_rigidbody);
                builder.RegisterInstance(motionSource);
                builder.RegisterInstance(observation);
                builder.RegisterEntryPoint<FixedPoseProbe>();
            },
            name: "Fixed Pose Probe Lifetime Scope");

        yield return new WaitForFixedUpdate();

        Assert.That(observation.HasSample, Is.True);
        Assert.That(observation.MotionPosition, Is.EqualTo(observation.RigidbodyPosition));

        yield return null;

        var cameraSource = (IRunCameraSource)_source;
        var renderPoseDistance = Vector3.Distance(_rigidbody.position, _rigidbody.transform.position);
        Assume.That(renderPoseDistance, Is.GreaterThan(expected: 0.0001f));
        Assert.That(cameraSource.Position, Is.EqualTo(_rigidbody.transform.position));
        Assert.That(motionSource.Position, Is.EqualTo(_rigidbody.position));
    }

    private sealed class FixedPoseObservation
    {
        public bool HasSample { get; private set; }
        public Vector3 MotionPosition { get; private set; }
        public Vector3 RigidbodyPosition { get; private set; }

        public void Record(Vector3 rigidbodyPosition, Vector3 motionPosition)
        {
            RigidbodyPosition = rigidbodyPosition;
            MotionPosition = motionPosition;
            HasSample = true;
        }
    }

    private sealed class FixedPoseProbe : IFixedTickable
    {
        private readonly IRunMotionSource _motionSource;
        private readonly FixedPoseObservation _observation;
        private readonly Rigidbody _rigidbody;

        public FixedPoseProbe(
            Rigidbody rigidbody,
            IRunMotionSource motionSource,
            FixedPoseObservation observation)
        {
            _rigidbody = rigidbody;
            _motionSource = motionSource;
            _observation = observation;
        }

        public void FixedTick()
        {
            _observation.Record(_rigidbody.position, _motionSource.Position);
        }
    }
}
