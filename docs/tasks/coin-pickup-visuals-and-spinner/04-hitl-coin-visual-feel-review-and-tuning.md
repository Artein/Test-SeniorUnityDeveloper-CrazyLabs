## Parent

[Coin Pickup Visuals And Spinner PRD](../../prd/prd-coin-pickup-visuals-and-spinner.md)

## What to build

Run a human-in-the-loop feel review for the migrated **Coin Pickup Visual** in Play Mode, then tune only presentation values: material color, brightness, spin axis, spin speed, initial phase feel, Regular Coin scale, and Big Coin scale.

This slice is complete when a human confirms that coins are readable during a normal Run, match the currency icon closely enough, spin in an appealing way, and make Big Coin value visually clear without changing reward rules.

Current state: complete. Human Play Mode approval confirmed readability, currency-icon color match, spin feel, phase variation, and Big Coin scale with no additional tuning requested.

## Acceptance criteria

- [x] Regular Coin Pickup reads as collectible currency during a normal Run.
- [x] Big Coin Pickup reads as the same currency with higher value.
- [x] Coin material color is visually consistent with the coin currency icon.
- [x] Coin rotation is noticeable but not distracting.
- [x] Deterministic phase variation prevents obvious identical alignment across nearby coins.
- [x] Big Coin scale is approved without relying on a different material or currency identity.
- [x] Pickup collection behavior remains unchanged during the review.
- [x] Any approved tuning changes are serialized into project-owned assets or prefabs.
- [x] Any rejected tuning options are not preserved as stale TODOs or unused assets.

## Verification

- EditMode tests:
  - Re-run affected prefab/Spinner validation tests after tuning.
- PlayMode tests:
  - Re-run affected pickup/gameplay scene tests if prefab or scene authoring changed.
- Static checks:
  - Unity compile through the existing connector after serialized asset changes.
- Manual Unity smoke check:
  - Human Play Mode approval confirmed Regular Coin and Big Coin readability from the run camera.
  - Human Play Mode approval confirmed the 3D coin material is close enough to the coin currency icon.
  - Human Play Mode approval confirmed spin feel, deterministic phase variation, and Big Coin scale.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 - Migrate Coin Pickup Prefabs To Project-Owned Visuals
