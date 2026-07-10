using Game.Gameplay;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public sealed class RunBodyLowSpeedAssistAttemptTests
{
    private RunBodyLowSpeedAssistAttempt _attempt;

    [SetUp]
    public void SetUp()
    {
        _attempt = new RunBodyLowSpeedAssistAttempt();
        _attempt.RearmForNewRun();
    }

    [Test]
    public void Advance_EligibleAttempt_ApproachesTargetAtAuthoredRate()
    {
        var requestedDelta = _attempt.Advance(CreateContext(
            sampledTangentSpeed: 2f,
            naturallyIntegratedTangentSpeed: 2f,
            targetSpeed: 5f,
            acceleration: 1f,
            fixedDeltaTime: 1f));

        Assert.That(requestedDelta, Is.EqualTo(1f));
        Assert.That(_attempt.Snapshot.State, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Active));
        Assert.That(_attempt.Snapshot.IsEligible, Is.True);
        Assert.That(_attempt.Snapshot.EffectiveTargetSpeed, Is.EqualTo(5f));
        Assert.That(_attempt.Snapshot.RemainingRequestedVelocityBudget, Is.EqualTo(2f));
    }

    [Test]
    public void Advance_RepeatedSolverCancellation_SpendsOnlyInitialDeficit()
    {
        var context = CreateContext(
            sampledTangentSpeed: 2f,
            naturallyIntegratedTangentSpeed: 2f,
            targetSpeed: 5f,
            acceleration: 1f,
            fixedDeltaTime: 1f);

        var totalRequestedDelta = 0f;

        for (var passIndex = 0; passIndex < 5; passIndex++)
        {
            totalRequestedDelta += _attempt.Advance(context);
        }

        Assert.That(totalRequestedDelta, Is.EqualTo(3f));
        Assert.That(_attempt.Snapshot.State, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Exhausted));
        Assert.That(_attempt.Snapshot.RemainingRequestedVelocityBudget, Is.Zero);
    }

    [Test]
    public void Advance_SupportLoss_PausesAndResumesSameBudget()
    {
        _attempt.Advance(CreateContext(
            sampledTangentSpeed: 2f,
            naturallyIntegratedTangentSpeed: 2f,
            targetSpeed: 5f,
            acceleration: 1f,
            fixedDeltaTime: 1f));

        var requestedWhileUnsupported = _attempt.Advance(CreateContext(
            sampledTangentSpeed: 2f,
            naturallyIntegratedTangentSpeed: 2f,
            hasValidGroundedRunSurface: false,
            targetSpeed: 0f,
            acceleration: 0f,
            fixedDeltaTime: 1f));

        Assert.That(requestedWhileUnsupported, Is.Zero);
        Assert.That(_attempt.Snapshot.State, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Paused));
        Assert.That(_attempt.Snapshot.RemainingRequestedVelocityBudget, Is.EqualTo(2f));

        var resumedRequest = _attempt.Advance(CreateContext(
            sampledTangentSpeed: 2f,
            naturallyIntegratedTangentSpeed: 2f,
            targetSpeed: 5f,
            acceleration: 1f,
            fixedDeltaTime: 1f));

        Assert.That(resumedRequest, Is.EqualTo(1f));
        Assert.That(_attempt.Snapshot.RemainingRequestedVelocityBudget, Is.EqualTo(1f));
    }

    [Test]
    public void Advance_DirectionlessMotion_DoesNotOpenOrSpendAttempt()
    {
        var requestedDelta = _attempt.Advance(CreateContext(
            sampledTangentSpeed: 0f,
            naturallyIntegratedTangentSpeed: 0f,
            hasUsableTangentDirection: false,
            targetSpeed: 5f,
            acceleration: 1f,
            fixedDeltaTime: 1f));

        Assert.That(requestedDelta, Is.Zero);
        Assert.That(_attempt.Snapshot.State, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Unavailable));
        Assert.That(_attempt.Snapshot.RemainingRequestedVelocityBudget, Is.Zero);
    }

    [Test]
    public void Advance_EligibleWithZeroRate_ReportsEligibleWithoutOpeningAttempt()
    {
        var requestedDelta = _attempt.Advance(CreateContext(
            sampledTangentSpeed: 2f,
            naturallyIntegratedTangentSpeed: 2f,
            targetSpeed: 5f,
            acceleration: 0f,
            fixedDeltaTime: 1f));

        Assert.That(requestedDelta, Is.Zero);
        Assert.That(_attempt.Snapshot.State, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Eligible));
        Assert.That(_attempt.Snapshot.RemainingRequestedVelocityBudget, Is.Zero);
    }

    [Test]
    public void Advance_IndependentRecoveryAboveTolerance_RearmsExhaustedAttempt()
    {
        var blockedContext = CreateContext(
            sampledTangentSpeed: 2f,
            naturallyIntegratedTangentSpeed: 2f,
            targetSpeed: 5f,
            acceleration: 3f,
            fixedDeltaTime: 1f);

        _attempt.Advance(blockedContext);

        var atTargetRequest = _attempt.Advance(CreateContext(
            sampledTangentSpeed: 5f,
            naturallyIntegratedTangentSpeed: 5f,
            targetSpeed: 0f,
            acceleration: 0f,
            fixedDeltaTime: 1f));

        var withinToleranceRequest = _attempt.Advance(CreateContext(
            sampledTangentSpeed: 5.0005f,
            naturallyIntegratedTangentSpeed: 5.0005f,
            targetSpeed: 0f,
            acceleration: 0f,
            fixedDeltaTime: 1f));

        Assert.That(atTargetRequest, Is.Zero);
        Assert.That(withinToleranceRequest, Is.Zero);
        Assert.That(_attempt.Snapshot.State, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Exhausted));

        var recoveredRequest = _attempt.Advance(CreateContext(
            sampledTangentSpeed: 5.002f,
            naturallyIntegratedTangentSpeed: 5.002f,
            targetSpeed: 0f,
            acceleration: 0f,
            fixedDeltaTime: 1f));

        Assert.That(recoveredRequest, Is.Zero);
        Assert.That(_attempt.Snapshot.State, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Rearmed));

        var nextEpisodeRequest = _attempt.Advance(blockedContext);

        Assert.That(nextEpisodeRequest, Is.EqualTo(3f));
        Assert.That(_attempt.Snapshot.State, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Exhausted));
    }

    [Test]
    public void Clear_ActiveAttempt_ClearsStateAndNewRunRearms()
    {
        var context = CreateContext(
            sampledTangentSpeed: 2f,
            naturallyIntegratedTangentSpeed: 2f,
            targetSpeed: 5f,
            acceleration: 1f,
            fixedDeltaTime: 1f);

        _attempt.Advance(context);
        _attempt.Clear();

        Assert.That(_attempt.Snapshot.State, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Unavailable));
        Assert.That(_attempt.Snapshot.RemainingRequestedVelocityBudget, Is.Zero);
        Assert.That(_attempt.Advance(context), Is.Zero);

        _attempt.RearmForNewRun();

        Assert.That(_attempt.Snapshot.State, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Rearmed));
        Assert.That(_attempt.Advance(context), Is.EqualTo(1f));
    }

    private RunBodyLowSpeedAssistAttemptContext CreateContext(
        float sampledTangentSpeed,
        float naturallyIntegratedTangentSpeed,
        bool hasValidGroundedRunSurface = true,
        bool hasUsableTangentDirection = true,
        float targetSpeed = 5f,
        float acceleration = 1f,
        float fixedDeltaTime = 0.02f)
    {
        return new RunBodyLowSpeedAssistAttemptContext(
            sampledTangentSpeed,
            naturallyIntegratedTangentSpeed,
            hasValidGroundedRunSurface,
            hasUsableTangentDirection,
            targetSpeed,
            acceleration,
            fixedDeltaTime);
    }
}
