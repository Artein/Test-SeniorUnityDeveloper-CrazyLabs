using System.Collections.Generic;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunSurfaceContextSourceTests
{
    // TODO - AI Note: Use editor asset provider with serialized values instead of hardcoding
    private readonly int _surfaceLayer = 8;

    private readonly List<GameObject> _createdObjects = new();

    [TearDown]
    public void OnTearDown()
    {
        for (var index = 0; index < _createdObjects.Count; index += 1)
        {
            if (_createdObjects[index] != null)
                Object.DestroyImmediate(_createdObjects[index]);
        }

        _createdObjects.Clear();
    }

    [Test]
    public void given_SurfaceBelowSupportButOutsideProbeDistance_when_Sampled_then_ContextIsUngrounded()
    {
        var source = CreateSource(1.25f, 0.08f);
        CreateSurface(0.05f, Quaternion.identity);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.False);
    }

    [Test]
    public void given_SurfaceWithinSupportProbeDistance_when_Sampled_then_ContextIsGrounded()
    {
        var source = CreateSource(1.1f, 0.08f);
        CreateSurface(0.05f, Quaternion.identity);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        Assert.That(source.Current.GroundNormal, Is.EqualTo(Vector3.up));
        Assert.That(source.Current.ForwardDownhillDegrees, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void given_GroundedForwardDownhillSurface_when_Sampled_then_ContextUsesSupportSlope()
    {
        var source = CreateSource(1.1f, 0.2f);
        CreateSurface(0.05f, Quaternion.AngleAxis(12f, Vector3.right));
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        Assert.That(source.Current.ForwardDownhillDegrees, Is.EqualTo(12f).Within(0.75f));
    }

    [Test]
    public void given_MissingReferences_when_Sampled_then_ContextIsUngrounded()
    {
        var source = CreateGameObject("Run Surface Context Source").AddComponent<PhysicsRunSurfaceContextSource>();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.False);
    }

    private PhysicsRunSurfaceContextSource CreateSource(float supportCenterY, float supportProbeDistance)
    {
        var sourceObject = CreateGameObject("Run Surface Context Source");
        sourceObject.AddComponent<RunProgressFrameSource>();

        var source = sourceObject.AddComponent<PhysicsRunSurfaceContextSource>();
        var supportCollider = CreateSupportCollider(sourceObject.transform, supportCenterY);
        source.SetReferencesForTests(supportCollider, sourceObject.GetComponent<RunProgressFrameSource>(), supportProbeDistance, 1 << _surfaceLayer);
        return source;
    }

    private Collider CreateSupportCollider(Transform parent, float supportCenterY)
    {
        var supportObject = CreateGameObject("Support Collider");
        supportObject.transform.SetParent(parent, false);
        supportObject.transform.localPosition = new Vector3(0f, supportCenterY, 0f);

        var supportCollider = supportObject.AddComponent<CapsuleCollider>();
        supportCollider.direction = 1;
        supportCollider.radius = 0.15f;
        supportCollider.height = 2f;
        return supportCollider;
    }

    private GameObject CreateSurface(float topY, Quaternion rotation)
    {
        var surface = CreateGameObject("Surface");
        surface.layer = _surfaceLayer;
        surface.transform.SetPositionAndRotation(new Vector3(0f, topY, 0f) - (rotation * Vector3.up * 0.05f), rotation);
        surface.transform.localScale = new Vector3(8f, 0.1f, 8f);
        surface.AddComponent<BoxCollider>();
        return surface;
    }

    private GameObject CreateGameObject(string name)
    {
        var gameObject = new GameObject(name);
        _createdObjects.Add(gameObject);
        return gameObject;
    }
}
