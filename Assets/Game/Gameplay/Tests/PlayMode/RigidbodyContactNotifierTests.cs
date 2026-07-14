using System.Collections;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class RigidbodyContactNotifierTests
{
    private RigidbodyContactNotifier _notifier;
    private GameObject _otherObject;
    private GameObject _targetObject;
    private Rigidbody _targetRigidbody;

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
        CreateTarget(new Vector3(x: 0f, y: 10f, z: 0f));
        var otherCollider = CreateOtherBox(objectName: "Trigger", new Vector3(x: 0f, y: 10f, z: 0f), isTrigger: true);
        RigidbodyTriggerNotification notification = null;
        _notifier.TriggerEntered += value => notification = value;

        yield return new WaitForFixedUpdate();

        Assert.That(notification, Is.Not.Null);
        Assert.That(notification.OtherCollider, Is.SameAs(otherCollider));
    }

    [UnityTest]
    public IEnumerator OnCollisionEnter_RaisesCopiedCollisionNotification()
    {
        CreateTarget(new Vector3(x: -1.5f, y: 10f, z: 0f));
        var otherCollider = CreateOtherBox(objectName: "Obstacle", new Vector3(x: 0f, y: 10f, z: 0f), isTrigger: false);
        RigidbodyCollisionNotification notification = null;
        _notifier.CollisionEntered += value => notification = value;

        _targetRigidbody.linearVelocity = new Vector3(x: 8f, y: 0f, z: 0f);

        for (var frame = 0; frame < 20 && notification == null; frame += 1)
        {
            yield return new WaitForFixedUpdate();
        }

        Assert.That(notification, Is.Not.Null);
        Assert.That(notification.OtherCollider, Is.SameAs(otherCollider));
        Assert.That(notification.RelativeVelocity.sqrMagnitude, Is.GreaterThan(expected: 0f));
        Assert.That(notification.ContactCount, Is.GreaterThan(expected: 0));
    }

    [UnityTest]
    public IEnumerator OnCollisionEnter_HighSpeedContinuousDynamicTargetHitsThinObstacle_RaisesCollisionNotification()
    {
        CreateTarget(new Vector3(x: -4f, y: 20f, z: 0f));
        var otherCollider = CreateOtherBox(objectName: "Thin Obstacle", new Vector3(x: 0f, y: 20f, z: 0f), isTrigger: false);
        _otherObject.transform.localScale = new Vector3(x: 0.05f, y: 4f, z: 4f);
        RigidbodyCollisionNotification notification = null;
        _notifier.CollisionEntered += value => notification = value;

        _targetRigidbody.linearVelocity = new Vector3(x: 60f, y: 0f, z: 0f);

        for (var frame = 0; frame < 20 && notification == null; frame += 1)
        {
            yield return new WaitForFixedUpdate();
        }

        Assert.That(notification, Is.Not.Null);
        Assert.That(notification.OtherCollider, Is.SameAs(otherCollider));
    }

    [UnityTest]
    public IEnumerator OnCollisionEnter_AdversarialContinuousDynamicStaticObstacle_ReportsApproachVelocity()
    {
        CreateTarget(new Vector3(x: -0.4f, y: 30f, z: 0f));
        _targetObject.GetComponent<SphereCollider>().radius = 0.35f;
        var otherCollider = CreateOtherBox(objectName: "Adversarial Thin Obstacle", new Vector3(x: 0f, y: 30f, z: 0f), isTrigger: false);
        _otherObject.transform.localScale = new Vector3(x: 0.05f, y: 4f, z: 4f);
        Physics.SyncTransforms();
        RigidbodyCollisionNotification notification = null;
        _notifier.CollisionEntered += value => notification = value;

        _targetRigidbody.linearVelocity = new Vector3(x: 40f, y: 0f, z: 0f);

        for (var frame = 0; frame < 6 && notification == null; frame += 1)
        {
            yield return new WaitForFixedUpdate();
        }

        Assert.That(notification, Is.Not.Null);
        Assert.That(notification.OtherCollider, Is.SameAs(otherCollider));
        Assert.That(Mathf.Abs(notification.RelativeVelocity.x), Is.EqualTo(expected: 40f).Within(amount: 0.1f));
    }

    private void CreateTarget(Vector3 position)
    {
        _targetObject = new GameObject(name: "Contact Notifier Target")
        {
            transform =
            {
                position = position
            }
        };

        _targetRigidbody = _targetObject.AddComponent<Rigidbody>();
        _targetRigidbody.useGravity = false;
        _targetRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _targetRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _targetObject.AddComponent<SphereCollider>();
        _notifier = _targetObject.AddComponent<RigidbodyContactNotifier>();
    }

    private Collider CreateOtherBox(string objectName, Vector3 position, bool isTrigger)
    {
        _otherObject = new GameObject(objectName)
        {
            transform =
            {
                position = position
            }
        };

        var collider = _otherObject.AddComponent<BoxCollider>();
        collider.isTrigger = isTrigger;
        return collider;
    }
}
