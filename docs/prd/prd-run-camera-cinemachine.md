## Problem Statement

The current gameplay camera stays fixed while the **Launch Target** begins a **Run**. Once the **Slingshot** launches the **Launch Target**, the player
can no longer reliably see the controlled object as it slides downhill, jumps, or moves around level geometry. A simple follow camera is not enough:
the camera must avoid entering slopes, showing below slope surfaces, or clipping into solid 3D object interiors.

The camera behavior also must not disturb the **Pre-Launch** interaction. The current rendered camera is used for **Slingshot** input projection, so
camera movement before **Launch** can change how **Pointer Input** maps onto the **Pull Plane**. The feature therefore needs a camera handoff that
keeps **Pre-Launch** stable, activates only after launch physics is applied, and follows a stable camera-facing reference rather than raw physics
rotation.

The implementation must respect the project architecture: gameplay behavior belongs in plain C# controllers and services, MonoBehaviours stay shallow,
scene-authored references are composed explicitly through VContainer, and camera terminology follows the project glossary: **Pre-Launch Camera**,
**Run Camera**, **Run Camera Anchor**, **Launch Target**, **Run**, and **Gameplay State**.

## Solution

Add a launch-gated Cinemachine-based camera system for the **Run Camera**.

The real Unity Main Camera remains the rendered camera and the **Slingshot** input camera. It receives a Cinemachine Brain. During **Pre-Launch**, a
scene-authored **Pre-Launch Camera** matches the current fixed Main Camera shot so **Pull** projection remains stable. After the **Launch Target** has
actually received launch velocity and the **Gameplay State** is Running, a scene-authored **Run Camera** takes over and follows a **Run Camera Anchor**.

The **Run Camera Anchor** is a project-owned reference point derived from the **Launch Target**. It smooths position, applies an authored height/look
offset, and faces the **Launch Target**'s planar velocity direction with a minimum-speed fallback to the last valid yaw. It does not copy the
**Launch Target** transform rotation, slope normal, or physics wobble.

Cinemachine owns camera composition, damping, blends, Third Person Follow behavior, and Decollider-based terrain/object safety. Project code owns only
domain timing, **Run Camera Anchor** motion, and activation/deactivation. Explicit camera collision layers such as `CameraTerrain` and
`CameraObstacle` define what the **Run Camera** treats as blocking geometry. The first slice uses Third Person Follow plus Decollider; Deoccluder is
deferred until line-of-sight blockers are proven to be a problem in playtesting.

## Unity Surfaces

- Runtime assemblies and asmdefs:
  - Gameplay runtime assembly gains **Run Camera** lifecycle, **Run Camera Anchor** motion, `RunCameraConfig`, and narrow camera rig/source adapter
    contracts.
  - Gameplay runtime assembly may need a Cinemachine assembly reference if a shallow camera rig adapter manipulates Cinemachine-specific camera
    priority or activation APIs directly.
  - Foundation runtime assembly remains the place for generic wrappers only if additional time/screen abstractions are needed; camera domain behavior
    should remain in Gameplay.
  - Slingshot runtime remains the source of launch-applied notification and should not own camera behavior.
  - Input runtime remains unchanged; the **Run Camera** does not consume **Pointer Input**.
- Editor assemblies, windows, inspectors, importers, or menu items:
  - No custom Editor assembly is required for the first slice.
  - Optional selected-object gizmos may be added for the **Run Camera Anchor** if useful, but they are not required for the first implementation.
- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:
  - Package manifest and lock file add Unity Cinemachine.
  - Gameplay Scene Main Camera receives a Cinemachine Brain.
  - Gameplay Scene gains a **Pre-Launch Camera** matching the current fixed shot.
  - Gameplay Scene gains a **Run Camera** configured with Third Person Follow and Decollider.
  - Gameplay Scene gains a **Run Camera Anchor** scene object or adapter target.
  - Gameplay composition gains serialized references for **Run Camera** config, anchor, camera rig adapter, and **Launch Target** camera source.
  - A `RunCameraConfig` ScriptableObject owns project-controlled **Run Camera Anchor** and lifecycle tuning values.
  - Cinemachine component settings remain scene-authored on Cinemachine components.
  - ProjectSettings layer names add explicit camera geometry layers such as `CameraTerrain` and `CameraObstacle`.
  - Level surfaces and solid objects that should affect the **Run Camera** must be assigned to camera geometry layers.
- RPC/helper commands, hooks, or shell wrappers:
  - Unity AI Agent Connector compile and test commands remain the primary verification path.
  - No new helper commands are required.
- Package versioning, changelog, and installation/sync behavior:
  - This is project gameplay work, not a distributable package release.
  - Cinemachine becomes a package dependency and must be captured in package manifest and lock/sync behavior.
  - No player save migration is expected.

## User Stories

1. As a player, I want the camera to follow the **Launch Target** after **Launch**, so that I can keep playing during the **Run**.
2. As a player, I want the camera to stay fixed during **Pre-Launch**, so that pulling the **Band** feels stable.
3. As a player, I want the **Run Camera** to start only after the **Launch Target** has actually launched, so that the camera does not move too early.
4. As a player, I want the camera to follow downhill slides, so that I can see where the **Launch Target** is going.
5. As a player, I want the camera to follow jumps, so that I do not lose sight of the **Launch Target** in the air.
6. As a player, I want the camera to stay outside slopes, so that I never see below the slope surface.
7. As a player, I want the camera to stay outside solid props and walls, so that I do not see object interiors.
8. As a player, I want camera movement to be smooth, so that sliding and steering feel casual rather than jittery.
9. As a player, I want the camera to look where the **Launch Target** is traveling, so that the view supports steering decisions.
10. As a player, I want the camera not to copy physics wobble, so that collisions and slope contacts do not shake the view unnecessarily.
11. As a player, I want the camera not to snap harshly from **Pre-Launch** to **Run**, so that launch feels polished.
12. As a player, I want camera collision correction to be unobtrusive, so that obstacle avoidance does not feel like an unexpected camera swing.
13. As a designer, I want a **Pre-Launch Camera**, so that the **Slingshot** framing can be authored independently from the **Run Camera**.
14. As a designer, I want a **Run Camera**, so that run framing can be tuned without changing **Pre-Launch** input behavior.
15. As a designer, I want a **Run Camera Anchor**, so that camera framing can stay stable even when the **Launch Target** physics body moves abruptly.
16. As a designer, I want Cinemachine Third Person Follow settings in the scene, so that camera distance, offsets, and damping can be tuned in familiar
    Unity components.
17. As a designer, I want Decollider settings in the scene, so that camera-object and camera-terrain safety can be tuned where level geometry is visible.
18. As a designer, I want explicit camera terrain and obstacle layers, so that the camera reacts only to intended geometry.
19. As a designer, I want **Run Camera Anchor** height and look offsets in a config asset, so that gameplay-adapter framing can be tuned without code.
20. As a designer, I want anchor smoothing values in a config asset, so that the follow target can be adjusted independently from Cinemachine damping.
21. As a designer, I want a minimum velocity threshold for anchor yaw, so that the camera does not spin when the **Launch Target** nearly stops.
22. As a designer, I want the **Run Camera** to ignore the **Launch Target**, **Band**, UI, triggers, collectibles, and non-blocking decorations, so that
    camera avoidance does not react to gameplay helpers.
23. As a designer, I want **Pre-Launch Camera** framing to match the current camera pose initially, so that existing **Pull** projection tests remain
    meaningful.
24. As a developer, I want **Run Camera** activation gated by `LaunchApplied` and Running **Gameplay State**, so that camera movement starts only after
    launch physics is applied.
25. As a developer, I want **Run Camera** deactivation when leaving Running, so that camera state cannot leak into future phases.
26. As a developer, I want project code to own **Run Camera Anchor** motion, so that domain timing remains testable.
27. As a developer, I want Cinemachine to own camera composition and collision, so that the project does not maintain custom camera collision math.
28. As a developer, I want Cinemachine APIs isolated behind a narrow camera rig adapter if referenced from gameplay code, so that tests can use fakes.
29. As a developer, I want **Run Camera Anchor** source data behind a narrow adapter, so that camera logic does not depend directly on Rigidbody details.
30. As a developer, I want **Run Camera Anchor** yaw to use planar velocity, so that visual/body rotation does not control camera direction.
31. As a developer, I want anchor yaw to preserve the last valid direction below minimum speed, so that the camera does not jitter at low velocity.
32. As a developer, I want the anchor update loop to run after relevant **Launch Target** movement has been applied, so that the camera tracks current
    motion.
33. As a developer, I want the real Main Camera to remain the input camera, so that **Slingshot** projection has one authoritative camera reference.
34. As a developer, I want the **Pre-Launch Camera** to be live before **Launch**, so that Cinemachine has a known source shot for blends.
35. As a developer, I want the **Run Camera** to become live through priority or activation changes, so that transition behavior is explicit.
36. As a developer, I want Cinemachine Brain update settings chosen deliberately, so that physics-driven target motion does not produce avoidable jitter.
37. As a developer, I want `RunCameraConfig` to avoid duplicating Cinemachine settings, so that there are not two sources of truth.
38. As a developer, I want `RunCameraConfig` validation, so that invalid smoothing, offsets, or speed thresholds fail visibly.
39. As a developer, I want Gameplay composition validation to include all camera references, so that missing scene wiring fails early.
40. As a developer, I want ProjectSettings layer changes to be explicit, so that camera collision filters can be reviewed.
41. As a developer, I want assembly definition references updated only where necessary, so that Cinemachine does not leak across unrelated assemblies.
42. As a developer, I want existing **PlayerSteeringController** behavior to remain independent, so that steering and camera follow can evolve separately.
43. As a developer, I want existing **Slingshot** behavior to remain independent, so that **Pull** interpretation does not know about the camera handoff.
44. As a developer, I want the **Run Camera** not to affect launch force, **Pull Offset**, **Launch Frame**, or **Pull Plane**, so that camera work cannot
    change launch math.
45. As a tester, I want EditMode tests for camera lifecycle gating, so that `LaunchApplied + Running` behavior is covered without scene loading.
46. As a tester, I want EditMode tests for anchor yaw and smoothing, so that low-speed and velocity-facing behavior is deterministic.
47. As a tester, I want fakes for camera rig activation, so that tests do not depend on Cinemachine runtime components.
48. As a tester, I want PlayMode composition tests for the Gameplay Scene, so that missing Brain, cameras, layers, and config wiring are caught.
49. As a tester, I want PlayMode smoke checks for terrain/object non-clipping where practical, so that the hard camera safety requirement has coverage.
50. As a tester, I want existing **Pre-Launch** input tests to continue passing, so that camera conversion does not break **Pull** projection.
51. As a maintainer, I want ADR-0009 to describe the Cinemachine dependency decision, so that future contributors do not replace it with custom camera
    collision casually.
52. As a maintainer, I want **Run Camera** terminology in the glossary, so that implementation and tests avoid ambiguous "player camera" language.
53. As a maintainer, I want Deoccluder deferred, so that first-slice camera behavior stays stable and only solves confirmed problems.
54. As a maintainer, I want exact framing values treated as tuning, so that the first implementation can start with sensible defaults and be playtested.
55. As a level author, I want solid camera-blocking level geometry clearly layered, so that camera collision behavior is predictable.
56. As a level author, I want non-blocking decorative objects excluded from camera geometry layers, so that decoration does not cause camera pops.
57. As a level author, I want slopes assigned to camera terrain, so that the **Run Camera** does not dip below the playable surface.
58. As a level author, I want walls and large props assigned to camera obstacle, so that object interiors are not visible.
59. As a player, I want the **Run Camera** to recover smoothly after obstacle correction, so that temporary camera safety adjustments do not feel broken.
60. As a player, I want future line-of-sight improvements only if needed, so that the first camera does not overreact around every obstacle.

## Implementation Decisions

- Use the project glossary terms **Pre-Launch Camera**, **Run Camera**, **Run Camera Anchor**, **Launch Target**, **Run**, **Launch**, **Gameplay State**,
  and **Gameplay Flow**.
- Implement the **Run Camera** using Unity Cinemachine as recorded in ADR-0009.
- Add Cinemachine as an explicit package dependency and let Unity package resolution capture the exact installed version.
- The real Unity Main Camera remains the render camera and the **Slingshot** input camera.
- Add a Cinemachine Brain to the real Main Camera.
- Represent the fixed **Pre-Launch Camera** as a Cinemachine camera matching the current fixed camera pose and lens values.
- Represent the **Run Camera** as a separate Cinemachine camera that follows the **Run Camera Anchor**.
- The **Run Camera** activates only when launch has been applied and the current **Gameplay State** is Running.
- Leaving Running deactivates the **Run Camera** and resets launch-gated camera lifecycle state.
- Do not activate the **Run Camera** on Running alone, because current flow transitions to Running before launch velocity is applied.
- Maintain the **Run Camera Anchor** as project-owned behavior derived from the **Launch Target**.
- The **Run Camera Anchor** uses **Launch Target** position plus configured height/look offsets.
- The **Run Camera Anchor** yaw follows planar **Launch Target** velocity, not body rotation, visual rotation, or slope normal.
- The **Run Camera Anchor** keeps the last valid yaw when planar velocity is below a configured minimum speed.
- Anchor position and yaw smoothing are project-owned and configurable through `RunCameraConfig`.
- Cinemachine component settings remain in Cinemachine scene components, not duplicated into `RunCameraConfig`.
- Cinemachine owns Third Person Follow composition, distance, offsets, and camera damping.
- Cinemachine owns Decollider terrain/object correction for the first slice.
- Deoccluder is deferred until line-of-sight blockers are confirmed in playtesting.
- Use explicit camera geometry layers such as `CameraTerrain` and `CameraObstacle`.
- The **Run Camera** collision and decollision filters include only intended camera geometry layers.
- The **Launch Target**, **Band**, UI, triggers, collectibles, and non-blocking decoration are excluded from camera obstacle filters.
- Add a `RunCameraConfig` ScriptableObject and a narrow config interface for project-owned anchor and lifecycle values.
- `RunCameraConfig` should include anchor offset, look offset if separate from anchor position, position smoothing, yaw smoothing, minimum yaw speed,
  and lifecycle priority/activation values if project code controls them.
- Do not put Cinemachine Third Person Follow offsets, Decollider radius, Decollider damping, or Brain blend settings in `RunCameraConfig`.
- Add a camera source adapter for the **Launch Target** that exposes only the data the anchor controller needs, such as position and linear velocity.
- Keep the camera source adapter shallow and Unity-facing.
- Add a camera anchor adapter or view that exposes the Transform-like target that Cinemachine follows.
- Add a camera rig adapter for activating the **Pre-Launch Camera** and **Run Camera** without leaking Cinemachine types into pure controller tests.
- If the camera rig adapter references Cinemachine APIs directly, keep that dependency at the adapter boundary and add the necessary asmdef reference.
- Add a plain C# **Run Camera** controller or service for lifecycle gating and anchor updates.
- The controller listens to launch-applied notification and **Gameplay State** changes, mirroring the proven steering activation pattern.
- The controller should be registered through the existing VContainer gameplay composition root.
- The controller should be testable with fake gameplay state, fake launch notification, fake camera source, fake anchor, fake rig, fake time, and fake config.
- The **Slingshot** controller should not know about **Run Camera**.
- The **PlayerSteeringController** should not own camera behavior.
- The camera implementation should not change **Launch Frame**, **Pull Plane**, **Pull Offset**, launch force, or steering math.
- Existing **Pre-Launch** camera projection behavior must remain stable before **Launch**.
- Scene validation should fail fast when required camera config, camera rig, anchor, or camera source references are missing.
- Layer authoring should be reviewed as part of scene setup because camera safety depends on correct collision filters.

## Testing Decisions

- Good tests verify observable camera contracts: lifecycle activation, anchor pose output, camera rig activation, scene composition, and collision-layer
  policy. They should not assert private helper methods or Cinemachine internals.
- Prefer EditMode tests for lifecycle gating, anchor yaw/position behavior, config validation, and camera rig adapter contract.
- Use PlayMode tests for scene wiring, Unity component presence, Cinemachine component setup, and engine-level camera collision smoke checks.
- Add EditMode tests that **Run Camera** activation requires both launch-applied notification and Running **Gameplay State**.
- Add EditMode tests that Running without launch-applied notification does not activate the **Run Camera**.
- Add EditMode tests that launch-applied notification outside Running does not activate the **Run Camera** until Running is reached.
- Add EditMode tests that leaving Running deactivates the **Run Camera** and clears launch-gated state.
- Add EditMode tests that repeated launch or state events are idempotent.
- Add EditMode tests that **Run Camera Anchor** position uses the configured offset from the **Launch Target**.
- Add EditMode tests that **Run Camera Anchor** yaw follows planar velocity.
- Add EditMode tests that **Run Camera Anchor** yaw ignores physics/body rotation.
- Add EditMode tests that low planar velocity preserves the last valid yaw.
- Add EditMode tests that invalid or degenerate velocity does not produce invalid rotations.
- Add EditMode tests that position and yaw smoothing respond to configured rates and time delta.
- Add EditMode tests that `RunCameraConfig` clamps or rejects invalid values.
- Add EditMode tests that camera controller uses only narrow rig/source/anchor interfaces.
- Add composition tests that gameplay LifetimeScope requires **Run Camera** references once the feature is wired.
- Add composition tests that scene-resolved **Run Camera** config is assigned exactly once.
- Add PlayMode scene tests that Main Camera has Cinemachine Brain.
- Add PlayMode scene tests that **Pre-Launch Camera** and **Run Camera** are present and assigned.
- Add PlayMode scene tests that **Run Camera** includes Third Person Follow and Decollider in the first slice.
- Add PlayMode scene tests that Deoccluder is not required in the first slice.
- Add PlayMode scene tests that `CameraTerrain` and `CameraObstacle` layers exist.
- Add PlayMode scene tests that the current surface uses camera terrain where practical.
- Add PlayMode smoke tests that **Pre-Launch** screen projection behavior still works for **Slingshot** input.
- Add PlayMode smoke tests that after a valid launch, the active camera changes to the **Run Camera**.
- Add PlayMode smoke tests for a simple slope/object arrangement where camera position remains outside camera geometry if practical.
- Keep tests focused on external behavior; do not assert Cinemachine private pipeline details.
- Run Unity script compilation before targeted tests.
- Run targeted EditMode camera tests, targeted PlayMode scene composition tests, and `git diff --check`.
- Manual Unity smoke testing should verify launch handoff, sliding follow, jump follow, slope clipping, object interior clipping, anchor smoothness, and
  camera feel on a touch device or editor simulation.

## Release and Compatibility

- Unity version assumption: the project currently targets Unity 6000.3.x.
- The implementation adds Unity Cinemachine as a package dependency, expected to be a Unity-supported Cinemachine 3.x-compatible package.
- The package manifest and lock file will change.
- Gameplay asmdefs may need additional Cinemachine references only where Cinemachine APIs are directly referenced.
- Gameplay Scene serialization will change for Main Camera Brain, Cinemachine cameras, **Run Camera Anchor**, config references, and layer assignments.
- ProjectSettings TagManager will change to add camera geometry layers.
- Existing **Pre-Launch** tests and behavior are compatibility-sensitive because the Main Camera remains the **Slingshot** input camera.
- Existing run steering should remain compatible because steering remains activated by launch-applied plus Running and mutates movement independently.
- Existing **Slingshot** launch math should remain compatible; camera work must not affect **Pull**, **Launch Frame**, **Pull Plane**, or velocity application.
- No remote issue publishing, package release, changelog release entry, or save migration is required for the PRD itself.

## Out of Scope

- Implementing run-ending conditions, results, bounds, finish line, or obstacle-crash semantics.
- Changing **Slingshot** input, **Pull**, **Pull Offset**, launch force, **Band Shape**, or **Band Release Recoil** behavior.
- Changing post-launch steering values or steering algorithm.
- Making the camera a Rigidbody or physics-simulated object.
- Writing custom camera collision, custom line-of-sight avoidance, or custom terrain clipping logic for the first slice.
- Enabling Cinemachine Deoccluder in the first slice unless playtesting proves target occlusion is a real problem.
- Adding custom Editor windows or inspectors.
- Solving all possible level-authoring mistakes automatically.
- Finalizing exact camera distance, shoulder offset, damping, blend duration, and Decollider numeric values before playtesting.

## Further Notes

- This PRD is based on the resolved camera grill decisions through Q10 and ADR-0009.
- Assumption: `CameraTerrain` and `CameraObstacle` are acceptable initial layer names.
- Assumption: Cinemachine scene components are the right source of truth for Cinemachine-authored settings.
- Assumption: a project `RunCameraConfig` is still useful for **Run Camera Anchor** motion and lifecycle values.
- Assumption: first-slice camera safety should prioritize not entering terrain/objects over preserving perfect line of sight.
- Unresolved question: exact camera framing values remain tuning work for implementation and playtesting.
- Unresolved question: exact Cinemachine package version should be resolved through Unity Package Manager when implementing.
- Unresolved question: the exact update phase for anchor motion should be chosen during implementation based on Rigidbody/interpolation behavior and
  verified against camera jitter.
