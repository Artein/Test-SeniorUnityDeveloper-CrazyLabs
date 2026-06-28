## Parent

[Pickup Resource Collection PRD](../../prd/prd-pickup-resource-collection.md)

## What to build

Add editor-only authoring helpers for pickup setup. Designers should be able to pick valid Unity tags from a selector for serialized tag-name fields
and use a `Refresh Pickup References From Scene` button on `GameplayLifetimeScope` to populate explicit pickup references from the current scene.

This slice should improve authoring ergonomics without changing runtime ownership or adding runtime discovery.

## Acceptance criteria

- [ ] Serialized tag-name fields can use a tag selector attribute/property drawer instead of typo-prone free text.
- [ ] `GameplayLifetimeScope` custom inspector includes exactly one button named `Refresh Pickup References From Scene`.
- [ ] Refresh finds same-scene **Pickup** objects.
- [ ] Refresh includes inactive pickups.
- [ ] Refresh orders pickup references deterministically for stable serialized diffs.
- [ ] Refresh records Undo and marks the scope dirty.
- [ ] Refresh does not populate player pickup-contact collider references.
- [ ] `OnValidate` does not auto-populate pickup references.

## Verification

- EditMode tests:
  - None expected unless editor helpers expose pure deterministic sorting logic.
- PlayMode tests:
  - None expected.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Use the custom inspector button in a scene with active and inactive pickups.
  - Confirm deterministic order, Undo behavior, and dirty marking.
  - Confirm tag selector lists project tags and writes the selected tag string.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 06 - Wire GameplayLifetimeScope Composition And Validation
