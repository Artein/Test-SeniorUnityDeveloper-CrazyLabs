# 11 — Run Seam, Trough, and Brief-Airborne Feel Review

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 1–22, 56–67, 71, 73–74.

## What to build

Conduct a human-in-the-loop gameplay review across the canonical seam, trough, brief-gap, true-airborne, and landing scenarios. Use diagnostics and deterministic traces to approve the player-facing result and, if needed, tune only the already approved timing and coherence fields.

## Acceptance criteria

- [ ] The reviewer traverses continuous slopes, fast seams, troughs, a brief gap, a true walk-off, a jump, and landing.
- [ ] The review covers representative low, nominal, and high run speeds.
- [ ] Brief gaps do not cause an objectionable movement, steering, presentation, or reward discontinuity.
- [ ] True support loss and reacquisition remain responsive and legible.
- [ ] Coherent seams and troughs settle without oscillation or prolonged incorrect orientation.
- [ ] Alternating noisy normals do not produce false confirmed discontinuities.
- [ ] Diagnostics and trace output agree with the visible transition in reviewed cases.
- [ ] Any tuning changes are limited to approved probe, support grace, coherence, confirmation, slew, or snap fields.
- [ ] Final approved values and rationale are recorded with units.
- [ ] No architectural workaround or consumer-local filter is accepted as a feel fix.
- [ ] A human owner explicitly approves the result or records a severity-ranked follow-up.

## Verification

- EditMode tests: Re-run all deterministic policy and consumer tests after any tuning adjustment.
- PlayMode tests: Re-run the full canonical seam, trough, gap, airborne, and landing suite after any tuning adjustment.
- Static checks: Confirm the review introduced no new temporal state, duplicate filter, package dependency, or project-setting change.
- Manual Unity smoke check: Required; complete the review matrix and record human approval or follow-up findings.
- Package version/changelog: Record final player-visible tuning changes if required by release policy.

## Blocked by

- [02 — Capture End-to-End Surface Transition Baseline](02-capture-end-to-end-surface-transition-baseline.md)
- [05 — Confirm Coherent Surface-Normal Discontinuities](05-confirm-coherent-surface-normal-discontinuities.md)
- [06 — Drive Run Steering Frame from Shared Transitions](06-drive-run-steering-frame-from-shared-transitions.md)
- [07 — Migrate Core Locomotion to Explicit Stable Support](07-migrate-core-locomotion-to-explicit-stable-support.md)
- [08 — Apply Approved Airtime Support Semantics](08-apply-approved-airtime-support-semantics.md)
- [09 — Apply Approved Character Presentation Support Semantics](09-apply-approved-character-presentation-support-semantics.md)
- [10 — Expose Observed, Stable, Transition, and Steering Diagnostics](10-expose-observed-stable-transition-and-steering-diagnostics.md)
