using System;
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
    public void FixedTick_FinishCandidate_LogsSuccessfulResultAndTransitionsToRunEnded()
    {
        ActivateRun();
        LogAssert.Expect(LogType.Log, new Regex(pattern: "Run Result: Reason=Finished, IsSuccess=True, ElapsedTime=0, DistanceTravelled=12.5"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        var fixedStepResult = ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runEndedStateId));
        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId }));
        Assert.That(fixedStepResult, Is.EqualTo(RunEndFixedStepResult.BlockRemainingRunSteps));
    }

    [Test]
    public void FixedTick_AcceptedResult_NotifiesRunResultOnce()
    {
        ActivateRun();
        var acceptedResults = new List<RunResult>();
        _flow.RunResultAccepted += acceptedResults.Add;
        LogAssert.Expect(LogType.Log, new Regex(pattern: "Run Result: Reason=Finished, IsSuccess=True"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(acceptedResults, Has.Count.EqualTo(expected: 1));
        Assert.That(acceptedResults[index: 0].Reason, Is.EqualTo(RunEndReason.Finished));
        Assert.That(acceptedResults[index: 0].IsSuccess, Is.True);
    }

    [Test]
    public void FixedTick_InvalidProgressSnapshot_NotifiesDegradedRunResultAndCanAcknowledge()
    {
        ActivateRun();
        _progressService.HasValidSnapshot = false;
        _progressService.SnapshotError = "bad frame";
        _motionSource.Position = new Vector3(x: 2f, y: 3f, z: 4f);
        _motionSource.LinearVelocity = new Vector3(x: 1f, y: 2f, z: 2f);
        _runAirTimeSource.CurrentRunAirTimeSeconds = 5f;
        ((IRunCurrencyAccumulator)_runCurrencyAccumulator).Grant(_runRewardSourceCatalog.PickedUpCoins, _coins, amount: 7);
        var acceptedResults = new List<RunResult>();
        _flow.RunResultAccepted += acceptedResults.Add;
        LogAssert.Expect(LogType.Error, new Regex(pattern: "Run End Flow accepted degraded Run Result.*bad frame"));
        LogAssert.Expect(LogType.Log, new Regex(pattern: "Run Result: Reason=Finished, IsSuccess=True, ElapsedTime=0, DistanceTravelled=0"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runEndedStateId));
        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId }));
        Assert.That(acceptedResults, Has.Count.EqualTo(expected: 1));
        Assert.That(acceptedResults[index: 0].Reason, Is.EqualTo(RunEndReason.Finished));
        Assert.That(acceptedResults[index: 0].IsSuccess, Is.True);
        Assert.That(acceptedResults[index: 0].ElapsedTime, Is.Zero.Within(amount: 0.001f));
        Assert.That(acceptedResults[index: 0].DistanceTravelled, Is.Zero);
        Assert.That(acceptedResults[index: 0].FinalPosition, Is.EqualTo(_motionSource.Position));
        Assert.That(acceptedResults[index: 0].FinalSpeed, Is.EqualTo(expected: 3f).Within(amount: 0.001f));
        Assert.That(acceptedResults[index: 0].CurrencySnapshot.GetAmount(_coins), Is.EqualTo(expected: 7));

        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(((IRunResultAcknowledgeCommand)_flow).TryAcknowledge(), Is.True);
        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runPreparationStateId));
        Assert.That(_stateService.RequestedStateIds, Is.EqualTo(new[] { _runEndedStateId, _runPreparationStateId }));
    }

    [Test]
    public void FixedTick_UnsubscribedRunResultHandler_IsNotNotified()
    {
        ActivateRun();
        var notificationCount = 0;
        Action<RunResult> handler = _ => notificationCount += 1;
        _flow.RunResultAccepted += handler;
        _flow.RunResultAccepted -= handler;

        LogAssert.Expect(LogType.Log, new Regex(pattern: "Run Result: Reason=Finished"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(notificationCount, Is.EqualTo(expected: 0));
    }

    [Test]
    public void FixedTick_FinishCandidate_PublishesRunResultWithCurrencySnapshot()
    {
        ActivateRun();
        ((IRunCurrencyAccumulator)_runCurrencyAccumulator).Grant(_runRewardSourceCatalog.PickedUpCoins, _coins, amount: 7);
        RunResult? acceptedResult = null;
        _flow.RunResultAccepted += result => acceptedResult = result;
        LogAssert.Expect(LogType.Log, new Regex(pattern: "Run Result: Reason=Finished"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();

        Assert.That(acceptedResult.HasValue, Is.True);
        Assert.That(acceptedResult.Value.CurrencySnapshot.GetAmount(_coins), Is.EqualTo(expected: 7));
    }

    [Test]
    public void GameplayStateChanged_EnteringRunPreparation_ResetsAccumulatorWithoutMutatingAcceptedResult()
    {
        ActivateRun();
        ((IRunCurrencyAccumulator)_runCurrencyAccumulator).Grant(_runRewardSourceCatalog.PickedUpCoins, _coins, amount: 7);
        RunResult? acceptedResult = null;
        _flow.RunResultAccepted += result => acceptedResult = result;
        LogAssert.Expect(LogType.Log, new Regex(pattern: "Run Result: Reason=Finished"));

        ((IRunEndCandidateReceiver)_flow).SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        ((IRunEndFixedStep)_flow).ResolveRunEnd();
        Assert.That(((IRunResultAcknowledgeCommand)_flow).TryAcknowledge(), Is.True);

        Assert.That(_stateService.CurrentStateId, Is.SameAs(_runPreparationStateId));
        Assert.That(((IRunCurrencyAccumulator)_runCurrencyAccumulator).CreateSnapshot().GetAmount(_coins), Is.Zero);
        Assert.That(acceptedResult.HasValue, Is.True);
        Assert.That(acceptedResult.Value.CurrencySnapshot.GetAmount(_coins), Is.EqualTo(expected: 7));
    }
}
