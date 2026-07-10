using Game.Gameplay;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public sealed class DefaultRunSteeringEvaluatorTests
{
    private DefaultRunSteeringEvaluator _evaluator;

    [SetUp]
    public void OnSetUp()
    {
        _evaluator = new DefaultRunSteeringEvaluator();
    }

    [Test]
    public void Evaluate_GroundedSteering_ReturnsTurnAndFacingIntentsWithoutOwningDirection()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            tangentSpeed: 12f,
            steeringMode: RunSteeringMode.Grounded,
            smoothedSteer: 1f,
            maximumTurnDegreesPerSecond: 90f,
            fixedDeltaTime: 1f,
            isGestureActive: true));

        Assert.That(decision.ShouldTurnVelocity, Is.True);
        Assert.That(decision.SignedTurnDegrees, Is.EqualTo(90f));
        Assert.That(decision.ShouldUpdateFacing, Is.True);
    }

    [Test]
    public void Evaluate_GroundedNeutralInput_ReturnsFacingWithoutVelocityTurn()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            tangentSpeed: 4f,
            steeringMode: RunSteeringMode.Grounded,
            smoothedSteer: 0f,
            maximumTurnDegreesPerSecond: 90f,
            fixedDeltaTime: 0.02f,
            isGestureActive: false));

        Assert.That(decision.ShouldTurnVelocity, Is.False);
        Assert.That(decision.SignedTurnDegrees, Is.Zero);
        Assert.That(decision.ShouldUpdateFacing, Is.True);
    }

    [Test]
    public void Evaluate_BelowMinimumSteerSpeed_ReturnsNoTurnOrFacingIntent()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            tangentSpeed: 0.1f,
            steeringMode: RunSteeringMode.Grounded,
            smoothedSteer: 1f,
            maximumTurnDegreesPerSecond: 90f,
            fixedDeltaTime: 1f,
            isGestureActive: true));

        Assert.That(decision.ShouldTurnVelocity, Is.False);
        Assert.That(decision.SignedTurnDegrees, Is.Zero);
        Assert.That(decision.ShouldUpdateFacing, Is.False);
    }

    [Test]
    public void Evaluate_AirWithoutActiveGesture_ReturnsNoTurnOrFacingIntent()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            tangentSpeed: 8f,
            steeringMode: RunSteeringMode.Air,
            smoothedSteer: 1f,
            maximumTurnDegreesPerSecond: 30f,
            fixedDeltaTime: 1f,
            isGestureActive: false));

        Assert.That(decision.ShouldTurnVelocity, Is.False);
        Assert.That(decision.ShouldUpdateFacing, Is.False);
    }

    [Test]
    public void Evaluate_AirWithNeutralSteer_ReturnsNoTurnOrFacingIntent()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            tangentSpeed: 8f,
            steeringMode: RunSteeringMode.Air,
            smoothedSteer: 0.00001f,
            maximumTurnDegreesPerSecond: 30f,
            fixedDeltaTime: 1f,
            isGestureActive: true));

        Assert.That(decision.ShouldTurnVelocity, Is.False);
        Assert.That(decision.ShouldUpdateFacing, Is.False);
    }

    [Test]
    public void Evaluate_MissingTangentDirection_ReturnsNoTurnOrFacingIntent()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            tangentSpeed: 8f,
            steeringMode: RunSteeringMode.Grounded,
            smoothedSteer: 1f,
            maximumTurnDegreesPerSecond: 90f,
            fixedDeltaTime: 1f,
            isGestureActive: true,
            hasUsableTangentDirection: false));

        Assert.That(decision.ShouldTurnVelocity, Is.False);
        Assert.That(decision.SignedTurnDegrees, Is.Zero);
        Assert.That(decision.ShouldUpdateFacing, Is.False);
    }

    private RunSteeringContext CreateContext(
        float tangentSpeed,
        RunSteeringMode steeringMode,
        float smoothedSteer,
        float maximumTurnDegreesPerSecond,
        float fixedDeltaTime,
        bool isGestureActive,
        bool hasUsableTangentDirection = true)
    {
        return new RunSteeringContext(
            tangentSpeed,
            hasUsableTangentDirection,
            steeringMode,
            smoothedSteer,
            maximumTurnDegreesPerSecond,
            minimumSteerSpeed: 0.25f,
            fixedDeltaTime,
            isGestureActive);
    }
}
