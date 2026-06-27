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
    private GameObject _bandCenterObject;
    private Rigidbody _rigidbody;
    private SphereCollider _collider;
    private Transform _bandCenter;
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

        _bandCenterObject = new GameObject("Band Center");
        _bandCenterObject.transform.SetParent(_rootObject.transform, false);
        _bandCenterObject.transform.localPosition = Vector3.up + (Vector3.forward * 0.25f);
        _bandCenter = _bandCenterObject.transform;

        _target.SetReferencesForTests(_rigidbody, _collider, _bandCenter);
    }

    [TearDown]
    public void OnTearDown()
    {
        UnityEngine.Object.Destroy(_rootObject);
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
        var colliderCenter = _collider.bounds.center;
        Assert.That(samples.Min(sample => sample.x), Is.LessThan(colliderCenter.x));
        Assert.That(samples.Max(sample => sample.x), Is.GreaterThan(colliderCenter.x));
        Assert.That(samples.Min(sample => sample.z), Is.LessThan(colliderCenter.z));
        Assert.That(samples.Max(sample => sample.z), Is.GreaterThan(colliderCenter.z));

        foreach (var sample in samples)
        {
            Assert.That(sample.y, Is.EqualTo(pullPoint.y).Within(0.0001f));
        }
    }

    [UnityTest]
    public IEnumerator SetHeldPosition_AfterHold_AlignsAssignedBandCenterToHeldPointAndPreservesRotation()
    {
        var originalRotation = Quaternion.Euler(0f, 35f, 0f);
        _rigidbody.rotation = originalRotation;
        var originalColliderOffset = _collider.bounds.center - _bandCenter.position;
        var pullPoint = new Vector3(0.5f, 1.05f, -1.5f);
        ((ILaunchTarget)_target).Hold();

        ((IHeldLaunchTarget)_target).SetHeldPosition(pullPoint);
        yield return null;

        Assert.That(_rigidbody.rotation.eulerAngles.y, Is.EqualTo(originalRotation.eulerAngles.y).Within(0.001f));
        Assert.That(_bandCenter.position.x, Is.EqualTo(pullPoint.x).Within(0.001f));
        Assert.That(_bandCenter.position.y, Is.EqualTo(pullPoint.y).Within(0.001f));
        Assert.That(_bandCenter.position.z, Is.EqualTo(pullPoint.z).Within(0.001f));
        var colliderOffset = _collider.bounds.center - _bandCenter.position;
        Assert.That(colliderOffset.x, Is.EqualTo(originalColliderOffset.x).Within(0.001f));
        Assert.That(colliderOffset.y, Is.EqualTo(originalColliderOffset.y).Within(0.001f));
        Assert.That(colliderOffset.z, Is.EqualTo(originalColliderOffset.z).Within(0.001f));
    }

    [Test]
    public void SetHeldPosition_BeforeHold_ThrowsInvalidOperationException()
    {
        Assert.That(
            () => ((IHeldLaunchTarget)_target).SetHeldPosition(Vector3.zero),
            Throws.InvalidOperationException);
    }

    [Test]
    public void TryWriteSilhouetteSamples_SkewedLaunchFrame_ThrowsArgumentException()
    {
        var query = new LaunchTargetSilhouetteQuery(
            Vector3.zero,
            Vector3.right,
            CreateSkewedUnitForward(),
            Vector3.up,
            8);
        var samples = new Vector3[8];

        Assert.That(
            () => ((ILaunchTargetSilhouetteSource)_target).TryWriteSilhouetteSamples(query, samples, out _),
            Throws.TypeOf<System.ArgumentException>());
    }

    private Vector3 CreateSkewedUnitForward()
    {
        return new Vector3(0.0002f, 0f, Mathf.Sqrt(1f - (0.0002f * 0.0002f)));
    }
}
