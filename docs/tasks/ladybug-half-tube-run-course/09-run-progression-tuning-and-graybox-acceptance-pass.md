## Parent

[Ladybug Half-Tube Run Course PRD](../../prd/prd-ladybug-half-tube-run-course.md)

## What to build

Run the full progression tuning and graybox acceptance pass for the Ladybug half-tube **Run Course**. This slice should validate the course as a complete one-minute upgraded Run, a useful first-run failure, and a 5-10 Run first-completion progression loop using existing **Run Preparation** upgrades, coin income, **Lost Momentum**, **Run Safety Net**, and **Run Finish** behavior.

This is HITL because acceptance depends on human play, movement feel, obstacle readability, ramp comfort, camera readability, and economy tuning judgment.

## Acceptance criteria

- [ ] A competent upgraded Run reaches **Run Finish** in about 55-65 seconds without relying on side-lip exploits or shortcuts.
- [ ] A decent no-upgrade or starter-upgrade Run usually reaches Band 2 around 85-125m, collects useful coins, and fails before the required ramp.
- [ ] Runs 2-3 can reach the first obstacle/ramp region more consistently after early upgrades.
- [ ] Runs 4-6 can reach mid-course and start learning bank/ramp risk lines.
- [ ] Runs 7-10 can become finish-capable through upgrades and improved control.
- [ ] Under-upgraded failure is primarily **Lost Momentum**/reach pressure, not mandatory blocker death.
- [ ] Every **Run Course Section** has at least one readable safe or near-safe traversal line.
- [ ] Required ramp has clear approach, visible coin arc, broad landing, and no immediate blocker punishment.
- [ ] Optional ramp rewards confidence but keeps center fallback viable.
- [ ] **Run Safety Net** catches below-course, side-lip, ramp-landing, optional escape, and finish-approach failures.
- [ ] Coin distribution remains approximately 65-75 percent safe/near-safe and 25-35 percent risk/reward.
- [ ] Final 30m clearly communicates **Run Finish** and introduces no new hazard type.
- [ ] Any tuning changes preserve the frozen first graybox scope rather than adding moving obstacles, new ramps, route branching, or hidden containment.
- [ ] Manual acceptance findings are recorded in the issue, PRD notes, or a local QA report agreed by the implementer.

## Verification

- EditMode tests:
  - Any course validation module tests remain green after final tuning.
- PlayMode tests:
  - Gameplay Scene composition, contact category checks, ramp surface checks, pickup wiring checks, safety-net checks, and representative slope sample checks remain green.
  - Existing **Run End Flow**, **Lost Momentum**, **Pickup**, **Upgrade**, and **Run Camera** test coverage remains green where touched.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector after any code, scene, prefab, or test change.
- Manual Unity smoke check:
  - HITL no-upgrade run reach check.
  - HITL early-upgrade reach check.
  - HITL mid-progression reach check.
  - HITL upgraded completion timing check.
  - HITL obstacle readability check.
  - HITL ramp safety check.
  - HITL coin distribution check.
  - HITL finish readability check.
  - HITL camera and visual theme fit check.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Author Band 1 Half-Tube Onboarding Slice
- 02 - Add Band 2 Early Obstacle And Reach Slice
- 03 - Add Required Center Tutorial Ramp Slice
- 04 - Add Band 3 Post-Ramp Bank Pressure Slice
- 05 - Add Band 4 Reach Pressure And Optional Ramp Slice
- 06 - Add Band 5 Finish Approach And Run Finish Slice
- 07 - Wire Course-Wide Pickup Distribution And Level Pickup State
- 08 - Add Rooftop Visual Dressing And Camera Readability Pass
