# PRD: Run Air Steering Control

## Problem Statement

The game is about firing the **Run Body** from the **Slingshot**, then sliding down the course. Once the **Run Body** is unsupported by a valid **Run Surface**, the player still expects limited directional agency because they are still in the same run and still using the same steering gesture.

The current steering model was hardened to protect launch energy from hidden speed caps, but it also leaves a control gap: fired motion and later unsupported motion can be treated as a blocked steering interval until support returns. From the player's perspective, this makes the character feel unresponsive in the air. If steering suddenly resumes only at landing, control can feel discontinuous.

The opposite naive fix is also wrong. Allowing normal grounded steering immediately after launch would make airborne movement too strong, could fight stale grounded support samples from ground-detection hysteresis, and risks turning steering back into a hidden speed or correction system. Airborne agency needs a weaker, direction-only steering model that preserves the speed the **Run Body** already has.

The root problem is ownership. **Run Air Steering Control** should be a **Running** control behavior selected from support and motion facts. It should not be selected from **Launch Flight**, **Airborne**, or any other **Character Presentation Mode**. It should not add speed, remove speed, drive toward a target velocity, or depend on Slingshot-only state after launch.

## Solution

Add **Run Air Steering Control** as a weaker in-air variant of **Run Steering Control**.

During **Running**, a **Run Steering Mode Selector** chooses between grounded **Run Steering Control** and **Run Air Steering Control** from current support and motion facts:

- Choose grounded steering when the current support is a valid **Run Surface** and the **Run Body** is not moving away from that support above the accepted lift tolerance.
- Choose air steering when current support is missing, invalid, not a valid **Run Surface**, or stale-grounded while the **Run Body** is moving away from the reported surface above the accepted lift tolerance.
- Do not expose a blocked or inactive selection. Low speed, missing input, invalid velocity, neutral gameplay state, and run-end state remain separate controller gates.
- Do not use a no-takeoff timeout to switch to grounded steering.

When air steering is selected and the player has an active **Run Steering Control** gesture, rotate the current air-frame planar velocity around the current **Run Steering Frame** up direction. Preserve planar speed and preserve vertical velocity. Rotate the **Run Body** to face the steered air-frame planar velocity when steering is actually applied.

When air steering is selected and the player has no active steering gesture, do not apply hidden guidance. The **Run Body** should keep its current velocity except for separate velocity-only systems such as **Launch Landing Stabilization** or the defensive **Run Body Speed Sanity Guard**.

The result should feel simple to the player: the **Slingshot** creates launch energy, the grounded **Run Body Speed Model** shapes intentional surface-tangent speed from course facts and tuning, and steering can gently bend direction both on the ground and in the air without creating an invisible wall.

## Unity Surfaces

Runtime assemblies and asmdefs:

- Gameplay runtime assembly containing **Run Steering Control**, **Run Air Steering Control**, **Run Steering Mode Selector**, **Run Body** movement orchestration, run-surface support facts, slingshot launch notifications, and VContainer composition.
- Gameplay EditMode test assembly for selector, steering controller, and config tests.
- Gameplay PlayMode test assembly for scene composition assertions if serialized steering config changes.
- No new runtime asmdef is expected for the first implementation.
- No Editor-only asmdef is expected.

Runtime modules and services:

- **Player Steering Controller** remains the orchestration point for fixed-tick steering, velocity sanitation, landing stabilization, and steering target writes.
- **Run Steering Mode Selector** should replace the current post-launch steering blocker concept with a small deep module that returns grounded or air steering.
- **Run Steering Mode Selector** should not rotate velocity, write **Run Body** pose, change speed, or know about animation modes.
- **Run Air Steering Control** should reuse the existing gesture input mapping, gesture range, deadzone, DPI handling, and steering smoothing.
- **Run Air Steering Control** should use separately tuned, weaker maximum turn authority than grounded **Run Steering Control**.
- **Run Steering Frame Source** remains the owner of the steering up direction and launch/world fallback behavior.
- **Run Surface Context Source** remains the physical support truth provider.
- **Launch Landing Stabilization** remains a velocity-only correction that may remove post-launch positive lift after actual landing while preserving tangent speed.
- **Run Body Speed Sanity Guard** remains defensive validation only and must not become a player-facing speed boundary.

Scenes, prefabs, ScriptableObjects, and assets:

- Player steering configuration gains an authored air-steering turn authority field.
- Existing run steering fields remain the owner of grounded steering gesture tuning and smoothing.
- Existing slingshot launch configuration remains the owner of **Launch Impulse**.
- Existing run-surface material or contact authoring remains the owner of ordinary slowdown.
- Scene composition assertions should protect the new serialized steering config value if the value is authored in the scene or config asset.
- No character animation controller change is required.
- No camera asset change is required.

Editor, inspectors, importers, and menus:

- No custom inspector, import pipeline, menu item, or editor window is required.
- Existing serialized asset authoring is enough.

Project settings, package manifests, and helper commands:

- No Unity version change.
- No package manifest change.
- No ProjectSettings change.
- Existing Unity connector compile and test workflow remains the verification path for implementation.

## User Stories

1. As a player, I want to steer a little after the **Slingshot** fires me, so that I still feel in control during the shot.
2. As a player, I want airborne steering to feel weaker than grounded steering, so that air control does not feel like driving on invisible ground.
3. As a player, I want airborne steering to bend my direction without slowing me down, so that the launch still feels powerful.
4. As a player, I want airborne steering to avoid adding speed, so that the game does not create a hidden boost.
5. As a player, I want maximum pull to still carry farther than a small pull, so that **Band** strength remains meaningful.
6. As a player, I want small pull to remain a short push, so that weak shots are not secretly upgraded.
7. As a player, I want to correct my direction after a bump lifts me off the surface, so that small course irregularities do not remove agency.
8. As a player, I want to correct my direction while falling before the run ends, so that unsupported **Running** still feels interactive.
9. As a player, I want the character not to turn by itself when I am not steering, so that air movement feels honest.
10. As a player, I want landing to return naturally to grounded steering, so that the control model stays understandable.
11. As a player, I want grounded steering to keep its stronger feel, so that sliding on the course remains responsive.
12. As a player, I want the transition from launch flight to later unsupported motion not to change movement rules suddenly, so that control stays consistent.
13. As a player, I want stale ground contact right after launch not to kill the fired feeling, so that takeoff still feels like a shot.
14. As a player, I want weak grounded launches to become steerable when they are truly sliding on the surface, so that poor launches are still readable.
15. As a player, I want speed loss to come from contact with the course, so that slowdown looks physical.
16. As a player, I want the avatar to face the direction I steer in the air, so that the visual direction matches the motion.
17. As a player, I want air steering to use the same touch gesture as grounded steering, so that I do not need to learn another control.
18. As a player, I want neutral or released touch to stop air steering, so that there is no hidden auto-correction.
19. As a player, I want the camera and animation state not to decide movement, so that visuals cannot unexpectedly change control.
20. As a player, I want the run to end normally when failure conditions happen, so that air control does not bypass run-end rules.
21. As a designer, I want **Run Air Steering Control** to be a **Running** feature, so that it applies to all unsupported run motion, not only launch.
22. As a designer, I want air steering to be selected from support facts, so that it matches what the player sees physically.
23. As a designer, I want air steering not to depend on **Launch Flight**, so that animation can be tuned independently.
24. As a designer, I want air steering not to depend on **Airborne**, so that fall presentation can remain a visual classification.
25. As a designer, I want a separate air turn authority value, so that air agency can be tuned without weakening grounded steering.
26. As a designer, I want shared steering responsiveness, so that input feel remains consistent across grounded and air steering.
27. As a designer, I want no player-facing speed cap in steering, so that launch strength and the explicit grounded speed model remain the visible speed knobs.
28. As a designer, I want no no-takeoff timeout, so that steering mode changes follow physical support rather than hidden time.
29. As a designer, I want stale grounded launch samples with positive lift to be treated as air steering, so that support hysteresis does not fight takeoff.
30. As a designer, I want valid grounded support with no lift to use grounded steering, so that weak grounded launches do not get stuck in air rules.
31. As a designer, I want landing stabilization to stay focused on lift removal only, so that it does not become artificial damping.
32. As a designer, I want speed balancing to remain available through **Launch Impulse** and **Run Body Speed Model** tuning, so that ownership remains visible and physical interactions remain intact.
33. As a designer, I want no new animation requirement, so that movement improvement is not blocked by art.
34. As a designer, I want no new camera requirement, so that camera work remains separate.
35. As a gameplay engineer, I want **Run Steering Mode Selector** to be a deep module, so that support-to-mode decisions are easy to test.
36. As a gameplay engineer, I want the selector to return only grounded or air, so that blocked and inactive states do not leak into selection semantics.
37. As a gameplay engineer, I want the steering controller to own velocity rotation and pose writes, so that the selector remains pure decision logic.
38. As a gameplay engineer, I want air steering to reuse the existing **Run Steering Frame**, so that banked surfaces and launch fallback stay consistent.
39. As a gameplay engineer, I want air steering tests to prove planar speed preservation, so that hidden braking does not return.
40. As a gameplay engineer, I want air steering tests to prove vertical velocity preservation, so that steering does not fake lift or drop.
41. As a gameplay engineer, I want no-input air mode to write no steering velocity, so that unsupported motion does not auto-guide.
42. As a gameplay engineer, I want grounded steering tests to stay green, so that air steering does not regress existing sliding control.
43. As a gameplay engineer, I want old post-launch blocking tests replaced with selector tests, so that tests describe the new model.
44. As a gameplay engineer, I want controller tests to cover launch, bump, and fall cases, so that air steering is not launch-only by accident.
45. As a gameplay engineer, I want leaving **Running** to clear steering mode state, so that state cannot leak into the next run.
46. As a gameplay engineer, I want neutral gameplay states to produce no air steering, so that prelaunch and preparation remain stable.
47. As a gameplay engineer, I want accepted run results to stop air steering immediately, so that terminal state remains deterministic.
48. As a gameplay engineer, I want active **Launch Landing Stabilization** to run before steering writes, so that lift correction remains velocity-only.
49. As a gameplay engineer, I want air steering to use current velocity rather than target speed, so that it cannot become a guidance system.
50. As a gameplay engineer, I want any sanity guard to remain unreachable by normal play, so that validation is not confused with tuning.
51. As a gameplay engineer, I want scene composition assertions for authored config values, so that tuning cannot regress silently.
52. As a gameplay engineer, I want no new package dependency, so that the change stays local to gameplay.
53. As a QA engineer, I want a test proving post-launch unsupported flight can be steered, so that the main player symptom is covered.
54. As a QA engineer, I want a test proving post-launch unsupported flight is not clamped, so that speed ownership is protected.
55. As a QA engineer, I want a test proving stale grounded support plus positive lift selects air steering, so that takeoff is protected.
56. As a QA engineer, I want a test proving valid support plus no lift selects grounded steering, so that weak launches are covered.
57. As a QA engineer, I want a test proving later unsupported bumps use air steering, so that the behavior is not launch-specific.
58. As a QA engineer, I want a test proving later normal falls can still use air steering until run end, so that unsupported **Running** stays consistent.
59. As a QA engineer, I want a test proving no touch means no air steering write, so that hidden guidance is covered.
60. As a QA engineer, I want a test proving left and right air input rotate in opposite directions, so that gesture sign remains correct.
61. As a QA engineer, I want a test proving air turn authority is weaker than grounded turn authority, so that design intent is protected.
62. As a QA engineer, I want a test proving grounded steering still uses grounded turn authority, so that existing control is preserved.
63. As a QA engineer, I want a test proving air steering uses **Run Steering Frame** up, so that banked and tilted courses remain stable.
64. As a QA engineer, I want a test proving air steering preserves vertical velocity, so that it does not become jump or downforce logic.
65. As a QA engineer, I want a test proving air steering preserves planar magnitude, so that it remains direction-only.
66. As a QA engineer, I want a test proving **RunEnded** stops steering, so that terminal flow is not changed.
67. As a QA engineer, I want a scene composition test for the authored air steering value, so that the scene matches the PRD contract.
68. As a maintainer, I want glossary terms to stay precise, so that "Launch Target" is not used for the player after launch.
69. As a maintainer, I want **Run Steering Mode Selector** terminology to replace post-launch blocking terminology, so that future code reviews discuss the correct boundary.
70. As a maintainer, I want no dependency from movement to **Character Presentation Mode**, so that presentation features cannot break steering.
71. As a maintainer, I want no dependency from movement to camera state, so that camera fixes remain isolated.
72. As a maintainer, I want no implementation hidden in Slingshot, so that Slingshot remains responsible for firing, not in-run control.
73. As a maintainer, I want a clear testing split between selector logic and controller velocity writes, so that regressions are cheap to diagnose.
74. As a maintainer, I want the PRD to record unresolved tuning values, so that implementation can proceed without pretending all numbers are final.

## Implementation Decisions

- Treat **Run Air Steering Control** as part of **Running**.
- Do not model **Run Air Steering Control** as a Slingshot feature.
- Do not model **Run Air Steering Control** as a **Character Presentation** feature.
- Keep **Slingshot** responsible for **Launch Impulse** only.
- Keep **Run Body Speed Model** responsible for intentional grounded tangent-speed gain, slowdown, recovery, and the soft envelope.
- Keep **Run Steering Control** responsible for direction changes only.
- Replace the current post-launch steering blocker concept with **Run Steering Mode Selector**.
- **Run Steering Mode Selector** should select one of two modes: grounded or air.
- **Run Steering Mode Selector** should not have blocked, inactive, launch, fall, or presentation modes.
- **Run Steering Mode Selector** should choose air when support is not a valid **Run Surface**.
- **Run Steering Mode Selector** should choose air when support is reported as valid but current velocity has positive surface-normal lift above an accepted tolerance.
- **Run Steering Mode Selector** should choose grounded when support is a valid **Run Surface** and current velocity has no positive surface-normal lift above the accepted tolerance.
- **Run Steering Mode Selector** should be condition-driven, not timeout-driven.
- Prefer a stateless selector if current support, velocity, and tolerance facts are enough. Keep state only if implementation proves a real hysteresis boundary is required, and do not use that state to disable air steering.
- The selector should accept support and motion facts and return a steering mode; it should not call Unity physics, query input, or write to a Rigidbody.
- The selector should not know about **LaunchFlight**, **LaunchPush**, **Airborne**, victory, defeat, animator states, or camera states.
- The selector should not decide whether a gesture is active; gesture activity remains a steering controller/input concern.
- Add a separate air turn authority field to player steering config.
- Air turn authority should be lower than grounded turn authority by default.
- Air steering should share existing steering gesture mapping, deadzone, range, DPI handling, and smoothing.
- Air steering should share **Run Steering Responsiveness** with grounded steering.
- Air steering should use the current **Run Steering Frame** up direction.
- Air steering should rotate current air-frame planar velocity around the selected up direction.
- Air steering should preserve planar speed.
- Air steering should preserve vertical velocity.
- Air steering should rotate **Run Body** facing to the steered air-frame planar velocity only when steering is actually applied.
- Air steering should not add pitch, roll, banking, or character-presentation rotation.
- Air steering should not add forward speed.
- Air steering should not reduce speed.
- Air steering should not apply player-facing planar speed caps.
- Air steering should not apply launch speed recovery.
- Air steering should not drive toward an authored target velocity.
- Air steering should do nothing without an active **Run Steering Control** gesture, except for separate velocity-only correction systems that already own their behavior.
- If the current air steering input is effectively zero, the implementation should avoid unnecessary velocity or facing writes unless preserving existing grounded behavior requires otherwise.
- **Minimum Steer Speed** remains a steering rotation gate, not a speed correction.
- **Launch Landing Stabilization** remains separate from **Run Steering Mode Selector**.
- **Launch Landing Stabilization** may run before steering selection or steering writes so that first-landing lift correction remains velocity-only.
- **Launch Landing Stabilization** must continue to preserve tangent speed.
- **Run Body Speed Sanity Guard** remains defensive validation only.
- Any sanity guard threshold must stay outside normal launch, upgrade, and course speeds.
- Keep current Rigidbody-backed movement architecture.
- Keep VContainer dependency style and plain C# controller style.
- Do not introduce a new package.
- Do not change Unity version.
- No ADR is required for the first implementation because the decision is internal, documented in glossary/context language, and covered by tests.
- Update local glossary/context docs only if implementation reveals a terminology gap.

## Testing Decisions

Good tests should assert observable movement contracts from support facts, velocity, gameplay state, and steering input. Tests should avoid private field coupling and should not use animation state as a movement oracle.

EditMode tests should cover **Run Steering Mode Selector**:

- Missing support selects air.
- Invalid support selects air.
- Non-surface support selects air.
- Valid **Run Surface** support with positive surface-normal lift above tolerance selects air.
- Valid **Run Surface** support with no positive lift selects grounded.
- Valid **Run Surface** support with only downward or into-surface velocity selects grounded.
- Selector decisions do not depend on elapsed time.
- Selector decisions do not depend on launch presentation state.
- Selector decisions do not depend on fall presentation state.
- Selector returns only grounded or air.

EditMode tests should cover **Player Steering Controller** air behavior:

- Post-launch unsupported flight with active steering rotates velocity using air turn authority.
- Post-launch unsupported flight with no active gesture does not apply a steering velocity write.
- Unsupported motion from a later course bump uses air steering while **Running**.
- Unsupported falling before **RunEnded** uses air steering while **Running**.
- Accepted run result stops air steering.
- Leaving **Running** clears any steering-mode state.
- Air steering preserves planar speed.
- Air steering preserves vertical velocity.
- Air steering uses **Run Steering Frame** up.
- Air steering rotates **Run Body** facing when steering is applied.
- Air steering does not apply normal grounded turn authority.
- Grounded steering continues to apply grounded turn authority.
- Stale grounded support with positive lift uses air steering rather than grounded steering.
- Valid grounded support with no lift uses grounded steering.
- Below **Minimum Steer Speed** still skips steering rotation.
- No air speed cap or launch recovery is applied.
- Existing landing stabilization tests stay green and prove lift correction is velocity-only.

EditMode tests should cover steering config:

- Default air turn authority is positive and lower than grounded turn authority.
- Invalid authored air turn authority resolves defensively.
- Existing grounded steering config defaults remain unchanged unless intentionally retuned.

PlayMode tests should cover scene composition:

- Gameplay scene resolves the steering config with the expected authored air steering turn authority.
- Existing slingshot, presentation, camera, run-end, and run-surface scene assertions remain valid.

Manual Unity smoke checks should cover:

- Small pull remains a short launch.
- Maximum pull remains a far launch.
- Air steering during launch flight gently bends direction without visible speed loss.
- Air steering after a side bump feels consistent with launch-flight air steering.
- Grounded steering still feels stronger and more responsive than air steering.
- Repeated win/fail/reset cycles do not leak air steering state into the next prelaunch setup.

## Release and Compatibility

- Assumes the current project Unity version and existing Unity Test Framework setup.
- No package manifest change is expected.
- No ProjectSettings change is expected.
- No save format change is expected.
- No economy, reward, run progress, or run-end compatibility impact is expected.
- No public player-facing input scheme change is expected.
- Player steering config gains a serialized tuning value; existing test fakes and composition tests must be updated.
- The old post-launch steering blocker name should be retired or replaced in code to avoid preserving the wrong concept.
- Existing scenes or config assets may need the new air steering value authored once.
- No migration layer is expected unless implementation chooses to rename an existing serialized field, which this PRD does not require.

## Out of Scope

- Changing **Launch Impulse** values.
- Changing **Run Body Speed Model** tuning or physics material friction.
- Reintroducing player-facing speed caps.
- Reintroducing launch speed recovery.
- Adding downforce, global drag, or artificial air braking.
- Changing **Launch Flight**, **Airborne**, **Slide**, victory, or defeat animation logic.
- Adding banking, pitch, roll, or special air pose behavior.
- Changing run camera behavior.
- Changing run-end flow, rewards, UI, or acknowledgement behavior.
- Changing **Run Surface Context Source** truthfulness or support filtering.
- Replacing Rigidbody movement with `CharacterController`.
- Adding new packages.
- Creating a new mobile input scheme.
- Creating an ADR.

## Further Notes

Assumptions:

- "In-flight steering" means **Run Air Steering Control** while the gameplay state is **Running**.
- "Touch ground" for steering mode means valid **Run Surface** support, not any collider contact.
- Existing slingshot launch energy and explicit speed-ownership decisions remain in force; the grounded speed model stays neutral while unsupported.
- Air steering should be weaker enough to feel like nudging trajectory, not steering on rails.
- The exact air turn authority default is a tuning decision for implementation; the PRD only requires it to be positive, authored, and lower than grounded authority.
- The exact accepted lift tolerance is an implementation tuning detail; it should be high enough to ignore numerical noise and low enough to catch stale grounded takeoff samples.

Unresolved questions:

- What exact default should ship for air turn authority?
- Should accepted lift tolerance reuse the existing launch landing maximum lift speed tolerance, or should the selector own a separate small tolerance?
