using System;
using Game.Utils.Invocation;

namespace Game.Foundation.Input
{
    public interface IEnhancedTouchSupportApi
    {
        IDisposable Enable();
    }

    public interface IEnhancedTouchPointerInput
    {
        event Action<PointerInput> PointerPressed;
        event Action<PointerInput> PointerMoved;
        event Action<PointerInput> PointerReleased;
        event Action<PointerInput> PointerCanceled;
    }

    public interface IUnityInput : IEnhancedTouchSupportApi, IEnhancedTouchPointerInput
    {
    }

    public sealed class UnityInput : IUnityInput, IDisposable
    {
        private readonly IUnityInputBackend _backend;
        private int _enableCount;
        private bool _isDisposed;

        public event Action<PointerInput> PointerPressed;
        public event Action<PointerInput> PointerMoved;
        public event Action<PointerInput> PointerReleased;
        public event Action<PointerInput> PointerCanceled;

        internal UnityInput(IUnityInputBackend backend)
        {
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));
            _backend.PointerInputReceived += HandlePointerInputReceived;
        }

        IDisposable IEnhancedTouchSupportApi.Enable()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(UnityInput));

            if (_enableCount == 0)
                _backend.Enable();

            _enableCount += 1;
            return new EnableHandle(this);
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _backend.PointerInputReceived -= HandlePointerInputReceived;

            try
            {
                if (_enableCount > 0)
                {
                    _enableCount = 0;
                    _backend.Disable();
                }
            }
            finally
            {
                _backend.Dispose();
            }
        }

        private void ReleaseEnableHandle()
        {
            if (_isDisposed || _enableCount <= 0)
                return;

            _enableCount -= 1;

            if (_enableCount == 0)
                _backend.Disable();
        }

        private void HandlePointerInputReceived(UnityInputBackendPointerInput backendPointerInput)
        {
            if (_isDisposed || _enableCount <= 0)
                return;

            switch (backendPointerInput.Phase)
            {
                case PointerInputPhase.Pressed:
                    PointerPressed?.InvokeSafely(backendPointerInput.PointerInput);
                    break;
                case PointerInputPhase.Moved:
                    PointerMoved?.InvokeSafely(backendPointerInput.PointerInput);
                    break;
                case PointerInputPhase.Released:
                    PointerReleased?.InvokeSafely(backendPointerInput.PointerInput);
                    break;
                case PointerInputPhase.Canceled:
                    PointerCanceled?.InvokeSafely(backendPointerInput.PointerInput);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(backendPointerInput), backendPointerInput.Phase, "Unsupported pointer input phase.");
            }
        }

        private sealed class EnableHandle : IDisposable
        {
            private UnityInput _owner;

            public EnableHandle(UnityInput owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                var owner = _owner;

                if (owner is null)
                    return;

                _owner = null;
                owner.ReleaseEnableHandle();
            }
        }
    }
}
