# 06 — Enforce Approved High-Speed Edge Precision

## Parent

[Run Locomotion Attachment and Steering Support Separation PRD](../../prd/prd-run-locomotion-attachment-and-steering-support.md) — user stories 1–3, 16, 22–23, 29, 61, 65–67, 87, 112–118, and 129.

## What to build

Implement and prove the high-speed observation guarantee selected in issue 02. If one-fixed-step uncertainty was approved, make the sampled bound explicit and ensure the first Missing observation accounts for the complete step. If strict sub-radius precision was approved, add bounded transition-local segment sampling or the approved equivalent behind the observation boundary. In both cases, fail closed when evidence or sampling capacity cannot satisfy the approved guarantee and demonstrate the result through the real physics pipeline at 5, 20, and 40 metres per second.

## Acceptance criteria

- [ ] The implementation and diagnostics identify the precision mode approved in issue 02.
- [ ] A 40-metres-per-second walk-off cannot receive attachment retention beyond the approved world-distance bound.
- [ ] The measured bound separates fixed-step observation uncertainty from configured attachment-continuity distance.
- [ ] Under sampled precision, the first Missing step includes its full displacement and documentation states that maximum edge uncertainty is one fixed-step travel interval.
- [ ] Under sampled precision, no test, diagnostic, or documentation claims strict sub-radius edge detection.
- [ ] Under strict precision, transition-local sampling or the approved equivalent bounds unsupported-distance error to the approved fraction of support radius at 40 metres per second.
- [ ] Under strict precision, sampling work has an explicit deterministic cap and reuses buffers without managed allocation.
- [ ] Sampling-budget exhaustion, invalid evidence, or uncovered travel fails closed instead of extending attachment.
- [ ] Walk-off, ramp departure, seam, gap, and greater-than-60-degree transition remain distinguishable at all three speeds.
- [ ] The precision mechanism stays behind the observation boundary and does not leak Unity query details into attachment, movement, or airtime policy.
- [ ] The change does not become a general continuous-collision, custom-solver, moving-platform, or course-geometry project.
- [ ] Baseline traces from issue 01 are retained as before evidence and the new traces report the bounded result.
- [ ] Finding severity is re-evaluated from deterministic player-visible evidence without conflating architectural validity with release priority.

## Verification

- EditMode tests: Verify step-displacement accounting, precision-mode configuration, deterministic query budgeting, fail-closed exhaustion, and unsupported-distance calculations.
- PlayMode tests: Run edge, ramp, seam, gap, and sharp-transition fixtures at 5, 20, and 40 metres per second at 0.02 seconds, plus focused 0.01-second variants.
- Static checks: Confirm bounded query work, buffer reuse, no per-tick allocation, no hidden physics-rate change, and no unsupported precision claim.
- Manual Unity smoke check: Compare baseline and corrected high-speed edge and ramp departures with diagnostics showing observation and attachment distance.
- Package version/changelog: Not required; project gameplay behavior only.

## Blocked by

- [02 — Approve Attachment, Precision, Airtime, and Authoring Semantics](02-approve-attachment-precision-airtime-and-authoring-semantics.md)
- [05 — Bound Missing-Support Attachment by Time and Body Radius](05-bound-missing-support-attachment-by-time-and-body-radius.md)
