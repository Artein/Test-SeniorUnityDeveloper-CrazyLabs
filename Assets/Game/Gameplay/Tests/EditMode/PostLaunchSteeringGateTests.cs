using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class PostLaunchSteeringGateTests
{
    private PostLaunchSteeringGate _gate;

    [SetUp]
    public void OnSetUp()
    {
        _gate = new PostLaunchSteeringGate();
    }

    [Test]
    public void ShouldBlockSteering_ArmedAndUnsupported_BlocksSteering()
    {
        _gate.Arm();

        var shouldBlock = _gate.ShouldBlockSteering(Ungrounded(), Vector3.forward * 10f, 0f);

        Assert.That(shouldBlock, Is.True);
    }

    [Test]
    public void ShouldBlockSteering_UnsupportedThenValidGrounded_AllowsSteeringAndClearsGate()
    {
        _gate.Arm();

        _gate.ShouldBlockSteering(Ungrounded(), Vector3.forward * 10f, 0f);
        var shouldBlockLanding = _gate.ShouldBlockSteering(Grounded(Vector3.up), new Vector3(0f, 2f, 10f), 0f);
        var shouldBlockLater = _gate.ShouldBlockSteering(Grounded(Vector3.up), new Vector3(0f, 2f, 10f), 0f);

        Assert.That(shouldBlockLanding, Is.False);
        Assert.That(shouldBlockLater, Is.False);
    }

    [Test]
    public void ShouldBlockSteering_StaleGroundedWithPositiveLift_BlocksSteering()
    {
        _gate.Arm();

        var shouldBlock = _gate.ShouldBlockSteering(Grounded(Vector3.up), new Vector3(0f, 2f, 10f), 0f);

        Assert.That(shouldBlock, Is.True);
    }

    [Test]
    public void ShouldBlockSteering_StaleGroundedWithoutPositiveLift_AllowsSteering()
    {
        _gate.Arm();

        var shouldBlock = _gate.ShouldBlockSteering(Grounded(Vector3.up), new Vector3(0f, 0f, 10f), 0f);

        Assert.That(shouldBlock, Is.False);
    }

    [Test]
    public void ShouldBlockSteering_ArmedAndInvalidGroundedSample_BlocksSteering()
    {
        _gate.Arm();

        var shouldBlock = _gate.ShouldBlockSteering(new RunSurfaceContext(true, Vector3.zero, 0f), Vector3.forward * 10f, 0f);

        Assert.That(shouldBlock, Is.True);
    }

    [Test]
    public void Clear_AfterArm_AllowsSteering()
    {
        _gate.Arm();

        _gate.Clear();

        Assert.That(_gate.ShouldBlockSteering(Grounded(Vector3.up), new Vector3(0f, 2f, 10f), 0f), Is.False);
    }

    private RunSurfaceContext Grounded(Vector3 groundNormal)
    {
        return new RunSurfaceContext(true, groundNormal, 0f);
    }

    private RunSurfaceContext Ungrounded()
    {
        return new RunSurfaceContext(false, Vector3.up, 0f);
    }
}
