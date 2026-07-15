using System;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Utils.Invocation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public interface IRunEndCandidateReceiver
    {
        void SubmitCandidate(RunEndCandidate candidate);
    }

    public interface IRunResultNotifier
    {
        event Action<RunResult> RunResultAccepted;
    }

    public interface IRunResultAcknowledgeCommand
    {
        bool TryAcknowledge();
    }

    internal interface IRunEndFixedStep
    {
        RunEndFixedStepResult ResolveRunEnd();
    }

    internal sealed class RunEndFlow : IInitializable, IRunEndFixedStep, IDisposable, IRunEndCandidateReceiver, IRunResultNotifier,
        IRunResultAcknowledgeCommand
    {
        private readonly ITime _clock;
        private readonly IRunEndConfig _config;
        private readonly IRunContactClassifier _contactClassifier;
        private readonly IRigidbodyContactNotifier _contactNotifier;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly IRunMotionSource _motionSource;
        private readonly List<RunEndObservation> _pendingObservations = new();
        private readonly IRunProgressService _progressService;
        private readonly GameplayStateId _restartStateId;
        private readonly IRunAirTimeSource _runAirTimeSource;
        private readonly IRunCurrencyAccumulator _runCurrencyAccumulator;
        private readonly GameplayStateId _runEndedStateId;
        private readonly GameplayStateId _runningStateId;
        private readonly RunRewardBreakdownBuilder _runRewardBreakdownBuilder;
        private float _elapsedSinceLaunch;
        private bool _hasAcceptedResult;
        private bool _hasLaunchApplied;
        private bool _isAwaitingAcknowledgement;
        private bool _isDisposed;

        private bool _isInitialized;
        private RunResult? _latchedResult;
        private float _runEndedElapsed;

        public event Action<RunResult> RunResultAccepted;

        public RunEndFlow(
            IGameplayStateService gameplayStateService,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            IRigidbodyContactNotifier contactNotifier,
            IRunContactClassifier contactClassifier,
            IRunProgressService progressService,
            IRunMotionSource motionSource,
            IRunCurrencyAccumulator runCurrencyAccumulator,
            RunRewardBreakdownBuilder runRewardBreakdownBuilder,
            IRunAirTimeSource runAirTimeSource,
            IRunEndConfig config,
            ITime clock,
            [Key(InjectKey.GameplayStateId.RunPreparation)]
            GameplayStateId restartStateId,
            [Key(InjectKey.GameplayStateId.Running)]
            GameplayStateId runningStateId,
            [Key(InjectKey.GameplayStateId.RunEnded)]
            GameplayStateId runEndedStateId)
        {
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _contactNotifier = contactNotifier ?? throw new ArgumentNullException(nameof(contactNotifier));
            _contactClassifier = contactClassifier ?? throw new ArgumentNullException(nameof(contactClassifier));
            _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            _motionSource = motionSource ?? throw new ArgumentNullException(nameof(motionSource));
            _runCurrencyAccumulator = runCurrencyAccumulator ?? throw new ArgumentNullException(nameof(runCurrencyAccumulator));
            _runRewardBreakdownBuilder = runRewardBreakdownBuilder ?? throw new ArgumentNullException(nameof(runRewardBreakdownBuilder));
            _runAirTimeSource = runAirTimeSource ?? throw new ArgumentNullException(nameof(runAirTimeSource));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _restartStateId = restartStateId != null ? restartStateId : throw new ArgumentNullException(nameof(restartStateId));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
            _runEndedStateId = runEndedStateId != null ? runEndedStateId : throw new ArgumentNullException(nameof(runEndedStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunEndFlow));

            if (_isInitialized)
                return;

            _launchAppliedNotifier.LaunchApplied += OnSlingshotLaunchApplied;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _contactNotifier.CollisionEntered += OnCollisionEntered;
            _contactNotifier.TriggerEntered += OnTriggerEntered;
            _isInitialized = true;
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _launchAppliedNotifier.LaunchApplied -= OnSlingshotLaunchApplied;
            _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
            _contactNotifier.CollisionEntered -= OnCollisionEntered;
            _contactNotifier.TriggerEntered -= OnTriggerEntered;

            ResetRunEndState();
        }

        void IRunEndCandidateReceiver.SubmitCandidate(RunEndCandidate candidate)
        {
            if (_isDisposed
                || _hasAcceptedResult
                || _latchedResult.HasValue
                || !_hasLaunchApplied
                || !_gameplayStateService.IsCurrent(_runningStateId))
            {
                return;
            }

            _pendingObservations.Add(
                new RunEndObservation(
                    candidate,
                    _elapsedSinceLaunch,
                    _motionSource.Position,
                    _motionSource.LinearVelocity,
                    _progressService.CurrentSample,
                    _runAirTimeSource.CurrentRunAirTimeSeconds));
        }

        RunEndFixedStepResult IRunEndFixedStep.ResolveRunEnd()
        {
            if (_isDisposed)
                return RunEndFixedStepResult.ContinueRunSteps;

            var fixedDeltaTime = Math.Max(val1: 0f, _clock.FixedDeltaTime);

            if (_isAwaitingAcknowledgement)
            {
                TickRunEndedAcknowledgementGuard(fixedDeltaTime);
                return RunEndFixedStepResult.BlockRemainingRunSteps;
            }

            if (!_hasLaunchApplied || !_gameplayStateService.IsCurrent(_runningStateId))
            {
                ClearPendingTerminalState();
                return RunEndFixedStepResult.ContinueRunSteps;
            }

            if (_latchedResult.HasValue || _pendingObservations.Count > 0)
            {
                ResolvePendingObservations();
                return RunEndFixedStepResult.BlockRemainingRunSteps;
            }

            _elapsedSinceLaunch += fixedDeltaTime;
            return RunEndFixedStepResult.ContinueRunSteps;
        }

        bool IRunResultAcknowledgeCommand.TryAcknowledge()
        {
            if (_isDisposed)
                return false;

            if (!_isAwaitingAcknowledgement || !_hasAcceptedResult || !_gameplayStateService.IsCurrent(_runEndedStateId))
                return false;

            if (_runEndedElapsed < _config.RunEndedAcknowledgeGuardDuration)
                return false;

            if (!_gameplayStateService.TryTransitionTo(_restartStateId))
                return false;

            _isAwaitingAcknowledgement = false;
            return true;
        }

        private void OnSlingshotLaunchApplied(SlingshotLaunchAppliedEvent launchApplied)
        {
            if (_isDisposed)
                return;

            _hasLaunchApplied = true;
            _hasAcceptedResult = false;
            _isAwaitingAcknowledgement = false;
            _elapsedSinceLaunch = 0f;
            _runEndedElapsed = 0f;
            ClearPendingTerminalState();
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (ReferenceEquals(nextStateId, _runningStateId))
                return;

            ClearPendingTerminalState();

            if (ReferenceEquals(nextStateId, _restartStateId))
            {
                _runCurrencyAccumulator.Reset();
                ResetRunEndState();
            }
        }

        private void OnCollisionEntered(RigidbodyCollisionNotification notification)
        {
            if (_contactClassifier.TryClassify(notification, out var candidate))
                ((IRunEndCandidateReceiver)this).SubmitCandidate(candidate);
        }

        private void OnTriggerEntered(RigidbodyTriggerNotification notification)
        {
            if (_contactClassifier.TryClassify(notification, out var candidate))
                ((IRunEndCandidateReceiver)this).SubmitCandidate(candidate);
        }

        private void ResolvePendingObservations()
        {
            if (!_latchedResult.HasValue)
            {
                var observation = GetHighestPriorityObservation();
                _pendingObservations.Clear();
                _latchedResult = CreateRunResult(observation);
            }

            TryAcceptLatchedResult();
        }

        private RunEndObservation GetHighestPriorityObservation()
        {
            var bestObservation = _pendingObservations[index: 0];
            var bestPriority = GetPriority(bestObservation.Candidate.Reason);

            for (var observationIndex = 1; observationIndex < _pendingObservations.Count; observationIndex += 1)
            {
                var observation = _pendingObservations[observationIndex];
                var priority = GetPriority(observation.Candidate.Reason);

                if (priority <= bestPriority)
                    continue;

                bestObservation = observation;
                bestPriority = priority;
            }

            return bestObservation;
        }

        private void TryAcceptLatchedResult()
        {
            var result = _latchedResult.GetValueOrDefault();

            if (!_gameplayStateService.TryTransitionTo(_runEndedStateId))
                return;

            _hasAcceptedResult = true;
            _isAwaitingAcknowledgement = true;
            _runEndedElapsed = 0f;
            RunResultAccepted?.InvokeSafely(result);
            Debug.Log(result.ToString());
        }

        private float ResolveDistanceTravelled(RunEndObservation observation)
        {
            var progressSample = observation.ProgressSample;

            if (progressSample.HasValidSnapshot)
            {
                var finalForwardProgress = progressSample.Snapshot.GetForwardProgress(observation.Position);

                return progressSample.MaximumForwardProgress > finalForwardProgress
                    ? progressSample.MaximumForwardProgress
                    : finalForwardProgress;
            }

            Debug.LogError(
                "Run End Flow accepted degraded Run Result because the Run Progress Frame snapshot is invalid. "
                + progressSample.SnapshotError);

            return 0f;
        }

        private RunResult CreateRunResult(RunEndObservation observation)
        {
            var distanceTravelled = ResolveDistanceTravelled(observation);
            var finalSpeed = observation.LinearVelocity.magnitude;

            var rewardBreakdown = _runRewardBreakdownBuilder.Build(
                new RunRewardContributorContext(
                    observation.Candidate.Reason,
                    observation.ElapsedTime,
                    distanceTravelled,
                    observation.Position,
                    finalSpeed,
                    observation.AirTimeSeconds));

            return new RunResult(
                observation.Candidate.Reason,
                observation.ElapsedTime,
                distanceTravelled,
                observation.Position,
                finalSpeed,
                rewardBreakdown);
        }

        private void TickRunEndedAcknowledgementGuard(float fixedDeltaTime)
        {
            if (!_gameplayStateService.IsCurrent(_runEndedStateId))
            {
                _isAwaitingAcknowledgement = false;
                return;
            }

            _runEndedElapsed += fixedDeltaTime;
        }

        private void ClearPendingTerminalState()
        {
            _pendingObservations.Clear();
            _latchedResult = null;
        }

        private static int GetPriority(RunEndReason reason)
        {
            return reason switch
            {
                RunEndReason.Finished => 3,
                RunEndReason.ObstacleHit => 2,
                RunEndReason.OutOfBounds => 1,
                RunEndReason.LostMomentum => 0,
                _ => -1
            };
        }

        private void ResetRunEndState()
        {
            _hasLaunchApplied = false;
            _hasAcceptedResult = false;
            _isAwaitingAcknowledgement = false;
            _elapsedSinceLaunch = 0f;
            _runEndedElapsed = 0f;
            ClearPendingTerminalState();
        }
    }
}
