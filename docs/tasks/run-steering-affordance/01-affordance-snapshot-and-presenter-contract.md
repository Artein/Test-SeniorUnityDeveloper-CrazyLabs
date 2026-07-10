# Affordance Snapshot And Presenter Contract

Type: AFK

## Parent

`docs/prd/prd-run-steering-affordance.md`

## What to build

Expose the read-only **Run Steering Affordance** state needed by presentation while preserving existing **Run Steering Control** behavior.

This slice should connect the existing Run Steering gesture lifecycle to a narrow affordance presenter contract that can be exercised with a fake in isolated tests. The controller should report show, update, hide, and reset presentation events for an active steering gesture without presentation owning or changing steering state.

No real UI hierarchy is required in this contract-only slice. The later production Gameplay scene composition must provide the authored serialized affordance view; missing production authoring is a validation error, not a supported steering-only fallback.

## Acceptance criteria

- [ ] A read-only affordance snapshot exposes active state, Run Steering Origin, current pointer position or horizontal displacement, captured Run Steering Range in pixels, and captured Run Steering Deadzone fraction.
- [ ] Presentation code cannot mutate Run Steering gesture state through the snapshot.
- [ ] Starting an accepted Running steering gesture produces an affordance show/update signal.
- [ ] Moving the active pointer produces an affordance update signal.
- [ ] Releasing or canceling the active pointer produces an affordance hide/reset signal.
- [ ] Leaving Running or disposing the steering controller hides/resets presentation.
- [ ] Isolated controller tests can inject a fake affordance presenter without authored Unity UI.
- [ ] Production GameplayLifetimeScope composition requires the serialized affordance view and reports missing authoring through validation.
- [ ] Requested steering values, release/cancel behavior, responsiveness smoothing, and non-active pointer behavior remain unchanged.
- [ ] No presentation code polls Unity touch APIs or owns input.

## Verification

- EditMode tests:
  - Snapshot is inactive before a gesture begins.
  - Snapshot captures origin, current pointer position or horizontal displacement, range, and deadzone from an accepted gesture.
  - Fake presenter receives show/update/hide for press/move/release/cancel.
  - Fake presenter is not notified for ignored non-active pointer moves.
  - Production composition validation rejects a missing serialized affordance view.
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
