using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunLaunchLandingStabilizerTests
{
    private FakeConfig _config;
    private RunLaunchLandingStabilizer _stabilizer;

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

        var result = _stabilizer.Stabilize(CreateContext(
            velocity,
            new RunSurfaceContext(true, Vector3.up, 0f),
            0.02f));

        Assert.That(result, Is.EqualTo(velocity));
    }

    [Test]
    public void Stabilize_FirstLandingAfterFlight_ClampsLiftAndPreservesTangentMotion()
    {
        _stabilizer.ArmForLaunch();

        _stabilizer.Stabilize(CreateContext(
            Vector3.forward * 10f + Vector3.up * 3f,
            new RunSurfaceContext(false, Vector3.up, 0f),
            0.02f));

        var result = _stabilizer.Stabilize(CreateContext(
            Vector3.forward * 10f + Vector3.up * 3f,
            new RunSurfaceContext(true, Vector3.up, 0f),
            0.02f));

        Assert.That(result.y, Is.Zero.Within(0.0001f));
        Assert.That(Vector3.ProjectOnPlane(result, Vector3.up).magnitude, Is.EqualTo(10f).Within(0.0001f));
    }

    [Test]
    public void Stabilize_DownwardSurfaceNormalVelocity_RemainsUntouched()
    {
        _stabilizer.ArmForLaunch();

        _stabilizer.Stabilize(CreateContext(
            Vector3.forward,
            new RunSurfaceContext(false, Vector3.up, 0f),
            0.02f));
        var velocity = Vector3.forward * 10f + Vector3.down * 3f;

        var result = _stabilizer.Stabilize(CreateContext(
            velocity,
            new RunSurfaceContext(true, Vector3.up, 0f),
            0.02f));

        Assert.That(result, Is.EqualTo(velocity));
    }

    [Test]
    public void Stabilize_AfterWindowExpires_StopsCorrectingLift()
    {
        _stabilizer.ArmForLaunch();

        _stabilizer.Stabilize(CreateContext(
            Vector3.forward,
            new RunSurfaceContext(false, Vector3.up, 0f),
            0.02f));

        _stabilizer.Stabilize(CreateContext(
            Vector3.forward * 10f + Vector3.up * 3f,
            new RunSurfaceContext(true, Vector3.up, 0f),
            0.02f));

        var result = _stabilizer.Stabilize(CreateContext(
            Vector3.forward * 10f + Vector3.up * 3f,
            new RunSurfaceContext(true, Vector3.up, 0f),
            0.31f));

        Assert.That(result.y, Is.EqualTo(3f).Within(0.0001f));
    }

    [Test]
    public void Reset_ArmedStabilizer_ClearsPendingLandingCorrection()
    {
        _stabilizer.ArmForLaunch();

        _stabilizer.Stabilize(CreateContext(
            Vector3.forward,
            new RunSurfaceContext(false, Vector3.up, 0f),
            0.02f));
        _stabilizer.Reset();
        var velocity = Vector3.forward * 10f + Vector3.up * 3f;

        var result = _stabilizer.Stabilize(CreateContext(
            velocity,
            new RunSurfaceContext(true, Vector3.up, 0f),
            0.02f));

        Assert.That(result, Is.EqualTo(velocity));
    }

    private RunLaunchLandingStabilizationContext CreateContext(
        Vector3 currentVelocity,
        RunSurfaceContext surfaceContext,
        float fixedDeltaTime)
    {
        return new RunLaunchLandingStabilizationContext(currentVelocity, surfaceContext, fixedDeltaTime);
    }

    private sealed class FakeConfig : IRunLaunchLandingStabilizationConfig
    {
        public float LaunchLandingStabilizationSeconds { get; set; }
        public float LaunchLandingMaximumLiftSpeed { get; set; }
    }
}
