using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RigidbodyRunCameraSourceTests
{
    private GameObject _gameObject;
    private Rigidbody _rigidbody;
    private RigidbodyRunCameraSource _source;

    [SetUp]
    public void OnSetUp()
    {
        _gameObject = new GameObject("Rigidbody Run Camera Source Test");
        _rigidbody = _gameObject.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _source = _gameObject.AddComponent<RigidbodyRunCameraSource>();
        _source.SetRigidbodyForTests(_rigidbody);
    }

    [TearDown]
    public void OnTearDown()
    {
        Object.DestroyImmediate(_gameObject);
    }

    [Test]
    public void PositionAndLinearVelocity_ReturnRigidbodyValuesForCameraAndRunMotionInterfaces()
    {
        _rigidbody.position = new Vector3(1f, 2f, 3f);
        _rigidbody.linearVelocity = new Vector3(4f, 5f, 6f);

        var cameraSource = (IRunCameraSource)_source;
        var motionSource = (IRunMotionSource)_source;

        Assert.That(cameraSource.Position, Is.EqualTo(_rigidbody.position));
        Assert.That(cameraSource.LinearVelocity, Is.EqualTo(_rigidbody.linearVelocity));
        Assert.That(motionSource.Position, Is.EqualTo(_rigidbody.position));
        Assert.That(motionSource.LinearVelocity, Is.EqualTo(_rigidbody.linearVelocity));
    }
}
