# 10 — Expose Observed, Stable, Transition, and Steering Diagnostics

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 20, 37–40, 66, 68.

## What to build

Expose a read-only runtime diagnostic view of the atomic run-surface snapshot so developers can distinguish a physics observation from a held or confirmed locomotion context. Use the same terminology as the policy and avoid any diagnostic side effects on gameplay.

## Acceptance criteria

- [ ] Diagnostics display observed support state and normal when present.
- [ ] Diagnostics display stable support state and normal when present.
- [ ] Diagnostics display the current transition.
- [ ] Diagnostics indicate when support is held through a missing observation.
- [ ] Diagnostics indicate when a normal discontinuity is awaiting confirmation.
- [ ] Diagnostics display steering validity and steering up.
- [ ] Missing, unavailable, and supported states are visually distinguishable.
- [ ] Diagnostic labels use observed, stable, transition, held, confirming, and steering consistently.
- [ ] The view reads one atomic snapshot and cannot sample partially updated providers.
- [ ] Diagnostics do not allocate per physics tick while hidden and do not alter policy state.
- [ ] Existing diagnostic enable/disable conventions are preserved.

## Verification

- EditMode tests: Verify diagnostic-model mapping for every support state, transition, and flag combination.
- PlayMode tests: Exercise seam, brief gap, confirmation, support loss, and reset while asserting displayed model values.
- Static checks: Confirm diagnostics are read-only, consume the atomic snapshot, and contain no gameplay decisions.
- Manual Unity smoke check: Enable diagnostics and traverse canonical fixtures; compare labels with observed behavior tick by tick.
- Package version/changelog: Not required unless diagnostics are part of a shipped developer-facing package surface.

## Blocked by

- [06 — Drive Run Steering Frame from Shared Transitions](06-drive-run-steering-frame-from-shared-transitions.md)
