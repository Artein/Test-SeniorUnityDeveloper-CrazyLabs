# Install the Single-Writer Movement Baseline

Type: AFK

## Parent

[PRD: Run Body Explicit Speed Ownership](../../prd/prd-run-body-explicit-speed-ownership.md)

## What to build

Replace the active steering-owned Rigidbody write path with one **Run Body Movement Controller** that orchestrates the current movement behavior in a fixed step. This slice establishes ownership without intentionally changing speed feel.

The controller samples one coherent steering-input snapshot, sanitizes impossible velocity, applies launch-landing stabilization, evaluates direction-only steering, resolves the accepted physical support plane, preserves corrected surface-normal velocity, derives final facing, and sends one final target state through a thin **Run Body Movement Target**. A neutral speed decision keeps intentional tangent-speed magnitude equivalent to current behavior until later slices add explicit effects.

The replacement and legacy writers must never be active together. Lifecycle state, launch activation, landing stabilization, steering-frame reset, and input reset remain explicit collaborators rather than hidden inside speed policy.

## Acceptance criteria

- [ ] The **Run Body Movement Controller** is the only active fixed-step owner of Run Body movement target writes.
- [ ] The legacy steering controller cannot write movement while the replacement controller is registered.
- [ ] Each active fixed pass advances and reads steering input once into a coherent snapshot.
- [ ] Defensive velocity sanitation occurs before landing stabilization and movement policy evaluation.
- [ ] Existing launch-landing stabilization behavior is preserved as a separate collaborator.
- [ ] Run steering evaluation changes direction intent without intentionally changing tangent-speed magnitude.
- [ ] Steering intent is projected onto the accepted **Run Surface** plane before the final movement target is built.
- [ ] An invalid projected steering intent falls back only to a usable existing tangent direction.
- [ ] Directionless motion does not receive a synthesized heading.
- [ ] Corrected surface-normal velocity is preserved when the final tangent velocity is composed.
- [ ] Final facing derives from the final projected tangent direction, not a pre-projection steering result.
- [ ] Exactly one final target-state write occurs during each active fixed pass.
- [ ] Inactive lifecycle state writes no movement target and clears movement-owned input, stabilization, and frame state.
- [ ] A new launch resets and activates the expected movement lifecycle.
- [ ] Unsupported motion keeps speed policy neutral while existing direction-only air steering behavior remains intact.
- [ ] Existing launch strength, airborne motion, landing tangent momentum, steering feel, and defensive speed containment remain behaviorally equivalent.

## Verification

- EditMode tests:
  - Fixed-pass composition orders sanitation, landing stabilization, steering evaluation, support projection, facing, and one final write.
  - Corrected surface-normal velocity survives tangent direction composition.
  - Invalid projected intent falls back only to an existing usable tangent direction.
  - Exact-zero tangent motion does not gain a heading.
  - Inactive state performs no movement write and reset/new-launch transitions clear owned state.
  - Steering evaluation preserves tangent-speed magnitude within numeric tolerance.
  - Unsupported speed output is neutral while air steering remains direction-only.
- PlayMode tests:
  - Gameplay composition resolves one movement controller and one movement target.
  - The legacy and replacement writers are never active together.
  - Launch, unsupported flight, first valid landing, steering, and run exit preserve existing scene behavior.
- Static checks:
  - Rider reformat and problem inspection for changed code and tests.
  - Unity connector compile before tests.
  - Search composition and fixed-step registrations for a second active Rigidbody movement writer.
- Manual Unity smoke check:
  - Compare strong and weak launches, steering after landing, short unsupported transitions, and run reset against the pre-slice baseline.
- Package version/changelog:
  - Not required.

## Blocked by

- [01 - Migrate Existing Movement Tuning to a Validated Config](01-migrate-existing-movement-tuning-to-validated-config.md)
