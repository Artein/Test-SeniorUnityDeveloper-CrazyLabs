using Game.Gameplay.Slingshot;
using NUnit.Framework;
using Unity.Mathematics;

// ReSharper disable once CheckNamespace
public sealed class PullPlaneTautBandSolverTests
{
    [Test]
    public void TrySolve_CenterPull_WritesFixedCountAndMiddleWrapAtPulledSideCenter()
    {
        var solver = new PullPlaneTautBandSolver(8, 5);

        var silhouette = new[]
        {
            new float2(-1f, 1f),
            new float2(-0.70710677f, 1.7071068f),
            new float2(0f, 2f),
            new float2(0.70710677f, 1.7071068f),
            new float2(1f, 1f),
            new float2(0.70710677f, 0.29289323f),
            new float2(0f, 0f),
            new float2(-0.70710677f, 0.29289323f)
        };
        var output = new float2[9];

        var solved = solver.TrySolve(
            new float2(-2f, 0f),
            new float2(2f, 0f),
            new float2(0f, 1f),
            silhouette,
            silhouette.Length,
            0f,
            5,
            output,
            out var pointCount);

        Assert.That(solved, Is.True);
        Assert.That(pointCount, Is.EqualTo(9));
        Assert.That(output[0], Is.EqualTo(new float2(-2f, 0f)));
        Assert.That(output[8], Is.EqualTo(new float2(2f, 0f)));
        AssertFloat2(output[4], new float2(0f, 2f));
    }

    [Test]
    public void TrySolve_LateralPull_UsesActualPullVectorForPulledSideCenter()
    {
        var solver = new PullPlaneTautBandSolver(4, 5);

        var silhouette = new[]
        {
            new float2(-1f, 0f),
            new float2(1f, 0f),
            new float2(1f, 2f),
            new float2(-1f, 2f)
        };
        var output = new float2[9];

        var solved = solver.TrySolve(
            new float2(-3f, 0f),
            new float2(3f, 0f),
            new float2(1f, 1f),
            silhouette,
            silhouette.Length,
            0f,
            5,
            output,
            out var pointCount);

        Assert.That(solved, Is.True);
        Assert.That(pointCount, Is.EqualTo(9));
        AssertFloat2(output[4], new float2(1f, 2f));
    }

    [Test]
    public void TrySolve_EvenWrapSampleCount_ReturnsFalseWithoutThrowing()
    {
        var solver = new PullPlaneTautBandSolver(4, 6);

        var silhouette = new[]
        {
            new float2(-1f, 0f),
            new float2(1f, 0f),
            new float2(1f, 2f),
            new float2(-1f, 2f)
        };
        var output = new float2[10];

        var solved = solver.TrySolve(
            new float2(-3f, 0f),
            new float2(3f, 0f),
            new float2(0f, 1f),
            silhouette,
            silhouette.Length,
            0f,
            6,
            output,
            out var pointCount);

        Assert.That(solved, Is.False);
        Assert.That(pointCount, Is.Zero);
    }

    [Test]
    public void TrySolve_WrapSampleCountAboveConstructorLimit_ReturnsFalseWithoutThrowing()
    {
        var solver = new PullPlaneTautBandSolver(4, 5);

        var silhouette = new[]
        {
            new float2(-1f, 0f),
            new float2(1f, 0f),
            new float2(1f, 2f),
            new float2(-1f, 2f)
        };
        var output = new float2[11];

        var solved = solver.TrySolve(
            new float2(-3f, 0f),
            new float2(3f, 0f),
            new float2(0f, 1f),
            silhouette,
            silhouette.Length,
            0f,
            7,
            output,
            out var pointCount);

        Assert.That(solved, Is.False);
        Assert.That(pointCount, Is.Zero);
    }

    [Test]
    public void TrySolve_DegenerateSilhouette_ReturnsFalseWithoutThrowing()
    {
        var solver = new PullPlaneTautBandSolver(3, 5);

        var silhouette = new[]
        {
            new float2(0f, 0f),
            new float2(0f, 0f),
            new float2(0f, 0f)
        };
        var output = new float2[9];

        var solved = solver.TrySolve(
            new float2(-3f, 0f),
            new float2(3f, 0f),
            new float2(0f, 1f),
            silhouette,
            silhouette.Length,
            0f,
            5,
            output,
            out var pointCount);

        Assert.That(solved, Is.False);
        Assert.That(pointCount, Is.Zero);
    }

    [Test]
    public void FreeSpanCrossesHullForTests_SpanThroughOppositeEdge_ReturnsTrue()
    {
        var solver = new PullPlaneTautBandSolver(4, 5);
        var hull = CreateSquareHull();

        var crosses = solver.FreeSpanCrossesHullForTests(
            new float2(2f, 1f),
            new float2(-1f, 0f),
            0,
            hull,
            hull.Length);

        Assert.That(crosses, Is.True);
    }

    [Test]
    public void FreeSpanCrossesHullForTests_TangentSpanToContact_ReturnsFalse()
    {
        var solver = new PullPlaneTautBandSolver(4, 5);
        var hull = CreateSquareHull();

        var crosses = solver.FreeSpanCrossesHullForTests(
            new float2(-2f, -1f),
            new float2(-1f, 0f),
            0,
            hull,
            hull.Length);

        Assert.That(crosses, Is.False);
    }

    private float2[] CreateSquareHull()
    {
        return new[]
        {
            new float2(-1f, 0f),
            new float2(1f, 0f),
            new float2(1f, 2f),
            new float2(-1f, 2f)
        };
    }

    private void AssertFloat2(float2 actual, float2 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
    }
}
