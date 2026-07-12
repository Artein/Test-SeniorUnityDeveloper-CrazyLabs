# 04 — Stabilize Brief Missing Support in Seconds

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 1–4, 7–9, 12–13, 17–19, 24–32, 36–40, 44, 56–58, 61–62, 64.

## What to build

Make the shared stability policy the sole owner of cross-tick support-loss behavior. Replace baked missed-sample counting with elapsed-seconds semantics while preserving stable locomotion across short missing observations and distinguishing a missing probe from an unavailable probe source.

## Acceptance criteria

- [x] A supported observation acquires or refreshes stable support immediately.
- [x] A missing observation holds the last stable support only while configured grace remains.
- [x] Support transitions to lost when accumulated missing duration reaches or exceeds the configured grace.
- [x] The first missing sample contributes its fixed delta time to accumulated duration.
- [x] Zero grace causes immediate support loss on the first missing observation.
- [x] An unavailable observation performs an immediate hard reset and never consumes grace.
- [x] Reacquisition clears missing duration and publishes the correct transition once.
- [x] The legacy source no longer owns missed-sample temporal state.
- [x] At 0.02 seconds, approved compatibility values reproduce the characterized loss boundary within one fixed tick.
- [x] At 0.01 and 0.02 seconds, the same seconds threshold differs by no more than one fixed tick.
- [x] Brief observed gaps do not falsely remove stable movement support.
- [x] Policy evaluation performs no managed allocation.

## Verification

- EditMode tests: Cover first-sample accumulation, below/at/above threshold, zero grace, unavailable reset, reacquisition, and timestep variation.
- PlayMode tests: Run the canonical brief gap and sustained loss scenarios at 0.01 and 0.02 seconds.
- Static checks: Confirm no missed-sample counter or duplicate support grace remains in the physics source or consumers.
- Manual Unity smoke check: Cross the canonical gap and walk off an edge; verify held support and true loss feel distinct.
- Package version/changelog: Record a behavior note if release policy requires one for seconds-based support-loss tuning.

## Blocked by

- [03 — Publish Atomic Observed and Compatibility-Stable Snapshot](03-publish-atomic-observed-and-compatibility-stable-snapshot.md)
