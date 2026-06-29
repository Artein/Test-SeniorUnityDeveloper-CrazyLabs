using Game.Gameplay.Slingshot;
using NUnit.Framework;
using Unity.Mathematics;

// ReSharper disable once CheckNamespace
public sealed class PullPlaneBandShapeClearanceTests
{
    [Test]
    public void IsClear_BandShapeOutsideSilhouetteRadius_ReturnsTrue()
    {
        var clearance = new PullPlaneBandShapeClearance(8);

        var isClear = clearance.IsClear(
            new[] { new float2(-1f, 1f), new float2(1f, 1f) },
            2,
            CreateSquareSamples(),
            4,
            0.1f);

        Assert.That(isClear, Is.True);
    }

    [Test]
    public void IsClear_BandShapeCrossesSilhouette_ReturnsFalse()
    {
        var clearance = new PullPlaneBandShapeClearance(8);

        var isClear = clearance.IsClear(
            new[] { new float2(-1f, 0f), new float2(1f, 0f) },
            2,
            CreateSquareSamples(),
            4,
            0f);

        Assert.That(isClear, Is.False);
    }

    [Test]
    public void IsClear_BandShapeTouchesVisibleRadiusBoundary_ReturnsFalse()
    {
        var clearance = new PullPlaneBandShapeClearance(8);

        var isClear = clearance.IsClear(
            new[] { new float2(-1f, 0.6f), new float2(1f, 0.6f) },
            2,
            CreateSquareSamples(),
            4,
            0.1f);

        Assert.That(isClear, Is.False);
    }

    [Test]
    public void IsClear_BandShapeJustBeyondVisibleRadius_ReturnsTrue()
    {
        var clearance = new PullPlaneBandShapeClearance(8);

        var isClear = clearance.IsClear(
            new[] { new float2(-1f, 0.61f), new float2(1f, 0.61f) },
            2,
            CreateSquareSamples(),
            4,
            0.1f);

        Assert.That(isClear, Is.True);
    }

    [Test]
    public void IsClear_BandShapeEndpointInsideSilhouette_ReturnsFalse()
    {
        var clearance = new PullPlaneBandShapeClearance(8);

        var isClear = clearance.IsClear(
            new[] { new float2(0f, 0f), new float2(1f, 1f) },
            2,
            CreateSquareSamples(),
            4,
            0f);

        Assert.That(isClear, Is.False);
    }

    private float2[] CreateSquareSamples()
    {
        return new[]
        {
            new float2(-0.5f, -0.5f),
            new float2(0.5f, -0.5f),
            new float2(0.5f, 0.5f),
            new float2(-0.5f, 0.5f)
        };
    }
}
