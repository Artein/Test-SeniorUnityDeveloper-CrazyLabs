## Problem Statement

After a **Run** ends and the same loaded gameplay session returns to **Pre-Launch**, the **Launch Target** can appear offset from the center of the
**Band**. The first scene load is authored correctly: the **Launch Target** **Band Center** aligns with the **Slingshot** **Rest Point**. The bug is a
same-session restart problem: runtime state from the previous **Run** can leave the **Launch Target**, Rigidbody, or cached **Slingshot** geometry out
of sync before capture is re-enabled.

The confusing part is ownership. The **Rest Point** is owned by the **Slingshot** and defines the unloaded **Pull Point** for capture. It is not the
whole level start pose. Returning to **Pre-Launch** must restore the authored relationship between the **Slingshot** and **Launch Target**, including
the target pose that makes its **Band Center** align with the **Rest Point**.

The implementation must preserve the existing architecture: **Slingshot** capture remains state-agnostic, **Gameplay Flow** remains the bridge between
**Gameplay State** and **Slingshot** behavior, and Unity-facing object manipulation stays behind shallow adapters composed through VContainer.

## Solution

Introduce **Pre-Launch Rig Pose** reset as the source of truth for same-session restart alignment.

A gameplay-level reset coordinator applies the authored **Pre-Launch Rig Pose** before **Slingshot** capture is enabled. The reset restores the
authored **Slingshot** rig pose and **Launch Target** pose, clears stale target motion, and establishes a known held Rigidbody baseline. The
**Launch Target** rotation is restored from the authored pose during restart, while ordinary held positioning during **Pull** remains position-only.

The reset runs before completed **Pre-Launch** state-change observers react. On a transition into **Pre-Launch**, reset happens on the
`GameplayStateChanging` boundary. When the scene starts already in **Pre-Launch**, the same reset path runs during initialization before capture is
enabled. By the time `GameplayStateChanged` observers see **Pre-Launch**, the rig is already aligned.

After the reset, **Slingshot** capture refreshes its geometry snapshot from the current scene transforms before aligning the held **Launch Target** and
showing idle **Band Shape**. This prevents a stale cached **Rest Point**, anchor, or **Launch Frame** from controlling a newly reset rig.

## Unity Surfaces

- Runtime assemblies and asmdefs
  - Gameplay runtime gains a small **Pre-Launch Rig Pose** reset coordinator or service.
  - Existing **Gameplay Flow** or adjacent orchestration subscribes to the pre-**Pre-Launch** transition boundary.
  - Existing **Slingshot** capture refreshes geometry from its view on capture enable.
  - Existing **Launch Target** adapter gains or exposes a narrow reset-to-held-baseline contract.
- Editor assemblies, windows, inspectors, importers, or menu items
  - No custom editor window, importer, or menu item is expected.
  - Existing scene authoring validation may be extended if missing pose-provider references need fail-fast feedback.
- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings
  - Gameplay Scene gains explicit authored pose-provider references for the **Slingshot** rig and **Launch Target**.
  - No new **Gameplay State** asset is required.
  - No package manifest or ProjectSettings change is expected.
- RPC/helper commands, hooks, or shell wrappers
  - Unity AI Agent Connector compile is the required compile gate.
  - Targeted EditMode and PlayMode tests should be run through the Unity AI Agent Connector.
  - No new helper command or RPC contract is required.
- Package versioning, changelog, and installation/sync behavior
  - This is project gameplay work, not a package release.
  - No package version, changelog, install, sync, save format, or player data migration is expected.

## User Stories

1. As a player, I want a restarted **Run** to begin with the **Launch Target** centered in the **Band**, so that the next shot looks correct.
2. As a player, I want same-session restart to look like first scene load, so that retrying does not expose hidden runtime drift.
3. As a player, I want the **Band** idle shape to pass through the intended **Band Center**, so that the **Slingshot** reads as loaded correctly.
4. As a player, I want a low-speed failed or ended **Run** to return cleanly to **Pre-Launch**, so that I can immediately pull again.
5. As a player, I want the **Launch Target** not to inherit odd rotation or physics motion from the previous **Run**, so that restart feels stable.
6. As a designer, I want an authored **Pre-Launch Rig Pose**, so that start alignment can be tuned explicitly in the scene.
7. As a designer, I want the **Slingshot** rig pose and **Launch Target** pose authored separately, so that each object keeps clear ownership.
8. As a designer, I want the **Rest Point** to keep its Slingshot meaning, so that it is not overloaded as a whole level spawn point.
9. As a designer, I want same-session restart to preserve the authored relationship between **Rest Point** and **Band Center**, so that scene tuning
   remains predictable.
10. As a designer, I want no new **Gameplay State** for reset, so that the existing **Pre-Launch -> Running -> Run Ended -> Pre-Launch** loop stays
    understandable.
11. As a developer, I want **Pre-Launch Rig Pose** to be the reset source of truth, so that restart alignment is not inferred from stale runtime
    transforms.
12. As a developer, I want reset owned above **Slingshot** capture, so that capture remains a consumer of aligned scene geometry.
13. As a developer, I want **Slingshot** capture to refresh geometry on enable, so that cached anchors, **Rest Point**, and **Launch Frame** cannot
    outlive a rig reset.
14. As a developer, I want reset to run before `GameplayStateChanged` observers see **Pre-Launch**, so that every completed-state listener observes a
    consistent rig.
15. As a developer, I want the initialization path to handle a scene that starts in **Pre-Launch**, so that first load and restart share one contract.
16. As a developer, I want the **Launch Target** pose reset to restore rotation, so that previous **Run** spin or collision rotation does not leak
    into the next pull.
17. As a developer, I want held **Pull** movement to remain position-only after reset, so that this fix does not add hidden target-facing behavior.
18. As a developer, I want the Rigidbody held baseline to be explicit, so that previous kinematic or constraint state is not accidentally preserved.
19. As a developer, I want stale velocity and angular velocity cleared at restart, so that no previous **Run** motion affects capture.
20. As a developer, I want reset behavior behind narrow interfaces, so that tests can use fakes and MonoBehaviours remain shallow.
21. As a developer, I want existing **Run End Flow** to keep ending runs only, so that it does not become responsible for scene object placement.
22. As a developer, I want existing **Gameplay State** semantics preserved, so that `Changing` still means about-to-change and `Changed` still means
    completed.
23. As a developer, I want **Gameplay Flow** to remain the bridge from state to **Slingshot** capture, so that the **Slingshot** stays state-agnostic.
24. As a developer, I want no launch math retuning, so that power, steering, and **Pull Offset** behavior stay unchanged.
25. As a developer, I want no unconditional physics synchronization added unless tests prove it is needed, so that restart does not add unnecessary
    runtime cost.
26. As a developer, I want authoring failures to fail loud, so that missing pose references do not silently produce misaligned restarts.
27. As a tester, I want EditMode tests for reset-before-capture ordering, so that lifecycle sequencing is deterministic.
28. As a tester, I want EditMode tests for initialization while already in **Pre-Launch**, so that first-load alignment does not rely on scene load
    timing luck.
29. As a tester, I want EditMode tests for geometry refresh on capture enable, so that stale geometry cannot regress silently.
30. As a tester, I want EditMode tests for Rigidbody held baseline reset, so that stale constraints or motion cannot leak across runs.
31. As a tester, I want one PlayMode same-session **GameplayScene** regression, so that the real scene, Rigidbody, transforms, and **Band** rendering
    are covered together.
32. As a tester, I want the PlayMode regression to assert **Band Center** equals **Rest Point** after restart, so that it catches the visible bug.
33. As a tester, I want the PlayMode regression to assert restored rotation and held Rigidbody state, so that the next **Pull** starts from a clean
    target.
34. As a tester, I want the PlayMode regression to verify idle **Band Shape** comes from aligned rest geometry, so that visual and gameplay anchors
    agree.
35. As a maintainer, I want this documented as a PRD and local issues, so that the next implementation pass has stable acceptance criteria.
36. As a maintainer, I want no new ADR for this behavior, so that ADRs remain reserved for hard-to-reverse architecture decisions.
37. As a maintainer, I want the design to respect existing ADRs on plain C# controllers, VContainer composition, shallow MonoBehaviours, and direct
    events, so that the fix stays consistent with the project.
38. As a maintainer, I want old "retry placement out of scope" assumptions superseded by this PRD, so that same-session restart is no longer treated
    as an incidental capture side effect.

## Implementation Decisions

- Use the glossary term **Pre-Launch Rig Pose** for the authored pose where the **Slingshot** and **Launch Target** are aligned before **Pre-Launch**
  interaction.
- **Pre-Launch Rig Pose** aligns one **Slingshot** with one **Launch Target**.
- **Pre-Launch Rig Pose** places the held **Launch Target** so its **Band Center** aligns with the **Rest Point**.
- The **Rest Point** remains owned by the **Slingshot**. It is not the whole level initial pose.
- Same-session restart restores the full **Pre-Launch** rig, not only the **Launch Target**.
- The authored pose provider exposes separate **Slingshot** rig pose and **Launch Target** pose references.
- Reset belongs above **Slingshot** capture in gameplay orchestration.
- **Slingshot** capture consumes current aligned geometry; it does not own level restart placement.
- Reset runs on the `GameplayStateChanging` boundary when transitioning into **Pre-Launch**.
- Initialization also applies reset before capture if the current **Gameplay State** is already **Pre-Launch**.
- By the time `GameplayStateChanged` observers see **Pre-Launch**, the **Pre-Launch Rig Pose** has already been applied.
- Same-session restart order is: finish or leave ended **Run** handling, keep **Slingshot** capture disabled, apply **Pre-Launch Rig Pose**, clear
  **Launch Target** motion and held baseline, refresh **Slingshot** geometry from current transforms, then enable capture.
- **Launch Target** restart restores authored position and rotation.
- Held **Pull** positioning after reset remains position-only and preserves current rotation.
- Restart forces the **Launch Target** into a known held Rigidbody baseline instead of deriving preserved Rigidbody state from the previous **Run**.
- The known held Rigidbody baseline clears linear and angular velocity and establishes intended held kinematic and constraint state.
- The reset path should not rely on stale preserved previous Rigidbody state from launch or collision behavior.
- **Slingshot** geometry should be refreshed from current view transforms on capture enable.
- The initial geometry snapshot can still validate setup and allocate buffers, but it must not be the only geometry source forever.
- **Run End Flow** remains responsible for producing one **Run Result** and returning state toward **Pre-Launch**; it does not own pose placement.
- **Gameplay Flow** or a nearby gameplay-level coordinator owns reset/capture ordering.
- No new **Gameplay State** is required.
- No new ADR is required because this behavior follows existing architecture decisions instead of introducing a hard-to-reverse architectural tradeoff.

## Testing Decisions

- Good tests verify externally visible contracts: reset order, aligned transforms, geometry refresh, held Rigidbody baseline, and scene restart
  behavior. They should not assert private helper structure.
- Prefer EditMode tests for orchestration and adapter contracts.
- Add EditMode coverage that transition into **Pre-Launch** applies **Pre-Launch Rig Pose** before capture is enabled.
- Add EditMode coverage that initialization while already in **Pre-Launch** applies reset before capture.
- Add EditMode coverage that `GameplayStateChanged` **Pre-Launch** listeners observe the rig after reset.
- Add EditMode coverage that **Slingshot** capture refreshes geometry before showing capture idle.
- Add EditMode coverage that the **Launch Target** reset restores authored rotation and clears motion.
- Add EditMode coverage that held **Pull** positioning remains position-only after restart.
- Add EditMode coverage that the held Rigidbody baseline does not preserve stale launch-time constraints or motion as the next baseline.
- Add PlayMode scene regression coverage for same-session restart in the existing Gameplay Scene.
- The PlayMode regression should start from one loaded scene session, disturb or launch the **Launch Target**, return to **Pre-Launch**, and assert
  alignment without reloading the scene.
- The PlayMode regression should assert the **Band Center** aligns with the **Rest Point** after restart.
- The PlayMode regression should assert the **Launch Target** rotation and held Rigidbody state are restored.
- The PlayMode regression should assert idle **Band Shape** uses the aligned current rest geometry.
- Existing first-load composition tests should remain green.
- Compile must be clean before running tests.
- Run targeted EditMode tests for changed orchestration, **Slingshot**, and **Launch Target** behavior.
- Run the targeted PlayMode Gameplay Scene restart regression after scene/composition changes.
- Manual Unity smoke should verify first load, one run restart, several repeated restarts, shallow launch, and a target-collision restart.

## Release and Compatibility

- Unity version assumption: the current Unity 6 project with Unity Test Framework, VContainer, Rigidbody **Launch Target** behavior, and the existing
  Gameplay Scene.
- No new third-party package dependency is expected.
- No package manifest, package lock, package version, or changelog change is expected.
- No save format, player data, or remote sync migration is expected.
- Backward compatibility risk is limited to lifecycle ordering around entering **Pre-Launch** and capture geometry refresh.
- Launch power, launch direction, steering, **Pull Offset**, **Pull Distance**, and **Band Shape** solver behavior should remain unchanged.
- Existing scene first-load alignment should remain unchanged except for being backed by the same reset contract used by same-session restart.

## Out of Scope

- Runtime rope physics, joints, or a physics-simulated **Band**.
- Retuning **Slingshot** launch power, launch direction, lateral steering, or up-speed behavior.
- Changing **Band Release Recoil** clearance rules.
- Adding a new **Gameplay State** for reset or retry.
- Adding score UI, result screens, coins, analytics, or progression.
- Supporting multiple **Launch Target** instances in one active **Slingshot**.
- Supporting compound or multi-collider **Launch Target** pose authoring beyond existing silhouette decisions.
- Custom editor tooling beyond narrow validation if implementation proves it useful.
- Package publishing or changelog updates.

## Further Notes

- Assumption: same-session restart can be reproduced by returning from **Run Ended** to **Pre-Launch** without reloading the Gameplay Scene.
- Assumption: first-load scene authoring already has **Band Center** aligned with **Rest Point**; the bug is runtime restart drift.
- Assumption: separate **Slingshot** and **Launch Target** pose references are clearer than a single opaque spawn transform because the current scene
  authors those roots independently.
- Assumption: reset on `GameplayStateChanging` is acceptable because existing event semantics already define `Changing` as about-to-change.
- No design question remains unresolved from the grill session: terminology, ownership, reset order, geometry refresh, authored provider contents,
  rotation, Rigidbody baseline, transition boundary, testing scope, and ADR scope are settled.
