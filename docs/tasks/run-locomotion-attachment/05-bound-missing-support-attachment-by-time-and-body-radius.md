# 05 — Bound Missing-Support Attachment by Time and Body Radius

## Parent

[Run Locomotion Attachment and Steering Support Separation PRD](../../prd/prd-run-locomotion-attachment-and-steering-support.md) — user stories 1, 4, 15–16, 22–23, 31–32, 48, 52, 58–65, 96–103, 112, 115, and 117–118.

## What to build

Add the approved Bounded Continuity Bridge to Run Locomotion Attachment so a brief sensor miss can preserve attachment only within simultaneous temporal, body-relative travel, predicted-separation, lift, lifecycle, and validity limits. Carry authored configuration through validation and composition, publish the active evidence kind and remaining budgets, and verify the bridge at low and high speed without moving physical consumers yet.

## Acceptance criteria

- [ ] Only a previously Attached state backed by current support may enter Bounded Continuity Bridge evidence.
- [ ] Retention requires every approved condition simultaneously; failure of any one condition detaches.
- [ ] The first Missing observation includes body travel since the previous Supported sample.
- [ ] Travel uses cumulative fixed-step path length rather than net displacement.
- [ ] The spatial limit is authored as the approved fraction of support radius and applies any approved absolute ceiling.
- [ ] Elapsed continuity time uses fixed-step seconds and expires at greater-than-or-equal to the approved limit.
- [ ] Travel continuity expires at greater-than-or-equal to the approved spatial limit.
- [ ] Zero time or zero distance disables the corresponding continuity allowance immediately.
- [ ] Predicted separation from the last physical support plane and relative lift remain within approved limits throughout the bridge.
- [ ] Unavailable, invalid, non-finite, teleport, run-end, or exhausted evidence bypasses grace and detaches immediately.
- [ ] Reacquiring valid current support clears time and path accumulation and publishes current evidence once.
- [ ] Attachment snapshot and diagnostics expose current or bridged evidence, elapsed and remaining time, accumulated and remaining distance, and the limiting reason on detachment.
- [ ] Configuration validation rejects negative, non-finite, inverted, or meaningless radius and absolute limits.
- [ ] Serialized fields follow issue 02's preserve, rename, replace, and retire decisions without silently reinterpreting the 0.12-second or 0.6-second values.
- [ ] Policy evaluation and diagnostic publication remain allocation-free after warm-up.

## Verification

- EditMode tests: Cover below/at/above time and distance limits, first-Missing travel, cumulative curved and reversing paths, predicted separation, lift, zero limits, reset causes, reacquisition, configuration validation, and 0.01/0.02-second quantization.
- PlayMode tests: Exercise a short seam, short gap, sustained walk-off, ramp departure, landing, and teleport at 5, 20, and 40 metres per second where applicable.
- Static checks: Confirm no consumer-local attachment grace, net-displacement shortcut, hidden default, duplicate timer, or runtime state in ScriptableObjects remains.
- Manual Unity smoke check: Cross the canonical seam and walk off the canonical edge at low and high speed while inspecting attachment evidence and budgets.
- Package version/changelog: Not required; internal policy and project-level serialized authoring only.

## Blocked by

- [04 — Publish Fail-Closed Launch-to-Landing Attachment](04-publish-fail-closed-launch-to-landing-attachment.md)
