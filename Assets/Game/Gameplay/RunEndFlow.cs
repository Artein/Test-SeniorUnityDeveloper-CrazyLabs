using System;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Pickups;
using Game.Gameplay.Slingshot;
using Game.Utils.Invocation;
using UnityEngine;
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

    internal sealed class RunEndFlow : IInitializable, IFixedTickable, IDisposable, IRunEndCandidateReceiver, IRunResultNotifier
    {
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly IRigidbodyContactNotifier _contactNotifier;
        private readonly IRunContactClassifier _contactClassifier;
        private readonly IRunProgressService _progressService;
        private readonly IRunMotionSource _motionSource;
        private readonly IRunResourceAccumulator _runResourceAccumulator;
        private readonly IRunEndConfig _config;
        private readonly ITime _clock;
        private readonly GameplayStateId _preLaunchStateId;
        private readonly GameplayStateId _runningStateId;
        private readonly GameplayStateId _runEndedStateId;
        private readonly List<RunEndCandidate> _pendingCandidates = new();

        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasLaunchApplied;
        private bool _hasAcceptedResult;
        private bool _isAwaitingPreLaunch;
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
            IRunResourceAccumulator runResourceAccumulator,
            IRunEndConfig config,
            ITime clock,
            GameplayStateId preLaunchStateId,
            GameplayStateId runningStateId,
            GameplayStateId runEndedStateId)
        {
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _contactNotifier = contactNotifier ?? throw new ArgumentNullException(nameof(contactNotifier));
            _contactClassifier = contactClassifier ?? throw new ArgumentNullException(nameof(contactClassifier));
            _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            _motionSource = motionSource ?? throw new ArgumentNullException(nameof(motionSource));
            _runResourceAccumulator = runResourceAccumulator ?? throw new ArgumentNullException(nameof(runResourceAccumulator));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));
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

            if (_isAwaitingPreLaunch)
            {
                TickRunEndedDelay(fixedDeltaTime);
                return;
            }

            if (!_hasLaunchApplied || !_gameplayStateService.IsCurrent(_runningStateId))
            {
                _pendingCandidates.Clear();
                return;
            }

            _elapsedSinceLaunch += fixedDeltaTime;
            _progressService.SamplePosition(_motionSource.Position);
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

        private void OnSlingshotLaunchApplied(SlingshotLaunchRequest launchRequest)
        {
            if (_isDisposed)
                return;

            _hasLaunchApplied = true;
            _hasAcceptedResult = false;
            _isAwaitingPreLaunch = false;
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

            if (ReferenceEquals(nextStateId, _preLaunchStateId))
            {
                _runResourceAccumulator.Reset();
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
            _hasAcceptedResult = true;
            _isAwaitingPreLaunch = _gameplayStateService.TryTransitionTo(_runEndedStateId);
            _runEndedElapsed = 0f;

            if (!_progressService.HasValidSnapshot)
            {
                Debug.LogError(
                    "Run End Flow skipped Run Result because the Run Progress Frame snapshot is invalid. "
                    + _progressService.SnapshotError);
                return;
            }

            var finalPosition = _motionSource.Position;
            var finalVelocity = _motionSource.LinearVelocity;
            _progressService.SamplePosition(finalPosition);

            var result = new RunResult(
                candidate.Reason,
                _elapsedSinceLaunch,
                _progressService.MaximumForwardProgress,
                finalPosition,
                finalVelocity.magnitude,
                _runResourceAccumulator.CreateSnapshot());

            RunResultAccepted?.InvokeSafely(result);
            Debug.Log(result.ToString());
        }

        private void TickRunEndedDelay(float fixedDeltaTime)
        {
            if (!_gameplayStateService.IsCurrent(_runEndedStateId))
            {
                _isAwaitingPreLaunch = false;
                return;
            }

            _runEndedElapsed += fixedDeltaTime;

            if (_runEndedElapsed < _config.RunEndedDelay)
                return;

            _isAwaitingPreLaunch = false;
            _gameplayStateService.TryTransitionTo(_preLaunchStateId);
        }

        private int GetPriority(RunEndReason reason)
        {
            switch (reason)
            {
                case RunEndReason.Finished:
                    return 3;
                case RunEndReason.ObstacleHit:
                    return 2;
                case RunEndReason.OutOfBounds:
                    return 1;
                case RunEndReason.LostMomentum:
                    return 0;
                default:
                    return -1;
            }
        }

        private void ResetRunEndState()
        {
            _hasLaunchApplied = false;
            _hasAcceptedResult = false;
            _isAwaitingPreLaunch = false;
            _elapsedSinceLaunch = 0f;
            _runEndedElapsed = 0f;
            _pendingCandidates.Clear();
        }
    }
}
