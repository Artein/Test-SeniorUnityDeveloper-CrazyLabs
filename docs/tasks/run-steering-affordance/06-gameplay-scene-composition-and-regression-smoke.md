# Gameplay Scene Composition And Regression Smoke

Type: AFK

## Parent

`docs/prd/prd-run-steering-affordance.md`

## What to build

Wire the complete **Run Steering Affordance** into the Gameplay scene through the existing serialized scene-view composition style.

This slice should make the feature demoable in the actual game scene: the serialized affordance view is present, registered, hidden by default, uses the generated sprites, remains raycast-transparent, and is driven by Running steering while preserving Run Steering Control and Pre-Launch behavior.

## Acceptance criteria

- [ ] Gameplay scene contains exactly one authored Run Steering Affordance view when the feature is enabled.
- [ ] The view is wired through existing composition conventions without runtime hierarchy creation.
- [ ] The view has serialized references for root, knob, left/right endpoint hints, deadzone hint, CanvasGroup, and generated sprites.
- [ ] The view starts hidden or fully transparent before an active Running steering gesture.
- [ ] Press/move/release during Running shows, updates, and hides the affordance in the Gameplay scene.
- [ ] Presses outside Running do not show the affordance.
- [ ] Pre-Launch Pull behavior is unchanged.
- [ ] Missing or disabled presentation still does not prevent Run Steering Control from functioning in non-authored contexts.
- [ ] Scene validation reports broken authored references clearly.
- [ ] No package manifest, save data, Addressables, input action asset, haptic, audio, or analytics changes are introduced.

## Verification

- EditMode tests:
  - Composition registration resolves the affordance view or safe fallback according to the chosen policy.
  - Lifetime/scope validation reports missing required references on an authored view.
  - Existing steering controller and gesture regression tests still pass.
- PlayMode tests:
  - Gameplay scene contains the authored affordance view.
  - Sprites are assigned for knob, endpoint hints, and deadzone hint.
  - All affordance graphics are raycast-transparent.
  - Root starts hidden or fully transparent.
  - View show/update/hide smoke call does not throw.
  - Scene-level Run Steering Control smoke path still works.
- Static checks:
  - Rider reformat and file problem inspection for changed C# files.
  - Unity connector compile before tests.
  - Unity import settings for generated PNGs are correct for UI sprite use.
- Manual Unity smoke check:
  - Enter Running in the Gameplay scene, touch/drag horizontally, confirm affordance appears, clamps, and hides.
  - Confirm Pre-Launch Pull still works.
- Package version/changelog:
  - Not required.

## Blocked by

- Range-End And Deadzone Hint Tracer
- Non-Positional Animation And Serialized Styling
- Interactive UI Priority And Raycast Guardrails

