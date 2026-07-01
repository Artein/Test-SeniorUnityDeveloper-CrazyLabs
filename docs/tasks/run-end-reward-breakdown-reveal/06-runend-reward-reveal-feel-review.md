## Parent

[Run End Reward Breakdown Reveal PRD](../../prd/prd-run-end-reward-breakdown-reveal.md)

## What to build

Run a human review pass for **Run Reward Reveal** feel after the AFK implementation slices are complete.

This slice is HITL because reveal timing, row copy, icon treatment, and whether any zero-value source should be shown for teaching are product/design judgments. The output should be a concise calibration note and any follow-up implementation tasks needed after review.

## Calibration notes

- Human feel review accepted treating **NEW BEST** as performance feedback, not a reward source. It should reveal immediately after reached distance and before any reward source rows.
- Human feel review accepted source-row coin amounts as unsigned white numbers, with the row coin icon revealing at the same time as label and amount.
- Human feel review accepted the final total as a distinct `RUN TOTAL` block whose icon reveals with the total value.

## Acceptance criteria

- [ ] A human reviews the result screen on a real or representative mobile aspect ratio.
- [ ] Reveal timing feels readable without making repeat runs tedious.
- [ ] Fast-forward behavior is understandable and does not accidentally acknowledge the result.
- [ ] Source labels are understandable for picked-up coins, **Distance Bonus**, and **Air Time Bonus**.
- [x] Icon/swatch treatment is either accepted or deferred with a clear follow-up.
- [ ] Zero-value row policy is confirmed for first-slice behavior.
- [x] Total run coins placement and timing are accepted.
- [ ] **Tap to Continue** appears only when the screen is ready for acknowledgement.
- [ ] Any requested tuning changes are captured as follow-up tasks or implemented if trivial and safe.

## Verification

- EditMode tests:
  - None expected unless review changes reward or reveal logic.
- PlayMode tests:
  - Rerun affected **Run Ended UI** reveal tests if any tuning changes touch behavior.
- Static checks:
  - If files change, run `git diff --check` and Unity compile through the existing connector.
- Manual Unity smoke check:
  - End failed and successful runs with multiple source rows.
  - Watch full reveal without tapping.
  - Tap during reveal to fast-forward, then tap again to continue.
  - Confirm the screen remains readable on the target mobile layout.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 04 - Run Reward Reveal And Fast-Forward Gate
- 05 - End-To-End Multi-Source RunEnd Reward Flow
