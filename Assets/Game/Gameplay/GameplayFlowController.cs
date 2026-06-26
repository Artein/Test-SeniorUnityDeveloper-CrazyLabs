using System;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class GameplayFlowController : IInitializable, IDisposable
    {
        private readonly ISlingshotLaunchNotifier _slingshotLaunchNotifier;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLauncher _slingshotLauncher;
        private readonly GameplayStateId _runningStateId;

        private bool _isInitialized;
        private bool _isDisposed;

        public GameplayFlowController(
            ISlingshotLaunchNotifier slingshotLaunchNotifier,
            IGameplayStateService gameplayStateService,
            ISlingshotLauncher slingshotLauncher,
            GameplayStateId runningStateId)
        {
            _slingshotLaunchNotifier = slingshotLaunchNotifier ?? throw new ArgumentNullException(nameof(slingshotLaunchNotifier));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _slingshotLauncher = slingshotLauncher ?? throw new ArgumentNullException(nameof(slingshotLauncher));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
        }

        // TODO - AI Note: For lifecycle interfaces like VContainer's IInitializable and IDisposable lets implement them explicitly
        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(GameplayFlowController));

            if (_isInitialized)
                return;

            _slingshotLaunchNotifier.LaunchRequested += HandleLaunchRequested;
            _isInitialized = true;
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_isInitialized)
                _slingshotLaunchNotifier.LaunchRequested -= HandleLaunchRequested;
        }

        private void HandleLaunchRequested(SlingshotLaunchRequest launchRequest)
        {
            if (_isDisposed)
                return;

            if (_gameplayStateService.TryTransitionTo(_runningStateId))
                _slingshotLauncher.Launch(launchRequest);
        }
    }
}
