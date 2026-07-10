using System;
using System.Collections;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class RigidbodyRunBodyMovementTargetTests
{
    private GameObject _gameObject;
    private Rigidbody _rigidbody;
    private RigidbodyRunBodyMovementTarget _target;

    [SetUp]
    public void OnSetUp()
    {
        _gameObject = new GameObject("Rigidbody Run Body Movement Target Test");
        _rigidbody = _gameObject.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _target = _gameObject.AddComponent<RigidbodyRunBodyMovementTarget>();
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

        Assert.That(((IRunBodyMovementTarget)_target).LinearVelocity, Is.EqualTo(velocity));
    }

    [Test]
    public void ApplyTargetState_FiniteVelocityWithoutRotation_SetsLinearVelocity()
    {
        var velocity = new Vector3(2f, -1f, 4f);

        ((IRunBodyMovementTarget)_target).ApplyTargetState(
            new RunBodyMovementTargetState(velocity, false, Quaternion.identity));

        Assert.That(_rigidbody.linearVelocity.x, Is.EqualTo(velocity.x).Within(0.0001f));
        Assert.That(_rigidbody.linearVelocity.y, Is.EqualTo(velocity.y).Within(0.0001f));
        Assert.That(_rigidbody.linearVelocity.z, Is.EqualTo(velocity.z).Within(0.0001f));
    }

    [Test]
    public void ApplyTargetState_NonFiniteVelocity_Throws()
    {
        Assert.That(
            () => ((IRunBodyMovementTarget)_target).ApplyTargetState(
                new RunBodyMovementTargetState(new Vector3(float.NaN, 0f, 1f), false, Quaternion.identity)),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("Run Body movement velocity"));
    }

    [UnityTest]
    public IEnumerator ApplyTargetState_FiniteVelocityAndRotation_SetsLinearVelocityAndMovesRotation()
    {
        var velocity = new Vector3(3f, 0.5f, 7f);
        var rotation = Quaternion.Euler(0f, 45f, 0f);

        ((IRunBodyMovementTarget)_target).ApplyTargetState(new RunBodyMovementTargetState(velocity, true, rotation));
        yield return new WaitForFixedUpdate();

        Assert.That(_rigidbody.linearVelocity.x, Is.EqualTo(velocity.x).Within(0.0001f));
        Assert.That(_rigidbody.linearVelocity.y, Is.EqualTo(velocity.y).Within(0.0001f));
        Assert.That(_rigidbody.linearVelocity.z, Is.EqualTo(velocity.z).Within(0.0001f));
        Assert.That(Quaternion.Angle(_rigidbody.rotation, rotation), Is.LessThan(0.1f));
    }

    [Test]
    public void ApplyTargetState_NonFiniteVelocityWithRotation_Throws()
    {
        Assert.That(
            () => ((IRunBodyMovementTarget)_target).ApplyTargetState(
                new RunBodyMovementTargetState(new Vector3(float.NaN, 0f, 1f), true, Quaternion.identity)),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("Run Body movement velocity"));
    }
}
