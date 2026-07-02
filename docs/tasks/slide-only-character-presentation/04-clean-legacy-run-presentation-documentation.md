## Parent

[Slide-Only Character Presentation PRD](../../prd/prd-slide-only-character-presentation.md)

## What to build

Clean local documentation so future work uses slide-only **Character Presentation** vocabulary consistently.

This slice should update or annotate stale docs that still describe flat forward grounded movement as normal **Run** presentation. The earlier **Ladybug Character Presentation** direction is only superseded in that specific area; the architecture remains valid: the Rigidbody-backed **Launch Target** is gameplay truth, and the **Character** is presentation only.

Documentation should keep gameplay **Run**, **Running**, **Run Surface**, **Run Progress**, and **Run Result** terminology intact. Only presentation-level visible running changes.

## Acceptance criteria

- [ ] Character Presentation context documentation uses **Slide**, **Coast**, **Meaningful Grounded Movement**, **Meaningful Grounded Movement Threshold**, **Slide Reference Speed**, and **Reserved Presentation Mode** consistently.
- [ ] Docs that previously described flat grounded locomotion as normal visible **Run** are updated or explicitly marked superseded by the slide-only PRD.
- [ ] Existing task docs that refer to old Slide/Run handoff behavior are updated where they would mislead future implementation.
- [ ] Gameplay **Run** language is not renamed when it refers to gameplay state, run flow, progress, rewards, or results.
- [ ] **Run Surface Slope Calculator** documentation describes slope as diagnostics or future **Slide Flavor**, not canonical mode selection.
- [ ] Out-of-scope items remain clear: no **Coast** mode, no new animation asset, no enum deletion, no physics architecture change, no package change.
- [ ] Documentation does not claim that the **Run** enum or Animator state has been deleted.
- [ ] The local task index points back to the slide-only PRD and notes the narrower supersession of earlier Ladybug presentation wording.

## Verification

- EditMode tests:
  - None expected; this is a documentation cleanup slice.
- PlayMode tests:
  - None expected; this is a documentation cleanup slice.
- Static checks:
  - Search docs for stale presentation statements that describe flat forward grounded movement as normal **Run**.
  - Search docs for old tuning names and confirm any remaining references are in historical, compatibility, or explicitly superseded context.
  - `git diff --check`.
- Manual Unity smoke check:
  - None expected.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Replace Grounded Mode Selection With Meaningful Slide
- 02 - Normalize Reserved Run And Slide Playback
- 03 - Update Ladybug Authoring And Scene Composition
