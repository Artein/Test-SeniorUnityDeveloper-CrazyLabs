# PRD: High-Speed Run Contact Safety

## Problem Statement

Players expect every run-ending boundary to remain authoritative at the fastest speed the game intentionally supports. A Run Body that visibly passes through a Run Obstacle, Run Finish, or Run Safety Net without the correct Run Result would break the central gameplay contract: Run Obstacle, Run Safety Net, and Run Finish can end a Run, and Run End Flow produces one accepted Run Result.

The current implementation has good foundations but incomplete scenario proof:

- The production Run Body uses a 0.35-meter SphereCollider and a Rigidbody configured for Continuous Dynamic collision detection.
- Unity physics owns collision detection and emits callbacks through Rigidbody Contact Notifier. Run Contact Classifier assigns gameplay meaning only after a collision or trigger notification exists.
- Solid Run Obstacles use non-trigger Colliders. Obstacle Impact classification additionally requires sufficient contact-normal relative speed.
- Run Finish and Run Safety Net use trigger Colliders. They are therefore a separate safety boundary from solid obstacle collision detection.
- The project fixed timestep is 0.02 seconds.
- Player Max Speed has a 20 m/s base soft maximum and a maximum 2× upgrade effect, yielding a supported max-upgraded soft envelope of 40 m/s. The envelope is intentionally soft, so launch, gravity, and collision response can produce transient overspeed.
- At 40 m/s, the Run Body travels 0.8 meters per fixed step. A 0.35-meter-radius sphere crossing a 0.05-meter obstacle has only 0.75 meters of geometric overlap span along the crossing axis. This creates a real discrete-sampling danger case.
- The existing production-scene 80 m/s test uses an approximately 1-meter-thick obstacle. Its nominal sphere-plus-obstacle overlap span is approximately 1.7 meters, slightly greater than the 1.6-meter fixed-step displacement, so the scenario does not prove that Continuous Dynamic CCD was required.
- The existing isolated thin-obstacle notifier test uses a larger 0.5-meter-radius sphere at 60 m/s and begins at a favorable sample phase. It proves that a collision notification can occur, but not that the setup defeats Discrete collision detection.
- Existing targeted PlayMode evidence passes: the production 80 m/s obstacle scenario ends with ObstacleHit, and the isolated Continuous Dynamic body reports a collision against a thin obstacle. This is evidence against a currently demonstrated defect, not proof of the complete safety contract.

The original P2 finding is therefore valid as a safety-contract coverage gap, not as proof that the production Run Body currently tunnels. The first-principles question is not whether a classifier runs after a callback—that is expected—but whether every supported run-ending crossing produces an authoritative observation before gameplay classification.

The shared ChatGPT discussion adds an important adjacent concern: sweep-based CCD must not be treated as a trigger-delivery guarantee. Primary PhysX documentation states that CCD contacts are not supported for trigger shapes. The solid Run Obstacle path and the trigger-driven Run Finish and Run Safety Net paths must be proved independently.

## Solution

Establish deterministic, production-aligned PlayMode coverage for all high-speed Run Body contact boundaries before adding runtime complexity.

The solution has three gates:

1. **Prove the existing solid obstacle path.**
   Exercise the real production Run Body, real Rigidbody Contact Notifier, Run Contact Classifier, Run End Flow, and a deliberately thin non-trigger Run Obstacle at the max-upgraded 40 m/s speed. Place the obstacle at a mathematically adversarial fixed-step phase, prove that the planned sampled poses lie outside the contact envelope, and require the production-authored Continuous Dynamic path to produce exactly one ObstacleHit Run Result. Do not use a mutable Discrete collision-mode control as the acceptance oracle: the owner-authorized one-fixed-step synchronization experiment showed that Discrete can still report this contact.

2. **Prove trigger-boundary reliability independently.**
   Exercise the production Run Body through the authored Run Finish and Run Safety Net volumes at their relevant normal crossing speeds. Verify both their geometric traversal margin and the resulting Finished or OutOfBounds Run Result. Do not claim that the Run Body CCD mode protects these trigger callbacks. Prefer sufficient authored volume thickness as the first protection for sampled trigger overlap.

3. **Add an explicit swept-query safety layer only after a reproduced miss.**
   If a production-aligned Continuous Dynamic scenario reproducibly misses a solid obstacle, or if an approved trigger-boundary scenario misses despite acceptable authoring, introduce a deep, injected run-contact sweep module. It must actively query the production sphere’s swept path, classify the returned Run Contact Category, preserve Obstacle Impact semantics and Run End priority, and submit candidates directly to Run End Flow. A second trigger callback is not a valid swept fallback.

Successful completion gives the team one of two legitimate outcomes:

- The new adversarial tests pass, establishing that current physics and authored boundary geometry satisfy the defined safety envelope without production changes.
- A test fails reproducibly, providing the exact evidence and scenario required to justify a narrowly scoped runtime or authoring fix.

## Unity Surfaces

- **Runtime assembly:** Game.Gameplay. The initial proof phase should not modify runtime behavior. A conditional fallback would add a plain C# sweep policy/orchestrator and a shallow Unity physics-query adapter in this assembly.
- **EditMode test assembly:** Game.Gameplay.Tests.EditMode. Used only if a production deep module is introduced, for deterministic sweep decision, classification, candidate, and priority behavior.
- **PlayMode test assembly:** Game.Gameplay.Tests.PlayMode. Owns production-scene contact scenarios, isolated physics controls, authored contact-volume assertions, and full Run Result verification.
- **Related feature assemblies:** Game.Gameplay.GameplayState, Game.Gameplay.Slingshot, Game.Gameplay.Upgrades, and Game.Gameplay.Economy remain dependencies of the existing production-scene fixture. No new dependency direction is required.
- **Editor assemblies:** No new custom window, inspector, importer, or menu item is required. Existing scene/course validation may be extended only if implementation reveals a reusable authoring invariant.
- **Scene:** GameplayScene remains the production integration surface. Tests use the typed Gameplay test-assets provider and isolated-save fixture to load it.
- **Scene objects:** MovementPhysicsRoot, RunBodyContactColliderRoot, Band 5 Run Finish, Run Safety Net, and authored Run Obstacles are the relevant contracts.
- **Prefabs:** Ladybug Rooftop Half-Tube obstacle prefabs remain solid non-trigger contacts. No prefab migration is planned in the proof phase.
- **ScriptableObjects:** Run Body Movement Config, Player Max Speed Upgrade, Run End Config, and the Ladybug course acceptance profile are read as sources of supported behavior. No serialized field is added in the proof phase.
- **ProjectSettings:** Fixed Timestep remains 0.02 seconds. The PRD does not authorize changing timestep or global physics settings to make a test pass.
- **Packages:** No package additions or upgrades. Existing Unity Physics module, Unity Test Framework, VContainer, and the local Unity AI Agent Connector are sufficient.
- **Helper commands:** Use the project-local Unity AI Agent Connector compile command first, then filtered EditMode or PlayMode test commands. No new shell wrapper or RPC contract is required.
- **Release metadata:** This repository is a game project rather than a published Unity package. The test-only proof phase requires no package version bump, install/sync change, or save migration. A changelog entry is only needed if a later runtime fallback changes shipped behavior.

## User Stories

1. As a player at maximum Player Max Speed upgrade, I want a thin Run Obstacle to end my Run when I hit it, so that upgrades do not let me pass through hazards.
2. As a player with transient overspeed from Launch, I want a solid Run Obstacle to remain authoritative, so that launch energy does not bypass the course rules.
3. As a player falling rapidly, I want Run Safety Net to end my Run, so that I cannot continue below the playable course.
4. As a player crossing the end of the course at maximum speed, I want Run Finish to produce a Finished result, so that a fast successful run is always credited.
5. As a player, I want one Run Result for one ending, so that duplicate physics observations do not duplicate rewards or state transitions.
6. As a player, I want Run Finish to retain priority over a simultaneous Obstacle Impact, so that reaching the finish is resolved consistently.
7. As a player, I want a light obstacle scrape to keep its existing non-ending semantics, so that safety hardening does not make every contact a crash.
8. As a player, I want solid obstacles to retain physical collision response, so that contact safety does not become an invisible trigger-only rule.
9. As a player, I want no premature crash from an obstacle that the Run Body would not actually hit, so that a safety query does not create false positives.
10. As a player, I want contact behavior to remain consistent across retries, so that a previous Run cannot leak pending contact state into the next Run.
11. As a level designer, I want the supported high-speed envelope stated numerically, so that contact volumes can be authored against a concrete traversal distance.
12. As a level designer, I want max-upgraded speed distinguished from transient overspeed, so that course requirements and stress coverage are not conflated.
13. As a level designer, I want Run Obstacles to remain non-trigger Colliders, so that Obstacle Impact continues to use physical contact facts.
14. As a level designer, I want Run Finish to remain an explicit Run Contact Category, so that completion does not depend on names, tags, or layers alone.
15. As a level designer, I want Run Safety Net to remain an explicit Run Contact Category, so that below-course failure stays part of the domain model.
16. As a level designer, I want trigger thickness evaluated along the actual crossing normal, so that rotated or scaled boundaries are assessed correctly.
17. As a level designer, I want authored finish and safety-net volumes to have a documented fixed-step traversal margin, so that later scene edits do not silently reduce reliability.
18. As a level designer, I want an actionable validation failure when a required boundary becomes too thin, so that unsafe authoring is caught before playtesting.
19. As a level designer, I want global Fixed Timestep left unchanged by this feature, so that contact coverage does not destabilize unrelated gameplay timing.
20. As a level designer, I want existing obstacle prefabs and course composition preserved when tests already prove them safe, so that evidence does not cause unnecessary content churn.
21. As a QA engineer, I want a production-body thin-obstacle scenario at 40 m/s, so that the max-upgraded safety contract has direct coverage.
22. As a QA engineer, I want the obstacle placed at an adversarial fixed-step phase, so that the test does not pass because of a lucky starting offset.
23. As a QA engineer, I want the sampled endpoints and contact envelope derived mathematically, so that the positive scenario demonstrates a meaningful fixed-step danger case without depending on a mutable collision-mode oracle.
24. As a QA engineer, I want the Continuous Dynamic scenario to assert the final ObstacleHit Run Result, so that a raw callback alone cannot satisfy acceptance.
25. As a QA engineer, I want the obstacle scenario to use the production 0.35-meter sphere, so that a larger synthetic body does not hide the danger case.
26. As a QA engineer, I want the test to cover the real notifier, classifier, candidate receiver, and Run End Flow, so that every safety boundary in the chain is exercised.
27. As a QA engineer, I want a real authored Run Finish crossing at 40 m/s, so that completion trigger reliability is covered independently.
28. As a QA engineer, I want a real authored Run Safety Net crossing at a defined normal speed, so that below-course trigger reliability is covered independently.
29. As a QA engineer, I want an 80 m/s stress scenario retained as diagnostic headroom, so that known transient overspeed has coverage without redefining the supported envelope.
30. As a QA engineer, I want 40 m/s acceptance and 80 m/s stress results reported separately, so that a stress failure does not ambiguously redefine product support.
31. As a QA engineer, I want bounded fixed-step polling instead of time-based sleeps, so that failures are deterministic and fast.
32. As a QA engineer, I want scene reload and isolated save state around production scenarios, so that test ordering cannot affect results.
33. As a QA engineer, I want exactly one accepted Run Result asserted, so that collision and sweep observations cannot create duplicate outcomes.
34. As a QA engineer, I want simultaneous Finish, Obstacle, and Safety Net candidates covered when a fallback exists, so that current priority remains stable.
35. As a QA engineer, I want a flaky physics scenario treated as a stop condition, so that unreliable coverage is not normalized into the suite.
36. As a gameplay developer, I want the P2 described as an unproven coverage gap, so that implementation effort follows evidence rather than assumption.
37. As a gameplay developer, I want solid collisions and triggers treated as separate detection mechanisms, so that Continuous Dynamic is not overclaimed.
38. As a gameplay developer, I want gameplay classification to remain downstream of authoritative contact facts, so that domain logic does not absorb Unity callback mechanics.
39. As a gameplay developer, I want Obstacle Impact to retain its contact-normal speed threshold, so that a fallback does not classify every Run Obstacle sighting as a crash.
40. As a gameplay developer, I want any swept fallback to use an explicit physics query, so that it does not repeat the trigger-callback limitation it is meant to address.
41. As a gameplay developer, I want the swept query aligned with the production Run Body sphere, so that its hit envelope matches the physical authority.
42. As a gameplay developer, I want a plain C# sweep policy behind an injected physics port, so that decision logic can be tested without MonoBehaviour lifecycle dependence.
43. As a gameplay developer, I want the Unity physics adapter to stay shallow, so that PhysicsScene query details do not leak into Run End rules.
44. As a gameplay developer, I want a sweep to consume the final intended fixed-step movement, so that it checks the path Unity is about to simulate.
45. As a gameplay developer, I want sweep results submitted through the existing Run End candidate boundary, so that result construction and state transition remain single-authority.
46. As a gameplay developer, I want query filters to honor Run Contact Category and collision layers, so that surfaces, pickups, and animated sensors are not misclassified.
47. As a gameplay developer, I want trigger inclusion to be explicit per query, so that solid Obstacle Impact and trigger Finish or Safety Net semantics remain visible.
48. As a gameplay developer, I want moving-obstacle relative velocity left unsupported until the course contract allows moving obstacles, so that static-course assumptions are not hidden.
49. As a gameplay maintainer, I want no new public API or serialized data in the proof phase, so that adding evidence has no migration cost.
50. As a gameplay maintainer, I want fallback code gated by a reproducible failing scenario, so that the runtime does not accumulate speculative complexity.
51. As a gameplay maintainer, I want existing ADR dependency direction preserved, so that Unity adapters do not become gameplay authorities.
52. As a gameplay maintainer, I want the main runtime assembly to avoid new dependencies, so that the feature remains cohesive and portable.
53. As a gameplay maintainer, I want test-only geometry helpers kept in the PlayMode test assembly, so that production code is not created solely to arrange a test.
54. As a gameplay maintainer, I want mobile query cost measured if a fallback is introduced, so that a safety layer does not create a per-frame performance regression.
55. As a gameplay maintainer, I want source documentation linked to the decision, so that future Unity or PhysX upgrades can revalidate the assumptions.
56. As a reviewer, I want acceptance criteria that can legitimately conclude no production fix is needed, so that passing safety evidence is treated as a valid outcome.
57. As a reviewer, I want any runtime fix to include a regression test that fails without it, so that the change remains justified and protected.
58. As a reviewer, I want authoring changes preferred for trigger thickness when they preserve gameplay intent, so that runtime querying is not the default response to unsafe content geometry.
59. As a reviewer, I want the exact target Unity version compiled before PlayMode tests, so that physics scenarios run only on a clean project.
60. As a reviewer, I want the final verification to distinguish compile, targeted tests, scene contracts, and manual smoke evidence, so that confidence is auditable.

## Implementation Decisions

1. **Classify the finding correctly.** Treat this work as closing a P2 safety-contract coverage gap. Do not state that production tunneling exists unless the new production-aligned scenario reproduces it.

2. **Define two speed tiers.** The supported acceptance tier is 40 m/s, derived from the 20 m/s base soft maximum and the maximum 2× Player Max Speed modifier. The initial stress tier is 80 m/s because it already exists as an unsupported-high-speed diagnostic. The stress tier is headroom, not a product guarantee.

3. **Define the fixed-step danger metric.** For a crossing speed v and fixed interval dt, compare v × dt with the projected overlap span of the Run Body and contacted volume along the crossing axis. A scenario is CCD-relevant only when the discrete step can move from one non-overlapping pose to another across the volume.

4. **Keep detection contracts separate.** Solid Run Obstacles use collision detection and collision notifications. Run Finish and Run Safety Net use trigger overlap and trigger notifications. A result from one contract does not prove the other.

5. **Make the first implementation slice test-only.** Add production-scene and isolated PlayMode coverage without changing runtime code, scenes, prefabs, ScriptableObjects, package dependencies, or ProjectSettings. Passing tests are an acceptable final outcome.

6. **Use the production Run Body authority.** The positive obstacle scenario must use the scene-authored 0.35-meter Run Body Contact Collider, production Rigidbody, production notifier, classifier, Run End Flow, Gameplay State transition, and Run Result notifier.

7. **Use a deliberately thin solid obstacle.** The test obstacle must be a non-trigger Collider with Run Contact Category Obstacle and a crossing-axis thickness small enough that the 40 m/s fixed-step displacement exceeds the sphere-plus-obstacle overlap span.

8. **Control the fixed-step phase mathematically.** Arrange the planned pre-step and post-step body centers on opposite sides of the obstacle with neither sampled pose inside the calculated contact envelope. The test helper may calculate placement from actual collider radius, contact offsets, obstacle thickness, fixedDeltaTime, and speed. Do not rely on an arbitrary distance and hope it is adversarial.

9. **Reject a mutable Discrete negative control as an acceptance oracle.** Assigning Discrete while held in PreLaunch, waiting exactly one fixed step, then phasing and launching still produced two collision contacts and an ObstacleHit in the isolated paired experiment. Keep that result as diagnostic evidence; use the geometry preconditions and production-authored Continuous Dynamic outcome for permanent acceptance.

10. **Assert domain outcome, not only callback delivery.** The Continuous Dynamic test passes only when exactly one Run Result with reason ObstacleHit is accepted and Gameplay State becomes Run Ended. A raw CollisionEntered notification is useful diagnostic evidence but insufficient acceptance.

11. **Preserve Obstacle Impact semantics.** The scenario must use an approach velocity whose contact-normal component exceeds the configured Obstacle Impact threshold. Safety work must not bypass or weaken the threshold.

12. **Prove authored trigger boundaries independently.** Use the production Run Body to cross Band 5 Run Finish and Run Safety Net in their relevant normal directions. Assert exactly one Finished or OutOfBounds Run Result, respectively.

13. **Record trigger traversal margin.** Tests or existing scene assertions should calculate projected collider thickness plus Run Body diameter along the crossing normal and compare it with supported and stress fixed-step displacement. This is an authoring contract, not a claim that CCD protects triggers.

14. **Prefer content correction for unsafe triggers.** If an authored Run Finish or Run Safety Net lacks adequate overlap margin, first widen or reshape that volume while preserving visual and gameplay intent. Introduce a runtime query only if authoring cannot robustly cover approved motion.

15. **Gate runtime fallback on reproducible evidence.** A fallback is authorized only when the Continuous Dynamic production-body scenario fails repeatedly under a deterministic adversarial arrangement, or when an approved trigger scenario fails despite an accepted authoring margin. The failing test becomes the regression test.

16. **Use an explicit query if fallback is required.** The fallback must query the Run Body sphere swept from its current pose along the final planned fixed-step displacement. It must not create another trigger sensor and wait for OnTriggerEnter.

17. **Extract a deep sweep module.** A plain C# run-contact sweep policy should own eligibility, hit ordering, category filtering, Obstacle Impact evaluation inputs, and candidate production behind a small injected query interface. A Unity-facing adapter should own PhysicsScene sphere casts and translate hit facts.

18. **Use live production geometry.** The Unity adapter should derive sphere center and world radius from the authoritative Run Body Contact Collider, including transform scale. Do not duplicate radius in an unrelated config.

19. **Place the sweep in the fixed-step pipeline.** If introduced, it must observe the final movement target for the current fixed tick and submit candidates before Run End resolution for that logical tick. Pipeline order must remain explicit and covered by tests.

20. **Reuse Run End Flow as the single result authority.** Sweep output enters through the existing Run End candidate receiver. The sweep must not transition Gameplay State, construct Run Result, grant rewards, or notify UI directly.

21. **Preserve same-tick priority.** Finished remains higher priority than ObstacleHit, which remains higher than OutOfBounds, which remains higher than LostMomentum. Collision, trigger, and sweep observations in the same logical tick must resolve through that existing order.

22. **Tolerate duplicate observations safely.** A sweep and a Unity callback may observe the same contact. Run End Flow must still accept one result. The fallback should avoid needless repeated candidates where practical, but deduplication must not create a second result authority.

23. **Keep filters explicit.** Solid-obstacle queries ignore trigger Colliders and accept only collider-local Run Contact Category Obstacle. Trigger-boundary queries, if ever required, explicitly include triggers and accept only Finish or SafetyNet. Run Surface, pickups, animated contact sensors, and uncategorized Colliders are ignored.

24. **Keep static-obstacle scope explicit.** The current Ladybug course contract disallows moving obstacles. Obstacle Impact from a sweep may therefore use Run Body velocity against a static hit normal. Supporting moving hazards requires a separate relative-motion decision and is not silently included.

25. **Do not add static access.** Physics queries, time, motion, and candidate submission stay behind injected instances composed by VContainer. No new static service, singleton, or global mutable state is permitted.

26. **Respect assembly direction.** The feature remains in Game.Gameplay and depends only on assemblies already referenced. Test helpers remain in the existing EditMode or PlayMode test assemblies. No runtime assembly may depend on a test or Editor assembly.

27. **Avoid production code solely for arrangement.** Fixed-step phase calculators and temporary thin-obstacle builders remain test helpers unless their rules become a reusable shipped authoring validator.

28. **Treat serialization as a stop boundary.** The proof phase adds no serialized data. If a fallback requires designer-authored thresholds, layer masks, margins, or enable flags, stop and obtain approval before changing public API or serialization.

29. **Do not tune global physics to satisfy coverage.** Fixed Timestep, solver settings, contact offset, and layer collision matrices remain unchanged unless a separately reviewed project-wide requirement justifies them.

30. **Make performance conditional and measurable.** If a sweep is introduced, it runs only while Running, uses non-allocating query storage where practical, avoids scene-wide searches, and records mobile profiling evidence. Optimization must not obscure correctness.

31. **Keep diagnostics bounded.** A failing test should report speed, fixedDeltaTime, displacement, body radius, obstacle or trigger thickness, collision mode, start/end positions, notification count, result count, and final state. Runtime player-facing diagnostics are not required.

32. **Update domain documentation only for a shipped rule.** If the project adopts a permanent Run Contact traversal invariant or explicit sweep authority, add that terminology and invariant to the gameplay context and create or update an ADR as appropriate. Test-only evidence does not require a new domain concept.

## Testing Decisions

1. A good test observes gameplay behavior through public or existing internal test boundaries: collision or trigger notification where diagnostically useful, accepted Run Result, and Gameplay State. It does not assert private fields, method call counts inside the classifier, or the concrete type of an optional policy.

2. The primary acceptance test is a PlayMode scenario using the production Gameplay scene and Run Body. Suggested behavior name: given_MaxUpgradedSpeedProductionRunBody_when_CrossingAdversarialThinObstacle_then_ObstacleHitEndsRun.

3. The one-fixed-step Discrete synchronization scenario is a completed diagnostic experiment, not a permanent test. It falsified the assumption that Discrete must miss this arrangement, so the committed acceptance fixture does not mutate the production collision mode.

4. The positive Continuous Dynamic scenario asserts the production body’s collision mode, actual sphere radius, obstacle non-trigger status, Obstacle Run Contact Category, one collision observation, one ObstacleHit result, and Run Ended state.

5. The adversarial helper derives placement from the actual world-space sphere extent, obstacle projected thickness, test speed, and Time.fixedDeltaTime. It also verifies as a precondition that fixed-step displacement exceeds overlap span.

6. The test uses bounded WaitForFixedUpdate loops and condition checks. It does not use arbitrary real-time delays.

7. The test isolates persistent save data and reloads the Gameplay scene when necessary, following Base Gameplay Scene PlayMode Fixture prior art.

8. The test creates only primitive temporary physics geometry when needed. It does not hardcode asset paths, GUIDs, Resources paths, or package paths. Any missing persistent asset must be exposed through the typed Gameplay test-assets provider.

9. The existing high-speed production test remains useful as overspeed and end-to-end regression coverage, but it is not renamed or represented as the adversarial CCD proof unless its geometry and phase are changed to meet the negative-control rule.

10. The existing Rigidbody Contact Notifier thin-obstacle test remains useful adapter coverage. It may be refactored to share an adversarial helper, but the production-scene result test remains necessary.

11. Add a PlayMode scenario for Band 5 Run Finish at 40 m/s normal crossing speed. Assert the authored Collider is a trigger, the category is Finish, exactly one Finished result is accepted, and the state becomes Run Ended.

12. Add a PlayMode scenario for Run Safety Net at a defined 40 m/s normal crossing speed. Assert the authored Collider is a trigger, the category is SafetyNet, exactly one OutOfBounds result is accepted, and the state becomes Run Ended.

13. Add 80 m/s trigger stress cases only as a separate category of evidence. The report must label them stress tests, not supported-envelope acceptance.

14. Extend existing scene/course assertions to verify obstacle Colliders remain non-trigger and finish/safety Colliders remain trigger. Preserve the existing collider-local Run Contact Category contract.

15. Add a deterministic geometry assertion for each authored trigger boundary: projected overlap span exceeds supported fixed-step travel by a documented positive margin. If the team adopts 80 m/s as required headroom, assert that margin separately.

16. If trigger thickness validation becomes a production deep module, cover its pure calculations in EditMode with rotated boxes, non-uniform scale, invalid geometry, exact-boundary equality, positive margin, and unsafe margin cases.

17. If a swept fallback is introduced, add EditMode tests for eligibility, zero or invalid displacement, solid versus trigger filtering, category mapping, Obstacle Impact threshold behavior, nearest relevant hit selection, and no candidate for Run Surface or uncategorized hits.

18. If a swept fallback is introduced, add a PlayMode regression that fails with the fallback disabled and passes with it enabled under the reproduced production scenario. Avoid a permanent feature toggle unless one already belongs to product requirements; test composition may wire the alternative directly.

19. If a swept fallback is introduced, add a same-tick duplicate scenario where both sweep and collision notification observe the obstacle. Assert one ObstacleHit result.

20. If a swept fallback is introduced, add a same-tick priority scenario containing Finish, Obstacle, and Safety Net observations. Assert Finished wins without changing the existing priority contract.

21. Pipeline-order prior art is Run Fixed Step Pipeline tests. Any new sweep fixed-step stage must have an EditMode ordering test and a PlayMode behavior test.

22. Physics integration prior art is Rigidbody Contact Notifier tests, Gameplay Scene Run Body Speed Ownership tests, Run Body Contact Physics Scenario, Gameplay Scene Composition tests, and Ladybug Half-Tube Run Course scene assertions.

23. Compile the project through the Unity AI Agent Connector before tests. Any compile error is fixed before running a test.

24. Run the exact changed EditMode tests after a clean compile. Then run the exact changed PlayMode scenarios. Broader suite execution is proportional to whether production code, scene authoring, or only tests changed.

25. Re-run each physics acceptance scenario enough times to detect obvious sample-phase instability without converting repetition into the oracle. Deterministic arrangement remains the primary defense against flakiness.

26. If a PlayMode physics test is flaky and cannot be made deterministic quickly, stop and ask for direction per project rules. Do not widen tolerances until a weak test passes.

27. Manual Unity smoke checks validate visible obstacle collision, finish completion, safety-net failure, retry, and absence of premature contact endings. Manual checks complement but do not replace automated results.

28. No Burst guard is required unless implementation touches Jobs or Burst, which this PRD does not propose.

29. Documentation/static verification checks that the PRD’s numeric assumptions still match the scene collider, Run Body Movement Config, Player Max Speed Upgrade, TimeManager, and course acceptance profile.

## Release and Compatibility

- **Unity version:** Target Unity 6000.3.18f1. Physics behavior and documentation assumptions must be rechecked when the project upgrades Unity or changes the 3D physics backend.
- **Physics assumptions:** Built-in Unity 3D physics, production SphereCollider radius 0.35 meters, Continuous Dynamic Run Body collision detection, and 0.02-second fixed timestep.
- **Supported speed assumption:** 40 m/s is the max-upgraded soft envelope from current assets. It is not a hard velocity cap. The 80 m/s tier is current stress coverage, not guaranteed product behavior.
- **Course assumption:** Current Ladybug Rooftop Half-Tube obstacles are static and use non-trigger Colliders; the course acceptance profile disallows moving obstacles.
- **Package impact:** No package version bump, dependency addition, package manifest edit, installation change, or synchronization step in the proof phase.
- **Changelog impact:** Test-only evidence does not require a player-facing changelog entry. A later runtime or authoring change should be recorded according to the repository’s release practice.
- **Save compatibility:** No changes to currency, upgrades, Run Result serialization, or persistent save format.
- **Scene/prefab compatibility:** No scene or prefab migration in the proof phase. If trigger volumes need widening, existing object identity and Run Contact Category remain stable.
- **Public API compatibility:** No public API changes are planned. Conditional fallback interfaces should remain internal unless a separately reviewed cross-assembly consumer exists.
- **Behavior compatibility:** Obstacle Impact threshold, Run End priority, one-result latching, reward construction, retry state, movement speed ownership, and Unity collision response must remain unchanged.
- **Mobile performance:** A test-only change has no runtime cost. Any later per-fixed-step query requires profiler evidence on representative mobile hardware or an equivalent target profile.
- **Rollback:** Test-only additions can be removed without asset migration. A later fallback must be independently removable while leaving callback-based Run Contact behavior intact.

## Out of Scope

- Claiming or fixing a production tunneling defect before the adversarial scenario reproduces one.
- Replacing Unity physics as the Run Body collision substrate.
- Changing Fixed Timestep, global solver iterations, contact offset, or the physics layer matrix.
- Converting solid Run Obstacles into triggers.
- Converting Run Finish or Run Safety Net into solid collision barriers.
- Making Animated Contact Sensors an Obstacle Impact authority.
- Changing Run Contact Category, Run End Reason, Run Result, reward, state-transition, or priority semantics.
- Changing the Player Max Speed upgrade, its maximum level, or the soft speed-envelope design.
- Adding a hard 40 m/s velocity clamp.
- Supporting moving obstacles or arbitrary moving trigger volumes.
- General-purpose projectile CCD, pickup tunneling, camera occlusion, or Run Surface probing changes.
- New packages, Unity upgrades, Addressables changes, build pipeline changes, or platform signing work.
- New public APIs, save migrations, or serialized configuration without separate approval.
- Visual redesign of obstacles, finish presentation, safety net, or course layout.
- Publishing this PRD or creating remote issues.

## Further Notes

### Acceptance summary

The proof phase is complete when:

- The 40 m/s production Run Body crosses a deliberately adversarial thin solid Run Obstacle.
- The planned sampled endpoints lie outside the calculated contact envelope while one fixed-step displacement crosses the obstacle.
- Continuous Dynamic produces one ObstacleHit Run Result and Run Ended state.
- The production Run Finish produces one Finished result at the supported crossing speed.
- The production Run Safety Net produces one OutOfBounds result at the approved normal crossing speed.
- Authored trigger overlap margins are explicitly verified.
- The project compiles cleanly and all targeted tests pass.
- No production fallback is added when the existing path satisfies these conditions.

If the Continuous Dynamic case misses, or an approved trigger case misses, the failure becomes the evidence gate for the smallest remediation. For triggers, safe authoring thickness is preferred. For solid obstacles or unavoidable trigger crossings, the fallback is an active swept query that submits typed candidates to Run End Flow.

### Assumptions

- The current asset values remain authoritative: 20 m/s base soft maximum, 2× maximum Player Max Speed effect, 0.35-meter production sphere radius, and 0.02-second fixed timestep.
- 40 m/s is the supported max-upgraded safety tier.
- 80 m/s is a useful initial transient-overspeed stress tier because existing diagnostics already exercise it.
- Existing Run End priority and one-result latching are correct product behavior.
- Existing authored Run Finish and Run Safety Net thicknesses appear to provide overlap spans above 40 m/s fixed-step travel; automated measurement must confirm this rather than relying on the estimate.
- Static obstacle relative velocity is sufficient for the current course because moving obstacles are disallowed.

### Unresolved questions

- Should 80 m/s become mandatory release acceptance for Run Finish and Run Safety Net, or remain diagnostic stress coverage?
- What is the approved credible maximum normal velocity for falling through Run Safety Net?
- If a trigger margin fails, what minimum authored safety factor above one fixed-step displacement should become the permanent course contract?
- If a runtime query is justified, can it remain entirely internal without a serialized enable flag or tuning asset? A required serialized change is a stop-and-approve boundary.

### Evidence and references

- Existing targeted evidence before this PRD: the production unsupported-high-speed obstacle test passed with ObstacleHit, and the isolated thin-obstacle Continuous Dynamic notifier test passed.
- Unity Continuous Collision Detection manual: https://docs.unity3d.com/6000.3/Documentation/Manual/ContinuousCollisionDetection.html
- Unity sweep-based CCD manual: https://docs.unity3d.com/6000.3/Documentation/Manual/sweep-based-ccd.html
- Unity fixed updates manual: https://docs.unity3d.com/6000.3/Documentation/Manual/fixed-updates.html
- Unity Rigidbody collision mode guidance: https://docs.unity3d.com/6000.3/Documentation/Manual/physics-optimization-cpu-rigidbody-collision-modes.html
- PhysX pair flags, including the trigger-shape CCD limitation: https://nvidia-omniverse.github.io/PhysX/physx/5.2.1/_build/physx/latest/struct_px_pair_flag.html
- Shared ChatGPT discussion supplied as context: https://chatgpt.com/share/6a5619c1-75d8-83eb-a147-cc1bb338541e
- The shared discussion is treated as secondary context. Unity and PhysX primary documentation, repository assets, and executable tests remain the decision authorities.
