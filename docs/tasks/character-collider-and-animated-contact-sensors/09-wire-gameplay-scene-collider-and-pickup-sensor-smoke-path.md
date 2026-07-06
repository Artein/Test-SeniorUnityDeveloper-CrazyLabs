## Parent

[Character Collider Split And Animated Contact Sensors PRD](../../prd/prd-character-collider-and-animated-contact-sensors.md)

## What to build

Wire the full Gameplay scene smoke path for the collider split and sensor-driven pickup collection. The scene should contain distinct **Launch Target Collider Root**, **Run Body Contact Collider Root**, **Animated Contact Sensor Physics Root**, pickup sensor entries, explicit pickups, and **Gameplay Pickups Scene Composition Installer** registration.

This slice is the end-to-end integration pass. It should prove that launch/band behavior, stable run-body movement and run-ending contacts, and animated body-part pickup collection can coexist in the same authored scene.

## Acceptance criteria

- [ ] Gameplay scene keeps **Launch Target Collider Root** as the slingshot launch target and band-contact surface.
- [ ] Gameplay scene uses **Run Body Contact Collider Root** with the dedicated `SphereCollider` for run support, finish, safety net, and default obstacle impact.
- [ ] Gameplay scene contains **Animated Contact Sensor Physics Root** with one kinematic Rigidbody and copied trigger sensors.
- [ ] Pickup-eligible sensors use `PlayerBodyPart` layer and explicit **Pickup Sensor Source** entries.
- [ ] Project layer matrix allows `PlayerBodyPart` to **Pickup Layer** interaction for pickup-bearing levels.
- [ ] Project layer matrix disables **Player Layer** to `PlayerBodyPart` self-interaction by default.
- [ ] First-pass `PlayerBodyPart` external interaction is **Pickup Layer** only.
- [ ] **Gameplay Pickups Scene Composition Installer** owns explicit pickup references and pickup-source wiring.
- [ ] No-pickup scene path still composes without fake pickups or fake sensor entries.
- [ ] Sensor-driven pickup collection works in the Gameplay scene without routing collection through **Run Body Contact Collider**.

## Verification

- EditMode tests:
  - Scene or composition validation catches missing run-body contact, missing pickup sensor source for pickup-bearing levels, invalid pickup references, and invalid layer matrix setup.
- PlayMode tests:
  - Gameplay scene composition smoke resolves collider split and pickup services.
  - Run-surface probing still detects support through the run-body sphere.
  - Sensor-driven pickup contact collects exactly once in the scene smoke path.
  - No-pickup composition path remains valid.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
- Manual Unity smoke check:
  - Launch and band contact still work.
  - Running support, finish, safety-net, and default obstacle-impact behavior still work.
  - An animated body-part sensor can collect a pickup while the run-body sphere remains stable.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 - Split Run Body Contact Collider From Launch Target
- 06 - Move Pickup Composition To Gameplay Pickups Scene Installer
- 08 - Prove Sensor-Driven Trigger Delivery In PlayMode
