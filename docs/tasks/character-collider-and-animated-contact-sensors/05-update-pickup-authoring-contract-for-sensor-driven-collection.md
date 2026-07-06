## Parent

[Character Collider Split And Animated Contact Sensors PRD](../../prd/prd-character-collider-and-animated-contact-sensors.md)

## What to build

Update pickup authoring and validation for sensor-driven collection. A pickup-bearing level should use explicit **Pickup** references, and each explicit pickup should have exactly one enabled trigger Collider under its hierarchy on **Pickup Layer**. The collider may be any Unity Collider subtype.

The new contract should not require pickup-side Rigidbodies or pickup-side `TriggerNotifier` components. Existing legacy authoring may remain during migration, but it should not be required by the new sensor-driven collection path.

## Acceptance criteria

- [ ] Explicit pickup validation rejects missing, duplicate, inactive, or otherwise invalid **Pickup** references.
- [ ] Each explicit **Pickup** requires exactly one enabled trigger Collider under its hierarchy on **Pickup Layer**.
- [ ] The exactly-one rule counts only Colliders on **Pickup Layer**.
- [ ] Any Unity Collider subtype is accepted for the single pickup trigger volume.
- [ ] Pickup-side Rigidbody is not required for sensor-driven pickup collection.
- [ ] Pickup-side `TriggerNotifier` is not required for sensor-driven pickup collection.
- [ ] Legacy pickup-side trigger/notifier authoring can remain harmless during migration if it already exists.
- [ ] Validation messages tell designers which pickup reference or pickup hierarchy is invalid.

## Verification

- EditMode tests:
  - Validation rejects null, duplicate, inactive, missing-collider, non-trigger, disabled, wrong-layer, and multiple-pickup-layer-trigger authoring.
  - Validation accepts a single pickup-layer trigger Collider for at least two Collider subtypes.
  - Validation ignores non-**Pickup Layer** colliders for the exactly-one pickup trigger rule.
  - Sensor-driven collection contract does not require pickup-side Rigidbody or pickup-side `TriggerNotifier`.
- PlayMode tests:
  - Deferred to the trigger-delivery integration slice unless engine trigger behavior is changed here.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
- Manual Unity smoke check:
  - Inspect representative pickup prefab or scene authoring after migration and confirm the new contract is readable in the Inspector.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 03 - Route Pickup Collection Through Pickup Contact Source
- 04 - Add Pickup Sensor Source And Sensor Entry Validation
