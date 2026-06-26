using System.Collections;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class SlingshotBandContactProviderTests
{
    private GameObject _rootObject;
    private GameObject _colliderObject;
    private Rigidbody _rigidbody;
    private SphereCollider _collider;
    private RigidbodyLaunchTarget _target;

    [SetUp]
    public void OnSetUp()
    {
        _rootObject = new GameObject("Band Contact Target");
        _rigidbody = _rootObject.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _target = _rootObject.AddComponent<RigidbodyLaunchTarget>();

        _colliderObject = new GameObject("Explicit Band Collider");
        _colliderObject.transform.SetParent(_rootObject.transform, false);
        _colliderObject.transform.localPosition = Vector3.up;
        _collider = _colliderObject.AddComponent<SphereCollider>();
        _collider.radius = 0.5f;

        _target.SetReferencesForTests(_rigidbody, _collider);
    }

    [TearDown]
    public void OnTearDown()
    {
        Object.Destroy(_rootObject);
    }

    [UnityTest]
    public IEnumerator CreateBandContactShape_AfterHeldPosition_UsesAssignedColliderCurrentPoseAndProjectsToBandPlane()
    {
        var pullPoint = new Vector3(0.25f, 1.05f, -1.25f);
        ((ILaunchTarget)_target).Hold();
        ((IHeldLaunchTarget)_target).SetHeldPosition(pullPoint);

        yield return null;

        var query = new SlingshotBandContactQuery(
            new Vector3(-1.35f, 1.05f, -0.25f),
            new Vector3(1.35f, 1.05f, -0.25f),
            pullPoint,
            Vector3.right,
            Vector3.forward,
            Vector3.up,
            0.05f,
            6);

        var shape = ((ISlingshotBandContactProvider)_target).CreateBandContactShape(query);

        Assert.That(shape.LeftContactPoint.y, Is.EqualTo(pullPoint.y).Within(0.0001f));
        Assert.That(shape.RightContactPoint.y, Is.EqualTo(pullPoint.y).Within(0.0001f));
        Assert.That(shape.WrapPoints.Count, Is.EqualTo(6));
        Assert.That(shape.LeftContactPoint.x, Is.LessThan(pullPoint.x));
        Assert.That(shape.RightContactPoint.x, Is.GreaterThan(pullPoint.x));

        foreach (var wrapPoint in shape.WrapPoints)
        {
            Assert.That(wrapPoint.y, Is.EqualTo(pullPoint.y).Within(0.0001f));
            Assert.That(wrapPoint.z, Is.LessThanOrEqualTo(pullPoint.z + 0.1f));
        }
    }

    [UnityTest]
    public IEnumerator SetHeldPosition_AfterHold_AlignsAssignedColliderCenterToHeldPointAndPreservesRotation()
    {
        var originalRotation = Quaternion.Euler(0f, 35f, 0f);
        _rigidbody.rotation = originalRotation;
        var pullPoint = new Vector3(0.5f, 1.05f, -1.5f);
        ((ILaunchTarget)_target).Hold();

        ((IHeldLaunchTarget)_target).SetHeldPosition(pullPoint);
        yield return null;

        Assert.That(_rigidbody.rotation.eulerAngles.y, Is.EqualTo(originalRotation.eulerAngles.y).Within(0.001f));
        Assert.That(_collider.bounds.center.x, Is.EqualTo(pullPoint.x).Within(0.001f));
        Assert.That(_collider.bounds.center.z, Is.EqualTo(pullPoint.z).Within(0.001f));
    }

    [Test]
    public void SetHeldPosition_BeforeHold_ThrowsInvalidOperationException()
    {
        Assert.That(
            () => ((IHeldLaunchTarget)_target).SetHeldPosition(Vector3.zero),
            Throws.InvalidOperationException);
    }
}
