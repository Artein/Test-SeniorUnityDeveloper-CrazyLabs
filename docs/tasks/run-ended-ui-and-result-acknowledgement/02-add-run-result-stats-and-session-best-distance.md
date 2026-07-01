## Parent

[Run Ended UI And Result Acknowledgement PRD](../../prd/prd-run-ended-ui-and-result-acknowledgement.md)

## What to build

Add the result-data layer needed by **Run Ended UI**: current-run coins from the **Run Currency Snapshot**, whole-meter **Run Distance Display**, and
current **Level Session** **Best Run Distance** improvement state.

This slice should be pure gameplay/application logic. It should not create UI, mutate save data, or depend on live physics. It should consume accepted
**Run Result** values and produce deterministic display-ready state for later presentation.

## Acceptance criteria

- [ ] Current-run earned coins are derived from the accepted **Run Result** **Run Currency Snapshot**.
- [ ] Earned coins do not read total **Currency Balance**.
- [ ] **Run Distance Display** rounds distance down to whole meters.
- [ ] Sub-meter distances display as zero meters.
- [ ] The service tracks **Best Run Distance** for the current **Level Session** only.
- [ ] A failed accepted result can establish or improve **Best Run Distance**.
- [ ] A successful accepted result can establish or improve **Best Run Distance**.
- [ ] A shorter accepted result does not reduce **Best Run Distance**.
- [ ] Improvement state compares the current accepted result against the previous session best before updating the stored best.
- [ ] No save-data schema, all-time best, or app-session global best is introduced.

## Verification

- EditMode tests:
  - Distance formatting floors fractional meters and clamps negative inputs to zero.
  - Coin display data comes from **Run Currency Snapshot**.
  - First accepted result establishes session best distance.
  - Longer failed and successful results improve session best distance.
  - Shorter results leave session best distance unchanged.
  - Improvement meters are shown only when the current result beats the previous session best.
- PlayMode tests:
  - Not required for this pure logic slice.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Not required until the UI slice consumes the data.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
