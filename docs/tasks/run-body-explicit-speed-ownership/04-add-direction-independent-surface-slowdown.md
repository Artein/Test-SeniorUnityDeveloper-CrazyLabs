# Add Direction-Independent Surface Slowdown

Type: AFK

## Parent

[PRD: Run Body Explicit Speed Ownership](../../prd/prd-run-body-explicit-speed-ownership.md)

## What to build

Add a globally authored ordinary surface slowdown to the **Run Body Speed Model** so valid grounded traversal loses tangent speed predictably without relying only on physics-material friction.

The pure speed evaluator returns the authored tangent drag rate whenever valid grounded **Run Surface** support allows speed policy. Unlike downhill acceleration, slowdown does not depend on **Course Forward Alignment**: forward, diagonal, lateral, and reversed travel all lose speed consistently. Movement orchestration integrates downhill acceleration and ordinary drag in the agreed order, never reverses tangent direction through drag, preserves corrected surface-normal velocity, and performs one final target write.

This is one default gameplay slowdown, not a **Run Surface Speed Profile** system. Physics materials continue to participate in contact solving, so tuning and tests must make the authored model contribution independently observable.

## Acceptance criteria

- [ ] The movement-tuning asset exposes one readable global ordinary surface slowdown control.
- [ ] Valid grounded tangent motion receives the configured slowdown regardless of course-forward alignment.
- [ ] Forward, diagonal, lateral, and reversed motion at equal tangent speed receives equal authored slowdown.
- [ ] Missing, invalid, ungrounded, stale, or meaningfully departing support receives no authored slowdown.
- [ ] Ordinary slowdown never invents a heading, reverses tangent direction, or changes corrected surface-normal velocity.
- [ ] Downhill acceleration and ordinary slowdown combine deterministically in the documented integration order.
- [ ] The surface-slowdown contributor appears only when authored slowdown is active.
- [ ] Authored slowdown remains separately testable from physics-material friction and contact-solver loss.
- [ ] Unsupported and airborne motion remains physically driven.
- [ ] No surface profile, terrain layer, visual material, physics-material identity, or course-section override selects gameplay tuning.

## Verification

- EditMode tests:
  - Equal-speed forward, diagonal, lateral, and reversed contexts return equal ordinary slowdown.
  - Unsupported and departing-support contexts return no slowdown.
  - Slowdown approaches zero speed without crossing through zero or reversing direction.
  - Downhill acceleration and slowdown integrate in the agreed order.
  - Contributor flags identify slowdown without changing output.
  - Movement integration preserves corrected normal velocity and writes once.
- PlayMode tests:
  - A valid grounded flat section loses tangent speed predictably with model slowdown enabled.
  - Equivalent authored settings produce an observable model contribution independent from contact friction.
  - Unsupported flight does not receive model slowdown.
- Static checks:
  - Rider reformat and problem inspection for changed code and tests.
  - Unity connector compile before tests.
  - Confirm no surface-profile or material-to-gameplay mapping was added.
- Manual Unity smoke check:
  - Compare flat traversal in forward, diagonal, and reversed directions and inspect the slowdown contribution separately from visible contact effects.
- Package version/changelog:
  - Not required.

## Blocked by

- [03 - Add Course-Aligned Downhill Acceleration](03-add-course-aligned-downhill-acceleration.md)
