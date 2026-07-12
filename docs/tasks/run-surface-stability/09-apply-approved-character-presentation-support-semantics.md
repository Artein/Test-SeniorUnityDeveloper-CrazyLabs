# 09 — Apply Approved Character Presentation Support Semantics

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 11, 20, 45, 66–67.

## What to build

Apply the character-presentation support semantics approved in issue 01 through the existing presentation tracker. Keep presentation ownership separate from locomotion authority and avoid introducing another smoothing or support-stability layer.

## Acceptance criteria

- [x] Character presentation consumes exactly the support context approved in issue 01.
- [x] The presentation tracker receives the selected context from the atomic snapshot.
- [x] A brief observed miss produces the approved visual response without changing locomotion support.
- [x] A confirmed seam or trough transition produces the approved orientation response.
- [x] Landing and support reacquisition update presentation exactly once.
- [x] Presentation code does not mutate stable support, steering state, or Rigidbody motion.
- [x] No new support grace, normal confirmation, or competing smoothing policy is introduced.
- [x] Existing presentation-specific interpolation remains unchanged unless issue 01 explicitly approves a semantic adjustment.
- [x] Presentation remains finite and stable when support is unavailable.

## Verification

- EditMode tests: Verify presentation input selection and response for observed miss, held stable support, transition, reacquisition, and unavailable state.
- PlayMode tests: Run seam, trough, brief-gap, and landing visual scenarios with deterministic state assertions.
- Static checks: Confirm presentation reads through the tracker and does not access raw probe physics or write locomotion state.
- Manual Unity smoke check: Observe the character across seam, trough, gap, and landing from the gameplay camera.
- Package version/changelog: Add a presentation note only if issue 01 approves an intentional visible behavior change.

## Blocked by

- [01 — Approve Support Semantics and Authoring Boundary](01-approve-support-semantics-and-authoring-boundary.md)
- [03 — Publish Atomic Observed and Compatibility-Stable Snapshot](03-publish-atomic-observed-and-compatibility-stable-snapshot.md)
