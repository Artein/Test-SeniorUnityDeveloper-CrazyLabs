# 02 — Capture End-to-End Surface Transition Baseline

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 56, 59–67, 70–71.

## What to build

Create deterministic characterization coverage for the current end-to-end path from physics probing through the steering frame and its consumers. Capture fast seams, troughs, brief gaps, alternating normals, and fixed-timestep variation before responsibilities move, so later slices can distinguish intended compatibility from deliberate policy improvements.

## Acceptance criteria

- [x] A canonical physics fixture represents continuous ground, a fast seam, a trough, and a brief support gap.
- [x] Characterization records raw hit selection, published support, surface normal, steering up, and relevant consumer output per fixed tick.
- [x] Current support-loss timing is captured at the project fixed timestep.
- [x] Current normal-discontinuity confirmation timing is captured at the project fixed timestep.
- [x] Equivalent scenarios run at 0.01-second and 0.02-second fixed timesteps and document existing tick-coupled differences.
- [x] Alternating discontinuous normals are covered separately from coherent discontinuous normals.
- [x] A brief airborne frame and a true sustained support loss are distinguishable in the trace.
- [x] Tests exercise the integrated physics-source-to-steering path rather than isolated helper behavior only.
- [x] Characterization introduces no production behavior change.
- [x] Failure output identifies the tick and observed transition that diverged.

## Verification

- EditMode tests: Run deterministic characterization tests for existing temporal filters and combined policy ordering where engine physics is not required.
- PlayMode tests: Run the canonical seam, trough, gap, and timestep scenarios through Unity physics.
- Static checks: Confirm fixtures use typed test assets where assets are required and contain no arbitrary waits or screenshot-only assertions.
- Manual Unity smoke check: Traverse the canonical fixture once and confirm the recorded scenarios match the visible geometry.
- Package version/changelog: Not required; characterization only, with no production behavior change.

## Blocked by

None — can start immediately.
