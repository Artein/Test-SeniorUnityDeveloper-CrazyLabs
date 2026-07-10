using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class DefaultRunBodySpeedEvaluatorTests
{
    private FakeRunBodySpeedConfig _config;
    private DefaultRunBodySpeedEvaluator _evaluator;

    [SetUp]
    public void SetUp()
    {
        _config = new FakeRunBodySpeedConfig
        {
            DownhillAcceleration = 8f,
            BaseSoftMaximumSpeed = 20f,
        };
        _evaluator = new DefaultRunBodySpeedEvaluator(_config);
    }

    [Test]
    public void Evaluate_UnsupportedContext_ReturnsNeutralDecisionWithResolvedEnvelope()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            hasValidGroundedRunSurface: false,
            forwardDownhillDegrees: 30f,
            courseForwardAlignment: 1f));

        Assert.That(decision.TangentAcceleration, Is.Zero);
        Assert.That(decision.TangentDrag, Is.Zero);
        Assert.That(decision.LowSpeedAssistTargetSpeed, Is.Zero);
        Assert.That(decision.LowSpeedAssistAcceleration, Is.Zero);
        Assert.That(decision.SoftMaximumSpeed, Is.EqualTo(24f));
        Assert.That(decision.Contributors, Is.EqualTo(RunBodySpeedDecisionContributors.None));
    }

    [Test]
    public void Evaluate_InvalidSurfaceNormal_ReturnsNeutralDecision()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            surfaceNormal: Vector3.zero,
            forwardDownhillDegrees: 30f,
            courseForwardAlignment: 1f));

        Assert.That(decision.TangentAcceleration, Is.Zero);
        Assert.That(decision.Contributors, Is.EqualTo(RunBodySpeedDecisionContributors.None));
    }

    [TestCase(0f)]
    [TestCase(-15f)]
    [TestCase(-45f)]
    public void Evaluate_FlatOrUphillContext_ReturnsNoDownhillAcceleration(float forwardDownhillDegrees)
    {
        var decision = _evaluator.Evaluate(CreateContext(
            forwardDownhillDegrees: forwardDownhillDegrees,
            courseForwardAlignment: 1f));

        Assert.That(decision.TangentAcceleration, Is.Zero);
        Assert.That(decision.Contributors, Is.EqualTo(RunBodySpeedDecisionContributors.None));
    }

    [TestCase(15f)]
    [TestCase(30f)]
    [TestCase(60f)]
    [TestCase(90f)]
    public void Evaluate_PositiveDownhillAngle_ReturnsSineScaledAcceleration(float forwardDownhillDegrees)
    {
        var decision = _evaluator.Evaluate(CreateContext(
            forwardDownhillDegrees: forwardDownhillDegrees,
            courseForwardAlignment: 1f));

        var expectedAcceleration = _config.DownhillAcceleration
                                   * Mathf.Sin(forwardDownhillDegrees * Mathf.Deg2Rad);

        Assert.That(decision.TangentAcceleration, Is.EqualTo(expectedAcceleration).Within(0.0001f));

        Assert.That(
            decision.Contributors,
            Is.EqualTo(RunBodySpeedDecisionContributors.DownhillAcceleration));
    }

    [TestCase(1f, 8f)]
    [TestCase(0.5f, 4f)]
    [TestCase(0.25f, 2f)]
    [TestCase(0f, 0f)]
    [TestCase(-0.5f, 0f)]
    [TestCase(-1f, 0f)]
    public void Evaluate_CourseForwardAlignment_ProportionallyGatesPositiveAcceleration(
        float courseForwardAlignment,
        float expectedAcceleration)
    {
        var decision = _evaluator.Evaluate(CreateContext(
            forwardDownhillDegrees: 90f,
            courseForwardAlignment: courseForwardAlignment));

        Assert.That(decision.TangentAcceleration, Is.EqualTo(expectedAcceleration).Within(0.0001f));

        Assert.That(
            decision.Contributors.HasFlag(RunBodySpeedDecisionContributors.DownhillAcceleration),
            Is.EqualTo(expectedAcceleration > 0f));
    }

    [Test]
    public void Evaluate_DirectionlessTravel_ReturnsNoDownhillAcceleration()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.zero,
            forwardDownhillDegrees: 30f,
            courseForwardAlignment: 0f));

        Assert.That(decision.TangentAcceleration, Is.Zero);
        Assert.That(decision.Contributors, Is.EqualTo(RunBodySpeedDecisionContributors.None));
    }

    [TestCase(24f)]
    [TestCase(28f)]
    public void Evaluate_AtOrAboveSoftEnvelope_ReturnsNoDownhillAcceleration(float tangentSpeed)
    {
        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * tangentSpeed,
            forwardDownhillDegrees: 30f,
            courseForwardAlignment: 1f));

        Assert.That(decision.TangentAcceleration, Is.Zero);
        Assert.That(decision.Contributors, Is.EqualTo(RunBodySpeedDecisionContributors.None));
    }

    [Test]
    public void Evaluate_ContributorMetadata_DoesNotChangeNumericalOutput()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            forwardDownhillDegrees: 30f,
            courseForwardAlignment: 1f));

        Assert.That(decision.TangentAcceleration, Is.EqualTo(4f).Within(0.0001f));
        Assert.That(decision.TangentDrag, Is.Zero);

        Assert.That(
            decision.Contributors,
            Is.EqualTo(RunBodySpeedDecisionContributors.DownhillAcceleration));
    }

    [TestCase(0f, 1f, 1f)]
    [TestCase(1f, 1f, 0.7071068f)]
    [TestCase(1f, 0f, 0f)]
    [TestCase(0f, -1f, -1f)]
    public void Evaluate_EqualSpeedGroundedTravel_ReturnsEqualSurfaceSlowdownRegardlessOfHeading(
        float x,
        float z,
        float courseForwardAlignment)
    {
        _config.SurfaceSlowdown = 2.5f;
        var velocity = new Vector3(x, 0f, z).normalized * 5f;

        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: velocity,
            courseForwardAlignment: courseForwardAlignment));

        Assert.That(decision.TangentDrag, Is.EqualTo(2.5f));
        Assert.That(decision.TangentAcceleration, Is.Zero);
        Assert.That(decision.Contributors, Is.EqualTo(RunBodySpeedDecisionContributors.SurfaceSlowdown));
    }

    [Test]
    public void Evaluate_UnsupportedContextWithSurfaceSlowdown_ReturnsNoSlowdown()
    {
        _config.SurfaceSlowdown = 2.5f;

        var decision = _evaluator.Evaluate(CreateContext(
            hasValidGroundedRunSurface: false,
            courseForwardAlignment: 1f));

        Assert.That(decision.TangentDrag, Is.Zero);
        Assert.That(decision.Contributors, Is.EqualTo(RunBodySpeedDecisionContributors.None));
    }

    [Test]
    public void Evaluate_DownhillTravelWithSurfaceSlowdown_ReturnsBothRatesAndContributors()
    {
        _config.SurfaceSlowdown = 2.5f;

        var decision = _evaluator.Evaluate(CreateContext(
            forwardDownhillDegrees: 30f,
            courseForwardAlignment: 1f));

        Assert.That(decision.TangentAcceleration, Is.EqualTo(4f).Within(0.0001f));
        Assert.That(decision.TangentDrag, Is.EqualTo(2.5f));

        Assert.That(
            decision.Contributors,
            Is.EqualTo(
                RunBodySpeedDecisionContributors.DownhillAcceleration
                | RunBodySpeedDecisionContributors.SurfaceSlowdown));
    }

    [TestCase(23f, 0f)]
    [TestCase(24f, 0f)]
    [TestCase(36f, 6f)]
    [TestCase(48f, 12f)]
    [TestCase(60f, 12f)]
    public void Evaluate_GroundedSpeedAtAndAboveEnvelope_ReturnsNormalizedResistance(
        float tangentSpeed,
        float expectedResistance)
    {
        _config.AboveMaximumSpeedResistance = 12f;

        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * tangentSpeed));

        Assert.That(decision.TangentDrag, Is.EqualTo(expectedResistance).Within(0.0001f));

        Assert.That(
            decision.Contributors.HasFlag(RunBodySpeedDecisionContributors.AboveEnvelopeResistance),
            Is.EqualTo(expectedResistance > 0f));
    }

    [TestCase(0f, 1f, 1f)]
    [TestCase(1f, 1f, 0.7071068f)]
    [TestCase(1f, 0f, 0f)]
    [TestCase(0f, -1f, -1f)]
    public void Evaluate_EqualOverspeed_ReturnsEqualResistanceRegardlessOfHeading(
        float x,
        float z,
        float courseForwardAlignment)
    {
        _config.AboveMaximumSpeedResistance = 12f;
        var velocity = new Vector3(x, 0f, z).normalized * 36f;

        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: velocity,
            courseForwardAlignment: courseForwardAlignment));

        Assert.That(decision.TangentDrag, Is.EqualTo(6f).Within(0.0001f));

        Assert.That(
            decision.Contributors,
            Is.EqualTo(RunBodySpeedDecisionContributors.AboveEnvelopeResistance));
    }

    [Test]
    public void Evaluate_SurfaceSlowdownAndOverspeed_ReturnsComposedDragAndContributors()
    {
        _config.SurfaceSlowdown = 2.5f;
        _config.AboveMaximumSpeedResistance = 12f;

        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * 36f));

        Assert.That(decision.TangentDrag, Is.EqualTo(8.5f).Within(0.0001f));

        Assert.That(
            decision.Contributors,
            Is.EqualTo(
                RunBodySpeedDecisionContributors.SurfaceSlowdown
                | RunBodySpeedDecisionContributors.AboveEnvelopeResistance));
    }

    [Test]
    public void Evaluate_UpgradedEnvelope_ShiftsResistanceThreshold()
    {
        _config.AboveMaximumSpeedResistance = 12f;

        var baseDecision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * 30f,
            resolvedSoftMaximumSpeed: 24f));

        var upgradedDecision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * 30f,
            resolvedSoftMaximumSpeed: 36f));

        Assert.That(baseDecision.TangentDrag, Is.EqualTo(3f).Within(0.0001f));
        Assert.That(upgradedDecision.TangentDrag, Is.Zero);
    }

    [Test]
    public void Evaluate_UnsupportedOverspeed_ReturnsNoResistance()
    {
        _config.AboveMaximumSpeedResistance = 12f;

        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * 48f,
            hasValidGroundedRunSurface: false));

        Assert.That(decision.TangentDrag, Is.Zero);
        Assert.That(decision.Contributors, Is.EqualTo(RunBodySpeedDecisionContributors.None));
    }

    [Test]
    public void Evaluate_EligibleLowSpeedTravel_ReturnsEnvelopeCappedAssist()
    {
        _config.LowSpeedAssistTargetSpeed = 30f;
        _config.LowSpeedAssistAcceleration = 8f;

        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * 5f,
            courseForwardAlignment: 0.5f,
            resolvedSoftMaximumSpeed: 24f));

        Assert.That(decision.LowSpeedAssistTargetSpeed, Is.EqualTo(24f));
        Assert.That(decision.LowSpeedAssistAcceleration, Is.EqualTo(4f));
        Assert.That(_config.LowSpeedAssistTargetSpeed, Is.EqualTo(30f));

        Assert.That(
            decision.Contributors.HasFlag(RunBodySpeedDecisionContributors.LowSpeedAssist),
            Is.True);
    }

    [TestCase(24f, 1f)]
    [TestCase(25f, 1f)]
    [TestCase(5f, 0f)]
    [TestCase(5f, -1f)]
    [TestCase(0f, 1f)]
    public void Evaluate_IneligibleLowSpeedTravel_ReturnsNoAssist(
        float tangentSpeed,
        float courseForwardAlignment)
    {
        _config.LowSpeedAssistTargetSpeed = 24f;
        _config.LowSpeedAssistAcceleration = 8f;

        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * tangentSpeed,
            courseForwardAlignment: courseForwardAlignment,
            resolvedSoftMaximumSpeed: 30f));

        Assert.That(decision.LowSpeedAssistTargetSpeed, Is.Zero);
        Assert.That(decision.LowSpeedAssistAcceleration, Is.Zero);

        Assert.That(
            decision.Contributors.HasFlag(RunBodySpeedDecisionContributors.LowSpeedAssist),
            Is.False);
    }

    private RunBodySpeedContext CreateContext(
        Vector3? currentVelocity = null,
        bool hasValidGroundedRunSurface = true,
        Vector3? surfaceNormal = null,
        float forwardDownhillDegrees = 0f,
        float courseForwardAlignment = 1f,
        float resolvedSoftMaximumSpeed = 24f)
    {
        return new RunBodySpeedContext(
            currentVelocity ?? Vector3.forward * 5f,
            hasValidGroundedRunSurface,
            surfaceNormal ?? Vector3.up,
            forwardDownhillDegrees,
            courseForwardAlignment,
            resolvedSoftMaximumSpeed);
    }

    private sealed class FakeRunBodySpeedConfig : IRunBodySpeedConfig
    {
        public float DownhillAcceleration { get; set; }
        public float SurfaceSlowdown { get; set; }
        public float LowSpeedAssistTargetSpeed { get; set; }
        public float LowSpeedAssistAcceleration { get; set; }
        public float BaseSoftMaximumSpeed { get; set; }
        public float AboveMaximumSpeedResistance { get; set; }
    }
}
