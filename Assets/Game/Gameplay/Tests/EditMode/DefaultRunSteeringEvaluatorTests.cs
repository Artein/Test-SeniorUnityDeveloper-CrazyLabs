using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

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
    public void Evaluate_GroundedSteering_RotatesDirectionWithoutOwningSpeed()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * 12f + Vector3.up * 3f,
            steeringMode: RunSteeringMode.Grounded,
            smoothedSteer: 1f,
            maximumTurnDegreesPerSecond: 90f,
            fixedDeltaTime: 1f,
            isGestureActive: true));

        Assert.That(decision.ShouldApplySteering, Is.True);
        Assert.That(decision.SteeringIntentDirection.magnitude, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(Vector3.Dot(decision.SteeringIntentDirection, Vector3.right), Is.GreaterThan(0.9999f));
    }

    [Test]
    public void Evaluate_GroundedNeutralInput_PreservesUsableCurrentDirectionForFacing()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * 4f,
            steeringMode: RunSteeringMode.Grounded,
            smoothedSteer: 0f,
            maximumTurnDegreesPerSecond: 90f,
            fixedDeltaTime: 0.02f,
            isGestureActive: false));

        Assert.That(decision.ShouldApplySteering, Is.True);
        Assert.That(Vector3.Dot(decision.SteeringIntentDirection, Vector3.forward), Is.GreaterThan(0.9999f));
    }

    [Test]
    public void Evaluate_BelowMinimumSteerSpeed_ReturnsNoSteeringIntent()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * 0.1f,
            steeringMode: RunSteeringMode.Grounded,
            smoothedSteer: 1f,
            maximumTurnDegreesPerSecond: 90f,
            fixedDeltaTime: 1f,
            isGestureActive: true));

        Assert.That(decision.ShouldApplySteering, Is.False);
        Assert.That(decision.SteeringIntentDirection, Is.EqualTo(Vector3.zero));
    }

    [Test]
    public void Evaluate_AirWithoutActiveGesture_ReturnsNoSteeringIntent()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * 8f,
            steeringMode: RunSteeringMode.Air,
            smoothedSteer: 1f,
            maximumTurnDegreesPerSecond: 30f,
            fixedDeltaTime: 1f,
            isGestureActive: false));

        Assert.That(decision.ShouldApplySteering, Is.False);
    }

    [Test]
    public void Evaluate_AirWithNeutralSteer_ReturnsNoSteeringIntent()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.forward * 8f,
            steeringMode: RunSteeringMode.Air,
            smoothedSteer: 0.00001f,
            maximumTurnDegreesPerSecond: 30f,
            fixedDeltaTime: 1f,
            isGestureActive: true));

        Assert.That(decision.ShouldApplySteering, Is.False);
    }

    [Test]
    public void Evaluate_InvalidSteeringPlaneVelocity_DoesNotInventDirection()
    {
        var decision = _evaluator.Evaluate(CreateContext(
            currentVelocity: Vector3.up * 8f,
            steeringMode: RunSteeringMode.Grounded,
            smoothedSteer: 1f,
            maximumTurnDegreesPerSecond: 90f,
            fixedDeltaTime: 1f,
            isGestureActive: true));

        Assert.That(decision.ShouldApplySteering, Is.False);
        Assert.That(decision.SteeringIntentDirection, Is.EqualTo(Vector3.zero));
    }

    private RunSteeringContext CreateContext(
        Vector3 currentVelocity,
        RunSteeringMode steeringMode,
        float smoothedSteer,
        float maximumTurnDegreesPerSecond,
        float fixedDeltaTime,
        bool isGestureActive)
    {
        return new RunSteeringContext(
            currentVelocity,
            Vector3.up,
            steeringMode,
            smoothedSteer,
            maximumTurnDegreesPerSecond,
            minimumSteerSpeed: 0.25f,
            fixedDeltaTime,
            isGestureActive);
    }
}
