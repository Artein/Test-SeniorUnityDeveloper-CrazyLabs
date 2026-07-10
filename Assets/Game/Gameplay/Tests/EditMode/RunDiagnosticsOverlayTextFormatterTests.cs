using Game.Gameplay;
using Game.Gameplay.Diagnostics;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public sealed class RunDiagnosticsOverlayTextFormatterTests
{
    private readonly RunDiagnosticsOverlayTextFormatter _formatter = new();

    [Test]
    public void FormatRunBodySpeed_InactiveSnapshot_ReportsInactiveState()
    {
        var text = _formatter.FormatRunBodySpeed(default);

        Assert.That(text, Is.EqualTo("Run Body Speed | state:inactive"));
    }

    [Test]
    public void FormatRunBodySpeed_ActiveSnapshot_ReportsPolicyAndRequestedContributorsSeparately()
    {
        var snapshot = CreateSnapshot(
            policyContributors: RunBodySpeedDecisionContributors.DownhillAcceleration
                                | RunBodySpeedDecisionContributors.SurfaceSlowdown
                                | RunBodySpeedDecisionContributors.AboveEnvelopeResistance
                                | RunBodySpeedDecisionContributors.LowSpeedAssist,
            requestedContributors: RunBodySpeedDecisionContributors.SurfaceSlowdown
                                   | RunBodySpeedDecisionContributors.LowSpeedAssist);

        var text = _formatter.FormatRunBodySpeed(snapshot);

        Assert.That(text, Does.Contain("grounded:yes"));
        Assert.That(text, Does.Contain("support:valid"));
        Assert.That(text, Does.Contain("direction:valid"));
        Assert.That(text, Does.Contain("speed:12.5/20.0m/s"));
        Assert.That(text, Does.Contain("downhill:28.0deg"));
        Assert.That(text, Does.Contain("align:0.75"));
        Assert.That(text, Does.Contain("policy:downhill+slowdown+above-envelope+low-speed-assist"));
        Assert.That(text, Does.Contain("requested:slowdown+low-speed-assist"));
    }

    [Test]
    public void FormatRunBodySpeed_UnsupportedDirectionlessSnapshot_ReportsUnavailableFacts()
    {
        var snapshot = new RunBodySpeedDiagnosticsSnapshot(
            RunBodySpeedDiagnosticsState.Active,
            isRunSurfaceGrounded: false,
            hasValidGroundedRunSurface: false,
            hasUsableTangentDirection: false,
            sampledTangentSpeed: 0f,
            effectiveSoftMaximumSpeed: 20f,
            forwardDownhillDegrees: 0f,
            courseForwardAlignment: 0f,
            policyContributors: RunBodySpeedDecisionContributors.None,
            requestedContributors: RunBodySpeedDecisionContributors.None,
            requestedLowSpeedAssistVelocityDelta: 0f,
            effectiveLowSpeedAssistTargetSpeed: 0f,
            lowSpeedAssistAttemptState: RunBodyLowSpeedAssistAttemptState.Unavailable,
            meetsLowSpeedAssistPolicyConditions: false,
            remainingRequestedLowSpeedAssistVelocityBudget: 0f);

        var text = _formatter.FormatRunBodySpeed(snapshot);

        Assert.That(text, Does.Contain("grounded:no"));
        Assert.That(text, Does.Contain("support:invalid"));
        Assert.That(text, Does.Contain("direction:unavailable"));
        Assert.That(text, Does.Contain("policy:none"));
        Assert.That(text, Does.Contain("requested:none"));
    }

    [Test]
    public void FormatLowSpeedAssist_ActiveSnapshot_ReportsAttemptStatePolicyConditionsRequestAndBudget()
    {
        var text = _formatter.FormatLowSpeedAssist(CreateSnapshot());

        Assert.That(
            text,
            Is.EqualTo(
                "Low-Speed Assist | target:5.0m/s state:Active conditions:yes request:+1.0m/s budget:2.5m/s"));
    }

    [Test]
    public void FormatLowSpeedAssist_InactiveSnapshot_ReportsUnavailableState()
    {
        var text = _formatter.FormatLowSpeedAssist(default);

        Assert.That(text, Is.EqualTo("Low-Speed Assist | state:unavailable"));
    }

    private RunBodySpeedDiagnosticsSnapshot CreateSnapshot(
        RunBodySpeedDecisionContributors policyContributors = RunBodySpeedDecisionContributors.LowSpeedAssist,
        RunBodySpeedDecisionContributors requestedContributors = RunBodySpeedDecisionContributors.LowSpeedAssist)
    {
        return new RunBodySpeedDiagnosticsSnapshot(
            RunBodySpeedDiagnosticsState.Active,
            isRunSurfaceGrounded: true,
            hasValidGroundedRunSurface: true,
            hasUsableTangentDirection: true,
            sampledTangentSpeed: 12.5f,
            effectiveSoftMaximumSpeed: 20f,
            forwardDownhillDegrees: 28f,
            courseForwardAlignment: 0.75f,
            policyContributors,
            requestedContributors,
            requestedLowSpeedAssistVelocityDelta: 1f,
            effectiveLowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAttemptState: RunBodyLowSpeedAssistAttemptState.Active,
            meetsLowSpeedAssistPolicyConditions: true,
            remainingRequestedLowSpeedAssistVelocityBudget: 2.5f);
    }
}
