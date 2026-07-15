# 03 — Publish Unambiguous Support Proximity Evidence

## Parent

[Run Locomotion Attachment and Steering Support Separation PRD](../../prd/prd-run-locomotion-attachment-and-steering-support.md) — user stories 37–42, 80, 87, 102, and 118.

## What to build

Publish one current-tick Observed Support contract whose geometric evidence can safely drive the approved attachment policy. Carry Supported, Missing, and Unavailable states through the real physics probe and atomic surface snapshot. For Supported observations, expose distinct separation and penetration meanings, a usable support anchor and normal, sample position, and the approved continuity identity. Preserve current consumer behavior while making the new evidence visible in deterministic tests and diagnostics.

## Acceptance criteria

- [ ] Observed Support has explicit Supported, Missing, and Unavailable states with no cross-tick history.
- [ ] Supported observations distinguish separated proximity from overlap penetration without an overloaded distance meaning.
- [ ] Separated observations expose finite non-negative separation; overlapping observations expose finite non-negative penetration depth.
- [ ] Supported observations expose a finite normalized normal, finite support anchor, and body sample position.
- [ ] The approved continuity identity or geometric-coherence evidence is represented without leaking Unity collider dependencies into pure policy.
- [ ] Missing means a valid active probe found no acceptable support; Unavailable means the runtime context cannot produce trustworthy evidence.
- [ ] The surface pipeline publishes the enriched observation atomically with existing compatibility output.
- [ ] Existing movement, steering, and airtime consumers retain their characterized behavior in this slice.
- [ ] Diagnostics label overlap, separation, penetration, support anchor, and observation state accurately.
- [ ] Probe and snapshot evaluation remain allocation-free after warm-up.
- [ ] Invalid or non-finite physics results are rejected deterministically and cannot masquerade as Supported.
- [ ] Existing scene references, probe authoring, packages, asmdefs, and ProjectSettings remain unchanged unless issue 02 explicitly approved otherwise.

## Verification

- EditMode tests: Validate observation states, proximity invariants, finite evidence, adapter mapping, snapshot atomicity, and invalid-result rejection.
- PlayMode tests: Exercise cast support, overlap support, Missing, Unavailable, seams, and candidate selection through the real Unity probe.
- Static checks: Confirm temporal state remains outside the probe, Unity collider types do not enter pure policy contracts, and no per-tick allocations or duplicate distance semantics remain.
- Manual Unity smoke check: Inspect diagnostics on continuous support, overlap, a seam, and a walk-off; confirm displayed evidence matches visible geometry.
- Package version/changelog: Not required; internal gameplay contract only.

## Blocked by

- [02 — Approve Attachment, Precision, Airtime, and Authoring Semantics](02-approve-attachment-precision-airtime-and-authoring-semantics.md)
