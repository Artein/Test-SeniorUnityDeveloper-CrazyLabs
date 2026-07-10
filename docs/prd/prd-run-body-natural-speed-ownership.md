# PRD: Run Body Natural Speed Ownership

Status: Superseded

> Superseded by [PRD: Run Body Explicit Speed Ownership](./prd-run-body-explicit-speed-ownership.md) and [ADR-0010](../adr/adr-0010-use-explicit-run-body-speed-model-with-rigidbody-contact-physics.md). This document remains as historical context for removing steering-owned speed caps and launch recovery; its claim that physics-material contact slowdown alone owns ordinary speed loss is no longer authoritative.

## Problem Statement

The game is about sliding down a course after being fired by a **Slingshot**. The player expects the **Run Body** speed to come from the **Launch Impulse**, the course, and **Run Surface Contact Slowdown**. Instead, the current movement stack can make speed feel like it hits an invisible wall.

The most visible symptom is after **Launch Flight**: even while the **Run Body** is still unsupported, speed can drop sharply and the character may transition into a fall presentation. From the player's perspective this feels like a hidden boundary, not like physical sliding. The same ownership issue also weakens the difference between small and maximum pulls because steering-era speed caps and launch recovery can flatten the physical result of the **Launch Impulse**.

The root problem is not animation, camera, or friction alone. It is speed ownership. **Run Steering Control** should steer direction and preserve the player-facing **Run Body** speed it receives. Ordinary slowdown should come from authored **Run Surface** contact behavior. Defensive validation may exist only for impossible velocity values, not for balance tuning.

## Solution

Remove player-facing **Run Body** speed caps and launch speed recovery from **Run Steering Control**. While **Running**, steering should rotate the current surface-relative velocity according to input, but it should not clamp normal or launch-derived speed back to an authored gameplay maximum.

Keep a condition-based **Post-Launch Steering Gate** so steering input does not fight the fired-by-slingshot interval:

- For a strong launch that becomes unsupported, steering remains inactive until the first valid post-launch **Run Surface** landing.
- For a weak launch that never leaves support, steering becomes active only when the current support is a valid **Run Surface** and the **Run Body** has no positive surface-normal lift.
- No no-takeoff timeout is used to enable steering.

Keep **Launch Landing Stabilization** as a separate contact correction. It may remove unwanted positive lift after the first real post-launch landing, but it must preserve surface-tangent speed.

Keep an unreachable **Run Body Speed Sanity Guard** for non-finite or absurd velocities. This guard is validation, not tuning. It must sit far above normal launch, upgrade, and course speeds so it cannot define run distance.

The resulting user-facing model is simple: pull strength creates launch energy, the **Run Body** carries that speed, steering changes direction, and the **Run Surface** naturally drains speed.

## Unity Surfaces

Runtime assemblies and asmdefs:

- Gameplay runtime assembly containing **Run Steering Control**, **Run Body** movement orchestration, slingshot launch notifications, run-surface support facts, and VContainer composition.
- No new runtime assembly definition is expected for the first implementation.
- No Editor-only assembly is required.

Runtime modules and services:

- **Run Steering Control** controller: remove player-facing planar speed cap, remove launch speed bypass/recovery, and keep direction steering.
- **Post-Launch Steering Gate**: a small deep module that decides whether steering input may affect velocity after launch.
- **Launch Landing Stabilization**: preserved as the owner of first-landing lift suppression, not tangent-speed recovery.
- **Run Body Speed Sanity Guard**: a small defensive module or controller boundary that validates impossible values only.
- **Run Surface Context Source**: remains the support-truth provider used by steering gate and landing stabilization.
- **Slingshot Launch Applied** notification: remains the event that arms post-launch steering and landing state.
- Gameplay stat resolution: no longer used to impose a player-facing maximum planar speed during **Running**.

Scenes, prefabs, ScriptableObjects, and assets:

- Player steering configuration asset should remove or retire authored fields that describe player-facing maximum planar speed, launch burst speed grace, launch burst recovery, and launch speed multiplier.
- If a sanity guard needs serialized tuning, it should use language that makes the defensive purpose explicit.
- Existing slingshot launch config remains the owner of **Launch Impulse** values.
- Existing run-surface physics materials remain the owner of ordinary contact slowdown.
- Existing character presentation assets remain presentation-only and must not control speed.
- Scene composition assertions should be updated so obsolete launch burst defaults cannot remain the protected contract.

Editor, inspectors, importers, and menus:

- No custom inspector, import pipeline, menu item, or editor window is required.
- Existing serialized asset authoring is enough.

Project settings, package manifests, and helper commands:

- No Unity version change.
- No package manifest change.
- No ProjectSettings change.
- Existing Unity connector compile/test workflow remains the verification path.

## User Stories

1. As a player, I want a small **Pull** to produce a short push, so that weak launches feel physically weak.
2. As a player, I want a maximum **Pull** to produce a far push, so that fully stretching the **Band** feels valuable.
3. As a player, I want the **Run Body** to keep launch speed while it is in **Launch Flight**, so that the fired motion does not hit an invisible wall.
4. As a player, I want speed to fade through contact with the **Run Surface**, so that slowdown feels like sliding friction.
5. As a player, I want steering to change direction without secretly reducing speed, so that steering feels like control rather than braking.
6. As a player, I want the end of **Launch Flight** not to cause a sudden speed drop, so that animation transitions do not feel physical.
7. As a player, I want entering **Airborne** presentation not to slow the **Run Body**, so that falling looks different but does not create hidden movement rules.
8. As a player, I want landing after launch to preserve sideways and forward slide speed, so that momentum carries into the level.
9. As a player, I want post-launch bounce to be reduced only in the surface-normal direction, so that landing feels stable without killing momentum.
10. As a player, I want weak grounded launches to become steerable when they are actually sliding, so that a low-energy run is still controllable.
11. As a player, I want strong unsupported launches not to accept steering too early, so that steering does not fight the shot while the character is flying.
12. As a player, I want no arbitrary timeout that changes steering behavior while still grounded, so that control changes follow visible contact.
13. As a player, I want high-speed launches to remain high-speed if the course allows them, so that max pull has a clear reward.
14. As a player, I want excessive speed to be solved by course friction and launch tuning, so that the game still feels physical.
15. As a player, I want the avatar to keep visually matching movement, so that presentation does not imply a fake collision.
16. As a player, I want run distance to come from physical momentum and course design, so that outcomes feel fair.
17. As a player, I want maximum pull to feel meaningfully different from medium pull, so that launch skill matters.
18. As a player, I want medium pull to land between weak and maximum results, so that the **Band** feels analog.
19. As a player, I want steering responsiveness to remain familiar, so that this change does not make input feel detached after landing.
20. As a player, I want the **Run Body** not to accelerate artificially during steering, so that no hidden boost is introduced.
21. As a player, I want the **Run Body** not to decelerate artificially during steering, so that no hidden brake is introduced.
22. As a designer, I want **Run Surface Contact Slowdown** to own ordinary speed loss, so that level feel is tuned through authored surfaces.
23. As a designer, I want **Launch Impulse** to own fired energy, so that pull strength and upgrades have a clear balance surface.
24. As a designer, I want **Run Steering Control** to own direction only, so that movement tuning has fewer competing knobs.
25. As a designer, I want a defensive speed guard to be clearly separate from gameplay tuning, so that it is not used as a balance cap.
26. As a designer, I want the sanity guard threshold to be far above normal play, so that it never shapes ordinary run distance.
27. As a designer, I want launch speed recovery fields removed or retired, so that no one can tune an invisible post-launch wall.
28. As a designer, I want max planar speed fields removed from normal steering behavior, so that there is no player-facing cap.
29. As a designer, I want surface friction to remain available for later tuning, so that strong launch energy can still be balanced naturally.
30. As a designer, I want weak grounded launches to stay possible, so that poor pulls can fail quickly rather than being secretly upgraded.
31. As a designer, I want no new movement state named after animation, so that gameplay logic stays independent from presentation.
32. As a designer, I want **Launch Flight** to remain presentation language, so that animation changes cannot alter velocity.
33. As a designer, I want **Airborne** to remain fall presentation language, so that it cannot define speed rules.
34. As a designer, I want landing stabilization values to remain small and focused, so that they do not become hidden damping.
35. As a designer, I want the first valid landing to be detected from **Run Surface** support, so that steering resumes on real playable terrain.
36. As a designer, I want obstacle, finish, safety, and invalid contacts not to resume post-launch steering, so that control follows run-surface truth.
37. As a gameplay engineer, I want **Run Steering Control** tests to prove over-speed is preserved, so that caps do not return unnoticed.
38. As a gameplay engineer, I want **Post-Launch Steering Gate** isolated behind a small interface, so that launch/landing readiness is testable without a scene.
39. As a gameplay engineer, I want **Run Body Speed Sanity Guard** isolated behind a small interface or helper, so that impossible-value handling is testable without steering complexity.
40. As a gameplay engineer, I want the steering controller to orchestrate modules rather than embed all launch state rules, so that the class stays readable.
41. As a gameplay engineer, I want launch speed bypass state removed, so that there is no temporary exception to a cap that no longer exists.
42. As a gameplay engineer, I want launch burst recovery state removed, so that landing does not start an automatic speed decay.
43. As a gameplay engineer, I want stat resolution no longer to cap planar velocity, so that upgrades cannot reintroduce hidden braking.
44. As a gameplay engineer, I want **Minimum Steer Speed** to remain only a steering-rotation gate, so that it does not become a speed clamp.
45. As a gameplay engineer, I want low-speed stabilization to write velocity only when lift is actually removed, so that normal low-speed behavior stays quiet.
46. As a gameplay engineer, I want steering rotation to preserve the magnitude of the steerable velocity, so that the controller changes direction only.
47. As a gameplay engineer, I want steering to keep using the current **Run Steering Frame**, so that banked surfaces remain supported.
48. As a gameplay engineer, I want landing stabilization to use raw **Run Surface Context** ground normal, so that lift suppression matches the surface actually touched.
49. As a gameplay engineer, I want landing stabilization to require observed unsupported motion before treating a later grounded sample as landing, so that stale support hysteresis does not kill launch lift.
50. As a gameplay engineer, I want weak grounded launch steering readiness separate from landing stabilization, so that no-takeoff launches can still become controllable.
51. As a gameplay engineer, I want leaving **Running** to clear post-launch gate and stabilization state, so that state cannot leak into the next run.
52. As a gameplay engineer, I want a new launch to reset post-launch gate and stabilization state, so that consecutive runs are deterministic.
53. As a gameplay engineer, I want no dependency from steering to **Character Presentation Mode**, so that animation cannot affect movement.
54. As a gameplay engineer, I want no dependency from steering to camera, run-end UI, economy, or rewards, so that this change stays local.
55. As a gameplay engineer, I want scene composition tests to reject obsolete speed-recovery authoring, so that assets do not drift back.
56. As a gameplay engineer, I want tests to use launch-applied velocity for launch impulse comparisons, so that steering/contact side effects do not pollute launch-energy assertions.
57. As a gameplay engineer, I want tests to avoid timing waits for the no-timeout steering gate, so that verification stays deterministic.
58. As a technical artist, I want **Launch Flight** and **Airborne** transitions to remain animation-only, so that animation work cannot break movement.
59. As a technical artist, I want no new animation clip requirement for this speed ownership change, so that implementation is not blocked by art.
60. As a QA engineer, I want a test proving small and max pull launch events are meaningfully different, so that **Band** strength is protected.
61. As a QA engineer, I want a test proving unsupported post-launch speed is not clamped, so that the invisible wall is covered.
62. As a QA engineer, I want a test proving landing does not start speed recovery, so that post-landing motion remains natural.
63. As a QA engineer, I want a test proving steering rotates high-speed velocity without reducing magnitude, so that direction-only steering is covered.
64. As a QA engineer, I want a test proving non-launch over-speed is not clamped by steering, so that caps are removed generally.
65. As a QA engineer, I want a test proving impossible non-finite velocity is guarded, so that validation still exists.
66. As a QA engineer, I want a test proving absurd but finite velocity is guarded above normal play range, so that catastrophic values are contained.
67. As a QA engineer, I want a test proving ordinary maximum launch speed is below the guard threshold, so that the guard cannot affect normal runs.
68. As a QA engineer, I want a test proving stale grounded launch samples do not enable steering when lift is positive, so that takeoff is not fought.
69. As a QA engineer, I want a test proving grounded no-lift weak launch enables steering, so that weak launches do not get stuck without input.
70. As a QA engineer, I want a test proving observed unsupported motion followed by valid support enables steering, so that strong launches regain control at landing.
71. As a QA engineer, I want a test proving invalid support does not enable steering, so that obstacles and safety volumes do not count as playable landing.
72. As a QA engineer, I want a test proving no timeout exists for post-launch steering enablement, so that the rejected fallback cannot reappear.
73. As a maintainer, I want glossary terms to use **Run Body** after launch and **Launch Target** only at the slingshot, so that future code reviews use precise language.
74. As a maintainer, I want old names such as launch burst to be removed where the concept is removed, so that code does not describe the wrong behavior.
75. As a maintainer, I want any remaining legacy asset names to be documented as legacy only, so that naming cleanup can be planned separately.
76. As a maintainer, I want no new package dependency, so that this remains a contained gameplay change.
77. As a maintainer, I want no Unity version change, so that verification remains on the current project baseline.
78. As a maintainer, I want this PRD to make the ownership boundary explicit, so that future tuning does not re-add hidden speed caps.

## Implementation Decisions

- Treat **Run Body** speed as player-facing physical state during **Running**.
- Treat **Run Steering Control** as direction control, not speed control.
- Remove ordinary maximum planar speed clamping from steering behavior.
- Remove launch speed bypass because it only existed to avoid a cap that should no longer affect player-facing motion.
- Remove launch burst recovery because ordinary slowdown belongs to **Run Surface Contact Slowdown**.
- Do not replace the removed cap with a different presentation-state or launch-state speed clamp.
- Keep steering input mapping, gesture lifecycle, responsiveness smoothing, turn rate, and **Run Steering Frame** behavior unless implementation reveals a direct compile dependency.
- Keep **Minimum Steer Speed** as a minimum speed for applying steering rotation, not as a velocity modifier.
- Keep **Launch Landing Stabilization** separate from **Post-Launch Steering Gate**.
- **Launch Landing Stabilization** may remove only positive surface-normal lift and must preserve surface-tangent velocity.
- **Launch Landing Stabilization** still requires observed unsupported support state before treating later support as a true landing.
- Add or extract **Post-Launch Steering Gate** as a deep module with a small decision surface: launch arms the gate, support and velocity facts update it, and the controller asks whether steering may apply.
- **Post-Launch Steering Gate** enables steering after observed unsupported motion followed by valid **Run Surface** support.
- **Post-Launch Steering Gate** enables steering for weak grounded launches only when support is valid **Run Surface** and velocity has no positive surface-normal lift.
- **Post-Launch Steering Gate** must not use elapsed time to enable steering.
- **Post-Launch Steering Gate** must clear when gameplay leaves **Running**.
- **Post-Launch Steering Gate** must reset on each accepted **Launch**.
- Do not let **Launch Flight**, **Airborne**, Animator states, or presentation elapsed time influence steering enablement.
- Add or extract **Run Body Speed Sanity Guard** as a defensive validation boundary.
- The sanity guard may handle non-finite velocity by replacing it with a safe value or rejecting a write according to the existing project style.
- The sanity guard may clamp absurd finite values only at a threshold unreachable by normal **Launch**, upgrades, and authored course traversal.
- If serialized, the sanity guard field name and tooltip must say defensive validation or impossible velocity, not max speed or movement limit.
- If the existing Player Max Speed stat remains in the project, **Run Steering Control** must not consume it as a velocity cap.
- Any exposed upgrade or economy copy that still promises a maximum-speed cap needs a separate product decision; this PRD only decouples steering from hidden speed limiting.
- Slingshot **Launch Impulse** tuning remains owned by the slingshot launch configuration and impulse calculator.
- Surface slowdown remains owned by authored **Run Surface** physics behavior.
- Do not add a new global drag, downforce, or artificial resistance system in this pass.
- Preserve current Rigidbody-backed movement architecture.
- Preserve current VContainer composition style with plain C# controllers and shallow Unity views.
- Update scene composition assertions to match the new ownership model rather than preserving obsolete launch burst values.
- Update local glossary/context docs if implementation reveals any remaining terminology conflict.

## Testing Decisions

Good tests should assert external behavior at the movement boundary: given launch, support, input, and velocity facts, the **Run Body** should keep or change velocity in the way a player would observe. Tests should avoid asserting private field names and should not use animation state as an oracle for movement behavior.

EditMode tests should cover **Post-Launch Steering Gate**:

- New launch starts with steering gated.
- Unsupported support after launch keeps steering gated.
- Unsupported support followed by valid **Run Surface** support enables steering.
- Stale grounded support with positive surface-normal lift keeps steering gated.
- Grounded valid **Run Surface** support with no positive lift enables steering for weak no-takeoff launches.
- Invalid, obstacle, finish, safety, trigger, or missing-contact support does not enable steering.
- Time advancing alone does not enable steering.
- Leaving **Running** clears gate state.
- A new launch resets gate state.

EditMode tests should cover **Run Steering Control** controller behavior:

- Unsupported post-launch velocity above old max speed is not clamped.
- Landing after launch does not start speed recovery.
- Non-launch over-speed is not clamped by steering.
- Steering rotates planar velocity without reducing its magnitude.
- Steering still uses the current **Run Steering Frame** up direction.
- Below **Minimum Steer Speed** skips steering rotation without applying a speed cap.
- Low-speed landing stabilization may apply velocity-only lift suppression.
- Landing stabilization preserves surface-tangent speed on flat and banked surfaces.
- Leaving **Running** clears post-launch gate, landing stabilization, and any obsolete speed state.

EditMode tests should cover **Run Body Speed Sanity Guard**:

- Finite normal launch velocities pass unchanged.
- Expected maximum authored launch velocities pass unchanged.
- Non-finite velocity is rejected or sanitized deterministically.
- Absurd finite velocity beyond the guard threshold is contained.
- Guard behavior is independent of steering input.

EditMode tests should cover slingshot launch energy where relevant:

- Minimum accepted pull produces the configured minimum forward impulse.
- Maximum pull produces the configured maximum forward and upward impulse.
- Mid pull is monotonic between minimum and maximum.
- Launch upgrades, if present, scale launch impulse without being flattened by steering.

PlayMode tests should cover scene composition and focused gameplay smoke behavior:

- Gameplay scene resolves steering configuration without obsolete launch burst/recovery as the active movement contract.
- If a sanity guard is serialized, its scene-authored threshold is above expected maximum launch and upgrade speeds.
- Slingshot input smoke test compares **Slingshot Launch Applied** velocity changes for small and maximum pulls and proves max is meaningfully larger.
- Scene-level tests use launch-applied velocity as the oracle for launch impulse strength, not Rigidbody velocity after contact and steering have run.
- Optional smoke scenario can compare short-window travel for small versus max pulls, but only if it can be made deterministic without time-based flakiness.

Verification workflow:

- Run Unity connector compile before tests.
- Run targeted EditMode tests for steering gate, steering controller, sanity guard, and slingshot launch impulse.
- Run targeted PlayMode tests for scene composition and slingshot input.
- Fix compile errors before running tests.

## Release and Compatibility

- Assumes the current Unity project version and existing Rigidbody-backed movement architecture.
- No new package dependency, Unity upgrade, Addressables schema change, or ProjectSettings change is required.
- Public gameplay interfaces may change if obsolete max-speed or launch-burst properties are removed from steering configuration.
- Serialized steering configuration assets require an intentional clean update because obsolete speed-cap and launch-recovery fields no longer represent valid behavior.
- No save-format migration is expected.
- Existing tests and fakes that still provide maximum planar speed or launch burst properties should be updated to the new ownership model.
- The existing Player Max Speed stat or upgrade, if still exposed to players, is a compatibility/product risk because it no longer belongs to **Run Steering Control** as a velocity cap.
- If the team wants to preserve a speed-related upgrade, that should become a separate product design for launch energy, surface interaction, steering responsiveness, or another visible effect.
- No ADR is required for a first implementation unless the team wants to record "no player-facing Run Body speed cap" as a durable architecture decision.

## Out of Scope

- Changing slingshot pull input, **Band Shape**, **Pull Plane**, or launch validation.
- Retuning launch impulse values beyond what is already established.
- Retuning terrain friction or physics materials in this pass.
- Adding downforce, artificial drag, magnet-to-surface behavior, or a global resistance system.
- Replacing Rigidbody movement with `CharacterController`.
- Changing **Run Surface Context Source** truthfulness or support filtering.
- Changing **Character Presentation Mode** selection, animation clips, Animator Controller transitions, or visual follower smoothing.
- Changing camera behavior.
- Changing run progress, run end, lost momentum, rewards, economy, pickups, UI, or persistence.
- Redesigning upgrade economy or player-facing upgrade copy.
- Adding new art, VFX, SFX, haptics, tutorials, or telemetry.

## Further Notes

- This PRD uses the updated glossary split: **Launch Target** is the slingshot-facing role, while **Run Body** is the post-launch physical role during **Running**.
- Older code or asset names may still use **Launch Target** for the same object. Implementation should rename only where scoped and safe, and should not mix broad naming cleanup with the movement behavior change unless explicitly planned.
- The key acceptance test is qualitative but concrete: ending **Launch Flight** or entering **Airborne** must not cause a hidden velocity wall.
- If future playtests show maximum launch speed is too high, tune **Launch Impulse** or **Run Surface Contact Slowdown** first. Do not reintroduce a steering-owned player-facing speed cap.
