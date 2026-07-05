using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunSupportColliderProbeFactoryTests
{
    private GameObject _colliderObject;
    private RunSupportColliderProbeFactory _factory;

    [SetUp]
    public void OnSetUp()
    {
        _factory = new RunSupportColliderProbeFactory();
    }

    [TearDown]
    public void OnTearDown()
    {
        if (_colliderObject != null)
            Object.DestroyImmediate(_colliderObject);
    }

    [Test]
    public void Create_CapsuleCollider_ReturnsCapsuleRunSupportColliderProbe()
    {
        AssertCreatedProbe<CapsuleRunSupportColliderProbe, CapsuleCollider>();
    }

    [Test]
    public void Create_SphereCollider_ReturnsSphereRunSupportColliderProbe()
    {
        AssertCreatedProbe<SphereRunSupportColliderProbe, SphereCollider>();
    }

    [Test]
    public void Create_BoxCollider_ReturnsBoxRunSupportColliderProbe()
    {
        AssertCreatedProbe<BoxRunSupportColliderProbe, BoxCollider>();
    }

    [Test]
    public void Create_UnsupportedCollider_ReturnsBoundsRunSupportColliderProbe()
    {
        AssertCreatedProbe<BoundsRunSupportColliderProbe, MeshCollider>();
    }

    [Test]
    public void Create_MissingCollider_ThrowsArgumentNullException()
    {
        Assert.That(
            () => _factory.Create(null),
            Throws.ArgumentNullException.With.Property("ParamName").EqualTo("collider"));
    }

    private void AssertCreatedProbe<TProbe, TCollider>()
        where TProbe : IRunSupportColliderProbe
        where TCollider : Collider
    {
        var collider = CreateCollider<TCollider>();

        var probe = _factory.Create(collider);

        Assert.That(probe, Is.TypeOf<TProbe>());
        Assert.That(probe.Collider, Is.SameAs(collider));
    }

    private TCollider CreateCollider<TCollider>()
        where TCollider : Collider
    {
        _colliderObject = new GameObject(typeof(TCollider).Name);
        return _colliderObject.AddComponent<TCollider>();
    }
}
