using System;
using Game.Foundation.Time;
using Game.Gameplay.GameplayState;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public interface IRunAirTimeSource
    {
        float CurrentRunAirTimeSeconds { get; }
    }

    internal interface IRunAirTimeFixedStep
    {
        void UpdateAirTime();
    }

    public sealed class RunAirTimeTracker : IRunAirTimeSource, IRunAirTimeFixedStep, IInitializable, IDisposable
    {
        private readonly ITime _clock;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly GameplayStateId _runningStateId;
        private readonly GameplayStateId _runPreparationStateId;
        private readonly IRunSurfaceFrameSource _surfaceFrameSource;
        private bool _isDisposed;
        private bool _isInitialized;

        public float CurrentRunAirTimeSeconds { get; private set; }

        public RunAirTimeTracker(
            IGameplayStateService gameplayStateService,
            IRunSurfaceFrameSource surfaceFrameSource,
            ITime clock,
            [Key(InjectKey.GameplayStateId.RunPreparation)]
            GameplayStateId runPreparationStateId,
            [Key(InjectKey.GameplayStateId.Running)]
            GameplayStateId runningStateId)
        {
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _surfaceFrameSource = surfaceFrameSource ?? throw new ArgumentNullException(nameof(surfaceFrameSource));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            _runPreparationStateId = runPreparationStateId != null
                ? runPreparationStateId
                : throw new ArgumentNullException(nameof(runPreparationStateId));

            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunAirTimeTracker));

            if (_isInitialized)
                return;

            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;

            if (_gameplayStateService.IsCurrent(_runPreparationStateId))
                ResetCurrentRunAirTime();
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
        }

        void IRunAirTimeFixedStep.UpdateAirTime()
        {
            if (_isDisposed || !_gameplayStateService.IsCurrent(_runningStateId))
                return;

            var surfaceFrame = _surfaceFrameSource.Current;

            if (surfaceFrame.Transition == RunSurfaceTransition.HardReset)
            {
                ResetCurrentRunAirTime();
                return;
            }

            if (surfaceFrame.StableSupport.IsGrounded)
                return;

            CurrentRunAirTimeSeconds += Mathf.Max(a: 0f, _clock.FixedDeltaTime);
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (ReferenceEquals(nextStateId, _runPreparationStateId) || ReferenceEquals(nextStateId, _runningStateId))
                ResetCurrentRunAirTime();
        }

        private void ResetCurrentRunAirTime()
        {
            CurrentRunAirTimeSeconds = 0f;
        }
    }
}
