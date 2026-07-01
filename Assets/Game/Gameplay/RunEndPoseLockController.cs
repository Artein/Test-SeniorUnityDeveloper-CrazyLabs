using System;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class RunEndPoseLockController : IInitializable, IDisposable
    {
        private readonly IRunEndPoseLockTarget _target;
        private readonly IRunResultNotifier _runResultNotifier;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly GameplayStateId _runPreparationStateId;

        private bool _isInitialized;
        private bool _isDisposed;
        private bool _isPoseLocked;

        public RunEndPoseLockController(
            IRunEndPoseLockTarget target,
            IRunResultNotifier runResultNotifier,
            IGameplayStateService gameplayStateService,
            [Key(InjectKey.GameplayStateId.RunPreparation)]
            GameplayStateId runPreparationStateId)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _runResultNotifier = runResultNotifier ?? throw new ArgumentNullException(nameof(runResultNotifier));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));

            _runPreparationStateId = runPreparationStateId != null
                ? runPreparationStateId
                : throw new ArgumentNullException(nameof(runPreparationStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunEndPoseLockController));

            if (_isInitialized)
                return;

            _runResultNotifier.RunResultAccepted += OnRunResultAccepted;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _runResultNotifier.RunResultAccepted -= OnRunResultAccepted;
            _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
        }

        private void OnRunResultAccepted(RunResult result)
        {
            if (_isDisposed || _isPoseLocked)
                return;

            _target.HoldRunEndPose(result.FinalPosition);
            _isPoseLocked = true;
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed || !_isPoseLocked || !ReferenceEquals(nextStateId, _runPreparationStateId))
                return;

            _target.ReleaseRunEndPose();
            _isPoseLocked = false;
        }
    }
}
