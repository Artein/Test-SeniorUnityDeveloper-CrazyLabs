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
    public IEnumerator TryCheckBandShapeClearance_AfterHeldPosition_UsesAssignedColliderCurrentPose()
    {
        var pullPoint = new Vector3(0.25f, 1.05f, -1.25f);
        ((ILaunchTarget)_target).Hold();
        ((IHeldLaunchTarget)_target).SetHeldPosition(pullPoint);

        yield return null;

        var clearanceSource = (ILaunchTargetBandShapeClearanceSource)_target;
        var colliderCenter = _collider.bounds.center;

        var blockedShape = new[]
        {
            colliderCenter + Vector3.left,
            colliderCenter + Vector3.right
        };

        var clearShape = new[]
        {
            colliderCenter + Vector3.left + Vector3.forward,
            colliderCenter + Vector3.right + Vector3.forward
        };

        var checkedBlocked = clearanceSource.TryCheckBandShapeClearance(blockedShape, 0.01f, out var blockedIsClear);
        var checkedClear = clearanceSource.TryCheckBandShapeClearance(clearShape, 0.01f, out var clearIsClear);

        Assert.That(checkedBlocked, Is.True);
        Assert.That(blockedIsClear, Is.False);
        Assert.That(checkedClear, Is.True);
        Assert.That(clearIsClear, Is.True);
    }

    [UnityTest]
    public IEnumerator TryCheckBandShapeClearance_ImmediatelyAfterHeldPosition_UsesAssignedColliderCurrentPose()
    {
        var pullPoint = new Vector3(2f, 1.05f, -2f);
        var expectedColliderCenter = pullPoint + (_colliderObject.transform.localPosition - _bandCenterObject.transform.localPosition);
        ((ILaunchTarget)_target).Hold();

        ((IHeldLaunchTarget)_target).SetHeldPosition(pullPoint);

        var clearanceSource = (ILaunchTargetBandShapeClearanceSource)_target;

        var blockedShape = new[]
        {
            expectedColliderCenter + Vector3.left,
            expectedColliderCenter + Vector3.right
        };

        var checkedBlocked = clearanceSource.TryCheckBandShapeClearance(blockedShape, 0.01f, out var blockedIsClear);

        Assert.That(checkedBlocked, Is.True);
        Assert.That(blockedIsClear, Is.False);
        yield break;
    }

    [UnityTest]
    public IEnumerator SetHeldPosition_AfterHold_AlignsAssignedBandCenterToHeldPointAndPreservesRotation()
    {
        var originalRotation = Quaternion.Euler(0f, 35f, 0f);
        _rigidbody.rotation = originalRotation;
        var expectedColliderOffset = originalRotation * (_colliderObject.transform.localPosition - _bandCenterObject.transform.localPosition);
        var pullPoint = new Vector3(0.5f, 1.05f, -1.5f);
        ((ILaunchTarget)_target).Hold();

        ((IHeldLaunchTarget)_target).SetHeldPosition(pullPoint);
        yield return null;

        Assert.That(_rigidbody.rotation.eulerAngles.y, Is.EqualTo(originalRotation.eulerAngles.y).Within(0.001f));
        Assert.That(_bandCenter.position.x, Is.EqualTo(pullPoint.x).Within(0.001f));
        Assert.That(_bandCenter.position.y, Is.EqualTo(pullPoint.y).Within(0.001f));
        Assert.That(_bandCenter.position.z, Is.EqualTo(pullPoint.z).Within(0.001f));
        var colliderOffset = _collider.bounds.center - _bandCenter.position;
        Assert.That(colliderOffset.x, Is.EqualTo(expectedColliderOffset.x).Within(0.001f));
        Assert.That(colliderOffset.y, Is.EqualTo(expectedColliderOffset.y).Within(0.001f));
        Assert.That(colliderOffset.z, Is.EqualTo(expectedColliderOffset.z).Within(0.001f));
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
