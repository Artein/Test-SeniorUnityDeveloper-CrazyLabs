using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunProgressFrameSnapshotTests
{
    [Test]
    public void GetCourseForwardSpeed_ForwardVelocity_ReturnsPositiveSpeed()
    {
        var snapshot = CreateSnapshot(Vector3.forward, Vector3.up);

        var speed = snapshot.GetCourseForwardSpeed(new Vector3(0f, 2f, 7f));

        Assert.That(speed, Is.EqualTo(7f).Within(0.0001f));
    }

    [Test]
    public void GetCourseForwardSpeed_BackwardVelocity_ReturnsNegativeSpeed()
    {
        var snapshot = CreateSnapshot(Vector3.forward, Vector3.up);

        var speed = snapshot.GetCourseForwardSpeed(new Vector3(0f, 2f, -4f));

        Assert.That(speed, Is.EqualTo(-4f).Within(0.0001f));
    }

    [Test]
    public void GetCourseForwardSpeed_LateralVelocity_ReturnsZero()
    {
        var snapshot = CreateSnapshot(Vector3.forward, Vector3.up);

        var speed = snapshot.GetCourseForwardSpeed(new Vector3(3f, 2f, 0f));

        Assert.That(speed, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void GetCourseForwardSpeed_NonFiniteVelocity_ReturnsZero()
    {
        var snapshot = CreateSnapshot(Vector3.forward, Vector3.up);

        var speed = snapshot.GetCourseForwardSpeed(new Vector3(float.NaN, 0f, 2f));

        Assert.That(speed, Is.EqualTo(0f));
    }

    [Test]
    public void GetCourseForwardSpeed_NonUnitForward_UsesNormalizedPlanarForward()
    {
        var snapshot = CreateSnapshot(new Vector3(0f, 1f, 3f), Vector3.up);

        var speed = snapshot.GetCourseForwardSpeed(new Vector3(0f, 4f, 6f));

        Assert.That(speed, Is.EqualTo(6f).Within(0.0001f));
    }

    private RunProgressFrameSnapshot CreateSnapshot(Vector3 forwardDirection, Vector3 upDirection)
    {
        Assert.That(RunProgressFrameSnapshot.TryCreate(Vector3.zero, forwardDirection, upDirection, out var snapshot, out var error), Is.True, error);
        return snapshot;
    }
}
