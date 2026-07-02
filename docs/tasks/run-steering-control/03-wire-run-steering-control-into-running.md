## Parent

[Run Steering Control PRD](../../prd/prd-run-steering-control.md)

## What to build

Replace the current screen-center Running steering input path with the Run Steering Gesture while preserving the existing gameplay boundary: Run Steering Control is active only during Running, Pre-Launch Pull is untouched, and Launch Target movement remains orchestrated by the player steering controller.

The controller should subscribe to the existing generic pointer stream, start the Run Steering Gesture only while Running, feed accepted pointer movement into the gesture, and clear requested steering on release, cancellation, or Running exit.

This slice should make the new control usable end-to-end in gameplay without changing responsiveness or final movement tuning yet.

## Acceptance criteria

- [ ] Pointer input before Running does not start Run Steering Control.
- [ ] The first pointer press during Running starts Run Steering Control from that touch position.
- [ ] The Run Steering Origin is the touch position, not screen center.
- [ ] Pointer movement during Running updates requested steering through Run Steering Gesture.
- [ ] Extra touches during an active gesture are ignored.
- [ ] Active pointer release clears requested steering.
- [ ] Active pointer cancellation clears requested steering.
- [ ] Leaving Running resets the active gesture and current/desired steering state.
- [ ] Pre-Launch Pull input behavior is unchanged.
- [ ] The controller stays responsible for gameplay state gating, input subscription, smoothing handoff, and Launch Target movement.
- [ ] Run Steering Gesture stays responsible for pointer lifecycle and mapping.
- [ ] No visible joystick UI, canvas, prefab, or scene visual is added.

## Verification

- EditMode tests:
  - Pointer press before Running is ignored.
  - Pointer press during Running starts a gesture and produces neutral requested steering.
  - Pointer movement during Running updates requested steering from the gesture output.
  - Extra touches do not change active control.
  - Release clears requested steering.
  - Cancellation clears requested steering.
  - Running exit resets gesture state and steering state.
  - Pre-Launch input behavior remains covered by existing or updated tests.
- PlayMode tests:
  - Add only if a runtime state transition or scene-composition behavior cannot be covered in EditMode.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
  - Source search confirms steering does not use screen-center input mapping as the active Running path.
  - Source search confirms gameplay steering still consumes generic pointer input rather than mobile touch APIs directly.
- Manual Unity smoke check:
  - In Editor, start a run, press away from screen center, and confirm the touch origin behaves neutral until moved horizontally.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 - Add DPI-Aware Run Steering Gesture
