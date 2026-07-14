# 07 — Migrate Core Locomotion to Explicit Stable Support

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 3–4, 8–10, 27, 39, 43–44, 52–54, 64.

## What to build

Migrate core locomotion from the compatibility surface contract to explicit stable support and the shared steering frame. Use stable support for movement-plane projection, speed behavior, steering mode, and landing stabilization while preserving the existing Rigidbody contact model and single movement writer.

## Acceptance criteria

- [x] Movement-plane projection reads explicit stable support.
- [x] Grounded speed behavior reads explicit stable support.
- [x] Steering-mode selection uses stable support and the shared steering frame consistently.
- [x] Landing stabilization uses the shared transition rather than a separately inferred surface change.
- [x] A brief observed miss does not switch core locomotion to airborne behavior while stable support is held.
- [x] Sustained support loss switches locomotion at the shared `SupportLost` transition.
- [x] An unavailable probe source causes the approved hard-reset locomotion response.
- [x] No core-locomotion consumer applies its own support grace or normal confirmation.
- [x] Rigidbody contact physics remains the collision authority.
- [x] Exactly one movement writer remains active.
- [x] Existing speed-model and contact-physics invariants from ADR-0010 remain intact.

## Verification

- EditMode tests: Verify locomotion decisions for supported, held-missing, support-lost, reacquired, and hard-reset snapshots.
- PlayMode tests: Run continuous ground, brief gap, walk-off, landing, seam, and trough locomotion scenarios.
- Static checks: Confirm core locomotion no longer reads the compatibility adapter and has no duplicate temporal filter.
- Manual Unity smoke check: Complete a representative run across slopes, gap, edge, and landing; verify control continuity.
- Package version/changelog: Add a behavior note only if the migrated explicit semantics produce an approved player-visible change.

## Blocked by

- [05 — Confirm Coherent Surface-Normal Discontinuities](05-confirm-coherent-surface-normal-discontinuities.md)
