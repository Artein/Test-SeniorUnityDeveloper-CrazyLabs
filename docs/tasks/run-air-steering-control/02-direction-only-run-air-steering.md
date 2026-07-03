# Direction-Only Run Air Steering

Type: AFK

## Parent

`docs/prd/prd-run-air-steering-control.md`

## What to build

Implement **Run Air Steering Control** as a weaker direction-only steering behavior when **Run Steering Mode Selector** selects air and the player has an active **Run Steering Control** gesture.

Air steering rotates current air-frame planar velocity around the current **Run Steering Frame** up direction. It preserves planar speed, preserves vertical velocity, and rotates the **Run Body** to face the steered air-frame planar velocity when steering is actually applied. It must not add speed, reduce speed, apply player-facing speed caps, apply launch speed recovery, or drive toward a target velocity.

When air mode is selected but the player has no active steering gesture, the controller must not apply hidden guidance. The **Run Body** should keep current velocity except for separate velocity-only systems such as **Launch Landing Stabilization** or **Run Body Speed Sanity Guard**.

## Acceptance criteria

- [ ] Active air steering rotates velocity using a separately tuned air turn authority.
- [ ] Air turn authority is weaker than grounded turn authority.
- [ ] Air steering uses existing gesture mapping, deadzone, range, DPI handling, and smoothing.
- [ ] Air steering uses the current **Run Steering Frame** up direction.
- [ ] Air steering preserves planar speed.
- [ ] Air steering preserves vertical velocity.
- [ ] Air steering rotates **Run Body** facing when steering is applied.
- [ ] Air steering does not add speed, reduce speed, clamp planar speed, recover launch speed, or drive toward a target velocity.
- [ ] Air mode with no active gesture does not apply a steering velocity write or hidden facing correction.
- [ ] Grounded steering keeps existing grounded behavior and grounded turn authority.
- [ ] **Minimum Steer Speed** remains a steering-rotation gate, not a speed correction.
- [ ] **Launch Landing Stabilization** remains velocity-only and continues to run before steering writes where needed.

## Verification

- EditMode tests:
  - Post-launch unsupported flight with active steering rotates velocity using air turn authority.
  - Post-launch unsupported flight with no active gesture does not write steering velocity.
  - Air steering preserves planar speed and vertical velocity.
  - Air steering uses **Run Steering Frame** up.
  - Air steering rotates **Run Body** facing when steering is applied.
  - Air steering uses weaker turn authority than grounded steering.
  - Grounded steering still uses grounded turn authority.
  - Below **Minimum Steer Speed** skips steering rotation without speed correction.
  - Existing landing stabilization tests stay green.
- PlayMode tests:
  - None required unless EditMode fakes cannot cover the target write behavior.
- Static checks:
  - Unity connector compile before tests in implementation.
  - Rider reformat/problems for changed code and tests in implementation.
- Manual Unity smoke check:
  - Optional quick launch: hold steering during unsupported launch and confirm direction bends without visible speed loss.
- Package version/changelog:
  - Not required.

## Blocked by

- Run Steering Mode Selector Boundary
