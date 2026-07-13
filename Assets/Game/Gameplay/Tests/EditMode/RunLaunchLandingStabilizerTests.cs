using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunLaunchLandingStabilizerTests
{
    private FakeConfig _config;
    private IRunLaunchLandingStabilizer _stabilizer;

    [SetUp]
    public void OnSetUp()
    {
        _config = new FakeConfig
        {
            LaunchLandingStabilizationSeconds = 0.3f,
            LaunchLandingMaximumLiftSpeed = 0f
        };

        _stabilizer = new RunLaunchLandingStabilizer(_config);
    }

    [Test]
    public void Stabilize_ArmedWithoutObservedFlight_DoesNotAlterStaleGroundedVelocity()
    {
        _stabilizer.ArmForLaunch();
        var velocity = Vector3.forward * 10f + Vector3.up * 3f;

        var result = _stabilizer.Stabilize(
            CreateContext(
                velocity,
                new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.None,
                fixedDeltaTime: 0.02f));

        Assert.That(result, Is.EqualTo(velocity));
    }

    [Test]
    public void Stabilize_FirstLandingAfterFlight_ClampsLiftAndPreservesTangentMotion()
    {
        _stabilizer.ArmForLaunch();

        _stabilizer.Stabilize(
            CreateContext(
                Vector3.forward * 10f + Vector3.up * 3f,
                new RunSurfaceContext(isGrounded: false, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportLost,
                fixedDeltaTime: 0.02f));

        var result = _stabilizer.Stabilize(
            CreateContext(
                Vector3.forward * 10f + Vector3.up * 3f,
                new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportAcquired,
                fixedDeltaTime: 0.02f));

        Assert.That(result.y, Is.Zero.Within(amount: 0.0001f));

        Assert.That(
            Vector3.ProjectOnPlane(result, Vector3.up).magnitude,
            Is.EqualTo(expected: 10f).Within(amount: 0.0001f));
    }

    [Test]
    public void Stabilize_LaunchStartsUnsupported_FirstSupportAcquiredClampsLift()
    {
        _stabilizer.ArmForLaunch();
        var velocity = Vector3.forward * 10f + Vector3.up * 3f;

        var result = _stabilizer.Stabilize(
            CreateContext(
                velocity,
                new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportAcquired,
                fixedDeltaTime: 0.02f));

        Assert.That(result.y, Is.Zero.Within(amount: 0.0001f));

        Assert.That(
            Vector3.ProjectOnPlane(result, Vector3.up).magnitude,
            Is.EqualTo(expected: 10f).Within(amount: 0.0001f));
    }

    [Test]
    public void Stabilize_ArmedSupportReattached_ClampsLiftAndPreservesTangentMotion()
    {
        _stabilizer.ArmForLaunch();
        var velocity = Vector3.forward * 10f + Vector3.up * 0.1f;

        var result = _stabilizer.Stabilize(
            CreateContext(
                velocity,
                new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportReattached,
                fixedDeltaTime: 0.02f));

        Assert.That(result.y, Is.Zero.Within(amount: 0.0001f));

        Assert.That(
            Vector3.ProjectOnPlane(result, Vector3.up).magnitude,
            Is.EqualTo(expected: 10f).Within(amount: 0.0001f));
    }

    [Test]
    public void Stabilize_AfterSupportReattachmentWindowExpires_LaterSupportAcquiredDoesNotReactivate()
    {
        _stabilizer.ArmForLaunch();
        var velocity = Vector3.forward * 10f + Vector3.up * 0.1f;

        _stabilizer.Stabilize(
            CreateContext(
                velocity,
                new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportReattached,
                fixedDeltaTime: 0.02f));

        var expiredResult = _stabilizer.Stabilize(
            CreateContext(
                velocity,
                new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.ContinuousUpdate,
                fixedDeltaTime: 0.31f));

        var laterAcquisitionResult = _stabilizer.Stabilize(
            CreateContext(
                velocity,
                new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportAcquired,
                fixedDeltaTime: 0.02f));

        Assert.That(expiredResult, Is.EqualTo(velocity));
        Assert.That(laterAcquisitionResult, Is.EqualTo(velocity));
    }

    [Test]
    public void Stabilize_DownwardSurfaceNormalVelocity_RemainsUntouched()
    {
        _stabilizer.ArmForLaunch();

        _stabilizer.Stabilize(
            CreateContext(
                Vector3.forward,
                new RunSurfaceContext(isGrounded: false, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportLost,
                fixedDeltaTime: 0.02f));

        var velocity = Vector3.forward * 10f + Vector3.down * 3f;

        var result = _stabilizer.Stabilize(
            CreateContext(
                velocity,
                new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportAcquired,
                fixedDeltaTime: 0.02f));

        Assert.That(result, Is.EqualTo(velocity));
    }

    [Test]
    public void Stabilize_AfterWindowExpires_StopsCorrectingLift()
    {
        _stabilizer.ArmForLaunch();

        _stabilizer.Stabilize(
            CreateContext(
                Vector3.forward,
                new RunSurfaceContext(isGrounded: false, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportLost,
                fixedDeltaTime: 0.02f));

        _stabilizer.Stabilize(
            CreateContext(
                Vector3.forward * 10f + Vector3.up * 3f,
                new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportAcquired,
                fixedDeltaTime: 0.02f));

        var result = _stabilizer.Stabilize(
            CreateContext(
                Vector3.forward * 10f + Vector3.up * 3f,
                new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.ContinuousUpdate,
                fixedDeltaTime: 0.31f));

        Assert.That(result.y, Is.EqualTo(expected: 3f).Within(amount: 0.0001f));
    }

    [Test]
    public void Reset_ArmedStabilizer_ClearsPendingLandingCorrection()
    {
        _stabilizer.ArmForLaunch();

        _stabilizer.Stabilize(
            CreateContext(
                Vector3.forward,
                new RunSurfaceContext(isGrounded: false, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportLost,
                fixedDeltaTime: 0.02f));

        _stabilizer.Reset();
        var velocity = Vector3.forward * 10f + Vector3.up * 3f;

        var result = _stabilizer.Stabilize(
            CreateContext(
                velocity,
                new RunSurfaceContext(isGrounded: true, Vector3.up, forwardDownhillDegrees: 0f),
                RunSurfaceTransition.SupportAcquired,
                fixedDeltaTime: 0.02f));

        Assert.That(result, Is.EqualTo(velocity));
    }

    private RunLaunchLandingStabilizationContext CreateContext(
        Vector3 currentVelocity,
        RunSurfaceContext surfaceContext,
        RunSurfaceTransition surfaceTransition,
        float fixedDeltaTime)
    {
        return new RunLaunchLandingStabilizationContext(
            currentVelocity,
            surfaceContext,
            surfaceTransition,
            fixedDeltaTime);
    }

    private sealed class FakeConfig : IRunLaunchLandingStabilizationConfig
    {
        public float LaunchLandingMaximumLiftSpeed { get; set; }
        public float LaunchLandingStabilizationSeconds { get; set; }
    }
}
