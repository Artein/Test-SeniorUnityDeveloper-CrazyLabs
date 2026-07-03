using System.Collections;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class RigidbodyLaunchTargetTests
{
    private const RigidbodyConstraints PostLaunchStabilizationConstraints =
        RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

    private GameObject _gameObject;
    private Rigidbody _rigidbody;
    private SphereCollider _collider;
    private Transform _bandCenter;
    private RigidbodyLaunchTarget _target;

    [SetUp]
    public void OnSetUp()
    {
        _gameObject = new GameObject("Rigidbody Launch Target Test");
        _rigidbody = _gameObject.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _collider = _gameObject.AddComponent<SphereCollider>();
        var bandCenterObject = new GameObject("Band Center");
        bandCenterObject.transform.SetParent(_gameObject.transform, false);
        bandCenterObject.transform.localPosition = Vector3.forward * 0.25f;
        _bandCenter = bandCenterObject.transform;
        _target = _gameObject.AddComponent<RigidbodyLaunchTarget>();
        _target.SetReferencesForTests(_rigidbody, _collider, _bandCenter);
    }

    [TearDown]
    public void OnTearDown()
    {
        Object.Destroy(_gameObject);
    }

    [Test]
    public void Hold_NotHeld_SavesStateSetsKinematicAndClearsVelocity()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.linearVelocity = new Vector3(3f, 4f, 5f);
        _rigidbody.angularVelocity = new Vector3(1f, 2f, 3f);

        ((ILaunchTarget)_target).Hold();

        Assert.That(_rigidbody.isKinematic, Is.True);
        Assert.That(_rigidbody.constraints, Is.EqualTo(RigidbodyConstraints.FreezeRotationX));
        Assert.That(_rigidbody.interpolation, Is.EqualTo(RigidbodyInterpolation.None));
        Assert.That(_rigidbody.linearVelocity, Is.EqualTo(Vector3.zero));
        Assert.That(_rigidbody.angularVelocity, Is.EqualTo(Vector3.zero));
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void Hold_AlreadyKinematic_DoesNotWriteVelocityToKinematicBody()
    {
        _rigidbody.isKinematic = true;

        ((ILaunchTarget)_target).Hold();

        Assert.That(_rigidbody.isKinematic, Is.True);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator Launch_AfterHold_PreservesSavedConstraintsAddsStabilizationAndAppliesVelocityChange()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        ((ILaunchTarget)_target).Hold();

        ((ILaunchTarget)_target).Launch(new Vector3(1f, 2f, 3f));
        yield return new WaitForFixedUpdate();

        Assert.That(_rigidbody.isKinematic, Is.False);
        Assert.That(_rigidbody.constraints, Is.EqualTo(RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ));
        Assert.That(_rigidbody.interpolation, Is.EqualTo(RigidbodyInterpolation.Interpolate));
        AssertConstraintIsNotSet(RigidbodyConstraints.FreezeRotationY);
        Assert.That(_rigidbody.linearVelocity, Is.EqualTo(new Vector3(1f, 2f, 3f)));
        Assert.That(_rigidbody.angularVelocity, Is.EqualTo(Vector3.zero));
    }

    [UnityTest]
    public IEnumerator Launch_WithoutHold_PreservesCurrentConstraintsAddsStabilizationAndAppliesVelocityChange()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationY;
        _rigidbody.linearVelocity = new Vector3(9f, 9f, 9f);
        _rigidbody.angularVelocity = new Vector3(4f, 5f, 6f);

        ((ILaunchTarget)_target).Launch(new Vector3(2f, 0f, 1f));
        yield return new WaitForFixedUpdate();

        Assert.That(_rigidbody.isKinematic, Is.False);

        Assert.That(_rigidbody.constraints,
            Is.EqualTo(RigidbodyConstraints.FreezeRotationY | PostLaunchStabilizationConstraints));
        Assert.That(_rigidbody.linearVelocity, Is.EqualTo(new Vector3(2f, 0f, 1f)));
        Assert.That(_rigidbody.angularVelocity, Is.EqualTo(Vector3.zero));
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator Hold_CalledTwice_DoesNotOverwriteOriginalSavedState()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezePositionX;
        ((ILaunchTarget)_target).Hold();
        _rigidbody.isKinematic = true;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;

        ((ILaunchTarget)_target).Hold();
        ((ILaunchTarget)_target).Launch(Vector3.forward);
        yield return new WaitForFixedUpdate();

        Assert.That(_rigidbody.isKinematic, Is.False);
        Assert.That(_rigidbody.constraints, Is.EqualTo(RigidbodyConstraints.FreezePositionX | PostLaunchStabilizationConstraints));
        AssertConstraintIsNotSet(RigidbodyConstraints.FreezeRotationY);
    }

    [Test]
    public void ResetToPreLaunchPose_DriftedTarget_RestoresPoseSetsHeldBaselineAndClearsVelocity()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezePositionX;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.linearVelocity = new Vector3(3f, 4f, 5f);
        _rigidbody.angularVelocity = new Vector3(1f, 2f, 3f);
        _rigidbody.position = new Vector3(-2f, 1f, 0.5f);
        _rigidbody.rotation = Quaternion.Euler(15f, -30f, 5f);
        var preLaunchPosition = new Vector3(2f, 3f, 4f);
        var preLaunchRotation = Quaternion.Euler(0f, 35f, 0f);

        ((ILaunchTargetPreLaunchReset)_target).ResetToPreLaunchPose(preLaunchPosition, preLaunchRotation);

        Assert.That(_rigidbody.isKinematic, Is.True);
        Assert.That(_rigidbody.constraints, Is.EqualTo(RigidbodyConstraints.FreezePositionX));
        Assert.That(_rigidbody.interpolation, Is.EqualTo(RigidbodyInterpolation.None));
        Assert.That(_rigidbody.linearVelocity, Is.EqualTo(Vector3.zero));
        Assert.That(_rigidbody.angularVelocity, Is.EqualTo(Vector3.zero));
        Assert.That(_rigidbody.position, Is.EqualTo(preLaunchPosition));
        AssertRotationEquals(preLaunchRotation, _rigidbody.rotation);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void SetHeldPosition_AfterPreLaunchPoseReset_MovesBandCenterAndPreservesResetRotation()
    {
        var preLaunchPosition = new Vector3(2f, 3f, 4f);
        var preLaunchRotation = Quaternion.Euler(0f, 35f, 0f);
        var heldPosition = new Vector3(4f, 3f, 2f);
        ((ILaunchTargetPreLaunchReset)_target).ResetToPreLaunchPose(preLaunchPosition, preLaunchRotation);

        ((IHeldLaunchTarget)_target).SetHeldPosition(heldPosition);

        AssertPositionEquals(heldPosition, _bandCenter.position);
        AssertRotationEquals(preLaunchRotation, _rigidbody.rotation);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void RunEndPoseLock_HoldAndRelease_DoesNotWriteVelocityToKinematicBody()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.linearVelocity = new Vector3(4f, 5f, 6f);
        _rigidbody.angularVelocity = new Vector3(7f, 8f, 9f);
        var runEndPosition = new Vector3(3f, 2f, 1f);

        ((IRunEndPoseLockTarget)_target).HoldRunEndPose(runEndPosition);
        ((IRunEndPoseLockTarget)_target).ReleaseRunEndPose();

        Assert.That(_rigidbody.isKinematic, Is.True);
        Assert.That(_rigidbody.interpolation, Is.EqualTo(RigidbodyInterpolation.None));
        Assert.That(_rigidbody.linearVelocity, Is.EqualTo(Vector3.zero));
        Assert.That(_rigidbody.angularVelocity, Is.EqualTo(Vector3.zero));
        Assert.That(_rigidbody.position, Is.EqualTo(runEndPosition));
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator ResetToPreLaunchPose_AfterLaunch_RestoresOriginalLaunchBaselineForNextLaunch()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        ((ILaunchTarget)_target).Hold();
        ((ILaunchTarget)_target).Launch(Vector3.forward);
        yield return new WaitForFixedUpdate();
        Assert.That(_rigidbody.constraints, Is.EqualTo(RigidbodyConstraints.FreezePositionY | PostLaunchStabilizationConstraints));
        Assert.That(_rigidbody.interpolation, Is.EqualTo(RigidbodyInterpolation.Interpolate));

        ((ILaunchTargetPreLaunchReset)_target).ResetToPreLaunchPose(Vector3.one, Quaternion.Euler(0f, 25f, 0f));
        Assert.That(_rigidbody.interpolation, Is.EqualTo(RigidbodyInterpolation.None));
        ((ILaunchTarget)_target).Launch(new Vector3(2f, 0f, 0f));
        yield return new WaitForFixedUpdate();

        Assert.That(_rigidbody.isKinematic, Is.False);
        Assert.That(_rigidbody.constraints, Is.EqualTo(RigidbodyConstraints.FreezePositionY | PostLaunchStabilizationConstraints));
        Assert.That(_rigidbody.interpolation, Is.EqualTo(RigidbodyInterpolation.Interpolate));
        Assert.That(_rigidbody.linearVelocity, Is.EqualTo(new Vector3(2f, 0f, 0f)));
    }

    private void AssertConstraintIsNotSet(RigidbodyConstraints constraint)
    {
        Assert.That(_rigidbody.constraints & constraint, Is.EqualTo(RigidbodyConstraints.None));
    }

    private void AssertRotationEquals(Quaternion expectedRotation, Quaternion actualRotation)
    {
        Assert.That(Quaternion.Angle(expectedRotation, actualRotation), Is.EqualTo(0f).Within(0.0001f));
    }

    private void AssertPositionEquals(Vector3 expectedPosition, Vector3 actualPosition)
    {
        Assert.That(Vector3.Distance(expectedPosition, actualPosition), Is.EqualTo(0f).Within(0.0001f));
    }
}
