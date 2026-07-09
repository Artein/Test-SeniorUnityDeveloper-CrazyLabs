# Affordance Snapshot And Optional View Contract

Type: AFK

## Parent

`docs/prd/prd-run-steering-affordance.md`

## What to build

Expose the read-only **Run Steering Affordance** state needed by presentation while preserving existing **Run Steering Control** behavior.

This slice should connect the existing Run Steering gesture lifecycle to a narrow affordance view contract that can be exercised with a fake view. The controller should be able to report show, update, hide, and reset presentation events for an active steering gesture without requiring an authored scene view to exist. Missing presentation must not block steering.

No real UI hierarchy is required in this slice. The deliverable is the tested production seam that later serialized UI can consume.

## Acceptance criteria

- [ ] A read-only affordance snapshot exposes active state, Run Steering Origin, current pointer position or horizontal displacement, captured Run Steering Range in pixels, and captured Run Steering Deadzone fraction.
- [ ] Presentation code cannot mutate Run Steering gesture state through the snapshot.
- [ ] Starting an accepted Running steering gesture produces an affordance show/update signal.
- [ ] Moving the active pointer produces an affordance update signal.
- [ ] Releasing or canceling the active pointer produces an affordance hide/reset signal.
- [ ] Leaving Running or disposing the steering controller hides/resets presentation.
- [ ] A missing affordance view, optional view, or null-object view does not throw and does not prevent steering.
- [ ] Requested steering values, release/cancel behavior, responsiveness smoothing, and non-active pointer behavior remain unchanged.
- [ ] No presentation code polls Unity touch APIs or owns input.

## Verification

- EditMode tests:
  - Snapshot is inactive before a gesture begins.
  - Snapshot captures origin, current pointer position or horizontal displacement, range, and deadzone from an accepted gesture.
  - Fake view receives show/update/hide for press/move/release/cancel.
  - Fake view is not notified for ignored non-active pointer moves.
  - Missing or null-object view does not throw.
  - Existing steering output tests still pass for requested steering, deadzone, clamp, and release/cancel.
- PlayMode tests:
  - None required for this contract slice unless Unity lifecycle behavior cannot be covered in EditMode.
- Static checks:
  - Rider reformat and file problem inspection for changed C# files.
  - Unity connector compile before tests.
- Manual Unity smoke check:
  - Not required for this non-visual seam.
- Package version/changelog:
  - Not required.

## Blocked by

None - can start immediately.

