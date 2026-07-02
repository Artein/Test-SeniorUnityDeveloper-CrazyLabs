## Parent

[Run Steering Control PRD](../../prd/prd-run-steering-control.md)

## What to build

Add a small plain C# Run Steering Gesture collaborator that owns the active pointer identity, Run Steering Origin, physical range conversion, deadzone handling, clamped output, release/cancel behavior, and reset behavior.

The gesture should accept generic pointer events and return a requested steering value. It should not know about gameplay states, Launch Target movement, upgrades, scene objects, or Unity lifecycle callbacks.

This slice should make the invisible floating Run Steering Control's input math correct and independently testable before it is wired into Running.

## Acceptance criteria

- [ ] A pointer press begins a gesture and captures the Run Steering Origin.
- [ ] The first accepted gesture captures its pixel range from the current validated DPI and authored centimeter range.
- [ ] The active gesture keeps its captured pixel range until release, cancellation, or reset.
- [ ] Later metric changes affect only the next gesture.
- [ ] Horizontal movement right from origin produces positive requested steering.
- [ ] Horizontal movement left from origin produces negative requested steering.
- [ ] Vertical-only movement produces neutral requested steering.
- [ ] Movement inside Run Steering Deadzone produces neutral requested steering.
- [ ] Movement outside deadzone remaps continuously toward full steering.
- [ ] Movement at Run Steering Range produces full steering.
- [ ] Movement beyond Run Steering Range stays clamped to full steering.
- [ ] A second pointer press while active does not steal control.
- [ ] Movement from a non-active pointer has no effect.
- [ ] Release from a non-active pointer has no effect.
- [ ] Release from the active pointer clears output and ends the gesture.
- [ ] Cancellation from the active pointer clears output and ends the gesture.
- [ ] Reset clears active pointer, origin, captured range, and output.
- [ ] The gesture does not create a separate mapper class unless reuse or complexity appears during implementation.

## Verification

- EditMode tests:
  - Press captures origin and returns neutral output.
  - Right movement maps positive; left movement maps negative.
  - Vertical movement is ignored.
  - Deadzone output is neutral.
  - Post-deadzone values remap smoothly and symmetrically.
  - Range and beyond-range values clamp to full steering.
  - Extra touches do not steal control.
  - Active release and cancellation clear output.
  - Non-active release, cancellation, and movement are ignored.
  - Reset clears all active gesture state.
  - Raw DPI validation cases from the config slice drive the captured pixel range.
  - Edge-origin gestures keep strict physical range behavior.
- PlayMode tests:
  - Not expected for this slice.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
  - Source search confirms the gesture does not depend on scene objects, gameplay state, input backend types, or upgrade storage.
- Manual Unity smoke check:
  - Not expected for this slice.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Run Steering Config And Screen Metrics
