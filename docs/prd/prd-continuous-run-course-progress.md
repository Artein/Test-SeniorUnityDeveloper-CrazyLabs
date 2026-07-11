## Problem Statement

The current **Run Progress Frame** correctly measures the authored Ladybug Rooftop Half-Tube because that course has one straight longitudinal +Z axis. Its progress is the dot product between Run Body displacement and one launch-time forward direction.

That model stops representing player intent when a future **Run Course** bends horizontally, changes longitudinal elevation, approaches vertical, overlaps itself, or introduces alternate routes. A fixed axis can report little or negative progress while the player is moving correctly around the course. It can also make **Lost Momentum**, **Run Result** distance, distance rewards, and character presentation disagree about the same motion.

A detector-only correction would leave several progress consumers using different coordinate systems. A direct replacement of the current frame with a spline frame would create another problem: the existing frame up direction is also used by support probing. An authored spline normal must not silently become gravity-up or redirect physics casts.

The project needs one authoritative, continuous, authored course-coordinate model that preserves current straight-course behavior, supplies local longitudinal motion semantics, handles projection ambiguity deterministically, and remains independent from the physics support/gravity frame.

## Solution

Introduce a project-owned **Run Course Coordinates** model behind **RunProgressService**.

A shallow authored source creates an immutable per-run course snapshot. The snapshot projects each Run Body world position into one **Run Course Location** containing scalar course progress, physical arc-length information, the local longitudinal tangent, optional authored lane orientation, projection quality, and an opaque continuity token.

**RunProgressService** remains the single runtime owner. It creates the snapshot at **Launch Applied**, projects the launch origin, samples the Run Body once per fixed tick using the previous continuity token, and publishes one immutable **Run Progress Sample**. Current progress may decrease during rollback; maximum reached progress remains monotonic and continues to drive **Run Result** distance and distance-based rewards.

The initial migration uses a straight-course adapter that is behaviorally identical to the existing **Run Progress Frame**. Curved and vertical course authoring can later use a spline adapter without exposing spline-package types to gameplay code.

**Lost Momentum** uses signed longitudinal speed—the dot product of linear velocity and the current course tangent—together with maximum-progress gain and the existing sustained-duration policy. Lateral motion no longer keeps a stalled run alive, and backward motion is explicitly negative.

Course coordinates and physics support remain separate. Course tangent may inform longitudinal slope and presentation, while support probe direction and gravity-up come from an explicit support reference-frame contract.

The first implementation slice supports one continuous open route. Projection tokens and contracts remain topology-agnostic, but branching, rejoining, laps, and canonical branch progress require a separate authored-topology decision before implementation.

## Unity Surfaces

Runtime assemblies and asmdefs:

- **Game.Gameplay** gains the project-owned course-coordinate contracts, immutable samples, straight-course adapter, continuity-aware projection policy, and migrated **RunProgressService** behavior.
- **Game.Gameplay** retains ownership of **Lost Momentum**, **Run End Flow**, character presentation integration, and support-context integration.
- The core gameplay assembly must not reference Unity Splines.
- If spline authoring is approved, a dedicated runtime adapter assembly references both the gameplay contract and Unity Splines. This keeps the optional package dependency at the infrastructure edge.
- The existing Ladybug course runtime assembly continues to depend on gameplay contracts and owns course-specific validation or source assignment, not generic progress algorithms.
- Existing EditMode and PlayMode test assemblies gain course-coordinate, migration-parity, composition, and integration coverage.
- No Jobs or Burst implementation is required for the first slice.

Editor assemblies, windows, inspectors, importers, or menu items:

- No custom Editor window, importer, or menu item is required.
- Existing Unity inspectors and spline authoring tools are sufficient if the spline adapter is selected.
- Shallow authoring validation and optional gizmos may show route direction, start/end, projection corridor, and invalid geometry.
- Validation must not contain gameplay progress policy.
- No automatic scene rewriting tool is required for the first migration.

Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:

- The current Gameplay Scene keeps an explicitly assigned course source.
- The current straight **Run Progress Frame Source** is retained or migrated without losing its serialized scene reference.
- A future curved-course scene assigns one authored continuous course source and validates it before entering Play Mode.
- **RunEndConfig** changes the Lost Momentum speed concept from unsigned planar speed to signed forward speed. Existing serialized tuning values must be migrated rather than reset.
- The current Ladybug course geometry, contacts, finish, rewards, and +Z behavior remain unchanged.
- No prefab ownership change is required unless a future course source is packaged as a reusable prefab.
- No ProjectSettings change is required.
- If gameplay directly adopts Unity Splines, it becomes an explicit manifest dependency rather than an accidental transitive dependency.

RPC/helper commands, hooks, or shell wrappers:

- Unity AI Agent Connector remains the compile and test verification path.
- No new RPC method, helper command, hook, or shell wrapper is required.
- Existing asset refresh and scene-load behavior is sufficient for serialized authoring verification.

Package versioning, changelog, and installation/sync behavior:

- This is project gameplay work, not a distributable package release.
- Unity Splines 2.8.4 is currently resolved transitively through Cinemachine. Direct gameplay use must promote it to an explicit compatible project dependency.
- The first straight-adapter migration requires no new package.
- No save-data migration or installation/sync workflow is required.
- Repository release notes or README architecture notes should change only when curved-course support becomes an actual shipped capability.

## User Stories

1. As a player, I want distance to increase as I follow a curved course, so that the result reflects how far I progressed rather than world-axis displacement.
2. As a player, I want climbing, dropping, and near-vertical course sections to count as forward progress, so that valid traversal is never mistaken for stopping.
3. As a player, I want sideways sliding to contribute little or no forward speed, so that **Lost Momentum** matches actual advancement.
4. As a player, I want rolling backward to be recognized as negative forward motion, so that the game can end a run that is no longer advancing.
5. As a player, I want short impacts and reversals to remain recoverable, so that transient physics reactions do not immediately end the run.
6. As a player, I want the furthest point reached to survive a rollback, so that bouncing backward does not erase my result.
7. As a player, I want distance rewards to use the same progress value shown in the **Run Result**, so that reward calculation feels consistent.
8. As a player, I want finish behavior to remain authoritative, so that crossing **Run Finish** still outranks a simultaneous Lost Momentum candidate.
9. As a player, I want the existing Ladybug course to feel identical after migration, so that architectural work does not alter current tuning.
10. As a player, I want movement near a hairpin or overlapping course section to remain stable, so that progress does not teleport to another section.
11. As a player, I want respawn or intentional teleport behavior to reacquire the course predictably, so that progress recovers only when the game explicitly permits it.
12. As a player, I want character animation to react to local course motion, so that presentation remains believable through bends and elevation changes.
13. As a player, I want support and grounding to remain physically correct on curved authoring, so that spline orientation does not redirect collision probes.
14. As a player, I want alternate routes to produce fair comparable results if branches are introduced, so that route choice does not accidentally exploit distance rewards.
15. As a player, I want a stopped run to end within the established grace and duration policy, so that retries remain responsive.
16. As a course designer, I want progress to come from an authored route, so that camera direction, launch aim, and Rigidbody wobble do not define course advancement.
17. As a course designer, I want straight routes to use a simple adapter, so that simple levels do not require spline authoring.
18. As a course designer, I want curved routes to expose start, end, and travel direction clearly, so that reversed authoring is easy to detect.
19. As a course designer, I want invalid or degenerate route geometry to fail loudly, so that broken progress cannot silently ship.
20. As a course designer, I want zero-length segments and invalid tangents rejected, so that local motion calculations remain finite.
21. As a course designer, I want the route snapshot to remain stable throughout a run, so that accidental Transform edits cannot change progress mid-run.
22. As a course designer, I want optional lane orientation separate from gravity-up, so that banked visuals do not redefine support physics.
23. As a course designer, I want projection-distance diagnostics, so that a Run Body far from the authored route can be identified during playtesting.
24. As a course designer, I want hairpins and near-overlaps validated, so that ambiguous projection areas are deliberate rather than accidental.
25. As a course designer, I want explicit teleport or reacquisition policy, so that large discontinuities do not masquerade as ordinary movement.
26. As a course designer, I want Lost Momentum forward-speed and progress thresholds in one config, so that curved-course tuning remains discoverable.
27. As a course designer, I want existing config values preserved during migration, so that updating the architecture does not reset tuned assets.
28. As a course designer, I want no custom editor dependency for the first slice, so that course authoring stays lightweight.
29. As a course designer, I want future branch junctions to declare topology explicitly, so that nearest geometry does not choose the active route.
30. As a course designer, I want actual traveled distance and canonical course progress treated as different concepts, so that branch balancing remains intentional.
31. As a gameplay developer, I want **RunProgressService** to remain the single progress authority, so that consumers cannot drift into conflicting calculations.
32. As a gameplay developer, I want one immutable **Run Progress Sample** per fixed tick, so that all consumers observe the same course location and maximum progress.
33. As a gameplay developer, I want course projection hidden behind a small project-owned interface, so that gameplay code is independent from spline vendors.
34. As a gameplay developer, I want the course snapshot to be a deep module, so that projection, arc-length lookup, continuity, and validation are encapsulated behind a stable API.
35. As a gameplay developer, I want the launch location projected once, so that run progress is relative to the actual **Launch Target** position.
36. As a gameplay developer, I want current progress allowed to decrease, so that rollback is represented truthfully.
37. As a gameplay developer, I want maximum progress monotonic, so that **Run Result** distance retains existing furthest-reached semantics.
38. As a gameplay developer, I want the final Run Body position sampled through **RunProgressService**, so that **Run End Flow** does not duplicate projection.
39. As a gameplay developer, I want signed longitudinal speed derived from the current tangent, so that forward, lateral, and reverse motion are distinguishable.
40. As a gameplay developer, I want Lost Momentum to require both insufficient forward speed and insufficient maximum-progress gain, so that either meaningful motion signal can keep a run alive.
41. As a gameplay developer, I want Lost Momentum duration and launch grace preserved, so that the detector remains resilient to transient movement.
42. As a gameplay developer, I want a reverse-recovery exception to be explicit if product design requests it, so that unsigned speed does not encode hidden policy.
43. As a gameplay developer, I want projection to use the previous location token, so that self-near routes do not jump between distant segments.
44. As a gameplay developer, I want projection search bounds based on plausible Run Body displacement, so that continuity enforcement remains deterministic.
45. As a gameplay developer, I want global reacquisition available only through an explicit run event or failure policy, so that ordinary sampling cannot teleport progress.
46. As a gameplay developer, I want the fixed-tick path to allocate no managed memory, so that mobile gameplay avoids recurring garbage collection pressure.
47. As a gameplay developer, I want course arc-length data prepared outside the hot path, so that sampling cost stays bounded.
48. As a gameplay developer, I want non-finite positions and velocities handled consistently, so that invalid physics input cannot corrupt progress state.
49. As a gameplay developer, I want failed projections to retain a clear validity/error state, so that consumers can fail safely rather than invent progress.
50. As a gameplay developer, I want spline-specific code isolated in its own adapter assembly, so that the gameplay core does not take a transitive package dependency.
51. As a gameplay developer, I want **Character Presenter** to consume explicit course and support motion fields, so that presentation does not infer physics semantics from one ambiguous frame.
52. As a gameplay developer, I want support probing to consume an explicit support/gravity frame, so that authored spline normals cannot become raycast directions.
53. As a gameplay developer, I want longitudinal slope calculation to use the course tangent where needed, so that “forward downhill” remains local to a curved route.
54. As a gameplay developer, I want the straight adapter to prove exact parity before spline integration, so that migration risk is separated from new authoring risk.
55. As a gameplay developer, I want branch topology hidden behind the same location token contract, so that future graph support does not force consumer rewrites.
56. As a gameplay developer, I want canonical branch progress decided before graph implementation, so that Run Result and reward semantics remain coherent.
57. As a gameplay developer, I want no static mutable course state, so that run state remains scoped and testable through VContainer.
58. As a gameplay developer, I want shallow MonoBehaviour adapters and plain C# policies, so that Unity lifecycle does not own progress rules.
59. As a gameplay developer, I want existing direct events and fixed-tick lifecycles reused, so that no global event bus is introduced.
60. As a gameplay developer, I want existing **Run Result** terminology preserved, so that UI and reward consumers do not require speculative renaming.
61. As a tester, I want a straight-route parity test, so that the new model matches the current dot-product result.
62. As a tester, I want quarter-circle arc-length tests, so that equal route distances produce equal progress around a bend.
63. As a tester, I want vertical-tangent tests, so that vertical longitudinal motion produces positive progress and speed.
64. As a tester, I want lateral-velocity tests, so that sideways motion produces approximately zero signed longitudinal speed.
65. As a tester, I want reverse-velocity tests, so that backward motion produces negative longitudinal speed.
66. As a tester, I want rollback tests, so that current progress decreases while maximum progress remains unchanged.
67. As a tester, I want hairpin and overlap tests, so that continuity tokens prevent projection jumps.
68. As a tester, I want explicit teleport tests, so that global reacquisition occurs only under its approved policy.
69. As a tester, I want invalid-authoring tests, so that zero-length, non-finite, and degenerate orientation data fails loudly.
70. As a tester, I want Lost Momentum tests using signed tangent speed, so that stopping, lateral sliding, reverse travel, and meaningful forward progress are distinguished.
71. As a tester, I want final-result tests, so that **Run End Flow** uses the most recent maximum progress without reimplementing path math.
72. As a tester, I want presentation tests, so that local tangent speed and support-relative vertical behavior remain independently testable.
73. As a tester, I want support-probe tests, so that changing authored course orientation cannot redirect gravity or support casts.
74. As a tester, I want PlayMode scene-composition tests, so that exactly one valid course source is assigned and resolved.
75. As a tester, I want the current Gameplay Scene smoke-tested after migration, so that the Ladybug course retains its +Z progress and existing tuning.
76. As a tester, I want asset-reference tests to use the project test-assets provider, so that tests do not hardcode scene or asset paths.
77. As a tester, I want domain reload and scene load included where serialized source migration matters, so that references survive real Unity lifecycle boundaries.
78. As a tester, I want the Unity compile gate clean before running tests, so that test failures are not confused with compilation errors.
79. As a maintainer, I want the design to respect the plain-controller ADR, so that geometry policy remains isolated from MonoBehaviour lifecycle.
80. As a maintainer, I want the design to respect VContainer composition, so that dependencies remain explicit.
81. As a maintainer, I want the design to respect Rigidbody contact-physics ownership, so that course coordinates do not replace gravity, contacts, or collision response.
82. As a maintainer, I want serialized and public contract changes approved before implementation, so that migration risk is explicit.
83. As a maintainer, I want the optional spline dependency pinned directly if adopted, so that a Cinemachine dependency change cannot remove gameplay functionality.
84. As a maintainer, I want implementation split into parity and curved-course slices, so that regressions are easy to localize.
85. As a maintainer, I want branch semantics recorded in an ADR before implementation, so that product meaning is not buried in projection code.

## Implementation Decisions

- Use the glossary terms **Run Course**, **Run Course Coordinates**, **Run Course Location**, **Run Progress Sample**, **Current Progress**, **Maximum Progress**, **Longitudinal Tangent**, **Signed Longitudinal Speed**, **Support Reference Frame**, and **Projection Continuity**.
- Treat the current fixed **Run Progress Frame** as a valid straight-route implementation, not as a defect in the current Ladybug course.
- Activate curved-course implementation only when a curved, vertical, or otherwise non-linear longitudinal route is approved for production.
- Define course progress as an authored scalar coordinate along the route. It is not final displacement, accumulated Rigidbody travel, raw world-Z movement, camera-forward motion, launch direction, or collider-derived geometry.
- Keep physical arc-length distance available separately from a future canonical branch-progress value. For one open route, the two values are identical.
- Preserve existing **Run Result** semantics: distance means maximum authored course progress reached from the launch origin.
- Make the immutable course snapshot the deep module. It encapsulates route validation, arc-length lookup, position projection, tangent/orientation evaluation, projection continuity, error quality, and reacquisition rules.
- Keep **IRunCourseSource** and the course snapshot contract project-owned. Do not expose Unity Splines containers, knots, native collections, or interpolation units to gameplay consumers.
- Create the course snapshot at **Launch Applied**. Runtime progress must not depend on live authored Transform changes during the run.
- Project the launch position and store its coordinate as the run origin. The authored source supplies route geometry and direction, not a hardcoded start displacement.
- Represent a projected location with scalar progress, physical distance, projected world position, normalized longitudinal tangent, optional authored orientation, projection distance, and an opaque continuity token.
- Require all projected values to be finite. Require non-zero tangents and a valid route length. Reject invalid course authoring with actionable errors.
- Sample progress once per physics tick in **RunProgressService** after the Run Body motion source is available.
- Publish one immutable current sample. Lost Momentum, Run End Flow, presentation, diagnostics, and future consumers read that sample rather than projecting independently.
- Allow **Current Progress** to decrease during rollback.
- Calculate **Maximum Progress** as the monotonic maximum of previous maximum and current progress.
- Provide a service-owned final-position sample/refresh operation for **Run End Flow** so the accepted result includes the final physics position without duplicating projection logic.
- Preserve a degraded invalid-snapshot result policy only if the existing product behavior requires it; otherwise invalid course authoring should prevent a valid run from starting. The chosen failure behavior must be consistent across progress consumers.
- Implement the initial straight adapter with exact behavior parity to the fixed-frame dot product.
- Migrate existing scene composition through the straight adapter before adding spline authoring. Do not combine parity migration and curved-course activation into one unreviewable change.
- For subsequent samples, search near the previous continuity token first. Bound plausible coordinate changes using actual Run Body displacement plus a tolerance.
- Do not select the globally nearest route point every fixed tick.
- Expand projection search only when the local search fails under an explicit policy.
- Treat respawn, teleport, run reset, and initial launch as explicit reacquisition boundaries.
- Precompute or cache arc-length lookup data outside the fixed-tick hot path.
- Require zero managed allocations in steady-state fixed-tick projection and progress sampling.
- Use signed longitudinal speed: linear velocity dotted with the current normalized longitudinal tangent.
- Define low momentum as signed longitudinal speed at or below the configured threshold and maximum-progress gain at or below the configured progress threshold for the configured duration after launch grace.
- Treat negative speed as absence of forward momentum. If fast reverse travel should delay run end, introduce a separately named product rule instead of using unsigned speed.
- Rename the Lost Momentum speed concept from planar speed to forward/longitudinal speed in public and designer-facing terminology.
- Preserve the existing serialized threshold value during the rename through Unity serialization migration metadata. Re-tuning is a separate playtesting activity.
- Keep total planar speed only where presentation genuinely needs motion magnitude; do not reuse it as the progress or Lost Momentum signal.
- Let character presentation consume explicit longitudinal, lateral/planar, and support-relative vertical inputs instead of deriving all meanings from one frame.
- Separate **Course Orientation** from the **Support Reference Frame**.
- Source gravity-up and support-probe direction from an explicit physics/support dependency.
- Permit longitudinal slope calculation to combine the current course tangent with the sampled support normal, but never use authored spline up as an implicit raycast direction.
- Keep Rigidbody gravity, contacts, collision response, and separation under the existing physics ownership established by the speed-model ADR.
- Keep gameplay rules in plain C# services and value types. MonoBehaviours own serialized authoring, shallow conversion, validation entry points, and Unity package adapters.
- Compose runtime instances through VContainer; do not add service location or static mutable state.
- Keep the core contracts and straight adapter in the existing gameplay dependency direction.
- If Unity Splines is adopted, place the spline adapter in a dedicated runtime assembly that depends inward on gameplay contracts. The gameplay core must not depend outward on the spline adapter.
- Promote Unity Splines to a direct manifest dependency before relying on it in gameplay, even though version 2.8.4 is currently resolved transitively through Cinemachine.
- Reuse Unity's spline authoring surface initially. Add custom editor tooling only after a demonstrated authoring problem.
- Keep the first supported non-linear topology to one continuous open route.
- Make the projection token opaque so a future edge-and-local-distance graph location can satisfy the same consumer contract.
- Do not infer branch choice from nearest geometry.
- Before branch implementation, approve an authored topology model and the mapping between physical route distance, canonical comparable progress, **Run Result** distance, and distance rewards.
- Recommended future branch policy: use canonical comparable progress for Run Result and reward fairness, and track physical odometer distance separately if analytics or route-specific rewards need it.
- Keep closed loops, laps, and dynamic rerouting outside the initial contract unless their wrap/reset semantics are separately defined.
- Update architecture documentation when this design becomes approved implementation scope; a dedicated ADR is recommended because the change affects progress meaning, physics-frame separation, assembly dependencies, and serialized authoring.

## Testing Decisions

- Good tests assert externally visible course-coordinate and consumer behavior. They do not assert spline-package internals, cache layout, lookup-table resolution, private fields, or a specific nearest-point implementation.
- Use deterministic EditMode tests for course snapshots, straight parity, arc-length progress, tangent speed, continuity policy, current/maximum progress, invalid data, and Lost Momentum decisions.
- Follow existing **RunProgressServiceTests** prior art for launch-origin sampling, current/maximum progress, invalid-source behavior, and launch-direction independence.
- Follow existing **LostMomentumDetectorTests** prior art for launch grace, duration, combined speed/progress thresholds, state activation, and single-candidate submission.
- Follow existing **RunEndFlowTests** prior art for final result production, candidate priority, latching, and degraded progress behavior.
- Follow existing character-presentation EditMode tests for explicit classification inputs rather than Animator or MonoBehaviour lifecycle coupling.
- Add a straight-route parity test covering lateral displacement, vertical displacement, forward travel, rollback, non-unit authored axes, and non-zero launch origin.
- Add quarter-circle and multi-segment curve tests proving equal arc-length increments produce equal progress.
- Add rising, falling, and near-vertical tangent tests proving course progress is independent from world-horizontal projection.
- Add signed-speed tests for forward, lateral, reverse, non-finite, and zero velocity.
- Add rollback tests proving current progress decreases while maximum progress remains monotonic.
- Add continuity tests for hairpins, crossings, vertically overlapping segments, near-equal candidates, and small oscillations around segment boundaries.
- Add an explicit reacquisition test proving a teleport does not globally reproject until the approved boundary is signalled.
- Add invalid-authoring tests for empty routes, insufficient route length, zero-length segments, non-finite values, degenerate tangents, invalid orientation, and reversed start/end configuration where applicable.
- Add projection-failure tests proving validity and errors propagate without corrupting the previous maximum.
- Add Lost Momentum tests proving high lateral speed does not prevent detection when forward speed and progress are low.
- Add Lost Momentum tests proving meaningful tangent-aligned movement prevents detection on horizontal, curved, and vertical route samples.
- Add Lost Momentum tests proving reverse motion follows the explicit negative-speed policy.
- Add final-result tests proving **Run End Flow** obtains a final service-owned sample and retains maximum progress after rollback.
- Add presentation tests proving authored course orientation and support/gravity orientation remain distinct inputs.
- Add pure slope-calculation tests proving longitudinal slope follows local course tangent while support normal remains physics-owned.
- Use PlayMode tests only where Unity engine behavior is required: serialized component references, scene load, VContainer resolution, spline component integration, support raycasts, and actual Rigidbody fixed-step sampling.
- Extend Gameplay Scene composition tests to require exactly one assigned valid course source and verify the current straight course remains normalized and +Z-equivalent.
- Extend support-context PlayMode tests to prove changing course orientation cannot change the support cast direction unless the explicit support reference frame changes.
- If the spline adapter is adopted, add EditMode adapter contract tests plus a minimal PlayMode serialized-authoring smoke test.
- Use typed gameplay test-asset providers for scene or asset references. Do not hardcode asset paths, GUIDs, Resources paths, or package paths.
- EditMode tests must not rely on Awake, Start, OnDestroy, or runtime MonoBehaviour callback order.
- Use NUnit constraint assertions and the project test naming conventions.
- After every implementation slice, reformat and inspect changed C# files, run the Unity compile gate to completion, then run the targeted tests.
- Verification order: straight parity tests; projection contract tests; migrated service and consumer tests; composition tests; relevant support/presentation PlayMode tests; broader gameplay regression suite.
- If Jobs or Burst is later introduced for projection acceleration, add Burst compile guards and deterministic parity tests against the plain implementation.
- Perform a mobile-oriented allocation and CPU smoke check on a representative long curved route. The steady fixed-tick sample path must allocate zero managed memory.
- Include an asset refresh/domain reload/scene reload verification when serialized source or config fields are migrated.
- Manual Unity smoke checks should cover visible progress stability through bends, rollback, a hairpin, a jump, an off-route excursion, Lost Momentum timing, Run Result distance, and unchanged Ladybug-course behavior.

## Release and Compatibility

- Target Unity version is 6000.3.18f1.
- The first parity migration should not require a package change and must preserve the current Gameplay Scene behavior.
- Unity Splines 2.8.4 is present only as a transitive resolved dependency through Cinemachine. If the spline adapter is approved, add a direct compatible dependency and record that manifest change intentionally.
- Do not couple gameplay contracts to a particular Splines minor version. Package-specific behavior remains inside the adapter and its tests.
- This project is not published as a reusable Unity package, so no semantic package version or package changelog is required.
- No player save-data format changes are required.
- **Run Result** terminology and the meaning of maximum progress remain backward compatible for the current straight course.
- Serialized scene and config changes are migration-sensitive. Implementation requires approval before changing the progress-source field type, public progress interfaces, or Lost Momentum serialized field names.
- Preserve serialized object references where possible. Use Unity's serialization migration support for renamed fields and verify references after domain reload and scene reload.
- Preserve the current Lost Momentum numeric threshold during field migration, then tune signed-forward-speed thresholds separately through playtesting.
- Preserve existing distance reward behavior for the straight course. Branch-based reward semantics remain blocked on the canonical-progress decision.
- Recommended delivery sequence:
  1. Introduce project-owned course contracts and straight adapter with exact parity tests.
  2. Migrate all consumers to the shared course sample and split support/gravity semantics.
  3. Validate the existing Gameplay Scene, results, rewards, presentation, and support behavior.
  4. Only after curved-course approval, add direct spline dependency, isolated adapter, authoring validation, and curved-route tests.
  5. Only after a topology ADR, add branch/graph support.
- A rollback from the first migration should be possible by restoring the fixed-frame composition while leaving result/save data untouched.
- Any future closed-loop, lap, branch, or dynamic-route behavior is a compatibility expansion and requires explicit progress-reset and reward semantics.

## Out of Scope

- Reauthoring the current Ladybug Rooftop Half-Tube as a curved spline route.
- Installing or directly declaring Unity Splines during this PRD-only task.
- Implementing branching, rejoining, laps, closed loops, dynamic rerouting, or procedural course generation in the first slice.
- Inferring a route from terrain, colliders, NavMesh, camera movement, Rigidbody history, or accumulated Vector3 distance.
- Generating track meshes, terrain, colliders, checkpoints, finish volumes, obstacles, pickups, or camera paths from the course source.
- Reworking Run Body steering, speed ownership, gravity, collision response, support-normal filtering, or character physics.
- Using authored course up as gravity-up or support-probe direction.
- Changing Run End candidate priority, finish authority, Run Result UI, reward presentation, economy persistence, or save data.
- Adding analytics, telemetry, leaderboards, best-distance persistence, or route-specific reward systems.
- Adding a global event bus, service locator, static mutable course registry, or DOTS/ECS implementation.
- Building a custom spline editor, graph editor, route-debugging window, importer, or content pipeline.
- Supporting runtime mutation of the authored route during an active run.
- Optimizing with Jobs or Burst before profiling demonstrates a need.
- Renaming broad project terminology such as **Run Result** or **Distance Bonus** without a separate product decision.

## Further Notes

Assumptions:

- The current Ladybug course remains an intentionally straight +Z route for the foreseeable first slice.
- A future curved course is authored before launch and remains immutable during one run.
- One Run Body and one active course source exist per Gameplay Scene.
- Current **Run Result** distance and distance rewards should remain comparable between attempts.
- The fixed-tick service remains the authoritative sampling lifecycle.
- Unity Splines is the leading adapter candidate, not part of the gameplay domain contract.

Unresolved questions:

- Whether the future roadmap actually includes curved, near-vertical, overlapping, looping, or branching longitudinal routes.
- Whether Unity Splines will be formally selected and promoted to a direct dependency.
- Whether projection failure should invalidate the run immediately, retain the last valid location temporarily, or use a bounded recovery window.
- Whether reverse travel should count immediately toward Lost Momentum or receive an explicit recovery grace.
- Whether future branches use canonical comparable progress, physical traveled distance, or both. The recommendation is canonical progress for Run Result/rewards plus a separate odometer where needed.
- What maximum projection distance, local search radius, displacement tolerance, and reacquisition thresholds should be authored or configured.
- Whether course orientation is needed for lane/presentation behavior in the first curved-course slice or whether tangent alone is sufficient.
- Whether the architecture should be recorded as a new ADR before implementation. This PRD recommends doing so.

Useful implementation reference: Unity's SplineUtility supports nearest-point queries, tangent/up evaluation, and distance conversions, but accuracy and performance policy must remain inside the project adapter: https://docs.unity3d.com/Packages/com.unity.splines%402.8/api/UnityEngine.Splines.SplineUtility.html
