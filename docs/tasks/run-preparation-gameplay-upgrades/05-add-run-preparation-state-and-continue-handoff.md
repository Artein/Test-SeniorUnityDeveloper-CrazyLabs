## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Add **Run Preparation** as the default gameplay state and create the `Continue` handoff into the existing pre-launch Slingshot flow. The completed slice should start the gameplay scene in Run Preparation, keep Slingshot capture disabled there, create the active run modifier snapshot when `Continue` is accepted, transition into Pre-Launch, and return to Run Preparation after Run Ended.

This slice should expose a command boundary that UI can call later, but it does not need the final visual Run Preparation UI.

## Acceptance criteria

- [ ] A Run Preparation gameplay state id asset exists and is included in the gameplay state config.
- [ ] Gameplay state config uses Run Preparation as the initial state.
- [ ] Valid transitions include Run Preparation -> Pre-Launch, Pre-Launch -> Running, Running -> Run Ended, and Run Ended -> Run Preparation.
- [ ] Slingshot capture is disabled while the current state is Run Preparation.
- [ ] Accepting `Continue` in Run Preparation creates or replaces the active run modifier snapshot.
- [ ] Accepting `Continue` transitions to Pre-Launch without starting the run.
- [ ] Slingshot launch still starts the run only from Pre-Launch.
- [ ] Run end/restart flow returns to Run Preparation for the next run.
- [ ] Invalid snapshot creation fails clearly and does not advance to Pre-Launch.

## Verification

- EditMode tests:
  - Gameplay flow accepts Continue only from Run Preparation.
  - Continue creates the active run modifier snapshot before entering Pre-Launch.
  - Continue failure leaves the state in Run Preparation.
  - Slingshot launch is ignored or rejected outside Pre-Launch according to the existing flow contract.
  - Run end restart target is Run Preparation.
- PlayMode tests:
  - Gameplay scene starts in Run Preparation.
  - Slingshot capture is disabled in Run Preparation.
  - Continue transitions to Pre-Launch and enables Slingshot capture.
  - Slingshot launch transitions from Pre-Launch to Running.
  - Run Ended returns to Run Preparation.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Enter the gameplay scene and confirm Run Preparation is visible/active before Slingshot capture is allowed.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 04 - Add Run Modifier Snapshot And Gameplay Stat Resolver
