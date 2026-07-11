# 01 — Approve Support Semantics and Authoring Boundary

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 18, 21–22, 45, 55, 73–75.

## What to build

Record the product and architecture decisions needed before implementation: which support context airtime and character presentation consume, what the compatibility surface means during migration, where probe and stability tuning remains authored, and how earlier overlapping planning is superseded. This is a human-in-the-loop decision issue; it changes no runtime behavior.

## Acceptance criteria

- [ ] Airtime is explicitly assigned observed support, stable support, or a documented combination, including the expected result during a one-tick observed miss.
- [ ] Character presentation is explicitly assigned observed support, stable support, or a documented combination, including the expected visual response at seams and landing.
- [ ] The compatibility surface-context contract is defined as stable support for the migration window.
- [ ] Probe geometry/filtering ownership and temporal stability ownership are separately named.
- [ ] The initial authoring boundary is approved: retain existing serialized owners or approve a migration with an explicit serialization plan.
- [ ] The dedicated `RunSurfaceTuning` asset is either approved now or explicitly deferred; the default is deferred.
- [ ] Runtime state is prohibited from shared authoring assets.
- [ ] Earlier overlapping run-surface planning is marked superseded, retained as historical context, or reconciled by an explicit precedence statement.
- [ ] The decision records observed support and stable support using the terminology from the parent PRD.
- [ ] A human owner records approval before dependent implementation begins.

## Verification

- EditMode tests: Not required; this issue approves semantics and authoring boundaries without code changes.
- PlayMode tests: Not required; this issue approves semantics and authoring boundaries without runtime changes.
- Static checks: Confirm every decision above is recorded unambiguously and uses the parent PRD terminology.
- Manual Unity smoke check: Not required; no Unity asset or runtime behavior changes.
- Package version/changelog: Not required; no shipped behavior or package surface changes.

## Blocked by

None — can start immediately.
