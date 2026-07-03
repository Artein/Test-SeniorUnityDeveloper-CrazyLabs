using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunSteeringModeSelectorTests
{
    private RunSteeringModeSelector _selector;

    [SetUp]
    public void OnSetUp()
    {
        _selector = new RunSteeringModeSelector();
    }

    [Test]
    public void Select_MissingSupport_ReturnsAir()
    {
        var mode = _selector.Select(Ungrounded(), Vector3.forward * 10f, 0f);

        Assert.That(mode, Is.EqualTo(RunSteeringMode.Air));
    }

    [Test]
    public void Select_InvalidGroundedSupport_ReturnsAir()
    {
        var mode = _selector.Select(new RunSurfaceContext(true, Vector3.zero, 0f), Vector3.forward * 10f, 0f);

        Assert.That(mode, Is.EqualTo(RunSteeringMode.Air));
    }

    [Test]
    public void Select_GroundedWithPositiveSurfaceNormalLift_ReturnsAir()
    {
        var mode = _selector.Select(Grounded(Vector3.up), new Vector3(0f, 0.2f, 10f), 0.1f);

        Assert.That(mode, Is.EqualTo(RunSteeringMode.Air));
    }

    [Test]
    public void Select_GroundedAtLiftTolerance_ReturnsGrounded()
    {
        var mode = _selector.Select(Grounded(Vector3.up), new Vector3(0f, 0.1f, 10f), 0.1f);

        Assert.That(mode, Is.EqualTo(RunSteeringMode.Grounded));
    }

    [Test]
    public void Select_GroundedWithTangentVelocity_ReturnsGrounded()
    {
        var mode = _selector.Select(Grounded(Vector3.up), Vector3.forward * 10f, 0f);

        Assert.That(mode, Is.EqualTo(RunSteeringMode.Grounded));
    }

    [Test]
    public void Select_GroundedWithDownwardVelocity_ReturnsGrounded()
    {
        var mode = _selector.Select(Grounded(Vector3.up), new Vector3(0f, -2f, 10f), 0f);

        Assert.That(mode, Is.EqualTo(RunSteeringMode.Grounded));
    }

    [Test]
    public void Select_TiltedGroundedWithPositiveSurfaceNormalLift_ReturnsAir()
    {
        var groundNormal = new Vector3(0f, 1f, 1f).normalized;
        var tangentVelocity = Vector3.ProjectOnPlane(Vector3.forward, groundNormal).normalized * 10f;
        var liftVelocity = groundNormal * 0.2f;

        var mode = _selector.Select(Grounded(groundNormal), tangentVelocity + liftVelocity, 0.1f);

        Assert.That(mode, Is.EqualTo(RunSteeringMode.Air));
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
