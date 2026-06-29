## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Add the central stat-resolution path that turns owned upgrade levels into immutable run modifiers and resolves gameplay values from caller-owned base values. The completed slice should let gameplay build a frozen run modifier snapshot from the upgrade catalog and progress storage, expose the active snapshot through a provider, and resolve `GameplayStatId` values through deterministic operation ordering.

This slice should be plain C# first. It should not wire Slingshot, movement, pickup, or UI behavior yet beyond tests/fakes proving those callers can use the resolver.

## Acceptance criteria

- [ ] `RunModifierSnapshotFactory` builds immutable run modifier snapshots from the upgrade catalog and current upgrade progress.
- [ ] Active snapshot read access is exposed through a small holder/provider without static registries or scene searches.
- [ ] `GameplayStatResolver` resolves a supplied base value for a supplied gameplay stat id.
- [ ] The resolver does not read Slingshot, movement, pickup, economy, or upgrade authoring configs directly beyond composed modifier sources.
- [ ] Modifier sources are provided explicitly through composition.
- [ ] Operation order is deterministic: `FlatAdd`, `AdditivePercent`, `MultiplicativeFactor`, `ClampMin`, then `ClampMax`.
- [ ] The four first upgrade stats can contribute `MultiplicativeFactor` modifiers from the run snapshot.
- [ ] Runtime changes to upgrade progress after snapshot creation do not mutate the active snapshot.
- [ ] The resolver supports future live modifier sources without changing gameplay callers.

## Verification

- EditMode tests:
  - Snapshot factory converts current upgrade levels into expected runtime modifiers.
  - Snapshot remains unchanged after later upgrade progress changes.
  - Resolver returns base value when no modifiers apply.
  - Resolver applies multiple modifier operations in the required order.
  - Resolver applies only modifiers for the requested gameplay stat id.
  - Resolver combines snapshot modifiers and fake live modifier sources deterministically.
  - Resolver output is deterministic for identical inputs.
- PlayMode tests:
  - None expected for this slice.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - None expected beyond inspecting test assets or temporary harness output.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 - Add Upgrade Catalog, Definitions, Progressions, And Previews
- 03 - Add Upgrade Progress And Atomic Purchase Flow
