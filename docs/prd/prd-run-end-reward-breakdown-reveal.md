## Problem Statement

The current **Run Ended UI** shows a single earned-coin value, distance reached, and best-run improvement. That is enough for a basic result screen, but it does not explain where the run reward came from. The player can pick up coins, travel distance, spend time in the air, and later interact with other level features or mechanics that produce coins. If the screen only shows one total, the player cannot connect their run behavior to the reward outcome.

This matters for the upgrade loop. Early Ladybug half-tube runs are expected to fail before the finish, then feed **Run Preparation** upgrades. The **Run Ended** phase should teach the player which actions generated value, so repeated attempts feel understandable rather than random.

The current screen also reveals all result information immediately. The desired presentation is sequential: show the run result, count up distance, show optional best-run improvement feedback, reveal each **Run Reward Source Row**, show the total received coins, then enable **Tap to Continue**. During that reveal, taps should fast-forward to the fully revealed state, not acknowledge the result by accident.

## Solution

Introduce a source-agnostic **Run Reward Breakdown** for each accepted **Run Result**. The breakdown is an ordered list of **Run Reward Source** entries plus the already calculated total run reward. Each source explains one coin contribution, such as picked-up coins, **Distance Bonus**, **Air Time Bonus**, or a future level mechanic. The **Run Ended UI** consumes only generic row state; it does not know which gameplay system produced each source.

When **Run End Flow** accepts a **Run Result**, reward contributors produce a **Run Reward Breakdown** and the matching **Run Currency Snapshot**. Existing economy commit behavior continues to grant rewards from the accepted result; the reveal screen only explains the reward and does not own the grant.

The scene-owned **Run Ended UI** animates the accepted result:

1. Fade in the run result label.
2. Fade in and count up the reached distance.
3. If the run improves the session best distance, fade in the best-improvement feedback.
4. For each ordered **Run Reward Source Row**, fade in the label and count up the source amount.
5. Fade in the total run coins with the final total value already calculated.
6. Show and enable **Tap to Continue**.

A tap before the reveal finishes fast-forwards the reveal and shows all final values immediately. Only the next tap after the reveal is complete is treated as **Acknowledge Run Result**.

## Unity Surfaces

- Runtime assemblies and asmdefs:
  - Extend the gameplay runtime with **Run Reward Breakdown**, **Run Reward Source**, and contributor-side reward calculation modules.
  - Keep reward contributors as plain C# services composed through VContainer.
  - Keep **Run End Flow** as the accepted-result authority and final acknowledgement boundary.
  - Extend **Run Result** data so presenters can read both the total **Run Currency Snapshot** and the source-level **Run Reward Breakdown**.
  - Add a **Run Air Time** tracker or metric service that measures gameplay air time during **Running** without coupling to character animation states.
  - Extend the **Run Ended** presenter/view state to carry ordered source rows, total run coins, reveal-copy fields, and visibility/enabled states.
  - Keep the **Run Ended UI** as a shallow scene view that renders state, instantiates row views, and owns the reveal coroutine.
- Editor assemblies, windows, inspectors, importers, or menu items:
  - No editor tooling is required for the first implementation.
  - No custom importers, menu items, or bake workflows are required.
- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:
  - The gameplay scene keeps a serialized **Run Ended UI** hierarchy.
  - The scene-owned view gains serialized references for source-row container, source-row prefab, total-coins text, reveal tap area, and **Tap to Continue** affordance.
  - The row prefab is generic and source-agnostic: label, amount text, and optional icon/swatch only.
  - Timing values for fades and counters may be serialized on the scene-owned view or a small config object, but the first slice should avoid new packages.
  - No UniTask package is added; reveal sequencing uses Unity coroutines for now.
  - No pooling system is added for row views in this slice.
  - No save-data schema change is expected.
- RPC/helper commands, hooks, or shell wrappers:
  - Continue using the Unity AI Agent Connector compile gate and targeted Unity Test Framework selectors for implementation verification.
  - No new shell command or RPC protocol is required.
- Package versioning, changelog, and installation/sync behavior:
  - This is project gameplay/UI work, not a distributable package release.
  - No package manifest, Unity version, Addressables schema, or installation behavior change is required.

## User Stories

1. As a player, I want to see the run result label first, so that I immediately understand whether the run ended in victory or defeat.
2. As a player, I want distance reached to count up, so that run progress feels earned and legible.
3. As a player, I want picked-up coins to appear as their own reward source, so that collecting coins during the run has clear payoff.
4. As a player, I want traveled distance to appear as a coin source when it grants coins, so that deeper progress feels valuable even before finishing.
5. As a player, I want time in the air to appear as a coin source when it grants coins, so that risky or skilled movement can be rewarded clearly.
6. As a player, I want future mechanics to appear as separate reward rows, so that new level features can teach themselves on the result screen.
7. As a player, I want the total run coins to appear after source rows, so that the total feels like the sum of understandable parts.
8. As a player, I want **Tap to Continue** to appear only after the reveal, so that I do not skip the result by accident.
9. As a player, I want the first tap during the reveal to fast-forward, so that I can skip the animation when I already understand it.
10. As a player, I want the next tap after fast-forward to continue, so that one impatient tap does not immediately leave **Run Ended**.
11. As a player, I want the final values to be shown immediately after fast-forward, so that skipping does not hide reward information.
12. As a player, I want source counters to land exactly on the awarded values, so that the UI never disagrees with my wallet reward.
13. As a player, I want failed runs to still explain reward sources, so that failure still feels productive.
14. As a player, I want successful runs to explain reward sources, so that victory rewards are just as transparent as failed-run rewards.
15. As a designer, I want the **Run Ended UI** serialized in the scene, so that I can tune layout and animation references without runtime hierarchy code.
16. As a designer, I want a generic source-row prefab, so that I can style all reward rows consistently.
17. As a designer, I want source rows to render in authored contributor order, so that the result screen can teach rewards in a stable sequence.
18. As a designer, I want row labels owned by reward sources, so that each mechanic can provide player-facing meaning without the UI knowing mechanics.
19. As a designer, I want zero-value source rows hidden by default, so that the screen stays focused on what contributed to the run.
20. As a designer, I want a mechanic to opt into showing a zero-value row when useful, so that tutorial or feature-teaching rows remain possible later.
21. As a designer, I want reveal timings to be tunable, so that the screen can feel responsive on mobile.
22. As a designer, I want no pooling requirement in the first slice, so that the first implementation stays small and easy to inspect.
23. As a developer, I want **Run Reward Breakdown** to be source-agnostic, so that pickup, distance, air-time, and future rewards share one result model.
24. As a developer, I want **Run Reward Contributors** to produce source entries, so that adding a new mechanic does not require editing the **Run Ended UI**.
25. As a developer, I want **Run Currency Snapshot** to stay the authoritative total granted to the wallet, so that reward commit remains stable.
26. As a developer, I want the breakdown total to be derived from the same accepted source entries, so that UI totals and granted totals cannot drift.
27. As a developer, I want **Distance Bonus** to be separate from **Run Distance Display**, so that meters shown and coins awarded can use different formatting and tuning.
28. As a developer, I want **Air Time Bonus** to use gameplay support state, so that reward math is not coupled to character presentation animation.
29. As a developer, I want the reveal sequence owned by the view, so that the presenter can stay a state builder and not manage Unity coroutines.
30. As a developer, I want the view to raise acknowledgement only after reveal completion, so that **Run End Flow** does not need to know about UI animation steps.
31. As a developer, I want **Acknowledge Run Result** to remain the only gameplay transition out of **Run Ended**, so that reset timing stays explicit.
32. As a developer, I want coroutine-based reveal sequencing for now, so that the implementation does not add a new async dependency.
33. As a developer, I want a future TODO for UniTask migration, so that a later async cleanup has a visible decision marker.
34. As a developer, I want a future TODO for row pooling, so that optimization is acknowledged without overbuilding the first slice.
35. As a tester, I want reward breakdown unit tests, so that source aggregation and totals are deterministic.
36. As a tester, I want contributor tests, so that picked-up coins, distance bonus, and air-time bonus can be validated independently.
37. As a tester, I want view tests for reveal and fast-forward behavior, so that early taps do not accidentally acknowledge the run.
38. As a tester, I want scene wiring tests for the row prefab and container, so that the authored UI cannot lose required references silently.
39. As a tester, I want tests to avoid exact UI layout assertions, so that designers can move and style the result panel freely.
40. As a tester, I want end-to-end coverage that granted coins match displayed total, so that reward trust is protected.

## Implementation Decisions

- Add **Run Reward Breakdown** as the source-level explanation for the accepted run reward.
- Add **Run Reward Source** as a generic row model with source identity, display label, awarded currency, awarded amount, display order, and optional visual metadata.
- The first implementation should aggregate source rows by source type, not by individual pickup or event instance.
- Picked-up coins use the final awarded pickup grant amount, after existing pickup grant resolution and modifiers, not raw pickup object count.
- **Distance Bonus** is a reward source, not the same concept as **Run Distance Display**.
- **Air Time Bonus** is a reward source based on gameplay **Run Air Time**, not **Character Presentation Mode.Airborne**.
- **Run Air Time** should be measured only while the game is in **Running**.
- **Run Air Time** should use support/surface context from gameplay traversal, not animation state or UI state.
- Reward contributors are the extension point for future mechanics that convert level features or run behavior into coins.
- Contributors should expose a small stable interface that accepts accepted run facts and returns zero or more source entries.
- Contributor order should be deterministic and supplied upstream; the **Run Ended UI** renders rows in the order provided.
- Hide zero-value reward rows by default, while allowing a contributor-level opt-in if a future mechanic needs to teach a zero result.
- The breakdown builder should derive the total **Run Currency Snapshot** from the same accepted source entries used for UI rows.
- Existing wallet commit behavior should continue to consume the accepted result total, not inspect UI rows.
- The **Run Ended UI** must not calculate pickup, distance, air-time, or future mechanic rewards.
- The presenter builds source-row view state from **Run Reward Breakdown** and passes generic rows to the view.
- The view owns the serialized row prefab and row container.
- The view instantiates as many row views as the accepted result needs.
- Add a code TODO near row instantiation noting that pooling can be introduced if row churn becomes a measured issue.
- Do not add pooling in this slice.
- The row prefab should render a label, generic currency icon, and amount; the icon reveals with the row and remains source-agnostic.
- The reveal sequence should be implemented with Unity coroutines for now.
- Add a code TODO near the coroutine sequence noting that UniTask migration can be considered if the project later standardizes on UniTask.
- Do not add UniTask or any other async package in this slice.
- The run result label fades in before numeric counters start.
- Distance reached fades in and counts up before best-improvement or reward source rows reveal.
- Best-improvement feedback, when present, fades in after reached distance and before reward source rows reveal.
- Each **Run Reward Source Row** fades in its label/icon/amount together and counts up before the next row begins.
- Source-row coin amounts are unsigned white numbers because the row already communicates that the value is a reward.
- Total run coins fades in after all source rows, uses `RUN TOTAL` copy with the total coin icon/value, and shows the final calculated total immediately without a counter-up requirement.
- **Tap to Continue** is hidden or disabled until the reveal sequence reaches the final state.
- A tap during reveal fast-forwards all pending reveal steps to their final visual values.
- A fast-forward tap must not raise **Acknowledge Run Result**.
- A tap after reveal completion raises **Acknowledge Run Result** through the existing RunEnd acknowledgement path.
- **Run End Flow** remains the final authority and may still reject acknowledgement if its gameplay guard has not elapsed.
- Scene-authored **Run Ended UI** remains the only result screen instance; runtime must not create the whole panel.
- Scene-owned view registration should follow the existing VContainer view pattern and keep MonoBehaviours shallow.
- Existing title, reached-distance, best-improvement, victory/defeat, pose-lock, and camera behavior should remain compatible.
- No save data change is introduced for source rows or best run distance.
- No new gameplay state is introduced.

## Testing Decisions

- Good tests for this feature assert reward contracts, state transitions, reveal behavior, and scene wiring; they should not assert exact art layout, font size, anchoring, or designer-owned row positions.
- Add EditMode tests for **Run Reward Breakdown** construction:
  - source entries aggregate by source type;
  - totals are derived from entries;
  - source order is deterministic;
  - zero-value rows are hidden by default;
  - contributor opt-in can keep a zero-value row when supported.
- Add EditMode tests for picked-up coin contribution:
  - collected pickup grants appear as a picked-up coin source;
  - final grant amounts are used;
  - multiple pickup grants aggregate into one source row.
- Add EditMode tests for **Distance Bonus**:
  - distance reward uses accepted **Run Result** distance;
  - distance display can differ from distance reward amount;
  - conversion and rounding behavior are deterministic.
- Add EditMode tests for **Run Air Time** and **Air Time Bonus**:
  - air-time tracking counts only during **Running**;
  - supported surface time is not counted as air time;
  - unsupported traversal time is counted as gameplay air time;
  - accepted air-time reward appears as a generic reward source.
- Add EditMode tests for **Run End Flow** and economy commit integration:
  - accepted results carry both breakdown and total snapshot;
  - committed wallet reward matches the accepted total snapshot;
  - acknowledgement is not required for wallet commit if current behavior remains accepted-result based.
- Add EditMode presenter tests:
  - hidden **Run Ended** state has no visible rows;
  - visible state includes ordered source-row view states;
  - source labels and amounts pass through without the presenter knowing source mechanics;
  - total run coins equals the breakdown total;
  - existing title, reached distance, and best-improvement state still render correctly.
- Add PlayMode or lifecycle-aware tests for **Run Ended UI** reveal behavior:
  - applying a visible state starts with reveal content hidden except the first intended step;
  - reveal progresses through title, distance, optional best improvement, source rows, total, then **Tap to Continue**;
  - source rows reveal label, icon, and unsigned amount together;
  - the final total is labeled `RUN TOTAL` and reveals its icon with the value;
  - a tap during reveal fast-forwards to fully revealed final values;
  - the fast-forward tap does not acknowledge the result;
  - a second tap after reveal completion acknowledges the result;
  - applying a hidden state stops any active reveal coroutine and hides the root.
- Add PlayMode scene composition tests:
  - the authored **Run Ended UI** has a row container and row prefab assigned;
  - the row prefab has the required row view references;
  - entering **Run Ended** does not create a duplicate result panel;
  - generated row instances are children of the authored row container.
- Keep existing PlayMode tests that prove **RunEnded -> RunPreparation** reset happens only after acknowledgement.
- Do not add tests that require exact reward tuning values unless those values become product requirements.
- Do not add tests that require exact row count for designer-owned future content beyond the controlled test setup.
- Use Unity AI Agent Connector for compile and targeted tests during implementation.
- Run compile before tests after any production or test code change.

## Release and Compatibility

- Unity 6 project assumptions remain unchanged.
- No package manifest change is expected.
- No UniTask dependency is introduced.
- No save-data schema change is expected.
- Existing wallet grants should remain backward compatible because they still use accepted run reward totals.
- Existing tests and constructors that create **Run Result** may need update for the new reward breakdown data.
- Existing scene and prefab serialization must be updated for row prefab, row container, total-coins, reveal tap area, and **Tap to Continue** references.
- Existing player balances should not be migrated.
- Existing **Run Preparation** upgrade flow remains the next phase after acknowledgement.
- The main compatibility risk is mismatch between source-row totals and granted totals; the breakdown builder and tests should make that impossible by deriving both from the same source entries.

## Out of Scope

- Adding UniTask or changing the project async standard.
- Implementing row pooling.
- Persisting per-source reward history.
- Adding analytics, telemetry, or event upload for reward sources.
- Adding localization infrastructure for source labels.
- Final art direction, typography, icon art, particle effects, audio, haptics, or screen polish.
- Exact economy balancing for distance-to-coins or air-time-to-coins conversion.
- Changing pickup collision or collection rules.
- Changing upgrade purchase logic.
- Changing level layout, Terrain, obstacles, pickups, or finish contacts.
- Introducing a new post-victory route, next-level flow, or level-select screen.
- Replacing the existing **Run Ended** state or adding new gameplay state IDs.
- Making the **Run Ended UI** calculate mechanic-specific rewards.

## Further Notes

- This PRD builds on the existing acknowledged **Run Ended UI** result screen work.
- The glossary terms that matter most are **Run Reward Breakdown**, **Run Reward Source**, **Run Reward Contributor**, **Run Reward Source Row**, **Run Reward Reveal**, **Run Reward Reveal Fast-Forward**, **Distance Bonus**, and **Air Time Bonus**.
- The first implementation should prefer deep, testable plain C# modules for reward math and metrics, with the Unity view limited to rendering and animation.
- The scene-owned view may instantiate row views because the row count depends on accepted result data, but it should not own gameplay reward rules.
- Unresolved: exact conversion rates for **Distance Bonus** and **Air Time Bonus**.
- Resolved by HITL feel review: first-slice source rows use unsigned white amounts, generic coin icons reveal with each row, and the final total is labeled `RUN TOTAL`.
- Unresolved: exact reveal durations and easing curves.
- Unresolved: whether any first-slice zero-value source should be intentionally shown for teaching, or whether all zero-value rows should stay hidden.
- Assumption: source labels are English strings for now; localization is deferred.
- Assumption: picked-up coin source rows display final awarded currency, not raw collectible count.
- Assumption: the total run coins shown on the result screen represents the accepted reward total for the run, not the player's post-commit wallet balance.
