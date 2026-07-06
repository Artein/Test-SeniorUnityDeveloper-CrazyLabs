## Parent

[Character Collider Split And Animated Contact Sensors PRD](../../prd/prd-character-collider-and-animated-contact-sensors.md)

## What to build

Migrate pickup collection away from pickup-side raw trigger events and **Player Tag** filtering by introducing a source-level **Pickup Contact** path. **Pickup Collection Controller** should consume typed pickup-contact reports and keep owning gameplay-state gating, rewards, pickup availability, collection events, and idempotency through **Level Pickup State**.

This slice should be implementable with a fake pickup contact source before real animated sensors exist. It establishes the runtime contract that later sensor sources plug into.

## Acceptance criteria

- [ ] A typed **Pickup Contact** fact exists for raw source-level pickup contact data.
- [ ] A pickup contact source abstraction publishes **Pickup Contact** reports through direct C# events or equivalent local callbacks.
- [ ] **Pickup Collection Controller** subscribes to the pickup contact source instead of raw pickup trigger events as its primary collection path.
- [ ] **Pickup Collection Controller** remains responsible for **Gameplay State** gating and rejects contacts when collection is not legal.
- [ ] Reward grant, pickup availability updates, and **Pickup Collection Event** semantics remain compatible with existing pickup behavior.
- [ ] Duplicate contacts for an already collected pickup do not grant duplicate rewards or emit duplicate accepted collection events.
- [ ] Legacy pickup-side trigger authoring is not required for the new primary collection path.
- [ ] The source contract stays state-agnostic: it reports contact facts and does not decide whether collection should be accepted.

## Verification

- EditMode tests:
  - A fake pickup contact source can drive **Pickup Collection Controller** collection.
  - Contacts are ignored outside legal gameplay states.
  - Duplicate contacts for the same pickup do not duplicate rewards or accepted collection events.
  - Collection output remains compatible with existing reward and **Level Pickup State** behavior.
- PlayMode tests:
  - Not required unless the migration touches engine physics in this slice.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Source search confirms the new primary collection path does not depend on **Player Tag** filtering.
- Manual Unity smoke check:
  - Not required if this slice remains controller/source-contract only.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Approve Collider Authority ADR And Migration Boundary
