## Parent

[Run Ended UI And Result Acknowledgement PRD](../../prd/prd-run-ended-ui-and-result-acknowledgement.md)

## What to build

Run a human design/QA review of the implemented **Run Ended** experience. Confirm the scene-authored UI copy, layout, stats readability, acknowledgement
guard timing, pose hold, and terminal character presentation feel appropriate for the Ladybug downhill upgrade loop.

This issue is intentionally HITL because it approves designer-owned content and feel. It should not turn exact copy, layout, or animation timing into
brittle automated tests.

## Acceptance criteria

- [ ] Success title/copy communicates level completion clearly.
- [ ] Failure title/copy communicates progress and retry clearly.
- [ ] Coins earned, reached meters, and best-run improvement are readable on target mobile aspect ratios.
- [ ] Tap prompt is understandable and does not conflict with **Run Preparation** **Continue** semantics.
- [ ] The acknowledgement guard feels long enough to prevent accidental skips and short enough to avoid sluggishness.
- [ ] **Victory** and **Defeat** presentation are readable while the result UI is visible.
- [ ] The held result pose feels intentional and does not expose obvious physics jitter.
- [ ] Any desired copy/layout/art tweaks are made in scene-authored UI assets, not hard-coded into gameplay logic.

## Verification

- EditMode tests:
  - Not required for designer copy and feel.
- PlayMode tests:
  - Existing scene UI contract tests should remain green after any designer-authored adjustments.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector if scene/code assets changed.
- Manual Unity smoke check:
  - Review at least one failed run and one successful run in editor Play Mode.
  - Review on the current target portrait aspect ratio.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 06 - Close End-To-End Run Ended Gameplay Loop
