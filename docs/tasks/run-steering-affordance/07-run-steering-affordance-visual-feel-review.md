# Run Steering Affordance Visual Feel Review

Type: HITL

## Parent

`docs/prd/prd-run-steering-affordance.md`

## What to build

Run a human visual feel review for the complete **Run Steering Affordance** in the Gameplay scene and commit only the final serialized tuning changes that come out of that review.

This slice is intentionally HITL because final opacity, scale, deadzone visibility, endpoint visibility, fade timing, and under-finger readability are product/design feel decisions. The implementation should already be functional before this issue begins.

## Acceptance criteria

- [ ] Human reviewer checks the affordance in the Gameplay scene during Running.
- [ ] Knob readability is acceptable while a real finger or editor pointer covers the control area.
- [ ] Endpoint hints are slightly visible but do not read as a horizontal track or rail.
- [ ] Deadzone hint either ships enabled with acceptable subtlety or is intentionally disabled in serialized tuning.
- [ ] Start and end animation feel responsive and do not imply positional lag.
- [ ] Edge-origin behavior is acceptable near left and right screen boundaries.
- [ ] Common portrait aspect ratios are reviewed.
- [ ] Final serialized tint, opacity, scale, and timing values are committed.
- [ ] Any deferred art concerns are recorded without blocking the implementation slice.

## Verification

- EditMode tests:
  - Existing affordance tests remain green after tuning values change.
- PlayMode tests:
  - Existing scene composition and view smoke tests remain green after tuning values change.
- Static checks:
  - Unity connector compile before tests if serialized changes cause asset/code refresh.
  - No generated diagnostics or temporary screenshots are committed unless explicitly accepted as review artifacts.
- Manual Unity smoke check:
  - Review touch near center, left edge, right edge, slow inside-deadzone motion, fast clamp swipe, release while clamped, and cancellation/focus-interruption where practical.
  - Review at least one small-phone and one tall-phone portrait aspect ratio if available.
- Package version/changelog:
  - Not required.

## Blocked by

- Gameplay Scene Composition And Regression Smoke

