## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Add the authored upgrade data path: gameplay stat ids, upgrade definitions, upgrade progressions, catalog ordering, icon references, validation, and immutable preview models. The slice should let designers author the four first upgrade definitions with curve-driven cost/effect progression, exact overrides, level 0 baseline semantics, and catalog-wide purchase currency.

This slice should prove authoring and preview math without depending on the final purchase service or run preparation UI.

## Acceptance criteria

- [ ] `GameplayStatId` assets can identify `SlingshotLaunchPower`, `PlayerMaxSpeed`, `PlayerSteeringResponsiveness`, and `CoinPickupMultiplier`.
- [ ] `UpgradeDefinition` assets contain stable upgrade id, display metadata, icon, target gameplay stat id, max level, cost progression, effect progression, operation type, and formatting metadata.
- [ ] `UpgradeCatalog` is a ScriptableObject with ordered upgrade definitions and one catalog-wide purchase currency.
- [ ] Upgrade progressions support min/max projection, normalized `AnimationCurve`, rounding or step rules, and exact level overrides.
- [ ] Exact level overrides win over curve-projected values.
- [ ] Level 0 is the baseline effect level; purchase costs are defined for levels 1 through max level.
- [ ] Preview generation can produce current effect, next effect, next cost, affordability, maxed state, and invalid-definition state from supplied current level and balance inputs.
- [ ] Validation catches missing icons, missing stat ids, duplicate upgrade ids, invalid max levels, invalid curves, invalid overrides, and missing purchase currency.
- [ ] Four initial upgrade definitions and a catalog asset exist with placeholder or final icon references assigned.

## Verification

- EditMode tests:
  - Progression evaluation handles level 0, first level, max level, nonlinear curve values, rounding, step behavior, and exact overrides.
  - Cost progression rejects level 0 cost queries and supports levels 1 through max level.
  - Effect progression supports levels 0 through max level.
  - Catalog validation catches duplicates, null entries, missing purchase currency, and invalid definitions.
  - Preview generation handles affordable, unaffordable, maxed, invalid definition, current effect, next effect, and next cost states.
- PlayMode tests:
  - None expected unless asset validation requires Unity runtime behavior.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Inspect the four initial upgrade assets and catalog; confirm icons, stats, max levels, curves, and purchase currency are assigned.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Extract Economy Currency Grant/Spend Path
