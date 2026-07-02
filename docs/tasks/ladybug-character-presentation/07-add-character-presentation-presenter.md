## Parent

[Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

## What to build

Add the plain C# Character Presentation Presenter that samples gameplay lifecycle, motion, surface context, classifier output, run-result notifications, and prefab-owned tuning once per rendered tick, then applies a Character Presentation Frame to the passive view.

This slice delivers a testable runtime path from gameplay facts to view frames without requiring the final Ladybug prefab or Gameplay Scene wiring.

## Acceptance criteria

- [ ] The presenter is a VContainer-managed plain C# entry point, not a MonoBehaviour update script.
- [ ] The presenter samples current lifecycle flags, motion, course-planar speed, course-forward speed, and Run Surface Context once per rendered tick.
- [ ] The presenter owns current mode memory and current mode elapsed seconds.
- [ ] The presenter owns ungrounded elapsed seconds and resets it on grounded, pre-launch, and new-run boundaries.
- [ ] The presenter subscribes to accepted Run Result notifications and maps success to Victory and failure to Defeat through classifier input or terminal presentation state.
- [ ] The presenter unsubscribes from run-result notifications on disposal.
- [ ] The presenter builds Character Presentation Frames containing only mode and playback speed multiplier.
- [ ] Playback speed multiplier uses course-planar speed, mode-specific reference speed, and tuning clamps.
- [ ] Non-locomotion modes use neutral playback speed.
- [ ] During short ungrounded gaps that preserve locomotion, playback still uses the preserved Slide reference speed.
- [ ] The presenter uses injected frame delta time for presentation timing and does not drive physics movement.
- [ ] The presenter does not expose raw Gameplay State IDs, Run Result, Run End Reason, slope values, or trigger commands to the view.
- [ ] The presenter can be registered without injecting concrete MonoBehaviours into domain logic.

## Verification

- EditMode tests:
  - Presenter applies one frame per rendered tick to a fake view.
  - Presenter builds classifier input with lifecycle flags, speed facts, surface context, mode memory, and ungrounded timing.
  - Stable mode advances elapsed mode time and mode changes reset it.
  - Grounded/pre-launch/new-run boundaries reset ungrounded elapsed time.
  - Accepted success result reaches Victory presentation.
  - Accepted failure result reaches Defeat presentation.
  - Disposal unsubscribes from run-result notifications.
  - Playback speed uses Slide reference, clamps, zero-speed handling, reserved Run normalization, and neutral non-locomotion speed.
- PlayMode tests:
  - None expected until scene/prefab wiring slices.
- Static checks:
  - `git diff --check`.
  - Unity compile through the connector before tests.
- Manual Unity smoke check:
  - None expected; behavior should be covered by presenter tests and later scene smoke checks.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 03 Add Surface And Course Speed Facts
- 04 Add Character Presentation Mode Classifier
- 05 Expose Accepted Run Result Notification
- 06 Add Passive Character Presentation View
