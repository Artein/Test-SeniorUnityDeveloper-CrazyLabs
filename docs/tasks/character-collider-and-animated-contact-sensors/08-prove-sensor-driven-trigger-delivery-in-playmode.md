## Parent

[Character Collider Split And Animated Contact Sensors PRD](../../prd/prd-character-collider-and-animated-contact-sensors.md)

## What to build

Prove the sensor-driven pickup path with real Unity trigger delivery. A copied animated sensor under **Animated Contact Sensor Physics Root** should use a trigger Collider, `TriggerNotifier`, and the root kinematic Rigidbody to collect a pickup trigger Collider without requiring a Rigidbody or `TriggerNotifier` on the pickup GameObject.

The test should accept next-physics-step delivery and avoid same-frame physics queries or per-frame `Physics.SyncTransforms`.

## Acceptance criteria

- [ ] A PlayMode test proves a copied `PlayerBodyPart` trigger sensor touching a **Pickup Layer** trigger Collider emits a **Pickup Contact** or accepted pickup collection through the new source path.
- [ ] The pickup GameObject does not require a Rigidbody for the trigger-delivery proof.
- [ ] The pickup GameObject does not require pickup-side `TriggerNotifier` for the trigger-delivery proof.
- [ ] Non-pickup trigger contacts are ignored without runtime warnings.
- [ ] Duplicate sensor contacts for the same pickup do not create duplicate accepted collection events.
- [ ] The test uses next-physics-step delivery with condition-plus-timeout helpers, not fixed time sleeps.
- [ ] The test does not depend on `Physics.SyncTransforms` for same-frame collection.

## Verification

- EditMode tests:
  - Existing source, validation, and controller tests remain green.
- PlayMode tests:
  - Sensor-to-pickup trigger delivery emits the expected pickup contact or accepted collection.
  - Non-pickup trigger contact is ignored.
  - Multiple sensor contacts for the same pickup do not duplicate accepted collection.
  - Pickup-side Rigidbody and pickup-side `TriggerNotifier` are absent in the test setup.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
- Manual Unity smoke check:
  - Not required if deterministic PlayMode coverage proves the physics trigger path.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 04 - Add Pickup Sensor Source And Sensor Entry Validation
- 05 - Update Pickup Authoring Contract For Sensor-Driven Collection
- 07 - Add Animated Contact Sensor Pose Sync
