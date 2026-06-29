## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Wire the completed upgrade path into the gameplay scene as one verifiable smoke path. The scene should start in Run Preparation, display coin balance and four upgrade entries, allow a purchase when funded, Continue into Pre-Launch with a frozen run modifier snapshot, launch through gameplay-owned impulse calculation, run with movement and pickup modifiers available, end the run, and return to Run Preparation.

This slice is the integration gate for scene assets, VContainer composition, serialized references, state ids, upgrade catalog assets, launch config, UI view registration, and smoke tests.

## Acceptance criteria

- [ ] Gameplay scene composition resolves required economy, upgrade, stat resolver, run snapshot, state, UI, launch, movement, and pickup services.
- [ ] Gameplay scene starts in Run Preparation by default.
- [ ] Run Preparation UI displays the app-session coin balance and four upgrade entries from the catalog.
- [ ] A funded upgrade purchase updates balance, level, cost, and effect preview in the scene.
- [ ] Continue creates the active run modifier snapshot and transitions to Pre-Launch.
- [ ] Slingshot capture is disabled before Continue and enabled after Continue.
- [ ] Launch transitions from Pre-Launch to Running and applies gameplay-calculated launch impulse.
- [ ] Running movement and pickup collection can consume active stat resolver values.
- [ ] Run end returns to Run Preparation for the next run.
- [ ] Lifetime scope validation fails clearly for missing catalog, missing launch config, missing state id, missing UI view, or missing required currency/stat assets.
- [ ] Placeholder content is acceptable, but all four upgrade definitions have icon references and valid authored values.

## Verification

- EditMode tests:
  - Lifetime scope or installer tests cover required service registrations and invalid serialized-reference failures where possible without scene lifecycle.
  - Scene composition validation helpers reject missing required upgrade/economy/state references.
- PlayMode tests:
  - Gameplay scene starts in Run Preparation.
  - UI renders four catalog upgrade entries.
  - Buy command updates visible balance and preview state.
  - Continue enters Pre-Launch and enables Slingshot capture.
  - Slingshot launch enters Running and applies gameplay launch impulse.
  - Pickup collection and movement stat paths are available during Running.
  - Run end returns to Run Preparation.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
  - Targeted EditMode and PlayMode tests through Unity AI Agent Connector after compile is clean.
- Manual Unity smoke check:
  - Play the scene manually through Run Preparation -> buy -> Continue -> launch -> collect/steer -> run end -> Run Preparation.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 05 - Add Run Preparation State And Continue Handoff
- 06 - Add Run Preparation UI Buy/Continue Path
- 07 - Move Slingshot Launch Impulse Calculation To Gameplay
- 08 - Apply Player Movement Upgrade Stats
- 09 - Apply Coin Pickup Multiplier To Pickup Grants
