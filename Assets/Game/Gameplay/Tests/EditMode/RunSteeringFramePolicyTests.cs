using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunSteeringFramePolicyTests
{
    private RunSteeringFramePolicy _policy;

    [SetUp]
    public void OnSetUp()
    {
        _policy = new RunSteeringFramePolicy(
            new RunSteeringFrameConfig(
                normalSlewDegreesPerSecond: 90f,
                airborneUpRetentionSeconds: 0.04f));
    }

    [Test]
    public void Current_BeforeReset_IsInvalidAndUsesFallback()
    {
        var fallback = new Vector3(1f, 1f, 0f).normalized;

        Assert.That(_policy.Current.IsValid, Is.False);
        AssertVectorNear(_policy.GetUpDirection(fallback), fallback);
    }

    [Test]
    public void Reset_ValidLaunchUp_InitializesFrame()
    {
        var launchUp = new Vector3(0f, 1f, 1f).normalized;

        _policy.Reset(launchUp);

        Assert.That(_policy.Current.IsValid, Is.True);
        AssertVectorNear(_policy.Current.UpDirection, launchUp);
    }

    [Test]
    public void Evaluate_SupportAcquired_InitializesFromStableNormal()
    {
        var normal = TiltedUp(30f, Vector3.right);
        _policy.Reset(Vector3.up);

        var result = _policy.Evaluate(Stable(normal, RunSurfaceTransition.SupportAcquired), 0.02f);

        Assert.That(result.IsValid, Is.True);
        AssertVectorNear(result.UpDirection, normal);
    }

    [Test]
    public void Evaluate_ContinuousUpdate_SlewsAtConfiguredRate()
    {
        _policy.Reset(Vector3.up);
        _policy.Evaluate(Stable(Vector3.up, RunSurfaceTransition.SupportAcquired), 0.02f);
        var target = TiltedUp(30f, Vector3.right);

        var result = _policy.Evaluate(Stable(target, RunSurfaceTransition.ContinuousUpdate), 0.1f);

        Assert.That(Vector3.Angle(Vector3.up, result.UpDirection), Is.EqualTo(9f).Within(0.1f));
        Assert.That(Vector3.Angle(result.UpDirection, target), Is.EqualTo(21f).Within(0.1f));
    }

    [Test]
    public void Evaluate_ConfirmedDiscontinuity_SnapsExactlyToStableNormal()
    {
        _policy.Reset(Vector3.up);
        var target = TiltedUp(80f, Vector3.right);

        var result = _policy.Evaluate(Stable(target, RunSurfaceTransition.ConfirmedDiscontinuity), 0.02f);

        AssertVectorNear(result.UpDirection, target);
    }

    [Test]
    public void Evaluate_HeldMissingSupport_PreservesSteeringFrame()
    {
        var normal = TiltedUp(20f, Vector3.right);
        _policy.Reset(Vector3.up);
        _policy.Evaluate(Stable(normal, RunSurfaceTransition.SupportAcquired), 0.02f);

        var result = _policy.Evaluate(
            new RunSurfaceStabilityResult(stableSupport: new RunSurfaceContext(true, normal, 0f), RunSurfaceTransition.None,
                isMissingSupportHeld: true, isConfirmingDiscontinuity: false),
            0.02f);

        Assert.That(result.IsValid, Is.True);
        AssertVectorNear(result.UpDirection, normal);
    }

    [Test]
    public void Evaluate_SupportLost_RetainsUpOnlyForConfiguredAirborneDuration()
    {
        var normal = TiltedUp(20f, Vector3.right);
        _policy.Reset(Vector3.up);
        _policy.Evaluate(Stable(normal, RunSurfaceTransition.SupportAcquired), 0.02f);

        var firstAirborne = _policy.Evaluate(Unsupported(RunSurfaceTransition.SupportLost), 0.02f);
        var thresholdAirborne = _policy.Evaluate(Unsupported(RunSurfaceTransition.None), 0.02f);

        Assert.That(firstAirborne.IsValid, Is.True);
        AssertVectorNear(firstAirborne.UpDirection, normal);
        Assert.That(thresholdAirborne.IsValid, Is.False);
    }

    [Test]
    public void Evaluate_ZeroAirborneRetention_ClearsOnSupportLost()
    {
        _policy = new RunSteeringFramePolicy(new RunSteeringFrameConfig(90f, 0f));
        _policy.Reset(Vector3.up);

        var result = _policy.Evaluate(Unsupported(RunSurfaceTransition.SupportLost), 0.02f);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Evaluate_HardReset_ClearsImmediately()
    {
        _policy.Reset(TiltedUp(20f, Vector3.right));

        var result = _policy.Evaluate(Unsupported(RunSurfaceTransition.HardReset), 0.02f);

        Assert.That(result.IsValid, Is.False);
        AssertVectorNear(_policy.GetUpDirection(Vector3.forward), Vector3.forward);
    }

    private RunSurfaceStabilityResult Stable(Vector3 normal, RunSurfaceTransition transition)
    {
        return new RunSurfaceStabilityResult(
            new RunSurfaceContext(true, normal, 0f),
            transition,
            isMissingSupportHeld: false,
            isConfirmingDiscontinuity: false);
    }

    private RunSurfaceStabilityResult Unsupported(RunSurfaceTransition transition)
    {
        return new RunSurfaceStabilityResult(
            new RunSurfaceContext(false, Vector3.up, 0f),
            transition,
            isMissingSupportHeld: false,
            isConfirmingDiscontinuity: false);
    }

    private Vector3 TiltedUp(float degrees, Vector3 axis)
    {
        return Quaternion.AngleAxis(degrees, axis) * Vector3.up;
    }

    private void AssertVectorNear(Vector3 actual, Vector3 expected)
    {
        Assert.That(Vector3.Angle(actual, expected), Is.LessThan(0.01f));
    }
}
