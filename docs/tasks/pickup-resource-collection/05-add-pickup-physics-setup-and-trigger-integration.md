## Parent

[Pickup Resource Collection PRD](../../prd/prd-pickup-resource-collection.md)

## What to build

Add the Unity physics/tag integration required for pickup collection. Pickup trigger colliders should be on **Pickup Layer**, player pickup-contact
colliders should be on **Player Layer** and carry the configured **Player Tag** on their own GameObjects, and the 3D Layer Collision Matrix should
allow player/pickup trigger overlap while avoiding unnecessary non-player pickup overlaps.

This slice should prove that real Unity trigger-enter physics reaches the generic collection path with bounded filtering cost.

## Acceptance criteria

- [ ] Project has **Player Layer** and **Pickup Layer**, reusing equivalent existing layers only if they already exist.
- [ ] 3D Layer Collision Matrix allows **Player Layer** to overlap **Pickup Layer**.
- [ ] Pickup trigger colliders use **Pickup Layer** and are authored as triggers.
- [ ] Player pickup-contact colliders use **Player Layer** and have the configured **Player Tag** on the collider GameObject itself.
- [ ] Root-only player tagging is rejected for pickup collection.
- [ ] Real trigger-enter contact from a correctly tagged/layered player collider collects one pickup.
- [ ] Non-player contacts and incorrectly tagged player colliders do not collect pickups.
- [ ] No `OnTriggerStay`, per-frame pickup scans, or hierarchy lookup are added.

## Verification

- EditMode tests:
  - Validation helpers reject pickup colliders on the wrong layer or not marked as triggers.
  - Validation helpers reject player pickup-contact colliders on the wrong layer or without the configured tag.
- PlayMode tests:
  - Trigger setup with **Pickup Layer** and **Player Layer** collects as expected.
  - Contacting collider GameObject must have the configured **Player Tag**.
  - Root-only player tagging does not collect.
  - Consumed pickup root deactivation stops further trigger participation.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Inspect ProjectSettings layer/tag/matrix changes and verify unrelated gameplay collisions still behave normally.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 03 - Add Pickup Adapter And Level Pickup State
- 04 - Add PickupCollectionController Transaction Flow
