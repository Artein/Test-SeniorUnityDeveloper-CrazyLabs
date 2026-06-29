using System;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using VContainer.Unity;

namespace Game.Gameplay
{
    public interface IRunPreparationContinueCommand
    {
        bool TryContinue();
    }

    public sealed class GameplayFlowController : IInitializable, IDisposable, IRunPreparationContinueCommand
    {
        private readonly ISlingshotCapture _slingshotCapture;
        private readonly ISlingshotRunPreparationReset _slingshotRunPreparationReset;
        private readonly ISlingshotLaunchNotifier _slingshotLaunchNotifier;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly IGameplaySlingshotLauncher _slingshotLauncher;
        private readonly IRunModifierSnapshotFactory _snapshotFactory;
        private readonly IRunModifierSnapshotStore _snapshotStore;
        private readonly IPreLaunchRigPoseResetter _preLaunchRigPoseResetter;
        private readonly GameplayStateId _runPreparationStateId;
        private readonly GameplayStateId _preLaunchStateId;
        private readonly GameplayStateId _runningStateId;

        private bool _isInitialized;
        private bool _isDisposed;

        public GameplayFlowController(
            ISlingshotCapture slingshotCapture,
            ISlingshotRunPreparationReset slingshotRunPreparationReset,
            ISlingshotLaunchNotifier slingshotLaunchNotifier,
            IGameplayStateService gameplayStateService,
            IGameplaySlingshotLauncher slingshotLauncher,
            IRunModifierSnapshotFactory snapshotFactory,
            IRunModifierSnapshotStore snapshotStore,
            IPreLaunchRigPoseResetter preLaunchRigPoseResetter,
            GameplayStateId runPreparationStateId,
            GameplayStateId preLaunchStateId,
            GameplayStateId runningStateId)
        {
            _slingshotCapture = slingshotCapture ?? throw new ArgumentNullException(nameof(slingshotCapture));

            _slingshotRunPreparationReset = slingshotRunPreparationReset
                                            ?? throw new ArgumentNullException(nameof(slingshotRunPreparationReset));
            _slingshotLaunchNotifier = slingshotLaunchNotifier ?? throw new ArgumentNullException(nameof(slingshotLaunchNotifier));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _slingshotLauncher = slingshotLauncher ?? throw new ArgumentNullException(nameof(slingshotLauncher));
            _preLaunchRigPoseResetter = preLaunchRigPoseResetter ?? throw new ArgumentNullException(nameof(preLaunchRigPoseResetter));
            _snapshotFactory = snapshotFactory ?? throw new ArgumentNullException(nameof(snapshotFactory));
            _snapshotStore = snapshotStore ?? throw new ArgumentNullException(nameof(snapshotStore));

            _runPreparationStateId = runPreparationStateId != null
                ? runPreparationStateId
                : throw new ArgumentNullException(nameof(runPreparationStateId));

            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
        }

        bool IRunPreparationContinueCommand.TryContinue()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(GameplayFlowController));

            if (!_gameplayStateService.IsCurrent(_runPreparationStateId))
                return false;

            var previousSnapshot = _snapshotStore.CurrentSnapshot;
            var nextSnapshot = _snapshotFactory.CreateSnapshot();
            _snapshotStore.SetSnapshot(nextSnapshot);

            if (_gameplayStateService.TryTransitionTo(_preLaunchStateId))
                return true;

            _snapshotStore.SetSnapshot(previousSnapshot);
            return false;
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(GameplayFlowController));

            if (_isInitialized)
                return;

            _slingshotLaunchNotifier.LaunchRequested += OnSlingshotLaunchRequested;
            _gameplayStateService.GameplayStateChanging += OnGameplayStateChanging;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;

            if (_gameplayStateService.IsCurrent(_runPreparationStateId))
            {
                ResetForRunPreparation();
                return;
            }

            if (_gameplayStateService.IsCurrent(_preLaunchStateId))
            {
                _preLaunchRigPoseResetter.ResetToPreLaunchRigPose();
                _slingshotCapture.EnableCapture();
            }
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_isInitialized)
            {
                _slingshotLaunchNotifier.LaunchRequested -= OnSlingshotLaunchRequested;
                _gameplayStateService.GameplayStateChanging -= OnGameplayStateChanging;
                _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
            }
        }

        private void OnGameplayStateChanging(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (ReferenceEquals(nextStateId, _runPreparationStateId))
            {
                ResetForRunPreparation();
                return;
            }

            if (ReferenceEquals(nextStateId, _preLaunchStateId)
                && !ReferenceEquals(previousStateId, _runPreparationStateId))
            {
                _preLaunchRigPoseResetter.ResetToPreLaunchRigPose();
            }
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (ReferenceEquals(nextStateId, _preLaunchStateId))
            {
                _slingshotCapture.EnableCapture();
                return;
            }

            if (ReferenceEquals(previousStateId, _preLaunchStateId)
                && !ReferenceEquals(nextStateId, _runPreparationStateId))
            {
                _slingshotCapture.DisableCapture();
            }
        }

        private void OnSlingshotLaunchRequested(SlingshotLaunchRequest launchRequest)
        {
            if (_isDisposed)
                return;

            if (!_gameplayStateService.IsCurrent(_preLaunchStateId))
                return;

            if (_gameplayStateService.TryTransitionTo(_runningStateId))
                _slingshotLauncher.Launch(launchRequest);
        }

        private void ResetForRunPreparation()
        {
            _preLaunchRigPoseResetter.ResetToPreLaunchRigPose();
            _slingshotRunPreparationReset.ResetForRunPreparation();
        }
    }
}
