## Parent

[Character Collider Split And Animated Contact Sensors PRD](../../prd/prd-character-collider-and-animated-contact-sensors.md)

## What to build

Add **Pickup Sensor Source** as the aggregate source for pickup-eligible **Animated Contact Sensors**. It should own explicit serialized sensor entries, subscribe to each entry's `TriggerNotifier`, resolve contacted colliders to a parent **Pickup**, and emit typed **Pickup Contact** facts for **Pickup Collection Controller**.

The source should keep `TriggerNotifier` dumb and local. Raw trigger callbacks stay inside the source, while gameplay receives source-level pickup-contact reports.

## Acceptance criteria

- [ ] **Pickup Sensor Source** is configured with explicit sensor entries instead of scanning all child notifiers at runtime.
- [ ] Each sensor entry references a `TriggerNotifier` on the copied animated sensor object.
- [ ] Each sensor entry requires a same-GameObject enabled trigger Collider on `PlayerBodyPart` layer.
- [ ] Sensor entries reject null, duplicate, inactive, disabled, missing-trigger, non-trigger, or wrong-layer authoring.
- [ ] Sensor identity is derived from sensor GameObject name or hierarchy path for the first pass.
- [ ] The implementation includes a TODO or equivalent marker for asset-backed **Animated Sensor Identity** before sensor identity becomes stable content contract.
- [ ] Triggered colliders resolve to a parent **Pickup** in the first implementation.
- [ ] Non-pickup trigger contacts are ignored without runtime warnings by default.
- [ ] Multiple sensors may report the same **Pickup**; idempotency remains owned by **Pickup Collection Controller** and **Level Pickup State**.
- [ ] **Pickup Sensor Source** does not require sensor GameObjects to carry **Player Tag** and does not require pickup-side `TriggerNotifier` events.

## Verification

- EditMode tests:
  - Validation rejects null, duplicate, inactive, disabled, missing-trigger, non-trigger, and wrong-layer sensor entries.
  - Source subscribes and unsubscribes from raw `TriggerNotifier` events without leaking callbacks.
  - Source emits **Pickup Contact** with pickup, sensor identity, source/sensor reference, and contacted Collider.
  - Source resolves pickups through parent lookup and ignores non-pickup contacts without warning.
  - Multiple sensor contacts for the same pickup can be reported without source-level deduplication.
- PlayMode tests:
  - Deferred to the trigger-delivery integration slice unless real Unity trigger behavior is required here.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Source search confirms `TriggerNotifier` stays gameplay-meaning-free.
- Manual Unity smoke check:
  - Not required if this slice remains source and validation code only.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 03 - Route Pickup Collection Through Pickup Contact Source
