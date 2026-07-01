## Parent

[Ladybug Half-Tube Run Course PRD](../../prd/prd-ladybug-half-tube-run-course.md)

## What to build

Wire the complete course-wide **Pickup** distribution and **Level Pickup State** for the authored Run Course. This slice should turn the per-band coin intent into reusable scene content that supports safe baseline income, optional risk/reward income, early upgrade purchases, ramp arcs, recovery rewards, and finish funnel guidance without introducing a parallel collection system.

The completed slice should verify that all authored coin **Pickups** use existing pickup definitions, configured layers/tags, trigger behavior, and level-session state.

## Acceptance criteria

- [ ] Coin **Pickups** are placed across all five **Run Progression Bands** according to the PRD's safe baseline plus optional risk/reward structure.
- [ ] Reachable coins are roughly distributed as 65-75 percent safe or near-safe and 25-35 percent risk/reward.
- [ ] Band 1 and Band 2 safe coins are reliable enough to make first failures useful and support at least one early reach-upgrade path after tuning.
- [ ] Risk/reward coins speed progression but are not required for basic upgrade progress.
- [ ] Required-ramp approach coins and ramp arc coins make the ramp intent visible before takeoff.
- [ ] Optional-ramp reward coins communicate risk without blocking the center fallback.
- [ ] Post-obstacle and post-ramp reward coins confirm successful reads without hiding immediate hazards.
- [ ] Finish funnel coins guide toward **Run Finish**.
- [ ] All course coins reuse existing **Pickup** prefabs/definitions and configured **Currency Grant** behavior.
- [ ] All authored pickups are included in **Level Pickup State** for the current **Level Session**.
- [ ] Pickup trigger setup uses the configured **Pickup Layer**, **Player Tag**, and existing collection path.
- [ ] Course pickup wiring remains compatible with `CoinPickupMultiplier`.

## Verification

- EditMode tests:
  - Pure validation tests for safe/risk distribution counts or band assignment, if pickup authoring metadata is represented in a validation module.
- PlayMode tests:
  - Gameplay Scene composition verifies all course **Pickups** are wired into **Level Pickup State**.
  - Representative pickups from safe lines, risk lines, ramp arcs, and finish funnel use valid **Pickup Definitions** and configured trigger/layer setup.
  - Existing pickup collection smoke path remains green with the course pickups present.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector after any code, scene, prefab, or test change.
- Manual Unity smoke check:
  - Play through representative safe and risk lines and confirm coins are collectible, visible, readable, and aligned with line choice.
  - Confirm collected coins feed Run Preparation upgrade purchases through existing systems.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Author Band 1 Half-Tube Onboarding Slice
- 02 - Add Band 2 Early Obstacle And Reach Slice
- 03 - Add Required Center Tutorial Ramp Slice
- 04 - Add Band 3 Post-Ramp Bank Pressure Slice
- 05 - Add Band 4 Reach Pressure And Optional Ramp Slice
- 06 - Add Band 5 Finish Approach And Run Finish Slice
