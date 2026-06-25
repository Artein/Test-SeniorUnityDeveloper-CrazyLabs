# Implement Issue 09: Run Human Feel And Authoring QA Pass

## Summary

- Run the human-in-the-loop tuning pass for the playable Slingshot from issue 08.
- Save accepted values in the authored scene/config assets, primarily `Assets/Game/Gameplay/SlingshotConfig.asset` and `Assets/Scenes/GameplayScene.unity`.
- Keep this issue focused on feel, authoring, and validation. Do not add haptics, audio, launch sequencing, rope physics, or new gameplay systems here.

## Key Changes

- Tune Slingshot config values through actual Play Mode use:
  - Band Touch Target radius feels finger-sized but not surprising.
  - Min Pull distance cancels weak accidental pulls naturally.
  - Max Pull distance gives a readable full-power gesture.
  - Speed curve, min/max launch speed, and launch up speed produce clear weak-to-strong launch scaling.
  - Max lateral pull and max launch angle make off-center pulls steer predictably without changing power.
- Tune scene-authored visuals:
  - Band `LineRenderer` is readable in idle and Active Pull states.
  - Pull Hint communicates the gesture without obstructing Player, Band, or Surface.
  - Touch Indicator appears at the interpreted pull point and remains readable during Pull.
  - Gizmos clearly show anchors, Launch Frame, Pull Plane, Pull limits, touch target radius, and lateral angle.
- Record findings:
  - Save accepted tuning values in assets.
  - Add follow-up task docs under `docs/tasks/slingshot-touch-launch/` for any out-of-scope concerns.
  - Fix only narrow defects that block validating the already-planned first-slice behavior; otherwise split work into follow-ups.

## Public API / Types

- No new public interfaces, controllers, services, or ScriptableObject types are expected.
- Existing Slingshot, Gameplay State, Unity Input, launch, and Gameplay Flow APIs remain unchanged.
- Any API change discovered as necessary must stop this issue and become a separate implementation task unless it is a minimal defect fix required to make issue 09 testable.

## Test Plan

- Manual Play Mode QA in `Assets/Scenes/GameplayScene.unity`:
  - Start Pulls near the visually thin Band from center, slightly left, and slightly right.
  - Confirm touches outside the intended finger-sized radius do not capture.
  - Pull backward with weak, medium, and strong distances and confirm launch power scales clearly.
  - Pull forward and confirm it does not create launch energy.
  - Pull laterally and confirm launch direction rotates while power remains based on backward distance.
  - Release below min Pull and confirm no launch occurs.
  - Confirm Pull Hint, Touch Indicator, Band Shape, and selected-object gizmos are readable.
- Verification:
  - Rerun Unity compile through Unity AI Agent Connector if any assets/scripts changed.
  - Rerun the PlayMode composition smoke test from issue 08 if it exists.
  - If code is changed for a defect, run Rider checks on changed scripts, compile clean, then add/run the smallest fitting regression test.
  - Run one final editor mouse smoke test; validate on touch device when available.

## Assumptions

- Issues 01-08 are implemented first and the scene is already playable.
- The first accepted tuning pass may be editor-mouse-first, but touch-device validation is preferred before calling the issue complete.
- Follow-up issues are documentation artifacts, not hidden TODOs in code.
- This issue accepts tuning and authoring changes only; broader feel upgrades are deferred.
