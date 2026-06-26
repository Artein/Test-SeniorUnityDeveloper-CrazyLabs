using System.Collections;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class RigidbodyLaunchTargetTests
{
    private GameObject _gameObject;
    private Rigidbody _rigidbody;
    private SphereCollider _collider;
    private RigidbodyLaunchTarget _target;

    [SetUp]
    public void OnSetUp()
    {
        _gameObject = new GameObject("Rigidbody Launch Target Test");
        _rigidbody = _gameObject.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _collider = _gameObject.AddComponent<SphereCollider>();
        _target = _gameObject.AddComponent<RigidbodyLaunchTarget>();
        _target.SetReferencesForTests(_rigidbody, _collider);
    }

    [TearDown]
    public void OnTearDown()
    {
        Object.Destroy(_gameObject);
    }

    [Test]
    public void Hold_NotHeld_SavesStateSetsKinematicAndClearsVelocity()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX;
        _rigidbody.linearVelocity = new Vector3(3f, 4f, 5f);
        _rigidbody.angularVelocity = new Vector3(1f, 2f, 3f);

        ((ILaunchTarget)_target).Hold();

        Assert.That(_rigidbody.isKinematic, Is.True);
        Assert.That(_rigidbody.constraints, Is.EqualTo(RigidbodyConstraints.FreezeRotationX));
        Assert.That(_rigidbody.linearVelocity, Is.EqualTo(Vector3.zero));
        Assert.That(_rigidbody.angularVelocity, Is.EqualTo(Vector3.zero));
    }

    [UnityTest]
    public IEnumerator Launch_AfterHold_RestoresSavedStateAndAppliesVelocityChange()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX;
        ((ILaunchTarget)_target).Hold();

        ((ILaunchTarget)_target).Launch(new Vector3(1f, 2f, 3f));
        yield return new WaitForFixedUpdate();

        Assert.That(_rigidbody.isKinematic, Is.False);
        Assert.That(_rigidbody.constraints, Is.EqualTo(RigidbodyConstraints.FreezeRotationX));
        Assert.That(_rigidbody.linearVelocity, Is.EqualTo(new Vector3(1f, 2f, 3f)));
        Assert.That(_rigidbody.angularVelocity, Is.EqualTo(Vector3.zero));
    }

    [UnityTest]
    public IEnumerator Launch_WithoutHold_ClearsStaleVelocityAndAppliesVelocityChange()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationY;
        _rigidbody.linearVelocity = new Vector3(9f, 9f, 9f);
        _rigidbody.angularVelocity = new Vector3(4f, 5f, 6f);

        ((ILaunchTarget)_target).Launch(new Vector3(2f, 0f, 1f));
        yield return new WaitForFixedUpdate();

        Assert.That(_rigidbody.isKinematic, Is.False);
        Assert.That(_rigidbody.constraints, Is.EqualTo(RigidbodyConstraints.FreezeRotationY));
        Assert.That(_rigidbody.linearVelocity, Is.EqualTo(new Vector3(2f, 0f, 1f)));
        Assert.That(_rigidbody.angularVelocity, Is.EqualTo(Vector3.zero));
    }

    [UnityTest]
    public IEnumerator Hold_CalledTwice_DoesNotOverwriteOriginalSavedState()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezePositionX;
        ((ILaunchTarget)_target).Hold();
        _rigidbody.isKinematic = true;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;

        ((ILaunchTarget)_target).Hold();
        ((ILaunchTarget)_target).Launch(Vector3.forward);
        yield return new WaitForFixedUpdate();

        Assert.That(_rigidbody.isKinematic, Is.False);
        Assert.That(_rigidbody.constraints, Is.EqualTo(RigidbodyConstraints.FreezePositionX));
    }
}
