# Run Steering Mode Selector Boundary

Type: AFK

## Parent

`docs/prd/prd-run-air-steering-control.md`

## What to build

Replace the post-launch steering blocker concept with a **Run Steering Mode Selector** that chooses only between grounded **Run Steering Control** and **Run Air Steering Control** during **Running**.

The selector decides from support and motion facts. It selects air when the **Run Body** has no valid **Run Surface** support, or when reported support is stale-grounded while the **Run Body** is moving away from the reported surface above the accepted lift tolerance. It selects grounded when support is a valid **Run Surface** and velocity has no positive surface-normal lift above the accepted tolerance.

This slice should be a complete decision-boundary change: production selector, controller integration point, and focused tests proving the old blocked/inactive selection has been removed from the mode vocabulary. It must not yet add full air steering behavior beyond choosing the correct mode.

## Acceptance criteria

- [ ] **Run Steering Mode Selector** returns only grounded or air mode.
- [ ] Missing, invalid, or non-surface support selects air.
- [ ] Valid **Run Surface** support with positive surface-normal lift above tolerance selects air.
- [ ] Valid **Run Surface** support with no positive lift selects grounded.
- [ ] Downward or into-surface velocity on valid **Run Surface** support selects grounded.
- [ ] Selector decisions do not depend on elapsed time or no-takeoff timeout.
- [ ] Selector decisions do not depend on **Launch Flight**, **LaunchPush**, **Airborne**, Animator state, camera state, or run-result presentation.
- [ ] Selector does not rotate velocity, write **Run Body** pose, query input, or call Unity physics.
- [ ] Existing post-launch blocker tests are replaced or rewritten so they describe selector behavior rather than blocked steering behavior.

## Verification

- EditMode tests:
  - Selector returns air for missing/invalid/non-surface support.
  - Selector returns air for valid support plus positive lift above tolerance.
  - Selector returns grounded for valid support plus no lift.
  - Selector returns grounded for valid support plus downward or into-surface velocity.
  - Selector output is independent of elapsed time and presentation state.
- PlayMode tests:
  - None required for this slice unless integration cannot be covered in EditMode.
- Static checks:
  - Unity connector compile before tests in implementation.
  - Rider reformat/problems for changed code and tests in implementation.
- Manual Unity smoke check:
  - Not required for this decision-boundary slice.
- Package version/changelog:
  - Not required.

## Blocked by

None - can start immediately.
