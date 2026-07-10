using Game.Gameplay;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public sealed class RunBodySpeedDiagnosticsTests
{
    [Test]
    public void Constructor_NoPublishedPass_ExposesInactiveSnapshot()
    {
        IRunBodySpeedDiagnosticsSource diagnostics = new RunBodySpeedDiagnostics();

        AssertInactive(diagnostics.Current);
    }

    [Test]
    public void Publish_ActiveFixedPass_PreservesExactProductionFacts()
    {
        var diagnostics = new RunBodySpeedDiagnostics();

        var expected = new RunBodySpeedDiagnosticsSnapshot(
            RunBodySpeedDiagnosticsState.Active,
            isRunSurfaceGrounded: true,
            hasValidGroundedRunSurface: true,
            hasUsableTangentDirection: true,
            sampledTangentSpeed: 7.5f,
            effectiveSoftMaximumSpeed: 24f,
            forwardDownhillDegrees: 28f,
            courseForwardAlignment: 0.75f,
            policyContributors: RunBodySpeedDecisionContributors.DownhillAcceleration
                                | RunBodySpeedDecisionContributors.SurfaceSlowdown
                                | RunBodySpeedDecisionContributors.AboveEnvelopeResistance
                                | RunBodySpeedDecisionContributors.LowSpeedAssist,
            requestedContributors: RunBodySpeedDecisionContributors.SurfaceSlowdown
                                   | RunBodySpeedDecisionContributors.LowSpeedAssist,
            requestedLowSpeedAssistVelocityDelta: 0.75f,
            effectiveLowSpeedAssistTargetSpeed: 6f,
            lowSpeedAssistAttemptState: RunBodyLowSpeedAssistAttemptState.Active,
            meetsLowSpeedAssistPolicyConditions: true,
            remainingRequestedLowSpeedAssistVelocityBudget: 1.25f);

        ((IRunBodySpeedDiagnosticsSink)diagnostics).Publish(expected);

        Assert.That(diagnostics.Current, Is.EqualTo(expected));
    }

    [Test]
    public void Clear_PreviousActivePass_ExposesInactiveSnapshotWithoutStaleValues()
    {
        var diagnostics = new RunBodySpeedDiagnostics();

        ((IRunBodySpeedDiagnosticsSink)diagnostics).Publish(new RunBodySpeedDiagnosticsSnapshot(
            RunBodySpeedDiagnosticsState.Active,
            true,
            true,
            true,
            7.5f,
            24f,
            28f,
            0.75f,
            RunBodySpeedDecisionContributors.DownhillAcceleration,
            RunBodySpeedDecisionContributors.DownhillAcceleration,
            0f,
            6f,
            RunBodyLowSpeedAssistAttemptState.Active,
            true,
            1.25f));

        ((IRunBodySpeedDiagnosticsSink)diagnostics).Clear();

        AssertInactive(diagnostics.Current);
    }

    private void AssertInactive(RunBodySpeedDiagnosticsSnapshot snapshot)
    {
        Assert.That(snapshot.State, Is.EqualTo(RunBodySpeedDiagnosticsState.Inactive));
        Assert.That(snapshot.IsRunSurfaceGrounded, Is.False);
        Assert.That(snapshot.HasValidGroundedRunSurface, Is.False);
        Assert.That(snapshot.HasUsableTangentDirection, Is.False);
        Assert.That(snapshot.SampledTangentSpeed, Is.Zero);
        Assert.That(snapshot.EffectiveSoftMaximumSpeed, Is.Zero);
        Assert.That(snapshot.ForwardDownhillDegrees, Is.Zero);
        Assert.That(snapshot.CourseForwardAlignment, Is.Zero);
        Assert.That(snapshot.PolicyContributors, Is.EqualTo(RunBodySpeedDecisionContributors.None));
        Assert.That(snapshot.RequestedContributors, Is.EqualTo(RunBodySpeedDecisionContributors.None));
        Assert.That(snapshot.RequestedLowSpeedAssistVelocityDelta, Is.Zero);
        Assert.That(snapshot.EffectiveLowSpeedAssistTargetSpeed, Is.Zero);

        Assert.That(
            snapshot.LowSpeedAssistAttemptState,
            Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Unavailable));
        Assert.That(snapshot.MeetsLowSpeedAssistPolicyConditions, Is.False);
        Assert.That(snapshot.RemainingRequestedLowSpeedAssistVelocityBudget, Is.Zero);
    }
}
