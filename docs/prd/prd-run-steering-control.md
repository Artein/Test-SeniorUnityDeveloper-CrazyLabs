# PRD: Run Steering Control

## Problem Statement

Running steering currently depends on absolute screen position around screen center. That makes the neutral point a global screen location instead of the player's active touch position, so the same thumb movement can feel different across screen sizes, DPI values, and touch start positions.

The current setup also keeps multiple concepts under names that are easy to misread:

- `SteeringSensitivity` behaves like an extra multiplier even though the desired control should have a clear physical range.
- `SteeringResponseRate` sounds like input polling frequency, but the actual domain meaning is how quickly the Launch Target responds to requested steering.
- `PlayerSteeringController` owns state gating, pointer lifecycle, input mapping, smoothing, and movement application in one class.

The game needs an invisible floating Run Steering Control during Running. The first active touch should define the Run Steering Origin, a small physical horizontal shift should map to steering, and the result should remain consistent across screen sizes and DPI values.

## Solution

Replace the screen-center steering mapping used during Running with an invisible floating Run Steering Control.

During Running, the first pointer press starts one active steering gesture and captures the Run Steering Origin. Horizontal movement from that origin maps to a requested steering value in the `-1..1` range. Movement at or beyond Run Steering Range produces full steering. Movement inside Run Steering Deadzone produces no steering. Movement between the deadzone and the full range is remapped continuously so full physical range still reaches full steering.

The requested steering value remains an input request, not direct motion. Run Steering Responsiveness still controls how quickly the Launch Target moves from its current steering value toward the requested steering value. The responsiveness upgrade keeps its current gameplay meaning, but the serialized/configuration name should be changed to match that meaning.

Acceptance criteria:

- Run Steering Control is available only during Running.
- Pre-Launch Pull behavior is not changed by this work.
- The Run Steering Origin is the first active touch position during Running, not screen center.
- Only one active steering gesture is accepted at a time.
- Extra touches are ignored while a steering gesture is active.
- Pointer release or cancellation clears the requested steering value.
- Leaving Running fully resets the active gesture and current/desired steering.
- Horizontal displacement controls steering.
- Vertical displacement has no steering meaning.
- Run Steering Range is configured as physical distance in centimeters.
- The physical range is converted to pixels using the current screen DPI.
- The converted pixel range is captured when a gesture starts and remains stable for that gesture.
- Screen metric changes affect the next gesture, not the gesture already in progress.
- Raw DPI values are used when valid.
- Raw DPI `96` is valid and should not automatically fallback.
- Raw DPI `0`, negative, NaN, infinite, or greater than `1000` should fallback.
- The initial fallback DPI is `326`.
- The initial Run Steering Range is `1.5cm`.
- The initial Run Steering Deadzone is `0.15` of Run Steering Range.
- Screen-edge touches are accepted.
- Screen-edge behavior keeps the strict physical range; do not shift the origin, compress one side, or reject edge touches.
- `SteeringSensitivity` is removed from the active steering model.
- `SteeringResponseRate` is renamed to `SteeringResponsiveness`.
- No serialized migration shim is required for old steering fields.

## Unity Surfaces

Runtime gameplay surfaces:

- `PlayerSteeringController` remains the orchestration point for gameplay state gating, input subscription, Run Steering Responsiveness smoothing, and Launch Target movement.
- A plain C# `RunSteeringGesture` owns active pointer identity, Run Steering Origin, deadzone/range mapping, release/cancel handling, and gesture reset.
- No separate `RunSteeringInputMapper` is introduced initially. Mapping stays inside `RunSteeringGesture` until reuse or complexity justifies extraction.
- `PlayerSteeringConfig` owns Run Steering Range, Run Steering Deadzone, Run Steering Responsiveness, fallback DPI, and DPI validation limits.
- Existing Foundation input stays as generic pointer input through `IUnityInput`.
- Existing Foundation screen abstraction is extended with raw DPI through `IScreen`.
- `UnityScreen` returns raw Unity screen facts and does not apply gameplay fallback policy.

Assets and composition:

- The current player steering configuration asset is updated intentionally.
- Old/outdated serialized steering fields are removed rather than migrated.
- No new visible joystick prefab, canvas, sprite, or UI hierarchy is required.
- No new package dependency is required.
- No new asmdef is expected for the first implementation.
- Existing VContainer composition should continue to inject gameplay configuration and Foundation services.

Documentation surfaces:

- Gameplay context documentation should keep the agreed terms:
  - Run Steering Control
  - Run Steering Origin
  - Run Steering Range
  - Run Steering Deadzone
  - Run Steering Responsiveness
  - Launch Target
  - Running
  - Pre-Launch
  - Pull
- Existing ADRs are sufficient:
  - Gameplay logic belongs in plain C# controllers/services.
  - Direct C# events are preferred before introducing an event bus.
  - Unity input remains centralized behind the existing input abstraction.
  - VContainer remains the composition mechanism.

## User Stories

1. As a player, I can touch anywhere on the screen during Running and begin steering from that touch.
2. As a player, I do not need to touch near screen center to get neutral steering.
3. As a player, my thumb's starting position feels neutral when the gesture begins.
4. As a player, moving my thumb right from the origin steers right.
5. As a player, moving my thumb left from the origin steers left.
6. As a player, moving my thumb vertically does not accidentally steer.
7. As a player, tiny hand jitter near the origin does not cause visible steering.
8. As a player, steering reaches full strength after a short physical thumb shift.
9. As a player, moving farther than the steering range keeps full steering instead of cancelling control.
10. As a player, lifting my finger stops requesting steering.
11. As a player, a cancelled touch stops requesting steering.
12. As a player, starting near the left screen edge still lets me steer with the same physical range.
13. As a player, starting near the right screen edge still lets me steer with the same physical range.
14. As a player, the control feels like an invisible floating joystick without a visible UI element.
15. As a player, steering does not unexpectedly begin during Pre-Launch Pull.
16. As a player, Running steering does not interfere with launch preparation input.
17. As a player, the Launch Target continues to respond smoothly rather than snapping instantly unless tuning says so.
18. As a player, responsiveness upgrades still make Running steering feel quicker.
19. As a designer, I can tune Run Steering Range in centimeters.
20. As a designer, I can tune Run Steering Deadzone as a fraction of Run Steering Range.
21. As a designer, I can tune Run Steering Responsiveness using terminology that matches the gameplay effect.
22. As a designer, I do not have to reason about both steering range and an additional sensitivity multiplier.
23. As a designer, I can keep maximum turn rate and speed limits separate from input mapping.
24. As a designer, I can use one initial tuning baseline across common phones and tablets.
25. As a designer, I can test edge-touch behavior without hidden origin shifting.
26. As a developer, I can test Run Steering Gesture behavior without a scene.
27. As a developer, I can test physical range mapping with deterministic screen metrics.
28. As a developer, I can keep Unity API calls behind Foundation services.
29. As a developer, I can keep `UnityScreen` as a raw facts adapter and keep gameplay fallback policy in gameplay config.
30. As a developer, I can reason about pointer lifecycle in one plain C# collaborator.
31. As a developer, I can keep `PlayerSteeringController` focused on game state, smoothing, and movement.
32. As a developer, I can avoid a premature `RunSteeringInputMapper` class.
33. As a developer, I can remove old steering config fields without maintaining unused compatibility code.
34. As a developer, I can preserve generic pointer input for touch, editor mouse, and tests.
35. As a developer, I can ignore extra touches while one active steering gesture is in progress.
36. As a developer, I can reset gesture state when gameplay leaves Running.
37. As a developer, I can keep active gesture range stable even if screen metrics change mid-gesture.
38. As a tester, I can verify deadzone, clamp, and remap behavior through EditMode tests.
39. As a tester, I can verify valid and fallback DPI cases deterministically.
40. As a tester, I can verify raw DPI `96` is accepted.
41. As a tester, I can verify raw DPI `0` falls back.
42. As a tester, I can verify impossible or extreme DPI values fall back.
43. As a tester, I can verify vertical movement is ignored.
44. As a tester, I can verify release clears requested steering.
45. As a tester, I can verify cancellation clears requested steering.
46. As a tester, I can verify extra touches do not steal control.
47. As a tester, I can verify Running exit resets current and desired steering.
48. As a tester, I can verify responsiveness smoothing still applies after input mapping changes.
49. As a tester, I can verify movement limits still apply after responsiveness smoothing.
50. As a maintainer, I can understand from names that Run Steering Responsiveness is not an input polling rate.
51. As a maintainer, I can find the physical control policy in one small module.
52. As a maintainer, I can update gameplay terminology without changing Foundation input semantics.
53. As a maintainer, I can avoid scene or UI churn for a control that has no visual surface.
54. As a maintainer, I can keep future two-axis or visible joystick work out of the current scope.

## Implementation Decisions

- Keep the feature in the Gameplay context.
- Use the term Run Steering Control for the invisible floating steering interaction during Running.
- Use the term Launch Target for the controlled object during Running.
- Use Run Steering Origin for the neutral point captured at the start of one active gesture.
- Use Run Steering Range for the physical distance that maps to full steering.
- Use Run Steering Deadzone for the neutral portion near the origin.
- Use Run Steering Responsiveness for the smoothing speed from requested steering to applied steering.
- Do not call responsiveness a rate in user-facing config or domain documentation.
- Begin a gesture from the first pointer press accepted while Running.
- Store the active pointer id until release, cancellation, or Running exit.
- Ignore all other pointer ids while a gesture is active.
- Map horizontal displacement from origin to steering.
- Ignore vertical displacement for steering.
- Clamp displacement beyond range to full steering.
- Treat displacement beyond range as accepted input, not invalid input.
- Apply deadzone before returning requested steering.
- Remap the post-deadzone portion so the configured range still reaches full steering.
- Use signed output so left and right remain symmetric.
- Keep Run Steering Range as centimeters in config.
- Convert centimeters to pixels using raw DPI when valid.
- Capture converted range at gesture start.
- Keep the gesture's pixel range stable until that gesture ends.
- Use fallback DPI only for invalid or extreme raw DPI values.
- Accept raw DPI `96` as valid because Unity may report desktop/editor or logical values and that raw fact is still usable.
- Use fallback DPI for raw DPI `0` because Unity documents that DPI can be unknown.
- Keep fallback and validation policy in gameplay configuration, not in the screen adapter.
- Extend the screen abstraction only with raw DPI for now.
- Do not introduce a broader display metrics abstraction for this feature.
- Keep the input path on existing generic pointer events.
- Do not depend directly on mobile touch APIs from gameplay steering code.
- Do not add a visible joystick, touch handle, radial background, or canvas overlay.
- Do not add haptic, audio, or tutorial feedback in this work.
- Remove `SteeringSensitivity` from the active steering model.
- Rename `SteeringResponseRate` to `SteeringResponsiveness`.
- Remove outdated serialized steering fields rather than preserving compatibility shims.
- Update the existing steering config asset intentionally.
- Keep maximum turn rate, minimum steer speed, and maximum planar speed independent from input mapping.
- Keep the controller responsible for state gating and movement application.
- Move pointer lifecycle and mapping into a small plain C# collaborator.
- Avoid extracting a separate mapper until the mapping is reused or grows more complex.
- Preserve existing behavior where release/cancel clears desired steering.
- Preserve existing behavior where leaving Running resets active pointer and current/desired steering.
- Preserve responsiveness stat integration under the renamed domain meaning.
- Prefer focused tests over broad scene tests for mapping behavior.

## Testing Decisions

Primary test level:

- Use EditMode NUnit tests for Run Steering Gesture mapping and lifecycle.
- Use controller-level EditMode tests for gameplay state gating, responsiveness, and movement integration where existing seams allow.
- Use PlayMode only if a runtime Unity lifecycle or scene composition behavior cannot be covered deterministically in EditMode.

Run Steering Gesture tests should cover:

- Press begins a gesture and captures origin.
- Initial output is neutral.
- Horizontal movement right produces positive steering.
- Horizontal movement left produces negative steering.
- Vertical-only movement produces neutral steering.
- Movement inside deadzone produces neutral steering.
- Movement outside deadzone is remapped continuously.
- Movement at range produces full steering.
- Movement beyond range remains full steering.
- Release by the active pointer clears output and ends the gesture.
- Cancellation by the active pointer clears output and ends the gesture.
- Release by a non-active pointer has no effect.
- Movement by a non-active pointer has no effect.
- A second press while active does not steal control.
- Reset clears active gesture and output.
- Range in pixels is captured at gesture start.
- Later metric changes do not affect the active gesture.
- A new gesture uses the latest validated metrics.
- Raw DPI `96` is accepted.
- Raw DPI `0` uses fallback DPI.
- Negative DPI uses fallback DPI.
- NaN DPI uses fallback DPI.
- Infinite DPI uses fallback DPI.
- DPI greater than `1000` uses fallback DPI.
- Edge-origin gestures keep strict physical range behavior.

Controller tests should cover:

- Pointer input before Running does not start Run Steering Control.
- Pointer input during Running starts Run Steering Control.
- Pointer movement during Running changes requested steering through the gesture.
- Pointer release during Running clears requested steering.
- Pointer cancellation during Running clears requested steering.
- Leaving Running resets active gesture and current/desired steering.
- Run Steering Responsiveness still smooths toward requested steering.
- Responsiveness upgrade/stat value still affects response speed.
- Maximum turn rate still limits rotation.
- Minimum steer speed and maximum planar speed behavior remain independent from input mapping.
- Launch Target movement remains deterministic in fixed-step tests.

Regression gates:

- Run Unity script compilation through the Unity AI Agent connector before tests.
- Fix compile errors before running tests.
- Run targeted tests touched by this change after compilation is clean.
- If Jobs or Burst are not touched, no Burst-specific guard is required.

## Release and Compatibility

This is gameplay implementation work inside the existing Unity mobile game. It is not a package release and does not require package manifest changes.

Serialization compatibility is intentionally not preserved for outdated steering fields. Existing project steering assets should be updated to the new fields as part of the implementation. No save format, player profile, Addressables schema, or public package API change is expected.

The main compatibility risk is tuning drift. The new physical range replaces the old screen-center plus sensitivity mapping, so device feel should be reviewed on at least one phone-class target after tests pass.

The Foundation input contract remains generic pointer input, preserving editor mouse, automated tests, and mobile touch routing.

## Out of Scope

- Visible joystick UI.
- Floating handle art.
- Canvas overlay changes.
- Haptic feedback.
- Audio feedback.
- Tutorial or onboarding copy.
- Multi-finger steering.
- Two-axis steering.
- Vertical steering.
- Camera changes.
- Slingshot Pull behavior changes.
- Pre-Launch input redesign.
- New input package dependency.
- New display metrics abstraction.
- Device-specific DPI database.
- Per-device steering presets.
- Save format migration.
- Serialized compatibility shim for old steering fields.
- New ADR.
- Performance profiling beyond normal compile and test gates.

## Further Notes

Initial tuning should start with:

- Run Steering Range: `1.5cm`
- Run Steering Deadzone: `0.15` of range
- Fallback DPI: `326`
- Minimum accepted raw DPI: `1`
- Maximum accepted raw DPI: `1000`

Assumptions:

- Human feel tuning can happen after the first implementation is test-clean.
- The existing responsiveness upgrade remains the correct upgrade path; only the terminology changes.
- The current Gameplay assembly is still the right home for this feature.
- Current ADRs already cover the architectural decision; no new ADR is needed.

Unresolved follow-up:

- Validate physical feel on a representative mobile device after implementation.
