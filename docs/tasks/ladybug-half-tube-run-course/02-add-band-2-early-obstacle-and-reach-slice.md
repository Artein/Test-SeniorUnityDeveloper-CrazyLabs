## Parent

[Ladybug Half-Tube Run Course PRD](../../prd/prd-ladybug-half-tube-run-course.md)

## What to build

Extend the **Run Course** through Band 2, 70-150 meters, with the first readable static rooftop obstacle choices and early reach-pressure setup. This slice should turn the Band 1 onboarding path into a longer playable Run that introduces one center blocker with wide left/right gaps, one later offset blocker with an obvious safe side, recovery space, safe split coin trails, optional tighter-line coins, and continued **Run Safety Net** coverage.

The completed slice should make early Runs useful without letting the first Run normally reach the required ramp. It should validate that obstacle pressure is readable and optional coins create risk/reward without becoming mandatory for upgrade progress.

## Acceptance criteria

- [ ] The **Run Course** is continuous from Launch through 150m, with Band 2 ranges matching the approved 70-95m, 95-120m, and 120-150m section roles.
- [ ] The first blocking obstacle appears only after Band 1 and is a static **Run Obstacle** with wide left/right safe gaps.
- [ ] The second Band 2 obstacle is an offset static **Run Obstacle** with one obvious safe side and an optional tighter reward line.
- [ ] Every Band 2 obstacle cluster preserves at least one obvious safe or near-safe traversal line.
- [ ] Band 2 includes recovery space after the first blocker before the second pressure beat.
- [ ] Safe coin trails around Band 2 blockers are reachable without risky precision.
- [ ] Optional Band 2 reward coins sit near tighter lines or low-bank choices without gating basic progression.
- [ ] Obstacles use project-owned gameplay colliders/wrappers and explicit **Run Obstacle** classification.
- [ ] Imported rooftop meshes, if used, are visual dressing and do not define unfair collision detail.
- [ ] **Run Safety Net** coverage continues under Band 2, side lips, and likely escape areas.
- [ ] A decent no-upgrade or starter-upgrade Run can collect useful Band 1/Band 2 coins before failing later through reach pressure rather than forced collision.
- [ ] The course still avoids the required ramp; the player should not normally reach ramp gameplay in this slice.

## Verification

- EditMode tests:
  - Pure authoring validation tests for obstacle safe-line metadata or section continuity, if a validation module exists.
- PlayMode tests:
  - Gameplay Scene composition verifies Band 2 blocker contacts are **Run Obstacle** and Band 2 traversal surface remains **Run Surface**.
  - Representative Band 2 slope samples stay within the intended normal sliding pitch range.
  - **Run Safety Net** coverage is present under Band 2 and side-lip escape areas.
  - Pickup wiring checks include Band 2 safe and optional reward pickups when pickup authoring is part of this slice.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector after any code, scene, prefab, or test change.
- Manual Unity smoke check:
  - Play through Band 2 and confirm the first blocker read is visible before impact, both safe gaps are understandable, the offset blocker has an obvious safe side, and recovery space feels fair.
  - Confirm safe coins support early upgrade progress while optional coins reward tighter line choices.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Author Band 1 Half-Tube Onboarding Slice
