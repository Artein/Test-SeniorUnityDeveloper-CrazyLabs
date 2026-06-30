## Parent

[Local Persistent Economy State PRD](../../prd/prd-local-persistent-economy-state.md)

## What to build

Change coin reward timing so pickups during `Running` update only the current-run accumulator and do not grant wallet currency directly. Add a run reward committer that listens to the accepted run result, applies the immutable run currency snapshot to the central wallet state once, and requests an immediate important save commit.

For this first version, failed reward save commits should log an error and continue gracefully without rollback machinery. Run-end UI and subsequent run preparation state should refresh from the central economy state instead of assuming a grant was durable if the commit failed.

## Acceptance criteria

- [ ] Accepted pickup collection during a run no longer grants directly to wallet currency storage.
- [ ] Pickup collection still updates the run currency accumulator with the final currency grant after multipliers.
- [ ] Accepted run results are the single first-version hook for applying run currency snapshots to wallet state.
- [ ] Run reward commit applies each accepted run result at most once.
- [ ] Run reward commit requests an immediate important save commit after updating central economy state.
- [ ] Save commit failure logs an error and does not crash the run-end flow.
- [ ] No rollback subsystem is introduced for failed run reward commits in this version.
- [ ] Run-end and run preparation UI can refresh from actual central economy state after success or failure.

## Verification

- EditMode tests:
  - Pickup collected during `Running` does not change wallet balance.
  - Pickup collected during `Running` updates the run currency accumulator.
  - Coin pickup multiplier still affects the run snapshot before run-end commit.
  - Accepted run result commits the snapshot to wallet exactly once.
  - Duplicate accepted notifications or repeated handling cannot double grant.
  - Save commit failure is logged and represented without throwing.
  - Run currency accumulator reset behavior remains correct when returning to preparation.
- PlayMode tests:
  - Existing pickup or run-end scene smoke tests are updated only if live scene wiring is affected in this slice.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Collect coins during a run, confirm wallet does not change until run end, then confirm run preparation shows the committed balance.
- Package version/changelog:
  - No package manifest change expected.

## Blocked by

- 01 - Record Persistence Failure Policy And API Contract
- 03 - Centralize Economy State In Memory
- 06 - Add Serialized Save Queue And Lifecycle Flush
