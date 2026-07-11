# 09 — Support hairpins, near-overlaps, teleports, and explicit reacquisition

**Type:** AFK
**User stories covered:** 10–11, 23–25, 43–45, 49, 67–69, 74–78, 84

## Parent

[Continuous Run Course Progress PRD](../../prd/prd-continuous-run-course-progress.md)

## What to build

Extend the non-linear tracer bullet to geometry where distant route sections are spatially close. Keep the active location stable through hairpins and near-overlaps by using the previous opaque token, plausible-displacement bounds, and deterministic candidate selection.

When ordinary continuity cannot produce a valid projection, publish an explicit failure state. Global reacquisition is permitted only through the approved teleport, respawn, or recovery event and must be observable through diagnostics. Complete the slice with ambiguous-route authoring fixtures and end-to-end tests through progress consumers.

## Acceptance criteria

- [ ] Hairpins and near-overlaps do not jump progress to a distant route section during plausible continuous motion.
- [ ] Search bounds are derived deterministically from previous location and plausible Run Body displacement.
- [ ] Ambiguous candidates are resolved by documented continuity rules or return an explicit invalid/ambiguous state.
- [ ] Ordinary fixed-tick sampling cannot initiate global nearest-point reacquisition.
- [ ] Teleport, respawn, or recovery can request global reacquisition only through the approved explicit event and policy.
- [ ] Reacquisition updates the location token and progress state according to the approved maximum-progress and rollback semantics.
- [ ] Projection distance, continuity validity, failure reason, and reacquisition are observable in diagnostics.
- [ ] Invalid or ambiguous samples cannot cause Run Result, rewards, Lost Momentum, support, or presentation to invent progress or motion.
- [ ] Existing straight and simple curved routes retain their prior behavior and performance.

## Verification

- EditMode tests: Hairpin continuity; overlapping segments; deterministic bounded search; excessive displacement failure; explicit teleport/reacquisition; invalid-state propagation; maximum-progress behavior after reacquisition.
- PlayMode tests: Traverse an ambiguous fixture without projection jumps; perform approved respawn/teleport and verify predictable reacquisition through all consumers.
- Static checks: Rider problems clean; Unity compile gate clean before tests; no ordinary-path global-nearest fallback; sampling remains allocation-free and bounded.
- Manual Unity smoke check: Traverse hairpin and near-overlap fixtures in both directions, then exercise explicit respawn/teleport diagnostics.
- Package version/changelog: No package change unless required by the approved adapter; document the expanded geometry and recovery support.

## Blocked by

- 08 — Ship one continuous curved and vertical Run Course end-to-end
