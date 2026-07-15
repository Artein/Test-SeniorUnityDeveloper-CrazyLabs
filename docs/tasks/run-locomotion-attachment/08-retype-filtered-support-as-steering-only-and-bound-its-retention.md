# 08 — Retype Filtered Support as Steering-Only and Bound Its Retention

## Parent

[Run Locomotion Attachment and Steering Support Separation PRD](../../prd/prd-run-locomotion-attachment-and-steering-support.md) — user stories 5–6, 17–19, 21, 26–29, 36, 53, 68–75, 80–82, 104–106, and 123–127.

## What to build

Replace the physical-sounding Stable Support concept with Steering Support Target across the surface snapshot, steering policy, configuration, diagnostics, and tests. Make the type incapable of authorizing grounded behavior, preserve approved spike rejection and coherent discontinuity handling, and apply the approved time and spatial retention limits. Allow Run Steering Frame to remain usable while attachment is Detached without feeding control-frame validity back into physical state.

## Acceptance criteria

- [ ] Steering Support Target replaces Stable Support as the filtered orientation concept in active runtime contracts.
- [ ] Steering Support Target exposes no Is Grounded, Allows Grounded Locomotion, physical-attachment state, or equivalent authority.
- [ ] Compiler-visible type separation prevents Steering Support Target from being passed to physical movement or airtime APIs.
- [ ] Current physical support updates immediately and independently while steering may retain or confirm an older orientation.
- [ ] A one-tick discontinuous spike can be rejected without preserving the old normal for physical projection.
- [ ] Coherent discontinuities confirm using the approved time, angle, and coherence rules.
- [ ] Missing-observation and airborne steering retention use the time and spatial semantics approved in issue 02.
- [ ] Retention expires when either approved bound is reached and cannot remain tied to departed geometry for unbounded world distance.
- [ ] Run Steering Frame may remain finite and valid while attachment is Detached for approved air steering and presentation behavior.
- [ ] Steering-frame validity cannot acquire, retain, or restore Run Locomotion Attachment.
- [ ] Existing 0.12-second and 0.6-second serialized values survive only as approved steering semantics and are never reused as physical attachment values.
- [ ] Configuration validation rejects invalid steering angles, durations, coherence, and spatial limits.
- [ ] Snapshot and diagnostics show Observed Support, Run Locomotion Attachment, Steering Support Target, and Run Steering Frame as four distinct concepts.
- [ ] Temporary compatibility names, if issue 02 requires them, are shallow adapters with a removal condition owned by issue 11 and contain no filtering state.
- [ ] The steering path remains allocation-free and has one update owner per fixed step.

## Verification

- EditMode tests: Cover missing retention, time and spatial expiry, one-spike rejection, coherent confirmation, incoherent replacement, slew, snap, airborne frame memory, reset, and one-way attachment independence.
- PlayMode tests: Exercise seam, gap, walk-off, sharp transition, airborne steering, and landing at 5, 20, and 40 metres per second.
- Static checks: Search for physical consumers of Steering Support Target, grounded members on the type, duplicate filters, mutable config state, legacy semantic labels, and per-tick allocation.
- Manual Unity smoke check: Compare physical support, attachment, steering target, and steering frame diagnostics through seam, sharp transition, walk-off, and air steering.
- Package version/changelog: Not required; internal project gameplay contract and approved feel behavior only.

## Blocked by

- [07 — Migrate Grounded Movement and Physical Airtime Atomically](07-migrate-grounded-movement-and-physical-airtime-atomically.md)
