## Parent

[Character Collider Split And Animated Contact Sensors PRD](../../prd/prd-character-collider-and-animated-contact-sensors.md)

## What to build

Move pickup scene references and pickup contact-source wiring into a dedicated **Gameplay Pickups Scene Composition Installer**, mirroring the existing gameplay physics scene-composition pattern. The broad lifetime scope should consume installer registrations instead of owning pickup arrays or player pickup-contact collider arrays directly.

This slice should make no-pickup levels a valid empty no-op while still enforcing pickup and sensor authoring when a level declares explicit pickups.

## Acceptance criteria

- [ ] **Gameplay Pickups Scene Composition Installer** owns explicit **Pickup** references for the scene.
- [ ] The installer registers or exposes the level pickup source and pickup contact source needed by gameplay composition.
- [ ] Empty pickup lists are valid and compose to a no-op source path.
- [ ] Shared player sensor authoring can remain present in a no-pickup level without validation noise.
- [ ] Pickup-bearing levels require at least one valid pickup sensor entry.
- [ ] Pickup-bearing levels validate `PlayerBodyPart` to **Pickup Layer** interaction in the Unity Layer Collision Matrix.
- [ ] No-pickup levels do not require fake pickups, fake pickup sensors, or fake pickup references.
- [ ] The lifetime scope no longer owns serialized pickup arrays or legacy player pickup-contact collider arrays for this path.
- [ ] Runtime composition uses explicit references, not broad runtime discovery.

## Verification

- EditMode tests:
  - Installer validation accepts empty no-pickup composition.
  - Installer validation rejects pickup-bearing composition without valid sensor source entries.
  - Installer validation rejects duplicate, null, inactive, or invalid explicit pickup references.
  - Layer-matrix validation for `PlayerBodyPart` to **Pickup Layer** is required only when explicit pickups exist.
- PlayMode tests:
  - Composition smoke resolves pickup services in a no-pickup scene path.
  - Composition smoke resolves pickup services in a pickup-bearing scene path.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Source search confirms pickup composition no longer depends on broad runtime pickup discovery.
- Manual Unity smoke check:
  - Inspect the Gameplay scene and representative no-pickup/pickup-bearing authoring paths after migration.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 04 - Add Pickup Sensor Source And Sensor Entry Validation
- 05 - Update Pickup Authoring Contract For Sensor-Driven Collection
