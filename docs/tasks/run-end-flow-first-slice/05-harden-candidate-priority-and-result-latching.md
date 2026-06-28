## Parent

[Run End Flow First Slice PRD](../../prd/prd-run-end-flow-first-slice.md)

## What to build

Harden the **Run End Flow** arbitration behavior so competing candidates are deterministic and a run can only produce one result. Same-fixed-tick
candidates should resolve by the agreed priority, and later collision, trigger, or momentum signals should be ignored until the next `LaunchApplied`.

This slice turns the earlier contact and momentum paths into a reliable arbitration surface under physics aftermath and simultaneous trigger cases.

## Acceptance criteria

- [ ] Same-fixed-tick candidates resolve by priority: `Finished > ObstacleHit > OutOfBounds > LostMomentum`.
- [ ] `Finished` wins over obstacle/safety-net/momentum candidates in the same fixed tick.
- [ ] `ObstacleHit` wins over `OutOfBounds` and `LostMomentum` in the same fixed tick.
- [ ] `OutOfBounds` wins over `LostMomentum` in the same fixed tick.
- [ ] After one result is accepted, later candidates are ignored until the next `LaunchApplied`.
- [ ] A new `LaunchApplied` resets the latch for a new **Run**.
- [ ] Candidate queues are cleared at appropriate state boundaries without resetting target/camera/steering/slingshot ownership.
- [ ] Tests cover duplicate contact and trigger notifications in the same run.

## Verification

- EditMode tests:
  - Priority matrix for same-fixed-tick candidate combinations.
  - Duplicate finish, safety-net, obstacle, and lost-momentum candidates log one result only.
  - Later candidates after accepted result do not trigger extra state transitions or logs.
  - New `LaunchApplied` permits a new result in a later run.
  - Leaving Running clears pending candidates without touching unrelated systems.
- PlayMode tests:
  - Not required unless a scene-bound duplicate-trigger case is cheap to add.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Create a case that can overlap finish/obstacle or obstacle/safety-net and verify only the highest-priority result logs once.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 03 - End Runs From Finish Safety Net And Obstacle Impact
- 04 - End Runs From Lost Momentum
