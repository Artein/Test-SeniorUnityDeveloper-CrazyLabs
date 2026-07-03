# Direction-Only Run Steering

Type: AFK

## Parent

`docs/prd/prd-run-body-natural-speed-ownership.md`

## What to build

Change **Run Steering Control** so it steers the **Run Body** direction without applying player-facing speed caps or launch speed recovery.

The controller should keep existing input mapping, steering responsiveness, maximum turn rate, **Minimum Steer Speed** as a rotation gate, **Run Steering Frame** behavior, and velocity/facing application. It should remove the behavior that clamps ordinary planar velocity to a maximum movement speed and remove launch speed bypass/recovery state that only existed to work around that cap.

After this slice, the **Run Body** can carry high launch-derived speed through **Launch Flight**, landing, **Airborne** presentation, and normal **Running** without hitting an invisible steering-owned wall. Steering may rotate the steerable velocity, but it must not add speed or reduce speed.

## Acceptance criteria

- [ ] Non-launch over-speed is not clamped by **Run Steering Control**.
- [ ] Unsupported post-launch over-speed is not clamped by **Run Steering Control**.
- [ ] Landing after launch does not start launch burst recovery or any automatic speed decay.
- [ ] Steering rotates steerable velocity without reducing its magnitude.
- [ ] Steering still uses the current **Run Steering Frame** up direction.
- [ ] **Minimum Steer Speed** still skips steering rotation when nearly stopped.
- [ ] Below **Minimum Steer Speed**, the controller does not write velocity unless a separate correction such as landing stabilization actually changed velocity.
- [ ] Existing steering input mapping, smoothing, turn rate, and lifecycle resets are preserved.
- [ ] Player-facing maximum planar speed, launch burst grace, launch burst recovery, and launch burst multiplier are removed or retired from active steering behavior.
- [ ] **Launch Flight** and **Airborne** presentation changes do not affect **Run Body** velocity.

## Verification

- EditMode tests:
  - Non-launch over-speed preserves speed while steering can rotate direction.
  - Post-launch unsupported over-speed preserves speed.
  - Post-landing speed is not recovered or decayed by steering.
  - Steering preserves magnitude while rotating around the steering frame up direction.
  - Low-speed input still skips steering rotation.
  - Existing steering gesture/responsiveness tests remain green.
- PlayMode tests:
  - None required unless scene-authored obsolete speed-cap fields are removed in this slice; if so, update focused scene composition assertions.
- Static checks:
  - Unity connector compile before tests.
  - Rider reformat/problems for changed code and tests.
- Manual Unity smoke check:
  - Fire a strong launch and observe that the end of **Launch Flight** no longer feels like a speed wall.
- Package version/changelog:
  - Not required.

## Blocked by

- Post-Launch Steering Gate.
