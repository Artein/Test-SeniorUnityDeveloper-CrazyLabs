# PRD: Run Body Explicit Speed Ownership

Status: Implemented and approved

Related documents:

- [ADR-0010: Use Explicit Run Body Speed Model With Rigidbody Contact Physics](../adr/adr-0010-use-explicit-run-body-speed-model-with-rigidbody-contact-physics.md)
- [Run Body Movement And Speed Model Diagram](../diagrams/run-body-speed-model.md)
- Supersedes [PRD: Run Body Natural Speed Ownership](./prd-run-body-natural-speed-ownership.md)

## Problem Statement

The game depends on the player understanding and trusting how the **Run Body** gains, loses, and carries speed after **Launch**. The current movement flow mainly sanitizes and rotates existing Rigidbody velocity. Speed magnitude therefore emerges from **Launch Impulse**, gravity, course geometry, physics materials, contacts, collisions, and solver behavior.

That emergent behavior preserves physical interactions, but it does not provide one gameplay authority that can answer: how strongly should a downhill section accelerate, how quickly should an ordinary surface slow the body, how should a recoverable near-stall behave, and what should an upgraded `PlayerMaxSpeed` mean?

As a result:

- Designers cannot tune core sliding outcomes without changing physics details that affect other behavior.
- `PlayerMaxSpeed` is registered and upgradeable but lacks a reliable player-facing movement effect.
- Small collider, material, timestep, or course-shape changes can alter speed feel unexpectedly.
- Near-stall recovery and **Lost Momentum** can compete without an explicit boundary.
- High-speed obstacle readability and collision reliability have no defined gameplay envelope.
- Tests cannot state expected supported-surface speed behavior independently from the Unity solver.

The problem is missing gameplay speed ownership, not the use of Rigidbody physics.

## Solution

Introduce an explicit **Run Body Speed Model** for intentional supported surface-tangent speed during **Running**.

The model will evaluate current movement, valid **Run Surface** facts, course-forward alignment, designer tuning, and the resolved `PlayerMaxSpeed` stat. It will produce acceleration, slowdown, bounded low-speed assistance, and a soft speed envelope for one fixed-step movement orchestrator to integrate.

Unity Rigidbody physics will continue to own gravity, contacts, collisions, separation, and externally produced surface-normal velocity. **Run Steering Control** will remain direction-only. Airborne speed will remain driven by Unity physics and **Run Air Steering Control**.

The player should experience:

- Downhill speed gain that follows the course and current forward travel.
- Predictable ordinary slowdown that is tunable without hidden steering behavior.
- Limited help through recoverable low-speed incidents without continuous pushing against blocked geometry.
- A visible `PlayerMaxSpeed` upgrade effect through a soft envelope rather than a hard wall.
- Preserved launch and collision overspeed that settles toward the envelope instead of being immediately clamped.
- No automatic heading selection, full-stop restart, or replacement of physical obstacle interaction.

## Unity Surfaces

Runtime assemblies and asmdefs:

- The existing Gameplay runtime assembly remains the owner of movement orchestration, speed policy, steering policy, run-surface facts, stat resolution, and VContainer composition.
- No new runtime asmdef is required for the first slice.
- Gameplay policy remains plain C# in accordance with ADR-0002.
- Runtime dependencies remain composed through VContainer in accordance with ADR-0005.

Runtime modules and services:

- A fixed-step **Run Body Movement Controller** becomes the single movement orchestration entry point.
- A pure **Run Body Speed Evaluator** maps normalized runtime facts and raw tuning to a speed decision.
- A pure steering evaluator contributes direction intent without writing Rigidbody state.
- A thin **Run Body Movement Target** applies one final target state to the Rigidbody.
- Existing velocity sanitation and launch-landing stabilization remain separate collaborators.
- Existing gameplay stat resolution supplies the active-run `PlayerMaxSpeed` value.
- A pure movement-config validator supports Editor and pre-use validation.

Scenes, prefabs, ScriptableObjects, and assets:

- One designer-facing **Run Body Movement Tuning** ScriptableObject groups related speed, movement-validity, launch-landing, steering, and steering-frame settings.
- The asset exposes narrow read-only runtime config interfaces for each consumer.
- Existing broad steering configuration may remain only as a temporary migration adapter.
- The gameplay composition root serializes and registers the movement config asset through all implemented narrow interfaces.
- Existing Rigidbody, colliders, **Run Surface** authoring, and physics materials remain in use.
- Physics materials continue to affect contact solving, but they are no longer the only gameplay speed-tuning mechanism.
- No first-slice **Run Surface Speed Profile** assets are required.

Editor surfaces:

- Inspector `Min` or meaningful `Range` attributes provide immediate tuning guardrails.
- The ScriptableObject's `OnValidate` path reports complete validation errors through the shared pure validator.
- No custom inspector, editor window, importer, or menu item is required.
- Authored values are validated again before movement starts so invalid authoring cannot reach fixed-step production logic.

Project and package surfaces:

- No Unity version, package manifest, Addressables schema, or ProjectSettings change is required.
- No save-format migration is expected.
- Existing Unity connector compile and test workflows remain the verification path.

## User Stories

1. As a player, I want downhill traversal to increase speed predictably, so that the course shape has an understandable effect.
2. As a player, I want flat or resistant traversal to slow me predictably, so that speed loss feels fair.
3. As a player, I want a stronger **Launch** to retain a meaningful advantage, so that pull strength still matters.
4. As a player, I want steering to change direction without secretly braking or boosting me, so that control remains trustworthy.
5. As a player, I want airborne motion to preserve launch and gravity behavior, so that the speed model does not create hidden air rules.
6. As a player, I want landing correction to preserve tangent momentum, so that touchdown does not create an invisible speed wall.
7. As a player, I want recoverable surface seams or soft scrapes to receive limited assistance, so that minor physics incidents do not end an otherwise valid run.
8. As a player, I want blocking geometry to remain capable of stopping me, so that recovery does not push forever through obstacles.
9. As a player, I want **Lost Momentum** to remain meaningful, so that genuinely stalled runs still end.
10. As a player, I want `PlayerMaxSpeed` upgrades to let me sustain a higher useful speed, so that progression is visible during **Running**.
11. As a player, I want upgraded speed to settle naturally rather than hit a hard cap, so that the game does not feel artificially constrained.
12. As a player, I want launch, gravity, or collision overspeed to survive initially, so that physical events retain impact.
13. As a player, I want excessive speed to encounter increasing resistance, so that obstacle readability remains manageable.
14. As a player, I want lateral or reversed travel not to receive forward downhill assistance, so that the model does not reward the wrong direction.
15. As a player, I want the speed model not to steer me automatically, so that heading remains under momentum and steering control.
16. As a player, I want a fully stopped body not to invent a travel direction, so that recovery does not become hidden auto-navigation.
17. As a designer, I want one movement-tuning asset, so that related settings are easy to find.
18. As a designer, I want speed, steering, validity, and landing groups to remain visibly distinct, so that one asset does not erase ownership.
19. As a designer, I want downhill acceleration as a readable control, so that I can tune slope reward directly.
20. As a designer, I want ordinary surface slowdown as a readable control, so that I can tune baseline speed loss directly.
21. As a designer, I want low-speed assist target and acceleration as separate controls, so that recovery destination and recovery rate are independent.
22. As a designer, I want a base soft maximum and above-maximum resistance as separate controls, so that comfort speed and overspeed settling are independent.
23. As a designer, I want Inspector tooltips to explain behavior rather than expose squared-unit names, so that tuning language remains accessible.
24. As a designer, I want invalid negative or non-finite values reported before Play, so that broken tuning fails early.
25. As a designer, I want only real upper bounds represented with `Range`, so that arbitrary limits do not block experimentation.
26. As a designer, I want raw authored values preserved by the config asset, so that hidden config resolution does not obscure what I authored.
27. As a designer, I want `PlayerMaxSpeed` progression to modify the soft envelope, so that upgrade data has one clear movement meaning.
28. As a designer, I want the sanity guard far above normal tuning, so that it cannot become a balance control.
29. As a designer, I want future ice, snow, or terrain paint to select explicit surface profiles only when needed, so that visual materials do not silently become gameplay policy.
30. As a designer, I want the first slice to use global/default speed tuning, so that surface-profile complexity does not block explicit ownership.
31. As a gameplay engineer, I want one fixed-step movement writer, so that speed and steering cannot overwrite each other.
32. As a gameplay engineer, I want a pure speed evaluator, so that policy can be tested without Rigidbody or scene lifecycle.
33. As a gameplay engineer, I want the evaluator to return rates rather than final velocity, so that fixed-step integration has one owner.
34. As a gameplay engineer, I want speed output separate from steering output, so that magnitude and heading responsibilities remain explicit.
35. As a gameplay engineer, I want final surface projection and facing resolved after policy evaluation, so that final movement uses one physical support plane.
36. As a gameplay engineer, I want corrected surface-normal velocity preserved, so that speed policy does not own contact separation or gravity.
37. As a gameplay engineer, I want speed effects gated by valid grounded **Run Surface** support, so that stale or departing contact does not act grounded.
38. As a gameplay engineer, I want unsupported speed decisions to be neutral, so that airborne motion stays physically driven.
39. As a gameplay engineer, I want course-forward alignment as a normalized scalar, so that positive effects can scale without giving the speed evaluator a heading.
40. As a gameplay engineer, I want slope acceleration based on forward-downhill angle, so that flat and uphill travel add no downhill speed.
41. As a gameplay engineer, I want overspeed resistance based on normalized excess, so that tuning behaves consistently across envelope values.
42. As a gameplay engineer, I want model-added acceleration prevented from crossing the soft envelope, so that ordinary gameplay policy respects its target.
43. As a gameplay engineer, I want external overspeed resisted instead of clamped, so that launch and collision events remain visible.
44. As a gameplay engineer, I want low-speed assist bounded by its initial deficit, so that solver loss cannot replenish endless acceleration.
45. As a gameplay engineer, I want requested assist delta to spend the attempt budget before physics resolution, so that contact loss cannot create free retries.
46. As a gameplay engineer, I want assist to require an existing usable tangent direction, so that it cannot restart exact zero.
47. As a gameplay engineer, I want support loss to pause rather than replenish an attempt, so that airborne transitions remain neutral.
48. As a gameplay engineer, I want an attempt rearmed only after independent recovery above target or a new run, so that one slowdown episode receives one bounded attempt.
49. As a gameplay engineer, I want no speculative obstruction raycast in the first slice, so that recovery does not claim sensing the project does not expose.
50. As a gameplay engineer, I want no dependency on **Lost Momentum** internals, so that movement assistance and failure observation remain separate.
51. As a gameplay engineer, I want `PlayerMaxSpeed` resolved from the immutable active-run snapshot, so that upgrade changes do not leak into an active run.
52. As a gameplay engineer, I want no second scalar stat cache, so that snapshot and cache ownership remain centralized.
53. As a gameplay engineer, I want narrow config interfaces registered from one asset, so that services are mockable without duplicating designer settings.
54. As a gameplay engineer, I want malformed authored values rejected before fixed-step evaluation, so that production code can rely on valid invariants.
55. As a gameplay engineer, I want impossible Rigidbody velocity handled defensively, so that external physics corruption remains contained.
56. As a QA engineer, I want deterministic slope and slowdown tests, so that tuning formulas cannot regress silently.
57. As a QA engineer, I want neutral airborne and unsupported tests, so that grounded policy cannot leak into flight.
58. As a QA engineer, I want soft-envelope and collision-overspeed tests, so that hard clamps cannot return unnoticed.
59. As a QA engineer, I want bounded recovery tests against repeated solver loss, so that assistance cannot become continuous propulsion.
60. As a QA engineer, I want validation tests for invalid config and resolved stats, so that bad authoring fails before gameplay.
61. As a QA engineer, I want integration tests for launch, landing, steering, and one final movement write, so that module composition matches policy.
62. As a QA engineer, I want high-speed obstacle approach coverage, so that upgraded speed remains playable and collision behavior stays reliable.
63. As a maintainer, I want the ADR to own the durable physics boundary, so that later tuning changes do not reopen the architecture question.
64. As a maintainer, I want the PRD to own product behavior and first-slice decisions, so that the ADR stays concise.
65. As a maintainer, I want the Mermaid artifact to show Player, Designer, Engineer, and runtime responsibilities, so that the system can be understood quickly.
66. As a maintainer, I want the natural-speed PRD preserved but marked superseded, so that historical steering fixes remain discoverable without remaining authoritative.
67. As a maintainer, I want future surface-profile work treated as a separate extension, so that material selection does not expand the first slice.
68. As a maintainer, I want no package or Unity-version change, so that this remains a contained gameplay feature.

## Implementation Decisions

- **Run Body Speed Model** is the canonical domain term for intentional player-facing speed ownership during **Running**.
- The architecture is hybrid: gameplay owns intentional valid-surface tangent-speed outcomes; Unity owns gravity, contacts, collisions, separation, and external normal velocity.
- **Run Body Movement Controller** is the canonical fixed-step orchestrator name. Avoid `RunBodyMotor`, which can imply a Rigidbody replacement.
- The controller owns one final movement target-state write per active fixed pass. The legacy steering controller and replacement controller must never both write movement.
- The first slice changes scalar tangent speed only. It does not choose a preferred heading or create movement from exact zero.
- **Run Steering Control** remains direction-only. Its evaluator returns steering intent; the movement controller projects that intent onto the accepted physical support plane.
- The accepted **Run Surface** normal defines tangent/normal decomposition. Stabilized steering up defines the control frame but does not replace the physical plane.
- Final facing is resolved by movement orchestration after support projection; it is not a pre-projection steering-evaluator output.
- Corrected surface-normal velocity is preserved when tangent speed changes.
- First-slice speed effects require valid grounded **Run Surface** support and no meaningful movement away from the support normal.
- Invalid or absent support produces no model acceleration, slowdown, recovery, or envelope resistance. Airborne motion remains Unity physics plus **Run Air Steering Control**.
- The pure runtime policy interface is named `IRunBodySpeedEvaluator`, with `DefaultRunBodySpeedEvaluator` as the first implementation.
- The evaluator consumes current velocity, valid-support state, support normal, forward-downhill angle, course-forward alignment, and resolved soft maximum.
- The evaluator returns `RunBodySpeedDecision`: tangent acceleration, tangent drag, low-speed assist target, low-speed assist acceleration, soft maximum, and diagnostic contributors.
- Diagnostic contributors are a flags-style list of active effects. They explain contribution, not one exclusive causal reason.
- Decision acceleration and drag are rates. The movement controller integrates them using fixed-step elapsed time.
- Positive effects use the clamped positive portion of **Course Forward Alignment**. Slowdown and overspeed resistance are direction-independent.
- Downhill strength is the sine of positive forward-downhill angle. Flat and uphill traversal add no downhill acceleration.
- Overspeed fraction is normalized excess above the soft maximum and reaches full authored resistance at twice the envelope.
- Ordinary acceleration and drag integrate before low-speed assistance.
- Model-authored positive speed cannot cross the soft envelope. Externally produced overspeed is not immediately clamped and settles through resistance.
- **Run Body Low-Speed Assist** uses separate target-speed and acceleration values.
- Effective assist target is the lower of the raw assist target and resolved soft maximum. This resolves two valid values and is not invalid-config repair.
- One low-speed episode receives one bounded assist attempt. Its total requested velocity budget equals the initial speed deficit to the effective target.
- Requested assist delta spends the remaining budget before Rigidbody resolution. Solver cancellation cannot replenish the attempt.
- Assist starts only with valid support, speed below target, a usable existing tangent heading, and positive course-forward alignment.
- Exact zero does not synthesize heading or spend the attempt. Losing support pauses without replenishment.
- Assist rearms after sampled tangent speed independently rises above the target by a small numeric tolerance, or when a new run begins.
- The first slice adds no obstruction raycast, persistent blocking-contact tracker, or dependency on **Lost Momentum**.
- `PlayerMaxSpeed` resolves through the existing active-run gameplay stat resolver and defines the soft envelope.
- The movement controller resolves `PlayerMaxSpeed` while building each active fixed-pass context. It relies on the immutable run snapshot and existing resolver cache; it adds no second scalar cache.
- One `RunBodyMovementConfig` ScriptableObject groups related movement tuning and implements narrow raw-value interfaces for speed, validity, launch landing, steering, and steering-frame consumers.
- Runtime services depend on those narrow interfaces, not the concrete asset or the broad legacy steering config.
- The config asset stores raw values. Cross-field resolution belongs to gameplay logic.
- Speed tuning includes downhill acceleration, surface slowdown, low-speed assist target, low-speed assist acceleration, base soft maximum, and above-maximum resistance.
- Inspector labels and tooltips communicate behavior and units in designer language. Property names avoid `Squared` suffixes.
- `Min` attributes cover basic non-negative or positive bounds. `Range` is used only for genuine design limits.
- A pure movement-config validator checks finite values and cross-field invariants. `OnValidate` reports all errors in the Editor.
- Config is validated again at composition or movement initialization. Invalid resolved `PlayerMaxSpeed` is rejected at active-run preparation.
- Fixed-step policy does not silently clamp, default, or neutralize malformed authored config or stats.
- The existing **Run Body Speed Sanity Guard** remains defensive for non-finite or impossible Rigidbody velocity and stays unreachable by normal launch, upgrades, and course traversal.
- The first slice uses global/default speed tuning. **Run Surface Speed Profiles** and terrain/material profile selection are deferred.
- Steering input remains lifecycle-aware and coherent per fixed pass. **Running** plus **Launch Applied** enables steering for the current run.
- Launch-up orientation remains a movement-side fact used to reset the steering frame. Launch-landing stabilization remains a separate movement-driven collaborator.
- Global movement tuning and evaluator registration belong in the gameplay lifetime scope. Scene-authored surface probes remain in scene physics composition.
- Registration should bind the single config asset through all narrow interfaces and register evaluators/controllers through VContainer.
- The standalone Mermaid artifact is the high-level engineering overview and must be referenced instead of duplicated.

## Testing Decisions

Good tests assert externally visible policy and movement-boundary outcomes. They should not lock down private fields, incidental call order beyond the single-write invariant, Unity lifecycle details in pure EditMode tests, or exact defaults that remain designer tuning.

EditMode tests should cover the pure speed evaluator:

- Neutral output when valid grounded support is absent.
- Flat and uphill travel add no downhill acceleration.
- Positive downhill angles produce monotonic sine-scaled acceleration.
- Positive effects scale proportionally with course-forward alignment.
- Lateral, reversed, and directionless motion receive no positive effects.
- Surface slowdown remains direction-independent.
- Soft-envelope resistance begins above the envelope and reaches full authored resistance at twice the envelope.
- Model-authored acceleration cannot cross the envelope.
- External overspeed is resisted without immediate clamping.
- Diagnostic contributors identify active effects without changing output behavior.

EditMode tests should cover movement composition:

- Sanitation precedes landing stabilization and policy evaluation.
- Tangent speed changes preserve corrected surface-normal velocity.
- Steering intent is projected onto the accepted support plane.
- Invalid projected intent falls back only to usable existing tangent direction.
- No usable direction means no speed-created heading.
- Final facing derives from final projected tangent direction.
- Exactly one final target-state write occurs per active fixed pass.
- Inactive lifecycle state writes no movement target.
- Airborne motion receives neutral speed policy while air steering remains direction-only.

EditMode tests should cover low-speed assistance:

- Eligible near-stall movement approaches the effective target at the authored rate.
- Assist never exceeds the lower of target and soft envelope.
- Total requested assistance cannot exceed the initial deficit.
- Solver-cancelled requested delta still spends the budget.
- Support loss pauses without replenishment.
- Exact zero does not invent heading or spend the attempt.
- A new run rearms assistance.
- Independent recovery above target rearms after numeric tolerance.
- One blocked low-speed episode cannot receive continuous propulsion.
- **Lost Momentum** remains able to end a genuine stall.

EditMode tests should cover config and stats:

- Inspector-compatible bounds and the pure validator agree on basic invariants.
- Non-finite values are rejected.
- Required positive values reject zero.
- Non-negative rates reject negative values.
- Valid raw assist target above base soft maximum is accepted and resolved by policy.
- Invalid config fails at startup rather than being repaired in fixed-step evaluation.
- Neutral `PlayerMaxSpeed` preserves the base soft envelope.
- Upgraded `PlayerMaxSpeed` changes the envelope in the expected direction.
- Non-finite, non-positive, or sanity-range resolved maximum prevents run movement.
- Repeated stat resolution on an unchanged active snapshot remains allocation-free.

PlayMode tests should cover engine and scene integration:

- Gameplay composition resolves one movement config through all narrow interfaces.
- Only the replacement movement controller owns active fixed-step Rigidbody writes.
- Launch applies initial velocity without speed-model clamping.
- Unsupported launch flight preserves Rigidbody speed behavior.
- First valid landing preserves tangent speed while landing stabilization corrects lift.
- Valid grounded traversal applies slope acceleration and slowdown.
- Collision-produced overspeed is not immediately clamped.
- High-speed obstacle approach still produces reliable collision/run-end behavior.
- Leaving **Running** clears movement, input, stabilization, and recovery-attempt state.
- A new **Launch** resets the expected movement lifecycle.

Static and manual verification:

- Run Rider formatting and problem inspection for changed C# files.
- Compile through Unity AI Agent Connector before tests.
- Run targeted EditMode tests before PlayMode tests.
- Run scene composition and focused movement PlayMode tests after compilation is clean.
- Run `git diff --check`.
- Validate ADR metadata and regenerate the ADR index.
- Manually compare weak, neutral, and upgraded runs on downhill, flat, seam, scrape, and high-speed obstacle cases.
- Use diagnostics to inspect tangent speed, slope, support validity, envelope, contributors, and remaining assist budget.

## Release and Compatibility

- The implementation targets Unity `6000.3.18f1` and the repository's current Rigidbody-backed gameplay architecture.
- No Unity upgrade, package dependency, package-manifest, Addressables, ProjectSettings, or save-format change is required.
- This is a project-level gameplay change rather than a distributable UPM package release. It requires no package version bump, package changelog entry, installation step, or managed-asset sync behavior.
- Serialized movement tuning requires an intentional migration from the broad steering config to the unified movement config. Existing steering, landing-stabilization, and steering-frame authored values must survive the migration, and the Gameplay scene must reference the replacement config before the legacy asset or adapter is removed.
- Public/internal gameplay interfaces and test fakes may change as the legacy steering controller is split into movement orchestration, speed policy, steering policy, and target adapter roles.
- Existing scene composition must remove the legacy controller as an active movement writer when the replacement is registered.
- Existing **Launch Impulse**, airborne steering, landing stabilization, run-end, camera, presentation, economy, and reward behavior remain compatible unless explicitly listed in implementation issues.
- `PlayerMaxSpeed` keeps its existing product name and upgrade pipeline but changes from ambiguous/unused speed limiting to the soft speed-envelope effect.
- The old natural-speed PRD remains as historical context and is marked superseded.
- ADR-0010 is approved because the branch merges the feature and architectural record atomically.

## Out of Scope

- Replacing Rigidbody with `CharacterController`, kinematic movement, DOTS physics, or a custom collision solver.
- Owning gravity, penetration resolution, contact separation, or arbitrary collision impulses.
- Adding automatic course-forward guidance, steepest-downhill steering, or a full-stop restart mechanic.
- Adding speed-model effects while airborne.
- Adding downforce or magnet-to-surface behavior.
- Adding per-surface profiles, terrain-layer mapping, material inference, volumes, or course-section speed overrides.
- Adding obstruction raycasts or persistent blocking-contact tracking for low-speed assistance.
- Redesigning **Lost Momentum**, run-end priorities, obstacles, finish behavior, safety nets, rewards, economy, pickups, UI, camera, animation, VFX, audio, or haptics.
- Retuning final production values for launch, speed, surface friction, obstacles, or upgrade curves beyond what implementation smoke testing requires.
- Adding a custom inspector, Editor window, importer, build-validation framework, package dependency, or Unity upgrade.
- Changing save data or upgrade ownership semantics during an active run.

## Further Notes

- **Run Body Speed Model** means intentional speed-magnitude authority, not "our own physics."
- **Run Steering Control** remains direction-only even though movement orchestration composes its result with speed.
- **Run Body Speed Envelope** is player-facing tuning; **Run Body Speed Sanity Guard** is defensive containment.
- Expected support absence or directionless motion is valid runtime observation, not malformed authoring.
- Future terrain materials such as ice or snow may select **Run Surface Speed Profiles**, but material identity should not become the profile itself.
- The Mermaid artifact is intentionally separate so designers, engineers, and future PRDs can reference the same actor-aware overview.

### Assumptions

- The active-run modifier snapshot remains immutable for the duration of a run, so repeated `PlayerMaxSpeed` resolution is stable and cacheable.
- The Gameplay scene continues to use one gameplay lifetime scope and its existing scene-composition installers.
- Rigidbody fixed-step contact solving remains the source of gravity, collision impulses, and surface-normal response.
- Initial numerical speed-tuning values will be selected during implementation smoke testing and refined through playtesting; they are not architectural constants.

### Unresolved Questions

- None for the first implementation slice.
