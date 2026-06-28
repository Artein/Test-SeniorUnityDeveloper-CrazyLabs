## Parent

[Run End Flow First Slice PRD](../../prd/prd-run-end-flow-first-slice.md)

## What to build

Add **Lost Momentum** run-ending behavior as a separate motion/progress detector. After launch grace, the detector should submit a `LostMomentum`
candidate only when both course-planar speed and new maximum forward progress remain below configured thresholds for a sustained configured window.

This slice should integrate with **Run End Flow** without mixing motion-based stopping into `RunContactClassifier`. It should not depend on grounded or
airborne state, because jumps and ramp-shaped **Run Surface** geometry are valid gameplay.

## Acceptance criteria

- [ ] **Lost Momentum** detection activates after `LaunchApplied` while Running.
- [ ] Detection waits through a configured launch grace period.
- [ ] Detection requires low course-planar speed.
- [ ] Detection requires almost no new maximum forward progress.
- [ ] Detection requires both low-speed and low-progress conditions for the configured sustained duration.
- [ ] Detection does not use grounded/airborne state.
- [ ] A valid **Lost Momentum** condition submits a `LostMomentum` candidate to **Run End Flow**.
- [ ] **Lost Momentum** results use the same **Run Result** logging and `Running -> RunEnded -> PreLaunch` path as contact endings.
- [ ] `RunEndConfig` owns lost-momentum grace, duration, speed threshold, and progress threshold.

## Verification

- EditMode tests:
  - No candidate before launch grace expires.
  - No candidate while course-planar speed is above threshold.
  - No candidate while maximum forward progress continues increasing.
  - Candidate appears only after both conditions persist for the configured duration.
  - Detector does not require any grounded/airborne signal.
  - Candidate is ignored when not Running or before `LaunchApplied`.
- PlayMode tests:
  - Not required unless a lightweight Rigidbody integration case is added.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Let the **Launch Target** settle without forward progress and verify one `LostMomentum` result is logged before returning to **Pre-Launch**.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Run Progress Frame And RunProgressService
- 03 - End Runs From Finish Safety Net And Obstacle Impact
