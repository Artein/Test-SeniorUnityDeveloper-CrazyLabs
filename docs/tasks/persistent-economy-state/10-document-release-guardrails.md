## Parent

[Local Persistent Economy State PRD](../../prd/prd-local-persistent-economy-state.md)

## What to build

Document the release and maintenance guardrails for local economy persistence after the implementation slices land. The goal is to make future changes to save schema, currency IDs, upgrade IDs, mobile storage behavior, and economy authority explicit enough that future agents do not reintroduce PlayerPrefs, per-class saves, or insecure hard-currency assumptions.

The completed slice should leave maintainers with concise guidance on what is supported, what is deliberately out of scope, and which changes require migration review.

## Acceptance criteria

- [ ] Documentation states that economy progress is local persistent soft progress, not hard currency, IAP, server authority, anti-cheat, or cloud sync.
- [ ] Documentation states that PlayerPrefs and Unity `com.unity.serialization` must not be used for economy state.
- [ ] Documentation explains why saves are centralized around the economy aggregate instead of each class saving itself.
- [ ] Documentation summarizes mobile storage behavior for `Application.persistentDataPath`, including app update expectations, uninstall behavior, and best-effort platform backup caveats.
- [ ] Documentation lists save schema fields and migration review triggers for schema changes, ID format changes, removed content, max-level changes, and reward policy changes.
- [ ] Documentation records the first-version failure policy: log errors, continue gracefully, no rollback machinery, and refresh UI from actual central state.
- [ ] Documentation points to relevant tests or smoke checks that protect stable IDs, migration, corruption recovery, purchase commits, and run-end rewards.
- [ ] No remote issue publishing or package version bump is required unless project release tooling says otherwise.

## Verification

- EditMode tests:
  - None expected for documentation-only changes.
- PlayMode tests:
  - None expected for documentation-only changes.
- Static checks:
  - `git diff --check`.
  - Link check by reading referenced local files where practical.
- Manual Unity smoke check:
  - Not required for this slice.
- Package version/changelog:
  - No package manifest change expected. Add internal changelog or release note only if the project has an active release-notes convention for gameplay features.

## Blocked by

- 01 - Record Persistence Failure Policy And API Contract
- 04 - Persist Schema V1 Local Save
- 05 - Add Migration And Recovery
- 06 - Add Serialized Save Queue And Lifecycle Flush
- 07 - Commit Upgrade Purchases Durably
- 08 - Commit Run Rewards At Run End
- 09 - Wire Gameplay Composition And UI States
