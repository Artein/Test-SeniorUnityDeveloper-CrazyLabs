## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Add app-session owned upgrade levels and the atomic purchase path that spends catalog currency and increments one upgrade level. The completed slice should let plain C# services answer current levels, buy the next level when affordable, reject maxed or invalid purchases, and update previews through the same source of truth.

This slice should not add persistence beyond the app session and should not put purchase rules in UI components.

## Acceptance criteria

- [ ] `IUpgradeProgressStorage` stores owned upgrade levels by upgrade id with app-session lifetime.
- [ ] Owned levels default to 0 and clamp or reject values outside 0..max according to the chosen service contract.
- [ ] `UpgradePurchaseService` is the only first-slice mutator of owned upgrade levels.
- [ ] Purchase service checks catalog membership, current level, next cost, and purchase currency before mutating state.
- [ ] Successful purchase spends currency and increments the owned upgrade level as one operation.
- [ ] Insufficient balance, max level, missing definition, and invalid definition do not spend currency or increment level.
- [ ] Purchase results distinguish success, max level, insufficient currency, missing definition, and invalid definition.
- [ ] Preview generation can use stored progress and current currency balance after purchases.
- [ ] Upgrade levels persist across runs while storage instances stay alive and reset when app-session storage is recreated.

## Verification

- EditMode tests:
  - Upgrade progress storage returns default level 0 for unknown upgrades.
  - Upgrade progress storage increments or sets levels within valid bounds.
  - Purchase succeeds with exact required currency.
  - Purchase fails with one currency less than required and leaves balance/level unchanged.
  - Purchase fails at max level and leaves balance/level unchanged.
  - Purchase fails for missing or invalid definitions and leaves balance/level unchanged.
  - Multiple purchases advance through increasing costs and preview states.
  - Recreated storage instances start from empty app-session progress.
- PlayMode tests:
  - None expected for this slice.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Use a debug or temporary harness to grant coins, buy an upgrade, and observe balance/level/preview changes.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Extract Economy Currency Grant/Spend Path
- 02 - Add Upgrade Catalog, Definitions, Progressions, And Previews
