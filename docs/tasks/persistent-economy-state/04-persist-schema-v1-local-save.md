## Parent

[Local Persistent Economy State PRD](../../prd/prd-local-persistent-economy-state.md)

## What to build

Add the first versioned local economy save that can persist and load the centralized player economy state from Unity persistent data. The save shape should be compact JSON DTOs with field-based list entries, `schemaVersion`, and a revision or commit counter.

This slice should prove a first launch with no save produces defaults, and a normal save/load round trip restores currency balances and upgrade levels without touching Unity objects or live gameplay services during serialization.

## Acceptance criteria

- [ ] Economy save paths are resolved through an injected persistent data path provider.
- [ ] The current save DTO includes `schemaVersion`, revision, currency balance entries, upgrade level entries, and room for unknown entries where needed.
- [ ] DTOs use lists instead of dictionaries so Unity `JsonUtility` can serialize them directly.
- [ ] Runtime state maps are separated from serialized DTO layout.
- [ ] A missing save file loads deterministic first-launch defaults.
- [ ] A valid save file restores currency balances and upgrade levels into the central state.
- [ ] Save writes use a temporary file before replacing the primary save.
- [ ] No PlayerPrefs, Unity `com.unity.serialization`, UniTask, direct Newtonsoft dependency, Unity Jobs, or Burst path is added.

## Verification

- EditMode tests:
  - Loading with no save file creates default economy state.
  - Saving and loading balances restores amounts by currency save ID.
  - Saving and loading upgrade progress restores levels by upgrade stable ID.
  - DTO serialization uses list entries and round trips with `JsonUtility`.
  - Test file paths are injected temporary paths, not real device persistent data.
  - Temporary-write behavior does not leave a partial primary file on simulated write failure where practical.
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

- 02 - Add Stable Currency Save IDs
- 03 - Centralize Economy State In Memory
