## Parent

[Pickup Resource Collection PRD](../../prd/prd-pickup-resource-collection.md)

## What to build

Author the first pickup content: a coin **Resource Definition**, regular small coin and big coin **Pickup Definition** assets with approved base
amounts, a neutral pickup base prefab, and regular/big coin prefab variants. The variants should assign their definitions, trigger setup, layer setup,
and visuals while keeping legacy meshes/materials in the existing content tree.

This slice is HITL because the initial coin amounts, visual choice, and prefab presentation need design approval.

## Acceptance criteria

- [x] Coin **Resource Definition** asset exists and is named/content-scoped as generic resource identity data.
- [x] Regular small coin **Pickup Definition** references the coin resource and uses an approved positive amount.
- [x] Big coin **Pickup Definition** references the coin resource and uses an approved positive amount larger than the regular coin amount.
- [x] Neutral pickup base prefab owns shared pickup setup without duplicating common shell work across coin variants.
- [x] Regular small coin prefab variant assigns the regular coin **Pickup Definition**.
- [x] Big coin prefab variant assigns the big coin **Pickup Definition**.
- [x] Pickup variants use trigger colliders on **Pickup Layer**.
- [x] Pickup variants disable their whole root when collected.
- [x] Legacy visual dependencies stay in the existing content tree and are referenced, not moved.
- [x] Runtime code remains generic; coin specificity is limited to authored assets and prefab names.

## Verification

- EditMode tests:
  - Existing definition/pickup validation tests accept the authored coin definitions and prefabs if prefab validation utilities exist.
- PlayMode tests:
  - Existing trigger integration tests can use the authored coin prefabs where deterministic and cheap.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Verified:
  - Regular coin amount: 1; big coin amount: 5.
  - `RegularCoinPickup.prefab` and `BigCoinPickup.prefab` are variants of `PickupBase.prefab`.
  - Coin variants reference legacy `Carapace_TokenMesh.fbx` and `CoinPickUp.mat` assets in place.
  - Unity compile passed through Unity AI Agent Connector.
  - Targeted pickup/gameplay selectors passed: 63 passed / 0 failed.
  - Full project test run passed: 227 passed / 0 failed.
  - `git diff --check` passed.
- Manual Unity smoke check:
  - Designer reviews regular/big coin amounts and visuals.
  - Open each prefab variant and confirm assigned definition, collider trigger, layer, and root disable behavior.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Generic Resource Grant Data Path
- 03 - Add Pickup Adapter And Level Pickup State
- 05 - Add Pickup Physics Setup And Trigger Integration
