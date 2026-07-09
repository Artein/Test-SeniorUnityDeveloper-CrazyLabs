# Non-Positional Animation And Serialized Styling

Type: AFK

## Parent

`docs/prd/prd-run-steering-affordance.md`

## What to build

Add serialized visual tuning for the **Run Steering Affordance** without changing active knob layout.

This slice should let designers tune tint, opacity, scale, fade-in, fade-out, and other non-positional presentation values from authored UI data. Animation may affect opacity, scale, or tint. It must not smooth the active knob position, tween the knob from origin to the current displacement, or animate the knob back to origin on release.

## Acceptance criteria

- [ ] Tint, opacity, scale, and timing values are serialized on the affordance view or a small serialized tuning object.
- [ ] On show, layout is applied to the current gesture state before any fade or scale animation begins.
- [ ] During active steering, knob position follows the latest horizontal displacement immediately.
- [ ] Active knob position does not use Run Steering Responsiveness, extra smoothing, damping, springing, or delayed catch-up.
- [ ] On hide, the affordance disappears from its final position rather than returning to origin.
- [ ] Fade/scale animation handles release, cancellation, Running exit, and disposal consistently.
- [ ] Zero-duration and very small-duration animation settings behave deterministically.
- [ ] Defaults are conservative and readable with the first-pass generated sprites.

## Verification

- EditMode tests:
  - Show applies layout before visible alpha/scale state changes.
  - Move updates active knob position immediately even while visual alpha/scale is changing.
  - Hide keeps the final layout and does not animate the knob back to origin.
  - Release, cancel, Running exit, and disposal share the same hide behavior.
  - Zero-duration timings produce final states without exceptions.
- PlayMode tests:
  - Serialized tuning values are applied by the authored view.
  - A smoke show/update/hide sequence with animation enabled completes without throwing.
- Static checks:
  - Rider reformat and file problem inspection for changed C# files.
  - Unity connector compile before tests.
- Manual Unity smoke check:
  - Optional editor smoke: verify the affordance appears and disappears cleanly without positional lag or return-to-origin movement.
- Package version/changelog:
  - Not required.

## Blocked by

- Range-End And Deadzone Hint Tracer

