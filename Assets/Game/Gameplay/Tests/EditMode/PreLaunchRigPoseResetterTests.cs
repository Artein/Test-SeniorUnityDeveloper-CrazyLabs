using Game.Gameplay;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class PreLaunchRigPoseResetterTests
{
    private GameObject _slingshotRigObject;
    private GameObject _slingshotRigPoseObject;
    private GameObject _launchTargetPoseObject;

    [TearDown]
    public void OnTearDown()
    {
        Object.DestroyImmediate(_slingshotRigObject);
        Object.DestroyImmediate(_slingshotRigPoseObject);
        Object.DestroyImmediate(_launchTargetPoseObject);
    }

    [Test]
    public void ResetToPreLaunchRigPose_DriftedRig_RestoresSlingshotRigAndLaunchTargetPose()
    {
        _slingshotRigObject = new GameObject("Slingshot Rig");
        _slingshotRigPoseObject = new GameObject("Pre-Launch Slingshot Rig Pose");
        _launchTargetPoseObject = new GameObject("Pre-Launch Launch Target Pose");
        _slingshotRigObject.transform.SetPositionAndRotation(new Vector3(3f, 0f, -2f), Quaternion.Euler(0f, 20f, 0f));
        _slingshotRigPoseObject.transform.SetPositionAndRotation(new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 45f, 0f));
        _launchTargetPoseObject.transform.SetPositionAndRotation(new Vector3(4f, 5f, 6f), Quaternion.Euler(0f, 90f, 0f));
        var launchTarget = new FakeLaunchTargetPreLaunchReset();

        var resetter = new PreLaunchRigPoseResetter(
            _slingshotRigObject.transform,
            _slingshotRigPoseObject.transform,
            launchTarget,
            _launchTargetPoseObject.transform);

        ((IPreLaunchRigPoseResetter)resetter).ResetToPreLaunchRigPose();

        Assert.That(_slingshotRigObject.transform.position, Is.EqualTo(_slingshotRigPoseObject.transform.position));
        AssertRotationEquals(_slingshotRigPoseObject.transform.rotation, _slingshotRigObject.transform.rotation);
        Assert.That(launchTarget.ResetCallCount, Is.EqualTo(1));
        Assert.That(launchTarget.ResetPosition, Is.EqualTo(_launchTargetPoseObject.transform.position));
        AssertRotationEquals(_launchTargetPoseObject.transform.rotation, launchTarget.ResetRotation);
    }

    private void AssertRotationEquals(Quaternion expectedRotation, Quaternion actualRotation)
    {
        Assert.That(Quaternion.Angle(expectedRotation, actualRotation), Is.EqualTo(0f).Within(0.0001f));
    }

    private sealed class FakeLaunchTargetPreLaunchReset : ILaunchTargetPreLaunchReset
    {
        public int ResetCallCount { get; private set; }
        public Vector3 ResetPosition { get; private set; }
        public Quaternion ResetRotation { get; private set; }

        public void ResetToPreLaunchPose(Vector3 position, Quaternion rotation)
        {
            ResetCallCount += 1;
            ResetPosition = position;
            ResetRotation = rotation;
        }
    }
}
