# 04 — Approve Contact-Safety Evidence and Remediation Boundary

## Parent

[High-Speed Run Contact Safety PRD](../../prd/prd-high-speed-run-contact-safety.md) — user stories 18, 36–38, 50, 55–56, 58, 60. Conditional fallback stories 9, 34, 39–49, 51–54, and 57 remain unclaimed unless this gate authorizes remediation.

## What to build

Review the evidence from the solid-obstacle, Run Finish, and Run Safety Net tracer bullets and record one explicit outcome. Either approve that the existing production physics and authored contact volumes satisfy the supported safety envelope with no production fix, or identify a deterministic failing boundary and authorize the smallest remediation class. Prefer trigger-volume authoring correction when it preserves gameplay intent. Authorize an explicit swept-query path only for a reproducible miss that acceptable authoring cannot solve. This is a human-in-the-loop decision issue and does not implement the remedy.

## Acceptance criteria

- [ ] The review records the exact Unity version, fixed timestep, production Run Body geometry, 40 m/s acceptance results, and separate 80 m/s stress results.
- [ ] Compile status, filtered PlayMode results, relevant EditMode priority results, scene-contract checks, and any manual smoke evidence are reported separately.
- [ ] Solid collision evidence and Finish or Safety Net trigger evidence are evaluated as separate detection contracts.
- [ ] Each boundary is classified as proved safe, deterministic failure, flaky/inconclusive, or outside the approved envelope.
- [ ] A flaky or inconclusive PlayMode scenario blocks closure and returns to diagnosis; repeated retries are not accepted as evidence.
- [ ] If all supported-tier evidence passes, the recorded outcome explicitly states that no production, authoring, package, or ProjectSettings fix is needed.
- [ ] If a trigger's supported-tier traversal margin is insufficient, widening or reshaping that authored volume is evaluated before runtime querying.
- [ ] A runtime swept query is authorized only when a deterministic regression demonstrates a miss and the accepted authoring remedy is insufficient.
- [ ] Any proposed swept path must use an explicit Unity physics query rather than another trigger callback.
- [ ] Any authorized runtime remedy preserves the production sphere authority, Obstacle Impact contact-normal threshold, explicit contact-category filtering, static-obstacle scope, Run End candidate boundary, and priority Finished > ObstacleHit > OutOfBounds > LostMomentum.
- [ ] Any authorized runtime remedy preserves plain C# policy behind an injected shallow Unity adapter, VContainer composition, existing assembly direction, and no static service.
- [ ] Any public API, serialized field, save format, scene contract, package, or ProjectSettings change is named explicitly and routed through the repository's approval boundary.
- [ ] Relevant official Unity fixed-timestep, collision-detection, and PhysX trigger-CCD sources are linked in the decision.
- [ ] If shipped behavior or a permanent authoring invariant changes, required gameplay context, ADR, changelog, and mobile query profiling follow-up are identified.
- [ ] If remediation is approved, the actual failing scenario becomes its regression test and a new tracer-bullet issue breakdown is produced from only the selected remedy branch.
- [ ] A human owner records approval of the outcome.

## Verification

- EditMode tests: Review the targeted priority and candidate-latching results supplied by Issues 01–03; do not add tests in this decision-only issue.
- PlayMode tests: Review the exact filtered obstacle, Finish, and Safety Net results and their deterministic diagnostics.
- Static checks: Confirm the evidence report distinguishes acceptance from stress, collision from trigger overlap, and no-fix from conditional remediation.
- Manual Unity smoke check: Run only when scene authoring or visual crossing intent requires human judgment; record the result independently from automated evidence.
- Package version/changelog: Not required for a no-fix outcome. Required follow-up must be named if shipped behavior changes.
- Decision audit: Confirm one outcome is selected, conditional fallback stories are not claimed prematurely, and any approved remedy is handed to a new issue breakdown before implementation.

## Blocked by

Issues 01–03 — all three safety boundaries must have deterministic evidence before this decision.
