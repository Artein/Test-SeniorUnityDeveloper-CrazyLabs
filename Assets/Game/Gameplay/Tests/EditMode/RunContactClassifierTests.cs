using System.Collections.Generic;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunContactClassifierTests
{
    private readonly List<Object> _objects = new();
    private RunContactClassifier _classifier;

    [SetUp]
    public void OnSetUp()
    {
        _classifier = new RunContactClassifier(new FakeRunEndConfig
        {
            ObstacleImpactSpeedThreshold = 5f
        });
    }

    [TearDown]
    public void OnTearDown()
    {
        foreach (var unityObject in _objects)
        {
            Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void TryClassify_Surface_EmitsNoCandidate()
    {
        var collider = CreateCollider("Surface", RunContactCategory.Surface);

        var classified = _classifier.TryClassify(CreateTrigger(collider), out _);

        Assert.That(classified, Is.False);
    }

    [Test]
    public void TryClassify_FinishTrigger_EmitsFinished()
    {
        var collider = CreateCollider("Finish", RunContactCategory.Finish);

        var classified = _classifier.TryClassify(CreateTrigger(collider), out var candidate);

        Assert.That(classified, Is.True);
        Assert.That(candidate.Reason, Is.EqualTo(RunEndReason.Finished));
    }

    [Test]
    public void TryClassify_SafetyNetTrigger_EmitsOutOfBounds()
    {
        var collider = CreateCollider("Safety Net", RunContactCategory.SafetyNet);

        var classified = _classifier.TryClassify(CreateTrigger(collider), out var candidate);

        Assert.That(classified, Is.True);
        Assert.That(candidate.Reason, Is.EqualTo(RunEndReason.OutOfBounds));
    }

    [Test]
    public void TryClassify_ObstacleBelowNormalImpactThreshold_EmitsNoCandidate()
    {
        var collider = CreateCollider("Obstacle", RunContactCategory.Obstacle);
        var notification = CreateCollision(collider, new Vector3(0f, 0f, -4.9f), Vector3.forward);

        var classified = _classifier.TryClassify(notification, out _);

        Assert.That(classified, Is.False);
    }

    [Test]
    public void TryClassify_ObstacleAtNormalImpactThreshold_EmitsObstacleHit()
    {
        var collider = CreateCollider("Obstacle", RunContactCategory.Obstacle);
        var notification = CreateCollision(collider, new Vector3(0f, 0f, -5f), Vector3.forward);

        var classified = _classifier.TryClassify(notification, out var candidate);

        Assert.That(classified, Is.True);
        Assert.That(candidate.Reason, Is.EqualTo(RunEndReason.ObstacleHit));
    }

    [Test]
    public void TryClassify_MetadataOnParentOnly_IsIgnored()
    {
        var parent = Track(new GameObject("Parent Contact"));
        parent.AddComponent<RunContact>().SetCategoryForTests(RunContactCategory.Finish);
        var child = Track(new GameObject("Child Collider"));
        child.transform.SetParent(parent.transform);
        var collider = child.AddComponent<BoxCollider>();

        var classified = _classifier.TryClassify(CreateTrigger(collider), out _);

        Assert.That(classified, Is.False);
    }

    private Collider CreateCollider(string objectName, RunContactCategory category)
    {
        var gameObject = Track(new GameObject(objectName));
        var collider = gameObject.AddComponent<BoxCollider>();
        var contact = gameObject.AddComponent<RunContact>();
        contact.SetCategoryForTests(category);

        return collider;
    }

    private RigidbodyTriggerNotification CreateTrigger(Collider collider)
    {
        return new RigidbodyTriggerNotification(collider);
    }

    private RigidbodyCollisionNotification CreateCollision(
        Collider collider,
        Vector3 relativeVelocity,
        Vector3 normal)
    {
        return new RigidbodyCollisionNotification(
            collider,
            relativeVelocity,
            new[] { new RunContactPoint(Vector3.zero, normal) });
    }

    private T Track<T>(T value)
        where T : Object
    {
        _objects.Add(value);
        return value;
    }

    private sealed class FakeRunEndConfig : IRunEndConfig
    {
        public float ObstacleImpactSpeedThreshold { get; set; }
        public float LostMomentumLaunchGraceDuration { get; set; }
        public float LostMomentumDuration { get; set; }
        public float LostMomentumPlanarSpeedThreshold { get; set; }
        public float LostMomentumProgressThreshold { get; set; }
        public float RunEndedDelay { get; set; }
    }
}
