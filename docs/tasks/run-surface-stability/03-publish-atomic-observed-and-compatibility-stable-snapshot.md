# 03 — Publish Atomic Observed and Compatibility-Stable Snapshot

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 20, 23–25, 30–31, 37–40, 46, 48–54, 64, 68.

## What to build

Introduce one ordered run-surface pipeline that resolves current-tick physics into observed support, applies compatibility-preserving temporal state, derives the steering frame, and publishes one immutable snapshot. Separate immutable probe configuration from stability configuration while retaining existing authored values and behavior at this slice.

## Acceptance criteria

- [x] Observed support has explicit `Unavailable`, `Missing`, and `Supported` states.
- [x] Observed support contains only same-tick spatial resolution and no cross-tick history.
- [x] Stable support is published separately and mirrors the legacy filtered surface context in all characterization scenarios.
- [x] One immutable snapshot atomically exposes observed support, stable support, transition, held/confirming state, steering up, and steering validity.
- [x] A compatibility adapter exposes stable support to unmigrated consumers without a second temporal filter.
- [x] Probe configuration owns geometry and same-tick hit-selection parameters only.
- [x] Stability configuration owns temporal support and normal-confirmation parameters only.
- [x] Runtime state belongs to the policy instance and is not stored in shared authoring data.
- [x] Dependency injection guarantees probe, stability, steering derivation, and publication run once in the documented order per physics tick.
- [x] The pipeline performs no per-tick managed allocation.
- [x] Existing characterization remains green without intentional behavior changes.

## Verification

- EditMode tests: Verify snapshot atomicity, state mapping, compatibility identity, update order, and independent policy instances.
- PlayMode tests: Re-run the end-to-end characterization suite and assert legacy-compatible stable output.
- Static checks: Confirm configuration types are immutable at runtime, the compatibility adapter does not filter, and the pipeline has one update owner.
- Manual Unity smoke check: Traverse continuous ground and a seam; confirm no visible behavior regression.
- Package version/changelog: Not required unless the project treats the new internal contract as release-note worthy; do not change package dependencies.

## Blocked by

- [01 — Approve Support Semantics and Authoring Boundary](01-approve-support-semantics-and-authoring-boundary.md)
- [02 — Capture End-to-End Surface Transition Baseline](02-capture-end-to-end-surface-transition-baseline.md)
