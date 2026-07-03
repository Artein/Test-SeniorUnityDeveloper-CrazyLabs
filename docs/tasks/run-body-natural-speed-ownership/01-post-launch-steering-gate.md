# Post-Launch Steering Gate

Type: AFK

## Parent

`docs/prd/prd-run-body-natural-speed-ownership.md`

## What to build

Add a condition-based **Post-Launch Steering Gate** so **Run Steering Control** does not fight the fired-by-slingshot interval.

After an accepted **Launch**, steering input must remain inactive while the **Run Body** is still in fired-by-slingshot unsupported motion. Steering becomes available when an observed unsupported post-launch sample is followed by valid **Run Surface** support.

Weak no-takeoff launches are handled by condition, not by timeout: if the **Run Body** remains grounded on a valid **Run Surface** and has no positive surface-normal lift, steering may become available. If the grounded sample still has positive lift, steering remains gated because the launch is still trying to take off.

This slice should deliver a complete movement path through launch notification, support sampling, controller gating, and tests, without changing speed caps yet.

## Acceptance criteria

- [ ] A new launch arms a post-launch steering gate.
- [ ] Unsupported support after launch keeps steering input from rotating or writing steering velocity.
- [ ] Observed unsupported support followed by valid **Run Surface** support enables steering.
- [ ] Stale grounded support with positive surface-normal lift does not enable steering.
- [ ] Grounded valid **Run Surface** support with no positive surface-normal lift enables steering for weak no-takeoff launches.
- [ ] Invalid support, obstacle, finish, safety, trigger, or missing-`RunContact` support does not enable steering.
- [ ] Time advancing alone never enables steering.
- [ ] Leaving **Running** clears gate state.
- [ ] A new launch resets gate state.
- [ ] **Launch Flight**, **Airborne**, Animator states, and presentation elapsed time are not inputs to the steering gate.

## Verification

- EditMode tests:
  - Gate arms on launch and blocks steering while unsupported.
  - Unsupported then valid grounded support enables steering.
  - Stale grounded positive-lift launch remains gated.
  - Grounded no-lift weak launch becomes steerable.
  - Invalid support does not enable steering.
  - Elapsed time alone does not enable steering.
  - Leaving **Running** and new launch reset state.
- PlayMode tests:
  - None required for the first slice unless composition wiring changes cannot be covered in EditMode.
- Static checks:
  - Unity connector compile before tests.
  - Rider reformat/problems for changed code and tests.
- Manual Unity smoke check:
  - Optional quick launch check: strong launch should not accept steering until real landing; weak grounded launch should become steerable once sliding without lift.
- Package version/changelog:
  - Not required.

## Blocked by

None - can start immediately.
