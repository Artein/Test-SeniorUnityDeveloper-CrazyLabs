## Parent

[Local Persistent Economy State PRD](../../prd/prd-local-persistent-economy-state.md)

## What to build

Add stable save identity to currency definitions so balances can be serialized without depending on asset names, folders, Unity instance IDs, or object references. A newly authored currency should receive an ID automatically in the Editor, existing currency assets should keep that ID across rename and move operations, and duplicate or missing IDs should be invalid authoring before persistence depends on them.

The completed slice should make the existing coin currency definition persistence-ready without changing pickup grant values or upgrade purchase tuning.

## Acceptance criteria

- [ ] Currency definitions serialize a stable save ID and expose it through a readonly runtime property.
- [ ] New currency definitions receive a save ID automatically in normal Editor authoring when the serialized ID is blank.
- [ ] Renaming or moving a currency asset does not change the save ID.
- [ ] Normal inspector workflows display the save ID readonly and do not offer accidental regeneration.
- [ ] Existing coin currency content has a stable save ID assigned.
- [ ] Missing currency save IDs are reported as authoring errors after editor generation has had a chance to run.
- [ ] Duplicate currency save IDs are reported as authoring errors, including duplicated assets that copied serialized fields.
- [ ] Tests use temporary assets or explicit test hooks, not hardcoded production asset paths or GUIDs.

## Verification

- EditMode tests:
  - New currency definitions can generate a non-empty stable save ID.
  - Generated save IDs remain stable when other serialized fields change.
  - Duplicate save IDs are reported by validation.
  - Missing save IDs are reported by validation.
  - Existing icon authoring behavior still works.
- PlayMode tests:
  - None expected for this slice.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Select the coin currency definition and confirm a readonly save ID is visible.
- Package version/changelog:
  - No package manifest change expected.

## Blocked by

None - can start immediately
