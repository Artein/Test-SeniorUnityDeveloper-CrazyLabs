# Run Surface Stability Baseline and Verification

## Legacy timing baseline

Before the unified pipeline, physics support and steering applied separate temporal filters.

| Concern | Legacy rule | 0.01 s fixed step | 0.02 s fixed step |
|---|---|---:|---:|
| Physics support loss | lose on second consecutive miss | 0.02 s | 0.04 s |
| Physics suspect normal | accept on second identical suspect sample | 0.02 s | 0.04 s |
| Steering support grace | serialized 0.12 s timer | approximately 0.12 s | approximately 0.12 s |
| Steering suspect normal | serialized 0.60 s timer | approximately 0.60 s | approximately 0.60 s |

The physics thresholds were sample-count based, so changing fixed timestep changed their real duration. Steering then filtered the already-filtered support again, making the visible and gameplay boundary difficult to predict.

## Canonical integrated fixture

`RunSurfaceFramePipelinePlayModeTests` drives the real physics probe, stability policy, steering policy, and published snapshot through:

- continuous ground;
- a fast seam;
- a trough;
- a brief support gap;
- sustained support loss and reacquisition;
- alternating discontinuous normals;
- coherent discontinuous normals;
- unavailable progress-frame hard reset.

Per tick, assertions and failure context cover the selected Observed Support, Stable Support, selected surface normal and distance, transition, held/confirming flags, steering validity/up, and representative consumer behavior. Failures identify the tick and scenario label. Characterization was introduced before behavior changes and did not modify production behavior.

## Unified seconds-based behavior

- The first qualifying `Missing` or discontinuous sample contributes its fixed delta time.
- Support is lost and coherent discontinuities confirm at `elapsed >= configured duration`.
- Zero duration transitions on the first qualifying sample.
- Equivalent 0.01 s and 0.02 s scenarios differ by at most one fixed tick.
- `Unavailable` bypasses timers and produces an immediate `HardReset`.
- A brief Observed gap held by Stable Support does not switch locomotion or award airtime.
- Coherent candidates use a normalized deterministic representative; alternating incompatible candidates cannot accumulate confirmation.

## Automated evidence

- Stability, steering, config, and pipeline EditMode coverage: 49/49 passing.
- Physics probe characterization and legacy-regression coverage: 22/22 passing.
- Integrated pipeline trace: 5/5 passing.
- Airtime EditMode behavior: 6/6 passing.
- End-to-end physics-pipeline-airtime-reward PlayMode coverage: 7/7 passing at 0.01 s and 0.02 s.
- Atomic diagnostics coverage: 20/20 EditMode plus integrated PlayMode trace passing.
- Managed-allocation guards cover stability evaluation and the warmed pipeline tick.

- Full-project verification after legacy removal: 1,034/1,034 passing, 0 failed, 0 warnings (`r_dvb2peyh`).
- Final compile fingerprint was fresh and the post-test settle barrier reported no compile errors.

The required human feel-review matrix and approval remain recorded separately in [feel-review.md](feel-review.md).
