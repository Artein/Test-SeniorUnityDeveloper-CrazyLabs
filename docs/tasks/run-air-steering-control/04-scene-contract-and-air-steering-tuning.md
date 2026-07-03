# Scene Contract And Air Steering Tuning

Type: AFK

## Parent

`docs/prd/prd-run-air-steering-control.md`

## What to build

Add the authored tuning and scene contract for **Run Air Steering Control**.

Player steering config should expose a separate air turn authority value. The value should be positive, defensively resolved when invalid, and lower than grounded turn authority by default. Existing grounded gesture mapping and **Run Steering Responsiveness** remain shared. Scene composition tests should protect the authored value so the Gameplay scene cannot silently regress.

This slice also protects ownership boundaries around tuning: air steering must not reintroduce speed caps or launch speed recovery, and **Launch Landing Stabilization** must remain a lift-only contact correction that preserves tangent speed.

## Acceptance criteria

- [ ] Player steering config exposes a separate air turn authority value.
- [ ] Default air turn authority is positive and lower than grounded turn authority.
- [ ] Invalid authored air turn authority resolves defensively.
- [ ] Existing grounded turn authority remains available for grounded steering.
- [ ] Existing gesture mapping, deadzone, range, DPI handling, and smoothing remain shared.
- [ ] Scene composition asserts the authored Gameplay steering config air turn authority.
- [ ] Tests and fakes that implement steering config are updated.
- [ ] No player-facing planar speed cap or launch speed recovery field is reintroduced.
- [ ] **Launch Landing Stabilization** config remains separate from air steering config.
- [ ] Documentation or glossary is updated only if implementation reveals a terminology gap.

## Verification

- EditMode tests:
  - Steering config default air turn authority is positive and lower than grounded turn authority.
  - Invalid authored air turn authority resolves defensively.
  - Grounded turn authority defaults remain unchanged unless explicitly retuned.
  - Test fakes compile with the new config contract.
- PlayMode tests:
  - Gameplay scene composition asserts the authored air turn authority value.
- Static checks:
  - Unity connector compile before tests in implementation.
  - Rider reformat/problems for changed code, tests, and assets in implementation.
- Manual Unity smoke check:
  - Optional quick run after scene authoring: grounded steering still feels stronger than air steering.
- Package version/changelog:
  - Not required.

## Blocked by

- Direction-Only Run Air Steering
