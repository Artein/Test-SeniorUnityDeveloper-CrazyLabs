# Knob-Only Serialized Affordance Tracer

Type: AFK

## Parent

`docs/prd/prd-run-steering-affordance.md`

## What to build

Add the first visible **Run Steering Affordance** tracer: a serialized UI view that renders only the generated knob sprite from the affordance snapshot.

The knob should appear at the active Run Steering Origin, move horizontally from that origin, clamp at the captured Run Steering Range, ignore vertical finger movement, and hide when the gesture ends. This slice proves the core presentation path from Run Steering gesture state to a serialized, raycast-transparent UI element without adding range-end hints, deadzone hints, or animation polish.

## Acceptance criteria

- [ ] The knob UI hierarchy is authored as serialized scene or prefab data, not created at runtime.
- [ ] The generated knob image is assigned through serialized UI data and imported for UI sprite use.
- [ ] The view starts hidden or fully transparent before an active steering gesture.
- [ ] Press during Running shows the knob at the Run Steering Origin.
- [ ] Horizontal movement by the active pointer moves the knob horizontally.
- [ ] Vertical-only movement keeps the knob on the origin's visual row.
- [ ] Movement beyond captured Run Steering Range clamps the knob visually at the endpoint.
- [ ] Release, cancellation, Running exit, and controller disposal hide the knob.
- [ ] No range-end hints, deadzone hint, track, rail, line, arrows, labels, or joystick base are shown in this slice.
- [ ] The knob Image and any CanvasGroup used by the tracer are non-interactive and raycast-transparent.

## Verification

- EditMode tests:
  - Visible state places the knob at the origin when displacement is zero.
  - Positive and negative horizontal displacement move the knob in the correct direction.
  - Vertical-only displacement does not move the knob vertically.
  - Displacement beyond range clamps to left/right endpoint.
  - Hide clears visibility or alpha according to the view contract.
  - Applying a visible state disables raycast targets on the knob image.
- PlayMode tests:
  - A serialized knob-only affordance view can be loaded or instantiated and show/update/hide without throwing.
  - The generated knob sprite is assigned and visible through the authored view.
- Static checks:
  - Rider reformat and file problem inspection for changed C# files.
  - Unity connector compile before tests.
  - Unity import settings for the knob asset are appropriate for Sprite (2D and UI) use.
- Manual Unity smoke check:
  - Optional editor smoke: begin Running steering and confirm the knob appears, follows horizontally, clamps, and hides.
- Package version/changelog:
  - Not required.

## Blocked by

- Affordance Snapshot And Presenter Contract
