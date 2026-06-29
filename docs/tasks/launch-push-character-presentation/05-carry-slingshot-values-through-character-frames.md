## Parent

[Launch Push Character Presentation PRD](../../prd/prd-launch-push-character-presentation.md)

## What to build

Carry slingshot presentation facts through the existing Character presentation presenter, frame, and passive view. The presenter should read the slingshot presentation context source each rendered tick, forward only mode-selection facts into classification input, and copy normalized pull and launch values into the Character presentation frame.

The frame should carry normalized pull strength, normalized launch power, normalized pull offset, and normalized launch offset. The view should remain shallow: it writes the frame values to serialized Animator float parameters and does not subscribe to slingshot events, inspect gameplay state, read pointer input, or interpret launch requests.

This slice should wire the new runtime dependencies through DI and add tests proving the facts reach the classifier and Animator-facing frame/view boundary. It should not author Animator state transitions yet.

## Acceptance criteria

- [ ] Character presentation frame carries normalized pull strength, normalized launch power, normalized pull offset, and normalized launch offset.
- [ ] Character presentation presenter reads the slingshot presentation context source current value each presentation tick.
- [ ] Presenter forwards active-pull presence, launch-push presence, and launch-push elapsed seconds into the classification input.
- [ ] Presenter copies normalized pull and launch values into the outgoing Character presentation frame.
- [ ] PullAnticipation frames carry live pull values and zero launch values.
- [ ] LaunchPush frames carry frozen launch values and zero pull values.
- [ ] Idle, Slide, Run, Airborne, Victory, and Defeat frames zero all slingshot-specific float values.
- [ ] Character presentation view exposes serialized Animator float parameter fields for all new normalized values.
- [ ] Animator parameter fields use dropdown attributes where the existing inspector tooling supports them.
- [ ] Character presentation view writes Animator parameters only and continues to disable root motion.
- [ ] DI registration resolves the presenter with the slingshot presentation context source through interfaces.
- [ ] No Animator Controller transitions, authored Ladybug states, scene geometry, collider, camera, launch physics, band recoil, package, or save-format changes are introduced in this slice.

## Verification

- EditMode tests:
  - Presenter tests prove active-pull, launch-push, and elapsed facts are forwarded into classification input.
  - Presenter tests prove normalized pull and launch values are copied into frames.
  - Presenter tests prove inactive slingshot channels are zeroed for pull, push, locomotion, terminal, and idle modes.
  - View tests prove the new Animator float parameters are written from the frame without interpreting gameplay state.
- PlayMode tests:
  - Composition test proves the container resolves the Character presenter and slingshot presentation context source through their expected interfaces.
  - Prefab or scene composition test verifies the view has the new serialized Animator parameter fields available for authoring.
- Static checks:
  - Rider reformat and file-problem checks for changed C# and asmdef files.
  - Unity compile through the connector before tests.
  - `git diff --check`.
- Manual Unity smoke check:
  - Inspect the Character view in the editor and confirm new Animator parameter fields are visible and constrained by dropdowns where available.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 03 Add Slingshot Presentation Context Source
- 04 Add Pull And Push Character Mode Classification
