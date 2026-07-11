# 01 — Approve the Run Course coordinate contract and migration policy

**Type:** HITL
**User stories covered:** 14, 29–30, 36–37, 42, 49, 55–56, 82, 85

## Parent

[Continuous Run Course Progress PRD](../../prd/prd-continuous-run-course-progress.md)

## What to build

Produce and approve the architecture decision that makes the Run Course coordinate system the shared semantic foundation for progress and local longitudinal motion. Define the project-owned course-source boundary, immutable fixed-tick sample, opaque location continuity token, launch-relative progress origin, current-versus-maximum progress semantics, invalid-sample behavior, and the separation between course orientation and the physics support/gravity frame.

The decision must also state the migration policy for public and serialized contracts, make reverse-recovery behavior explicit, and record branching, loops, laps, and route canonicalization as deferred work rather than hiding those product decisions inside projection code.

## Acceptance criteria

- [ ] An ADR defines the Run Course glossary and the ownership boundary between the authored source, stable run snapshot, Run Progress Service, and downstream consumers.
- [ ] Progress is defined from authored longitudinal distance relative to the projected launch location; current progress may decrease and maximum progress is monotonic.
- [ ] Signed longitudinal speed is defined as Run Body velocity projected onto the current normalized course tangent.
- [ ] Projection failure and non-finite input produce an explicit invalid state; consumers may retain the last valid sample only under a documented policy and may not invent progress.
- [ ] Course orientation is explicitly independent from support/gravity direction and collision-probe policy.
- [ ] Any reverse-recovery exception is an explicit product rule; unsigned speed is not used as a hidden substitute.
- [ ] Public API and serialized-data migration boundaries are identified and approved before implementation.
- [ ] Branching, loops, laps, active-route topology, and canonical branch progress are explicitly deferred to a separate ADR.
- [ ] The decision states which later issues can proceed without selecting a spline package.

## Verification

- EditMode tests: Not applicable to the decision-only slice.
- PlayMode tests: Not applicable to the decision-only slice.
- Static checks: Validate ADR structure and index; check terminology against the PRD and existing architecture decisions.
- Manual Unity smoke check: Not applicable.
- Package version/changelog: No package change; document compatibility and migration implications in the ADR.

## Blocked by

None - can start immediately.
