# PRD: Run Steering Affordance

## Problem Statement

Running already has an invisible floating **Run Steering Control**. The control captures a **Run Steering Origin** at the active touch start, maps horizontal displacement through **Run Steering Range** and **Run Steering Deadzone**, and then lets **Run Steering Responsiveness** smooth gameplay steering toward the requested value.

That interaction is mechanically correct but visually silent. A player cannot see where the neutral point was captured, how far the usable horizontal range extends, or whether small movement is still inside the deadzone. This is especially risky on mobile because the thumb hides the area around the active touch, and the current control accepts touches anywhere during **Running**, including near screen edges.

The game needs a serialized, raycast-transparent **Run Steering Affordance** that makes the active steering gesture legible without changing the existing Run Steering Control semantics. The affordance must not become a second input path, a visible fixed joystick, or a track-style UI. It should feel like a subtle horizontal thumb knob with slight range-end visibility, no horizontal rail, and careful start/end animation that never fights the finger.

## Solution

Add a screen-space **Run Steering Affordance** for the existing Run Steering Control.

During **Running**, when the existing Run Steering Control accepts a pointer press, the affordance appears at that gesture's Run Steering Origin. It presents:

- A **knob** that moves horizontally from the Run Steering Origin according to current active pointer displacement.
- A reusable **Run Steering Range End Hint** shown on each side of the origin, just visible enough to communicate the clamp endpoints.
- An optional **Run Steering Deadzone Hint** centered on the origin, sized from the existing Run Steering Deadzone.

The affordance is presentation-only:

- It uses the same Run Steering Origin, Run Steering Range, Run Steering Deadzone, and pointer lifecycle as the existing control.
- It does not own input, poll input, receive interactive UI events, or consume raycasts.
- It does not change requested steering values.
- It does not add active smoothing or spring lag to the knob position.
- Run Steering Control remains logically independent from presentation, but every production Gameplay scene must provide a valid serialized affordance view; missing or invalid authoring fails composition validation before gameplay starts.

The UI hierarchy must be authored and serialized in the Gameplay scene or a prefab instance. The implementation must not create the joystick UI hierarchy at runtime. Runtime code may enable, hide, position, tint, scale, and animate already-authored serialized elements.

Initial generated visual assets:

- `RunSteeringAffordanceKnob`: neutral circular/tactile thumb pad, tintable, no arrows, text, direction marks, joystick base, or track cues.
- `RunSteeringAffordanceRangeEndHint`: subtle reusable vertical endpoint marker, mirrored left/right in UI, no arrowhead, rail stub, or horizontal continuation line.
- `RunSteeringAffordanceDeadzoneHint`: subtle radial soft plate/halo, tintable, no central dot, ring, target mark, joystick base, or button-like border.

Acceptance criteria:

- The affordance is visible only while an accepted Run Steering Control gesture is active.
- The affordance appears from the gesture's true Run Steering Origin.
- The affordance follows only horizontal displacement; vertical finger movement does not move the knob vertically.
- The knob clamps visually at Run Steering Range.
- The visible range endpoints align with the captured Run Steering Range for the active gesture.
- The deadzone hint, if enabled, is centered on the Run Steering Origin and sized from the captured Run Steering Deadzone.
- Movement inside the deadzone may be visible on the knob, but requested steering remains neutral according to the existing control mapping.
- Screen-edge gestures keep their true origin and range; the affordance may clip or fade near edges but must not shift, compress, or remap the control.
- The knob position updates immediately from active displacement and does not use smoothing, springing, return-to-origin tweening, or delayed catch-up.
- On gesture start, the knob appears at the current horizontal displacement for that gesture. Opacity and scale may animate.
- On gesture end or cancel, the affordance disappears from its final position. Opacity and scale may animate.
- No horizontal track, line, rail, capsule, or full joystick base is shown.
- The affordance UI is serialized in scene/prefab authoring.
- All affordance graphics and CanvasGroups are non-interactive and raycast-transparent.
- The feature preserves existing Run Steering Control behavior, Pre-Launch Pull behavior, Running state gating, range/deadzone mapping, and responsiveness smoothing.

## Unity Surfaces

Runtime assemblies and asmdefs:

- Gameplay runtime assembly remains the home for Run Steering Control orchestration, gesture state, affordance presentation state, and the serialized affordance view adapter.
- Foundation input remains the only runtime source of pointer events.
- Foundation screen metrics remain the source of raw DPI and screen facts used by the existing Run Steering Range conversion.
- No new third-party package is required.
- No new runtime asmdef is expected for the first implementation unless the existing Gameplay assembly needs to be split for size or ownership reasons.

Test assemblies and asmdefs:

- Gameplay EditMode tests cover pure layout/state calculation, gesture snapshot data, controller-to-presenter coordination with fakes, and required-view composition validation.
- Gameplay PlayMode tests cover serialized scene/prefab composition, sprite assignment, raycast transparency, and view registration where EditMode cannot cover Unity scene behavior.
- Existing steering and slingshot tests remain the regression safety net for unchanged control semantics.

Editor assemblies, windows, inspectors, importers, or menu items:

- No custom editor window or inspector is required for the first slice.
- Generated PNGs should be imported as Sprite (2D and UI) assets with alpha preserved, centered pivot intent, no mipmaps for UI use, and compression/filter settings appropriate for subtle translucent mobile UI.
- Editor-only tests may inspect sprite import setup if existing test patterns allow it without hardcoding brittle paths.

Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:

- The Gameplay scene requires a serialized Run Steering Affordance view reference through the existing scene composition style.
- The affordance hierarchy may live as a prefab instance or directly authored scene hierarchy, but all required RectTransforms, Images, CanvasGroup, and sprite references must be serialized.
- Existing Player Steering Config remains the source for Run Steering Range, Run Steering Deadzone, fallback DPI, and Run Steering Responsiveness.
- Visual style and animation tuning live on the affordance view or a small serialized tuning object referenced by the view.
- No save data, package manifest, input action asset, Addressables schema, or ProjectSettings change is expected.

RPC/helper commands, hooks, or shell wrappers:

- Unity AI Agent Connector compile is the implementation gate.
- Targeted Unity tests should run after compile is clean.
- No new shell wrapper is required.

Package versioning, changelog, and installation/sync behavior:

- This is project gameplay work, not a package release.
- Binary sprite assets are project assets and should follow the repository's existing binary asset/LFS conventions.
- No serialized migration shim is expected because this feature adds presentation on top of existing control data rather than changing saved player data.

## User Stories

1. As a player, I see a subtle steering affordance only while I am actively steering during Running.
2. As a player, I do not see the affordance during Pre-Launch.
3. As a player, I do not see the affordance while idle in Run Preparation.
4. As a player, I do not see the affordance after the run ends.
5. As a player, when I touch during Running, the affordance appears around the touch that actually started steering.
6. As a player, I can start steering from any screen region and the affordance still appears from that local touch.
7. As a player, I can start near the left screen edge without the affordance moving my origin inward.
8. As a player, I can start near the right screen edge without the affordance moving my origin inward.
9. As a player, I can start near the top or bottom of the screen without vertical touch position changing steering meaning.
10. As a player, the knob follows my horizontal thumb movement.
11. As a player, vertical thumb movement does not drag the knob up or down.
12. As a player, the knob reaches a clear left or right limit when I move beyond full steering range.
13. As a player, the knob stays at the visual limit when my thumb moves beyond the configured range.
14. As a player, I can still see that the endpoint exists even though there is no horizontal rail.
15. As a player, I do not see arrows or direction labels that make the control feel like a tutorial overlay.
16. As a player, I do not see a fixed joystick base that implies I must start from a specific screen region.
17. As a player, I can make tiny thumb corrections near the origin without the game visually implying strong steering.
18. As a player, I can see the knob move inside the deadzone while steering remains neutral.
19. As a player, the affordance does not jump from the origin if my finger has already moved by the time the first visual frame renders.
20. As a player, the knob does not lag behind my finger because of an extra visual smoothing layer.
21. As a player, the knob does not spring back to center after I release; it simply disappears cleanly.
22. As a player, releasing my finger hides the affordance.
23. As a player, touch cancellation hides the affordance.
24. As a player, leaving Running hides the affordance even if a touch was active.
25. As a player, the affordance does not alter Run Steering Control behavior after valid gameplay composition.
26. As a player, the affordance does not block future interactive UI during Running.
27. As a player, the affordance does not steal my steering touch by being an invisible UI button.
28. As a designer, I can tune the affordance opacity without changing gameplay steering.
29. As a designer, I can tune knob size without changing Run Steering Range.
30. As a designer, I can tune endpoint hint size and opacity without changing the clamp distance.
31. As a designer, I can enable, disable, or restyle the deadzone hint without changing the deadzone's gameplay value.
32. As a designer, I can tint the generated sprites from serialized UI fields.
33. As a designer, I can tune fade-in and fade-out timing from serialized fields.
34. As a designer, I can tune scale punch or settling animation from serialized fields.
35. As a designer, I cannot accidentally introduce a second range value that disagrees with Player Steering Config.
36. As a designer, I can preview the serialized hierarchy in the Unity scene or prefab.
37. As a designer, I can replace the generated knob sprite without touching steering code.
38. As a designer, I can replace the generated range-end sprite without touching steering code.
39. As a designer, I can replace the generated deadzone sprite without touching steering code.
40. As a designer, I can verify the affordance on common phone aspect ratios.
41. As a designer, I can verify the affordance under a finger even though most of it is partly occluded during use.
42. As a developer, I can keep the existing Run Steering Gesture as the source of active pointer identity and origin.
43. As a developer, I can expose a read-only steering gesture snapshot without letting presentation mutate input state.
44. As a developer, I can keep `PlayerSteeringController` as the coordinator that knows when Running steering is active.
45. As a developer, I can keep Unity input centralized behind the existing input abstraction.
46. As a developer, I can keep the affordance view as a shallow MonoBehaviour adapter.
47. As a developer, I can keep layout math in plain C# where it can be EditMode tested.
48. As a developer, I can use VContainer registration for existing serialized scene view instances.
49. As a developer, I can avoid runtime hierarchy creation for the affordance UI.
50. As a developer, I can make missing production affordance authoring fail composition with an actionable validation error.
51. As a developer, I can keep missing required serialized child references visible through validation and tests.
52. As a developer, I can make all visual Images raycast-transparent from code and from authored settings.
53. As a developer, I can verify CanvasGroup raycast blocking is disabled.
54. As a developer, I can add future interactive Running UI without rewriting Run Steering Control.
55. As a developer, I can decide touch priority with interactive UI first for touches that begin on UI.
56. As a developer, I can keep an active steering gesture captured until its release or cancellation.
57. As a developer, I can avoid reading input directly from the affordance view.
58. As a developer, I can avoid coupling visual animation to gameplay responsiveness smoothing.
59. As a developer, I can keep current steering tests meaningful because requested steering values are unchanged.
60. As a developer, I can use the generated sprites as first-pass assets without treating their exact opacity as final design truth.
61. As a tester, I can verify that press during Running shows the affordance.
62. As a tester, I can verify that press outside Running does not show the affordance.
63. As a tester, I can verify horizontal move updates the knob position.
64. As a tester, I can verify vertical-only move leaves the knob on the origin's horizontal line.
65. As a tester, I can verify movement beyond range clamps knob position.
66. As a tester, I can verify deadzone hint size follows the captured range and deadzone fraction.
67. As a tester, I can verify release hides the affordance.
68. As a tester, I can verify cancellation hides the affordance.
69. As a tester, I can verify Running exit hides the affordance and clears state.
70. As a tester, I can verify a non-active pointer does not move or hide the affordance.
71. As a tester, I can verify a second pointer press does not move the origin while a gesture is active.
72. As a tester, I can verify edge-origin behavior preserves the authored origin/range rather than recentering.
73. As a tester, I can verify all affordance graphics have raycast targets disabled.
74. As a tester, I can verify the serialized view has sprites assigned.
75. As a tester, I can verify the generated PNGs import as sprites with alpha.
76. As a tester, I can verify missing production view authoring is rejected before gameplay starts.
77. As a maintainer, I can understand from documentation that Run Steering Affordance is not Run Steering Control.
78. As a maintainer, I can change visuals later without reopening the input model.
79. As a maintainer, I can convert this PRD to independently grabbable implementation issues.
80. As a maintainer, I can keep the UI serialized so scene composition remains visible and reviewable.

## Implementation Decisions

- Use the term **Run Steering Affordance** for the visual presentation only.
- Keep **Run Steering Control** as the gameplay/input interaction name.
- Keep **Run Steering Origin**, **Run Steering Range**, **Run Steering Deadzone**, and **Run Steering Responsiveness** semantics unchanged.
- Treat this feature as presentation over existing control state, not as a new joystick input system.
- Keep gameplay steering values owned by the existing gesture/controller path.
- Expose only the read-only data presentation needs: active flag, origin screen position, current pointer screen position or horizontal displacement, captured range pixels, captured deadzone fraction, active pointer identity if needed for tests, and final lifecycle state.
- Do not let presentation code write to the steering gesture.
- Do not poll Unity touch APIs from the affordance view.
- Do not add an input action asset path for the affordance.
- Do not make the affordance an EventSystem control.
- Interactive UI has first claim for new touches that begin on interactive UI.
- The serialized affordance view itself is not interactive UI and must not affect that priority rule.
- Active steering gesture capture remains stable until release, cancellation, Running exit, or controller disposal.
- Update the affordance from pointer lifecycle events so it can respond as soon as the control state changes.
- Hide the affordance when the steering controller deactivates.
- Hide the affordance on disposal.
- Keep visual position updates immediate and deterministic.
- Do not use Run Steering Responsiveness for knob motion.
- Do not add a second visual smoothing or damping parameter for active knob position.
- Allow only non-positional animation during active gesture start and end, such as alpha, scale, or subtle tint.
- On show, set layout to the current gesture snapshot before playing fade/scale animation.
- On hide, keep the last layout while fading/scaling out, then deactivate or set alpha to zero.
- Do not animate the knob back to the origin.
- Do not tween the knob from origin to current displacement on show.
- Compute knob horizontal offset from raw horizontal displacement clamped to captured range.
- Keep the knob's vertical anchored coordinate at the Run Steering Origin's visual row.
- Size and place the endpoint hints from the captured range.
- Use one range-end sprite mirrored or anchored on the left and right side.
- Do not render a horizontal track, rail, capsule, or line between endpoints.
- Size the deadzone hint from captured range multiplied by captured deadzone fraction.
- Keep the deadzone hint visually optional and serialized, but if enabled it must use gameplay deadzone data.
- Preserve true origin and true range near screen edges.
- If endpoint hints or deadzone hint extend off-screen, clip/fade naturally through the Canvas rather than shifting the origin.
- Keep screen-to-canvas conversion in the view adapter or another Unity-facing adapter.
- Keep pure layout decisions testable without a scene where practical.
- Keep the MonoBehaviour view shallow: serialized references, Unity coordinate conversion, applying RectTransform/Image/CanvasGroup state, and validation.
- Register the required serialized scene/prefab view instance through the existing composition style.
- Do not use dynamic scene searches for normal production composition.
- Validate the required affordance view and its required child references before installing GameplayLifetimeScope dependencies.
- Do not register a production null-object or no-op affordance fallback; it would hide invalid scene authoring.
- Keep isolated controller and presenter tests independent from scene authoring by injecting fakes at their interface boundaries.
- Validate that CanvasGroup blocksRaycasts is false.
- Validate that CanvasGroup interactable is false.
- Validate that every affordance Image has raycastTarget false.
- Apply non-interactive settings in view validation/initialization so authored mistakes are corrected where possible.
- Keep generated sprites as neutral/tintable first-pass assets.
- Tune final alpha, scale, tint, and timing in serialized UI fields, not in generated PNG pixels alone.
- Import the generated PNGs as UI sprites with alpha.
- Keep sprites square with centered pivot intent.
- Prefer no mipmaps for these UI sprites.
- Do not introduce Addressables for this first slice.
- Do not change save data or economy state.
- Do not change Pre-Launch Pull, Slingshot Touch Indicator, Pull Hint, or Run Preparation UI behavior.
- Do not rename existing steering config again in this work.
- Update Gameplay context documentation if implementation introduces new public/domain terms beyond the PRD.

## Testing Decisions

Primary test level:

- Use EditMode NUnit tests for pure layout, gesture snapshot exposure, controller coordination with fakes, and required-view composition validation.
- Use PlayMode tests for serialized scene/prefab wiring, Canvas/Image raycast settings, sprite assignment, and any Unity lifecycle behavior that cannot be covered deterministically in EditMode.
- Run Unity compile before tests during implementation.

Layout/state tests should cover:

- Inactive snapshot produces hidden affordance state.
- Active snapshot at origin places knob at origin.
- Positive horizontal displacement moves knob right.
- Negative horizontal displacement moves knob left.
- Vertical-only displacement does not move knob vertically.
- Displacement beyond positive range clamps to right endpoint.
- Displacement beyond negative range clamps to left endpoint.
- Endpoint hint positions equal captured range from origin.
- Deadzone hint size equals captured range multiplied by deadzone fraction.
- Zero or invalid range is defensively clamped or hidden according to the chosen implementation contract.
- Edge-origin calculations preserve origin and range instead of shifting them inward.

Controller/presenter tests should cover:

- Press during Running shows affordance.
- Press outside Running does not show affordance.
- Move by active pointer updates affordance.
- Move by non-active pointer does not update affordance.
- Second press while active does not replace origin or view state.
- Release by active pointer hides affordance.
- Cancel by active pointer hides affordance.
- Release by non-active pointer does not hide active affordance.
- Running exit hides affordance and resets presentation state.
- Disposal hides affordance.
- Isolated controller and presenter tests use injected fakes and do not require authored Unity UI.
- Affordance update uses the same captured range as the steering gesture.
- Affordance update uses the same captured deadzone fraction as the steering gesture.
- Requested steering values remain unchanged compared with existing Run Steering Control tests.
- Run Steering Responsiveness smoothing remains applied only to gameplay steering, not to knob layout.

View tests should cover:

- Show applies root visibility and CanvasGroup alpha state.
- Hide clears visibility or alpha according to the view contract.
- Applying a visible state positions knob, endpoints, and deadzone from the state.
- Applying a visible state disables raycast targets on all images.
- On validation/initialization disables CanvasGroup interaction and raycast blocking.
- Disabled optional deadzone presentation does not require deadzone references; enabled presentation validates them.
- Required serialized references report actionable validation errors.

Scene/prefab PlayMode tests should cover:

- Gameplay scene contains one authored Run Steering Affordance view.
- The view is registered through composition consistently with other gameplay UI views.
- Knob, endpoint, and deadzone sprites are assigned.
- All affordance Images are raycast-transparent.
- CanvasGroup blocksRaycasts remains false.
- The root starts hidden or fully transparent before Running steering is active.
- A smoke call to the view's show/update/hide contract does not throw.

Manual QA should cover:

- Common portrait aspect ratios.
- Touch near left edge.
- Touch near right edge.
- Touch near top and bottom UI-safe areas.
- Fast swipe to range clamp.
- Slow movement inside deadzone.
- Release while knob is clamped.
- Touch cancellation from OS/app focus interruption.
- Editor mouse path if the existing input layer supports it for Running.
- Visual readability with a real finger covering the knob.

## Release and Compatibility

- Existing Run Steering Control behavior must remain backward compatible.
- Existing Player Steering Config remains the gameplay source of truth for range, deadzone, DPI fallback, and responsiveness.
- No player save migration is required.
- No new package dependency is required.
- Binary generated PNGs are part of the change and should follow the repository's existing asset storage conventions.
- Scene/prefab changes may create normal Unity YAML merge risk and should be kept focused.
- Missing or invalid affordance authoring makes production GameplayLifetimeScope composition invalid and is caught by validation/tests before release.
- Run Steering Control logic remains testable without scene UI through injected interface fakes.
- The feature should be compatible with future Running UI by preserving the rule that interactive UI can claim new touches and the affordance itself is raycast-transparent.
- The feature should be compatible with future visual iteration because sprites, tint, opacity, scale, and timing are serialized presentation choices.

## Out of Scope

- Changing Run Steering Control input mapping.
- Changing Run Steering Range, Run Steering Deadzone, or Run Steering Responsiveness tuning values.
- Adding a fixed on-screen joystick region.
- Adding a full joystick base.
- Adding a horizontal rail, track, line, or capsule.
- Adding arrows, text, labels, or tutorial copy to the affordance.
- Adding haptics.
- Adding audio feedback.
- Adding analytics.
- Adding accessibility settings.
- Adding new input packages or input action assets.
- Adding Addressables setup.
- Adding save-data migration.
- Changing Slingshot Pull, Pull Hint, Touch Indicator, or Pre-Launch behavior.
- Shipping final art polish beyond the first-pass generated sprites.
- Creating implementation issues in this PRD step.

## Further Notes

- Assumption: the first implementation can use the existing Gameplay assembly and scene-level VContainer composition.
- Assumption: the generated PNGs are first-pass art direction inputs, not final locked art.
- Assumption: visual tuning values should be authored after seeing the sprites in the actual Gameplay Canvas.
- Resolved decision: every production GameplayLifetimeScope requires a valid serialized affordance view and fails fast when that authoring is missing or invalid.
- Resolved decision: isolated controller and presenter tests use injected fakes; production composition does not register a null-object or no-op fallback.
- Open decision for implementation: exact default fade, scale, color, and opacity values.
- Open decision for implementation: whether endpoint and deadzone hints clip only, fade near edges, or use a simple alpha falloff when partially off-screen.
- Open decision for implementation: whether the deadzone hint ships enabled by default after in-scene visual QA.
