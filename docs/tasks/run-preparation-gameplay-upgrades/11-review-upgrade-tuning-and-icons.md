## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Run the human-in-the-loop tuning and icon review for the first upgrade set. The completed slice should confirm or revise the initial prices, max levels, effect ranges, curve shapes, exact overrides, effect formatting, UI copy, and four upgrade icons after the end-to-end upgrade smoke path exists.

This is intentionally HITL because it depends on design judgment and gameplay feel, not only engineering correctness.

## Acceptance criteria

- [ ] Slingshot launch power price curve, max level, effect range, exact overrides, and formatting are reviewed and either approved or revised.
- [ ] Player max speed price curve, max level, effect range, exact overrides, and formatting are reviewed and either approved or revised.
- [ ] Player steering responsiveness price curve, max level, effect range, exact overrides, and formatting are reviewed and either approved or revised.
- [ ] Coin pickup multiplier price curve, max level, effect range, exact overrides, and formatting are reviewed and either approved or revised.
- [ ] Four upgrade icons are reviewed in the Run Preparation UI context and either approved or replaced.
- [ ] UI text for levels, costs, current effect, next effect, maxed state, insufficient funds, and Continue is reviewed and either approved or revised.
- [ ] Revised authored assets remain valid after validation checks.
- [ ] Any intentionally unresolved balance concerns are captured as follow-up notes rather than hidden assumptions.
- [ ] The final reviewed content still supports the end-to-end smoke path.

## Verification

- EditMode tests:
  - Existing progression, catalog, preview, stat resolver, launch, movement, and pickup multiplier tests remain green after tuning changes.
- PlayMode tests:
  - Existing Run Preparation and gameplay scene smoke tests remain green after tuning/icon changes.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
  - Targeted tests through Unity AI Agent Connector after compile is clean.
- Manual Unity smoke check:
  - Review the scene with a human designer/developer: buy each upgrade, inspect labels/icons, Continue, launch, steer, collect coins, and return to Run Preparation.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 - Add Upgrade Catalog, Definitions, Progressions, And Previews
- 06 - Add Run Preparation UI Buy/Continue Path
- 10 - Wire Gameplay Scene Upgrade Smoke Path
