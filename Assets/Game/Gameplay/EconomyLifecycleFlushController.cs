using System;
using Game.Gameplay.Economy;
using Game.Foundation.ApplicationLifecycle;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class EconomyLifecycleFlushController : IInitializable, IDisposable
    {
        private readonly IEconomyCommitter _economyCommitter;
        private readonly IApplicationPauseNotifier _applicationPauseNotifier;
        private readonly IApplicationFocusChangeNotifier _applicationFocusChangeNotifier;
        private readonly IApplicationQuitNotifier _applicationQuitNotifier;

        private bool _isInitialized;
        private bool _isDisposed;

        public EconomyLifecycleFlushController(
            IEconomyCommitter economyCommitter,
            IApplicationPauseNotifier applicationPauseNotifier,
            IApplicationFocusChangeNotifier applicationFocusChangeNotifier,
            IApplicationQuitNotifier applicationQuitNotifier)
        {
            _economyCommitter = economyCommitter ?? throw new ArgumentNullException(nameof(economyCommitter));
            _applicationPauseNotifier = applicationPauseNotifier ?? throw new ArgumentNullException(nameof(applicationPauseNotifier));

            _applicationFocusChangeNotifier =
                applicationFocusChangeNotifier ?? throw new ArgumentNullException(nameof(applicationFocusChangeNotifier));
            _applicationQuitNotifier = applicationQuitNotifier ?? throw new ArgumentNullException(nameof(applicationQuitNotifier));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(EconomyLifecycleFlushController));

            if (_isInitialized)
                return;

            _applicationPauseNotifier.PauseChanged += OnApplicationPauseChanged;
            _applicationFocusChangeNotifier.FocusChanged += OnApplicationFocusChanged;
            _applicationQuitNotifier.Quitting += OnApplicationQuitting;
            _isInitialized = true;
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (!_isInitialized)
                return;

            _applicationPauseNotifier.PauseChanged -= OnApplicationPauseChanged;
            _applicationFocusChangeNotifier.FocusChanged -= OnApplicationFocusChanged;
            _applicationQuitNotifier.Quitting -= OnApplicationQuitting;
        }

        private void OnApplicationPauseChanged(bool isPaused)
        {
            if (!isPaused || _isDisposed)
                return;

            _economyCommitter.RequestBestEffortFlush("application-paused");
        }

        private void OnApplicationFocusChanged(bool hasFocus)
        {
            if (hasFocus || _isDisposed)
                return;

            _economyCommitter.RequestBestEffortFlush("application-focus-lost");
        }

        private void OnApplicationQuitting()
        {
            if (_isDisposed)
                return;

            _economyCommitter.RequestBestEffortFlush("application-quit");
        }
    }
}
