## Parent

[Local Persistent Economy State PRD](../../prd/prd-local-persistent-economy-state.md)

## What to build

Move JSON serialization and file I/O behind a serialized save queue that accepts immutable DTO snapshots from the main thread. Gameplay services should create snapshots after mutating central economy state, enqueue important commits, and await completion only where gameplay semantics need an honest durability result.

The completed slice should avoid file I/O per pickup, preserve commit order, prevent overlapping writes, and request a best-effort flush on mobile lifecycle events without touching Unity objects from the worker path.

## Acceptance criteria

- [ ] Save queue has a single-writer execution model that preserves commit order.
- [ ] Queue work receives immutable DTO snapshots built on the main thread.
- [ ] Background work serializes DTOs and writes files without reading ScriptableObjects, MonoBehaviours, scene objects, catalogs, or mutable gameplay state.
- [ ] Important commits can await completion and return the structured save result from slice 01.
- [ ] Non-critical flushes can be coalesced to avoid excessive disk writes.
- [ ] App pause and quit hooks request a best-effort flush and log failures without crashing.
- [ ] Queue cancellation or disposal during shutdown is deterministic and does not leave overlapping workers.
- [ ] No Unity Jobs, Burst, UniTask, PlayerPrefs, or new package dependency is introduced.

## Verification

- EditMode tests:
  - Multiple enqueued commits complete in order.
  - Later commits cannot be overwritten by earlier worker completions.
  - Important commit returns success from a successful file store.
  - Important commit returns failure from a failing file store and logs.
  - Coalesced non-critical flushes reduce redundant writes where implemented.
  - Worker path receives DTO snapshots and does not require Unity objects.
- PlayMode tests:
  - Lifecycle flush hook is covered if implemented through MonoBehaviour callbacks.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Optional: pause or quit during a debug session and confirm no visible error loop or hang.
- Package version/changelog:
  - No package manifest change expected.

## Blocked by

- 01 - Record Persistence Failure Policy And API Contract
- 04 - Persist Schema V1 Local Save
- 05 - Add Migration And Recovery
