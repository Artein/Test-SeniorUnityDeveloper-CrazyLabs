## Parent

[Coin Pickup Visuals And Spinner PRD](../../prd/prd-coin-pickup-visuals-and-spinner.md)

## What to build

Add or update lightweight validation so **Coin Pickup Visual** authoring remains technically valid without freezing designer-owned art choices.

The completed slice should fail fast when coin pickup prefabs lose required gameplay contracts or basic visual contracts, while avoiding assertions about exact mesh shape, exact spin angle, exact renderer count, or level placement.

## Acceptance criteria

- [x] Validation confirms coin pickup roots still have valid Pickup components.
- [x] Validation confirms coin pickup roots still have valid Pickup Definitions.
- [x] Validation confirms pickup trigger colliders remain enabled triggers on the pickup root contract.
- [x] Validation confirms visual renderers have non-null meshes and non-null materials.
- [x] Validation confirms coin visual materials are not missing-shader materials.
- [x] Validation confirms Spinner is on or targets the visual child, not the Pickup root.
- [x] Validation confirms Big Coin visual scale does not imply a separate currency concept.
- [x] Tests do not assert exact mesh topology, exact mesh asset name, exact spin speed, exact scene phase, or pixel-perfect appearance.
- [x] Existing Pickup Collection Controller, Level Pickup State, Run Reward Breakdown, and reward commit tests remain compatible.

## Verification

- EditMode tests:
  - Coin pickup prefab contracts are valid.
  - Spinner target boundary is valid for coin pickup prefabs.
  - Materials used by coin pickup visuals have valid shaders.
- PlayMode tests:
  - Existing pickup physics integration tests still pass if prefab collider authoring changed.
  - Existing gameplay scene composition or pickup scene tests still pass if scene-authored pickup instances are affected.
- Static checks:
  - Reformat changed C# files and inspect Rider problems.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Confirm validation failures are understandable by temporarily inspecting expected failure messages or test names.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 - Migrate Coin Pickup Prefabs To Project-Owned Visuals
