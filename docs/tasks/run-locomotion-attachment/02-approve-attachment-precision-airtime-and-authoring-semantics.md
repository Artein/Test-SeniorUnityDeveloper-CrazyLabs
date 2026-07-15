# 02 — Approve Attachment, Precision, Airtime, and Authoring Semantics

## Parent

[Run Locomotion Attachment and Steering Support Separation PRD](../../prd/prd-run-locomotion-attachment-and-steering-support.md) — user stories 21–36, 48, 65–67, 78–79, 85–86, 90, and 123–130.

## What to build

Use the baseline evidence to record the human-owned product and architecture decisions that constrain all implementation slices. Decide the physical-attachment continuity envelope, spatial-precision guarantee, reward-airtime semantics, steering-retention boundary, proximity evidence, support continuity rule, authoring and serialization plan, compatibility policy, moving-surface scope, and finding severity. This issue changes no runtime behavior and must be explicitly approved before dependent implementation begins.

## Acceptance criteria

- [ ] Physical airtime and reward-qualified airtime are defined separately, including the expected outcome for a tiny real detachment.
- [ ] The largest intentional seam or trough is recorded in metres and as a fraction of the Run Body support radius.
- [ ] The accepted edge-precision model is selected: documented one-fixed-step uncertainty or strict sub-radius transition-local sampling.
- [ ] The maximum allowed attachment uncertainty at 40 metres per second and the canonical fixed timestep is stated in world distance.
- [ ] Acquisition separation, detachment separation, relative-lift, continuity-time, and body-radius-distance fields are approved conceptually; exact feel values may remain for issue 10.
- [ ] Steering-target missing retention and discontinuity confirmation are assigned approved time and spatial semantics.
- [ ] Overlap and cast observations have an approved support-anchor and proximity interpretation.
- [ ] Cross-seam continuity is defined using approved support identity, geometric coherence, or nearby valid Run Surface evidence.
- [ ] Static Run Surfaces are approved for the first slice or relative support velocity is explicitly added to scope.
- [ ] Existing internal and public compatibility requirements are recorded, including the removal condition for Stable Support.
- [ ] Serialized fields are classified as preserved, safely renamed, replaced with new semantics, or retired; production asset edits are explicitly approved or deferred.
- [ ] The finding severity is recorded from issue 01 evidence with a stated threshold for later promotion or demotion.
- [ ] ADR-0010, one movement writer, VContainer ownership, plain-C# policy, and allocation-free fixed-step constraints are reaffirmed.
- [ ] A human owner records approval before issue 03 starts.

## Verification

- EditMode tests: Not required; this issue records decisions without code changes.
- PlayMode tests: Not required beyond reviewing issue 01 evidence.
- Static checks: Confirm every unresolved PRD question is answered, explicitly deferred with an owner, or declared out of scope; confirm terminology matches the parent PRD.
- Manual Unity smoke check: Review the issue 01 baseline traces and representative playback when needed for the severity and precision decisions.
- Package version/changelog: Not required; decision record only.

## Blocked by

- [01 — Capture Speed-Scaled Grounding Baseline](01-capture-speed-scaled-grounding-baseline.md)
