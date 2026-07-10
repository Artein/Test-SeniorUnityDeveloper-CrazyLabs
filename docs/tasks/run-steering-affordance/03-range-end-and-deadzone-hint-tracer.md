# Range-End And Deadzone Hint Tracer

Type: AFK

## Parent

`docs/prd/prd-run-steering-affordance.md`

## What to build

Extend the serialized affordance view so the knob is accompanied by subtle **Run Steering Range End Hint** visuals and an optional **Run Steering Deadzone Hint**.

The endpoint hints should make the horizontal clamp limits slightly visible without drawing a horizontal track. The deadzone hint should stay centered on the Run Steering Origin and size itself from the same captured Run Steering Deadzone used by gameplay steering. This slice should still preserve the true origin and range near screen edges instead of shifting or compressing the visual control.

## Acceptance criteria

- [ ] Left and right endpoint hints are positioned from the captured Run Steering Range.
- [ ] The same endpoint sprite can be reused or mirrored for both sides.
- [ ] No horizontal track, rail, line, capsule, arrow, label, or joystick base is rendered.
- [ ] The deadzone hint, when enabled, is centered on the Run Steering Origin.
- [ ] The deadzone hint size is derived from captured Run Steering Range multiplied by captured Run Steering Deadzone fraction.
- [ ] The deadzone hint can be enabled, disabled, or restyled through serialized presentation data without changing gameplay deadzone values.
- [ ] Movement inside the deadzone may still move the knob visually, while requested steering remains neutral through existing control mapping.
- [ ] Near screen edges, origin and range remain true; visuals may clip or fade but must not recenter or compress the control.
- [ ] Endpoint and deadzone Images are non-interactive and raycast-transparent.

## Verification

- EditMode tests:
  - Endpoint positions equal origin plus/minus captured range.
  - Deadzone hint size follows range multiplied by deadzone fraction.
  - Enabling or disabling the deadzone hint does not affect requested steering.
  - Edge-origin layout preserves origin and endpoint math.
  - Endpoint and deadzone images have raycast targets disabled when state is applied.
- PlayMode tests:
  - Serialized view has knob, endpoint, and deadzone sprites assigned.
  - View show/update/hide path works with all three generated visual elements.
- Static checks:
  - Rider reformat and file problem inspection for changed C# files.
  - Unity connector compile before tests.
  - Unity import settings for endpoint and deadzone assets are appropriate for Sprite (2D and UI) use.
- Manual Unity smoke check:
  - Optional editor smoke: confirm endpoints are visible without any horizontal track, and deadzone hint reads as subtle/neutral.
- Package version/changelog:
  - Not required.

## Blocked by

- Knob-Only Serialized Affordance Tracer

