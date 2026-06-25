# Implement Issue 03: UnityInput Pointer Stream

## Summary

- Add shared Unity Input under `Assets/Game/Input/UnityInput`.
- Centralize Enhanced Touch enablement and source-agnostic pointer events behind `IUnityInput`.
- Use a ref-counted disposable enable handle so multiple consumers can safely share global Enhanced Touch state.
- Add editor-only mouse simulation with pointer id `-1`; player builds use Enhanced Touch only.
- Assume issue 01 is complete: VContainer is installed and available for the feature installer.

## Key Changes

- Create runtime/test asmdefs:
  - `Game.Input.UnityInput.asmdef`, referencing `Unity.InputSystem` and VContainer.
  - `Game.Input.UnityInput.Tests.EditMode.asmdef`, referencing runtime, NUnit/Test Framework, and test runner assemblies.
- Add public API:
  - `IEnhancedTouchSupportApi` with `IDisposable Enable()`.
  - `IEnhancedTouchPointerInput` with `PointerPressed`, `PointerMoved`, `PointerReleased`, and `PointerCanceled`.
  - `IUnityInput : IEnhancedTouchSupportApi, IEnhancedTouchPointerInput`.
  - `PointerInput` as an immutable readonly payload with `int PointerId` and `Vector2 ScreenPosition`.
  - `PointerInputHandler(PointerInput pointerInput)` delegate for all pointer events.
- Add implementation:
  - `UnityInput : IUnityInput, IDisposable` owns reference counting, handle idempotency, disposal rules, and safe public event dispatch.
  - Calling `Enable()` after `UnityInput.Dispose()` throws `ObjectDisposedException`.
  - Disposing live handles after service disposal is a no-op.
  - First live handle enables the backend; last disposed handle disables it; no events forward while disabled.
  - Public subscriber invocation is per-subscriber with `try/catch`; failures use existing Unity logging and do not block later subscribers.
- Add internal backend seam:
  - `IUnityInputBackend` exposes enable/disable, disposal, and a single internal pointer event stream with phase.
  - Production backend calls `EnhancedTouchSupport.Enable()` before subscribing and unsubscribes before `EnhancedTouchSupport.Disable()`.
  - Map `Touch.onFingerDown` to pressed, `Touch.onFingerMove` to moved, and `Touch.onFingerUp` to released or canceled based on `finger.currentTouch`/`lastTouch.phase`.
  - Map touch pointer id from `Finger.index`.
  - Compile mouse polling only under `UNITY_EDITOR`; use `Mouse.current.leftButton` and `Mouse.current.position` to emit pressed/moved/released with pointer id `-1`.
  - Do not synthesize pressed events for touches already active when enabled; only forward callbacks received after enable.
- Add VContainer installer:
  - `UnityInputInstaller : IInstaller` registers production backend, `UnityInput`, `IUnityInput`, `IEnhancedTouchSupportApi`, and `IEnhancedTouchPointerInput`.
  - Register backend lifecycle so VContainer ticks the backend for editor mouse polling; do not tick the public facade.
  - Do not add MonoBehaviours, scene wiring, Slingshot references, input action asset changes, package changes, or public diagnostics in this issue.

## Test Plan

- EditMode tests use a local fake backend through `InternalsVisibleTo`; they must not call real `EnhancedTouchSupport`.
- Test reference counting:
  - first enable enables backend once.
  - multiple handles keep backend enabled until the last handle is disposed.
  - handle disposal is idempotent.
  - no events forward after the last handle is disposed.
- Test lifecycle:
  - `Enable()` after service disposal throws.
  - outstanding handle disposal after service disposal is a no-op.
  - disposing service disables backend if currently enabled and disposes backend.
- Test event behavior:
  - backend pressed/moved/released/canceled phases forward to matching public events with unchanged payload.
  - subscriber exception is logged and later subscribers still receive the event.
  - disabled or disposed service does not forward backend events.
- Test installer:
  - build a VContainer container and resolve `IUnityInput`, `IEnhancedTouchSupportApi`, and `IEnhancedTouchPointerInput` as the same public service instance.
- Verification order: Rider reformat/problems, Unity compile via Unity AI Agent Connector, then targeted EditMode tests.
- Manual smoke: in Editor Play Mode, use a temporary local subscriber to confirm mouse press/move/release events without touching Slingshot code.

## Assumptions

- New Input System is already active and `com.unity.inputsystem` 1.19.0 is installed.
- Input System package source confirms Enhanced Touch exposes down/move/up callbacks; canceled is distinguished from released through the touch phase on up.
- No thread-safety work is required.
- Existing Unity logging is sufficient; structured logging remains deferred.
- No PlayMode tests, package/changelog changes, or Gameplay Scene changes are required for this slice.

## References

- [Issue 03: Add UnityInput Pointer Stream](03-add-unityinput-pointer-stream.md)
- [Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)
- [ADR-0007: Centralize Unity Input Behind UnityInput](../../adr/adr-0007-centralize-unity-input-behind-unityinput.md)
