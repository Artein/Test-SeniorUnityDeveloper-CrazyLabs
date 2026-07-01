## Parent

[Run Ended UI And Result Acknowledgement PRD](../../prd/prd-run-ended-ui-and-result-acknowledgement.md)

## What to build

Hold the **Launch Target** gameplay body at the accepted **Run Result** pose for the whole **Run Ended** phase. The body should not continue sliding,
falling, or drifting while the player reads the result screen.

The pose lock should affect gameplay physics ownership, not character presentation animation. Visual children should still be able to play terminal
presentation while the underlying body remains held.

## Acceptance criteria

- [ ] When **Run Result** is accepted, the **Launch Target** is held at the accepted result position.
- [ ] The held body does not continue accumulating visible physics displacement during **Run Ended**.
- [ ] The pose lock remains active until **Acknowledge Run Result** returns the game to **Run Preparation** or existing reset behavior supersedes it.
- [ ] The pose lock is released/reset cleanly before the next run.
- [ ] The implementation does not block the character visual child from playing **Victory** or **Defeat**.
- [ ] The lock is accessed through a gameplay-facing boundary rather than direct UI ownership.

## Verification

- EditMode tests:
  - Pose-lock controller/service requests hold on accepted **Run Result**.
  - Pose-lock controller/service releases when returning to **Run Preparation**.
  - The lock ignores duplicate result notifications for the same held state.
- PlayMode tests:
  - If Rigidbody behavior is involved, entering **Run Ended** holds the target position over multiple physics ticks.
  - Returning to **Run Preparation** releases or resets the held target through the existing reset path.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - End a run on a slope and confirm the character stays at the result location while the result UI is visible.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Acknowledge Run Result Flow
