# Implement Issue 08: Wire GameplayScene Playable Slingshot

## Summary

- Wire the already-built slices from issues 01-07 into `GameplayScene.unity`.
- Keep this issue scene/composition focused: explicit serialized references, shallow MonoBehaviours, no new gameplay rules.
- Make the scene playable with Editor mouse input: Pre-Launch holds Player, Pull updates Band/UI, valid release transitions to Running and launches Player.

## Key Changes

- Extend `GameplayLifetimeScope` to own explicit serialized references for:
  - `GameplayStateConfig`, Pre-Launch `GameplayStateId`, Running `GameplayStateId`, and `SlingshotConfig`.
  - Main Camera/input camera, `SlingshotView`, and `RigidbodyLaunchTarget`.
  - Feature installers for Unity Input, Gameplay State, Slingshot, Slingshot launch, and Gameplay Flow.
  - Existing scene adapters registered as interface instances: `ISlingshotView` and `ILaunchTarget`.
  - `OnValidate` plus runtime assertions/fail-fast validation for every required reference.
- Author `GameplayScene.unity`:
  - Add a `GameplayLifetimeScope` root object.
  - Add a `Slingshot` root with left/right/rest anchors, Launch Frame, Band `LineRenderer`, and `SlingshotView`.
  - Place Launch Frame at Player rest, with forward along world `+Z`, right along `+X`, and up along `+Y`.
  - Author initial anchors around the existing Player: left/right posts symmetric on `X`, rest point centered near the Player, Band behind the Player toward the camera so backward Pull is toward `-Z`.
  - Add `RigidbodyLaunchTarget` to Player and assign the existing Player `Rigidbody`.
- Add visual scene assets only where needed:
  - Assign a simple visible Band material to the `LineRenderer`.
  - Add a `Screen Space - Overlay` Canvas with `CanvasScaler`.
  - Add scene-authored `Pull Hint` and `Touch Indicator` UI objects under that Canvas.
  - Keep UI objects passive; `SlingshotView` only shows/hides and positions them.
- Wire authored gameplay assets:
  - Assign existing `PreLaunchStateId`, `RunningStateId`, `GameplayStateConfig`, and `SlingshotConfig`.
  - Do not create duplicate state ids/configs in the scene slice.
  - Confirm Gameplay State config initial state is Pre-Launch and transition Pre-Launch to Running exists.

## Test Plan

- Add a PlayMode composition smoke test if scene loading infrastructure is practical:
  - Use a typed test assets provider ScriptableObject to reference `GameplayScene.unity`; do not hardcode scene path/GUID.
  - Load the scene, wait one frame, and assert required scene components exist.
  - Assert `GameplayLifetimeScope`, `SlingshotView`, and `RigidbodyLaunchTarget` have no missing references.
  - Assert Player Rigidbody is held in Pre-Launch after composition initializes.
- Run verification in this order:
  - Rider reformat/problems on changed C# and asmdef files.
  - Unity compile via Unity AI Agent Connector.
  - Targeted PlayMode smoke test after compile is clean.
  - Manual Unity smoke: open Gameplay Scene, enter Play Mode, pull/release with editor mouse, confirm Player launches and no missing-reference/errors appear.

## Assumptions

- Issues 01-07 are implemented first and provide the required installers, configs, state ids, Slingshot view/controller, launch target adapter, launcher, and flow controller.
- This issue may modify `GameplayScene.unity` and composition code, but should not add new Slingshot math, input semantics, launch rules, haptics, audio, camera sequences, or rope physics.
- Human feel tuning remains issue 09; this issue only needs a playable, correctly wired baseline.

## References

- [Issue 08: Wire GameplayScene Playable Slingshot](08-wire-gameplayscene-playable-slingshot.md)
- [Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)
- [Issue 01 Implementation Plan](01-add-gameplay-composition-spine-impl-plan.md)
- [Issue 02 Implementation Plan](02-add-asset-backed-gameplay-state-impl-plan.md)
- [Issue 03 Implementation Plan](03-add-unityinput-pointer-stream-impl-plan.md)
- [Issue 04 Implementation Plan](04-add-band-capture-and-pull-visualization-impl-plan.md)
- [Issue 05 Implementation Plan](05-add-pull-release-to-launchrequested-impl-plan.md)
- [Issue 06 Implementation Plan](06-add-launch-target-and-slingshot-launcher-impl-plan.md)
- [Issue 07 Implementation Plan](07-add-gameplay-flow-launch-coordination-impl-plan.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
