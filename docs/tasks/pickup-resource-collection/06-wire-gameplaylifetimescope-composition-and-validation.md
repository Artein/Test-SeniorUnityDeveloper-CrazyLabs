## Parent

[Pickup Resource Collection PRD](../../prd/prd-pickup-resource-collection.md)

## What to build

Wire pickup/resource collection into gameplay composition. `GameplayLifetimeScope` should serialize explicit level pickup references, the configured
**Player Tag**, and explicit player pickup-contact colliders for setup validation. Composition should create the resource services, run accumulator,
level pickup state, and pickup collection controller through VContainer, and fail hard before the first **Run** when authoring is invalid.

This slice should make the runtime dependency graph complete without relying on runtime scene discovery.

## Acceptance criteria

- [ ] `GameplayLifetimeScope` serializes explicit pickup references used by pickup collection.
- [ ] `GameplayLifetimeScope` serializes the configured **Player Tag** and passes it into `PickupCollectionController`.
- [ ] `GameplayLifetimeScope` serializes explicit player pickup-contact collider references for validation only.
- [ ] Composition registers `ResourceStorage`, `RunResourceAccumulator`, `LevelPickupState`, and `PickupCollectionController` through VContainer.
- [ ] **Level Pickup State** is initialized for the current **Level Session** at composition time until a dedicated level lifecycle exists.
- [ ] Runtime setup validation fails loud for missing references, duplicate pickups, invalid definitions, non-positive amounts, wrong layers, missing tags, non-trigger pickup colliders, and missing player pickup-contact colliders.
- [ ] `OnValidate` may warn but does not auto-populate pickup references.
- [ ] Root gameplay references the pickup feature only where composition and run-result integration need it.

## Verification

- EditMode tests:
  - Lifetime-scope validation catches missing pickup list, duplicate pickups, invalid definitions, empty tag, and missing player pickup-contact collider references.
  - Composition resolves pickup/resource services and controller dependencies through VContainer.
  - Existing gameplay composition tests continue to pass.
- PlayMode tests:
  - Scene composition fails loudly when required pickup/player setup is invalid.
  - A valid composed scene can resolve the pickup collection path without runtime scene scans.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Break one serialized pickup/player reference at a time and confirm setup errors identify the bad authoring.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 - Attach Run Resource Snapshot To RunResult
- 04 - Add PickupCollectionController Transaction Flow
- 05 - Add Pickup Physics Setup And Trigger Integration
