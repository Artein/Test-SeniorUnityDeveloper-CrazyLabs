## Parent

[Local Persistent Economy State PRD](../../prd/prd-local-persistent-economy-state.md)

## What to build

Update upgrade purchasing so a purchase applies to the centralized economy state and immediately requests a durable save commit. The purchase path should still validate catalog membership, current level, next cost, max level, and affordability, but the success and failure results must now reflect save commit behavior as well as gameplay validation.

For this first version, do not add rollback machinery. If a save commit fails after in-memory state changes, log the error, return or publish a save-failed result, continue gracefully, and make UI refresh from the central economy state so the screen reflects what the player actually has in the current session.

## Acceptance criteria

- [ ] Upgrade purchase spends currency and advances upgrade level through the centralized economy state.
- [ ] Successful purchase requests an immediate important save commit.
- [ ] Purchase result semantics distinguish normal validation failures from save commit failure or save warning states.
- [ ] Insufficient balance, max level, missing definition, and invalid definition do not mutate currency or level.
- [ ] Save commit failure logs an error and does not crash gameplay.
- [ ] No rollback subsystem is introduced for save commit failure in this version.
- [ ] UI-facing code can disable rapid duplicate purchase taps while an important commit is pending.
- [ ] Preview generation reflects the actual central economy state after success, validation failure, or save failure.

## Verification

- EditMode tests:
  - Purchase succeeds with exact required currency and commits save.
  - Purchase fails with one currency less than required and leaves balance/level unchanged.
  - Purchase fails at max level and leaves balance/level unchanged.
  - Purchase fails for missing or invalid definitions and leaves balance/level unchanged.
  - Save commit failure is logged and represented in the purchase result without throwing.
  - Multiple purchases advance through increasing costs and persisted state revisions.
  - Preview state refreshes from central economy state after a failed commit result.
- PlayMode tests:
  - None expected unless UI pending state cannot be covered through presenter tests.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Grant coins in a debug harness, buy an upgrade, close/reopen, and confirm balance/level are present when save commit succeeds.
- Package version/changelog:
  - No package manifest change expected.

## Blocked by

- 01 - Record Persistence Failure Policy And API Contract
- 03 - Centralize Economy State In Memory
- 06 - Add Serialized Save Queue And Lifecycle Flush
