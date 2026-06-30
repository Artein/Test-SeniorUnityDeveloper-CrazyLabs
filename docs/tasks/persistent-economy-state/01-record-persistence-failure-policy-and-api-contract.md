## Parent

[Local Persistent Economy State PRD](../../prd/prd-local-persistent-economy-state.md)

## What to build

Record and enforce the approved first-version persistence failure policy before deeper save work lands. Economy save and commit paths should log errors, continue gracefully, avoid rollback machinery for this version, and make callers refresh UI from the actual central economy state after any failed operation.

This slice should also settle the local platform backup stance for the first release: default iOS/Android platform backup behavior is acceptable for small soft-progress files, but it must not be described as cloud sync or relied on for guaranteed recovery.

## Acceptance criteria

- [ ] Local economy save APIs return a structured result that distinguishes success from failure and carries enough diagnostic context for logging.
- [ ] Save load, save commit, purchase commit, run reward commit, pause flush, and quit flush failures are defined to log an error and continue gracefully.
- [ ] No rollback subsystem, transaction replay system, or pending-recovery queue is introduced in this first version.
- [ ] Callers treat save failure as a reason to re-read the central economy state before updating UI, so UI reflects what the player actually has in the current session.
- [ ] Purchase and run-end result contracts can represent a save failure or partial durability warning without crashing gameplay.
- [ ] Platform backup policy is documented as local persistent data that may be backed up by the OS, not as product-owned cloud sync.
- [ ] The policy does not add PlayerPrefs, Unity `com.unity.serialization`, UniTask, Newtonsoft, server authority, or encryption requirements.

## Verification

- EditMode tests:
  - Save result and commit result contracts cover success and failure states.
  - A failing save dependency is logged and returned as failure without throwing from normal gameplay-facing paths.
  - UI-facing presenters or view models refresh from current state after a failed commit result where applicable.
- PlayMode tests:
  - None expected for policy-only contracts unless lifecycle hooks are implemented in this slice.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector if code contracts are added.
- Manual Unity smoke check:
  - Not required for this slice.
- Package version/changelog:
  - No package manifest change expected.

## Blocked by

None - can start immediately
