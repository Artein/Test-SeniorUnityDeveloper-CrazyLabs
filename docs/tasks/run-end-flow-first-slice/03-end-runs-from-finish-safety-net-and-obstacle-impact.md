## Parent

[Run End Flow First Slice PRD](../../prd/prd-run-end-flow-first-slice.md)

## What to build

Add the first **Run End Flow** runtime path for contact-driven run endings. When **Run Finish**, **Run Safety Net**, or qualifying **Obstacle Impact**
candidates arrive while the game is Running, the flow should produce one **Run Result**, log it, transition `Running -> RunEnded`, then transition
back to **Pre-Launch** after the configured transient delay.

This slice should reuse the existing authored **RunEnded** state and state transitions, and it should rely on existing state listeners for target reset,
camera deactivation, steering deactivation, and slingshot capture re-enable.

## Acceptance criteria

- [ ] `RunEndReason` first-slice values include `Finished`, `ObstacleHit`, `OutOfBounds`, and `LostMomentum`.
- [ ] Contact candidates from **Run Finish** produce successful `Finished` results.
- [ ] Contact candidates from **Run Safety Net** produce non-success `OutOfBounds` results.
- [ ] Qualifying **Run Obstacle** impact candidates produce non-success `ObstacleHit` results.
- [ ] **Run Result** includes reason, success flag, elapsed time, distance travelled, final position, and final speed.
- [ ] Result metrics start at `LaunchApplied`, not at Running state transition alone.
- [ ] `DistanceTravelled` is read from `RunProgressService` maximum forward progress.
- [ ] Invalid progress-frame snapshot logs a clear error and suppresses fake **Run Result** output.
- [ ] First-slice results are logged through Unity `Debug.Log`.
- [ ] The flow transitions `Running -> RunEnded -> PreLaunch` and does not bypass **RunEnded**.
- [ ] **Run End Flow** resets only its own contact/result state.
- [ ] Existing systems continue owning **Launch Target** reset, steering deactivation, camera deactivation, and slingshot capture re-enable.
- [ ] `RunEndConfig` owns obstacle impact threshold and transient **RunEnded** delay.

## Verification

- EditMode tests:
  - Finish candidate logs one successful result and requests `Running -> RunEnded`.
  - Safety-net candidate logs one non-success `OutOfBounds` result.
  - Obstacle-impact candidate logs one non-success `ObstacleHit` result.
  - Result fields include expected reason, success, elapsed time, distance, final position, and final speed.
  - Result starts on `LaunchApplied`, not Running alone.
  - Invalid progress snapshot suppresses fake result output.
  - Delayed `RunEnded -> PreLaunch` transition uses the configured delay.
  - Flow does not call target, camera, steering, or slingshot reset APIs directly.
- PlayMode tests:
  - Not required beyond existing state/composition coverage for this slice unless a scene-bound integration is added.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Reach finish, fall into safety net, and hit obstacle hard; each logs one result and returns to **Pre-Launch**.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Run Progress Frame And RunProgressService
- 02 - Add Run Contact Metadata And Target Notifications
