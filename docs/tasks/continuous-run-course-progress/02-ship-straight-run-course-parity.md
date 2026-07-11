# 02 — Ship the straight Run Course sample with exact Ladybug parity

**Type:** AFK
**User stories covered:** 9, 16–21, 31–37, 46–49, 54, 57–61, 66, 69, 74–80, 84

## Parent

[Continuous Run Course Progress PRD](../../prd/prd-continuous-run-course-progress.md)

## What to build

Deliver the first complete Run Course tracer bullet using a straight authored route. Introduce the project-owned course contracts, capture a stable route snapshot at run start, project the Launch Target once as the progress origin, and have Run Progress Service publish one immutable sample per fixed tick.

Wire the current Gameplay Scene through VContainer to exactly one straight source. Preserve the existing Ladybug +Z calculations, tuning, lifecycle, and consumer compatibility while establishing the seam used by later curved-course work.

## Acceptance criteria

- [ ] The course boundary is project-owned and exposes only stable course sampling concepts, with no dependency on an optional spline vendor.
- [ ] The straight source rejects non-finite inputs, zero-length direction, invalid tangents, and invalid authoring with actionable diagnostics.
- [ ] The launch position is projected once and subsequent progress is relative to that origin.
- [ ] Run Progress Service owns exactly one immutable sample per fixed tick, including current progress, maximum progress, tangent, longitudinal speed, validity, and location continuity state.
- [ ] Current progress can decrease during rollback while maximum progress remains monotonic.
- [ ] Straight-source progress and longitudinal speed are numerically equivalent to the existing progress-frame behavior for the Ladybug route.
- [ ] Sampling performs no recurring managed allocations and uses no static mutable run state.
- [ ] VContainer scene composition resolves exactly one valid source and keeps lifecycle policy in plain C# services with shallow Unity adapters.
- [ ] Existing serialized scene references survive domain reload and scene load without resetting current tuning.
- [ ] The current Gameplay Scene behaves identically in launch, traversal, progress, and run-end smoke checks.

## Verification

- EditMode tests: Straight projection parity; launch-relative origin; rollback and monotonic maximum; lateral/reverse velocity; invalid/non-finite authoring; immutable per-tick sample; allocation guard where deterministic.
- PlayMode tests: Scene composition resolves one source; serialized references survive scene load/domain reload boundaries; fixed-tick sample is shared; current Ladybug progress remains +Z-compatible.
- Static checks: Rider problems clean; Unity compile gate clean before tests; dependency direction and assembly references remain valid.
- Manual Unity smoke check: Launch and traverse the Ladybug course; compare progress and tuning behavior with the pre-migration baseline.
- Package version/changelog: No new package; add release notes only if serialized migration or externally observable behavior requires them.

## Blocked by

- 01 — Approve the Run Course coordinate contract and migration policy
