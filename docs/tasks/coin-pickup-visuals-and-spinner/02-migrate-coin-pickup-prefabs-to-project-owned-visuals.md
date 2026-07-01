## Parent

[Coin Pickup Visuals And Spinner PRD](../../prd/prd-coin-pickup-visuals-and-spinner.md)

## What to build

Migrate Regular Coin Pickup and Big Coin Pickup prefabs to use a project-owned **Coin Pickup Visual**: proper coin mesh, URP-compatible material, visual-child Spinner, and visual-only Big Coin scale.

The completed slice should be demoable by opening the coin pickup prefabs and running the scene: coin visuals render with the project-owned material, spin at runtime, and keep collection/reward behavior unchanged.

## Acceptance criteria

- [x] Coin Pickup prefabs reference a project-owned coin mesh or project-owned imported mesh asset.
- [x] Coin Pickup prefabs reference a project-owned URP-compatible coin material.
- [x] Coin material is tuned against the existing coin currency icon palette.
- [x] Regular Coin Pickup keeps its existing Pickup Definition.
- [x] Big Coin Pickup keeps its existing higher-value Pickup Definition.
- [x] Spinner is added to the visual child or targets the visual child.
- [x] Spinner does not rotate the Pickup root.
- [x] Big Coin size is expressed through visual scale, not through currency identity.
- [x] Big Coin trigger collider remains intentionally authored and readable.
- [x] No runtime code assigns the coin mesh or coin material.
- [x] Existing authored scene pickup instances continue resolving through their prefab variants.

## Verification

- EditMode tests:
  - Existing pickup/prefab validation continues to pass.
  - Add direct prefab checks in this slice only if a focused provider already exposes the coin prefabs.
- PlayMode tests:
  - Existing pickup trigger integration tests continue to pass if collider/root authoring changed.
  - Existing gameplay scene pickup composition tests continue to pass if scene prefab instances are affected.
- Static checks:
  - Confirm prefab YAML no longer references unsupported imported coin material for runtime coin visuals.
  - Reformat changed C# files if any and inspect Rider problems.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Open Regular Coin Pickup and Big Coin Pickup prefabs and confirm mesh/material/spinner/visual scale setup.
  - Enter Play Mode and confirm coins are visible, gold, spinning, and collectible.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Generic Spinner Presentation Component
