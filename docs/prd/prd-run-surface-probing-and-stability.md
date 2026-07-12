# PRD: Run Surface Probing and Stability

## Problem Statement

The Run Body needs one coherent answer to two different questions on every fixed step:

- What support did Unity physics observe beneath the support footprint on this tick?
- What support should locomotion trust after accounting for very short misses and implausible one-tick normal discontinuities?

The current implementation does not represent those questions separately. PhysicsRunSurfaceContextSource performs Unity physics queries and same-tick candidate selection, but it also owns cross-tick grounding grace and large-normal confirmation through hard-coded sample thresholds. RunSurfaceSteeringFrameSource then applies a second, independently configured layer of missing-support grace and suspect-normal confirmation before producing the Run Steering Frame.

Tuning is split between scene-authored probe distance and mask, baked physics-source thresholds, and steering-frame values in RunBodyMovementConfig. At the current 0.02-second fixed timestep, the physics source retains one missed sample and confirms a large normal after two matching samples. The active movement asset then retains its steering frame for 0.12 seconds and waits 0.6 seconds before accepting a suspect normal. The two layers do not share transition state, candidate-coherence semantics, or one time model.

Fast seams, troughs, brief support loss, and fixed-timestep changes can therefore affect grounding, movement-plane selection, airtime, presentation, diagnostics, and steering on different schedules. IRunSurfaceContextSource.Current compounds the ambiguity: it looks like direct physics truth but is already temporally filtered. Some consumers treat it as locomotion truth, while presentation and diagnostics describe it as raw support.

The existing Run Steering Frame Stability PRD assumed that the physics source remained raw. Later source-level temporal filtering created architectural drift from that assumption. The finding is valid as an ownership and tunability problem, but remains P2 by current evidence unless a deterministic gameplay reproduction demonstrates release-blocking player impact sufficient for P1.

## Solution

Introduce one atomic fixed-step **Run Surface Frame Pipeline** with narrow collaborators and one immutable output:

1. **Physics Run Support Probe** owns Unity physics access, hit validation, footprint sampling, same-tick normal clustering, and current-tick candidate selection.
2. **Run Surface Stability Policy** is the sole owner of cross-tick locomotion support retention, support loss, normal-discontinuity confirmation, and transition classification.
3. **Run Steering Frame Policy** owns continuous angular response and optional airborne up-frame memory. It does not independently decide whether support is trustworthy.
4. **Run Surface Frame Snapshot** atomically exposes current-tick observation, stable locomotion context, transition metadata, diagnostic policy state, and Run Steering Frame.

Call the probe result **Observed Support**, not raw physics. The probe may cast multiple rays, reject hits, cluster normals, and choose one candidate. Observed Support is the selected result after same-tick spatial rules but before cross-tick retention or confirmation.

Observed Support has three states:

- **Unavailable**: no valid Run Progress Frame exists; the observation is meaningless and temporal state hard-resets.
- **Missing**: the progress frame is valid but no acceptable support was found; the miss participates in support-loss timing.
- **Supported**: a valid normal and distance were selected for this tick.

The stability policy produces **Stable Locomotion Support** with these rules:

- first valid support acquires immediately;
- brief Missing intervals retain the last stable support;
- sustained Missing intervals lose support;
- Unavailable hard-resets immediately;
- continuous normal changes are accepted;
- a large one-tick change is retained as a candidate while the previous stable normal remains authoritative;
- coherent large candidates confirm after a configured duration and accept their normalized representative normal;
- alternating or incoherent seam normals do not accumulate toward confirmation;
- reacquisition after support loss is immediate.

All temporal thresholds use seconds. The first qualifying observation contributes the current fixed delta time, transition occurs when elapsed time is greater than or equal to the threshold, and zero duration applies immediately. Variation between fixed timesteps is limited to one-tick quantization.

The policy emits one transition: None, Continuous Update, Support Acquired, Support Lost, Confirmed Discontinuity, or Hard Reset. It also reports whether missing support is being held or a discontinuity is being confirmed.

The steering policy consumes that shared decision:

- Continuous Update slews toward stable normal at the configured angular rate.
- Confirmed Discontinuity snaps to the accepted stable normal.
- Support Acquired initializes from stable normal.
- Support Lost may retain the last steering up for a short airborne-control window, but never changes locomotion back to grounded.
- Hard Reset clears steering state.

Configuration follows responsibility:

- **Run Surface Probe Config**: distance, skin width, surface mask, minimum support-normal dot, footprint sample-offset scale, and footprint normal-cluster angle. Fixed buffer capacity remains an implementation detail.
- **Run Surface Stability Config**: support-loss confirmation seconds, discontinuous-normal threshold degrees, discontinuous-normal confirmation seconds, and candidate-coherence degrees.
- **Run Steering Frame Config**: continuous-normal slew degrees per second and airborne up-frame retention seconds after duplicate validation is removed.

The first migration preserves serialization: scene fields and RunBodyMovementConfig remain authored where they are and are copied into immutable runtime configs. A dedicated RunSurfaceTuning ScriptableObject is cleaner long term, but it is a separate serialization decision rather than a prerequisite.

Consumer mapping becomes explicit:

- movement plane, grounded speed, steering mode, launch/landing stabilization, and default gameplay airtime consume Stable Locomotion Support;
- steering consumes stable support plus transition metadata;
- character presentation should consume Observed Support through its existing tracker, subject to approval of that semantic change;
- diagnostics expose observed support, stable support, transition state, and steering frame together.

Introduce IRunSurfaceFrameSource. Keep a temporary IRunSurfaceContextSource compatibility adapter returning Stable Locomotion Support, migrate consumers individually, then remove the ambiguous legacy interface.

## Unity Surfaces

Runtime assemblies and deep modules:

- Game.Gameplay continues to own the pipeline, contracts, policies, composition, and consumers.
- Game.Foundation continues to provide fixed delta time.
- The deep modules are Run Surface Frame Pipeline, Run Surface Stability Policy, Physics Run Support Probe, and Run Steering Frame Policy.
- No new runtime asmdef, physics package, or custom solver is required.

Editor and authoring:

- No new Editor assembly, window, importer, menu item, or custom inspector is required.
- Existing validation should cover authored probe, stability, and steering settings.
- Mutable policy state remains runtime-owned, never stored in a ScriptableObject.

Scenes, prefabs, and assets:

- Assets/Scenes/GameplayScene.unity currently owns the physics scene installer, support collider, 0.08-meter probe distance, and Run Surface mask.
- Assets/Game/Gameplay/RunBodyMovementConfig.asset currently owns the steering-frame values and remains serialization-compatible initially.
- The scene installer stays a shallow adapter that validates references and constructs immutable runtime config.
- No geometry, collider topology, physics material, Rigidbody, prefab hierarchy, or containment change is required.
- A later RunSurfaceTuning asset requires explicit serialization approval.

Project and tests:

- Target Unity 6000.3.18f1 and the existing 0.02-second fixed timestep; neither changes.
- Game.Gameplay.Tests.EditMode owns pure policy, config, consumer, and composition coverage.
- Game.Gameplay.Tests.PlayMode owns Unity probe and deterministic seam, trough, and short-gap scenarios.
- GameplayLifetimeScope tests continue protecting shared identity and fixed-tick order.
- No package manifest, package lock, Addressables, helper-command, install, or sync change is required.
- Use the existing Unity AI Agent connector for compile and targeted tests.

## User Stories

1. As a player, I want a visually continuous seam not to twitch steering so that the course feels intentional.
2. As a player, I want a short trough not to cancel grounded behavior so that traversal remains smooth.
3. As a player, I want real airtime recognized promptly so that jumps remain honest.
4. As a player, I want steering and movement to agree about support so that controls do not fight motion.
5. As a player, I want confirmed sharp banks to respond quickly so that filtering does not feel sluggish.
6. As a player, I want alternating seam normals rejected so that contact noise does not decide my run.
7. As a player, I want repeatable results over the same geometry so that outcomes remain learnable.
8. As a player, I want launch landing to survive a tiny probe gap so that run entry remains stable.
9. As a player, I want one noisy tick not to remove grounded speed behavior so that momentum remains readable.
10. As a player, I want steering memory not to fake grounding so that airborne control remains distinct.
11. As a designer, I want acquisition values grouped by responsibility so that probing is tuned coherently.
12. As a designer, I want support-loss grace in seconds so that its meaning survives timestep changes.
13. As a designer, I want discontinuity confirmation in seconds so that it describes player-visible duration.
14. As a designer, I want candidate coherence tunable so that small PhysX variation does not restart confirmation.
15. As a designer, I want steering slew separate from support trust so that response can change without changing gameplay truth.
16. As a designer, I want airborne up retention separate from grounded grace so that steering memory cannot award grounded benefits.
17. As a designer, I want zero-duration settings to apply immediately so that grace can be disabled explicitly.
18. As a designer, I want invalid values rejected during authoring so that bad tuning fails early.
19. As a designer, I want the first migration behavior-compatible so that architecture is not confused with retuning.
20. As a designer, I want observed and stable diagnostics together so that each threshold has visible effects.
21. As a designer, I want Run Surface geometry to remain authoritative so that stability does not become invisible containment.
22. As a designer, I want a dedicated tuning asset considered separately so that asset organization does not block the fix.
23. As an engineer, I want current-tick selection named Observed Support so that it is not confused with every raw hit.
24. As an engineer, I want Unavailable, Missing, and Supported states so that invalid frames differ from contact loss.
25. As an engineer, I want Unavailable to hard-reset so that stale support cannot survive invalid progress data.
26. As an engineer, I want Missing to participate in one shared loss timer so that short gaps can be stabilized.
27. As an engineer, I want acquisition and reacquisition immediate so that stability adds no avoidable landing latency.
28. As an engineer, I want one locomotion stability policy so that consumers cannot compound hidden delays.
29. As an engineer, I want that policy in plain C# so that fixed-step sequences are deterministic in EditMode.
30. As an engineer, I want Unity physics behind a shallow adapter so that engine queries do not own gameplay policy.
31. As an engineer, I want footprint clustering retained as a same-tick concern so that several hits become one observation.
32. As an engineer, I want continuity tie-breaking passed explicitly so that spatial selection does not hide temporal state.
33. As an engineer, I want angular candidate coherence so that confirmation does not require identical normals.
34. As an engineer, I want incoherent candidates to restart confirmation so that alternating seams never confirm accidentally.
35. As an engineer, I want accepted candidates averaged and normalized so that their result represents the coherent observations.
36. As an engineer, I want duration boundary semantics documented so that the first tick and zero values are testable.
37. As an engineer, I want transition reasons emitted so that downstream systems do not re-infer them.
38. As an engineer, I want held and confirming flags so that retained values remain diagnosable.
39. As an engineer, I want one immutable snapshot so that consumers cannot observe a partially updated tick.
40. As an engineer, I want observed and stable slopes built from the same progress frame so that both contexts are coherent.
41. As an engineer, I want steering to consume confirmed transitions so that it has no second validation state machine.
42. As an engineer, I want continuous normals to slew and confirmed jumps to snap so that response matches transition meaning.
43. As an engineer, I want airborne up memory not to change stable grounding so that support truth remains singular.
44. As an engineer, I want locomotion consumers on Stable Support so that movement-plane, speed, steering mode, and landing agree.
45. As an engineer, I want airtime and presentation semantics selected explicitly so that rewards and visuals are not accidental.
46. As an engineer, I want a compatibility adapter so that consumer migration can be incremental.
47. As an engineer, I want the ambiguous legacy source removed afterward so that future consumers must choose semantics.
48. As an engineer, I want immutable config injected through VContainer so that dependencies remain explicit.
49. As an engineer, I want mutable state runtime-owned so that serialized assets remain data only.
50. As an engineer, I want no fixed-tick allocations so that mobile garbage collection is unaffected.
51. As an engineer, I want reusable physics buffers internal so that capacity is not designer-facing without evidence.
52. As an engineer, I want one pipeline entry point before movement so that order is centralized.
53. As an engineer, I want one singleton exposed through narrow interfaces so that state cannot split across registrations.
54. As an engineer, I want Rigidbody contact ownership and one movement writer preserved so that this does not become custom physics.
55. As an engineer, I want config extraction separated from behavior changes so that reviews remain clear.
56. As a QA engineer, I want tests for brief and sustained Missing so that both retention and loss are covered.
57. As a QA engineer, I want tests for Unavailable hard reset so that invalid-frame behavior is covered.
58. As a QA engineer, I want tests for immediate acquisition and reacquisition so that landing latency is covered.
59. As a QA engineer, I want tests for one large spike and a coherent confirmed jump so that both noise and real transitions are covered.
60. As a QA engineer, I want alternating-normal tests so that seam oscillation cannot confirm.
61. As a QA engineer, I want 0.02- and 0.01-second tests so that timestep variation stays within one tick.
62. As a QA engineer, I want zero-duration tests so that disabling grace is deterministic.
63. As a QA engineer, I want steering tests for slew, snap, airborne retention, and reset so that response is isolated.
64. As a QA engineer, I want composition tests for identity and order so that VContainer wiring cannot regress.
65. As a QA engineer, I want deterministic PlayMode seam and short-gap fixtures so that Unity physics and policy are tested together.
66. As a QA engineer, I want per-tick observed, stable, transition, steering, and movement assertions so that failure ownership is visible.
67. As a QA engineer, I want screenshots diagnostic-only so that committed tests remain deterministic.
68. As a maintainer, I want diagnostics to stop calling filtered support raw so that terminology matches behavior.
69. As a maintainer, I want the older steering-frame PRD marked superseded where its raw-source assumption is obsolete.
70. As a maintainer, I want characterization tests before duplicate filtering is removed so that behavior changes are measurable.
71. As a maintainer, I want each migration slice to compile and pass targeted tests so that the refactor remains reviewable.
72. As a maintainer, I want no new package, Unity upgrade, or ProjectSettings change so that delivery risk stays local.
73. As a reviewer, I want final tuning based on deterministic scenarios rather than intuition.
74. As a reviewer, I want architecture validity separated from P1 severity so that priority reflects evidence.
75. As a reviewer, I want unresolved consumer and serialization choices visible so that implementation does not bury product decisions.

## Implementation Decisions

- Define Observed Support after validation, footprint selection, clustering, and ranking but before cross-tick policy.
- Represent observation state as Unavailable, Missing, or Supported.
- Hard-reset on invalid Run Progress Frame; apply loss timing only to valid Missing observations.
- Keep same-tick spatial selection in the probe and all cross-tick locomotion state in one stability policy.
- Pass continuity preference into the selector explicitly.
- Implement policies as runtime-owned plain-C# services with immutable constructor-injected config.
- Acquire and reacquire valid support immediately.
- Retain stable support until Missing duration reaches its threshold.
- Accept continuous normals immediately; hold discontinuous candidates while confirming them.
- Confirm by angular coherence, accumulate a representative normal, and reset on incoherent candidates.
- Use seconds; the first observation contributes delta time; transition at greater-than-or-equal; zero is immediate.
- Publish None, Continuous Update, Support Acquired, Support Lost, Confirmed Discontinuity, or Hard Reset.
- Publish held-missing and confirming-discontinuity diagnostic flags.
- Calculate observed and stable slope from their respective normals and the same progress snapshot.
- Execute progress snapshot, probe, stability, context construction, steering, then atomic publication.
- Keep the previous complete snapshot visible until the next is fully built.
- Use one fixed-tick pipeline before movement consumers.
- Let steering choose initialization, slew, snap, airborne retention, or clear from shared transition metadata.
- Never let steering memory change locomotion grounding.
- Preserve Rigidbody gravity, contacts, collision response, separation, and the one-writer movement boundary.
- Put spatial fields in Run Surface Probe Config and temporal locomotion fields in Run Surface Stability Config.
- Reduce steering config to angular response and airborne memory after duplicate validation is removed.
- Keep query-buffer capacity internal until overflow evidence says otherwise.
- Validate masks, references, distances, durations, normal thresholds, and angular tolerances.
- Keep scene serialization shallow and copy authored values into immutable runtime config.
- Preserve serialized names and active values during the initial migration.
- Defer a dedicated RunSurfaceTuning asset to a separately approved serialization change.
- Introduce a Run Surface Frame source and keep a temporary stable-context compatibility adapter.
- Migrate movement plane, speed, steering mode, launch/landing, and default airtime to Stable Locomotion Support.
- Migrate steering to stable support plus transition metadata.
- Prefer Observed Support for presentation through its existing tracker, subject to acceptance.
- Expose observed, stable, transition, policy-state, and steering values in diagnostics.
- Remove raw terminology for filtered values and remove the legacy interface after all consumers migrate.
- Keep the fixed-tick path allocation-free and use one VContainer singleton through narrow interfaces.
- Do not introduce static state, service location, scene lookup from policies, or a second implementation.
- Slice 1: characterize current compounded behavior at representative timesteps, misses, and angle boundaries.
- Slice 2: extract probe config without behavior change.
- Slice 3: introduce the three-state observation contract.
- Slice 4: extract stability policy while preserving source-level behavior.
- Slice 5: publish observed and stable contexts with compatibility adapter.
- Slice 6: migrate consumers explicitly.
- Slice 7: move steering response behind the atomic pipeline while preserving response behavior.
- Slice 8: remove duplicate steering validation and retune after deterministic scenarios are accepted.
- Slice 9: consider a dedicated tuning asset after serialization approval.

## Testing Decisions

Characterize current behavior first:

- Capture output for first support, one and two misses, small changes, changes above 45 degrees, between 45 and 60 degrees, and above 60 degrees.
- Capture compounded behavior at 0.02 seconds with active asset values, then representative sequences at 0.01 seconds.
- Assert external outputs, not private counters.

Pure EditMode stability tests cover:

- Unavailable hard-reset from initial and active states.
- Immediate acquisition and reacquisition.
- Continuous update.
- Missing retention, exact-threshold loss, and no repeated loss transition.
- One discontinuous spike, coherent confirmation, small candidate noise, incoherent replacement, alternating candidates, and return to stable normal.
- Finite normalized representative normals.
- Zero-duration loss and confirmation.
- Negative or invalid delta-time handling at the pipeline boundary.
- Equivalent durations at 0.02 and 0.01 seconds within one-tick quantization.

Steering policy tests cover:

- acquisition initialization, continuous slew rate, confirmed snap, support-loss retention, reacquisition, hard reset, invalid directions, and idempotent reads;
- explicit proof that airborne frame retention never changes stable grounding;
- explicit proof that confirmed discontinuity receives no second confirmation delay.

Probe, snapshot, composition, and consumer tests cover:

- mask and minimum-normal filtering, footprint clustering, deterministic ranking, and continuity tie-breaking;
- distinct Missing and Unavailable causes and use of every runtime probe value;
- buffer reuse without per-tick allocation;
- one-tick coherence across observed context, stable context, transition, and steering frame;
- one shared VContainer pipeline and ordering before RunBodyMovementController;
- compatibility adapter mapping and explicit consumer mappings;
- diagnostics terminology and serialized-value copying;
- actionable scene-reference and config-validation failures.

Deterministic PlayMode tests cover:

- runtime-built flat-to-bank seam, short support gap, and controlled competing-normal trough;
- per-tick Observed Support, Stable Locomotion Support, transition, Run Steering Frame, and movement mode;
- rejection of one-tick discontinuity, retention then loss over a gap, and coherent confirmed bank transition;
- condition-based completion with timeout, never arbitrary timing sleeps;
- screenshots only as diagnostics, never the committed oracle.

Verification order is compile, focused EditMode tests, then focused PlayMode tests. Existing Run Surface, steering-frame, movement, presentation, airtime, and composition regressions remain green. The planned work does not touch Jobs or Burst; if that changes, Burst compile guards become mandatory.

## Release and Compatibility

- Target Unity 6000.3.18f1 and the existing dependency set.
- Do not change package manifests, ProjectSettings, Addressables, build targets, or Unity version.
- Preserve GameplayScene references, current probe distance and mask, and active movement-asset values initially.
- Preserve serialized names until an approved migration supplies safe rename or explicit asset updates.
- Keep IRunSurfaceContextSource temporarily, returning Stable Locomotion Support.
- Remove compatibility only after repository search and composition tests prove it unused.
- Keep the Game.Gameplay assembly boundary and current test assemblies.
- Preserve VContainer singleton identity, fixed-tick order, Rigidbody contact ownership, and one movement writer.
- Avoid player-facing retuning in the structural change. Map sample counts to the closest documented duration when exact behavior cannot be timestep-independent.
- Deliver duplicate-filter removal and final tuning after deterministic fixtures exist.
- Treat presentation migration and a dedicated tuning asset as explicit behavior/serialization approvals.
- No package version, install, sync, or documentation-only changelog change is required.
- Keep the finding at P2 unless deterministic evidence demonstrates material release impact.

## Out of Scope

- Replacing Rigidbody physics, using CharacterController, DOTS Physics, Jobs, Burst, ECS, or a custom solver.
- Changing speed ownership, steering input, launch impulse, gravity, collision response, or the one-writer invariant.
- Redesigning meshes, colliders, Course Lip geometry, materials, layers, or containment.
- Adding hidden walls, recentering, escape prevention, or artificial grounding.
- Changing fixed timestep, Unity version, packages, Addressables, or build settings.
- Exposing every raw RaycastHit to gameplay or making buffer capacity designer-authored without evidence.
- Rewriting presentation, camera, animation, slingshot, economy, pickups, or run-end behavior.
- Deciding new airtime reward semantics without product confirmation.
- Creating custom editor tooling, a new debug system, helper command, or RPC endpoint.
- Moving tuning into a new ScriptableObject in the first migration.
- Selecting final thresholds solely from current constants.
- Reclassifying the finding as P1 without a deterministic release-impact reproduction.
- Publishing this PRD remotely.

## Further Notes

Assumptions:

- Observed Support is the selected current-tick candidate, not every raw hit.
- Stable Locomotion Support is the default truth for movement, speed, steering mode, landing, and airtime.
- Presentation should eventually consume Observed Support through its own tracker, but that change is isolated.
- Initial implementation preserves serialized assets and maps their values into new runtime configs.
- A dedicated RunSurfaceTuning asset is cleaner long term but not required for ownership.
- Candidate confirmation accepts angularly coherent normals and a normalized representative direction.
- The pipeline remains allocation-free and runs once per fixed tick.
- The finding remains architectural P2 until evidence supports P1.

Resolved implementation decisions:

- Gameplay airtime uses Stable Locomotion Support transitions. A brief Observed Support miss awards no airtime; `HardReset` clears tracking without reward.
- Character Presentation consumes Observed Support through its existing presentation tracker without changing locomotion support.
- Initial authoring stays with the existing scene installer and `RunBodyMovementConfig`; a dedicated `RunSurfaceTuning` asset is deferred.
- Existing serialized thresholds are preserved for the first migration. Candidate coherence and airborne retention use explicit defaults until human feel approval.
- A confirmed discontinuity snaps steering exactly once; ordinary Stable Support changes use the configured slew.
- The canonical PlayMode fixtures cover continuous ground, fast seams, troughs, brief gaps, sustained loss, jump, landing, and unavailable hard reset at 0.01-second and 0.02-second fixed steps.
- The older Run Steering Frame Stability PRD is retained as historical rationale and marked superseded where it assumes a raw source and steering-local temporal policy.

See [implementation decisions](../tasks/run-surface-stability/implementation-decisions.md), [baseline and verification](../tasks/run-surface-stability/baseline-and-verification.md), and the pending [feel review](../tasks/run-surface-stability/feel-review.md).
