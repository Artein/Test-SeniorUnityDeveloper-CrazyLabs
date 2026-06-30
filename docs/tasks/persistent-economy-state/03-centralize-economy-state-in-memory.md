## Parent

[Local Persistent Economy State PRD](../../prd/prd-local-persistent-economy-state.md)

## What to build

Introduce a centralized in-memory player economy aggregate that owns soft currency balances and upgrade levels before any disk persistence is added. Economy should store durable identity as strings, while gameplay-facing compatibility surfaces can continue to accept currency definitions and upgrade definitions where that keeps call sites small.

The completed slice should make currency balance reads/writes and upgrade progress reads/writes share one source of truth inside the app session, while preserving module direction: Economy owns generic state, and upgrade-specific adapters translate upgrade definitions to stable IDs outside the Economy module.

## Acceptance criteria

- [ ] A single player economy state owns currency balances by currency save ID and upgrade levels by upgrade stable ID.
- [ ] Runtime lookup remains efficient without exposing DTO layout to gameplay controllers.
- [ ] Currency storage compatibility reads and writes through the central economy state.
- [ ] Upgrade progress compatibility reads and writes through the central economy state using upgrade stable IDs.
- [ ] Unknown saved IDs can be represented in state without requiring current content definitions.
- [ ] Invalid currency definitions without save IDs and upgrade definitions without stable IDs fail through explicit validation or safe non-success behavior.
- [ ] Existing run preparation previews and purchase tests can use the centralized state without changing player-facing behavior yet.
- [ ] No file I/O, serialization, PlayerPrefs, or background queue is introduced in this slice.

## Verification

- EditMode tests:
  - Currency balance defaults to zero and can be granted/spent through the compatibility surface.
  - Upgrade level defaults to zero and can be set/read across definition instances with the same stable ID.
  - Currency and upgrade state are visible from the same aggregate after mutations.
  - Unknown ID entries can be preserved in aggregate state.
  - Invalid or missing IDs are handled deterministically.
  - Existing purchase and preview behavior remains compatible with in-memory state.
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
