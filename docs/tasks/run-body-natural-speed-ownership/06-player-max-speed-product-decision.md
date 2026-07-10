# Player Max Speed Product Decision

Status: Resolved

Type: HITL decision

## Parent

Historical parent: [Run Body Natural Speed Ownership](../../prd/prd-run-body-natural-speed-ownership.md)

Authoritative replacement: [Run Body Explicit Speed Ownership](../../prd/prd-run-body-explicit-speed-ownership.md)

Architecture decision: [ADR-0010](../../adr/adr-0010-use-explicit-run-body-speed-model-with-rigidbody-contact-physics.md)

## Decision

Keep the player-facing `PlayerMaxSpeed` stat and make it modify the soft **Run Body Speed Envelope** during **Running**. The existing gameplay stat resolver supplies the active-run value to **Run Body Movement Controller**, and **Run Body Speed Model** uses it as the speed around which above-envelope resistance begins.

A higher value lets the player sustain a higher useful grounded speed and can increase course reach. It is not a hard velocity clamp: launch, gravity, or collision overspeed remains visible and settles through authored resistance. It does not modify **Launch Impulse**, steering responsiveness, or physical contact response.

This gives the upgrade one observable product promise while keeping speed ownership out of **Run Steering Control**. See the [actor-aware model diagram](../../diagrams/run-body-speed-model.md) for the configuration and runtime path.

## Acceptance criteria

- [x] Retain `PlayerMaxSpeed` and define one gameplay owner: **Run Body Speed Model**.
- [x] Define the player-visible effect as a higher soft speed envelope and higher sustainable grounded speed.
- [x] Keep launch energy, steering responsiveness, contact response, and the defensive sanity guard separate.
- [ ] Inventory player-facing terminology, upgrade data, UI copy, icons, and stat bindings during implementation.
- [ ] Implement and test active-run stat resolution, soft-envelope behavior, and neutral-upgrade compatibility.
- [ ] Add any required UI, asset, or economy cleanup to the feature implementation scope after the inventory.

## Verification

- EditMode tests:
  - Not required for the decision itself.
- PlayMode tests:
  - Not required for the decision itself.
- Static checks:
  - Confirm all `PlayerMaxSpeed` movement references describe a soft envelope rather than steering-owned limiting.
- Manual Unity smoke check:
  - Review upgrade UI/copy if the upgrade is currently exposed.
- Package version/changelog:
  - Not required for the decision itself. Required only for any follow-up implementation that changes player-facing upgrade behavior.

## Blocked by

None - decision is complete; implementation follows the replacement PRD.
