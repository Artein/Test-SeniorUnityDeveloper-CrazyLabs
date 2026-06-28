## Parent

[Pickup Resource Collection PRD](../../prd/prd-pickup-resource-collection.md)

## What to build

Add `PickupCollectionController` as the plain C# owner of pickup contact subscriptions and accepted collection transactions. The controller should
listen to explicit pickups, accept contacts only while gameplay is `Running`, require the configured **Player Tag** on the contacted collider, consume
**Level Pickup State** before granting, update both `ResourceStorage` and `RunResourceAccumulator`, disable the pickup root, and publish a generic
**Pickup Collection Event** after the grant.

This slice should prove transaction ordering and duplicate-contact safety without depending on real physics where possible.

## Acceptance criteria

- [x] Controller subscribes to all explicit pickup contact events and unsubscribes when disposed.
- [x] Contacts outside `Running` do not consume pickups, grant resources, disable pickups, or publish collection events.
- [x] Contacts from colliders without the configured **Player Tag** are ignored.
- [x] Accepted collection consumes `LevelPickupState` before mutating resource totals.
- [x] Duplicate accepted contacts for the same pickup cannot double grant.
- [x] Accepted collection grants the same resource amount to `ResourceStorage` and `RunResourceAccumulator`.
- [x] Accepted collection disables the pickup root after the grant.
- [x] **Pickup Collection Event** is published after grant application and includes pickup, resource, amount, and world position.
- [x] No `PickupCollector`, parent hierarchy lookup, `PickupContact` struct, event bus, or coin-specific controller is introduced.

## Verification

- EditMode tests:
  - Controller ignores contacts outside `Running`.
  - Controller ignores contacts without configured **Player Tag**.
  - Controller consumes pickup state before resource grants.
  - Controller grants both app-session storage and run accumulator.
  - Controller publishes one **Pickup Collection Event** after the grant.
  - Controller does not double grant duplicate contacts.
  - Controller unsubscribes from pickup contact events when disposed.
- PlayMode tests:
  - None expected for transaction rules beyond what later physics integration covers.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Not required for this slice; later scene smoke covers live gameplay.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Generic Resource Grant Data Path
- 03 - Add Pickup Adapter And Level Pickup State
