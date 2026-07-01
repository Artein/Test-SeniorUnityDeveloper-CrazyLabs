## Parent

[Run End Reward Breakdown Reveal PRD](../../prd/prd-run-end-reward-breakdown-reveal.md)

## What to build

Add **Run Air Time** tracking and **Air Time Bonus** as a **Run Reward Contributor**.

Gameplay **Run Air Time** should measure unsupported traversal time during **Running**. It must not read character animation state or **Character Presentation Mode.Airborne**. When a run is accepted, the air-time contributor converts accepted run air time into a generic **Run Reward Source** entry that the **Run Ended UI** renders like any other source row.

## Acceptance criteria

- [x] **Run Air Time** is tracked only while gameplay state is **Running**.
- [x] Time with supported **Run Surface** context is not counted as **Run Air Time**.
- [x] Unsupported traversal time is counted as gameplay **Run Air Time**.
- [x] Air-time tracking resets at the correct run lifecycle boundary before a new run.
- [x] **Air Time Bonus** converts accepted **Run Air Time** into coins using deterministic authored/configured values.
- [x] Zero-value **Air Time Bonus** rows are hidden by default.
- [x] Accepted **Run Reward Breakdown** includes **Air Time Bonus** when it awards coins.
- [x] Accepted total **Run Currency Snapshot** includes picked-up coins, optional **Distance Bonus**, and optional **Air Time Bonus**.
- [x] **Run Ended UI** renders **Air Time Bonus** through the generic row path.
- [x] No character presentation or animation classifier API is required for reward calculation.

## Verification

- EditMode tests:
  - **Run Air Time** tracker counts only during **Running**.
  - Supported surface frames do not increase air-time.
  - Unsupported frames increase air-time using deterministic time input.
  - Tracker reset clears air-time before a new run.
  - Air-time contributor converts accepted air-time into the expected reward.
  - Zero-value air-time contribution is hidden by default.
- PlayMode tests:
  - Controlled gameplay scene path can display **Air Time Bonus** as a generic reward row when accepted run facts include air time.
  - Displayed total matches the accepted total when air-time bonus is present.
- Static checks:
  - Reformat changed C# files and inspect Rider problems.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Trigger a run with airtime and confirm **Air Time Bonus** appears when configured to award coins.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Picked-Up Coins Run Reward Source Tracer Bullet
