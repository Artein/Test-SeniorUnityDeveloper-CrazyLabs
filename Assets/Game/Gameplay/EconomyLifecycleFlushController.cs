using System;
using Game.Gameplay.Economy;
using VContainer.Unity;
using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class EconomyLifecycleFlushController : IInitializable, IDisposable
    {
        private readonly IEconomyCommitter _economyCommitter;

        private bool _isInitialized;
        private bool _isDisposed;

        public EconomyLifecycleFlushController(IEconomyCommitter economyCommitter)
        {
            _economyCommitter = economyCommitter ?? throw new ArgumentNullException(nameof(economyCommitter));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(EconomyLifecycleFlushController));

            if (_isInitialized)
                return;

            Application.focusChanged += OnApplicationFocusChanged;
            Application.quitting += OnApplicationQuitting;
            _isInitialized = true;
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (!_isInitialized)
                return;

            Application.focusChanged -= OnApplicationFocusChanged;
            Application.quitting -= OnApplicationQuitting;
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
