using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunSurfaceSlopeCalculatorTests
{
    [Test]
    public void CalculateForwardDownhillDegrees_FlatSurface_ReturnsZero()
    {
        var calculator = new RunSurfaceSlopeCalculator();
        var snapshot = CreateSnapshot();

        var degrees = calculator.CalculateForwardDownhillDegrees(Vector3.up, snapshot);

        Assert.That(degrees, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void CalculateForwardDownhillDegrees_SurfaceDescendsForward_ReturnsPositiveDegrees()
    {
        var calculator = new RunSurfaceSlopeCalculator();
        var snapshot = CreateSnapshot();
        var normal = CreateForwardSlopeNormal(30f);

        var degrees = calculator.CalculateForwardDownhillDegrees(normal, snapshot);

        Assert.That(degrees, Is.EqualTo(30f).Within(0.0001f));
    }

    [Test]
    public void CalculateForwardDownhillDegrees_SurfaceAscendsForward_ReturnsNegativeDegrees()
    {
        var calculator = new RunSurfaceSlopeCalculator();
        var snapshot = CreateSnapshot();
        var normal = CreateForwardSlopeNormal(-20f);

        var degrees = calculator.CalculateForwardDownhillDegrees(normal, snapshot);

        Assert.That(degrees, Is.EqualTo(-20f).Within(0.0001f));
    }

    [Test]
    public void CalculateForwardDownhillDegrees_BankedSurface_ReturnsZero()
    {
        var calculator = new RunSurfaceSlopeCalculator();
        var snapshot = CreateSnapshot();
        var bankedNormal = new Vector3(0.5f, 0.8660254f, 0f);

        var degrees = calculator.CalculateForwardDownhillDegrees(bankedNormal, snapshot);

        Assert.That(degrees, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void CalculateForwardDownhillDegrees_NonFiniteNormal_ReturnsZero()
    {
        var calculator = new RunSurfaceSlopeCalculator();
        var snapshot = CreateSnapshot();

        var degrees = calculator.CalculateForwardDownhillDegrees(new Vector3(float.NaN, 1f, 0f), snapshot);

        Assert.That(degrees, Is.EqualTo(0f));
    }

    [Test]
    public void Constructor_GroundedContext_StoresGroundNormalAndSlope()
    {
        var context = new RunSurfaceContext(true, new Vector3(0f, 2f, 0f), 12f);

        Assert.That(context.IsGrounded, Is.True);
        Assert.That(context.GroundNormal, Is.EqualTo(Vector3.up));
        Assert.That(context.ForwardDownhillDegrees, Is.EqualTo(12f));
    }

    [Test]
    public void Constructor_UngroundedContext_UsesUprightNormalAndZeroSlope()
    {
        var context = new RunSurfaceContext(false, Vector3.forward, 20f);

        Assert.That(context.IsGrounded, Is.False);
        Assert.That(context.GroundNormal, Is.EqualTo(Vector3.up));
        Assert.That(context.ForwardDownhillDegrees, Is.EqualTo(0f));
    }

    private RunProgressFrameSnapshot CreateSnapshot()
    {
        Assert.That(RunProgressFrameSnapshot.TryCreate(Vector3.zero, Vector3.forward, Vector3.up, out var snapshot, out var error), Is.True, error);
        return snapshot;
    }

    private Vector3 CreateForwardSlopeNormal(float degrees)
    {
        var radians = degrees * Mathf.Deg2Rad;
        return new Vector3(0f, Mathf.Cos(radians), Mathf.Sin(radians));
    }
}
