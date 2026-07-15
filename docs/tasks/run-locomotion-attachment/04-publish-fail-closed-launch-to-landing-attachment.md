# 04 — Publish Fail-Closed Launch-to-Landing Attachment

## Parent

[Run Locomotion Attachment and Steering Support Separation PRD](../../prd/prd-run-locomotion-attachment-and-steering-support.md) — user stories 13–15, 24–25, 43–57, 80–83, 91–95, 102–104, 116, and 119.

## What to build

Publish Run Locomotion Attachment as an explicit lifecycle-aware state and snapshot from launch through departure and landing. Begin every launched run Detached, acquire Attached only from approved current support evidence, update the physical support frame immediately from current evidence, and fail closed on lift, separation, invalid context, non-finite data, or teleport. Expose the result through a narrow source, atomic surface snapshot, composition, diagnostics, and deterministic tests while existing physical consumers remain on compatibility behavior until issue 07.

## Acceptance criteria

- [ ] Attachment has explicit Inactive, Detached, and Attached states.
- [ ] Pre-run state is Inactive and beginning a launched run enters Detached without first publishing Attached or Unknown authority.
- [ ] Attached is acquired only from current Supported evidence satisfying the approved proximity and relative-lift conditions.
- [ ] Reattachment uses the approved stricter acquisition conditions without adding an unapproved landing delay.
- [ ] Positive relative lift above the approved threshold detaches immediately.
- [ ] Current separation beyond the approved threshold detaches immediately.
- [ ] Unavailable, non-finite, invalid, run-end, or teleport evidence fails closed and clears temporal state.
- [ ] A valid current support normal replaces the physical support frame immediately, including changes greater than 60 degrees.
- [ ] Steering discontinuity confirmation cannot delay or replace the physical support frame.
- [ ] Current state, transition reason, evidence kind, physical support frame, separation, and relative lift are published atomically.
- [ ] State transitions publish exactly once and stable states do not repeat acquisition or detachment transitions.
- [ ] One shared VContainer instance serves fixed-step update, lifecycle, snapshot, and narrow attachment-read interfaces.
- [ ] Launch, surface-frame update, movement, and run-end ordering are explicit and protected by composition tests.
- [ ] Until issue 05 adds approved continuity, a Missing observation fails closed rather than inheriting unbounded grace.
- [ ] Existing movement and airtime behavior remains compatibility-driven in this slice, and diagnostics make that temporary consumer distinction explicit.

## Verification

- EditMode tests: Cover lifecycle, acquisition, reattachment, lift detachment, separation detachment, unavailable reset, non-finite rejection, teleport invalidation, greater-than-60-degree physical-normal replacement, and transition uniqueness.
- PlayMode tests: Drive launch, first landing, walk-off, upward ramp departure, sharp transition, teleport/reset, and run-end through the integrated pipeline.
- Static checks: Confirm no State-pattern hierarchy, static state, service lookup, duplicate attachment instance, consumer-local reconstruction, or per-tick allocation is introduced.
- Manual Unity smoke check: Launch, land, depart a ramp, walk off an edge, and inspect attachment state and physical support frame in diagnostics.
- Package version/changelog: Not required; internal runtime behavior is not yet consumed physically.

## Blocked by

- [03 — Publish Unambiguous Support Proximity Evidence](03-publish-unambiguous-support-proximity-evidence.md)
