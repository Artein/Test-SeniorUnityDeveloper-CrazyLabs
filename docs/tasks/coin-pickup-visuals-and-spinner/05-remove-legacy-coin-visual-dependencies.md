## Parent

[Coin Pickup Visuals And Spinner PRD](../../prd/prd-coin-pickup-visuals-and-spinner.md)

## What to build

Close the migration by removing runtime coin prefab dependence on legacy Ladybug plugin coin visual assets and confirming no code path patches coin visuals at runtime.

The completed slice should leave external or downloaded meshes as source material only, while runtime Coin Pickup prefabs reference project-owned visual assets and tests/docs reflect the final boundary.

## Acceptance criteria

- [x] Runtime Regular Coin Pickup visual references are project-owned.
- [x] Runtime Big Coin Pickup visual references are project-owned.
- [x] Runtime coin pickup prefabs do not reference the old unsupported imported coin material.
- [x] Runtime coin pickup prefabs do not depend directly on Ladybug plugin coin mesh/material references unless explicitly retained as source-only mesh import input.
- [x] No runtime code assigns or patches Coin Pickup Visual mesh/material.
- [x] Pickup glossary remains aligned with the final **Coin Pickup Visual** boundary.
- [x] PRD/task docs remain aligned with the final implementation outcome.
- [x] Unity compile passes.
- [x] Targeted validation and pickup tests pass.

## Verification

- EditMode tests:
  - Coin pickup prefab contract validation passes.
  - Spinner tests pass.
- PlayMode tests:
  - Existing pickup physics integration tests pass if collider/root authoring changed.
  - Gameplay scene pickup/composition tests pass if scene instances were touched.
- Static checks:
  - Search runtime coin prefabs and code for legacy plugin coin material/mesh references.
  - Reformat changed C# files and inspect Rider problems.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Confirm the final approved coin visuals still render and spin in Play Mode.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 - Migrate Coin Pickup Prefabs To Project-Owned Visuals
- 04 - HITL Coin Visual Feel Review And Tuning
