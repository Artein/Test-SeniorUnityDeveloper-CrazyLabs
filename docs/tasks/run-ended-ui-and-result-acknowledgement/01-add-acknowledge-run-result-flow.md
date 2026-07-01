## Parent

[Run Ended UI And Result Acknowledgement PRD](../../prd/prd-run-ended-ui-and-result-acknowledgement.md)

## What to build

Turn **Run Ended** from a transient auto-return phase into a guarded, player-acknowledged phase. When **Run End Flow** accepts a **Run Result**, it
should transition into **Run Ended**, publish the accepted result as it does today, then wait for **Acknowledge Run Result** before returning to
**Run Preparation**.

The acknowledgement path should be owned by **Run End Flow**, not by UI or presentation code. The first acknowledgement input shortly after entering
**Run Ended** should be ignored by the **Run Ended Acknowledge Guard** so accidental launch/end taps do not skip the result phase.

## Acceptance criteria

- [ ] Accepted **Run Result** still transitions the game into **Run Ended**.
- [ ] **Run Ended** no longer exits to **Run Preparation** automatically through the old transient delay path.
- [ ] A new **Acknowledge Run Result** command can request the **RunEnded -> RunPreparation** transition.
- [ ] Acknowledgement is accepted only while the current state is **Run Ended** and a result is awaiting acknowledgement.
- [ ] Acknowledgement before the configured guard duration does not transition out of **Run Ended**.
- [ ] Acknowledgement after the configured guard duration transitions to **Run Preparation** for successful and failed results.
- [ ] The accepted **Run Result** notification still fires before acknowledgement so reward commit and character presentation continue to work.
- [ ] Existing run-end candidate latching still prevents duplicate results for one run.
- [ ] The default guard value is `0.25s` unless a project config value overrides it.

## Verification

- EditMode tests:
  - Accepted result enters **Run Ended** and publishes one **Run Result**.
  - Fixed ticks beyond the old delay do not return to **Run Preparation** without acknowledgement.
  - Acknowledgement outside **Run Ended** is ignored.
  - Acknowledgement before the guard expires is ignored.
  - Acknowledgement after the guard expires returns to **Run Preparation**.
  - Reward commit subscribers still receive **Run Result Accepted** before acknowledgement.
- PlayMode tests:
  - Not required for this slice unless the implementation needs scene-bound command wiring.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - End a run and confirm the game stays in **Run Ended** until acknowledged.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
