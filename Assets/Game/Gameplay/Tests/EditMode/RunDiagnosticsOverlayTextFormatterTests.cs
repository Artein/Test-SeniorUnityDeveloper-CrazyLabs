using Game.Gameplay;
using Game.Gameplay.Diagnostics;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunDiagnosticsOverlayTextFormatterTests
{
    private readonly RunDiagnosticsOverlayTextFormatter _formatter = new();

    [Test]
    public void FormatMotionSummary_SurfaceSnapshot_UsesExplicitPolicyTerminology()
    {
        var sample = CreateSurfaceSample(
            RunSupportObservationState.Supported,
            isStableGrounded: true,
            RunSurfaceTransition.ConfirmedDiscontinuity,
            isMissingSupportHeld: true,
            isConfirmingDiscontinuity: true,
            isSteeringFrameValid: true);

        var text = _formatter.FormatMotionSummary(sample);

        Assert.That(text, Does.Contain("observed:Supported"));
        Assert.That(text, Does.Contain("stable:grounded"));
        Assert.That(text, Does.Contain("transition:ConfirmedDiscontinuity"));
        Assert.That(text, Does.Contain("held:yes"));
        Assert.That(text, Does.Contain("confirming:yes"));
        Assert.That(text, Does.Contain("steering:valid"));
        Assert.That(text, Does.Contain("observed:Supported normal:(0.000,1.000,0.000)"));
        Assert.That(text, Does.Contain("stable:grounded normal:(0.000,1.000,0.000)"));
        Assert.That(text, Does.Contain("steering:valid up:(0.000,0.000,1.000)"));
    }

    [TestCase(RunSupportObservationState.Unavailable)]
    [TestCase(RunSupportObservationState.Missing)]
    [TestCase(RunSupportObservationState.Supported)]
    public void FormatMotionSummary_ObservedState_ReportsEveryState(RunSupportObservationState observedState)
    {
        var text = _formatter.FormatMotionSummary(CreateSurfaceSample(observedState));

        Assert.That(text, Does.Contain($"observed:{observedState}"));
    }

    [TestCase(RunSurfaceTransition.None)]
    [TestCase(RunSurfaceTransition.ContinuousUpdate)]
    [TestCase(RunSurfaceTransition.SupportAcquired)]
    [TestCase(RunSurfaceTransition.SupportLost)]
    [TestCase(RunSurfaceTransition.ConfirmedDiscontinuity)]
    [TestCase(RunSurfaceTransition.HardReset)]
    public void FormatMotionSummary_Transition_ReportsEveryTransition(RunSurfaceTransition transition)
    {
        var text = _formatter.FormatMotionSummary(CreateSurfaceSample(surfaceTransition: transition));

        Assert.That(text, Does.Contain($"transition:{transition}"));
    }

    [Test]
    public void FormatMotionSummary_UnavailableSnapshot_ReportsAbsentDirectionsAndFalseFlags()
    {
        var text = _formatter.FormatMotionSummary(CreateSurfaceSample());

        Assert.That(text, Does.Contain("observed:Unavailable normal:n/a"));
        Assert.That(text, Does.Contain("stable:unsupported normal:n/a"));
        Assert.That(text, Does.Contain("held:no"));
        Assert.That(text, Does.Contain("confirming:no"));
        Assert.That(text, Does.Contain("steering:unavailable up:n/a"));
    }

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

    private RunDiagnosticsOverlaySample CreateSurfaceSample(
        RunSupportObservationState observedSupportState = RunSupportObservationState.Unavailable,
        bool isStableGrounded = false,
        RunSurfaceTransition surfaceTransition = RunSurfaceTransition.None,
        bool isMissingSupportHeld = false,
        bool isConfirmingDiscontinuity = false,
        bool isSteeringFrameValid = false)
    {
        return new RunDiagnosticsOverlaySample(
            speedMetersPerSecond: 0f,
            motionStepMetersPerSecond: 0f,
            visualTargetStepMetersPerSecond: 0f,
            visualTargetStepMeters: 0f,
            observedGroundNormalDeltaDegrees: 0f,
            steeringUpDeltaDegrees: 0f,
            visualLagCentimeters: 0f,
            cameraStepMetersPerSecond: 0f,
            targetToMotionCentimeters: 0f,
            visualTargetRotationDeltaDegrees: 0f,
            visualRotationDeltaDegrees: 0f,
            cameraRotationDeltaDegrees: 0f,
            estimatedVisualSnapReason: RunDiagnosticsOverlaySnapReason.None,
            fixedStepsThisFrame: 1,
            CreateSurfaceFrame(
                observedSupportState,
                isStableGrounded,
                surfaceTransition,
                isMissingSupportHeld,
                isConfirmingDiscontinuity,
                isSteeringFrameValid),
            speedDiagnostics: default);
    }

    private RunSurfaceFrameSnapshot CreateSurfaceFrame(
        RunSupportObservationState observedSupportState,
        bool isStableGrounded,
        RunSurfaceTransition surfaceTransition,
        bool isMissingSupportHeld,
        bool isConfirmingDiscontinuity,
        bool isSteeringFrameValid)
    {
        RunProgressFrameSnapshot.TryCreate(
            Vector3.zero,
            Vector3.forward,
            Vector3.up,
            out var progressFrame,
            out _);

        var observedContext = observedSupportState == RunSupportObservationState.Supported
            ? new RunSurfaceContext(true, Vector3.up, 0f)
            : new RunSurfaceContext(false, Vector3.up, 0f);

        var observation = new RunSupportObservation(
            observedSupportState,
            observedSupportState == RunSupportObservationState.Unavailable ? default : progressFrame,
            observedContext,
            0f);

        return new RunSurfaceFrameSnapshot(
            observation,
            new RunSurfaceContext(isStableGrounded, Vector3.up, 0f),
            surfaceTransition,
            isMissingSupportHeld,
            isConfirmingDiscontinuity,
            new RunSteeringFrameSnapshot(isSteeringFrameValid, Vector3.forward));
    }
}
