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

    internal sealed class RunEndFlow : IInitializable, IFixedTickable, IDisposable, IRunEndCandidateReceiver, IRunResultNotifier,
        IRunResultAcknowledgeCommand
    {
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly IRigidbodyContactNotifier _contactNotifier;
        private readonly IRunContactClassifier _contactClassifier;
        private readonly IRunProgressService _progressService;
        private readonly IRunMotionSource _motionSource;
        private readonly IRunCurrencyAccumulator _runCurrencyAccumulator;
        private readonly RunRewardBreakdownBuilder _runRewardBreakdownBuilder;
        private readonly IRunAirTimeSource _runAirTimeSource;
        private readonly IRunEndConfig _config;
        private readonly ITime _clock;
        private readonly GameplayStateId _restartStateId;
        private readonly GameplayStateId _runningStateId;
        private readonly GameplayStateId _runEndedStateId;
        private readonly List<RunEndCandidate> _pendingCandidates = new();

        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasLaunchApplied;
        private bool _hasAcceptedResult;
        private bool _isAwaitingAcknowledgement;
        private float _elapsedSinceLaunch;
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

        void IFixedTickable.FixedTick()
        {
            if (_isDisposed)
                return;

            var fixedDeltaTime = Math.Max(0f, _clock.FixedDeltaTime);

            if (_isAwaitingAcknowledgement)
            {
                TickRunEndedAcknowledgementGuard(fixedDeltaTime);
                return;
            }

            if (!_hasLaunchApplied || !_gameplayStateService.IsCurrent(_runningStateId))
            {
                _pendingCandidates.Clear();
                return;
            }

            _elapsedSinceLaunch += fixedDeltaTime;
            ResolvePendingCandidates();
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_isInitialized)
            {
                _launchAppliedNotifier.LaunchApplied -= OnSlingshotLaunchApplied;
                _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
                _contactNotifier.CollisionEntered -= OnCollisionEntered;
                _contactNotifier.TriggerEntered -= OnTriggerEntered;
            }

            ResetRunEndState();
        }

        void IRunEndCandidateReceiver.SubmitCandidate(RunEndCandidate candidate)
        {
            if (_isDisposed
                || _hasAcceptedResult
                || !_hasLaunchApplied
                || !_gameplayStateService.IsCurrent(_runningStateId))
            {
                return;
            }

            _pendingCandidates.Add(candidate);
        }

        bool IRunResultAcknowledgeCommand.TryAcknowledge()
        {
            if (_isDisposed)
                return false;

            if (!_isAwaitingAcknowledgement || !_hasAcceptedResult || !_gameplayStateService.IsCurrent(_runEndedStateId))
                return false;

            if (_runEndedElapsed < _config.RunEndedAcknowledgeGuardDuration)
                return false;

            _isAwaitingAcknowledgement = false;
            return _gameplayStateService.TryTransitionTo(_restartStateId);
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
            _pendingCandidates.Clear();
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (ReferenceEquals(nextStateId, _runningStateId))
                return;

            _pendingCandidates.Clear();

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

        private void ResolvePendingCandidates()
        {
            if (_pendingCandidates.Count <= 0 || _hasAcceptedResult)
                return;

            var candidate = GetHighestPriorityCandidate();
            _pendingCandidates.Clear();
            AcceptCandidate(candidate);
        }

        private RunEndCandidate GetHighestPriorityCandidate()
        {
            var bestCandidate = _pendingCandidates[0];
            var bestPriority = GetPriority(bestCandidate.Reason);

            for (var candidateIndex = 1; candidateIndex < _pendingCandidates.Count; candidateIndex += 1)
            {
                var candidate = _pendingCandidates[candidateIndex];
                var priority = GetPriority(candidate.Reason);

                if (priority <= bestPriority)
                    continue;

                bestCandidate = candidate;
                bestPriority = priority;
            }

            return bestCandidate;
        }

        private void AcceptCandidate(RunEndCandidate candidate)
        {
            var finalPosition = _motionSource.Position;
            var finalVelocity = _motionSource.LinearVelocity;
            var distanceTravelled = ResolveDistanceTravelled(finalPosition);

            var result = CreateRunResult(candidate, finalPosition, finalVelocity, distanceTravelled);

            if (!_gameplayStateService.TryTransitionTo(_runEndedStateId))
                return;

            _hasAcceptedResult = true;
            _isAwaitingAcknowledgement = true;
            _runEndedElapsed = 0f;
            RunResultAccepted?.InvokeSafely(result);
            Debug.Log(result.ToString());
        }

        private float ResolveDistanceTravelled(Vector3 finalPosition)
        {
            var progressSample = _progressService.CurrentSample;

            if (progressSample.HasValidSnapshot)
            {
                var finalForwardProgress = progressSample.Snapshot.GetForwardProgress(finalPosition);

                return progressSample.MaximumForwardProgress > finalForwardProgress
                    ? progressSample.MaximumForwardProgress
                    : finalForwardProgress;
            }

            Debug.LogError(
                "Run End Flow accepted degraded Run Result because the Run Progress Frame snapshot is invalid. "
                + progressSample.SnapshotError);

            return 0f;
        }

        private RunResult CreateRunResult(RunEndCandidate candidate, Vector3 finalPosition, Vector3 finalVelocity, float distanceTravelled)
        {
            var finalSpeed = finalVelocity.magnitude;

            var rewardBreakdown = _runRewardBreakdownBuilder.Build(new RunRewardContributorContext(
                candidate.Reason,
                _elapsedSinceLaunch,
                distanceTravelled,
                finalPosition,
                finalSpeed,
                _runAirTimeSource.CurrentRunAirTimeSeconds));

            return new RunResult(
                candidate.Reason,
                _elapsedSinceLaunch,
                distanceTravelled,
                finalPosition,
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
            _pendingCandidates.Clear();
        }
    }
}
