## Parent

[Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

## What to build

Add the motion and terrain facts needed for Character presentation without choosing any animation mode yet. Runtime code should expose a signed course-forward speed beside existing course-planar speed and expose a continuous Run Surface Context containing grounded state, ground normal, and forward downhill degrees.

This slice creates the presentation-facing facts that later slices use to choose Slide, Run, and Airborne. It should not depend on Ladybug assets or Animator setup.

## Acceptance criteria

- [ ] A signed course-forward speed helper exists beside the existing course-planar speed helper.
- [ ] Course-forward speed is positive for forward movement, negative for backward movement, and near zero for lateral-only movement.
- [ ] Run Surface Context exposes grounded state, ground normal, and forward downhill degrees.
- [ ] Run Surface Context does not expose raw collider, raycast hit, tag, contact category, or implementation-specific probe details.
- [ ] Forward downhill degrees are calculated by a pure slope calculator from ground normal plus course forward/up directions.
- [ ] A Unity-facing surface context source continuously provides the latest Run Surface Context for the current Launch Target/run surface.
- [ ] The existing contact-enter run-ending classifier is not reused as the per-frame surface/slope source.
- [ ] No Character Presentation Mode, Animator, Ladybug prefab, or run-result mapping is introduced in this slice.

## Verification

- EditMode tests:
  - Course-forward speed covers forward, backward, lateral-only, zero-velocity, and non-unit direction cases if supported by the contract.
  - Slope calculator covers flat, downhill, uphill, banked, normalized, and non-normalized inputs.
  - Run Surface Context payload tests verify it stays semantic and does not leak raw contact/probe details.
- PlayMode tests:
  - Surface context source reports grounded context over a simple surface and ungrounded context when the probe has no valid ground.
  - The probe does not interfere with existing Rigidbody movement or contact notifications.
- Static checks:
  - `git diff --check`.
  - Unity compile through the connector before tests.
- Manual Unity smoke check:
  - In Gameplay Scene, inspect/debug the surface context while the Launch Target is grounded, ungrounded, downhill, and flat if simple test geometry is available.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
