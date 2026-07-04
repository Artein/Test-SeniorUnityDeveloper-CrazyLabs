## Parent

[Character Visual Follower Presentation Smoothing PRD](../../prd/prd-character-visual-follower-presentation-smoothing.md)

## What to build

Replace the snap-only visual follower output with bounded position and heading smoothing while keeping the same presentation-only data flow.

The implementation should add a pure, testable pose-smoothing component that follows the **Launch Target** tightly enough that the player never sees a detached character. Position should converge quickly, heading should remain fairly tight, and large discontinuities should snap instead of visibly trailing. The smoother should be deterministic for supplied delta time and easy to test without Unity scene lifecycle callbacks.

This slice should not yet solve the slower up/tilt smoothing problem; it may preserve the current target up basis or use the existing direct target rotation for tilt until the next slice.

## Acceptance criteria

- [ ] A pure internal pose smoother accepts current visual pose, target render pose, tuning, delta time, and reset/snap state.
- [ ] Position smoothing uses the configured position response default of `60`.
- [ ] Heading smoothing uses the configured heading response default of `45`.
- [ ] Position lag is clamped by the configured max position lag default of `0.06` meters.
- [ ] Target jumps at or beyond the configured snap distance default of `0.75` meters snap immediately instead of easing across space.
- [ ] Smoothed heading follows yaw/forward changes without overshoot or invalid rotation output.
- [ ] The follower uses the smoother output after the snap-only tracer-bullet path is replaced.
- [ ] Existing forced-snap lifecycle boundaries from issue 01 still snap immediately and clear any accumulated smoothing state.
- [ ] The smoothed **Character Visual Anchor** remains visual-only and is not consumed by camera, steering, progress, run-end, physics, or slingshot logic.
- [ ] No public gameplay API, new package, ScriptableObject config, Rigidbody movement change, or camera follow-target change is introduced.

## Verification

- EditMode tests:
  - Position converges toward a moving target for fixed delta-time steps.
  - Position never exceeds the configured max lag when the target remains within normal movement distance.
  - A target jump at or beyond snap distance snaps immediately.
  - Heading converges toward a changed target heading without overshooting past the target.
  - Forced reset/snap clears smoothing state and copies the target pose exactly.
  - The smoother is deterministic for the same input pose sequence and delta times.
- PlayMode tests:
  - Not expected unless VContainer or scene wiring needs additional coverage beyond issue 01.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Source search confirms no physics, camera, steering, progress, or run-end dependency was redirected to smoothed visual pose.
- Manual Unity smoke check:
  - Start a run and confirm the visible character remains attached during launch, sliding, and run end.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

[01 - Snap-Only Character Visual Follower Tracer Bullet](01-snap-only-character-visual-follower-tracer-bullet.md)
