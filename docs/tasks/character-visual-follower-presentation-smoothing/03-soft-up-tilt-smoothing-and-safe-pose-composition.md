## Parent

[Character Visual Follower Presentation Smoothing PRD](../../prd/prd-character-visual-follower-presentation-smoothing.md)

## What to build

Add the side-bank-specific part of the presentation smoothing: soft up/tilt following and safe pose composition.

The implementation should let position and heading stay responsive while making the visible character's up vector and body tilt less sensitive to tiny render-pose changes on U-shaped side banks. The smoother should build a valid orthonormal rotation from smoothed up and heading inputs, reject invalid vectors safely, and snap on large orientation discontinuities rather than easing through an obviously wrong pose.

This is the slice that should directly address the tiny visible character flicker on the raised sides while still leaving physics truth and camera behavior unchanged.

## Acceptance criteria

- [ ] Up/tilt smoothing uses the configured up/tilt response default of `18`.
- [ ] Heading remains tighter than up/tilt so the character keeps facing motion while bank tilt is visually damped.
- [ ] Orientation jumps at or beyond the configured snap angle default of `45` degrees snap immediately.
- [ ] The smoother composes output rotation from a valid orthonormal basis, not from accumulated skewed vectors.
- [ ] Invalid target forward/up vectors, near-zero vectors, and nearly parallel forward/up inputs fall back safely without producing NaN or identity-popping artifacts.
- [ ] Forced lifecycle snaps from earlier slices copy the full target pose exactly, including orientation.
- [ ] Tilt smoothing is presentation-only; **RunSurfaceContext**, **RunSurfaceSteeringFrameSource**, player steering, camera, progress, and run-end logic still use their existing raw/owned data.
- [ ] The visible character remains close enough to the **Launch Target** that the player does not perceive body detachment.
- [ ] No slope classifier, animation mode, physics material, Rigidbody, collider, camera target, or run-end threshold is changed.

## Verification

- EditMode tests:
  - Up vector converges smoothly across small bank-normal changes instead of snapping every sample.
  - Heading response remains faster/tighter than up/tilt response for the same pose sequence.
  - Snap angle forces immediate orientation snap for large discontinuities.
  - Orthonormal pose composition returns perpendicular, normalized basis vectors within tolerance.
  - Near-zero target forward/up inputs preserve the last valid visual basis or configured fallback.
  - Nearly parallel forward/up inputs are repaired without NaN, zero-length vectors, or sudden arbitrary flips.
  - Forced reset copies target orientation exactly and clears prior smoothed basis state.
- PlayMode tests:
  - Not expected unless a scene-level banked transform fixture is needed to prove composition.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Source search confirms tilt smoothing remains in Character Presentation and is not used by physics, steering, progress, run-end, or camera systems.
- Manual Unity smoke check:
  - Traverse the U-shaped side bank and verify visible tilt changes look softer without changing gameplay path or camera movement.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

[02 - Bounded Position And Heading Smoothing](02-bounded-position-and-heading-smoothing.md)
