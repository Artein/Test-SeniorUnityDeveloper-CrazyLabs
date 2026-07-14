using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Gameplay;
using Game.Gameplay.Economy;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed partial class RunEndFlowTests
{
    [Test]
    public void FixedTick_SameTickCandidates_ResolveByPriority()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.LostMomentum));
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.OutOfBounds));
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runEndedStateId));
    }

    [Test]
    public void FixedTick_CandidateSubmission_CapturesMotionProgressElapsedTimeAndAirTime()
    {
        ActivateRun();
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        var submittedPosition = new Vector3(5f, 2f, 8f);
        var submittedVelocity = new Vector3(3f, 0f, 4f);
        _motionSource.Position = submittedPosition;
        _motionSource.LinearVelocity = submittedVelocity;
        _progressService.MaximumForwardProgress = 6f;
        _runAirTimeSource.CurrentRunAirTimeSeconds = 2.5f;
        var acceptedResults = new List<RunResult>();
        ((IRunResultNotifier)_flow).RunResultAccepted += acceptedResults.Add;

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));

        _motionSource.Position = new Vector3(0f, 0f, 100f);
        _motionSource.LinearVelocity = new Vector3(0f, 0f, 30f);
        _progressService.MaximumForwardProgress = 99f;
        _runAirTimeSource.CurrentRunAirTimeSeconds = 9f;
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=ObstacleHit"));

        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(acceptedResults, Has.Count.EqualTo(1));
        Assert.That(acceptedResults[0].ElapsedTime, Is.EqualTo(0.1f).Within(0.001f));
        Assert.That(acceptedResults[0].FinalPosition, Is.EqualTo(submittedPosition));
        Assert.That(acceptedResults[0].FinalSpeed, Is.EqualTo(5f).Within(0.001f));
        Assert.That(acceptedResults[0].DistanceTravelled, Is.EqualTo(8f).Within(0.001f));
        Assert.That(_capturingRewardContributor.LastContext.HasValue, Is.True);
        Assert.That(_capturingRewardContributor.LastContext.Value.AirTimeSeconds, Is.EqualTo(2.5f).Within(0.001f));
    }

    [Test]
    public void FixedTick_HigherPriorityCandidate_UsesItsOwnObservation()
    {
        ActivateRun();
        _motionSource.Position = new Vector3(0f, 0f, 4f);
        _motionSource.LinearVelocity = Vector3.forward;
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.LostMomentum));

        var winningPosition = new Vector3(0f, 0f, 7f);
        _motionSource.Position = winningPosition;
        _motionSource.LinearVelocity = new Vector3(6f, 0f, 8f);
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));

        _motionSource.Position = new Vector3(0f, 0f, 30f);
        _motionSource.LinearVelocity = Vector3.forward * 30f;
        RunResult? acceptedResult = null;
        ((IRunResultNotifier)_flow).RunResultAccepted += result => acceptedResult = result;
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished"));

        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(acceptedResult.HasValue, Is.True);
        Assert.That(acceptedResult.Value.FinalPosition, Is.EqualTo(winningPosition));
        Assert.That(acceptedResult.Value.FinalSpeed, Is.EqualTo(10f).Within(0.001f));
    }

    [Test]
    public void FixedTick_EqualPriorityCandidates_UsesFirstObservation()
    {
        ActivateRun();
        var firstPosition = new Vector3(0f, 0f, 5f);
        _motionSource.Position = firstPosition;
        _motionSource.LinearVelocity = Vector3.forward * 5f;
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));

        _motionSource.Position = new Vector3(0f, 0f, 9f);
        _motionSource.LinearVelocity = Vector3.forward * 9f;
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));

        RunResult? acceptedResult = null;
        ((IRunResultNotifier)_flow).RunResultAccepted += result => acceptedResult = result;
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=ObstacleHit"));

        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(acceptedResult.HasValue, Is.True);
        Assert.That(acceptedResult.Value.FinalPosition, Is.EqualTo(firstPosition));
        Assert.That(acceptedResult.Value.FinalSpeed, Is.EqualTo(5f).Within(0.001f));
    }

    [Test]
    public void FixedTick_AfterAcceptedResult_IgnoresLaterCandidatesUntilNextLaunch()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId }));
    }

    [Test]
    public void LaunchApplied_AfterPreviousResult_AllowsNewResult()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished"));
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        _stateService.ChangeTo(_runPreparationStateId);

        _stateService.ChangeTo(_runningStateId);
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent());
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=ObstacleHit"));
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId, _runEndedStateId }));
    }

    [Test]
    public void FixedTick_RunEndedTransitionRejected_RetriesSameFrozenResultAndIgnoresLaterCandidates()
    {
        ActivateRun();
        var submittedPosition = new Vector3(0f, 0f, 6f);
        _motionSource.Position = submittedPosition;
        _motionSource.LinearVelocity = new Vector3(3f, 0f, 4f);
        ((IRunCurrencyAccumulator)_runCurrencyAccumulator).Grant(_runRewardSourceCatalog.PickedUpCoins, _coins, 7);
        _stateService.TryTransitionResult = false;
        var acceptedResults = new List<RunResult>();
        ((IRunResultNotifier)_flow).RunResultAccepted += acceptedResults.Add;

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        var firstFixedStepResult = ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runningStateId));
        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId }));
        Assert.That(acceptedResults, Is.Empty);
        Assert.That(((IRunResultAcknowledgeCommand)_flow).TryAcknowledge(), Is.False);
        Assert.That(firstFixedStepResult, Is.EqualTo(RunEndFixedStepResult.BlockRemainingRunSteps));

        _motionSource.Position = new Vector3(0f, 0f, 20f);
        _motionSource.LinearVelocity = Vector3.forward * 20f;
        _runAirTimeSource.CurrentRunAirTimeSeconds = 10f;
        ((IRunCurrencyAccumulator)_runCurrencyAccumulator).Grant(_runRewardSourceCatalog.PickedUpCoins, _coins, 5);
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));
        _stateService.TryTransitionResult = true;
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished"));

        var secondFixedStepResult = ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId, _runEndedStateId }));
        Assert.That(acceptedResults, Has.Count.EqualTo(1));
        Assert.That(acceptedResults[0].Reason, Is.EqualTo(RunEndReason.Finished));
        Assert.That(acceptedResults[0].FinalPosition, Is.EqualTo(submittedPosition));
        Assert.That(acceptedResults[0].FinalSpeed, Is.EqualTo(5f).Within(0.001f));
        Assert.That(acceptedResults[0].CurrencySnapshot.GetAmount(_coins), Is.EqualTo(7));
        Assert.That(secondFixedStepResult, Is.EqualTo(RunEndFixedStepResult.BlockRemainingRunSteps));
    }

    [Test]
    public void TriggerNotification_UsesClassifierAndQueuesCandidate()
    {
        ActivateRun();
        _contactClassifier.TriggerCandidate = new RunEndCandidate(RunEndReason.OutOfBounds);
        LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=OutOfBounds"));

        _contactNotifier.RaiseTrigger(new RigidbodyTriggerNotification(null));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runEndedStateId));
    }
}
