## Parent

[Run Steering Frame Stability PRD](../../prd/prd-run-steering-frame-stability.md)

## What to build

Establish the stable **Run Steering Frame** baseline as an end-to-end gameplay seam without changing smoothing behavior yet.

The implementation should make the **Run Steering Frame** source resettable, fixed-tick sampled, and composition-safe. Before the first run reset, reads should return the caller fallback and support samples should not leak into steering. When **Run Steering Control** actually activates for **Running**, the steering controller should reset the frame from the accepted launch up direction. The same steering-frame implementation instance must be used for read, reset, and fixed-tick roles.

This slice also adds the **Run Steering Frame Stability** tuning section to the existing steering config with the agreed defaults, so later behavior slices can consume one stable configuration contract.

## Acceptance criteria

- [ ] Existing **Run Steering Frame** reads still return a valid up direction for active grounded support.
- [ ] Before first reset, **Run Steering Frame** reads return the caller fallback and do not consume raw support as active steering state.
- [ ] A minimal reset seam primes the frame from the launch up direction and clears any prior state.
- [ ] Invalid reset input falls back safely and cannot create an invalid steering up direction.
- [ ] **PlayerSteeringController** resets the steering frame when steering activates, not merely when launch is reported.
- [ ] Launch reported before **Running** stores launch orientation but does not start steering-frame sampling until steering activation.
- [ ] Running entered before launch does not activate steering-frame reset without launch data.
- [ ] The steering-frame source samples once per fixed tick before steering movement consumes it.
- [ ] Composition uses one shared singleton instance for read, reset, and fixed-tick roles.
- [ ] The player steering config exposes normal slew, snap angle, ungrounded grace, and suspect-normal confirmation defaults through read-only config properties.
- [ ] Defaults are authored as `180` degrees/second, `60` degrees, `0.08` seconds, and `0.04` seconds.
- [ ] Raw **RunSurfaceContext**, **Run Progress**, **Character Presentation**, **Run End Flow**, camera, slingshot, and physics material behavior remain unchanged.

## Verification

- EditMode tests:
  - Pre-reset `GetUpDirection` returns caller fallback.
  - Reset with valid launch up primes the frame from launch up.
  - Reset with invalid launch up falls back safely.
  - Reset clears previous steering-frame state.
  - Controller resets the steering frame on steering activation.
  - Launch-before-Running and Running-before-launch ordering do not activate reset prematurely.
  - Composition resolves one shared steering-frame instance for read, reset, and fixed-tick roles.
  - Fixed-tick ordering samples the steering frame before steering applies movement.
  - Config tests cover the four new default values and defensive value resolution.
- PlayMode tests:
  - Not expected unless composition behavior cannot be covered in EditMode.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Source search confirms raw **RunSurfaceContext** consumers outside steering were not changed.
- Manual Unity smoke check:
  - Start a run and confirm baseline steering still works with no visible input or launch regression.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
