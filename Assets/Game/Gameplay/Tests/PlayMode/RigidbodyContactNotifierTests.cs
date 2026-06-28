using System.Collections;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class RigidbodyContactNotifierTests
{
    private GameObject _targetObject;
    private GameObject _otherObject;
    private Rigidbody _targetRigidbody;
    private RigidbodyContactNotifier _notifier;

    [TearDown]
    public void OnTearDown()
    {
        if (_targetObject != null)
            Object.DestroyImmediate(_targetObject);

        if (_otherObject != null)
            Object.DestroyImmediate(_otherObject);
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter_RaisesCopiedTriggerNotification()
    {
        CreateTarget(new Vector3(0f, 10f, 0f));
        var otherCollider = CreateOtherBox("Trigger", new Vector3(0f, 10f, 0f), true);
        RigidbodyTriggerNotification notification = null;
        _notifier.TriggerEntered += value => notification = value;

        yield return new WaitForFixedUpdate();

        Assert.That(notification, Is.Not.Null);
        Assert.That(notification.OtherCollider, Is.SameAs(otherCollider));
    }

    [UnityTest]
    public IEnumerator OnCollisionEnter_RaisesCopiedCollisionNotification()
    {
        CreateTarget(new Vector3(-1.5f, 10f, 0f));
        var otherCollider = CreateOtherBox("Obstacle", new Vector3(0f, 10f, 0f), false);
        RigidbodyCollisionNotification notification = null;
        _notifier.CollisionEntered += value => notification = value;

        _targetRigidbody.linearVelocity = new Vector3(8f, 0f, 0f);

        for (var frame = 0; frame < 20 && notification == null; frame += 1)
        {
            yield return new WaitForFixedUpdate();
        }

        Assert.That(notification, Is.Not.Null);
        Assert.That(notification.OtherCollider, Is.SameAs(otherCollider));
        Assert.That(notification.RelativeVelocity.sqrMagnitude, Is.GreaterThan(0f));
        Assert.That(notification.ContactCount, Is.GreaterThan(0));
    }

    private void CreateTarget(Vector3 position)
    {
        _targetObject = new GameObject("Contact Notifier Target");
        _targetObject.transform.position = position;
        _targetRigidbody = _targetObject.AddComponent<Rigidbody>();
        _targetRigidbody.useGravity = false;
        _targetRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _targetObject.AddComponent<SphereCollider>();
        _notifier = _targetObject.AddComponent<RigidbodyContactNotifier>();
    }

    private Collider CreateOtherBox(string objectName, Vector3 position, bool isTrigger)
    {
        _otherObject = new GameObject(objectName);
        _otherObject.transform.position = position;
        var collider = _otherObject.AddComponent<BoxCollider>();
        collider.isTrigger = isTrigger;
        return collider;
    }
}
