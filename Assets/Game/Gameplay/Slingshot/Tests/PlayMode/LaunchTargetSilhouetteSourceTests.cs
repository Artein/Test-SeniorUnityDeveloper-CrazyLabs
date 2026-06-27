using System.Collections;
using System.Linq;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class LaunchTargetSilhouetteSourceTests
{
    private GameObject _rootObject;
    private GameObject _colliderObject;
    private Rigidbody _rigidbody;
    private SphereCollider _collider;
    private RigidbodyLaunchTarget _target;

    [SetUp]
    public void OnSetUp()
    {
        _rootObject = new GameObject("Band Silhouette Target");
        _rigidbody = _rootObject.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _target = _rootObject.AddComponent<RigidbodyLaunchTarget>();

        _colliderObject = new GameObject("Explicit Silhouette Collider");
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
    public IEnumerator TryWriteSilhouetteSamples_AfterHeldPosition_UsesAssignedColliderCurrentPoseAndProjectsToBandPlane()
    {
        var pullPoint = new Vector3(0.25f, 1.05f, -1.25f);
        ((ILaunchTarget)_target).Hold();
        ((IHeldLaunchTarget)_target).SetHeldPosition(pullPoint);

        yield return null;

        var query = new LaunchTargetSilhouetteQuery(
            pullPoint,
            Vector3.right,
            Vector3.forward,
            Vector3.up,
            8);
        var samples = new Vector3[8];

        var solved = ((ILaunchTargetSilhouetteSource)_target).TryWriteSilhouetteSamples(query, samples, out var sampleCount);

        Assert.That(solved, Is.True);
        Assert.That(sampleCount, Is.EqualTo(8));
        Assert.That(samples.Min(sample => sample.x), Is.LessThan(pullPoint.x));
        Assert.That(samples.Max(sample => sample.x), Is.GreaterThan(pullPoint.x));
        Assert.That(samples.Min(sample => sample.z), Is.LessThan(pullPoint.z));
        Assert.That(samples.Max(sample => sample.z), Is.GreaterThan(pullPoint.z));

        foreach (var sample in samples)
        {
            Assert.That(sample.y, Is.EqualTo(pullPoint.y).Within(0.0001f));
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
