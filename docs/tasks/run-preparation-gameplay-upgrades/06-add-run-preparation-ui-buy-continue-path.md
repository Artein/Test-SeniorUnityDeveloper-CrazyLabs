## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Add the first usable **Run Preparation** UI path. The UI should display app-session coin balance, ordered upgrade previews with icons, current level, current and next effect, next cost, maxed state, unaffordable state, buy commands, and a `Continue` command. The UI should be shallow: it renders immutable preview models and forwards buy/continue actions to injected controllers or services.

This slice should make upgrade purchasing visible and usable before a run, while relying on the already-built purchase and state handoff services for rules.

## Acceptance criteria

- [ ] Run Preparation UI displays current coin balance from currency storage.
- [ ] Run Preparation UI displays upgrade entries in catalog order.
- [ ] Each upgrade entry displays icon, name, current level, current effect, next effect, and next cost when available.
- [ ] Unaffordable upgrade purchases are disabled or clearly non-actionable.
- [ ] Maxed upgrades show a maxed state and no next purchase cost.
- [ ] Buy command calls the purchase service and refreshes balance and previews immediately after success or failure.
- [ ] Purchase failure states are handled without corrupting UI state.
- [ ] Continue command calls the Run Preparation handoff and does not start the run directly.
- [ ] UI MonoBehaviours do not calculate costs, effects, stat modifiers, or purchase rules.
- [ ] Scene views are registered through interfaces and VContainer composition, not injected as concrete MonoBehaviours.

## Verification

- EditMode tests:
  - UI presenter/controller maps preview models to view state without using Unity lifecycle.
  - Buy success refreshes balance and preview state.
  - Buy insufficient funds leaves level unchanged and renders unaffordable state.
  - Maxed previews render non-purchasable state.
  - Continue command delegates to the state handoff boundary.
- PlayMode tests:
  - Gameplay scene Run Preparation UI renders the four catalog upgrades.
  - Buy button updates visible balance, level, cost, and effect preview.
  - Unaffordable and maxed entries are not purchasable.
  - Continue button transitions to Pre-Launch.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Launch the gameplay scene, buy an upgrade with available coins, observe UI refresh, then press Continue and verify Slingshot capture becomes available.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 03 - Add Upgrade Progress And Atomic Purchase Flow
- 05 - Add Run Preparation State And Continue Handoff
