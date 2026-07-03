using System;
using Game.Gameplay.GameplayState;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    internal sealed class FinishCelebrationPresenter : IInitializable, IDisposable
    {
        private readonly IGameplayStateService _gameplayStateService;
        private readonly IRunResultNotifier _runResultNotifier;
        private readonly IFinishPresentationView _view;
        private readonly GameplayStateId _runPreparationStateId;

        private bool _hasPlayedSuccessCelebration;
        private bool _isInitialized;
        private bool _isDisposed;

        public FinishCelebrationPresenter(
            IGameplayStateService gameplayStateService,
            IRunResultNotifier runResultNotifier,
            IFinishPresentationView view,
            [Key(InjectKey.GameplayStateId.RunPreparation)]
            GameplayStateId runPreparationStateId)
        {
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _runResultNotifier = runResultNotifier ?? throw new ArgumentNullException(nameof(runResultNotifier));
            _view = view ?? throw new ArgumentNullException(nameof(view));

            _runPreparationStateId = runPreparationStateId != null
                ? runPreparationStateId
                : throw new ArgumentNullException(nameof(runPreparationStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(FinishCelebrationPresenter));

            if (_isInitialized)
                return;

            _runResultNotifier.RunResultAccepted += OnRunResultAccepted;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;

            if (_gameplayStateService.IsCurrent(_runPreparationStateId))
                ResetForRunPreparation();
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
            if (_isDisposed || !result.IsSuccess || _hasPlayedSuccessCelebration)
                return;

            _hasPlayedSuccessCelebration = true;
            _view.PlaySuccessCelebration();
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed || !ReferenceEquals(nextStateId, _runPreparationStateId))
                return;

            ResetForRunPreparation();
        }

        private void ResetForRunPreparation()
        {
            _hasPlayedSuccessCelebration = false;
            _view.ResetForRunPreparation();
        }
    }
}
