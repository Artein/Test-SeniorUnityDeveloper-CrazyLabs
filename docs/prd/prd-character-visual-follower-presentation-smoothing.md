# PRD: Character Visual Follower Presentation Smoothing

## Problem Statement

The player can see a small but noticeable flicker in the visible **Character** while it moves along the banked sides of the U-shaped **Run Surface**. The camera does not show the same jitter, which means the problem is not simply "the whole player motion is unstable." It is specifically a presentation problem: the visible **Character** inherits or applies small pose changes that the player reads as avatar jitter.

The game already treats the Rigidbody-backed **Launch Target** as gameplay truth and the **Character** as presentation only. That architecture is correct. The issue is that the visible **Character Visual Anchor** currently follows the physical target too directly. Even after **Run Steering Frame** stabilization, tiny render-time or hierarchy-time pose changes can still appear on the model, especially on side banks where support normals, target rotation, and surface orientation change continuously.

From the player's perspective, the side bank looks smooth and the camera feels stable, so the **Character** flicker feels illogical. The goal is not to hide real physics, fake containment, or change steering. The goal is to make the visible **Character** follow the **Launch Target** with presentation-grade smoothing so small visual noise does not break readability.

## Solution

Add a **Character Visual Follower** in the Character Presentation layer. It produces a bounded **Smoothed Character Pose** for the **Character Visual Anchor** while leaving the Rigidbody, colliders, camera source, run progress, steering, support probes, and run-end systems untouched.

The **Character Visual Follower** samples the **Launch Target** Transform render pose in the presentation late update phase. It must not sample raw Rigidbody position for the visual pose, because the visible **Character** should follow Unity's interpolated render pose rather than the fixed-tick physics pose.

The follower should keep position tight, keep heading fairly tight, and smooth up/tilt more strongly. This targets the visible problem: the avatar should not visibly lag behind the controlled body, but small bank-normal and tilt changes should not shake the model. Distance and angle failsafes snap the visual pose when smoothing would create an obvious mismatch.

The implementation should stay local to Character Presentation:

- **Launch Target** remains gameplay and physics truth.
- **Smoothed Character Pose** remains visual-only.
- Gameplay systems must not consume the smoothed pose.
- The passive view owns serialized **Character Visual Follow Tuning**.
- A VContainer-owned presentation entry point owns lifecycle, smoothing, and event subscriptions.
- The scene should not rely on **Character Visual Anchor** being a child of the **Launch Target** hierarchy. The anchor is a presentation-space role, not a gameplay transform-child requirement.

## Unity Surfaces

Runtime assemblies and asmdefs:

- Gameplay runtime assembly that owns Character Presentation, gameplay composition, and VContainer entry points.
- Character Presentation runtime namespace for the **Character Visual Follower**, its passive view/tuning seam, and its testable pose-smoothing module.
- Existing gameplay state service for lifecycle snap points.
- Existing slingshot launch-applied notifier for launch pose snap points.
- Existing foundation time abstraction for render-frame delta time.
- Existing VContainer registration patterns for plain C# controllers and shallow scene views.
- No new runtime assembly definition is expected.

Editor assemblies, windows, inspectors, importers, or menu items:

- No new Editor assembly, custom inspector, importer, or menu item is required.
- Existing Unity serialization and inspector display are enough for **Character Visual Follow Tuning**.
- Existing authoring validation should be extended only if implementation reveals an unsafe scene-wiring failure mode.

Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:

- Gameplay Scene wiring for the **Character Visual Anchor**, **Character**, **Launch Target**, and composition root.
- Project-owned Ladybug **Character** prefab or scene instance carrying the passive Character Presentation view.
- Character Presentation view serialized fields for visual follow tuning and visual anchor reference.
- Scene hierarchy should treat the **Character Visual Anchor** as presentation-owned. It should not require child-of-Launch-Target hierarchy for correctness.
- Existing Animator Controller, Animator parameters, root-motion disabled rule, and **Character Presentation Mode** behavior remain unchanged.
- No new ScriptableObject config asset is required.
- No package manifest, ProjectSettings, Unity version, or Addressables schema change is required.

RPC/helper commands, hooks, or shell wrappers:

- Use the existing Unity AI Agent connector workflow for compile and targeted tests during implementation.
- No new RPC command, shell wrapper, or hook is needed.

Package versioning, changelog, and installation/sync behavior:

- This is local game feature work, not a distributable package release.
- No install, sync, or package migration behavior is required.
- If a project changelog is maintained later, this should be described as adding presentation-only character visual smoothing for banked-surface traversal.

## User Stories

1. As a player, I want the visible **Character** to stop flickering on smooth-looking side banks, so that the run feels visually stable.
2. As a player, I want the camera and **Character** to feel equally stable, so that the avatar does not look disconnected from the rest of the run.
3. As a player, I want high side-bank traversal to read as intentional sliding, so that banked movement feels like part of the course.
4. As a player, I want small surface or pose noise not to shake the **Character**, so that visual feedback stays comfortable.
5. As a player, I want the **Character** to remain close to the **Launch Target**, so that smoothing does not look like input lag.
6. As a player, I want the **Character** to snap back after teleports or resets, so that it never drifts away from the controlled body.
7. As a player, I want launch release to show an immediately aligned **Character**, so that the shot starts cleanly.
8. As a player, I want pre-launch and run-preparation states to show a clean pose, so that aiming and restarting do not inherit old jitter.
9. As a player, I want run-ended presentation to settle cleanly, so that victory or defeat does not leave the avatar vibrating.
10. As a player, I want real movement and collision outcomes to remain unchanged, so that the game still feels physical.
11. As a player, I want the visual smoothing to be invisible when motion is already clean, so that the **Character** simply looks correct.
12. As a player, I want the **Character** not to visibly cut through or detach from its physical body, so that the avatar remains believable.
13. As a player, I want sideways sliding on the U-shape to look smooth, so that side-bank recovery feels fair.
14. As a player, I want small orientation changes on the bank to look natural, so that the avatar does not appear to twitch.
15. As a player, I want big real pose changes to appear immediately enough, so that real transitions remain readable.
16. As a player, I want no change to steering controls, so that this fix does not alter how I play.
17. As a player, I want no change to launch power, friction, or run-end thresholds, so that this fix addresses only visible jitter.
18. As a player, I want the **Character** to keep showing Slide, Idle, Airborne, Victory, and Defeat consistently, so that animation state behavior is not regressed.
19. As a designer, I want **Character Visual Follow Tuning** to be visible in the Character Presentation authoring surface, so that feel can be adjusted without code changes.
20. As a designer, I want position response to be tight, so that the visible avatar remains attached to the controlled body.
21. As a designer, I want heading response to be tight but not noisy, so that the **Character** faces movement convincingly.
22. As a designer, I want up/tilt response to be softer, so that bank-normal noise does not shake the model.
23. As a designer, I want maximum position lag to be clamped, so that smoothing cannot create a visible offset.
24. As a designer, I want snap distance to be tunable, so that teleports and resets recover immediately.
25. As a designer, I want snap angle to be tunable, so that large pose discontinuities do not smooth through ugly rotations.
26. As a designer, I want default values that reduce side-bank jitter in the first pass, so that manual review starts from a plausible feel.
27. As a designer, I want tuning to live with Character Presentation, so that it is not confused with movement, steering, upgrade, or economy tuning.
28. As a designer, I want no new **Run Surface** smoothing, so that raw physics support facts remain debuggable.
29. As a designer, I want no hidden wall or forced recentering, so that side-bank risk remains authored by the course.
30. As a designer, I want no new animation requirement, so that this fix can land before a dedicated animation polish pass.
31. As a designer, I want a later visual flavor pass to remain possible, so that smoothing does not block future lean or banking polish.
32. As a technical artist, I want root motion to remain disabled, so that imported animation cannot move the **Launch Target**.
33. As a technical artist, I want the Animator Controller and parameters to keep working, so that smoothing does not disrupt animation state authoring.
34. As a technical artist, I want the **Character Model Root** alignment role to stay separate, so that imported model offsets are not mixed with follow smoothing.
35. As a technical artist, I want the **Character Visual Anchor** to be the pose target, so that model alignment under it stays stable.
36. As a technical artist, I want the **Character Visual Anchor** role not to imply Unity transform parenting, so that scene hierarchy can be changed safely.
37. As a gameplay engineer, I want the Rigidbody-backed **Launch Target** to remain the only movement truth, so that physics behavior remains coherent.
38. As a gameplay engineer, I want **Smoothed Character Pose** to stay presentation-only, so that gameplay systems cannot depend on filtered visuals.
39. As a gameplay engineer, I want the follower owned by a plain C# VContainer entry point, so that behavior is deterministic and testable.
40. As a gameplay engineer, I want the view to remain passive, so that MonoBehaviours do not accumulate controller logic.
41. As a gameplay engineer, I want a narrow internal view/tuning seam, so that tests can drive the follower without public gameplay API.
42. As a gameplay engineer, I want the smoothing math isolated as a deep module, so that complex pose filtering is testable without scene setup.
43. As a gameplay engineer, I want the follower to sample the **Launch Target** Transform render pose, so that it follows the rendered physics body.
44. As a gameplay engineer, I want the follower not to read raw Rigidbody pose for visual smoothing, so that it does not fight interpolation.
45. As a gameplay engineer, I want lifecycle snap events to be explicit, so that old smoothed state cannot leak between runs.
46. As a gameplay engineer, I want initialization to snap, so that the first visible frame is correct.
47. As a gameplay engineer, I want entering **Run Preparation** to snap, so that restart presentation is clean.
48. As a gameplay engineer, I want entering **Pre-Launch** to snap, so that aiming starts from the authored rig pose.
49. As a gameplay engineer, I want launch-applied to snap, so that launch handoff does not interpolate from stale pre-launch pose.
50. As a gameplay engineer, I want entering **Run Ended** to snap or settle predictably, so that terminal presentation is stable.
51. As a gameplay engineer, I want distance and angle failsafes, so that smoothing cannot create long-lived visible divergence.
52. As a gameplay engineer, I want no dependency from steering, progress, surface detection, or run end to the visual follower, so that dependency direction remains clean.
53. As a gameplay engineer, I want composition tests to prove the follower is registered once, so that lifecycle and state are not split across instances.
54. As a gameplay engineer, I want scene composition tests to prove the **Character** remains visual-only, so that no colliders or Rigidbody appear under the avatar.
55. As a gameplay engineer, I want the existing **Character Presenter** to keep owning animation frames, so that animation mode classification is not mixed with visual follow smoothing.
56. As a gameplay engineer, I want no new package or Unity version change, so that the implementation stays low risk.
57. As a gameplay engineer, I want no `CharacterController` migration, so that this remains a presentation fix.
58. As a QA engineer, I want EditMode tests proving position smoothing stays within the lag clamp, so that the avatar cannot drift visibly.
59. As a QA engineer, I want EditMode tests proving up/tilt smoothing is softer than heading, so that the side-bank jitter target is covered.
60. As a QA engineer, I want EditMode tests proving heading remains tight, so that smoothing does not make the avatar look late.
61. As a QA engineer, I want EditMode tests proving snap distance works, so that teleports and resets are safe.
62. As a QA engineer, I want EditMode tests proving snap angle works, so that large rotations do not interpolate through bad poses.
63. As a QA engineer, I want EditMode tests proving invalid target poses do not create invalid visual poses, so that NaN or degenerate vectors are contained.
64. As a QA engineer, I want controller tests proving lifecycle snap events, so that run restarts and launch handoff are protected.
65. As a QA engineer, I want PlayMode composition tests proving serialized tuning defaults, so that scene authoring cannot regress silently.
66. As a QA engineer, I want PlayMode composition tests proving the follower view and tuning resolve correctly, so that VContainer wiring is protected.
67. As a QA engineer, I want no fragile camera-capture oracle in the first implementation, so that automated tests stay deterministic.
68. As a QA engineer, I want a manual side-bank smoke checklist, so that the actual user-facing jitter is reviewed in Unity.
69. As a maintainer, I want docs to use **Character Visual Follower** and **Smoothed Character Pose** consistently, so that future work does not confuse visuals with physics.
70. As a maintainer, I want this PRD to stay local to Character Presentation, so that future movement fixes do not accidentally inherit presentation smoothing.
71. As a maintainer, I want current scene composition assertions updated intentionally, so that old child-of-Launch-Target assumptions do not fight the new boundary.
72. As a maintainer, I want any later broader rule for physics-backed visuals to be handled separately, so that this PRD does not overreach.

## Implementation Decisions

- Preserve the Rigidbody-backed **Launch Target** as the authoritative physical and gameplay body.
- Preserve existing steering, run progress, run surface detection, collision, camera source, run-end, slingshot launch, and animation mode classification behavior.
- Add a **Character Visual Follower** as a Character Presentation controller, not a movement controller.
- The **Character Visual Follower** should be an internal plain C# controller owned by VContainer lifecycle, using presentation late update.
- The passive Character Presentation view should expose an internal visual-follow view/tuning seam for tests and composition.
- Do not introduce a public gameplay-facing follower API.
- The visual-follow interfaces may live beside the single production implementation unless a second production implementation appears later.
- The follower should sample the **Launch Target** Transform render pose, not raw Rigidbody position.
- The target pose should be read in the presentation late update phase so Unity Rigidbody interpolation has already contributed to the visible Transform pose.
- The follower should write only the **Character Visual Anchor** world pose.
- **Smoothed Character Pose** must not be passed into physics, steering, progress, camera, collision, support probing, rewards, or run-end logic.
- The **Character Visual Anchor** should be treated as presentation-owned scene hierarchy. It should not need to be a child of the **Launch Target** hierarchy for correctness.
- If implementation keeps the anchor parented for a transitional reason, the follower must still own world pose without double-applying parent motion, and tests should make that risk explicit.
- The preferred implementation is to avoid relying on child-of-Launch-Target hierarchy because direct inheritance can reintroduce the visual jitter the follower is meant to smooth.
- Keep the **Character Model Root** role separate from the **Character Visual Anchor** role.
- Keep `CharacterPresenter` responsible for presentation mode classification and Animator frame creation.
- Keep `CharacterPresentationView` passive: it applies Animator parameters, enforces root motion disabled, exposes serialized tuning, and applies visual anchor pose through a narrow view method.
- Do not create a separate ScriptableObject config for the first implementation.
- **Character Visual Follow Tuning** should be serialized on the Character Presentation view or its scene/prefab authoring surface.
- Initial tuning should start with position response around `60`, heading response around `45`, and up/tilt response around `18`.
- Initial maximum position lag should start around `0.06m`, within the agreed `0.05m` to `0.08m` range.
- Initial snap distance should be `0.75m`.
- Initial snap angle should be `45` degrees.
- Tuning should clamp invalid authored values to safe non-negative or minimum-positive values at the property boundary.
- Smoothing should keep position tight rather than cinematic. The camera is already stable; this is avatar jitter cleanup, not a new camera-style follow rig.
- Smoothing should keep heading fairly tight so the **Character** remains visually responsive to movement direction.
- Smoothing should make up/tilt slower than heading to absorb small bank-normal and target-rotation noise.
- The pose smoother should compose an orthonormal rotation from smoothed up and smoothed heading, with safe fallbacks for degenerate vectors.
- If target heading is invalid, the smoother should preserve last valid heading when possible.
- If target up is invalid, the smoother should preserve last valid up or fall back to world up.
- The smoothing math should be isolated as a deep module with a small input/output shape so most behavior can be tested without PlayMode.
- The follower should snap on initialization.
- The follower should snap when entering **Run Preparation**.
- The follower should snap when entering **Pre-Launch**.
- The follower should snap when launch is applied.
- The follower should snap or force-align when entering **Run Ended**.
- The follower should snap when target distance exceeds the snap distance.
- The follower should snap when target rotation divergence exceeds the snap angle.
- Lifecycle snaps should clear any accumulated smoother state that would make the next frame lag from stale state.
- The follower should tolerate repeated lifecycle events idempotently.
- The follower should dispose event subscriptions cleanly.
- The follower should be registered as an entry point after the scene view and target references are registered.
- The follower should not depend on **Run Surface Context** or **Run Steering Frame**.
- The follower should not change Rigidbody interpolation or collision detection settings.
- Existing Character Presentation context documentation is the documentation source for this local decision; no ADR is required for the first implementation.
- Scene composition tests that currently require **Character Visual Anchor** to be child-of-Launch-Target should be updated to the new presentation-space boundary.

## Testing Decisions

Good tests should assert externally observable behavior at the follower boundary: given a target render pose, tuning, lifecycle event, and prior visual pose, the **Character Visual Anchor** receives the expected bounded **Smoothed Character Pose**. Tests should not assert private smoothing fields or implementation-only state names.

EditMode tests should cover the deep pose-smoothing module:

- First sample snaps to the target pose.
- Position smoothing moves toward target pose with the configured response.
- Position smoothing cannot exceed the configured maximum lag.
- Heading smoothing follows heading changes faster than up/tilt smoothing.
- Up/tilt smoothing absorbs small alternating up-vector changes.
- Snap distance forces an immediate pose match.
- Snap angle forces an immediate pose match.
- Invalid target position does not create an invalid anchor position.
- Invalid target heading preserves the last valid heading or safe fallback.
- Invalid target up preserves the last valid up or safe fallback.
- Output rotation remains finite and orthonormal enough for Unity Transform assignment.
- Reinitialization clears previous pose memory.

EditMode tests should cover the **Character Visual Follower** controller through fakes or lightweight Unity objects:

- Initialize snaps the visual anchor to the target render pose.
- Late update samples the current target Transform pose and applies smoothing.
- Entering **Run Preparation** snaps.
- Entering **Pre-Launch** snaps.
- Launch-applied snaps using the current target render pose.
- Entering **Run Ended** snaps or force-aligns predictably.
- Repeated lifecycle events are idempotent.
- Disposing unsubscribes from gameplay state and launch events.
- The follower does not call movement, physics, steering, run-progress, or surface APIs.
- The follower uses delta time from the project time abstraction.

EditMode tests should use existing project patterns:

- Plain C# controller tests similar to current camera and presenter controller tests.
- NUnit constraint assertions.
- No reliance on runtime MonoBehaviour callbacks for runtime scripts.
- Internal test hooks only through existing `UNITY_INCLUDE_TESTS` and InternalsVisibleTo patterns if needed.

PlayMode tests should cover scene composition and authoring:

- Gameplay Scene resolves the visual follow view and visual follow tuning from the Character Presentation authoring surface.
- VContainer resolves one **Character Visual Follower** entry point with the expected dependencies.
- The **Character Visual Anchor** exists and the **Character** remains under it.
- The **Character** has no Rigidbody, Collider, Joint, or CharacterController.
- Animator root motion remains disabled.
- Serialized **Character Visual Follow Tuning** defaults match the agreed starting values.
- The scene does not require **Character Visual Anchor** to be a child of the **Launch Target**.
- The current scene composition assertions that assume child-of-Launch-Target hierarchy are updated intentionally.

No first-pass automated camera-capture jitter oracle is required. The visual bug is real, but screenshot/video jitter assertions are likely to be fragile. A later PlayMode visual fixture can be added only if deterministic tests and manual review fail to protect the behavior.

Manual Unity smoke should cover:

- Launch into a side bank and ride high along the U-shaped **Run Surface**.
- Confirm the camera remains stable.
- Confirm the visible **Character** no longer has noticeable side-bank flicker.
- Confirm the **Character** does not visibly lag behind the **Launch Target** during normal sliding.
- Confirm run preparation, pre-launch aiming, accepted launch, and run-ended states align immediately.
- Confirm steering feel, launch distance, run end, pickups, and collision behavior are unchanged.

Verification order during implementation should follow the project rule:

- Run Unity connector compile first.
- Fix compile errors before tests.
- Run targeted EditMode visual smoother and follower tests.
- Run targeted PlayMode scene composition tests.
- Run broader tests only if touched surfaces or failures justify them.

## Release and Compatibility

Unity version assumptions:

- The work targets the current Unity 6 project setup used by this repository.
- No Unity upgrade, package upgrade, or ProjectSettings change is required.

Package version/changelog impact:

- This is local game feature work, not a package release.
- No package version bump is required.
- If the project maintains a gameplay changelog later, note this as presentation-only character visual smoothing for side-bank traversal.

Migration or install/sync behavior:

- No save-data migration is required.
- No package install or sync behavior is required.
- No `FormerlySerializedAs` migration is required for the first implementation if fields are added cleanly.
- Existing scene or prefab authoring must be updated in the same slice to wire the visual anchor and tuning.

Backward compatibility risks:

- If position smoothing is too slow, the **Character** may visibly lag behind the **Launch Target**.
- If max position lag is too large, the **Character** can look detached from the physical body.
- If up/tilt response is too fast, side-bank jitter may remain visible.
- If up/tilt response is too slow, banking can look floaty or delayed.
- If snap thresholds are too permissive, teleports or state resets may smear visually.
- If snap thresholds are too aggressive, smoothing may be bypassed too often.
- If the **Character Visual Anchor** remains parented under the **Launch Target** without careful world-pose handling, inherited parent motion can reintroduce jitter or cause double application.
- If gameplay code starts consuming **Smoothed Character Pose**, presentation smoothing can corrupt physics, camera, progress, or collision assumptions.
- Scene composition tests must prevent accidental Rigidbody, Collider, Joint, or CharacterController ownership under the visible **Character**.

## Out of Scope

- Replacing Rigidbody movement with `CharacterController`.
- Changing Rigidbody interpolation, collision detection, mass, drag, or damping.
- Changing physics materials, slingshot launch impulse, or lost momentum tuning.
- Changing **Run Surface Context** probing, support filtering, or support hysteresis.
- Changing **Run Steering Frame** stability or steering behavior.
- Changing camera follow behavior.
- Changing run progress, run end, rewards, pickups, or collision outcome logic.
- Changing **Character Presentation Mode** classification, Slide-only behavior, or Animator mode semantics.
- Adding a new **Coast** animation, lean system, procedural animation system, or IK pass.
- Adding hidden containment, invisible walls, or forced recentering on side banks.
- Creating a new public gameplay API for smoothed visual pose.
- Creating a full ADR for this local presentation decision.
- Adding fragile automated video or camera-capture jitter assertions in the first implementation.

## Further Notes

This PRD sits after the **Run Steering Frame Stability** work. Steering-frame stability addresses control-frame noise. **Character Visual Follower** addresses visible avatar pose noise. They should remain separate because they solve different problems and feed different consumers.

The user-facing success criterion is simple: on the same side-bank route where the camera currently looks stable but the **Character** flickers, the **Character** should now look stable without making the body feel delayed or changing gameplay outcomes.

The first implementation should bias toward a conservative smoothing pass with manual review. The agreed initial tuning is intentionally close to the target pose for position and heading, with extra damping on up/tilt. If manual review shows lag or remaining jitter, tune those values before broadening the architecture.
