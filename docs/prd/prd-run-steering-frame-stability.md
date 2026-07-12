# PRD: Run Steering Frame Stability

> **Superseded in part.** The [Run Surface Probing and Stability PRD](prd-run-surface-probing-and-stability.md) replaces this document's raw-source and steering-local temporal-filter architecture. This document remains historical rationale for steering-frame feel, slew, airborne memory, and human review.

## Problem Statement

The Ladybug Rooftop Half-Tube uses a **U-shaped Run Surface** with banked **Side Banks** for **Soft Containment**. When the **Launch Target** rides high on those sides near the **Course Lip**, the visible slope can look smooth while raw support contact normals still change abruptly from frame to frame.

Today the **Run Steering Frame** follows the current grounded **Run Surface** normal directly. That means one-frame edge/contact noise can rotate the steering orientation immediately. The player experiences this as side-edge flicker, unstable steering response, and jittery movement even though the authored course looks readable and recoverable.

The player-facing problem is not that the Rigidbody is physically invalid. The problem is that a noisy support sample is allowed to become a control frame instantly. On a half-tube side, this makes a recoverable **Side Bank** feel like a glitchy boundary instead of a smooth part of the **Run Course**.

## Solution

Keep the existing movement architecture: the Rigidbody-backed **Launch Target** remains the physical truth, **PhysicsRunSurfaceContextSource** remains the raw **Run Surface** support truth, and **Run Steering Control** remains responsible only for steering during **Running**.

Add stability inside the **Run Steering Frame** layer. The stable **Run Steering Frame** should be derived from raw **Run Surface** support, but it must not be identical to a single raw contact normal every tick. The frame should:

- slew gradually across ordinary grounded normal changes;
- snap or reinitialize for confirmed real discontinuities;
- keep the last stable steering frame briefly during very short support misses;
- reject implausible one-frame steering-normal spikes unless they persist long enough to be treated as real support.

From the player's perspective, riding a **Side Bank** should remain a recoverable part of the downhill slide. Steering should continue to feel consistent when the visible surface is smooth. If the **Launch Target** crests the **Course Lip**, it can still leave **Soft Containment**; this PRD does not add hidden walls, forced recentering, or escape prevention.

## Unity Surfaces

Runtime assemblies and asmdefs:

- Gameplay runtime assembly that owns **Run Steering Control**, **Run Steering Frame**, **Run Surface** context consumption, and VContainer composition.
- Existing Foundation time abstraction through `ITime` for fixed-step timing.
- Existing VContainer entry point dispatch for fixed-tick order.
- No new runtime assembly definition is expected for the first implementation.

Editor assemblies, windows, inspectors, importers, or menu items:

- No new Editor assembly, custom inspector, importer, or menu item is required.
- Existing Unity serialization and inspector display for the steering config should be enough.

Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:

- Existing player steering configuration asset gains a **Run Steering Frame Stability** serialized section.
- Gameplay composition registers one shared **Run Steering Frame** implementation as a fixed-tick entry point and as the read/reset interfaces needed by consumers.
- Scene geometry, half-tube mesh shape, physics layers, and authored **Run Surface** colliders are not changed by this PRD.
- No package manifest, ProjectSettings, Unity version, or Addressables schema change is required.

RPC/helper commands, hooks, or shell wrappers:

- Use the existing Unity AI Agent connector workflow for compile and targeted tests during implementation.
- No new shell wrapper or RPC command is needed.

Package versioning, changelog, and installation/sync behavior:

- This is a project gameplay change, not a package release workflow change.
- No install, sync, or migration behavior is required.

## User Stories

1. As a player, I want steering on the half-tube side to feel stable so that a smooth-looking **Side Bank** does not feel broken.
2. As a player, I want riding a **Side Bank** to remain recoverable so that high side traversal feels like part of the game, not a failure state.
3. As a player, I want the character to keep sliding predictably near the **Course Lip** so that edge riding feels skill-based.
4. As a player, I want steering not to twitch when the surface visually looks continuous so that control feels logical.
5. As a player, I want a brief ground-probe miss to avoid jolting steering so that small contact gaps do not create visible movement noise.
6. As a player, I want real airborne or escape moments to still behave like real airborne or escape moments so that stability does not fake containment.
7. As a player, I want a confirmed sharp surface change to update steering quickly so that real geometry transitions still feel responsive.
8. As a player, I want a one-frame contact spike not to redirect steering so that noisy edge samples do not punish me.
9. As a player, I want downhill sliding to remain momentum-driven so that the physics fantasy stays intact.
10. As a player, I want the Launch Target to keep obeying Rigidbody physics so that movement still feels physical.
11. As a player, I want no invisible wall near the half-tube edge so that cresting the **Course Lip** still has consequences.
12. As a player, I want steering corrections on the side bank to feel smooth enough to read so that I can recover intentionally.
13. As a player, I want the character not to jitter while the camera follows it so that the run remains comfortable to watch.
14. As a player, I want side riding to feel consistent across repeated runs so that I can learn the course.
15. As a player, I want small support noise not to change the outcome of a run so that failures feel fair.
16. As a player, I want large real surface changes to remain visible in movement so that the course still has physical character.
17. As a designer, I want **Soft Containment** to stay authored by **Run Surface** shape so that the half-tube design remains honest.
18. As a designer, I want **Side Banks** to remain traversable **Run Surface** so that banked recovery is possible.
19. As a designer, I want the **Course Lip** to remain an escape edge so that high-risk side riding still has risk.
20. As a designer, I want steering-frame stability to be tunable without changing terrain geometry so that feel can be refined safely.
21. As a designer, I want normal smoothing speed to be expressed in degrees per second so that tuning maps to visible angular change.
22. As a designer, I want real discontinuity snap angle to be tunable so that sharp geometry can remain responsive.
23. As a designer, I want short ungrounded grace to be tunable so that steering survives contact noise but not real airtime.
24. As a designer, I want suspect-normal confirmation time to be tunable so that edge noise can be filtered without making steering sluggish.
25. As a designer, I want default values that bias toward stability on smooth-looking banks so that side-edge jitter is reduced in the first pass.
26. As a designer, I want stability tuning in the existing steering config so that all steering feel lives in one familiar asset.
27. As a designer, I want physics support detection to stay raw so that movement, progress, animation, and run-end systems are not softened accidentally.
28. As a designer, I want the steering frame to be stable only for control so that gameplay facts remain debuggable.
29. As a designer, I want no new visible steering UI so that the player's input model does not change.
30. As a designer, I want no camera, animation, or run-end behavior change in this slice so that the jitter fix is isolated.
31. As a gameplay engineer, I want **RunSurfaceSteeringFrameSource** to become the deep module for steering-frame stability so that complex filtering has a small interface.
32. As a gameplay engineer, I want `IRunSteeringFrameSource` to remain read-only so that consumers cannot mutate steering-frame state accidentally.
33. As a gameplay engineer, I want a separate reset interface so that lifecycle control does not pollute the read interface.
34. As a gameplay engineer, I want reset to use the launch up direction so that a new run starts from a known orientation.
35. As a gameplay engineer, I want the steering-frame source to stay inactive before the first reset so that preparation-time support samples cannot leak into running.
36. As a gameplay engineer, I want fixed-tick sampling to happen once before steering consumes the frame so that reads are deterministic.
37. As a gameplay engineer, I want controller code to consume the current frame without driving sampling so that ownership stays clear.
38. As a gameplay engineer, I want time-based slew limiting through `ITime.FixedDeltaTime` so that behavior is deterministic and testable.
39. As a gameplay engineer, I want normal validation to preserve existing invalid-normal fallback rules so that bad vectors never become steering up.
40. As a gameplay engineer, I want raw **RunSurfaceContext** behavior unchanged so that existing ground, progress, presentation, and run-end tests remain meaningful.
41. As a gameplay engineer, I want brief ungrounded steering grace to be steering-only so that true ungrounded state is still available to other systems.
42. As a gameplay engineer, I want snap/reinitialize after real airborne gaps so that recontact does not slew across unrelated orientations.
43. As a gameplay engineer, I want large suspect normals to be confirmed before acceptance so that one-frame edge spikes are filtered.
44. As a gameplay engineer, I want confirmed large normals to snap rather than slowly rotate across a huge angle so that real transitions do not feel delayed.
45. As a gameplay engineer, I want one shared singleton registration so that read, reset, and fixed-tick behavior operate on the same state.
46. As a gameplay engineer, I want VContainer entry-point order protected by tests so that sampling remains before steering application.
47. As a gameplay engineer, I want no dependency from the steering-frame source to gameplay state service so that lifecycle stays explicit and narrow.
48. As a gameplay engineer, I want `PlayerSteeringController` to reset the frame when steering actually activates so that launch events before Running do not start sampling too early.
49. As a gameplay engineer, I want the steering-frame source to keep running after steering deactivates until the next reset so that no extra deactivate API is needed.
50. As a gameplay engineer, I want the next reset to clear stale state so that runs cannot inherit suspect or grace timers.
51. As a gameplay engineer, I want configuration properties exposed through the existing steering config interface so that runtime code does not depend on concrete assets.
52. As a gameplay engineer, I want no `CharacterController` migration so that the Rigidbody-backed architecture remains intact.
53. As a gameplay engineer, I want no hidden level colliders for this problem so that movement truth stays visible in authored **Run Surface**.
54. As a gameplay engineer, I want focused EditMode tests for the state machine so that the fix can be verified without flaky scene setup.
55. As a gameplay engineer, I want composition tests for identity and order so that dependency wiring cannot silently regress.
56. As a QA engineer, I want a test proving pre-reset reads return caller fallback so that preparation state is isolated.
57. As a QA engineer, I want a test proving reset primes the frame from launch up so that a new run starts predictably.
58. As a QA engineer, I want a test proving ordinary normal changes slew over time so that smooth banks do not jitter.
59. As a QA engineer, I want a test proving slew amount depends on fixed delta time so that timing behavior is deterministic.
60. As a QA engineer, I want a test proving invalid grounded normals fall back safely so that bad data cannot corrupt steering.
61. As a QA engineer, I want a test proving one missed support sample uses last stable steering up so that brief ground noise is masked.
62. As a QA engineer, I want a test proving grace expires after the configured duration so that real ungrounded state is not hidden forever.
63. As a QA engineer, I want a test proving one-frame large normal spikes are ignored so that edge noise is covered.
64. As a QA engineer, I want a test proving a large normal that persists through confirmation is accepted so that real discontinuities still work.
65. As a QA engineer, I want a test proving confirmed large discontinuities snap rather than slew so that sharp transitions stay responsive.
66. As a QA engineer, I want a test proving `PlayerSteeringController` calls reset on steering activation so that lifecycle is wired.
67. As a QA engineer, I want a test proving launch events alone do not reset active steering prematurely so that state ordering is protected.
68. As a QA engineer, I want a test proving composition resolves one shared steering-frame instance for all exposed interfaces so that state is not split.
69. As a QA engineer, I want a test proving fixed-tick order samples the steering frame before steering applies movement so that runtime behavior is deterministic.
70. As a maintainer, I want the domain terms in code, tests, and docs to stay aligned so that future work does not confuse **Run Steering Frame** with raw ground normals.
71. As a maintainer, I want the fix to be local to gameplay steering so that unrelated systems do not inherit smoothing they did not ask for.
72. As a maintainer, I want first-pass defaults to be documented so that future tuning changes are intentional.
73. As a maintainer, I want a later PlayMode half-tube fixture to remain possible so that scene-level regression coverage can be added only if needed.
74. As a maintainer, I want no serialized migration burden beyond adding new config fields so that the implementation stays clean.
75. As a maintainer, I want stable steering-frame behavior to be inspectable in tests so that bug reports can be reduced to deterministic cases.

## Implementation Decisions

- Preserve the Rigidbody-backed **Launch Target** as the authoritative movement body.
- Preserve **PhysicsRunSurfaceContextSource** as the raw **Run Surface** support source.
- Do not smooth or debounce **RunSurfaceContext** itself for this problem.
- Keep **Run Steering Frame** as the gameplay orientation used by **Run Steering Control** during **Running**.
- Treat **Run Steering Frame** as distinct from **Run Progress Frame** and distinct from raw contact normals.
- Implement steering-frame stability inside **RunSurfaceSteeringFrameSource**.
- Keep `IRunSteeringFrameSource` read-only with the existing frame-read responsibility.
- Add a separate lifecycle/reset seam for steering-frame reset.
- Keep reset minimal: accept a launch up direction, normalize or fallback invalid input, prime stable up from that direction, clear stable timers, clear suspect state, and mark the source active.
- Before the first reset, the steering-frame source should avoid sampling support and should return the caller fallback.
- The steering-frame source should be a fixed-tick entry point that samples raw support once per fixed tick.
- Register the steering-frame source before **PlayerSteeringController** in fixed-tick order.
- Register one shared singleton implementation for the read interface, reset interface, and fixed-tick entry point.
- Do not make **PlayerSteeringController** explicitly drive steering-frame sampling.
- **PlayerSteeringController** should reset the steering frame from steering activation, using the stored launch up direction.
- A launch-applied event by itself should store launch orientation but not start steering-frame sampling before **Running** activation.
- The steering-frame source should remain gameplay-state agnostic and should not depend on a gameplay state service.
- The steering-frame source does not need a separate deactivate or clear API for the first implementation.
- The next reset should clear state from the previous run.
- Ordinary grounded normal changes should be angular-slew-limited.
- Normal slew should be time-based, expressed in degrees per second, and use `ITime.FixedDeltaTime`.
- Large confirmed discontinuities should snap or reinitialize instead of slowly slewing across the full angle.
- A large normal jump should be considered suspicious when the source has been continuously grounded and the jump exceeds the configured snap angle.
- Suspicious large normals should require a short confirmation window before they are accepted.
- A suspicious normal that disappears before confirmation should be treated as contact noise and should not rotate the steering frame.
- A suspicious normal that persists through confirmation should be accepted as real and should snap/reinitialize the steering frame.
- Brief ungrounded samples should preserve the last stable steering frame for steering only.
- Ungrounded steering grace should expire after a configured duration.
- After a real airborne gap, the next valid grounded support should reinitialize or snap rather than slew from stale support.
- Invalid normals should never become steering up.
- If no active stable frame exists, reads should fall back to the caller-provided fallback direction.
- Stability tuning belongs in the existing **PlayerSteeringConfig** asset, not a new config asset.
- Add a **Run Steering Frame Stability** serialized section to the existing steering config.
- Initial normal-slew default should be `180` degrees per second.
- Initial snap-angle default should be `60` degrees.
- Initial ungrounded-grace default should be `0.08` seconds.
- Initial suspect-normal-confirmation default should be `0.04` seconds.
- Expose read-only steering-frame stability values through the existing steering config interface.
- Do not add public gameplay API beyond the narrow reset seam required for composition and tests.
- Do not change **Run Surface** authoring, `RunContact` category rules, or support-collider filtering in this PRD.
- Do not change **Run Progress**, **Lost Momentum**, **Run End Flow**, **Character Presentation**, camera, slingshot launch, or physics material tuning in this PRD.
- Use existing domain vocabulary in tests and documentation: **Run Steering Frame**, **Run Surface**, **Side Bank**, **Course Lip**, **Soft Containment**, and **Launch Target**.
- Defer scene-level half-tube jitter fixture work unless deterministic unit and composition tests cannot cover the regression.

## Testing Decisions

Good tests should assert externally observable behavior at module boundaries rather than private fields. The steering-frame source can have rich internal state, but tests should express what a controller would read, when reset is called, and how the frame evolves across fixed ticks.

EditMode tests should cover the steering-frame source as a deep module:

- Before reset, `GetUpDirection` returns caller fallback and support samples are ignored.
- Reset with a valid launch up direction primes the frame from launch up.
- Reset with invalid input falls back safely.
- Reset clears previous stable, suspect, and grace state.
- A grounded valid support normal becomes the steering frame after activation.
- Small grounded normal changes slew over multiple fixed ticks.
- Slew amount respects configured degrees per second and `ITime.FixedDeltaTime`.
- Invalid grounded normals do not become steering up.
- Brief ungrounded samples keep the last stable frame during grace.
- Grace expires after the configured duration.
- Recontact after a real airborne gap reinitializes or snaps rather than slewing from stale support.
- One-frame large normal spikes are ignored while the last stable frame is used.
- Large normal changes that persist through confirmation are accepted.
- Confirmed large changes snap or reinitialize instead of slowly slewing.
- Pure reads are idempotent inside a fixed tick; fixed-tick sampling owns state advancement.

EditMode tests should cover steering-controller lifecycle integration:

- The controller resets the steering frame when steering activates.
- The reset uses the launch up direction stored from the accepted launch.
- A launch event before **Running** does not reset steering-frame state until steering actually activates.
- Running activation before launch does not start steering without launch data.
- Existing velocity-preserving steering behavior still uses the current **Run Steering Frame** up direction.

Composition tests should cover VContainer wiring:

- The **RunSurfaceSteeringFrameSource** implementation is registered as one shared singleton across read, reset, and fixed-tick roles.
- The fixed-tick entry point for steering-frame sampling is ordered before **PlayerSteeringController**.
- The expected fixed-tickable count and resolved gameplay services are updated intentionally.
- The existing steering config asset exposes the new stability defaults.

PlayMode tests are not required for the first implementation unless EditMode coverage cannot express the bug. A later PlayMode fixture may load the half-tube scene and exercise high **Side Bank** traversal if the deterministic unit tests are insufficient or if the bug persists in playtest.

Verification order during implementation should follow the project rule:

- Run Unity connector compile first.
- Fix compile errors before tests.
- Run targeted EditMode steering-frame source tests.
- Run targeted EditMode steering-controller lifecycle tests.
- Run targeted composition tests.
- Run broader tests only if touched surfaces or failures justify them.

Manual Unity smoke should focus on the Ladybug Rooftop Half-Tube:

- Launch into the banked side and ride high without cresting the **Course Lip**.
- Confirm visible steering and movement no longer flicker on smooth-looking side surfaces.
- Confirm cresting the **Course Lip** can still escape **Soft Containment**.
- Confirm ordinary center-line downhill sliding feels unchanged.

## Release and Compatibility

Unity version assumptions:

- The work targets the current Unity 6 project setup used by this repository.
- No Unity upgrade, package upgrade, or ProjectSettings change is required.

Package version/changelog impact:

- This is local game feature work, not a package release.
- If the project maintains a gameplay changelog later, this should be described as stabilizing **Run Steering Frame** behavior on banked **Run Surface** traversal.

Migration or install/sync behavior:

- No save-data migration is required.
- No `FormerlySerializedAs` migration is required because this PRD adds new steering-frame stability fields rather than renaming existing persisted fields.
- Existing project-owned steering config assets should be intentionally authored with the new defaults.

Backward compatibility risks:

- Steering feel can become too damped if the normal slew rate or suspect confirmation is too conservative.
- Steering can still feel jittery if snap angle or confirmation duration is too permissive.
- Very sharp authored transitions may feel delayed if they are incorrectly classified as suspect noise.
- Incorrect VContainer registration could create separate state instances for read/reset/tick roles; composition tests must prevent that.
- This PRD intentionally does not change raw support detection, so any truly bad Run Surface authoring remains a content issue.

## Out of Scope

- Replacing Rigidbody movement with `CharacterController`.
- Adding hidden walls, guard rails, forced recentering, or invisible side containment.
- Preventing the **Launch Target** from cresting the **Course Lip**.
- Changing half-tube mesh geometry, collider shape, or level art.
- Changing **PhysicsRunSurfaceContextSource** raw grounding semantics.
- Smoothing **RunSurfaceContext** for progress, presentation, run-end, camera, or physics systems.
- Changing **Run Progress Frame** behavior.
- Changing **Run End Flow**, **Lost Momentum**, or reward behavior.
- Changing **Character Presentation** run/slide classification.
- Changing slingshot launch impulse, physics material friction, or damping.
- Adding new packages or changing Unity version.
- Adding a full PlayMode half-tube jitter harness in the first implementation slice.
- Adding telemetry, debug overlay, or designer tooling beyond tests and config fields.

## Further Notes

Assumptions:

- The visible jitter is primarily caused by noisy support normals feeding the steering frame, not by camera follow, animation, or Rigidbody interpolation.
- Existing **Run Surface** colliders on the half-tube remain correctly authored as `RunContact.Surface`.
- The chosen defaults are first-pass values and should be tuned by playtest after deterministic regressions are protected.
- Current VContainer fixed-tick ordering remains stable enough to protect with composition tests.

Unresolved questions:

- No blocking product questions remain for implementation.
- If playtest still shows side-edge jitter after this change, the next investigation should separate steering-frame noise from collider geometry, camera smoothing, visual character alignment, and Rigidbody contact resolution.
