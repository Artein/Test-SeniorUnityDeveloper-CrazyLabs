## Parent

[Coin Pickup Visuals And Spinner PRD](../../prd/prd-coin-pickup-visuals-and-spinner.md)

## What to build

Add a generic **Spinner** presentation component that rotates a target transform with scaled game time and supports deterministic per-instance initial phase variation.

This slice should be complete without touching Coin Pickup prefabs. A developer should be able to add Spinner to any visual GameObject, configure axis/speed/phase behavior, and verify deterministic behavior through focused tests.

## Acceptance criteria

- [x] Spinner is generic presentation behavior and has no dependency on Pickups, Economy, Gameplay State, or VContainer.
- [x] Spinner can rotate an assigned target transform.
- [x] Spinner can rotate its own transform when no target is assigned.
- [x] Spinner uses scaled game time.
- [x] Spinner stops naturally when inactive.
- [x] Spinner supports deterministic per-instance initial phase variation.
- [x] Deterministic phase does not use runtime random values or unstable Unity instance IDs.
- [x] Spinner supports an authored phase value or additive authored phase offset.
- [x] Initial phase application does not accumulate incorrectly across disable/enable cycles.
- [x] The public serialized surface stays small enough for designer use.

## Verification

- EditMode tests:
  - Spinner rotates by the expected amount for a supplied scaled delta.
  - Same deterministic inputs produce the same initial phase.
  - Different deterministic inputs can produce different initial phases.
  - Authored phase changes the initial visual rotation.
  - Reinitialization does not keep adding phase on top of prior phase.
- PlayMode tests:
  - None expected unless Unity lifecycle behavior cannot be covered deterministically in EditMode.
- Static checks:
  - Reformat changed C# files and inspect Rider problems.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Add Spinner to a temporary visual object and confirm it rotates in Play Mode.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
