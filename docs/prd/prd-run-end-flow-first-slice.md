## Problem Statement

The current **Run** can continue after it should be over. The **Launch Target** can lose meaningful momentum, fall below the playable course, hit an
obstacle, or reach the level end without one central runtime flow producing a single reliable **Run Result** and returning the game to **Pre-Launch**.

This makes playtesting difficult. Designers cannot tune finish, obstacle, safety-net, and lost-momentum behavior against consistent result data, and
developers cannot rely on one authority for `Running -> RunEnded -> PreLaunch` lifecycle. The issue is especially sensitive because the game is a
forward-downhill casual run: distance and momentum must be measured as downhill progress, not raw world displacement, raw path length, camera
direction, or slingshot aim direction.

The first slice must also avoid overbuilding result presentation. For now, the game should log one **Run Result** and proceed to a new **Run** by
returning to **Pre-Launch**. Score UI, progression, analytics, coins, result screens, and side/off-course boundaries are separate future work.

## Solution

Implement the first **Run End Flow** slice.

The solution adds a shared **Run Progress Frame** model for forward-downhill progress. A designer-authored scene source supplies the course axes, and
runtime services snapshot those axes at `LaunchApplied`. The **Launch Target** position at `LaunchApplied` is the progress origin. A shared
`RunProgressService` owns the immutable snapshot and maximum forward-progress state so **Run End Flow**, **Lost Momentum**, camera fallback, and
future steering constraints do not each calculate progress differently.

The solution adds explicit **Run Contact Category** authoring for first-slice contacts: **Run Surface**, **Run Obstacle**, **Run Safety Net**, and
**Run Finish**. A shallow target-side contact notifier copies Unity collision/trigger data into immutable payloads. A pure classifier interprets those
payloads into run-ending candidates. **Run Obstacle** ends the run only on **Obstacle Impact**, measured by contact-normal impact speed. **Run Safety
Net** and **Run Finish** are explicit trigger contacts. **Run Surface** never creates a run-ending candidate, and ramp-shaped traversal geometry is
treated as **Run Surface**.

**Lost Momentum** is a separate detector. It starts after launch grace and ends the run only when both course-planar speed and new maximum forward
progress stay below configured thresholds for a sustained configured window. It does not depend on grounded or airborne state, because ramps and jumps
are valid gameplay.

**Run End Flow** accepts run-ending candidates, resolves same-fixed-tick candidates by priority, produces one **Run Result**, logs it with Unity
`Debug.Log`, transitions `Running -> RunEnded`, then transitions `RunEnded -> PreLaunch` after the configured transient delay. It resets only its own
candidate/result latch. Existing owners handle target reset, steering deactivation, camera deactivation, and slingshot capture re-enable through state
changes.

## Unity Surfaces

- Runtime assemblies and asmdefs:
  - Gameplay runtime assembly gains **Run End Flow**, **Run Progress Frame**, **Run Contact Category**, **Lost Momentum**, and **Run Result** runtime
    types.
  - Gameplay runtime assembly gains shallow Unity adapters for authored progress frame data, target motion data, target contact notification, and
    contact metadata.
  - Gameplay runtime assembly gains plain C# services/controllers for progress, classification, lost-momentum detection, and result arbitration.
  - Gameplay runtime composition registers the new services through VContainer and wires them to existing **Gameplay State**, launch-applied, and
    fixed-tick lifecycles.
  - Slingshot runtime remains the source of `LaunchApplied`; it does not own run ending.
  - Camera runtime remains Cinemachine-based per ADR-0009; first slice may use **Run Progress Frame** only as camera yaw fallback/bias behavior if
    included with the run-end implementation.
- Editor assemblies, windows, inspectors, importers, or menu items:
  - No custom Editor window, importer, or menu item is required for the first slice.
  - Optional gizmo drawing for run-contact placeholders is allowed only if it stays shallow and scene-authoring focused.
  - Existing editor validation patterns may be extended through MonoBehaviour `OnValidate` warnings where useful.
- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:
  - Gameplay Scene gains one authored **Run Progress Frame** source.
  - Gameplay Scene gains a `Run Contacts` organizational root.
  - Gameplay Scene gains designer-visible placeholders for **Run Safety Net**, **Run Finish**, and **Run Obstacle**.
  - **Run Safety Net** and **Run Finish** are trigger volumes with explicit run-contact metadata.
  - **Run Obstacle** uses a physical collider by default and has run-contact metadata on the same GameObject as the collider.
  - Existing level traversal surface is marked as **Run Surface** where needed.
  - Scene placeholders are project-owned wrapper objects; optional plugin assets may be visual children only.
  - Gameplay composition gains references to run-end config, progress-frame source, motion source, and contact notifier as needed.
  - A `RunEndConfig` ScriptableObject owns first-slice run-end tuning.
  - Existing `RunEnded` state and `Running -> RunEnded -> PreLaunch` transitions are reused.
  - No new Unity tags are required.
  - New gameplay layers are optional for filtering/performance only; tags and layers are not the semantic source of truth.
- RPC/helper commands, hooks, or shell wrappers:
  - Unity AI Agent Connector compile and test commands remain the verification path.
  - No new helper command is required.
- Package versioning, changelog, and installation/sync behavior:
  - This is project gameplay work, not a distributable package release.
  - No package dependency change is expected for this slice.
  - No save-data migration is expected.

## User Stories

1. As a player, I want the **Run** to end when I reach the level finish, so that a successful run has a clear result.
2. As a player, I want the **Run** to end when I fall below the course, so that the game does not continue forever after a fall.
3. As a player, I want the **Run** to end when I hit a real obstacle hard, so that crashes feel recognized by the game.
4. As a player, I want light obstacle scrapes to be bearable, so that small side contacts do not feel unfair.
5. As a player, I want the **Run** to end when I have stopped making meaningful progress, so that I can quickly try again.
6. As a player, I want the game to return to **Pre-Launch** after a run ends, so that I can start the next run without a result screen yet.
7. As a player, I want downhill distance to reflect how far I got forward, so that bouncing backward after a good run does not erase progress.
8. As a player, I want sideways sliding not to inflate distance, so that the result matches downhill progress.
9. As a player, I want jumping from ramps to remain valid gameplay, so that airborne motion is not treated as being stopped.
10. As a player, I want ramp-shaped geometry to behave like traversal surface, so that jumps and redirects do not end the run.
11. As a designer, I want one authored **Run Progress Frame**, so that downhill-forward progress is explicit in the level.
12. As a designer, I want **Run Progress Frame** axes to be independent from slingshot aim, so that lateral pulls do not corrupt distance.
13. As a designer, I want **Run Progress Frame** axes to be independent from camera direction, so that presentation does not define gameplay progress.
14. As a designer, I want **Run Progress Frame** axes to be snapshotted per run, so that runtime progress stays stable during a run.
15. As a designer, I want the progress origin to be the **Launch Target** position at launch, so that the progress-frame object can be placed for
    authoring convenience.
16. As a designer, I want one editable **Run Safety Net** volume below the course, so that falling below the level ends the run.
17. As a designer, I want one editable **Run Finish** trigger, so that the level end can be moved without code.
18. As a designer, I want initial **Run Obstacle** placeholders, so that obstacle-hit behavior can be tested immediately.
19. As a designer, I want run-contact metadata on the same object as the collider, so that classification is obvious in the hierarchy.
20. As a designer, I want placeholders grouped under `Run Contacts`, so that run-ending authoring is easy to find.
21. As a designer, I want **Run Surface** to include ramp-shaped traversal geometry, so that I do not need a separate ramp category in the first slice.
22. As a designer, I want **Run Safety Net** to map to `OutOfBounds`, so that result reasons stay player-facing.
23. As a designer, I want side/off-course boundaries deferred, so that the first slice does not guess the final course width.
24. As a designer, I want run-end thresholds in one config asset, so that tuning is discoverable.
25. As a designer, I want obstacle impact threshold tuning, so that crash strictness can be adjusted.
26. As a designer, I want lost-momentum launch grace tuning, so that early launch behavior does not end too soon.
27. As a designer, I want lost-momentum speed and progress thresholds, so that "stopped" can be tuned against the level slope.
28. As a designer, I want transient **RunEnded** delay tuning, so that reset timing can be adjusted before result UI exists.
29. As a developer, I want **Run End Flow** to be the single authority for ending a **Run**, so that contacts and momentum cannot produce competing
    results.
30. As a developer, I want **Run End Flow** to latch after accepting a result, so that later collision aftermath cannot produce duplicate logs.
31. As a developer, I want same-fixed-tick candidates resolved by priority, so that finish, obstacle, safety net, and momentum conflicts are
    deterministic.
32. As a developer, I want `Finished` to outrank `ObstacleHit`, so that crossing the finish and touching something in the same tick is successful.
33. As a developer, I want `ObstacleHit` to outrank `OutOfBounds`, so that a crash before falling is recorded as a crash.
34. As a developer, I want `OutOfBounds` to outrank `LostMomentum`, so that falling below the course is not mislabeled as stopping.
35. As a developer, I want **Run Result** to include reason, success flag, elapsed time, distance, final position, and final speed, so that later UI can
    consume stable data.
36. As a developer, I want success derived from `Finished`, so that result semantics are simple in the first slice.
37. As a developer, I want **Run Result** logged directly with Unity logging, so that no analytics or structured logging abstraction is introduced yet.
38. As a developer, I want `RunProgressService` to own progress state, so that consumers do not duplicate progress math.
39. As a developer, I want `RunProgressService` instead of "Tracker" naming, so that the runtime service does not sound like analytics.
40. As a developer, I want `RunProgressFrameSource` to be the scene adapter name, so that authored source and runtime snapshot are not confused.
41. As a developer, I want `RunProgressFrameSnapshot` to be immutable per run, so that distance and momentum use stable axes.
42. As a developer, I want invalid progress-frame authoring to fail loud, so that bad distance results are not hidden.
43. As a developer, I want invalid progress-frame snapshots to suppress **Run Result** production, so that fake-distance results do not pollute logs.
44. As a developer, I want `IRunMotionSource`, so that run-end logic does not depend on camera-named source interfaces.
45. As a developer, I want the same Rigidbody adapter to be able to expose camera and run-motion interfaces where appropriate, so that scene wiring
    stays simple without conflating interface names.
46. As a developer, I want `RigidbodyContactNotifier` to copy Unity callback data into immutable payloads, so that gameplay services do not hold raw
    `Collision` objects.
47. As a developer, I want `RunContactClassifier` to emit candidates only, so that it does not own result priority or state transitions.
48. As a developer, I want **Lost Momentum** detection outside `RunContactClassifier`, so that motion-based stopping is not mixed with contact
    classification.
49. As a developer, I want contact callbacks to queue signals and fixed tick to resolve results, so that physics callbacks do not transition gameplay
    state directly.
50. As a developer, I want `RunContact` to be one shallow metadata component with a category enum, so that authoring stays simple.
51. As a developer, I want first-slice `RunContactCategory` enum values to be exactly `Surface`, `Obstacle`, `SafetyNet`, and `Finish`, so that
    speculative categories do not leak into code.
52. As a developer, I want `RunEndReason` values to be `Finished`, `ObstacleHit`, `OutOfBounds`, and `LostMomentum`, so that result reasons stay
    stable and player-facing.
53. As a developer, I want **Run Safety Net** to produce `OutOfBounds`, so that authored contact categories remain separate from result reasons.
54. As a developer, I want `RunEndConfig` to centralize thresholds, so that tuning does not spread across multiple services.
55. As a developer, I want **Run End Flow** to reset only its own latch state, so that reset ownership stays with existing systems.
56. As a developer, I want existing steering and camera systems to deactivate when leaving Running, so that run-ending work does not duplicate those
    responsibilities.
57. As a developer, I want existing **Gameplay Flow** to re-enable slingshot capture on **Pre-Launch**, so that the **Launch Target** reset remains in
    the current ownership boundary.
58. As a developer, I want the implementation to reuse existing `RunEnded` state assets and transitions, so that the state model remains authored.
59. As a developer, I want no score UI in this slice, so that gameplay correctness ships before presentation.
60. As a developer, I want no coins, multipliers, best-distance, or progression in this slice, so that the result model stays small.
61. As a tester, I want EditMode tests for progress math, so that max forward progress is independent of final displacement and path length.
62. As a tester, I want EditMode tests for invalid progress-frame snapshots, so that broken authoring fails loudly.
63. As a tester, I want EditMode tests for **Lost Momentum**, so that sustained low speed and low progress are required together.
64. As a tester, I want EditMode tests for launch grace, so that the run does not end immediately after launch.
65. As a tester, I want EditMode tests for obstacle impact, so that contact-normal speed threshold behavior is deterministic.
66. As a tester, I want EditMode tests for trigger categories, so that **Run Safety Net** and **Run Finish** map to the correct reasons.
67. As a tester, I want EditMode tests for candidate priority, so that same-tick conflicts produce one expected result.
68. As a tester, I want EditMode tests for latching, so that one run cannot log multiple results.
69. As a tester, I want PlayMode composition tests for the Gameplay Scene, so that required references and placeholders are present.
70. As a tester, I want PlayMode composition tests for **Run Progress Frame** validity, so that scene authoring errors are caught before playtesting.
71. As a tester, I want PlayMode composition tests for contact metadata on collider objects, so that classification does not depend on ambiguous parent
    lookup.
72. As a tester, I want PlayMode composition tests for the player-side contact notifier and motion source, so that the **Launch Target** can report run
    contacts and motion.
73. As a maintainer, I want the PRD to respect ADR-0002 and ADR-0006, so that MonoBehaviours stay shallow and logic stays in plain C# services.
74. As a maintainer, I want the PRD to respect ADR-0005, so that runtime services are explicitly composed through VContainer.
75. As a maintainer, I want the PRD to respect ADR-0003, so that local direct events are used before introducing a bus.
76. As a maintainer, I want the PRD to respect ADR-0004, so that structured logging remains deferred.
77. As a maintainer, I want camera fallback implications to align with ADR-0009, so that **Run Camera** behavior remains Cinemachine-based.

## Implementation Decisions

- Use the project glossary terms **Run**, **Run End Reason**, **Run Result**, **Run End Flow**, **Run Progress Frame**, **Run Contact Category**,
  **Run Surface**, **Run Obstacle**, **Obstacle Impact**, **Lost Momentum**, **Run Safety Net**, **Run Finish**, **Launch Target**, and
  **Pre-Launch**.
- First-slice **Run End Reason** values are `Finished`, `ObstacleHit`, `OutOfBounds`, and `LostMomentum`.
- First-slice **Run Contact Category** values are exactly `Surface`, `Obstacle`, `SafetyNet`, and `Finish`.
- Do not add **Run Boundary** to the first-slice enum, scene placeholders, or required composition tests.
- Ramp-shaped traversal geometry is **Run Surface** unless future ramp-specific gameplay events require a separate category.
- **Run Safety Net** produces `RunEndReason.OutOfBounds`; do not add `RunEndReason.SafetyNet`.
- **Run Surface** produces no run-ending candidate.
- **Run Finish** produces a `Finished` candidate.
- **Run Safety Net** produces an `OutOfBounds` candidate.
- **Run Obstacle** produces an `ObstacleHit` candidate only when the contact qualifies as **Obstacle Impact**.
- **Obstacle Impact** is gated by contact-normal impact speed in the first slice.
- Course-forward direction may be used for tuning or log context, but it must not prevent a real non-forward crash from ending a run.
- **Run End Flow** is the single authority that accepts candidates, creates one **Run Result**, logs the result, and transitions state.
- **Run End Flow** collects candidates during the current fixed tick and resolves conflicts by priority: `Finished > ObstacleHit > OutOfBounds >
  LostMomentum`.
- **Run End Flow** latches after accepting one result and ignores later candidates until the next `LaunchApplied`.
- **Run End Flow** resolves candidates on fixed tick, not directly inside collision or trigger callbacks.
- First slice logs **Run Result** directly through Unity logging and does not introduce an analytics, telemetry, or result-sink abstraction.
- First **Run Result** payload contains reason, success flag, elapsed time, distance travelled, final position, and final speed.
- `Finished` is the success reason in the first slice; all other first-slice reasons are non-success results.
- `DistanceTravelled` means maximum forward progress reached along the **Run Progress Frame** from the launch start position.
- `DistanceTravelled` is not final displacement, path length, raw world-Z movement, camera-forward movement, or slingshot aim direction.
- The **Run Progress Frame** is sourced from a designer-authored scene object.
- Runtime services consume an immutable `RunProgressFrameSnapshot` captured at `LaunchApplied`.
- Runtime services do not read the authored progress-frame Transform live every tick during a run.
- The progress origin is the **Launch Target** position at `LaunchApplied`; the authored frame supplies axes only.
- `SlingshotLaunchRequest.LaunchDirection` must not define progress because it includes lateral **Pull Offset**.
- Velocity must not define progress axes because it can wobble during impacts, jumps, and slides.
- Camera direction must not define progress axes because camera presentation is not gameplay progress.
- If the authored **Run Progress Frame** is missing or invalid, runtime fails loud with a clear error.
- If the `RunProgressFrameSnapshot` is invalid at `LaunchApplied`, **Run End Flow** skips producing a **Run Result** for that run rather than logging
  partial or fake-distance results.
- Name the scene adapter `RunProgressFrameSource`.
- Name the immutable per-run value `RunProgressFrameSnapshot`.
- Name the shared runtime progress owner `RunProgressService` / `IRunProgressService`.
- Avoid "Tracker" naming for the progress service because it can imply analytics/user/session data gathering.
- `RunProgressService` owns snapshot validity, current forward progress, and maximum forward progress.
- `RunProgressService` should expose progress data through a simple read interface so consumers do not duplicate progress math.
- Add a gameplay-facing `IRunMotionSource` with position and linear velocity.
- Keep `IRunCameraSource` camera-specific even if the same Rigidbody adapter implements both camera and run-motion interfaces.
- **Lost Momentum** detection is a separate injected service, not part of `RunContactClassifier`.
- **Lost Momentum** starts after a configured launch grace period.
- **Lost Momentum** requires both low course-planar speed and almost no new maximum forward progress for a sustained configured window.
- **Lost Momentum** remains purely motion/progress based in the first slice and does not depend on grounded or airborne state.
- `RunEndConfig` is a ScriptableObject that owns first-slice run-end tuning.
- `RunEndConfig` includes obstacle impact threshold, lost-momentum launch grace, lost-momentum duration, low speed threshold, low progress threshold,
  and transient **RunEnded** delay.
- `RunContact` is one shallow MonoBehaviour with a serialized `RunContactCategory` enum.
- `RunContact` metadata lives on the same GameObject as the Collider or Trigger that reports the contact.
- Parent groups such as `Run Contacts` are organizational only and are not the semantic source of contact category.
- Tags are not the semantic source of **Run Contact Category**.
- Layers are optional filtering/performance aids only.
- `RigidbodyContactNotifier` is the shallow target-side MonoBehaviour that raises copied contact/trigger notifications.
- `RigidbodyContactNotifier` copies Unity callback data into immutable payloads and does not expose raw mutable `Collision` callback objects.
- `RunContactClassifier` interprets contact notifications into zero or more run-ending candidates.
- `RunContactClassifier` does not own candidate priority, **Run Result** creation, logging, or gameplay state transitions.
- **Run Safety Net** and **Run Finish** are trigger volumes with explicit run-contact metadata.
- **Run Obstacle** uses physical colliders by default.
- Trigger obstacles are reserved for intentionally non-physical hazard volumes and are not required for the first slice.
- Scene placeholders live under a `Run Contacts` organizational root.
- First-slice scene placeholders are **Run Safety Net**, **Run Finish**, and **Run Obstacle**.
- Scene placeholders are project-owned wrapper objects. Optional assets under plugin folders may be visual/effect children only.
- `Running -> RunEnded -> PreLaunch` is the first-slice state path; do not bypass **RunEnded** even though presentation is deferred.
- Transient **RunEnded** lasts by configured delay, initially expected to be one fixed tick.
- **Run End Flow** resets only its own candidate/result latch.
- Existing systems own **Launch Target** reset, steering deactivation, camera deactivation, and slingshot capture re-enable through state changes.
- Existing **Gameplay Flow** re-enables slingshot capture on **Pre-Launch**, which holds and snaps the **Launch Target** to rest.
- Existing **PlayerSteeringController** and **RunCameraController** deactivate when leaving Running.
- Post-launch steering should eventually constrain touch input against the **Run Progress Frame** so the player can steer laterally but not intentionally
  turn back uphill.
- **Run Camera** yaw should continue following planar **Launch Target** velocity, with the **Run Progress Frame** as fallback/bias when velocity is too
  low or noisy.
- Score UI, result presentation, analytics, coins, multipliers, progression, best distance, and side/off-course boundaries are not part of this slice.

## Testing Decisions

- Good tests should assert observable behavior at service boundaries: emitted candidates, accepted result, logged result content, state transitions,
  progress values, and scene composition. They should not assert private fields or internal implementation order beyond documented lifecycle behavior.
- EditMode tests should cover `RunProgressService` progress math:
  - valid authored frame snapshot creates normalized axes;
  - invalid frame data fails loud;
  - progress origin is **Launch Target** position at `LaunchApplied`;
  - max forward progress increases with downhill motion;
  - final backward bounce does not reduce `DistanceTravelled`;
  - sideways movement does not inflate `DistanceTravelled`;
  - path length and vertical jump height do not inflate downhill distance.
- EditMode tests should cover `LostMomentumDetector`:
  - no detection before launch grace expires;
  - no detection while course-planar speed is above threshold;
  - no detection while max forward progress continues increasing;
  - detection only after both low speed and low progress persist for the configured duration;
  - airborne or grounded state is not required by the detector.
- EditMode tests should cover `RunContactClassifier`:
  - **Run Surface** emits no candidate;
  - **Run Finish** emits `Finished`;
  - **Run Safety Net** emits `OutOfBounds`;
  - **Run Obstacle** below normal-impact threshold emits no candidate;
  - **Run Obstacle** above normal-impact threshold emits `ObstacleHit`;
  - metadata must be taken from the collider/trigger object itself.
- EditMode tests should cover `RigidbodyContactNotifier` payload copying:
  - collision payload contains enough copied collider/contact/relative-velocity data for classification;
  - trigger payload contains the copied collider identity needed for classification;
  - gameplay services do not receive raw Unity callback objects.
- EditMode tests should cover **Run End Flow**:
  - result starts on `LaunchApplied`, not Running alone;
  - result includes reason, success flag, elapsed time, distance, final position, and final speed;
  - same-fixed-tick candidates resolve by agreed priority;
  - flow latches after the first accepted result;
  - later candidates are ignored until a new `LaunchApplied`;
  - invalid progress snapshot logs/fails loud and does not emit fake result;
  - `Running -> RunEnded` and delayed `RunEnded -> PreLaunch` transitions occur through the existing state service;
  - flow resets only its own latch state.
- EditMode tests should cover `RunEndConfig` validation:
  - thresholds and durations clamp or validate to non-negative sensible values;
  - missing config references fail composition validation.
- PlayMode scene composition tests should cover:
  - exactly one authored **Run Progress Frame** source is assigned to gameplay composition;
  - progress frame axes are valid and non-degenerate;
  - **Launch Target** has target-side contact notifier and run-motion source wiring;
  - the scene has a `Run Contacts` root;
  - **Run Safety Net**, **Run Finish**, and **Run Obstacle** placeholders exist;
  - **Run Safety Net** and **Run Finish** use trigger colliders with metadata on the same object;
  - **Run Obstacle** uses a physical collider with metadata on the same object;
  - scene composition does not require **Run Boundary**;
  - scene composition does not require a separate **Run Ramp** category.
- Prior art:
  - Controller lifecycle tests should follow existing EditMode tests for gameplay flow, player steering, and run camera controllers.
  - Composition validation tests should follow existing Gameplay Scene composition tests.
  - Rigidbody adapter PlayMode tests should follow existing Rigidbody launch, steering, and camera source tests.
- Shell/static checks:
  - Run `git diff --check` after edits.
  - Use Unity compile through the existing connector before test execution.
- Manual Unity smoke checks after automated tests:
  - launch and fall into **Run Safety Net**;
  - launch and reach **Run Finish**;
  - launch and collide hard with **Run Obstacle**;
  - launch and lightly scrape obstacle below threshold;
  - launch and let **Lost Momentum** trigger;
  - verify each case logs one result and returns to **Pre-Launch**.

## Release and Compatibility

- Unity version assumption: Unity 6000.3.18f1.
- Runtime package assumptions:
  - VContainer remains the dependency injection system.
  - Cinemachine 3.1.7 is already installed and remains the **Run Camera** solution per ADR-0009.
  - Unity Input System remains centralized behind project input adapters.
- This feature is project gameplay work, not a published package release.
- No package manifest update is expected.
- No player save migration is expected.
- Existing **Pre-Launch**, **Launch**, steering, and **Run Camera** behavior should remain backward-compatible except where run-ending state transitions
  intentionally reset the run after result logging.
- Existing authored gameplay state assets are reused. The first slice depends on already-authored `Running -> RunEnded -> PreLaunch` transitions.
- Backward compatibility risks:
  - Broken or missing scene authoring can now fail loud during run-start/result logic.
  - Existing surfaces without **Run Surface** metadata may be treated as unclassified contacts until scene authoring is updated.
  - Obstacle threshold defaults may need device playtesting to avoid false-positive or false-negative crashes.
  - **Lost Momentum** thresholds may need tuning for the actual slope angle and physics feel.

## Out of Scope

- Score UI.
- Result screen or **RunEnded** presentation.
- Analytics, telemetry, or structured result logging.
- Coins, multipliers, best distance, level progression, or rewards.
- Side/off-course **Run Boundary** authoring or enum values.
- Separate **Run Ramp** category.
- Grounded/airborne state detection for **Lost Momentum**.
- Full curved-course or spline-based progress.
- Custom camera collision, Cinemachine replacement, or camera Deoccluder expansion.
- Reworking slingshot launch math.
- Reworking existing steering implementation beyond documenting future progress-frame constraints.
- New global event bus.
- New custom Editor tooling beyond optional shallow gizmo/validation helpers.

## Further Notes

- Assumption: first-slice levels are effectively straight downhill, so one authored **Run Progress Frame** is enough. Curved courses should introduce a
  future path/spline progress model without changing **Run Result** terminology.
- Assumption: `Debug.Log` is acceptable for the first result output because structured logging and analytics are deferred.
- Assumption: initial placeholder positions can be rough and designer-adjusted after implementation.
- Assumption: plugin assets under existing plugin folders are optional visuals only; colliders and metadata remain on project-owned wrapper objects.
- Unresolved tuning: exact obstacle impact threshold, lost-momentum thresholds, launch grace, and transient **RunEnded** delay require playtesting.
- Unresolved tuning: steering constraints against the **Run Progress Frame** are documented as a gameplay implication but can be implemented after this
  first run-ending slice if needed.
