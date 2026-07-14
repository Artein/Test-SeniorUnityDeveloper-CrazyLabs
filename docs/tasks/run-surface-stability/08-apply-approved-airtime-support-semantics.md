# 08 — Apply Approved Airtime Support Semantics

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 3, 10, 45, 56, 61, 64.

## What to build

Apply the airtime support semantics approved in issue 01. Make reward timing and airborne state transitions consume the named support signal directly, with deterministic behavior for a brief observed miss, sustained loss, reacquisition, and an unavailable probe source.

## Acceptance criteria

- [x] Airtime consumes exactly the support context approved in issue 01.
- [x] The first tick considered airborne is defined by an explicit shared state or transition.
- [x] A one-tick observed miss produces the approved airtime result and cannot accidentally award airtime through a hidden raw-probe read.
- [x] Sustained support loss starts airtime exactly once.
- [x] Support reacquisition ends or commits airtime exactly once according to existing reward rules.
- [x] An unavailable probe source follows the approved hard-reset behavior and cannot create a reward exploit.
- [x] Airtime has no private miss grace, sample counter, or normal filter.
- [x] Existing reward thresholds and scoring remain unchanged except where necessary to apply the approved support semantics.
- [x] Timestep variation changes the transition boundary by no more than one fixed tick.

## Verification

- EditMode tests: Cover brief miss, threshold loss, reacquisition, unavailable reset, duplicate-transition prevention, and timestep variation.
- PlayMode tests: Run canonical short-gap, true-jump, walk-off, and landing reward scenarios.
- Static checks: Confirm airtime does not read raw probe hits or own temporal surface state.
- Manual Unity smoke check: Cross the short gap and perform a real jump; verify only the approved case accrues airtime.
- Package version/changelog: Add a scoring/behavior note if the approved semantics intentionally change player-visible rewards.

## Blocked by

- [01 — Approve Support Semantics and Authoring Boundary](01-approve-support-semantics-and-authoring-boundary.md)
- [04 — Stabilize Brief Missing Support in Seconds](04-stabilize-brief-missing-support-in-seconds.md)
