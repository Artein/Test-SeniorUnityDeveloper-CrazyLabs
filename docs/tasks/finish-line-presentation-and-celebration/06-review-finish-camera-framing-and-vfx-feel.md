## Parent

[Finish Line Presentation And Celebration PRD](../../prd/prd-finish-line-presentation-and-celebration.md)

## What to build

Run a human visual review of the finished presentation in Unity. Confirm the final approach reads clearly in the run camera, the checkered threshold feels like a crossing line rather than a wall, confetti reads as celebration without hiding the character or result UI, and **Victory Facing** feels like part of the **Victory** animation.

This slice is intentionally HITL because camera framing, VFX density, threshold opacity, and victory-pose feel are aesthetic decisions that automated tests can guard but not fully approve.

## Acceptance criteria

- [ ] The billboard, threshold, confetti, and **Victory Facing** are visible together from the intended run camera framing.
- [ ] The threshold is readable at speed, strongest near the run surface, and transparent enough not to obscure the course.
- [ ] Confetti starts after successful crossing and reads as a reward rather than a navigation hint.
- [ ] Confetti does not obscure the result UI or the character victory pose.
- [ ] **Victory Facing** blends naturally with the beginning of **Victory** rather than snapping or fighting the character follower.
- [ ] A failed run does not play finish celebration and does not show successful **Victory Facing**.
- [ ] Any tuning changes discovered during review stay within finish presentation scope and do not reopen economy, progression, reward, or camera-system architecture.

## Verification

- EditMode tests:
  - No new EditMode tests expected unless review exposes a deterministic controller bug.
- PlayMode tests:
  - Existing end-to-end finish presentation tests remain green after tuning.
  - If a camera-framing bug is deterministic, add or extend a scene/camera smoke assertion before closing the review.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector after any code or serialized scene changes.
- Manual Unity smoke check:
  - Review desktop and representative mobile aspect ratios.
  - Run a successful finish pass and verify threshold fade, confetti timing, and **Victory Facing**.
  - Run a failure/no-finish pass and verify celebration and **Victory Facing** remain inactive.
  - Capture screenshots or short clips only if they help compare tuning; do not commit diagnostic captures unless explicitly accepted as artifacts.
- Package version/changelog:
  - No package manifest, package version, or changelog update expected.

## Blocked by

- 05 - Close End-To-End Finish Presentation Flow
