using System.Collections.Generic;
using Game.Foundation.Physics;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class TriggerNotifierTests
{
    private readonly List<UnityEngine.Object> _objects = new();

    [TearDown]
    public void OnTearDown()
    {
        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void TriggerEntered_EnteredCollider_PublishesCollider()
    {
        var notifier = CreateGameObject("Notifier").AddComponent<TriggerNotifier>();
        var collider = CreateGameObject("Other").AddComponent<SphereCollider>();
        Collider observedCollider = null;
        notifier.TriggerEntered += enteredCollider => observedCollider = enteredCollider;

        notifier.NotifyTriggerEnteredForTests(collider);

        Assert.That(observedCollider, Is.SameAs(collider));
    }

    [Test]
    public void TriggerEntered_NullCollider_IsIgnored()
    {
        var notifier = CreateGameObject("Notifier").AddComponent<TriggerNotifier>();
        var eventCount = 0;
        notifier.TriggerEntered += _ => eventCount += 1;

        notifier.NotifyTriggerEnteredForTests(null);

        Assert.That(eventCount, Is.Zero);
    }

    private GameObject CreateGameObject(string objectName)
    {
        return Track(new GameObject(objectName));
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }
}
