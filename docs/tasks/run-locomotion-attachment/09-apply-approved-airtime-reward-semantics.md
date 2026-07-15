# 09 — Apply Approved Airtime Reward Semantics

## Parent

[Run Locomotion Attachment and Steering Support Separation PRD](../../prd/prd-run-locomotion-attachment-and-steering-support.md) — user stories 10–12, 33, 77–79, and 109–111.

## What to build

Apply the physical-airtime and reward-qualification decision from issue 02 through the existing airtime-to-reward path. If every physical detachment is reward-eligible, keep the path direct and prove that behavior. If tiny physical episodes are excluded, add a downstream qualification policy that treats an episode as provisional, discards it only when it remains below the approved limits, and commits its full duration from the original detachment once qualified. In both cases, reward semantics must never change attachment, movement, or physical airtime truth.

## Acceptance criteria

- [ ] Physical airtime remains driven only by Run Locomotion Attachment and is unchanged by reward qualification.
- [ ] The issue 02 reward decision is represented explicitly in runtime composition and documentation.
- [ ] When all physical airtime is reward-eligible, the existing reward consumer receives the complete physical episode without an unnecessary second state machine.
- [ ] When qualification is required, a new physical episode starts provisional at its original detachment.
- [ ] An unqualified episode that ends below the approved time and travel limits contributes no reward airtime.
- [ ] A qualifying episode commits its complete duration from the original detachment rather than only time after the threshold.
- [ ] A qualifying episode remains committed if later state changes before landing.
- [ ] Reattachment closes the current episode exactly once and cannot leak provisional state into the next episode.
- [ ] Run end, unavailable reset, and new launch have explicit approved handling and clear transient qualification state.
- [ ] Steering Support Target and Run Steering Frame cannot start, stop, suppress, or qualify airtime.
- [ ] Reward qualification has no dependency back into movement, attachment, probe, or steering policy.
- [ ] Existing run-end reward presentation and economy behavior remain unchanged outside the approved qualification result.
- [ ] Runtime evaluation remains deterministic and allocation-free.

## Verification

- EditMode tests: Cover direct eligibility or provisional qualification, below/at/above thresholds, full-duration commitment, reattachment, repeated episodes, run end, reset, launch, and dependency direction.
- PlayMode tests: Run seam, short gap, true jump, walk-off, landing, and run-end reward scenarios at representative low and high speeds.
- Static checks: Confirm no reward dependency enters attachment or movement, no duplicate physical airtime tracker exists, and no retained steering signal influences reward qualification.
- Manual Unity smoke check: Compare a seam-sized detachment and a qualifying jump with diagnostics and the existing reward result visible.
- Package version/changelog: Record a project gameplay behavior note only if reward eligibility changes require one; no package version change.

## Blocked by

- [02 — Approve Attachment, Precision, Airtime, and Authoring Semantics](02-approve-attachment-precision-airtime-and-authoring-semantics.md)
- [07 — Migrate Grounded Movement and Physical Airtime Atomically](07-migrate-grounded-movement-and-physical-airtime-atomically.md)
