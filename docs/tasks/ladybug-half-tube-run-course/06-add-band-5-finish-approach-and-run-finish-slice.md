## Parent

[Ladybug Half-Tube Run Course PRD](../../prd/prd-ladybug-half-tube-run-course.md)

## What to build

Extend the **Run Course** through Band 5, 350-420 meters, with a readable finish approach, final funnel coins, sparse familiar obstacle pressure, and a visible **Run Finish**. This slice should make the full course completable from **Launch** to **Run Finish** by an upgraded competent player while preserving the PRD rule that the final approach introduces no new hazard type.

The completed slice should be demoable as a full static graybox course before course-wide pickup distribution, visual dressing, and final tuning pass.

## Acceptance criteria

- [ ] The **Run Course** is continuous from Launch through the final 420m region.
- [ ] The 350-390m finish approach keeps wide sightlines and uses at most one familiar blocker pattern.
- [ ] The 390-420m final funnel has no new hazard type.
- [ ] **Run Finish** is visible and placed in the final 410-420m region.
- [ ] Final coin lines guide the player toward **Run Finish** instead of distracting away from completion.
- [ ] Finish contact authoring uses the existing **Run Finish** category and ends the **Run** as `Finished`.
- [ ] **Run Safety Net** coverage includes the finish approach, final funnel, and likely late escape areas.
- [ ] The **Run Camera** can frame the finish approach and **Run Finish** clearly enough for manual validation.
- [ ] The full static course can be traversed end-to-end in-editor with sufficiently upgraded movement tuning or temporary debug tuning.
- [ ] The final approach does not rely on moving obstacles, rotating gates, power-ups, route branching, or hidden containment.

## Verification

- EditMode tests:
  - Pure validation tests for total section continuity or finish-range expectations, if represented in a validation module.
- PlayMode tests:
  - Gameplay Scene composition verifies a real **Run Finish** exists and has the **Run Finish** contact category.
  - **Run Safety Net** coverage is verified under the finish approach and final escape areas.
  - The **Run Contact Classifier** and **Run End Flow** still accept finish contact behavior.
  - Representative Band 5 slope samples stay within intended finish-approach pitch expectations.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector after any code, scene, prefab, or test change.
- Manual Unity smoke check:
  - Play or debug-traverse the full course to confirm the finish is visible, the last 30m is clear, final coins funnel toward the finish, and finishing ends the **Run** correctly.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 05 - Add Band 4 Reach Pressure And Optional Ramp Slice
