## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Apply upgrade-resolved player movement stats during the Running state. Player movement should use caller-owned base values from movement configuration, resolve `PlayerMaxSpeed` and `PlayerSteeringResponsiveness` through the central gameplay stat resolver, and avoid reading upgrade levels or upgrade definitions directly.

This slice should make movement upgrades affect gameplay through the same stat-resolution path used by the rest of the upgrade system.

## Acceptance criteria

- [ ] Player movement resolves `PlayerMaxSpeed` from a base movement value through `GameplayStatResolver`.
- [ ] Player movement resolves `PlayerSteeringResponsiveness` from a base movement value through `GameplayStatResolver`.
- [ ] Movement code does not read upgrade progress, upgrade definitions, upgrade catalog, or currency storage directly.
- [ ] Resolved movement values are used only during Running movement, not during Slingshot pull or launch impulse calculation.
- [ ] Neutral upgrade levels preserve existing movement behavior within expected tolerance.
- [ ] Non-neutral upgrade levels change max speed and steering responsiveness in the expected direction.
- [ ] Runtime modifier snapshot semantics are respected: the active run uses the snapshot created on Continue.

## Verification

- EditMode tests:
  - Movement controller/service uses base values unchanged when resolver has no relevant modifiers.
  - Fake `PlayerMaxSpeed` modifier changes speed limiting in the expected direction.
  - Fake `PlayerSteeringResponsiveness` modifier changes steering response in the expected direction.
  - Movement code consumes a stat resolver abstraction instead of upgrade storage or catalog types.
- PlayMode tests:
  - Running state movement uses resolved movement stats in a scene or runtime harness where Unity lifecycle is required.
  - Changing owned movement upgrade level before Continue affects the next run after Continue.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
  - Source search confirms movement code does not depend directly on upgrade progress/catalog or currency storage.
- Manual Unity smoke check:
  - Compare neutral and boosted movement upgrade settings in a short run; confirm the effect is visible and not applied during Slingshot pull.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 04 - Add Run Modifier Snapshot And Gameplay Stat Resolver
- 05 - Add Run Preparation State And Continue Handoff
