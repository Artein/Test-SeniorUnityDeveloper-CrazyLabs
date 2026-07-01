# PRD: Ladybug Half-Tube Run Course

## Problem Statement

The game has the core pieces for a downhill mobile Run: a Slingshot, a controllable Launch Target, character presentation, Run Surface probing, Run Progress Frame calculations, Run Contact Categories, Pickups, Run Preparation upgrades, Run End Flow, and a Cinemachine Run Camera. What it does not yet have is the actual authored Run Course that turns those systems into a complete replayable level.

From the player's perspective, the current experience needs a clear place to slide downhill, dodge readable rooftop obstacles, collect coins, use ramps, fail with useful partial progress, buy upgrades in Run Preparation, and eventually reach a visible Run Finish. The Run Course must not be a short prototype strip that can be completed on the first Run. It must support the intended repeated-attempt progression loop where early Runs earn coins, upgrades improve reach, and a competent player can finish after roughly 5-10 Runs.

From the team's perspective, the missing level is also a systems validation problem. The first full Run Course must prove that the current gameplay vocabulary works together: Run Progress Frame-relative distance, ForwardDownhillDegrees, Soft Containment, Lost Momentum, Run Obstacles, Run Safety Net, Run Finish, Pickups, and Upgrade-driven reach. If the level is built as arbitrary scene decoration or imported rooftop chunks with gameplay colliders hidden inside them, the implementation will be hard to test, tune, and safely extend.

## Solution

Build the first Ladybug-themed downhill Run Course as one continuous Paris rooftop half-tube. The course is authored relative to the Run Progress Frame forward axis, uses project-owned Run Surface geometry as the gameplay source of truth, and is dressed with imported Ladybug rooftop assets for theme and readability.

The first implementation should be a static-first graybox with final-enough structure:

- About 420 meters of forward progress.
- About 60 seconds Target Run Duration for a competent upgraded Run.
- Target First Completion Run Count of about 5-10 Runs.
- First Run expectation that a decent no-upgrade or starter-upgrade player usually fails before the required ramp and does not reach Run Finish.
- Five Run Progression Bands, each with a pacing and upgrade-pressure role.
- Fifteen continuous Run Course Sections, each with an explicit range, pitch target, surface purpose, obstacle role, and coin placement purpose.
- A wide banked half-tube profile that creates Soft Containment through geometry rather than hidden clamps or invisible walls.
- Static-first rooftop obstacle patterns with at least one readable safe or near-safe line.
- One required centered tutorial ramp and one optional risk/reward ramp, both authored as Run Surface.
- Safe baseline coins plus optional risk/reward coins that support Run Preparation upgrades without requiring risky collection.
- Run Safety Net coverage below the full course, side lips, ramp landings, and finish approach.
- A visible Run Finish in the final 410-420 meter region.

The first level should feel like a Sled Surfers-style repeated downhill reach game, adapted to a Ladybug Paris rooftop theme. It should not copy exact Sled Surfers layouts. The transferable principles are continuous sliding, readable obstacle dodging, ramp/air beats, coin collection, upgrade-driven reach, and meaningful partial success before first completion.

## Unity Surfaces

Runtime assemblies and asmdefs:

- Gameplay runtime surfaces that already own Run Progress Frame, Run Contact Categories, Run End Flow, Lost Momentum, Run Surface Context, player steering, Run Camera, Slingshot handoff, and scene composition.
- Pickup runtime surfaces that already own Pickup Definitions, Pickup collection, Level Pickup State, Currency Grants, and Run Currency Accumulator integration.
- Upgrade runtime surfaces that already own Upgrade Definitions, Upgrade Catalog, Run Modifier Snapshot, Gameplay Stat Resolver, and gameplay stat ids such as SlingshotLaunchPower, PlayerMaxSpeed, PlayerSteeringResponsiveness, and CoinPickupMultiplier.
- Economy runtime surfaces that own Currency Definitions, Currency Storage, and purchase currency behavior.
- Slingshot runtime surfaces should be consumed through existing gameplay stat resolution and launch config behavior. The Run Course should not add direct Slingshot-to-upgrade coupling.
- If new runtime validation or authoring support is needed, it should be placed where dependency direction stays clean and plain C# logic remains testable without Unity lifecycle callbacks.

Editor assemblies, windows, inspectors, importers, or menu items:

- Existing scene or prefab authoring may be enough for the first slice.
- If the implementation adds authoring validation, it should live in Editor-only surfaces unless runtime validation is genuinely needed during gameplay.
- Any new helper should validate authored Unity objects and report clear course-authoring errors rather than silently fixing scene content.
- No new import pipeline is required for the first Run Course.

Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:

- The Gameplay Scene is the expected integration surface for the first playable Run Course.
- The current prototype Surface, Run Contacts, Run Obstacle, Run Safety Net, and Run Finish placeholders will need to become real level content or be replaced by real level content.
- Project-owned Run Surface meshes/colliders should be the gameplay source of truth.
- Imported Ladybug rooftop meshes should be visual dressing, not authoritative Run Surface geometry.
- Static obstacle wrappers should expose Run Obstacle meaning through project-owned collider/contact authoring.
- Ramps should be authored as Run Surface, not Run Obstacle.
- Run Finish should be a clear finish contact/trigger near the final 410-420 meter region.
- Run Safety Net should cover the playable course below the trough, side lips, ramp landings, optional escape areas, and finish approach.
- Pickup prefabs and Pickup Definitions should be reused for coin placement.
- Upgrade Catalog and gameplay stat id assets are tuning inputs, not new level-specific upgrade definitions for this PRD.
- Existing camera config and run camera rig may need tuning if the full 420 meter course exposes framing, decollision, or follow issues.
- ProjectSettings should not require changes for the first graybox unless physics layers, tags, or collision matrix setup are demonstrably missing.
- Package manifest changes are not expected.

RPC/helper commands, hooks, or shell wrappers:

- Use the Unity AI Agent Connector compile gate for later implementation work.
- Use targeted Unity Test Framework runs after compilation is clean.
- No new RPC command contract is required by this PRD.
- If a course validation helper is added, it can be run from tests or Editor validation rather than a new shell command unless repeated local QA proves that a command wrapper saves meaningful time.

Package versioning, changelog, and installation/sync behavior:

- This is project game content, not a reusable Unity package release.
- No package version bump is expected.
- No changelog entry is required unless the project later formalizes content-release notes.
- No install or sync behavior changes are expected.

## User Stories

1. As a player, I want a continuous downhill Run Course, so that each Run feels like one coherent slide rather than disconnected prototype pieces.
2. As a player, I want the Run Course to be readable immediately after Launch, so that I understand the downhill direction and where to steer.
3. As a player, I want the first 20 meters to be calm, so that I can settle into movement before obstacles appear.
4. As a player, I want early center coins, so that my first Run feels productive even if I fail quickly.
5. As a player, I want gentle early lateral coin trails, so that I learn steering through rewards rather than punishment.
6. As a player, I want the half-tube banks to guide me back toward the course, so that recovery feels physical and fair.
7. As a player, I want excessive side speed to carry risk, so that riding high on the banks feels exciting rather than consequence-free.
8. As a player, I want no invisible side clamps, so that steering outcomes feel consistent with the visible surface.
9. As a player, I want no invisible hard walls on the side lips, so that escape and failure are understandable.
10. As a player, I want a Run Safety Net below the course, so that falling out ends the Run cleanly instead of leaving me stuck.
11. As a player, I want the first obstacle to appear only after I have learned steering, so that failure does not feel unfair.
12. As a player, I want the first obstacle to have wide left and right gaps, so that I understand obstacles are line-choice challenges.
13. As a player, I want every obstacle cluster to preserve at least one obvious safe line, so that skilled play is about reading and steering.
14. As a player, I want risky coin lines near tighter gaps, so that choosing danger has a visible reward.
15. As a player, I want safe coin lines around obstacles, so that basic progression does not require risky play.
16. As a player, I want recovery space after each pressure beat, so that one dodge does not immediately chain into an unreadable failure.
17. As a player, I want a required centered tutorial ramp, so that airtime becomes part of the core Run fantasy.
18. As a player, I want the required ramp to have a visible coin arc, so that I understand the intended takeoff line.
19. As a player, I want the required ramp landing to be broad and obstacle-free, so that the first jump teaches success rather than precision failure.
20. As a player, I want ramp landings to return me to the half-tube, so that airtime does not throw me into unfair side exits.
21. As a player, I want later bank lines to offer richer coins, so that mastery has an economy reward.
22. As a player, I want an optional risk/reward ramp later in the Run, so that confident play can earn more without blocking completion.
23. As a player, I want a safe center fallback around the optional ramp, so that I can choose consistency over risk.
24. As a player, I want a reach-pressure glide section, so that under-upgraded Runs naturally lose momentum instead of hitting scripted fail gates.
25. As a player, I want first Runs to end after useful progress, so that failure still gives me coins and a reason to upgrade.
26. As a player, I want early upgrades to noticeably improve reach, so that repeated Runs feel meaningful.
27. As a player, I want PlayerMaxSpeed upgrades to help me travel farther, so that speed progression maps to course reach.
28. As a player, I want SlingshotLaunchPower upgrades to help me reach the first ramp region, so that stronger Launches matter.
29. As a player, I want PlayerSteeringResponsiveness upgrades to improve control and risky coin access, so that steering progression feels useful.
30. As a player, I want CoinPickupMultiplier upgrades to accelerate later progression, so that collection investment pays off.
31. As a player, I want the first completion to happen after several upgraded Runs, so that the finish feels earned.
32. As a player, I want a competent upgraded completion to take about one minute, so that the level has enough duration for mobile replay without dragging.
33. As a player, I want the Run Finish to be visible before I reach it, so that completing the Run feels intentional.
34. As a player, I want the final 30 meters to avoid new hazard types, so that I can focus on finishing.
35. As a player, I want the finish approach coin line to funnel toward Run Finish, so that rewards reinforce completion.
36. As a player, I want rooftop obstacles to match the Ladybug Paris theme, so that the level feels integrated with the character.
37. As a player, I want obstacle silhouettes to be easy to read at speed, so that dodging depends on skill rather than guessing.
38. As a player, I want the Run Camera to frame the half-tube, ramps, obstacles, and finish clearly, so that my decisions are visible.
39. As a player, I want camera collision to avoid rooftop props blocking the view, so that the course remains readable.
40. As a player, I want the Ladybug character to slide and jump through the level without presentation glitches, so that the course feels polished.
41. As a designer, I want a frozen 15-section first graybox plan, so that implementation can start without reopening layout scope.
42. As a designer, I want Run Progression Bands tied to distance and upgrade pressure, so that reach tuning is organized.
43. As a designer, I want Band 1 to teach center movement and light banking, so that onboarding happens through course shape.
44. As a designer, I want Band 2 to introduce simple obstacles, so that early upgraded reach has a clear skill check.
45. As a designer, I want Band 3 to introduce the required ramp and faster reads, so that mid-run play expands the vocabulary.
46. As a designer, I want Band 4 to create late upgrade pressure, so that under-upgraded players can fail by Lost Momentum.
47. As a designer, I want Band 5 to focus on finish readability, so that the first completion is not undermined by late novelty.
48. As a designer, I want every Run Course Section to have a known range, pitch, feature, obstacle role, and coin purpose, so that graybox authoring is traceable.
49. As a designer, I want section ranges to remain continuous and non-overlapping, so that progress tuning is easy to reason about.
50. As a designer, I want pitch targets to be authored per section, so that speed and reach pressure are visible in the geometry.
51. As a designer, I want average pitch to start near 7 degrees, so that the first course has a simple speed baseline.
52. As a designer, I want 3-5 degree glide sections, so that Lost Momentum can appear through physical reach pressure.
53. As a designer, I want 10-12 degree pitch limited to short acceleration beats, so that early reach is not accidentally overpowered.
54. As a designer, I want ramp sections separated from new obstacle introductions, so that players learn one demand at a time.
55. As a designer, I want no blockers immediately before or after ramps, so that ramp success is not punished unfairly.
56. As a designer, I want 65-75 percent of reachable coins on safe or near-safe lines, so that progression is reliable.
57. As a designer, I want 25-35 percent of reachable coins on risk/reward lines, so that mastery has extra value.
58. As a designer, I want Band 1 and Band 2 coins to fund at least one early reach-upgrade path, so that the first failure can lead to a meaningful purchase.
59. As a designer, I want risk coins to speed progression but not gate it, so that casual players are not forced into expert lines.
60. As a designer, I want imported rooftop chunks to dress the course edges and skyline, so that the theme is rich without corrupting gameplay collision.
61. As a designer, I want rooftop blockers to use AC units, sunroofs, solar panels, satellite dishes, water tanks, billboards, and roof exits, so that obstacles stay theme-consistent.
62. As a designer, I want rotating-gate assets deferred as moving gameplay, so that the static readability gates pass first.
63. As a designer, I want the course to avoid branching routes, so that progression, camera, and upgrade reach remain tunable.
64. As a designer, I want local line choices inside the half-tube, so that the player still has expression without route complexity.
65. As a designer, I want failed validation gates treated as layout problems first, so that art polish does not hide gameplay issues.
66. As a technical artist, I want the half-tube Run Surface to be project-owned geometry, so that collider shape and visual dressing can be controlled separately.
67. As a technical artist, I want imported Ladybug meshes to remain visual-first, so that vendor asset structure does not define gameplay behavior.
68. As a technical artist, I want obstacle wrappers to own gameplay colliders, so that mesh detail does not create unfair contact shape.
69. As a technical artist, I want ramps to have simple readable collision, so that the Launch Target launches and lands predictably.
70. As a technical artist, I want side lips and bank transitions to be smooth, so that Soft Containment feels physical.
71. As a technical artist, I want visible rooftop dressing outside the playable line, so that the course feels like Paris rooftops without cluttering the safe path.
72. As a technical artist, I want the final finish marker to stand out in the level, so that the player recognizes completion.
73. As a gameplay programmer, I want the Run Course to use existing Run Contact Categories, so that run-ending behavior stays centralized.
74. As a gameplay programmer, I want Run Surface, Run Obstacle, Run Safety Net, and Run Finish authoring to remain explicit, so that classification is testable.
75. As a gameplay programmer, I want ramps to remain Run Surface, so that the Run Contact Classifier does not need a Ramp category.
76. As a gameplay programmer, I want side escape to resolve through Run Safety Net, so that out-of-bounds remains one clear Run End Reason path.
77. As a gameplay programmer, I want Lost Momentum to remain the primary under-upgraded failure mode, so that early reach is governed by movement/progression rather than forced collision.
78. As a gameplay programmer, I want Run Progress Frame-relative distance to drive band validation, so that world-axis assumptions do not leak into gameplay decisions.
79. As a gameplay programmer, I want ForwardDownhillDegrees sampled on center, left bank, and right bank surfaces, so that side riding does not break slope-aware systems.
80. As a gameplay programmer, I want upgrade effects consumed through existing gameplay stat resolution, so that the level does not depend on upgrade storage directly.
81. As a gameplay programmer, I want pickup placement to reuse existing Pickup prefabs and definitions, so that coin collection and multipliers stay consistent.
82. As a gameplay programmer, I want Level Pickup State references updated for the authored course, so that collected coins are tracked for the Level Session.
83. As a gameplay programmer, I want any course validation logic to be a deep, testable module, so that authoring checks do not become scattered scene assertions.
84. As a gameplay programmer, I want any Unity-facing authoring helper to be shallow and serialized, so that MonoBehaviours do not own hidden gameplay policy.
85. As a gameplay programmer, I want no new static service state, so that the course follows the repo's dependency and testability rules.
86. As a gameplay programmer, I want no public save-format change for this level, so that content work does not block persistence decisions.
87. As a QA engineer, I want a composition test that the Gameplay Scene has real Run Surface, Run Safety Net, Run Finish, and obstacle contacts, so that placeholders do not ship accidentally.
88. As a QA engineer, I want a check that no Boundary or Ramp Run Contact Category appears, so that glossary decisions remain enforced.
89. As a QA engineer, I want PlayMode checks for safety net coverage, so that side-lip and ramp-fall failures end cleanly.
90. As a QA engineer, I want PlayMode or authoring checks for pickup references, layers, triggers, and definitions, so that coins work in the full course.
91. As a QA engineer, I want PlayMode checks for scene DI after the level is added, so that level content does not break GameplayLifetimeScope wiring.
92. As a QA engineer, I want slope sample checks for representative sections, so that pitch targets are not accidentally inverted or flattened.
93. As a QA engineer, I want a manual no-upgrade run pass, so that first-run failure lands in the intended Band 2 range.
94. As a QA engineer, I want a manual upgraded run pass, so that completion time lands near 55-65 seconds.
95. As a QA engineer, I want a manual ramp safety pass, so that takeoff, airtime, landing, and camera framing feel readable.
96. As a QA engineer, I want a manual obstacle readability pass, so that safe lines are visible before impact.
97. As a QA engineer, I want a manual coin distribution pass, so that safe/risk collection matches the intended economy split.
98. As a QA engineer, I want visual smoke checks for the Ladybug theme, so that rooftop dressing supports gameplay instead of obscuring it.
99. As a producer, I want the scope limited to one static-first Run Course, so that implementation can finish and validate quickly.
100. As a producer, I want moving obstacles deferred, so that schedule risk stays focused on the core sliding/reach loop.
101. As a producer, I want final coin values and exact upgrade math treated as tuning inputs, so that the level can integrate with the separate upgrade branch.
102. As a producer, I want the PRD to distinguish authored layout from future polish, so that the next implementation branch has a clear definition of done.

## Implementation Decisions

The first implementation is one continuous Run Progress Frame-relative Run Course, not multiple stages, not a route graph, and not a branching level. The current Gameplay Scene forward axis is expected to align with world +Z for the first graybox, but the gameplay meaning must remain Run Progress Frame-relative.

The first graybox uses the frozen 15-section layout from the design draft as its source of truth. Section ranges may move during physics validation, but implementation should scale and tune the existing section plan before adding new beats.

The course is approximately 420 meters long. The target upgraded completion time is about 60 seconds, with an acceptable validation window of about 55-65 seconds.

The Target First Completion Run Count is about 5-10 Runs. The first Run should not normally reach Run Finish. A decent no-upgrade or starter-upgrade Run should usually reach Band 2 around 85-125 meters, collect useful coins, and fail before the required ramp near 170 meters.

Under-upgraded failure should be caused primarily by Lost Momentum and insufficient reach. Obstacles can end Runs through Obstacle Impact when the player clearly hits them, but they must not become mandatory fail gates used to enforce progression.

The half-tube is the main Run Surface shape. The first graybox target is about 10 meters total playable width, with a 6 meter comfortable center trough, 2 meter bank per side, 25-35 degree banks, and 1.0-1.4 meter lip height.

Soft Containment is achieved through visible Run Surface geometry. Hidden lateral clamps, invisible side walls, and forced recenter volumes are explicitly disallowed for primary containment.

High lateral speed may let the Launch Target crest a side lip. That is acceptable and should resolve through falling toward Run Safety Net, not through invisible correction.

The pitch model is authored by Run Course Section. The starting tuning model is 3-5 degrees for Launch settle and reach-pressure glide, 6-8 degrees for normal sliding, 8-10 degrees for ramp setup or faster beats, and 10-12 degrees only for short takeoff or acceleration segments.

Pitch targets are center-trough safe-line targets, not guarantees that every point on every bank has identical ForwardDownhillDegrees. Representative center, left bank, and right bank samples must still preserve useful downhill behavior.

Project-owned Run Surface geometry is authoritative for movement, slope, ramp, and collision behavior. Imported rooftop chunks do not define the gameplay surface.

Imported Ladybug rooftop content is used for visual dressing, silhouettes, and thematic obstacle meshes. Gameplay collider shape and Run Contact Category authoring live on project-owned wrappers or project-owned child objects.

Static-first obstacles are the only required obstacle behavior for the first implementation. Moving or rotating obstacle gameplay is deferred until static readability, reach pacing, ramp safety, safety net coverage, and coin distribution gates pass.

Obstacle authoring should use Run Obstacle contacts only where impact can reasonably end the Run. Visual-only rooftop props outside the playable line should not participate in Run Contact classification.

Every obstacle cluster must preserve at least one obvious safe or near-safe line. Early obstacle pressure is lateral and readable, not vertical, timing-based, or hidden.

The first blocking obstacle appears in Band 2 after the player has had enough distance to learn steering. Band 1 has no blocking obstacle pressure.

The required tutorial ramp is centered, forgiving, and authored as Run Surface. It has no blocker immediately before, on, or immediately after the landing.

The optional later ramp is a risk/reward line with a viable center fallback. It rewards coins or expression but does not gate completion.

Ramp coin arcs communicate ramp intent before takeoff. Ramp landings are broad and return the Launch Target toward the half-tube rather than toward side exits.

Run Safety Net coverage is required below the trough, side lips, ramp landings, optional escape areas, and finish approach. Safety net behavior should stay aligned with the existing Run Safety Net contact category and OutOfBounds Run End Reason.

Run Finish is a visible contact region in the final 410-420 meter region. The final 30 meters should introduce no new hazard type and should funnel the player toward completion.

Coins use existing Pickup behavior and existing coin Pickup Definitions. The Run Course should place pickups; it should not create a parallel collection system.

Reachable coin placement should target roughly 65-75 percent safe or near-safe coins and 25-35 percent risk/reward coins. The first 70 meters must reliably produce useful coin income even on failed Runs.

Basic reach upgrades must be affordable from safe and near-safe coins across failed Runs. Risk/reward coins may accelerate progression but must not be required for the first ramp reach path.

Early reach tuning should be driven primarily by SlingshotLaunchPower and PlayerMaxSpeed. PlayerSteeringResponsiveness improves line control and risk coin access. CoinPickupMultiplier improves later progression speed without becoming mandatory for first ramp reach.

The level should consume the upgrade system through existing Run Modifier Snapshot and Gameplay Stat Resolver behavior. The Run Course should not directly read upgrade storage or purchase state.

If implementation needs reusable validation, prefer a deep course-authoring validation module with a small input/output surface. It should report facts like section continuity, required contacts, invalid categories, missing safety net coverage, pickup wiring, and representative slope samples.

Any Unity authoring MonoBehaviour added for this feature should stay shallow: serialized references, scene integration, or validation entry points only. It should not own reach progression policy, economy formulas, or hidden movement rules.

If course section data is encoded as an asset or serialized plan for validation, it should represent authoring expectations such as range, band, pitch target, feature purpose, and required safety notes. It should not become a second runtime level system unless later features prove that need.

The existing Run Camera should remain the camera behavior for the Run. Course implementation may tune camera collision layers, obstacle layers, or config values only when the authored course demonstrates a concrete framing problem.

The existing Ladybug character presentation remains a visual child of the authoritative Launch Target. The level must not add imported character physics or root-motion ownership.

The first implementation should prefer simple project-owned primitive or clean mesh colliders for gameplay. Final material, prop density, VFX, audio, and decorative detail are later polish after graybox gates pass.

The scope is a first full Run Course. It is not a general level editor, not procedural generation, and not a full content pipeline for many courses.

## Testing Decisions

Good tests for this feature check external gameplay and authoring behavior. They should assert that the course exposes the right contacts, surfaces, slopes, pickups, finish, and safety net coverage. They should not assert brittle mesh vertex counts, exact scene hierarchy depth, decorative prop names, or internal implementation details.

EditMode tests are appropriate for any new pure C# validation module. These tests should cover section range continuity, Run Progression Band mapping, coin split math if encoded, required feature presence, invalid category detection, and clear validation error messages.

EditMode tests should avoid runtime MonoBehaviour lifecycle expectations. If a test depends on scene loading, physics callbacks, colliders, or Play Mode transitions, it belongs in PlayMode.

PlayMode tests are appropriate for Gameplay Scene composition and Unity integration. Existing composition-test style is the closest prior art: load the Gameplay Scene, find expected components, verify DI resolution, check layers, check Run Contact Categories, and assert scene-owned references are wired.

The implementation should extend composition coverage to the real Run Course content after placeholders are replaced. Expected checks include Run Surface contacts, Run Obstacle contacts, Run Safety Net contacts, Run Finish contacts, player support probe compatibility, camera terrain/obstacle layers, pickup trigger setup, and Level Pickup State references.

PlayMode tests should verify that no authored level object introduces unsupported Run Contact Categories such as Boundary or Ramp.

PlayMode tests should include representative slope sampling if practical. Samples should cover center trough, left bank, and right bank on several Run Course Sections, and should validate that ForwardDownhillDegrees stays within intended directional expectations.

PlayMode tests should verify that required ramp and optional ramp colliders are classified as Run Surface, not Run Obstacle.

PlayMode tests should verify that Run Safety Net regions exist below representative course, side-lip, ramp-landing, and finish-approach areas. Exact coverage can be validated through authoring helpers or scene probes rather than fragile world-coordinate assertions where possible.

Pickup-related tests should verify external behavior: pickup triggers are on the configured Pickup Layer, collect through the configured Player Tag/collider path, use Pickup Definitions with Currency Grants, and are included in Level Pickup State.

Upgrade and economy tuning should not be fully automated by this PRD unless deterministic harness support exists. The first implementation should at minimum preserve existing upgrade tests and add manual graybox validation for reach and coin income.

Manual Unity smoke checks are required for the first full course because player feel, readability, one-minute pacing, ramp comfort, camera framing, and obstacle silhouettes are not fully captured by existing deterministic tests.

Manual no-upgrade validation should confirm that a decent Run usually reaches Band 2 around 85-125 meters, collects useful coins, and does not reach the required ramp under normal play.

Manual early-upgrade validation should confirm that Runs 2-3 reach the first obstacle/ramp region more consistently and that the required ramp becomes reliable after early reach upgrades.

Manual mid-progression validation should confirm that Runs 4-6 can reach Band 3/Band 4 and start using bank/ramp risk lines.

Manual upgraded validation should confirm that Runs 7-10 can reach Run Finish and that a competent upgraded clear takes about 55-65 seconds.

Manual coin validation should estimate whether reachable coins are roughly 65-75 percent safe/near-safe and 25-35 percent risk/reward.

Manual obstacle validation should check that every obstacle read has a visible safe line and recovery window.

Manual ramp validation should check takeoff readability, coin arc readability, landing safety, camera framing, and lack of immediate blockers.

Manual finish validation should check that the final 30 meters communicate Run Finish and do not introduce new hazards.

Manual visual validation should confirm that rooftop dressing improves theme without occluding the course, confusing colliders, or blocking the Run Camera.

Compilation remains a gate for later implementation. After code or test changes, compile through the Unity AI Agent Connector before running tests.

Targeted tests should run after compilation is clean. Do not run broad test suites before resolving compile errors.

Generated screenshots or captures may be used for diagnostics when camera framing, visual overlap, or rendered positioning is unclear. They should not replace deterministic assertions as the committed oracle unless explicitly accepted as test artifacts.

## Release and Compatibility

The project currently targets Unity 6000.3.18f1. The PRD assumes the existing package set, including VContainer, Cinemachine 3.x, Unity Input System, URP, Unity Test Framework, and the Unity AI Agent Connector.

No package manifest change is expected for the first Run Course implementation.

No Unity upgrade is expected.

No Addressables schema change is expected.

No public API change is expected unless a later implementation creates a reusable course-authoring validation module. If such a module is added, keep its public surface small and testable.

No save-format change is expected. The level uses existing app-session currency and upgrade behavior. Persistent progression is outside this PRD unless a separate persistence branch defines it.

No migration path is expected for existing saved content because this is first full course content, not a migration of shipped levels.

Backward compatibility risk is concentrated in scene and prefab serialization. Review Unity YAML diffs carefully when the level is implemented, especially Gameplay Scene changes, collider layers, camera layers, pickup references, and Run Contact authoring.

Imported Ladybug asset compatibility requires preserving source asset metadata. Do not modify imported plugin assets to make gameplay work; wrap or instance them from project-owned level content instead.

Physics feel compatibility is a risk. The course should be tuned against existing Slingshot launch, player steering, Run End Flow, Lost Momentum, and Run Camera behavior before changing those systems.

Economy compatibility is a risk because exact coin values and upgrade math are being completed in a separate branch. The first level implementation should validate placement intent and reach bands, then retune numbers when the upgrade branch is available.

## Out of Scope

Final art polish, dense prop dressing, VFX, audio, lighting polish, and marketing-quality presentation are out of scope for the first graybox.

Moving obstacles, rotating gates as gameplay, timed gates, enemy behavior, power-ups, and procedural hazards are out of scope.

Additional Run Courses, level selection, route branching, shortcuts, and alternate endings are out of scope.

Procedural level generation and a general-purpose level editor are out of scope.

New currencies, new upgrade definitions, new save format, persistent progression, ads, monetization, and post-run reward doubling are out of scope.

Exact final friction values, final coin values, final upgrade curves, and final economy balance are out of scope. The PRD defines placement and pacing intent, not final economy numbers.

Changing the authoritative Launch Target model, Ladybug character physics ownership, root-motion gameplay, Slingshot architecture, Run End Flow architecture, or Pickup architecture is out of scope.

Changing package dependencies, Unity version, Addressables setup, or build pipeline behavior is out of scope.

Full automation of one-minute human skill validation is out of scope unless a deterministic autopilot harness already exists or is created in a later task.

## Further Notes

Source planning artifacts are the Ladybug Half-Tube Run Course draft and the generated layout sketch. The draft is the more precise source of truth for section ranges, pitch targets, progression bands, validation gates, and scope freeze.

The design intentionally follows Sled Surfers-style level principles without copying specific layouts. The game-specific adaptation is a Ladybug-themed Paris rooftop gutter/chute with rooftop blockers and coin lines.

The first implementation should start with gameplay readability and validation. Art dressing should remain subordinate to collision clarity, camera readability, safe lines, ramp safety, and upgrade reach pacing.

The key unresolved tuning inputs are friction, final coin values, exact purchase curve, and exact reach contribution from SlingshotLaunchPower, PlayerMaxSpeed, PlayerSteeringResponsiveness, and CoinPickupMultiplier.

If the first graybox fails the early-run reach target, adjust slope, friction, safe coins, upgrade cost/effect, or reach-pressure spans before adding more obstacles.

If the first graybox fails the one-minute completion target, adjust total course length, average speed, pitch, or upgrade effect before adding new mechanics.

If players miss the required ramp, improve sightline, center approach coins, takeoff width, launch speed, or ramp placement before adding tutorial UI.

If players escape the half-tube too easily, tune bank angle, lip height, steering response, and safety net placement before introducing invisible containment.

If the Run Camera hides obstacles or finish, tune camera config and obstacle dressing before changing the core Run Camera architecture.

Definition of done for the later implementation branch should include: the full 420 meter graybox exists, the Run Finish is reachable by an upgraded competent Run, first-run failure normally occurs before the required ramp, safe/risk coin placement is present, safety net coverage works, static obstacle readability passes, required and optional ramps are safe, composition tests are updated, compilation is clean, and manual graybox validation has recorded results.
