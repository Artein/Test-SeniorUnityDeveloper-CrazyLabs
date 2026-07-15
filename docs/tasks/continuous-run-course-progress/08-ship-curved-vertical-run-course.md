# 08 — Ship one continuous curved and vertical Run Course end-to-end

**Type:** AFK
**User stories covered:** 1–2, 16, 18–23, 31–35, 39, 43–50, 57–63, 66, 69, 74–84

## Parent

[Continuous Run Course Progress PRD](../../prd/prd-continuous-run-course-progress.md)

## What to build

Deliver one complete non-linear Run Course through every migrated consumer. Implement the approved adapter in an isolated assembly, capture a stable route snapshot, prepare bounded arc-length data outside the hot path, and sample progress, tangent, optional orientation, distance-from-route, validity, and continuity state without recurring managed allocation.

Author one continuous open acceptance route containing horizontal curvature and meaningful elevation or vertical tangent. Use continuity-aware local projection for ordinary motion and reject geometry outside this slice's approved ambiguity envelope. Run Result, rewards, Lost Momentum, support probing, presentation, diagnostics, and scene composition must all work from the shared sample.

## Acceptance criteria

- [ ] The optional route technology is isolated behind the project-owned contract; gameplay core has no vendor-specific types or transitive dependency.
- [ ] If an optional package is adopted, it is pinned as a direct project dependency at the approved version.
- [ ] Route snapshot and arc-length lookup data are prepared outside the fixed-tick hot path and remain stable throughout the run.
- [ ] Equal traveled distance along a bend produces equal progress, and valid vertical longitudinal motion produces positive progress and signed speed.
- [ ] Ordinary projection uses the previous location token and bounded local search; it never falls back silently to global nearest.
- [ ] Unsupported ambiguous geometry, invalid tangents, non-finite data, and failed projection produce actionable validation or explicit invalid samples.
- [ ] Fixed-tick sampling produces no recurring managed allocations and has bounded work suitable for mobile gameplay.
- [ ] Run Result, rewards, Lost Momentum, support, presentation, and diagnostics consume the same shared non-linear sample.
- [ ] Exactly one valid source is resolved in the acceptance scene through VContainer.
- [ ] Tests obtain route and scene assets through typed test-asset providers rather than hardcoded paths.
- [ ] The current Ladybug scene remains on the straight adapter and retains exact behavior parity.

## Verification

- EditMode tests: Quarter-circle arc length; vertical tangent; signed forward/lateral/reverse speed; rollback; invalid authoring; projection validity; stable snapshot; bounded/allocation-free sampling.
- PlayMode tests: Curved/vertical acceptance route drives all consumers end-to-end; scene composition resolves one source; serialized references survive lifecycle boundaries; current Ladybug scene remains unchanged.
- Static checks: Rider problems clean; Unity compile gate clean before tests; adapter assembly isolation and dependency direction verified; package manifest consistency verified.
- Manual Unity smoke check: Traverse the curved/vertical fixture while observing progress, result, Lost Momentum, grounding, animation, and diagnostics.
- Package version/changelog: Pin the approved direct dependency if adopted; record the new course capability and any serialized authoring migration.

## Blocked by

- 03 — Make Run Result and rewards consume final shared progress
- 04 — Detect Lost Momentum from signed longitudinal course motion
- 05 — Decouple physics support probing from Run Course orientation
- 06 — Drive character presentation from explicit course and support motion
- 07 — Approve non-linear authoring, package, and projection-recovery policy
