## Parent

[Local Persistent Economy State PRD](../../prd/prd-local-persistent-economy-state.md)

## What to build

Add migration, repair, and corruption recovery around the versioned local economy save. The loader should inspect the save envelope first, migrate known older schemas step by step, reject unknown future schemas safely, preserve unknown economy IDs, and recover from a corrupt primary file through a backup or defaults.

The completed slice should make app updates safe for economy state changes without spreading migration logic into UI, pickups, upgrades, or gameplay controllers.

## Acceptance criteria

- [ ] Save loading reads enough envelope data to route by schema version before assuming the current DTO shape.
- [ ] Stepwise migration from older known schemas produces the current schema deterministically.
- [ ] Unknown future schema versions fail safely and log an error instead of loading as current data.
- [ ] Unknown currency and upgrade IDs are preserved across load/save unless a deliberate migration tombstones them.
- [ ] Known negative soft currency balances are repaired according to the first-version policy and logged.
- [ ] Known upgrade levels above current max are clamped or repaired according to the first-version policy and logged.
- [ ] Corrupt primary save recovery tries backup before falling back to defaults.
- [ ] Corrupt or unreadable save files are retained or renamed for diagnostics when practical.

## Verification

- EditMode tests:
  - Old schema fixture migrates to current schema.
  - Missing fields in old fixtures receive explicit defaults.
  - Unknown future schema fails safely and logs.
  - Unknown currency IDs are preserved.
  - Unknown upgrade IDs are preserved.
  - Negative known balances repair deterministically.
  - Over-max known upgrade levels repair deterministically.
  - Corrupt primary with valid backup loads backup.
  - Corrupt primary and corrupt backup load defaults.
- PlayMode tests:
  - None expected for this slice.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Not required for this slice.
- Package version/changelog:
  - No package manifest change expected.

## Blocked by

- 04 - Persist Schema V1 Local Save
