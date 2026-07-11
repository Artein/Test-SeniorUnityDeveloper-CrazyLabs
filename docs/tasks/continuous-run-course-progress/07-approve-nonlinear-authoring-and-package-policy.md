# 07 — Approve non-linear authoring, package, and projection-recovery policy

**Type:** HITL
**User stories covered:** 18–25, 28, 43–45, 50, 55–56, 82–85

## Parent

[Continuous Run Course Progress PRD](../../prd/prd-continuous-run-course-progress.md)

## What to build

Approve the concrete authoring and technology policy for the first non-linear Run Course. Select the route technology, define whether Unity Splines becomes a pinned direct gameplay dependency behind an isolated adapter assembly, and approve the supported course shape and validation envelope.

The decision must define authoring direction, snapshot timing, arc-length preparation, ordinary continuity bounds, projection failure, explicit global reacquisition, optional lane orientation, diagnostic expectations, and the first curved/vertical acceptance fixture. The first slice remains one continuous open route without branching, loops, or custom editor tooling.

## Acceptance criteria

- [ ] The selected route technology and ownership rationale are recorded and approved.
- [ ] If Unity Splines is selected, its gameplay use requires a pinned direct manifest dependency and an isolated adapter assembly; gameplay core remains vendor-independent.
- [ ] The first supported topology is one continuous open route with explicit start, end, and travel direction.
- [ ] Snapshot timing and runtime mutation policy are explicit.
- [ ] Arc-length preparation, ordinary local projection bounds, validity state, off-route diagnostics, and allocation expectations are explicit.
- [ ] The policy distinguishes ordinary continuity from explicit teleport/reacquisition and specifies who may request global reacquisition.
- [ ] Validation rules cover non-finite data, zero-length sections, invalid tangents/orientation, and geometry outside the first slice's ambiguity envelope.
- [ ] The first curved/vertical acceptance fixture and its ownership through typed test assets are approved.
- [ ] Branches, loops, laps, and canonical branch progress remain deferred to a separate ADR.
- [ ] No custom editor dependency is required for the first non-linear slice.

## Verification

- EditMode tests: Not applicable to the decision-only slice.
- PlayMode tests: Not applicable to the decision-only slice.
- Static checks: Validate decision/ADR structure, package-policy consistency, dependency direction, and agreement with the parent PRD.
- Manual Unity smoke check: Inspect the proposed acceptance fixture and authoring workflow in Unity before approval.
- Package version/changelog: Record the approved package/version policy; do not install or promote the dependency in this HITL slice.

## Blocked by

- 01 — Approve the Run Course coordinate contract and migration policy
