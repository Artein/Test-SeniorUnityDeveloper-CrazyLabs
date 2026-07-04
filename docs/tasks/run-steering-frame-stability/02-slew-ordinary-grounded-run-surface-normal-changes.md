## Parent

[Run Steering Frame Stability PRD](../../prd/prd-run-steering-frame-stability.md)

## What to build

Make ordinary grounded **Run Surface** normal changes rotate the **Run Steering Frame** smoothly instead of instantly.

When the **Launch Target** remains grounded on valid **Run Surface** support and the support normal changes within the configured snap threshold, the stable **Run Steering Frame** should move toward the new normal at the configured angular slew rate. This gives smooth-looking **Side Banks** stable control without changing raw support facts for any other gameplay system.

This slice is complete when a player can ride normal bank curvature without one-frame steering-up jumps, while confirmed real support remains represented by the steering frame over time.

## Acceptance criteria

- [ ] Grounded valid support normals remain the source for the stable **Run Steering Frame** after activation.
- [ ] Small-to-moderate grounded normal changes are angular-slew-limited instead of applied instantly.
- [ ] Slew speed uses the configured degrees-per-second value and fixed delta time.
- [ ] Larger fixed delta time advances the stable frame farther than smaller fixed delta time under the same config.
- [ ] The stable frame never overshoots the target support normal.
- [ ] Invalid grounded normals do not become steering up.
- [ ] Reads remain pure inside a fixed tick; state advances only from the fixed-tick sampling path.
- [ ] **PlayerSteeringController** continues to rotate velocity around the stable **Run Steering Frame**, preserving existing velocity and speed-limit behavior.
- [ ] Raw **RunSurfaceContext** remains unchanged and no non-steering consumer receives smoothed normals.

## Verification

- EditMode tests:
  - Grounded valid support after reset becomes the steering frame.
  - A normal change below snap threshold slews over multiple fixed ticks.
  - Slew amount matches configured degrees per second and fixed delta time.
  - Slew clamps to the target normal without overshoot.
  - Invalid grounded normals fall back safely and do not corrupt stable state.
  - Multiple `GetUpDirection` calls in one fixed tick return the same value.
  - Existing controller test coverage still proves velocity rotates around the frame supplied by the steering-frame source.
- PlayMode tests:
  - Not expected for this slice unless EditMode coverage cannot express timing behavior.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Targeted EditMode tests for steering-frame source and steering-controller movement integration.
- Manual Unity smoke check:
  - Ride a gentle half-tube bank and confirm steering no longer visually reacts to tiny normal changes as immediate snaps.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Resettable Fixed-Tick Run Steering Frame Baseline
