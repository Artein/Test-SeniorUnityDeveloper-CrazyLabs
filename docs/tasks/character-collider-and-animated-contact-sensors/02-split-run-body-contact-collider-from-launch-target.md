## Parent

[Character Collider Split And Animated Contact Sensors PRD](../../prd/prd-character-collider-and-animated-contact-sensors.md)

## What to build

Split run movement and default run-ending contact authority away from **Launch Target Collider Root** by introducing a dedicated **Run Body Contact Collider Root** under the **Run Body** Rigidbody root. The first implementation should use one `SphereCollider` and wire existing run support, finish, safety-net, and default obstacle-impact paths to that run-body contact surface.

The slingshot and band path should keep using **Launch Target Collider Root**. This slice proves that launch/band contacts and run-body contacts can be tuned and reasoned about independently before animated pickup sensors are added.

## Acceptance criteria

- [ ] Player hierarchy has a distinct **Run Body Contact Collider Root** separate from **Launch Target Collider Root**.
- [ ] **Run Body Contact Collider Root** carries one enabled `SphereCollider` under the **Run Body** Rigidbody root.
- [ ] Run-surface support probing uses the **Run Body Contact Collider** and still works through the existing support probe factory.
- [ ] **Run Finish**, **Run Safety Net**, and default **Obstacle Impact** still use the stable run-body contact path.
- [ ] Slingshot launch targeting and band-contact logic continue to use **Launch Target Collider Root**.
- [ ] Pickup collection is not routed through the **Run Body Contact Collider**.
- [ ] No animation-driven resizing, switching, or compound movement shape is introduced in this slice.
- [ ] Scene or prefab validation clearly reports missing or miswired run-body contact collider references.

## Verification

- EditMode tests:
  - Run-support collider probing still supports a `SphereCollider` path.
  - Composition or validation tests reject missing, duplicate, or wrongly typed run-body contact references where practical.
- PlayMode tests:
  - Scene smoke verifies run-surface support still detects through the run-body sphere.
  - Scene smoke verifies finish, safety net, and default obstacle-impact contacts still reach the existing run-end path.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
- Manual Unity smoke check:
  - Launch, band contact, running support, finish, safety-net, and obstacle-impact behavior still work in the Gameplay scene.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Approve Collider Authority ADR And Migration Boundary
