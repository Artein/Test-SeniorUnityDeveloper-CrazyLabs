# 01 — Capture Speed-Scaled Grounding Baseline

## Parent

[Run Locomotion Attachment and Steering Support Separation PRD](../../prd/prd-run-locomotion-attachment-and-steering-support.md) — user stories 1–20, 29–30, 65–67, 91–122, and 129.

## What to build

Extend the existing integrated Run Surface characterization into a deterministic speed-and-geometry baseline that shows how current Observed Support, retained Stable Support, steering, movement, and airtime behave together. Exercise the production configuration at 5, 20, and 40 metres per second across walk-off, ramp departure, seam, short gap, greater-than-60-degree transition, and landing scenarios. Record the current physical distance over which grounded behavior persists without changing production behavior, then use the evidence to support the semantic and severity decisions in issue 02.

## Acceptance criteria

- [ ] Characterization reuses or extends the existing integrated Run Surface fixtures rather than creating a competing test architecture.
- [ ] The canonical matrix covers 5, 20, and 40 metres per second where each geometry is physically meaningful.
- [ ] Walk-off, upward ramp departure, short seam, short unsupported gap, greater-than-60-degree support transition, landing, and unavailable lifecycle reset are represented deterministically.
- [ ] Each fixed-step trace includes body position and velocity, Observed Support, Stable Support, published attachment transition, Run Steering Frame, grounded movement effects, and physical airtime output available from the current contracts.
- [ ] The active 0.12-second and 0.6-second values are exercised and their time-to-distance exposure is reported at all three speeds.
- [ ] The baseline distinguishes sampled edge uncertainty from additional filter-retention distance.
- [ ] Equivalent focused scenarios run at 0.02-second and 0.01-second fixed steps and report quantization differences.
- [ ] Failure output identifies scenario, speed, fixed tick, expected signal, and observed signal.
- [ ] No production policy, composition, serialized value, scene geometry, Rigidbody setting, or player behavior changes in this issue.
- [ ] The evidence records whether current player-visible impact supports P1, P2, or remains inconclusive without changing the PRD's architecture decision.

## Verification

- EditMode tests: Characterize current attachment-transition, retained-support, movement-gating, and airtime sequences that do not require Unity physics.
- PlayMode tests: Run the full geometry and speed matrix through the real physics probe, surface pipeline, movement boundary, and airtime tracker.
- Static checks: Confirm no production behavior or serialized asset changed; confirm tests use no reflection, hard-coded asset paths, arbitrary waits, or screenshot-only assertions.
- Manual Unity smoke check: Traverse one edge, ramp, seam, sharp transition, and landing at representative low and high speed with existing diagnostics visible.
- Package version/changelog: Not required; characterization only.

## Blocked by

None — can start immediately.
