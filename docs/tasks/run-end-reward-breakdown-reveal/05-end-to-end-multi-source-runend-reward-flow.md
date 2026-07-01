## Parent

[Run End Reward Breakdown Reveal PRD](../../prd/prd-run-end-reward-breakdown-reveal.md)

## What to build

Close the multi-source **Run Ended** reward path after picked-up coins, **Distance Bonus**, **Air Time Bonus**, and reveal sequencing exist.

This slice should prove that an accepted run can carry multiple reward sources, render them as generic rows, show a final total matching the accepted reward, commit the wallet reward once, and still follow the existing **RunEnded -> RunPreparation** acknowledgement/reset boundary.

## Acceptance criteria

- [x] Accepted **Run Reward Breakdown** can contain picked-up coins, **Distance Bonus**, and **Air Time Bonus** together.
- [x] Source rows render in deterministic upstream order.
- [x] **Run Ended UI** does not branch on source type to render rows.
- [x] Displayed total run coins equals the accepted total **Run Currency Snapshot**.
- [x] Wallet commit grants the same accepted total exactly once.
- [x] Fast-forward reveal shows all source rows and the final total immediately.
- [x] Acknowledgement remains blocked until reveal completion or fast-forward completion.
- [x] **RunEnded -> RunPreparation** reset still occurs only after acknowledgement.
- [x] Existing best-distance, title, victory/defeat, camera, and pose-lock behavior remains compatible.
- [x] Future reward contributors can add source rows without changing the **Run Ended UI** rendering path.

## Verification

- EditMode tests:
  - Breakdown total combines multiple contributors without drift.
  - Source ordering remains deterministic across multiple contributors.
  - Presenter emits generic row states for all sources without source-specific rendering branches.
  - Wallet commit uses accepted total and is independent of reveal/acknowledgement timing.
- PlayMode tests:
  - Controlled scene path displays multiple reward source rows and a matching total.
  - Fast-forward reveals all rows and total without acknowledgement.
  - Second tap after fast-forward returns to **Run Preparation**.
  - Reset, camera, pose-lock, and existing RunEnded UI behavior remain intact.
- Static checks:
  - Reformat changed C# files and inspect Rider problems.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Run a scenario that earns picked-up coins plus configured bonus rewards; confirm rows, total, fast-forward, and continuation all behave as expected.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 - Distance Bonus Run Reward Source
- 03 - Air Time Bonus Run Reward Source
- 04 - Run Reward Reveal And Fast-Forward Gate
