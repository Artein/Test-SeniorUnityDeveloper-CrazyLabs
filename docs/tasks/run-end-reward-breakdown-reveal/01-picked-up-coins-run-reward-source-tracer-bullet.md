## Parent

[Run End Reward Breakdown Reveal PRD](../../prd/prd-run-end-reward-breakdown-reveal.md)

## What to build

Add the first end-to-end **Run Reward Breakdown** tracer bullet using picked-up coins.

When a run is accepted, picked-up coin grants should aggregate into one **Run Reward Source** entry. The accepted **Run Result** should carry both the total **Run Currency Snapshot** and the source-level **Run Reward Breakdown**. The scene-authored **Run Ended UI** should render at least one generic **Run Reward Source Row** for picked-up coins without knowing pickup mechanics.

This slice should keep reward commit stable: the wallet grant remains derived from the accepted run total, and the displayed total must match that accepted total.

## Acceptance criteria

- [x] Accepted **Run Result** contains a **Run Reward Breakdown** with ordered generic source entries.
- [x] Picked-up coin grants aggregate into one picked-up coin **Run Reward Source**.
- [x] Multiple picked-up coin grants in one run produce one row whose amount is the summed final grant amount.
- [x] The total **Run Currency Snapshot** is derived from the same source entries used by the UI rows.
- [x] Existing wallet commit behavior grants the accepted total once and does not inspect UI rows.
- [x] **Run Ended** presenter state exposes ordered generic source-row view states.
- [x] **Run Ended UI** has serialized row container and row prefab references.
- [x] The view instantiates row views from the serialized row prefab and initializes them from generic row state.
- [x] Runtime does not create the whole **Run Ended UI** hierarchy.
- [x] A code TODO near row instantiation notes that pooling can be introduced later if row churn becomes a measured issue.
- [x] Existing title, reached-distance, best-improvement, acknowledgement, pose-lock, and camera behavior remain compatible.

## Verification

- EditMode tests:
  - **Run Reward Breakdown** builds a total snapshot from source entries.
  - Picked-up coin grants aggregate by source type.
  - Zero picked-up coins do not create a visible picked-up coin row by default.
  - **Run Ended** presenter maps breakdown entries into ordered generic row states.
  - **Run Reward Committer** grants the accepted total once from the accepted result.
- PlayMode tests:
  - Gameplay scene **Run Ended UI** has row container and row prefab references assigned.
  - Row prefab has required label and amount view references.
  - Entering **Run Ended** with picked-up coins shows a picked-up coin source row and matching total.
  - No duplicate **Run Ended UI** panel is created in Play Mode.
- Static checks:
  - Reformat changed C# files and inspect Rider problems.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Collect at least one coin, end the run, and confirm the result screen shows a picked-up coins row whose amount matches the earned total.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
