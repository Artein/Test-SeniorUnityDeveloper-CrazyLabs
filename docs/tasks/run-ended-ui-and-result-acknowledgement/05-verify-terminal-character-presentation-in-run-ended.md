## Parent

[Run Ended UI And Result Acknowledgement PRD](../../prd/prd-run-ended-ui-and-result-acknowledgement.md)

## What to build

Verify and harden terminal character presentation during the acknowledged **Run Ended** phase. Successful accepted **Run Result** values should drive
**Victory** presentation, failed accepted results should drive **Defeat**, and that presentation should remain stable while **Run Ended** waits for
acknowledgement.

This slice should prefer preserving the existing character presentation model. It should only change production code if the longer **Run Ended** phase
reveals a real reset or classification gap.

## Acceptance criteria

- [ ] Successful accepted **Run Result** selects **Victory** presentation.
- [ ] Failed accepted **Run Result** selects **Defeat** presentation.
- [ ] Terminal presentation remains available during the acknowledged **Run Ended** wait.
- [ ] Returning to **Run Preparation** clears accepted-result presentation state before the next run.
- [ ] UI title copy remains independent from **Victory** and **Defeat** presentation mode names.
- [ ] The implementation does not introduce new animation assets unless existing clips are missing or broken.

## Verification

- EditMode tests:
  - Existing classifier tests cover successful result to **Victory**.
  - Existing classifier tests cover failed result to **Defeat**.
  - Presenter tests prove accepted-result state persists while **Run Ended** is active.
  - Presenter tests prove accepted-result state clears on **Run Preparation**.
- PlayMode tests:
  - Scene composition still resolves the character presentation view and classifier.
  - If needed, a scene smoke test verifies terminal mode is applied while result UI is visible.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Finish a run and fail a run; confirm **Victory** and **Defeat** presentation are visible while the result screen is visible.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Acknowledge Run Result Flow
