## Parent

[Local Persistent Economy State PRD](../../prd/prd-local-persistent-economy-state.md)

## What to build

Wire the persistent economy stack into gameplay composition so run preparation, upgrades, pickups, run-end rewards, and scene smoke paths all use the loaded central economy state. The player should see loaded balances and upgrade levels before interacting with run preparation, and UI should represent loading, pending commit, failed commit, and loaded states without putting save rules inside MonoBehaviours.

The completed slice should preserve shallow views, explicit VContainer registration, and one integrated gameplay scene smoke path that proves serialized scene references and service composition work together.

## Acceptance criteria

- [ ] Gameplay composition registers the central economy state, local save store, migration/recovery services, save queue, purchase path, and run reward committer through VContainer.
- [ ] Run preparation waits for or reacts to economy load before rendering spendable balances and upgrade previews.
- [ ] UI-facing presenter state can show loading, loaded, purchase pending, purchase failed, and save failed states where relevant.
- [ ] UI refreshes from central economy state after successful and failed commits.
- [ ] Existing upgrade preview and purchase UI behavior remains consistent with the new persistence-backed state.
- [ ] Existing pickup scene composition still validates pickup definitions and contacts after wallet grants move to run end.
- [ ] App-session defaults still work when no save file exists.
- [ ] No unrelated UI redesign, prefab reorganization, package change, or scene cleanup is included.

## Verification

- EditMode tests:
  - Gameplay lifetime composition can resolve the new economy persistence services.
  - Run preparation presenter renders loaded balances and upgrade levels from central state.
  - Run preparation presenter shows pending or failed purchase/save states where applicable.
  - Existing composition validation reports missing required references clearly.
- PlayMode tests:
  - Gameplay scene composition resolves persistence services through VContainer.
  - Run preparation smoke path displays loaded balance and upgrade progress.
  - Optional lifecycle smoke covers pause/quit flush if not already covered in slice 06.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Start scene, buy an upgrade, complete a run with coins, return to run preparation, restart the app/editor play session, and confirm loaded state matches the saved economy file.
- Package version/changelog:
  - No package manifest change expected.

## Blocked by

- 01 - Record Persistence Failure Policy And API Contract
- 04 - Persist Schema V1 Local Save
- 06 - Add Serialized Save Queue And Lifecycle Flush
- 07 - Commit Upgrade Purchases Durably
- 08 - Commit Run Rewards At Run End
