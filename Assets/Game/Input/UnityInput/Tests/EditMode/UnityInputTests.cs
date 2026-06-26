using System;
using System.Text.RegularExpressions;
using Game.Input.UnityInput;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class UnityInputTests
{
    [Test]
    public void Enable_FirstHandle_EnablesBackendOnce()
    {
        var backend = new FakeUnityInputBackend();
        using var unityInput = new UnityInput(backend);

        using var handle = ((IEnhancedTouchSupportApi)unityInput).Enable();

        Assert.That(handle, Is.Not.Null);
        Assert.That(backend.EnableCount, Is.EqualTo(1));
        Assert.That(backend.DisableCount, Is.Zero);
    }

    [Test]
    public void Enable_MultipleHandles_DisablesBackendAfterLastHandleDisposed()
    {
        var backend = new FakeUnityInputBackend();
        using var unityInput = new UnityInput(backend);

        var firstHandle = ((IEnhancedTouchSupportApi)unityInput).Enable();
        var secondHandle = ((IEnhancedTouchSupportApi)unityInput).Enable();

        firstHandle.Dispose();

        Assert.That(backend.EnableCount, Is.EqualTo(1));
        Assert.That(backend.DisableCount, Is.Zero);

        secondHandle.Dispose();

        Assert.That(backend.DisableCount, Is.EqualTo(1));
    }

    [Test]
    public void EnableHandle_DisposedTwice_DisablesBackendOnce()
    {
        var backend = new FakeUnityInputBackend();
        using var unityInput = new UnityInput(backend);
        var handle = ((IEnhancedTouchSupportApi)unityInput).Enable();

        handle.Dispose();
        handle.Dispose();

        Assert.That(backend.DisableCount, Is.EqualTo(1));
    }

    [Test]
    public void Enable_AfterDispose_ThrowsObjectDisposedException()
    {
        var backend = new FakeUnityInputBackend();
        var unityInput = new UnityInput(backend);

        ((IDisposable)unityInput).Dispose();

        Assert.That(() => ((IEnhancedTouchSupportApi)unityInput).Enable(), Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void EnableHandle_DisposedAfterServiceDispose_DoesNothing()
    {
        var backend = new FakeUnityInputBackend();
        var unityInput = new UnityInput(backend);
        var handle = ((IEnhancedTouchSupportApi)unityInput).Enable();

        ((IDisposable)unityInput).Dispose();

        Assert.That(handle.Dispose, Throws.Nothing);
        Assert.That(backend.DisableCount, Is.EqualTo(1));
        Assert.That(backend.DisposeCount, Is.EqualTo(1));
    }

    [Test]
    public void Dispose_WithLiveHandle_DisablesAndDisposesBackend()
    {
        var backend = new FakeUnityInputBackend();
        var unityInput = new UnityInput(backend);
        _ = ((IEnhancedTouchSupportApi)unityInput).Enable();

        ((IDisposable)unityInput).Dispose();

        Assert.That(backend.DisableCount, Is.EqualTo(1));
        Assert.That(backend.DisposeCount, Is.EqualTo(1));
    }

    [Test]
    public void BackendEvent_WhenDisabled_DoesNotForwardPointerInput()
    {
        var backend = new FakeUnityInputBackend();
        using var unityInput = new UnityInput(backend);
        var pressedCount = 0;
        unityInput.PointerPressed += _ => pressedCount += 1;

        backend.Raise(PointerInputPhase.Pressed, new PointerInput(7, new Vector2(10f, 20f)));

        Assert.That(pressedCount, Is.Zero);
    }

    [Test]
    public void BackendEvent_AfterLastHandleDisposed_DoesNotForwardPointerInput()
    {
        var backend = new FakeUnityInputBackend();
        using var unityInput = new UnityInput(backend);
        var pressedCount = 0;
        unityInput.PointerPressed += _ => pressedCount += 1;
        var handle = ((IEnhancedTouchSupportApi)unityInput).Enable();

        handle.Dispose();
        backend.Raise(PointerInputPhase.Pressed, new PointerInput(7, new Vector2(10f, 20f)));

        Assert.That(pressedCount, Is.Zero);
    }

    [Test]
    public void BackendEvent_WhenEnabled_ForwardsEachPhaseToMatchingEvent()
    {
        var backend = new FakeUnityInputBackend();
        using var unityInput = new UnityInput(backend);
        using var handle = ((IEnhancedTouchSupportApi)unityInput).Enable();
        var pressed = default(PointerInput);
        var moved = default(PointerInput);
        var released = default(PointerInput);
        var canceled = default(PointerInput);
        unityInput.PointerPressed += value => pressed = value;
        unityInput.PointerMoved += value => moved = value;
        unityInput.PointerReleased += value => released = value;
        unityInput.PointerCanceled += value => canceled = value;

        backend.Raise(PointerInputPhase.Pressed, new PointerInput(1, new Vector2(11f, 12f)));
        backend.Raise(PointerInputPhase.Moved, new PointerInput(2, new Vector2(21f, 22f)));
        backend.Raise(PointerInputPhase.Released, new PointerInput(3, new Vector2(31f, 32f)));
        backend.Raise(PointerInputPhase.Canceled, new PointerInput(4, new Vector2(41f, 42f)));

        Assert.That(pressed.PointerId, Is.EqualTo(1));
        Assert.That(pressed.ScreenPosition, Is.EqualTo(new Vector2(11f, 12f)));
        Assert.That(moved.PointerId, Is.EqualTo(2));
        Assert.That(moved.ScreenPosition, Is.EqualTo(new Vector2(21f, 22f)));
        Assert.That(released.PointerId, Is.EqualTo(3));
        Assert.That(released.ScreenPosition, Is.EqualTo(new Vector2(31f, 32f)));
        Assert.That(canceled.PointerId, Is.EqualTo(4));
        Assert.That(canceled.ScreenPosition, Is.EqualTo(new Vector2(41f, 42f)));
    }

    [Test]
    public void PointerInput_ConstructedWithValues_ExposesPointerIdAndScreenPosition()
    {
        var pointerInput = new PointerInput(42, new Vector2(123f, 456f));

        Assert.That(pointerInput.PointerId, Is.EqualTo(42));
        Assert.That(pointerInput.ScreenPosition, Is.EqualTo(new Vector2(123f, 456f)));
    }

    [Test]
    public void InputSystemUpdate_EditorMousePressed_WhenEnabled_ForwardsPointerPressed()
    {
        var backend = new UnityInputBackend();
        using var unityInput = new UnityInput(backend);
        using var handle = ((IEnhancedTouchSupportApi)unityInput).Enable();
        var mouse = InputSystem.AddDevice<Mouse>("Unity Input Backend Test Mouse");
        var receivedCount = 0;
        var receivedPointerInput = default(PointerInput);

        unityInput.PointerPressed += pointerInput =>
        {
            receivedCount += 1;
            receivedPointerInput = pointerInput;
        };

        try
        {
            mouse.MakeCurrent();

            var screenPosition = new Vector2(123f, 456f);

            var mouseState = new MouseState
            {
                position = screenPosition
            }.WithButton(MouseButton.Left, true);

            InputSystem.QueueStateEvent(mouse, mouseState);
            InputSystem.Update();

            Assert.That(receivedCount, Is.EqualTo(1));
            Assert.That(receivedPointerInput.PointerId, Is.EqualTo(-1));
            Assert.That(receivedPointerInput.ScreenPosition, Is.EqualTo(screenPosition));
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [Test]
    public void PointerPressed_WhenSubscriberThrows_LogsAndNotifiesRemainingSubscribers()
    {
        var backend = new FakeUnityInputBackend();
        using var unityInput = new UnityInput(backend);
        using var handle = ((IEnhancedTouchSupportApi)unityInput).Enable();
        var receivedCount = 0;
        unityInput.PointerPressed += _ => throw new InvalidOperationException("subscriber failed");
        unityInput.PointerPressed += _ => receivedCount += 1;
        LogAssert.Expect(LogType.Exception, new Regex("subscriber failed"));

        backend.Raise(PointerInputPhase.Pressed, new PointerInput(9, new Vector2(90f, 91f)));

        Assert.That(receivedCount, Is.EqualTo(1));
    }

    private sealed class FakeUnityInputBackend : IUnityInputBackend
    {
        public int EnableCount { get; private set; }
        public int DisableCount { get; private set; }
        public int DisposeCount { get; private set; }

        public event Action<UnityInputBackendPointerInput> PointerInputReceived;

        public void Enable()
        {
            EnableCount += 1;
        }

        public void Disable()
        {
            DisableCount += 1;
        }

        public void Dispose()
        {
            DisposeCount += 1;
        }

        public void Raise(PointerInputPhase phase, PointerInput pointerInput)
        {
            PointerInputReceived?.Invoke(new UnityInputBackendPointerInput(phase, pointerInput));
        }
    }
}
