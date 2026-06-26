using System;
using Game.Utils.Invocation;
using JetBrains.Annotations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using VContainer.Unity;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Game.Input.UnityInput
{
    internal interface IUnityInputBackend : IDisposable
    {
        void Enable();
        void Disable();
        
        event Action<UnityInputBackendPointerInput> PointerInputReceived;
    }

    internal enum PointerInputPhase
    {
        Pressed,
        Moved,
        Released,
        Canceled
    }

    internal readonly struct UnityInputBackendPointerInput
    {
        public PointerInputPhase Phase { get; }
        public PointerInput PointerInput { get; }

        public UnityInputBackendPointerInput(PointerInputPhase phase, PointerInput pointerInput)
        {
            Phase = phase;
            PointerInput = pointerInput;
        }
    }

    [UsedImplicitly]
    internal sealed class UnityInputBackend : IUnityInputBackend, ITickable
    {
        private bool _isEnabled;
        private bool _isDisposed;

#if UNITY_EDITOR
        private bool _wasEditorMousePressed;
#endif

        public event Action<UnityInputBackendPointerInput> PointerInputReceived;

        void IUnityInputBackend.Enable()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(UnityInputBackend));

            if (_isEnabled)
                return;

            EnhancedTouchSupport.Enable();
            Touch.onFingerDown += HandleFingerDown;
            Touch.onFingerMove += HandleFingerMove;
            Touch.onFingerUp += HandleFingerUp;
            _isEnabled = true;

#if UNITY_EDITOR
            _wasEditorMousePressed = IsEditorMousePressed();
#endif
        }

        void IUnityInputBackend.Disable()
        {
            if (!_isEnabled)
                return;

            Touch.onFingerDown -= HandleFingerDown;
            Touch.onFingerMove -= HandleFingerMove;
            Touch.onFingerUp -= HandleFingerUp;
            EnhancedTouchSupport.Disable();
            _isEnabled = false;

#if UNITY_EDITOR
            _wasEditorMousePressed = false;
#endif
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            ((IUnityInputBackend)this).Disable();
            _isDisposed = true;
        }

        void ITickable.Tick()
        {
#if UNITY_EDITOR
            if (!_isEnabled || _isDisposed)
                return;

            var mouse = Mouse.current;
            if (mouse is null)
            {
                _wasEditorMousePressed = false;
                return;
            }

            var isPressed = mouse.leftButton.isPressed;
            var screenPosition = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame || (!_wasEditorMousePressed && isPressed))
                Raise(PointerInputPhase.Pressed, new PointerInput(-1, screenPosition));
            else if (mouse.leftButton.wasReleasedThisFrame || (_wasEditorMousePressed && !isPressed))
                Raise(PointerInputPhase.Released, new PointerInput(-1, screenPosition));
            else if (isPressed)
                Raise(PointerInputPhase.Moved, new PointerInput(-1, screenPosition));

            _wasEditorMousePressed = isPressed;
#endif
        }

        private void HandleFingerDown(Finger finger)
        {
            Raise(PointerInputPhase.Pressed, CreatePointerInput(finger));
        }

        private void HandleFingerMove(Finger finger)
        {
            Raise(PointerInputPhase.Moved, CreatePointerInput(finger));
        }

        private void HandleFingerUp(Finger finger)
        {
            var touch = GetCurrentOrLastTouch(finger);
            var phase = touch is { valid: true, phase: InputTouchPhase.Canceled }
                ? PointerInputPhase.Canceled
                : PointerInputPhase.Released;

            Raise(phase, CreatePointerInput(finger));
        }

        private PointerInput CreatePointerInput(Finger finger)
        {
            var touch = GetCurrentOrLastTouch(finger);
            var screenPosition = touch.valid ? touch.screenPosition : finger.screenPosition;

            return new PointerInput(finger.index, screenPosition);
        }

        private Touch GetCurrentOrLastTouch(Finger finger)
        {
            var touch = finger.currentTouch;
            return touch.valid ? touch : finger.lastTouch;
        }

#if UNITY_EDITOR
        private bool IsEditorMousePressed()
        {
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.isPressed;
        }
#endif

        private void Raise(PointerInputPhase phase, PointerInput pointerInput)
        {
            PointerInputReceived?.InvokeSafely(new UnityInputBackendPointerInput(phase, pointerInput));
        }
    }
}
