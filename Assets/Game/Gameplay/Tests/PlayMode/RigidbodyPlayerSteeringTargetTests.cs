using System;
using System.Collections;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class RigidbodyPlayerSteeringTargetTests
{
    private GameObject _gameObject;
    private Rigidbody _rigidbody;
    private RigidbodyPlayerSteeringTarget _target;

    [SetUp]
    public void OnSetUp()
    {
        _gameObject = new GameObject("Rigidbody Player Steering Target Test");
        _rigidbody = _gameObject.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _target = _gameObject.AddComponent<RigidbodyPlayerSteeringTarget>();
        _target.SetRigidbodyForTests(_rigidbody);
    }

    [TearDown]
    public void OnTearDown()
    {
        UnityEngine.Object.DestroyImmediate(_gameObject);
    }

    [Test]
    public void LinearVelocity_ReturnsRigidbodyLinearVelocity()
    {
        var velocity = new Vector3(1f, 2f, 3f);
        _rigidbody.linearVelocity = velocity;

        Assert.That(((IPlayerSteeringTarget)_target).LinearVelocity, Is.EqualTo(velocity));
    }

    [Test]
    public void ApplyVelocity_FiniteVelocity_SetsLinearVelocity()
    {
        var velocity = new Vector3(2f, -1f, 4f);

        ((IPlayerSteeringTarget)_target).ApplyVelocity(velocity);

        Assert.That(_rigidbody.linearVelocity.x, Is.EqualTo(velocity.x).Within(0.0001f));
        Assert.That(_rigidbody.linearVelocity.y, Is.EqualTo(velocity.y).Within(0.0001f));
        Assert.That(_rigidbody.linearVelocity.z, Is.EqualTo(velocity.z).Within(0.0001f));
    }

    [Test]
    public void ApplyVelocity_NonFiniteVelocity_Throws()
    {
        Assert.That(
            () => ((IPlayerSteeringTarget)_target).ApplyVelocity(new Vector3(float.NaN, 0f, 1f)),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("Steering velocity"));
    }

    [UnityTest]
    public IEnumerator ApplySteering_FiniteVelocity_SetsLinearVelocityAndMovesRotation()
    {
        var velocity = new Vector3(3f, 0.5f, 7f);
        var rotation = Quaternion.Euler(0f, 45f, 0f);

        ((IPlayerSteeringTarget)_target).ApplySteering(velocity, rotation);
        yield return new WaitForFixedUpdate();

        Assert.That(_rigidbody.linearVelocity.x, Is.EqualTo(velocity.x).Within(0.0001f));
        Assert.That(_rigidbody.linearVelocity.y, Is.EqualTo(velocity.y).Within(0.0001f));
        Assert.That(_rigidbody.linearVelocity.z, Is.EqualTo(velocity.z).Within(0.0001f));
        Assert.That(Quaternion.Angle(_rigidbody.rotation, rotation), Is.LessThan(0.1f));
    }

    [Test]
    public void ApplySteering_NonFiniteVelocity_Throws()
    {
        Assert.That(
            () => ((IPlayerSteeringTarget)_target).ApplySteering(new Vector3(float.NaN, 0f, 1f), Quaternion.identity),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("Steering velocity"));
    }
}
