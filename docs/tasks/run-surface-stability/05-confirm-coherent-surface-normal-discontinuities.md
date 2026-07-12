# 05 — Confirm Coherent Surface-Normal Discontinuities

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 5–6, 14, 33–40, 42, 59–62, 65–66.

## What to build

Move cross-tick normal-discontinuity confirmation into the shared stability policy. Confirm only coherent candidate normals for the configured duration, reject alternating or incompatible candidates, and publish a representative confirmed normal with an explicit transition.

## Acceptance criteria

- [x] Ordinary normal changes within the continuity threshold update stable support continuously without confirmation delay.
- [x] A discontinuous candidate starts confirmation and preserves the current stable normal until confirmation succeeds.
- [x] Coherent candidates accumulate elapsed fixed time and confirm at or above the configured duration.
- [x] The first candidate sample contributes its fixed delta time.
- [x] Zero confirmation duration confirms on the first discontinuous candidate.
- [x] Alternating incompatible candidates cannot accumulate into a false confirmation.
- [x] A return to the stable-normal neighborhood cancels pending confirmation.
- [x] The confirmed normal is a deterministic representative of coherent candidates, not an arbitrary last sample.
- [x] Confirmation publishes `ConfirmedDiscontinuity` exactly once, while ordinary changes publish `ContinuousUpdate`.
- [x] Missing or unavailable support clears or preserves confirmation state according to the documented support-state rules.
- [x] The legacy physics source no longer owns suspect-normal sample state.
- [x] Equivalent 0.01-second and 0.02-second scenarios confirm within one fixed tick of the configured seconds threshold.

## Verification

- EditMode tests: Cover continuous changes, coherent confirmation, alternating rejection, cancellation, representative normal, zero duration, and timestep variation.
- PlayMode tests: Run canonical fast-seam and trough scenarios and assert transitions and stable normals.
- Static checks: Confirm only the shared policy owns cross-tick normal candidates and that transition values are exhaustive.
- Manual Unity smoke check: Traverse seam and trough fixtures; verify no oscillation or delayed response after coherent confirmation.
- Package version/changelog: Record a behavior note if release policy requires one for seconds-based normal confirmation.

## Blocked by

- [04 — Stabilize Brief Missing Support in Seconds](04-stabilize-brief-missing-support-in-seconds.md)
