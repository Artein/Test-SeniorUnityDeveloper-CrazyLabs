# 06 — Drive Run Steering Frame from Shared Transitions

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 4–5, 10, 15–16, 41–43, 63–64.

## What to build

Derive the run steering frame from the shared stable-support state and transition emitted by the stability policy. Retain steering-specific slew and snap presentation while removing its duplicate normal-validity and discontinuity-confirmation state.

## Acceptance criteria

- [ ] `ContinuousUpdate` steers toward stable up using the configured angular slew rate.
- [ ] `ConfirmedDiscontinuity` applies the configured confirmed-transition snap behavior exactly once.
- [ ] `SupportAcquired` initializes a valid steering frame deterministically.
- [ ] `SupportLost` preserves the approved airborne steering memory without representing the body as grounded.
- [ ] `HardReset` clears steering validity and transient state immediately.
- [ ] Steering consumes the atomic snapshot transition and does not infer the same transition from raw normals.
- [ ] Duplicate suspect-normal, confirmation-duration, and rounded-normal validation state is removed.
- [ ] Existing authored slew, snap, grace, and confirmation values remain unchanged until the feel-review issue approves tuning.
- [ ] The steering frame remains finite and normalized for every supported transition.
- [ ] Only one pipeline owner updates and publishes steering state per fixed tick.
- [ ] Characterized ordinary-slope, seam, gap, and airborne behavior remains compatible except for approved stability-policy changes.

## Verification

- EditMode tests: Drive every transition through the steering derivation and verify slew, snap, initialization, airborne memory, and reset.
- PlayMode tests: Re-run seam, trough, brief-gap, landing, and walk-off scenarios through the integrated pipeline.
- Static checks: Search for duplicate steering normal-confirmation state and confirm transition handling is exhaustive.
- Manual Unity smoke check: Traverse slopes, seam, gap, and landing; inspect heading stability and airborne orientation.
- Package version/changelog: Not required unless externally visible feel changes exceed the approved compatibility boundary.

## Blocked by

- [05 — Confirm Coherent Surface-Normal Discontinuities](05-confirm-coherent-surface-normal-discontinuities.md)
