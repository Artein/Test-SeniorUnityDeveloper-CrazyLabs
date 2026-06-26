## Problem Statement

The current Gameplay Scene contains a Player and Surface, but the run cannot be started through the intended touch-driven Slingshot interaction.
Players need to touch the Slingshot Band, Pull it backward, optionally Pull slightly left or right to steer, and release to Launch the Player forward.

The interaction must feel like a finger-sized Slingshot gesture, not a precise line tap. The Band is visually thin, so the Band Touch Target must be
generous. The player must not be able to Pull the Band forward to gain invalid launch energy. Longer backward Pull distance should produce more
launch speed, and Pull Offset should steer direction. The game should also show a Pull Hint while idle and a Touch Indicator during an Active Pull so
the mechanic is discoverable.

The implementation must respect the project direction established during design: gameplay rules live in testable plain C# controllers and services,
MonoBehaviours stay shallow, direct C# events coordinate local gameplay, Unity Enhanced Touch is centralized behind Unity Input, and Gameplay State
is asset-backed instead of hardcoded.

## Solution

Add a touch-driven Slingshot launch feature for the Gameplay Scene.

During Pre-Launch, Unity Input enables Enhanced Touch and emits source-agnostic Pointer Input events. The Slingshot accepts the first pointer press
inside the Band Touch Target, captures that pointer as the only Active Pull, projects pointer movement onto the Slingshot-owned Pull Plane, clamps
forward motion, clamps lateral motion, moves the held Launch Target to the interpreted Pull Point, updates Band Shape and Touch Indicator visuals,
and raises `LaunchRequested` only when the Pull Release is valid. The Pull Point is the held target and launch position; Band Shape uses separate
Band Contact Points and a Band Wrap so the visible Band follows the Launch Target collider instead of passing through the Launch Target mesh.

Gameplay Flow listens to the Slingshot launch request. If the authored Gameplay State transition to Running succeeds, it applies launch physics
through a Slingshot launcher. The Launch Target is held during Pre-Launch, follows the Active Pull while captured, returns to rest on cancel or weak
release, and launches from the pulled position on valid release. The accepted launch path must not reset the held Launch Target to the Rest Point
while leaving Pre-Launch. At the launch boundary, the Slingshot launcher re-applies the launch request's final Pull Point before applying velocity,
so launch does not depend on mutable scene state from a prior controller update. Band reset/recoil starts only after the shot is applied. At that
point the Band enters Band Release Recoil: it follows the moving Launch Target collider contact points while returning from the loaded Band Shape
to the rest/idle/default shape, then detaches and stops following the Launch Target. Velocities are reset and the final launch velocity is applied
with mass-agnostic velocity change.

The first slice should ship a complete usable mechanic: touch/mouse-in-editor input, Band visuals, Pull Hint, Touch Indicator, Gameplay State
gating, launch physics, scene composition, configuration assets, gizmos, and tests for the core behavior. Designer-facing launch sequencing, haptics,
audio, richer rope simulation, and retry placement are explicitly deferred.

## Unity Surfaces

- Runtime assemblies and asmdefs:
  - `Game.Input.UnityInput` for shared Unity Input infrastructure.
  - `Game.Gameplay.GameplayState` for reusable asset-backed Gameplay State mechanics.
  - `Game.Gameplay.Slingshot` for Slingshot input interpretation, launch request data, launch application, and shallow view contracts.
  - `Game.Gameplay` for game-specific Gameplay Flow composition between Slingshot and Gameplay State.
- Test assemblies and asmdefs:
  - EditMode tests for Unity Input.
  - EditMode tests for Gameplay State.
  - EditMode tests for Slingshot.
  - EditMode tests for Gameplay Flow.
  - One PlayMode scene/composition smoke test if practical.
- Editor assemblies, windows, inspectors, importers, or menu items:
  - No custom Editor assembly in the first slice.
  - Slingshot authoring gizmos use `OnDrawGizmosSelected` on the shallow Slingshot view.
- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:
  - Gameplay Scene gets narrow required wiring: Slingshot anchors, a Band LineRenderer, Canvas UI Pull Hint and Touch Indicator, Gameplay LifetimeScope,
    and Rigidbody Launch Target adapter.
  - Gameplay State Id assets: Pre-Launch, Running, Run Ended.
  - Gameplay State Transition assets: Pre-Launch to Running, Running to Run Ended, Run Ended to Pre-Launch.
  - Gameplay State config ScriptableObject with initial state and allowed transitions.
  - Slingshot config ScriptableObject with Pull, launch, touch-target, speed-curve, lateral, lift, and Band visual quality tuning.
  - Package manifest adds VContainer by direct UPM Git URL pinned to a release tag.
  - Existing Unity Input System remains enabled; the existing input actions asset is not the primary Slingshot input path.
- RPC/helper commands, hooks, or shell wrappers:
  - Unity AI Agent Connector compile and tests are used for verification during implementation.
  - No new project shell wrappers are required for the feature.
- Package versioning, changelog, and installation/sync behavior:
  - This is project gameplay work, not a package release.
  - VContainer is a new third-party dependency and must be locked through the package manifest/package lock.
  - Unity Logging package is not introduced; existing Unity logging is used until structured logging is handled separately.

## User Stories

1. As a player, I want to touch the Slingshot Band to start a Pull, so that launching the Player feels like a direct physical gesture.
2. As a player, I want the Band Touch Target to be larger than the visible Band, so that I can reliably start a Pull with my finger.
3. As a player, I want to Pull the Band backward, so that I can control launch power.
4. As a player, I want longer backward Pull distance to produce more launch speed, so that the mechanic has readable power scaling.
5. As a player, I want weak Pulls to cancel instead of launching, so that accidental small movements do not start a Run.
6. As a player, I want forward Pull movement to be ignored or clamped, so that I cannot create launch energy by pushing the Band forward.
7. As a player, I want to Pull slightly left or right, so that I can steer launch direction.
8. As a player, I want lateral Pull Offset to rotate the launch direction, so that off-center Pulls have predictable aiming value.
9. As a player, I want launch speed to depend on backward Pull distance, not side Pull, so that aiming does not unexpectedly change power.
10. As a player, I want the Band Shape to follow my Pull, so that the Slingshot gives clear visual feedback.
11. As a player, I want a Touch Indicator during an Active Pull, so that I can see the interpreted Pull Point.
12. As a player, I want a Pull Hint while the Slingshot is idle, so that I understand the gesture before first use.
13. As a player, I want Slingshot input disabled after Launch, so that the Run cannot be relaunched accidentally.
14. As a player, I want only the first captured touch to control the Slingshot, so that additional fingers do not break the interaction.
15. As a player, I want canceling or losing valid projection during a Pull to return the Band to idle, so that the Slingshot never remains visually stuck.
16. As a designer, I want Slingshot tuning in a ScriptableObject, so that touch radius, Pull limits, speed, lateral steering, and lift can be adjusted without code changes.
17. As a designer, I want a tunable speed curve, so that launch power can be shaped independently from raw Pull distance.
18. As a designer, I want separate min and max launch speed values, so that curve output maps into a clear speed range.
19. As a designer, I want a separate launch lift setting, so that forward launch and upward feel can be tuned independently.
20. As a designer, I want scene-authored Slingshot anchors and Launch Frame, so that the mechanic can be positioned visually in the scene.
21. As a designer, I want Slingshot gizmos, so that anchors, Pull Plane, Launch Frame axes, Pull limits, and touch target behavior can be inspected in the editor.
22. As a designer, I want the Pull Hint and Touch Indicator to be scene-authored UI objects, so that their visuals can be replaced without code changes.
23. As a designer, I want Band Shape to be visual-only in the first slice, so that gameplay tuning remains deterministic while visuals can improve later.
24. As a player, I want the Player to move with the pulled Band during Active Pull, so that the Slingshot feels like it is loading the target directly.
25. As a developer, I want Slingshot rules in a plain C# controller, so that Pull interpretation and Launch request creation can be EditMode tested.
26. As a developer, I want MonoBehaviours to stay shallow, so that Unity callbacks and serialized references do not accumulate gameplay decisions.
27. As a developer, I want the Slingshot view to expose command-style methods, so that presentation can be driven by controller state.
28. As a developer, I want Slingshot geometry captured as an immutable snapshot, so that hot-path Pull math does not repeatedly validate scene references.
29. As a developer, I want the Slingshot controller to receive an input projector abstraction, so that camera projection details are isolated and mockable.
30. As a developer, I want Pointer Input events to be source-agnostic, so that Slingshot does not care whether input came from touch or editor mouse.
31. As a developer, I want Unity Enhanced Touch enablement wrapped by Unity Input, so that multiple features cannot fight over global input state.
32. As a developer, I want Unity Input enable handles to be reference-counted and disposable, so that ownership is explicit and safe.
33. As a developer, I want Unity Input subscriber exceptions isolated, so that one bad subscriber does not block other input consumers.
34. As a developer, I want Unity Input to log subscriber exceptions with existing Unity logging, so that failures are visible without adding a logging package.
35. As a developer, I want Enhanced Touch callbacks hidden behind an internal backend seam, so that input lifecycle can be tested without touching Unity global state.
36. As a developer, I want editor mouse simulation to emit the same Pointer Input events, so that the mechanic can be tried in the Editor without a touch device.
37. As a developer, I want player builds to use touch input only for this mechanic, so that editor convenience does not become unintended runtime behavior.
38. As a developer, I want Gameplay State Ids as ScriptableObject assets, so that feature code does not hardcode state enums or strings.
39. As a developer, I want Gameplay State Transition assets, so that allowed flow changes are authored explicitly.
40. As a developer, I want Gameplay State validation centralized, so that config assets and runtime service construction catch the same broken data.
41. As a developer, I want Gameplay State transitions to expose changing and changed events, so that other systems can react consistently.
42. As a developer, I want same-state transitions to return false without warning, so that idempotent flow calls are harmless.
43. As a developer, I want invalid transitions to return false and warn, so that broken flow is visible without crashing ordinary runtime logic.
44. As a developer, I want Gameplay Flow to own the transition from Pre-Launch to Running, so that Slingshot remains a feature-level intent producer.
45. As a developer, I want launch force applied only after the Running transition succeeds, so that physics cannot start outside an accepted gameplay state.
46. As a developer, I want launch application failures to surface after a successful transition, so that broken composition is not silently hidden.
47. As a developer, I want launch request payloads to contain pure data only, so that launch coordination is testable and not tied to Unity object lifetimes.
48. As a developer, I want launch request direction and up direction normalized by contract, so that launch application remains simple.
49. As a developer, I want launch application hidden behind `ILaunchTarget` and held positioning hidden behind a separate narrow interface, so that Rigidbody specifics do not leak into Slingshot intent.
50. As a developer, I want Rigidbody launch to use velocity change, so that launch tuning is independent from Rigidbody mass.
51. As a developer, I want the Launch Target held during Pre-Launch, so that stale physics motion does not influence launch.
52. As a developer, I want hold and launch to clear linear and angular velocity, so that each Run begins deterministically.
53. As a developer, I want VContainer composition through a gameplay LifetimeScope, so that dependencies are explicit and scene references are centralized.
54. As a developer, I want views registered as existing interface instances without injection, so that MonoBehaviours stay shallow.
55. As a tester, I want EditMode tests for pure gameplay logic, so that regressions are fast and deterministic.
56. As a tester, I want local fakes for Slingshot dependencies, so that tests do not couple to another feature's test doubles.
57. As a tester, I want compile verification before tests, so that test failures are not polluted by script compilation errors.
58. As a tester, I want a PlayMode composition smoke test if practical, so that scene wiring failures are caught without overtesting engine behavior.
59. As a maintainer, I want the first slice to avoid haptics, audio, launch sequencing, and rope physics, so that the core launch mechanic ships without speculative complexity.
60. As a maintainer, I want ADRs and glossary vocabulary reflected in the implementation, so that future work continues using stable project language.
61. As a player, I want the Band to appear to meet the Launch Target instead of passing through it, so that the loaded Slingshot reads as physical.
62. As a player, I want the Band to appear aligned with the Launch Target collider shape, so that the loaded Slingshot reads like it is wrapping the
    target rather than cutting through or floating off it.
63. As a player, I want the Band Wrap to use enough visual points to look elastic and polished, so that the loaded Slingshot feels responsive rather
    than angular.

## Implementation Decisions

- Use the project glossary terms: Slingshot, Band, Band Touch Target, Band Shape, Band Wrap, Pull Hint, Touch Indicator, Pull, Pull Release, Active
  Pull, Pull Offset, Pull Point, Band Contact Point, Rest Point, Launch Frame, Pull Plane, Launch, Launch Target, Gameplay State, Gameplay State Id,
  Gameplay State Transition, Gameplay Flow, Unity Input, and Pointer Input.
- Keep gameplay rules in plain C# controllers and services. MonoBehaviours are shallow Unity adapters for serialized data, Unity callbacks,
  scene references, visuals, gizmos, and component calls.
- Use VContainer for dependency injection. Controllers and services may participate in VContainer lifecycle; MonoBehaviours should not receive
  injection or VContainer lifecycle responsibilities.
- Use one scene-level gameplay LifetimeScope for the first slice. The scope is composition-only: it validates serialized references and registers
  services, controllers, configs, and existing scene views/adapters.
- Do not use dynamic scene searches for composition. Required scene objects and config assets are explicit serialized references.
- Use direct C# events for local coordination before adding any event bus.
- Use event naming semantics where about-to-happen events use `-ing` and completed events use `-ed`; the Slingshot intent event is
  `LaunchRequested`.
- Use a typed `SlingshotLaunchRequestedHandler` delegate for `LaunchRequested`, carrying a single launch request payload.
- The launch request payload is pure data and may use Unity value types, but must not hold Unity object/lifecycle references.
- The launch request includes horizontal direction, launch speed, up direction, launch up speed, normalized power, Pull distance, Pull Offset, and
  final clamped `PullPointWorldPosition`.
- Keep both normalized power and Pull distance because they serve different future consumers: presentation scaling and debugging/tuning.
- Keep final `PullPointWorldPosition` because launch application, launch presentation, debugging, and tests should not infer release position from
  mutable scene state.
- Use `PullOffset` as signed lateral world-unit displacement on the Pull Plane. Positive is to the Slingshot right.
- Use `PullDistance` as effective backward world-unit displacement after forward movement is clamped to zero.
- Lateral steering is configured through max lateral pull and max launch angle. Pull Offset normalizes to `[-1, 1]` and rotates Launch Frame forward.
- Lateral Pull Offset affects launch direction only. Backward Pull distance controls launch power.
- Use a single Slingshot controller for first-slice Slingshot input gating, Pull interpretation, simple presentation commands, and launch notification.
  Defer extracting a separate presenter until visual responsibilities grow.
- The Slingshot controller depends on Unity Input, Gameplay State, Slingshot view interfaces, Slingshot config, immutable Slingshot geometry, and an
  input projector abstraction.
- The Slingshot controller subscribes to Pointer Input once for its lifetime, but only enables capture and holds a Unity Input enable handle while
  the current Gameplay State is Pre-Launch.
- The Slingshot controller reacts to Gameplay State changed events, not changing events, so input side effects happen only after accepted
  transitions.
- On initialization, the Slingshot controller checks whether Gameplay State already is Pre-Launch and enables capture immediately if needed.
- Each Pre-Launch re-entry creates a fresh Unity Input enable handle, resets capture state, and shows the Pull Hint.
- Leaving Pre-Launch without an accepted launch cancels any Active Pull, restores idle Band Shape, hides the Touch Indicator, disables capture, and
  disposes the input enable handle.
- The accepted launch path clears Pull ownership and input capture without returning the held Launch Target to the Rest Point. The Launch Target
  stays at the valid final Pull Point until launch application.
- The accepted launch path keeps the final loaded Band Shape through the synchronous launch handoff. It must not show the idle/rest Band Shape before
  the launch request has been accepted and applied.
- After the launch request is accepted and applied, the Band enters Band Release Recoil. Recoil starts from the final loaded Band Shape and keeps
  querying contact/wrap against the moving Launch Target collider while the Band returns toward Rest Shape.
- Band Release Recoil ends when the Band reaches the rest/idle/default shape. At that point the Band detaches and must not keep following the
  Launch Target.
- Slingshot capture enable and disable paths are idempotent.
- The Slingshot controller acquires the Unity Input enable handle before enabling the view, and resets/disables the view before disposing the handle.
- The Slingshot controller does not catch exceptions from input enablement, view commands, or its pointer handlers. These are configuration or logic
  failures and should surface.
- The Slingshot accepts only the first captured pointer as the Active Pull. All other pointer events are ignored while a Pull is active.
- Active Pull starts only while capture is enabled, no Active Pull exists, and the pointer press is within the Band Touch Target.
- Band Touch Target hit testing is screen-space distance from pointer to the projected visible rest Band polyline, with a configured pixel radius
  large enough for finger input.
- First-slice Band Touch Target geometry checks the two visible rest segments: left anchor to Rest Point, and Rest Point to right anchor. Richer
  curved or sagging segment hit testing is deferred.
- After capture, pointer movement is interpreted anywhere on the Pull Plane; the touch does not need to remain near the Band.
- Use a Slingshot-owned Pull Plane defined by Launch Frame right/forward axes, with Launch Frame up as plane normal.
- Slingshot authoring validation requires left anchor, right anchor, and Rest Point to be coplanar with the authored Pull Plane/Band Plane within a
  small tolerance. Invalid planar authoring fails fast instead of being silently projected into place.
- Coplanarity tolerance is a small implementation-owned validation constant, not a `SlingshotConfig` field or designer-tuned inspector value.
- Use camera projection math through an input projector abstraction to map screen positions to the Pull Plane and world points to screen positions.
  Do not use physics raycasts, Surface colliders, or input layers for Pull mapping.
- If screen-to-Pull-Plane projection fails during an Active Pull, cancel the Pull and return to idle.
- Forward Pull displacement is clamped to zero instead of canceling. Lateral displacement may still be shown at zero backward distance, but release
  does not launch until the minimum backward threshold is met.
- Backward distance and lateral offset are clamped independently before updating visuals and before creating a launch request.
- Weak, forward-only, canceled, or invalidly projected Pulls do not raise `LaunchRequested`.
- The Band Shape is visual-only in the first slice. It does not decide launch power.
- During Active Pull, the held Launch Target follows the clamped Pull Point.
- During Active Pull, first-slice held target updates are position-only and preserve the Launch Target rotation. Future target-facing, lean, spin, or
  orientation changes should be added through an explicit rotation/orientation contract or presentation layer, not hidden inside position updates.
- During an Active Pull update, `SlingshotController` computes the clamped Pull Point, calls
  `IHeldLaunchTargetPositioner.SetHeldPosition(pullPoint)`, then requests Band Contact Points and Band Wrap data from
  `ILaunchTargetBandContactProvider`.
- `SetHeldPosition` is an immediate held-target positioning contract: after it returns, the assigned Collider pose is expected to be current enough
  for same-frame Band contact calculation.
- The Pull Point remains the gameplay held/launch position and must not be treated as a Band renderer point when it would place the Band inside the
  Launch Target mesh.
- Band Shape uses Band Contact Points derived from the held Launch Target's single assigned `Collider` so the visible Band meets the target instead of
  passing through it.
- First-slice Band Contact and Band Wrap generation supports one explicitly assigned Unity `Collider` of any Collider type, not only the current
  Gameplay Scene `CapsuleCollider`. Compound or multi-collider target shape support is deferred.
- First-slice Band Contact Points are computed as closest padded points from each Band anchor to the assigned Launch Target Collider using generic
  Collider surface queries. This is intentionally not a full rope-physics or tangent solve.
- First-slice rendered Band Contact Points and Band Wrap samples are projected onto the Slingshot Pull Plane/Band height after Collider contact,
  wrap, and padding calculations. The assigned Collider provides horizontal contact shape; arbitrary vertical closest-point coordinates from tall
  Colliders are not preserved in the rendered Band Shape.
- Generic any-Collider Band Wrap is a best-effort visual approximation. It does not promise exact silhouette, tangent, or mesh-topology wrapping for
  arbitrary collider shapes.
- Generic Band Wrap sample origins are generated on an arc in the Pull Plane around the Pull Point, then projected to the assigned Collider with
  generic surface queries and padded outward.
- When contact directions are asymmetric, the Band Wrap arc uses the backward/pulled side of the Launch Target: the side most aligned with
  `-LaunchFrameForward` in the Pull Plane. It does not choose the shortest arc if that would move the visible wrap to the front or side of the target.
- Generic Collider contact and wrap padding offsets from the `Collider.ClosestPoint` result back toward the query origin, using
  `normalize(queryOrigin - closestPoint) * padding` when that direction is valid. For Band Contact Points, the query origin is the relevant Band
  anchor. For Band Wrap samples, the query origin is the Pull Plane arc sample origin.
- If `queryOrigin - closestPoint` is near zero, generic Collider padding falls back in order: projected direction from Collider bounds center to the
  closest point in the Pull Plane, then `-LaunchFrameForward`, then no padding if no valid direction exists.
- Do not generate the first-slice Band Wrap by linearly interpolating between Band Contact Points, because that describes a chord through the target
  before projection and weakens the visual feeling that the Band wraps around the held target.
- Arc-based Band Wrap sampling remains presentation-only and uses `BandWrapSegmentCount`. It does not affect Pull Point, Pull Offset, launch power, or
  launch direction.
- Band contact calculation aligns the visual contact data to the assigned Collider shape instead of using arbitrary scene-authored offsets.
  Shape-specific precision can be improved inside the Launch Target Band contact provider/adapter without changing SlingshotController or
  SlingshotView.
- First-slice Band Shape is an ordered visual path: left anchor, left Band leg, left Band Contact Point, collider-aligned Band Wrap, right Band
  Contact Point, right Band leg, right anchor.
- First-slice Band Shape does not draw a straight visible segment between Band Contact Points through the Launch Target. The Band Wrap follows the
  padded target collider shape between the Band Contact Points.
- Band Shape data may contain multiple visual sample points so the Band Wrap can read as smooth, elastic, and polished without changing launch math.
- First-slice Band Shape is represented to the view as one ordered world-space polyline, not separate renderer pieces for left leg, Band Wrap, and
  right leg.
- The ordered Band Shape polyline starts at the left anchor, includes Band Contact Points and Band Wrap samples in order, and ends at the right
  anchor.
- Band Wrap visual quality is designer-tuned through `SlingshotConfig`, using a clamped `BandWrapSegmentCount` value such as `[Range(2, 24)]` with
  a sensible default around `12`.
- Band Contact Points may include a small designer-authored non-negative padding value so the Band visually sits just outside the target mesh.
- The Slingshot geometry Rest Point is the source of truth for the unloaded Pull Point and held Launch Target reset position.
- Canceling, weak release, or invalid projection returns the held Launch Target to the Rest Point.
- Valid release keeps the Launch Target at the final pulled point until launch is applied, so launch starts from the loaded Slingshot position.
- After launch application/shoot, the Band enters Band Release Recoil while the Launch Target leaves the Slingshot. This reset/recoil is a post-shot
  visual behavior, not pre-launch cleanup.
- During Band Release Recoil, the Band continues computing Band Contact Points and Band Wrap from the moving Launch Target collider so the shot
  reads as the Band pushing the target forward.
- Band Release Recoil stops following the Launch Target when the Band reaches the rest/idle/default shape.
- Rest Band Shape may use the Rest Point only when no visible target contact is required. If the held Launch Target is visible at the Rest Point, the
  same Band Contact Point rule applies so the Band does not pass through the mesh while idle.
- Additional Band sag, secondary motion, or richer rope visuals can be added later without changing launch math.
- Pull Hint is a scene-authored UI object controlled by the Slingshot view.
- Touch Indicator is a scene-authored UI object controlled by the Slingshot view during Active Pull.
- Pull Hint and Touch Indicator live under a Canvas; the Band remains world-rendered.
- Slingshot view exposes command-style methods for pull visuals, idle visuals, and capture availability. It does not own Launch Target movement.
- Slingshot view receives an already computed Band Shape. It does not query Launch Target colliders or decide Band Contact Points or Band Wrap
  geometry.
- Slingshot view renders the first-slice Band Shape with one LineRenderer by applying every ordered polyline point.
- Slingshot view exposes a read-only geometry snapshot method. It fails fast when required serialized references are missing.
- Slingshot view or geometry snapshot validation fails fast when left anchor, right anchor, or Rest Point is authored off the Pull Plane/Band Plane
  beyond tolerance. First-slice geometry does not silently repair non-planar authoring by projection.
- The Pull Plane/Band Plane coplanarity tolerance is private/internal to geometry validation and covered by tests; designers do not tune it through
  scene or config assets.
- Slingshot geometry is captured once at controller construction/initialization and treated as static during a run.
- Slingshot config is a ScriptableObject with touch target radius, min/max Pull distance, max lateral pull, max launch angle, min/max launch speed,
  launch speed curve, launch up speed, and Band visual sample tuning.
- Slingshot config uses inspector attributes for simple scalar limits and `OnValidate` for cross-field or structural validation.
- Slingshot config exposes Band visual sample tuning through `ISlingshotConfig` so controllers and pure tests do not depend on ScriptableObject
  instances.
- The launch speed curve maps normalized Pull power to a clamped `0..1` factor, then interpolates between min and max launch speed.
- Missing or empty launch speed curves must not be silently replaced at runtime. Sensible asset defaults are allowed, but broken serialized tuning
  must be reported or fail during validation/construction.
- Slingshot view includes selected-object gizmos for anchors, Launch Frame axes, Pull Plane, Pull limits, touch target radius, and lateral angle.
- Use Unity Input System Enhanced Touch for runtime touch input.
- Centralize Enhanced Touch enable/disable and Pointer Input translation behind a root-scoped Unity Input service.
- Unity Input implements `IEnhancedTouchSupportApi`, `IEnhancedTouchPointerInput`, and composed `IUnityInput`.
- `IEnhancedTouchSupportApi.Enable()` returns an idempotent disposable handle. First handle enables/subscribes; last handle unsubscribes/disables.
- Unity Input throws if `Enable()` is called after disposal. Outstanding handles disposed after service disposal are no-ops.
- Unity Input subscribes to Enhanced Touch callbacks only while at least one enable handle is alive.
- Unity Input maps Enhanced Touch finger index to Pointer Id and reserves `-1` for editor mouse simulation.
- Unity Input exposes Pointer Pressed, Moved, Released, and Canceled events with a tiny immutable Pointer Input payload containing Pointer Id and raw
  screen pixel position.
- Unity Input does not synthesize pressed events for touches already active when input is enabled.
- Unity Input does not filter UI hits, coalesce moves, expose public diagnostics, include timestamps/deltas, or include input source type in the
  first slice.
- Unity Input invokes pointer event subscribers individually, catches subscriber exceptions, logs with existing Unity logging, and continues notifying
  remaining subscribers.
- Unity Input production backend owns Enhanced Touch callbacks and editor-only mouse polling. VContainer ticks the backend, not the public facade.
- Player builds use Enhanced Touch only for this mechanic; editor mouse simulation is guarded out of player builds unless a later debug input mode is
  introduced.
- Gameplay State uses asset-backed state and transition identities compared by reference.
- Gameplay State has one current state at a time.
- Gameplay State service requires an explicit initial state from config and fails fast for missing or broken composition.
- Gameplay State config validation is centralized in a pure validator used by the config asset and runtime service construction.
- Gameplay State config validation catches nulls, self-transitions, and duplicate transitions. It does not enforce reachability or full flow
  completeness.
- Gameplay State service exposes current state, `IsCurrent`, `TryTransitionTo`, and typed changing/changed events carrying previous and next state.
- `TryTransitionTo` returns `true` for an actual transition and `false` for same-state or invalid transitions.
- Same-state transitions are no-op and do not warn.
- Invalid transitions warn with existing Unity logging and return false.
- Gameplay State `Changing` event is raised before model mutation. Subscriber exceptions are logged and isolated so mutation can complete, then
  `Changed` is raised after mutation.
- Initial Gameplay State assets are Pre-Launch, Running, and Run Ended.
- Initial Gameplay State Transition assets are Pre-Launch to Running, Running to Run Ended, and Run Ended to Pre-Launch.
- Run Ended represents any finished run outcome for now.
- Gameplay Flow is game-specific orchestration outside generic Gameplay State and outside Slingshot.
- Gameplay Flow listens to `LaunchRequested`, requests transition to Running, and calls the Slingshot launcher only if the transition succeeds.
- Gameplay Flow does not pre-validate launch requests, does not duplicate invalid transition logging, and does not rollback if launch application
  throws after a successful transition.
- Gameplay Flow is configured with Running state id and lets Gameplay State service decide legality from the current state.
- Slingshot launch controller implements the launching boundary and exposes only `ISlingshotLauncher` to consumers.
- Slingshot launch controller subscribes to Gameplay State and holds the Launch Target when entering Pre-Launch. It also holds immediately on
  initialization if the game already starts in Pre-Launch.
- Slingshot launch controller does not authorize flow transitions. It assumes Gameplay Flow already accepted the transition before calling launch.
- Slingshot launch controller validates launch request direction, speeds, Pull Point, and final velocity only at launch boundary. Invalid requests warn
  and skip.
- Slingshot launch controller sets the held Launch Target to the request's final `PullPointWorldPosition` immediately before calling launch, so
  launch starts from the same pulled position described by the request payload.
- Final velocity is horizontal direction multiplied by launch speed plus up direction multiplied by launch up speed.
- Slingshot launcher returns void. Defensive invalid-request skips warn instead of returning a result.
- `ILaunchTarget` exposes `Hold()` and `Launch(velocity)`.
- Held Launch Target positioning is exposed through a separate narrow interface, such as `IHeldLaunchTargetPositioner.SetHeldPosition(Vector3 position)`.
- Launch Target Band contact is exposed through a separate narrow interface, such as `ILaunchTargetBandContactProvider`, so Slingshot can compute
  presentation contact points without leaking Rigidbody or Collider details into the controller.
- `ILaunchTargetBandContactProvider` is backed by an explicitly assigned Launch Target Collider in the Unity adapter, not by a separate
  designer-authored proxy shape in the first slice.
- `ILaunchTargetBandContactProvider` accepts exactly one explicitly assigned Unity `Collider` of any Collider type and uses generic Collider surface
  queries for first-slice contact and wrap data.
- `ILaunchTargetBandContactProvider` does not auto-discover child Colliders or choose among multiple Colliders in the first slice. Multi-collider
  Launch Target shapes require a later compound-shape decision.
- `ILaunchTargetBandContactProvider` calculates contacts from the assigned Collider's current Transform after `SetHeldPosition`. It does not accept a
  virtual Pull Point, target pose, or predicted transform in the first slice.
- `ILaunchTargetBandContactProvider` may use `Collider.ClosestPoint(Vector3)` as the generic first-slice surface query. Exact collider-specific
  silhouette support is deferred.
- The Launch Target Band contact adapter validates the assigned Collider and contact padding through `OnValidate`, and fails fast before use if
  required references are missing.
- Slingshot pull interpretation depends on held positioning, while Slingshot launch application depends on both `ILaunchTarget` and held positioning.
- Held positioning is valid only after the Launch Target has been held; calling `SetHeldPosition` before `Hold()` fails fast instead of implicitly
  holding or silently no-oping.
- Held positioning preserves the Launch Target's rotation in the first slice. The code should keep a TODO at the Launch Target boundary for future
  explicit rotation/orientation support if game feel later needs the target to face the launch direction.
- SlingshotController does not position the held Launch Target during initialization or Pre-Launch state entry; it positions only from pointer
  lifecycle handling after input capture begins, avoiding dependency on VContainer initializer order or Gameplay State subscriber order.
- Rigidbody Launch Target is a shallow MonoBehaviour adapter over an explicitly assigned Rigidbody.
- Rigidbody Launch Target uses `OnValidate` for missing reference authoring validation and production assertions before dereference.
- Rigidbody Launch Target hold preserves previous kinematic state and constraints, sets kinematic true, and clears linear/angular velocity.
- Rigidbody Launch Target can be positioned while held so Active Pull can load the Player into the Slingshot.
- Rigidbody Launch Target held positioning uses immediate Rigidbody/Transform pose assignment while held, not `Rigidbody.MovePosition`, because Band
  contact calculation needs the assigned Collider pose to be observable in the same frame.
- Do not call Unity physics transform synchronization unconditionally after every held target move in the hot path.
- Add a boundary test for same-frame `SetHeldPosition -> Collider.ClosestPoint` correctness. If that test proves explicit Unity physics transform
  synchronization is required, add the narrow sync in the Rigidbody Launch Target or contact-calculation path and document why there.
- Rigidbody Launch Target launch restores saved state if held, clears linear/angular velocity again, and applies velocity change.
- Launch Target launch behaves deterministically even if hold was never called.
- First slice does not add a retry/reset placement controller beyond returning canceled or weak Active Pulls to the Slingshot Rest Point.
- Use existing Unity logging for warnings and exception logging. Structured logging is deferred by ADR.

## Testing Decisions

- Good tests should verify externally visible behavior and contracts, not private implementation details. Use local fakes and public/internal
  testable seams instead of reflection.
- Prefer EditMode NUnit tests for pure controllers, services, validators, input facades, and launch math.
- Run Unity compile before tests. Do not run tests against a broken script compilation state.
- Use NUnit constraint-style assertions in tests.
- Production assertions use fully-qualified Unity assertions.
- Add runtime assembly internals visibility to corresponding EditMode test assemblies where internal seams are required.
- Unity Input EditMode tests:
  - Reference-counted enable/disable ordering.
  - Idempotent enable handle disposal.
  - Enable after Unity Input disposal throws.
  - Outstanding handle disposal after service disposal is a no-op.
  - Backend phase mapping to pressed, moved, released, and canceled events.
  - No events forwarded after the last enable handle is disposed.
  - Subscriber exception isolation and logging, where practical.
- Unity Input tests should use a fake internal backend and must not manipulate real Enhanced Touch global state in EditMode.
- Gameplay State EditMode tests:
  - Initial state construction.
  - Valid transitions mutate state and raise changing/changed in order.
  - Same-state transition returns false with no events and no warning.
  - Invalid transition returns false and warns.
  - Changing subscriber exception is logged and isolated while the transition still completes.
  - Config validator reports nulls, self-transitions, and duplicate transitions with stable typed errors.
- Slingshot config EditMode tests:
  - Validator reports invalid Band Wrap segment counts outside the supported range.
- Slingshot EditMode tests:
  - Initialization while already Pre-Launch enables capture and input.
  - Entering and leaving Pre-Launch acquire/dispose input handles and update view state in the expected order.
  - Leaving Pre-Launch cancels Active Pull and returns visuals to idle before input handle disposal.
  - Disposal unsubscribes from input and state events.
  - Screen-space Band Touch Target accepts nearby presses against the visible rest Band polyline and rejects distant presses.
  - Active Pull captures only the first pointer and ignores others.
  - Pull projection failure cancels the Pull.
  - Forward displacement clamps to zero.
  - Backward and lateral clamping produce expected view state.
  - Weak, forward-only, and canceled Pulls produce no launch request.
  - Active Pull moves the held Launch Target to the clamped Pull Point.
  - Active Pull held target positioning preserves the Launch Target rotation.
  - Active Pull creates Band Shape from target Band Contact Points and Band Wrap, not from the Pull Point as a renderer middle point.
  - Active Pull Band Shape contains visible Band legs, a collider-aligned Band Wrap, and no straight segment through the Launch Target.
  - Active Pull requests Band Contact Points through the target contact provider and does not query Collider data directly.
  - Active Pull positions the held Launch Target before requesting Band Contact Points and Band Wrap data.
  - Active Pull Band Contact Points are derived as left-anchor and right-anchor closest padded points against the assigned target Collider.
  - Geometry validation rejects left anchor, right anchor, or Rest Point authoring that is off the Pull Plane/Band Plane beyond tolerance.
  - Geometry validation does not silently project misaligned anchors or Rest Point to make invalid authoring pass.
  - Geometry validation uses an implementation-owned coplanarity tolerance and does not read tolerance from `SlingshotConfig` or scene-authored
    values.
  - Active Pull Band Shape contains enough ordered visual sample points to represent the configured Band Wrap quality.
  - Active Pull Band Shape respects the configured `BandWrapSegmentCount` while staying within the validated clamp range.
  - Changing `BandWrapSegmentCount` changes visual sample density but does not change launch request power, direction, Pull Offset, or Pull Point.
  - Active Pull Band Shape is one ordered polyline that starts at the left anchor and ends at the right anchor.
  - Forward-only Pull keeps the held Launch Target at the Rest Point.
  - Canceled, invalidly projected, and weak Pull releases return the held Launch Target to the Rest Point.
  - Valid Pull Release keeps the held Launch Target at the final pulled point before launch application.
  - Valid Pull Release keeps the loaded Band Shape through launch request notification instead of resetting visuals before launch handoff.
  - Accepted launch starts Band Release Recoil after launch application.
  - During Band Release Recoil, the Band continues requesting Band Contact Points and Band Wrap from the moving Launch Target collider.
  - Band Release Recoil detaches from the Launch Target when the Band reaches the rest/idle/default shape.
  - Initialization and Pre-Launch state entry do not call held target positioning.
  - Valid Pull Release raises a launch request with expected normalized power, Pull distance, Pull Offset, Pull Point world position, direction,
    speed, and lift.
  - Touch Indicator screen position follows the projected clamped Pull Point, not the raw finger.
- Slingshot tests should use local fakes for Unity Input, Gameplay State, Slingshot view, input projector, and launcher dependencies.
- Slingshot launch EditMode tests:
  - Target is held on initial Pre-Launch and every Pre-Launch re-entry.
  - Valid launch request positions the held Launch Target at the request's final Pull Point before launching.
  - Valid launch request computes final velocity and calls target launch.
  - Invalid request, invalid Pull Point, or invalid final velocity warns and skips held target positioning and target launch.
  - Launcher does not duplicate Gameplay Flow authorization.
- Rigidbody Launch Target tests:
  - Use PlayMode only if Unity Rigidbody behavior requires engine lifecycle.
  - Verify hold preserves/restores kinematic state and constraints and clears velocities.
  - Verify held positioning moves the kinematic Rigidbody deterministically.
  - Verify held positioning preserves Rigidbody rotation.
  - Verify held positioning uses immediate pose assignment rather than `Rigidbody.MovePosition`.
  - Verify held positioning fails fast if called before hold.
  - Verify held positioning updates the assigned Collider pose before same-frame Band Contact Point and Band Wrap calculation.
  - Add a boundary test for same-frame `SetHeldPosition -> Collider.ClosestPoint` correctness before introducing explicit physics transform
    synchronization.
  - Verify Band Contact Point calculation uses the assigned target Collider and contact padding.
  - Verify Band Contact Point and Band Wrap calculation use the single explicitly assigned Collider and do not auto-select from child Colliders.
  - Verify Band Contact Point calculation returns collider-aligned contacts for the assigned target Collider shape.
  - Verify Band Contact Points and Band Wrap samples are projected onto the Slingshot Pull Plane/Band height instead of preserving arbitrary Collider
    vertical closest-point coordinates.
  - Verify generic Collider padding offsets valid `ClosestPoint` results back toward the query origin: anchor for Band Contact Points, arc sample
    origin for Band Wrap samples.
  - Verify degenerate generic Collider padding falls back to Pull Plane bounds-center direction, then `-LaunchFrameForward`, then no padding.
  - Verify Band Wrap calculation returns padded collider-aligned sample points between the left and right Band Contact Points.
  - Verify Band Wrap generation samples from a Pull Plane arc around the Pull Point before Collider projection, not from a linear chord between Band
    Contact Points.
  - Verify asymmetric contact directions choose the backward/pulled Band Wrap arc side instead of the shortest arc when those differ.
  - Verify Band Contact Point and Band Wrap calculation work through the base Collider contract with representative Collider types, not just
    `CapsuleCollider`.
  - Verify generic Band Wrap behavior is stable and padded, without asserting exact arbitrary-collider silhouettes.
  - Verify launch clears stale velocities and applies velocity change.
- Gameplay Flow EditMode tests:
  - Valid launch request transitions to Running before calling launcher.
  - Failed transition skips launcher.
  - Launcher exception after successful transition propagates and no rollback occurs.
  - Dispose unsubscribes without mutating state.
- Scene/composition verification:
  - Add one PlayMode smoke test if practical to load or exercise the Gameplay Scene composition.
  - Verify missing serialized references fail fast through validation or runtime assertions.
  - Verify the Band LineRenderer receives the full ordered Band Shape polyline instead of collapsing the Band Shape to three points.
  - Verify accepted launch shows Band Release Recoil after the shot is applied, following the Launch Target contact points until the Band reaches
    rest/idle/default shape.
  - Use manual Unity smoke testing for touch feel, gizmo usefulness, UI positioning, and actual device comfort.
- Do not hardcode asset paths in tests if test assets are required; use the project's typed test asset provider convention if/when such assets are
  introduced.
- Do not rely on runtime MonoBehaviour callbacks in EditMode tests for runtime scripts.
- No Burst compile guard tests are needed in the first slice because Burst and rope physics are out of scope.

## Release and Compatibility

- Unity version assumption: Unity 6 project using the New Input System, URP, UGUI, and Unity Test Framework.
- The project already has New Input System active and Input System installed.
- The existing input actions asset remains in the project, but Slingshot input uses Enhanced Touch directly through Unity Input.
- This feature introduces VContainer as a new project dependency through the package manifest. The implementation must pin a release tag and commit
  package lock changes.
- This feature does not introduce `com.unity.logging`; structured logging remains a future ADR-backed refactor.
- This feature adds new asmdefs and test asmdefs; dependencies must remain acyclic:
  - Unity Input must not depend on Slingshot or Gameplay State.
  - Slingshot may depend on Unity Input and Gameplay State.
  - Gameplay Flow may depend on Slingshot and Gameplay State.
  - Generic Gameplay State must not depend on Slingshot or Gameplay Flow.
- Backward compatibility risk is low because the repo currently has no gameplay C# source or asmdefs, but scene serialization and package changes
  must be reviewed carefully.
- The Gameplay Scene will intentionally change. Scene references must be stable, explicit, and validated.
- Mobile runtime behavior targets touch input. Editor mouse support is a development convenience only.
- Package/version release notes are not required unless this project later adopts a package changelog policy.

## Out of Scope

- Real rope physics, joints, cloth, spline simulation, Burst-optimized rope logic, or physically simulated Band tension.
- Non-planar Slingshot Band geometry or 3D anchor/Rest Point layouts.
- Compound or multi-collider Launch Target Band contact/wrap solving.
- Multi-touch gameplay support beyond ignoring non-captured touches.
- Haptics, audio, particles, camera transition, Timeline/Feel launch presentation, or a full Launch Sequence.
- Delaying physics launch until a presentation sequence finishes.
- Retry placement controller beyond returning canceled or weak Active Pulls to the current Slingshot Rest Point.
- Run end detection, crash detection, lost momentum detection, or level completion rules.
- Save/load representation of Gameplay State.
- Remote issue creation or tracker publishing.
- Custom inspectors, editor windows, or editor-only authoring tools beyond selected-object gizmos.
- Structured logging backend or Unity Logging package integration.
- UI hit-test blocking policy.
- Device-specific DPI scaling for touch target radius.
- Runtime mouse input in player builds.
- Migrating existing gameplay source, because no gameplay source exists yet.

## Further Notes

- The PRD is based on the completed design/grilling session, the domain glossary, and approved ADRs for Gameplay State, shallow MonoBehaviours, direct
  C# events, VContainer, non-injected views, and Unity Input.
- Deep modules to protect during implementation:
  - Unity Input: small input-facing API over global Enhanced Touch state and editor input simulation.
  - Gameplay State: reusable asset-backed state service with pure validation.
  - Slingshot Controller: deterministic Pull interpretation and launch-request creation.
  - Slingshot Launch Controller: launch request validation and Launch Target application.
  - Gameplay Flow: small game-specific transition-before-launch coordinator.
- The implementation should follow the repo workflow: discovery, exploration, plan, red tests, green implementation, refactor, verify, summary.
- The first implementation should be intentionally narrow but complete enough for the player to launch the Player from the Gameplay Scene.
