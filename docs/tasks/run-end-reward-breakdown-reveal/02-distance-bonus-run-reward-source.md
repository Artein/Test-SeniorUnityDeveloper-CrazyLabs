## Parent

[Run End Reward Breakdown Reveal PRD](../../prd/prd-run-end-reward-breakdown-reveal.md)

## What to build

Add **Distance Bonus** as a second **Run Reward Contributor** that converts accepted run distance into coins and appears as a generic **Run Reward Source Row**.

The distance reward should use accepted **Run Result** distance facts, not live counters. **Run Distance Display** remains the meters shown to the player, while **Distance Bonus** is reward math with its own conversion and rounding. The **Run Ended UI** should continue to render source rows generically and should not know distance reward rules.

## Acceptance criteria

- [x] A distance reward contributor can produce a **Distance Bonus** source entry from accepted run distance.
- [x] **Distance Bonus** uses deterministic conversion and rounding rules from authored/configured values.
- [x] **Run Distance Display** remains separate from distance reward calculation.
- [x] Zero-value **Distance Bonus** rows are hidden by default.
- [x] Contributor-level row order places **Distance Bonus** in the intended source sequence after picked-up coins unless configured otherwise.
- [x] Accepted **Run Reward Breakdown** includes **Distance Bonus** when it awards coins.
- [x] Accepted total **Run Currency Snapshot** includes picked-up coins plus **Distance Bonus**.
- [x] **Run Ended UI** renders the **Distance Bonus** row through the same generic row path as picked-up coins.
- [x] Wallet commit grants the combined accepted total once.
- [x] No save-data schema change is introduced.

## Verification

- EditMode tests:
  - Distance contributor converts accepted distance into the expected coin amount.
  - Distance reward rounding is deterministic and does not affect **Run Distance Display** formatting.
  - Zero-value distance contribution is hidden by default.
  - Breakdown total combines picked-up coins and **Distance Bonus**.
  - Presenter keeps source rows ordered by upstream source order.
- PlayMode tests:
  - RunEnded scene/UI path can display picked-up coins and **Distance Bonus** rows together in a controlled setup.
  - Displayed total matches the accepted total when distance bonus is present.
- Static checks:
  - Reformat changed C# files and inspect Rider problems.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - End a run with non-zero distance and confirm **Distance Bonus** appears only when configured to award coins.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Picked-Up Coins Run Reward Source Tracer Bullet
