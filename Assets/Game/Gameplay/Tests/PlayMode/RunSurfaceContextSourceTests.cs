using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Tests.Common;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunSurfaceContextSourceTests : BaseGameplayTestAssetsFixture
{
    private static readonly Vector3 TestOrigin = new(1000f, 0f, 1000f);

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

        ((IFixedTickable)source).FixedTick();

        Assert.That(source.Current.IsGrounded, Is.True);
        Assert.That(source.Current.GroundNormal, Is.EqualTo(Vector3.up));
        Assert.That(source.Current.ForwardDownhillDegrees, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void given_SurfaceExactlyTouchingSupport_when_Sampled_then_ContextIsGrounded()
    {
        var source = CreateSource(1.05f, 0.08f);
        CreateSurface(0.05f, Quaternion.identity);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        Assert.That(source.Current.GroundNormal, Is.EqualTo(Vector3.up));
    }

    [Test]
    public void given_SurfaceSlightlyPenetratingSupport_when_Sampled_then_ContextIsGrounded()
    {
        var source = CreateSource(1.03f, 0.08f);
        CreateSurface(0.05f, Quaternion.identity);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        Assert.That(source.Current.GroundNormal, Is.EqualTo(Vector3.up));
    }

    [Test]
    public void given_GroundedSampleMissesOnce_when_Sampled_then_ContextStaysGrounded()
    {
        var source = CreateSource(1.1f, 0.08f);
        var surface = CreateSurface(0.05f, Quaternion.identity);
        Physics.SyncTransforms();
        source.SampleForTests();
        Assert.That(source.Current.IsGrounded, Is.True);

        surface.transform.position += Vector3.down * 10f;
        Physics.SyncTransforms();
        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
    }

    [Test]
    public void given_GroundedSampleMissesTwice_when_Sampled_then_ContextBecomesUngrounded()
    {
        var source = CreateSource(1.1f, 0.08f);
        var surface = CreateSurface(0.05f, Quaternion.identity);
        Physics.SyncTransforms();
        source.SampleForTests();
        Assert.That(source.Current.IsGrounded, Is.True);

        surface.transform.position += Vector3.down * 10f;
        Physics.SyncTransforms();
        source.SampleForTests();
        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.False);
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
    public void given_ColliderWithoutRunContactWithinProbeDistance_when_Sampled_then_ContextIsUngrounded()
    {
        var source = CreateSource(1.1f, 0.08f);
        CreateSurface(0.05f, Quaternion.identity, RunContactCategory.Surface, false);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.False);
    }

    [TestCase(RunContactCategory.Obstacle)]
    [TestCase(RunContactCategory.SafetyNet)]
    [TestCase(RunContactCategory.Finish)]
    public void given_NonSurfaceRunContactWithinProbeDistance_when_Sampled_then_ContextIsUngrounded(RunContactCategory category)
    {
        var source = CreateSource(1.1f, 0.08f);
        CreateSurface(0.05f, Quaternion.identity, category);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.False);
    }

    [Test]
    public void given_CloserSteepSurfaceAndFartherFlatSurface_when_Sampled_then_ContextUsesMostSupportiveSurfaceNormal()
    {
        var source = CreateSource(1.35f, 0.35f);
        CreateSurface(0.22f, Quaternion.AngleAxis(55f, Vector3.right));
        CreateSurface(0.05f, Quaternion.identity);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        Assert.That(Vector3.Dot(source.Current.GroundNormal, Vector3.up), Is.GreaterThan(0.98f));
        Assert.That(source.Current.ForwardDownhillDegrees, Is.EqualTo(0f).Within(0.75f));
    }

    [Test]
    public void given_MissingSupportCollider_when_Constructed_then_ThrowsArgumentNullException()
    {
        var runProgressFrameSource = CreateGameObject("Run Progress Frame Source").AddComponent<RunProgressFrameSource>();

        Assert.That(
            () => new PhysicsRunSurfaceContextSource(
                null,
                runProgressFrameSource,
                new RunSupportColliderProbeFactory(),
                0.08f,
                TestAssets.RunSurfaceLayerMask),
            Throws.ArgumentNullException.With.Property("ParamName").EqualTo("supportCollider"));
    }

    [Test]
    public void given_MissingRunProgressFrameSource_when_Constructed_then_ThrowsArgumentNullException()
    {
        var supportCollider = CreateSupportCollider(CreateGameObject("Run Surface Context Source").transform, 1.1f);

        Assert.That(
            () => new PhysicsRunSurfaceContextSource(
                supportCollider,
                null,
                new RunSupportColliderProbeFactory(),
                0.08f,
                TestAssets.RunSurfaceLayerMask),
            Throws.ArgumentNullException.With.Property("ParamName").EqualTo("runProgressFrameSource"));
    }

    [Test]
    public void given_MissingRunSupportColliderProbeFactory_when_Constructed_then_ThrowsArgumentNullException()
    {
        var sourceObject = CreateGameObject("Run Surface Context Source");
        var runProgressFrameSource = sourceObject.AddComponent<RunProgressFrameSource>();
        var supportCollider = CreateSupportCollider(sourceObject.transform, 1.1f);

        Assert.That(
            () => new PhysicsRunSurfaceContextSource(
                supportCollider,
                runProgressFrameSource,
                null,
                0.08f,
                TestAssets.RunSurfaceLayerMask),
            Throws.ArgumentNullException.With.Property("ParamName").EqualTo("runSupportColliderProbeFactory"));
    }

    private PhysicsRunSurfaceContextSource CreateSource(float supportCenterY, float supportProbeDistance)
    {
        var sourceObject = CreateGameObject("Run Surface Context Source");
        sourceObject.transform.position = TestOrigin;
        var runProgressFrameSource = sourceObject.AddComponent<RunProgressFrameSource>();

        var supportCollider = CreateSupportCollider(sourceObject.transform, supportCenterY);

        return new PhysicsRunSurfaceContextSource(
            supportCollider,
            runProgressFrameSource,
            new RunSupportColliderProbeFactory(),
            supportProbeDistance,
            TestAssets.RunSurfaceLayerMask);
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

    private GameObject CreateSurface(
        float topY,
        Quaternion rotation,
        RunContactCategory category = RunContactCategory.Surface,
        bool addRunContact = true)
    {
        var surface = CreateGameObject("Surface");
        surface.layer = GetSingleLayer(TestAssets.RunSurfaceLayerMask, "Run Surface");

        surface.transform.SetPositionAndRotation(
            TestOrigin + new Vector3(0f, topY, 0f) - (rotation * Vector3.up * 0.05f),
            rotation);
        surface.transform.localScale = new Vector3(8f, 0.1f, 8f);
        surface.AddComponent<BoxCollider>();

        if (addRunContact)
            surface.AddComponent<RunContact>().SetCategoryForTests(category);

        return surface;
    }

    private int GetSingleLayer(LayerMask layerMask, string description)
    {
        Assert.That(layerMask.value, Is.Not.EqualTo(0), description);
        Assert.That(layerMask.value & (layerMask.value - 1), Is.Zero, description);
        return Mathf.RoundToInt(Mathf.Log(layerMask.value, 2f));
    }

    private GameObject CreateGameObject(string name)
    {
        var gameObject = new GameObject(name);
        _createdObjects.Add(gameObject);
        return gameObject;
    }
}
