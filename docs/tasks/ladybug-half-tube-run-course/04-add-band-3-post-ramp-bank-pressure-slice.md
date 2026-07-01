## Parent

[Ladybug Half-Tube Run Course PRD](../../prd/prd-ladybug-half-tube-run-course.md)

## What to build

Extend Band 3 after the required ramp, covering 200-250 meters with landing recovery, simple staggered low blockers, and first bank-line pressure. This slice should convert the tutorial ramp into a complete mid-run beat: recover from landing, read a familiar low static obstacle pattern, then choose between a viable center fallback and richer bank-side coins.

The completed slice should be playable through the first post-ramp skill escalation without adding new mechanics beyond faster reads, bank pressure, and coin temptation.

## Acceptance criteria

- [ ] The **Run Course** is continuous from Launch through 250m.
- [ ] The 200-225m section provides landing recovery before adding obstacle pressure.
- [ ] Post-ramp blockers are simple, static, low, and readable with clear recovery windows.
- [ ] The 225-250m section introduces bank-line pressure while keeping the center fallback viable.
- [ ] Bank-side reward coins are richer than the center line but do not block basic completion.
- [ ] Static blockers use explicit **Run Obstacle** authoring through project-owned wrappers/colliders.
- [ ] The center trough and bank transitions remain recoverable after ramp landing.
- [ ] No new ramp or new obstacle mechanic is introduced in the same section as the first post-ramp recovery.
- [ ] **Run Safety Net** coverage continues under Band 3, side lips, and post-ramp escape areas.
- [ ] The **Run Camera** keeps the landing recovery and first post-ramp reads visible.

## Verification

- EditMode tests:
  - Pure validation tests for section order, obstacle category expectations, and optional risk-line metadata, if represented in a validation module.
- PlayMode tests:
  - Gameplay Scene composition verifies Band 3 post-ramp obstacles are **Run Obstacle** and traversal/landing surfaces are **Run Surface**.
  - Representative Band 3 slope samples cover landing recovery, center trough, and bank-line areas.
  - **Run Safety Net** coverage remains present under post-ramp escape areas.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector after any code, scene, prefab, or test change.
- Manual Unity smoke check:
  - Confirm the landing recovery feels fair, first post-ramp blockers are readable, bank coins are tempting but optional, and center fallback stays viable.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 03 - Add Required Center Tutorial Ramp Slice
