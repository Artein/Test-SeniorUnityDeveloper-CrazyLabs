# 10 — Tune and Approve Production Edge, Ramp, and Seam Behavior

## Parent

[Run Locomotion Attachment and Steering Support Separation PRD](../../prd/prd-run-locomotion-attachment-and-steering-support.md) — user stories 1–36, 65–67, 91–122, and 128–130.

## What to build

Conduct the human-in-the-loop production acceptance pass across the canonical edge, ramp, seam, gap, sharp-transition, launch, landing, steering, and reward scenarios at 5, 20, and 40 metres per second. Use deterministic traces and the four-signal diagnostics to approve or tune only the fields authorized in issue 02. Record final serialized values, units, rationale, player-visible outcomes, precision evidence, and finding severity before compatibility is removed.

## Acceptance criteria

- [ ] The review covers continuous support, walk-off, upward ramp departure, short seam, short gap, greater-than-60-degree transition, launch, landing, and unavailable reset.
- [ ] Every applicable scenario is reviewed at 5, 20, and 40 metres per second.
- [ ] Attachment, physical support frame, Steering Support Target, Run Steering Frame, movement mode, physical airtime, and reward qualification are visible together during review.
- [ ] Walk-off and ramp departure stop grounded behavior within the approved world-distance guarantee.
- [ ] A short intentional seam receives no more continuity than the approved time and body-radius budgets.
- [ ] Sharp current support changes affect physical projection immediately while steering remains readable.
- [ ] Air steering remains usable without restoring grounded movement or suppressing physical airtime.
- [ ] Landing reacquires promptly under approved proximity and lift conditions without duplicate stabilization.
- [ ] Reward behavior matches issue 02 for seam-sized and qualifying airborne episodes.
- [ ] Final attachment and steering values are recorded with units, effective world-distance implications, and rationale.
- [ ] Production serialized asset changes are limited to the fields and migration plan approved in issue 02.
- [ ] Automated EditMode and PlayMode coverage is rerun after every accepted tuning change.
- [ ] No hidden collider, magnetic force, consumer-local filter, duplicated timer, new package, or ProjectSettings change is accepted as a feel fix.
- [ ] The reviewer either approves the matrix or records severity-ranked follow-up findings tied to specific scenarios and signals.
- [ ] Final P1/P2 severity is recorded separately from acceptance of the architectural solution.
- [ ] A human owner records explicit approval before issue 11 begins.

## Verification

- EditMode tests: Re-run all observation, attachment, steering, movement, airtime, reward, configuration, composition, and allocation tests after tuning.
- PlayMode tests: Re-run the full 5/20/40-metres-per-second geometry matrix at 0.02 seconds and focused 0.01-second quantization variants.
- Static checks: Confirm only approved serialized values changed and no architecture, package, project setting, scene topology, or compatibility scope expanded.
- Manual Unity smoke check: Required; complete and record the full human review matrix with diagnostics visible.
- Package version/changelog: Record approved player-visible tuning or reward changes if project release policy requires it; no package version bump.

## Blocked by

- [08 — Retype Filtered Support as Steering-Only and Bound Its Retention](08-retype-filtered-support-as-steering-only-and-bound-its-retention.md)
- [09 — Apply Approved Airtime Reward Semantics](09-apply-approved-airtime-reward-semantics.md)
