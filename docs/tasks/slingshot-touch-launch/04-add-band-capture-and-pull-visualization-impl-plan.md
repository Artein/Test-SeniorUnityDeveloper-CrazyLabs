# Implement Issue 04: Band Capture And Pull Visualization

> Superseded behavior note: this implementation plan records the original first-slice visualization. Current Band Shape behavior is tracked by
> `docs/tasks/slingshot-natural-band-shape/` and uses fixed `BandWrapSampleCount + 4` point polylines from the natural taut solver instead of the
> three-point path described below.

## Summary

- Add the reusable Slingshot feature under `Assets/Game/Gameplay/Slingshot`.
- Implement Pre-Launch-only pointer capture, Band Touch Target hit testing, Pull Plane projection, clamping, and visual commands.
- Stop before `LaunchRequested`: releases/cancels return visuals to idle, but no launch payload/event is raised in this slice.
- Leave `GameplayScene.unity` wiring to issue 08; this issue supplies the runtime code, config, view adapter, gizmos, installer/test seams, and
  EditMode tests.

## Key Changes

- Create runtime/test asmdefs:
  - `Game.Gameplay.Slingshot.asmdef`, referencing `Game.Input.UnityInput`, `Game.Gameplay.GameplayState`, Unity UI/runtime assemblies, and
    VContainer.
  - `Game.Gameplay.Slingshot.Tests.EditMode.asmdef`, referencing Slingshot plus NUnit/Test Framework.
- Add `SlingshotConfig : ScriptableObject` and default asset at `Assets/Game/Gameplay/SlingshotConfig.asset`.
  - Fields: Band Touch Target radius in pixels, min/max Pull distance, max lateral Pull, max launch angle, min/max launch speed, launch speed curve,
    launch up speed.
  - Use `[Min]`/`[Range]` for simple limits and `OnValidate` plus runtime validation for cross-field fail-fast checks.
- Add shallow view API and MonoBehaviour adapter:
  - `ISlingshotView` exposes geometry snapshot creation and command methods for inactive idle, capture idle, and active Pull visuals.
  - `SlingshotView : MonoBehaviour` owns serialized refs for left/right/rest anchors, Launch Frame, Band `LineRenderer`, Pull Hint UI object, Touch
    Indicator UI object, and optional gizmo config.
  - `OnValidate` validates assigned references; no `Reset()` or dependency injection.
  - Original superseded first-slice Band Shape was three points: left anchor, pull/rest point, right anchor. Current natural Band Shape behavior is
    owned by `docs/tasks/slingshot-natural-band-shape/`.
  - Pull Hint and Touch Indicator are controlled as authored UI objects, with Touch Indicator positioned from the clamped projected point.
- Add plain C# controller and math seams:
  - `SlingshotController : IInitializable, IDisposable` subscribes once to `IUnityInput` pointer events and
    `IGameplayStateService.GameplayStateChanged`.
  - Controller enables capture only while current state is the injected `PreLaunchStateId`.
  - Enter Pre-Launch: acquire `IUnityInput.Enable()` handle, reset capture, show capture idle.
  - Leave Pre-Launch: cancel Active Pull, restore inactive idle, disable capture, then dispose the input handle.
  - Only the first captured pointer controls Active Pull; all other pointer ids are ignored.
  - `ISlingshotInputProjector` handles screen-to-Pull-Plane and world-to-screen projection without physics raycasts or layers.
  - Band Touch Target uses screen-space distance from pointer to the projected visible rest Band polyline through the rest point.
  - Pull math uses Launch Frame axes: backward distance is `max(0, -dot(delta, forward))`; lateral offset is `dot(delta, right)`.
  - Backward distance and lateral offset clamp independently; clamped pull point is `rest + right * lateral - forward * backward`.
  - Projection failure during Active Pull cancels Pull and restores capture idle.
- Add selected-object gizmos on `SlingshotView` for anchors, Launch Frame axes, Pull Plane, Pull limits, lateral limits/angle, and Band Touch Target
  radius preview.
- Add a small `SlingshotInstaller : IInstaller` for later LifetimeScope use; it registers config, pre-launch state id, projector, controller
  lifecycle, and view interface instances supplied by composition. It must not inject MonoBehaviours.

## Test Plan

- EditMode tests use local fakes for Unity Input, Gameplay State, Slingshot view, and input projector.
- Cover state/input lifecycle:
  - Initializing while already Pre-Launch acquires input handle and shows capture idle.
  - Entering Pre-Launch acquires input before enabling capture visuals.
  - Leaving Pre-Launch cancels Active Pull and restores idle before disposing input handle.
  - Disposal unsubscribes from input and state events and disposes any active handle.
- Cover capture behavior:
  - Press inside generous screen-space Band Touch Target, including near either visible rest Band segment, starts Active Pull.
  - Press outside target is ignored.
  - First pointer wins; other pointers cannot move/release/cancel the Active Pull.
- Cover Pull behavior:
  - Movement projects through `ISlingshotInputProjector`.
  - Projection failure cancels Pull and restores idle.
  - Forward displacement clamps to zero.
  - Backward distance and lateral offset clamp independently.
  - Touch Indicator uses clamped projected screen position, not raw finger position.
- Cover release/cancel behavior:
  - Release returns Band Shape and UI to capture idle without raising launch.
  - Cancel returns Band Shape and UI to capture idle.
- Verification order for implementation: Rider reformat/problems, Unity compile via Unity AI Agent Connector, then targeted Slingshot EditMode tests.
- Manual smoke: temporary scene or later Gameplay Scene wiring, using editor mouse simulation to check Band, Pull Hint, Touch Indicator, and gizmos.

## Assumptions

- Issues 01-03 are implemented first: VContainer composition spine, Gameplay State, and Unity Input exist.
- No `LaunchRequested`, launch payload, Gameplay Flow, Rigidbody launch, rope physics, Burst, haptics, audio, or camera sequence work in this issue.
- No `GameplayScene.unity` mutation is required here; issue 08 owns final scene composition.
- Controller exceptions are allowed to surface; Unity Input subscriber isolation is owned by issue 03.

## References

- [Issue 04: Add Band Capture And Pull Visualization](04-add-band-capture-and-pull-visualization.md)
- [Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)
- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
- [ADR-0007: Centralize Unity Input Behind UnityInput](../../adr/adr-0007-centralize-unity-input-behind-unityinput.md)
