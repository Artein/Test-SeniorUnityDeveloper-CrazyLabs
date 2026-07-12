using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunSurfaceStabilityPolicyTests
{
    private RunSurfaceStabilityConfig _config;
    private RunSurfaceStabilityPolicy _policy;
    private RunProgressFrameSnapshot _frame;

    [SetUp]
    public void OnSetUp()
    {
        _config = new RunSurfaceStabilityConfig(
            supportLossConfirmationSeconds: 0.06f,
            discontinuousNormalThresholdDegrees: 45f,
            discontinuousNormalConfirmationSeconds: 0.04f,
            candidateCoherenceDegrees: 5f);
        _policy = new RunSurfaceStabilityPolicy(_config, new RunSurfaceSlopeCalculator());

        var created = RunProgressFrameSnapshot.TryCreate(
            Vector3.zero,
            Vector3.forward,
            Vector3.up,
            out _frame,
            out var error);

        Assert.That(created, Is.True, error);
    }

    [Test]
    public void Evaluate_FirstSupportedObservation_AcquiresImmediately()
    {
        var normal = TiltedUp(20f, Vector3.right);

        var result = _policy.Evaluate(Supported(normal), 0.02f);

        Assert.That(result.Transition, Is.EqualTo(RunSurfaceTransition.SupportAcquired));
        Assert.That(result.StableSupport.IsGrounded, Is.True);
        AssertVectorNear(result.StableSupport.GroundNormal, normal);
        Assert.That(result.IsMissingSupportHeld, Is.False);
        Assert.That(result.IsConfirmingDiscontinuity, Is.False);
    }

    [Test]
    public void Evaluate_MissingBeforeThreshold_HoldsStableSupport()
    {
        var normal = TiltedUp(20f, Vector3.right);
        _policy.Evaluate(Supported(normal), 0.02f);

        var firstMiss = _policy.Evaluate(Missing(), 0.02f);
        var secondMiss = _policy.Evaluate(Missing(), 0.02f);

        Assert.That(firstMiss.StableSupport.IsGrounded, Is.True);
        Assert.That(secondMiss.StableSupport.IsGrounded, Is.True);
        Assert.That(firstMiss.IsMissingSupportHeld, Is.True);
        Assert.That(secondMiss.IsMissingSupportHeld, Is.True);
        Assert.That(secondMiss.Transition, Is.EqualTo(RunSurfaceTransition.None));
    }

    [Test]
    public void Evaluate_MissingAtThreshold_LosesSupportExactlyOnce()
    {
        _policy.Evaluate(Supported(Vector3.up), 0.02f);
        _policy.Evaluate(Missing(), 0.02f);
        _policy.Evaluate(Missing(), 0.02f);

        var thresholdMiss = _policy.Evaluate(Missing(), 0.02f);
        var laterMiss = _policy.Evaluate(Missing(), 0.02f);

        Assert.That(thresholdMiss.StableSupport.IsGrounded, Is.False);
        Assert.That(thresholdMiss.Transition, Is.EqualTo(RunSurfaceTransition.SupportLost));
        Assert.That(thresholdMiss.IsMissingSupportHeld, Is.False);
        Assert.That(laterMiss.Transition, Is.EqualTo(RunSurfaceTransition.None));
    }

    [Test]
    public void Evaluate_ZeroSupportLossDuration_LosesOnFirstMissingObservation()
    {
        _policy = new RunSurfaceStabilityPolicy(
            new RunSurfaceStabilityConfig(0f, 45f, 0.04f, 5f),
            new RunSurfaceSlopeCalculator());
        _policy.Evaluate(Supported(Vector3.up), 0.02f);

        var result = _policy.Evaluate(Missing(), 0.02f);

        Assert.That(result.Transition, Is.EqualTo(RunSurfaceTransition.SupportLost));
        Assert.That(result.StableSupport.IsGrounded, Is.False);
    }

    [Test]
    public void Evaluate_UnavailableAfterSupport_HardResetsImmediately()
    {
        _policy.Evaluate(Supported(Vector3.up), 0.02f);
        _policy.Evaluate(Missing(), 0.02f);

        var result = _policy.Evaluate(Unavailable(), 0.02f);

        Assert.That(result.Transition, Is.EqualTo(RunSurfaceTransition.HardReset));
        Assert.That(result.StableSupport.IsGrounded, Is.False);
        Assert.That(result.IsMissingSupportHeld, Is.False);
        Assert.That(result.IsConfirmingDiscontinuity, Is.False);
    }

    [Test]
    public void Evaluate_SupportedAfterLoss_ReacquiresImmediately()
    {
        _policy = new RunSurfaceStabilityPolicy(
            new RunSurfaceStabilityConfig(0f, 45f, 0.04f, 5f),
            new RunSurfaceSlopeCalculator());
        _policy.Evaluate(Supported(Vector3.up), 0.02f);
        _policy.Evaluate(Missing(), 0.02f);
        var reacquiredNormal = TiltedUp(30f, Vector3.right);

        var result = _policy.Evaluate(Supported(reacquiredNormal), 0.02f);

        Assert.That(result.Transition, Is.EqualTo(RunSurfaceTransition.SupportAcquired));
        AssertVectorNear(result.StableSupport.GroundNormal, reacquiredNormal);
    }

    [Test]
    public void Evaluate_ContinuousNormalChange_UpdatesWithoutConfirmation()
    {
        _policy.Evaluate(Supported(Vector3.up), 0.02f);
        var continuousNormal = TiltedUp(30f, Vector3.right);

        var result = _policy.Evaluate(Supported(continuousNormal), 0.02f);

        Assert.That(result.Transition, Is.EqualTo(RunSurfaceTransition.ContinuousUpdate));
        AssertVectorNear(result.StableSupport.GroundNormal, continuousNormal);
        Assert.That(result.IsConfirmingDiscontinuity, Is.False);
    }

    [Test]
    public void Evaluate_FirstDiscontinuousCandidate_HoldsPreviousStableSupport()
    {
        _policy.Evaluate(Supported(Vector3.up), 0.02f);

        var result = _policy.Evaluate(Supported(TiltedUp(80f, Vector3.right)), 0.02f);

        Assert.That(result.Transition, Is.EqualTo(RunSurfaceTransition.None));
        AssertVectorNear(result.StableSupport.GroundNormal, Vector3.up);
        Assert.That(result.IsConfirmingDiscontinuity, Is.True);
    }

    [Test]
    public void Evaluate_CoherentCandidatesAtDuration_ConfirmsRepresentativeNormal()
    {
        var firstCandidate = TiltedUp(80f, Vector3.right);
        var secondCandidate = TiltedUp(82f, Vector3.right);
        _policy.Evaluate(Supported(Vector3.up), 0.02f);
        _policy.Evaluate(Supported(firstCandidate), 0.02f);

        var result = _policy.Evaluate(Supported(secondCandidate), 0.02f);

        Assert.That(result.Transition, Is.EqualTo(RunSurfaceTransition.ConfirmedDiscontinuity));
        Assert.That(result.IsConfirmingDiscontinuity, Is.False);
        Assert.That(Vector3.Angle(result.StableSupport.GroundNormal, TiltedUp(81f, Vector3.right)), Is.LessThan(0.1f));
    }

    [Test]
    public void Evaluate_AlternatingIncoherentCandidates_NeverAccumulatesConfirmation()
    {
        var firstCandidate = TiltedUp(80f, Vector3.right);
        var secondCandidate = TiltedUp(80f, Vector3.forward);
        _policy.Evaluate(Supported(Vector3.up), 0.02f);

        for (var sampleIndex = 0; sampleIndex < 8; sampleIndex += 1)
        {
            var candidate = sampleIndex % 2 == 0 ? firstCandidate : secondCandidate;
            var result = _policy.Evaluate(Supported(candidate), 0.02f);

            Assert.That(result.Transition, Is.EqualTo(RunSurfaceTransition.None), $"sample {sampleIndex}");
            AssertVectorNear(result.StableSupport.GroundNormal, Vector3.up);
        }
    }

    [Test]
    public void Evaluate_ReturnToStableNeighborhood_CancelsPendingConfirmation()
    {
        _policy.Evaluate(Supported(Vector3.up), 0.02f);
        _policy.Evaluate(Supported(TiltedUp(80f, Vector3.right)), 0.02f);

        var result = _policy.Evaluate(Supported(TiltedUp(10f, Vector3.right)), 0.02f);

        Assert.That(result.Transition, Is.EqualTo(RunSurfaceTransition.ContinuousUpdate));
        Assert.That(result.IsConfirmingDiscontinuity, Is.False);
    }

    [Test]
    public void Evaluate_ZeroDiscontinuityDuration_ConfirmsFirstCandidate()
    {
        _policy = new RunSurfaceStabilityPolicy(
            new RunSurfaceStabilityConfig(0.06f, 45f, 0f, 5f),
            new RunSurfaceSlopeCalculator());
        _policy.Evaluate(Supported(Vector3.up), 0.02f);

        var result = _policy.Evaluate(Supported(TiltedUp(80f, Vector3.right)), 0.02f);

        Assert.That(result.Transition, Is.EqualTo(RunSurfaceTransition.ConfirmedDiscontinuity));
    }

    [TestCase(0.01f, 6)]
    [TestCase(0.02f, 3)]
    public void Evaluate_SupportLossThreshold_UsesSecondsWithinOneTick(float fixedDeltaTime, int expectedMissCount)
    {
        _policy.Evaluate(Supported(Vector3.up), fixedDeltaTime);

        for (var missIndex = 1; missIndex < expectedMissCount; missIndex += 1)
        {
            var held = _policy.Evaluate(Missing(), fixedDeltaTime);
            Assert.That(held.StableSupport.IsGrounded, Is.True, $"miss {missIndex}");
        }

        var lost = _policy.Evaluate(Missing(), fixedDeltaTime);

        Assert.That(lost.Transition, Is.EqualTo(RunSurfaceTransition.SupportLost));
    }

    [TestCase(0.01f, 4)]
    [TestCase(0.02f, 2)]
    public void Evaluate_DiscontinuityThreshold_UsesSecondsWithinOneTick(float fixedDeltaTime, int expectedCandidateCount)
    {
        var candidate = TiltedUp(80f, Vector3.right);
        _policy.Evaluate(Supported(Vector3.up), fixedDeltaTime);

        for (var candidateIndex = 1; candidateIndex < expectedCandidateCount; candidateIndex += 1)
        {
            var confirming = _policy.Evaluate(Supported(candidate), fixedDeltaTime);
            Assert.That(confirming.IsConfirmingDiscontinuity, Is.True, $"candidate {candidateIndex}");
        }

        var confirmed = _policy.Evaluate(Supported(candidate), fixedDeltaTime);

        Assert.That(confirmed.Transition, Is.EqualTo(RunSurfaceTransition.ConfirmedDiscontinuity));
    }

    private RunSupportObservation Supported(Vector3 normal)
    {
        return new RunSupportObservation(
            RunSupportObservationState.Supported,
            _frame,
            new RunSurfaceContext(true, normal, 0f),
            supportDistance: 0.01f);
    }

    private RunSupportObservation Missing()
    {
        return new RunSupportObservation(
            RunSupportObservationState.Missing,
            _frame,
            new RunSurfaceContext(false, Vector3.up, 0f),
            supportDistance: 0f);
    }

    private RunSupportObservation Unavailable()
    {
        return new RunSupportObservation(
            RunSupportObservationState.Unavailable,
            default,
            new RunSurfaceContext(false, Vector3.up, 0f),
            supportDistance: 0f);
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
