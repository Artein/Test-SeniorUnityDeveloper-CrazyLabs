## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Apply upgrade-resolved player movement stats during the Running state. **Run Body Movement Controller** should resolve `PlayerMaxSpeed` for the soft speed envelope, steering should resolve `PlayerSteeringResponsiveness`, and neither path should read upgrade levels or upgrade definitions directly.

This slice should make movement upgrades affect gameplay through the same stat-resolution path used by the rest of the upgrade system.

## Acceptance criteria

- [ ] Player movement resolves `PlayerMaxSpeed` from the base soft-envelope value through `GameplayStatResolver`.
- [ ] Player movement resolves `PlayerSteeringResponsiveness` from a base movement value through `GameplayStatResolver`.
- [ ] Movement code does not read upgrade progress, upgrade definitions, upgrade catalog, or currency storage directly.
- [ ] Resolved movement values are used only during Running movement, not during Slingshot pull or launch impulse calculation.
- [ ] Neutral upgrade levels preserve existing movement behavior within expected tolerance.
- [ ] Non-neutral upgrade levels change the soft speed envelope and steering responsiveness in the expected direction.
- [ ] Runtime modifier snapshot semantics are respected: the active run uses the snapshot created on Continue.

## Verification

- EditMode tests:
  - Movement controller/service uses base values unchanged when resolver has no relevant modifiers.
  - Fake `PlayerMaxSpeed` modifier changes the soft speed envelope in the expected direction without introducing a hard clamp.
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
  - Compare neutral and boosted movement upgrade settings in a short run; confirm higher sustainable grounded speed without changing Slingshot pull or immediately clamping external overspeed.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 04 - Add Run Modifier Snapshot And Gameplay Stat Resolver
- 05 - Add Run Preparation State And Continue Handoff
