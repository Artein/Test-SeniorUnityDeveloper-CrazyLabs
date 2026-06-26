# Issue 09 Human Feel And Authoring QA Report

## Status

- Automated authoring checks: passed.
- Automated editor-mouse interaction smoke: passed.
- Live editor Play Mode route: passed.
- Manual Play Mode feel validation: pending.
- Touch-device validation: pending; preferred before final acceptance when a device is available.

## Current Authored Values

Source: `Assets/Game/Gameplay/SlingshotConfig.asset`.

- Touch target radius: `80` pixels.
- Minimum Pull distance: `0.25` world units.
- Maximum Pull distance: `3` world units.
- Maximum lateral Pull: `1.5` world units.
- Maximum launch angle: `35` degrees.
- Minimum launch speed: `4`.
- Maximum launch speed: `12`.
- Launch up speed: `1.5`.
- Launch speed curve: linear from `0` to `1`.

## Automated Evidence

- Unity compile succeeded after issue 08 scene smoke test changes and after each issue 09 code/test adjustment.
- Live GUI RPC was restored by opening Unity `6000.3.18f1` for this project.
- Connector Play Mode enter succeeded and reported state `playing`.
- Connector Play Mode exit succeeded and reported state `edit`.
- Unity test inventory lists `Game.Gameplay.Tests.PlayMode::Game.Gameplay.Tests.PlayMode.GameplaySceneCompositionSmokeTests` as PlayMode.
- Targeted PlayMode scene smoke run `r_g5de4zlf`: `1` total, `1` passed, `0` failed, `0` warnings.
- Broad project test run `r_cu88isnz`: `73` total, `73` passed, `0` failed, `0` warnings.
- Refreshed targeted PlayMode scene smoke run `r_grun147b`: `1` total, `1` passed, `0` failed, `0` warnings.
- Refreshed broad project test run `r_9lp580ne`: `73` total, `73` passed, `0` failed, `0` warnings.
- Expanded PlayMode scene coverage first exposed a teardown lifecycle defect: repeated scene loads caused `SlingshotController.Dispose()` to drive `SlingshotView` while scene objects were being destroyed.
- Fixed teardown behavior so controller disposal unsubscribes and releases input without driving the view; normal gameplay state exit still restores inactive idle visuals.
- Targeted Slingshot lifecycle and GameplayScene PlayMode run `r_6kidcygz`: `18` total, `18` passed, `0` failed, `0` warnings. This covers scene composition, editor-mouse launch, outside-band ignore, weak Pull cancel, forward Pull clamp/no-launch, lateral steering, and controller dispose behavior.
- Broad project test run `r_v1dmb50c`: `78` total, `78` passed, `0` failed, `0` warnings.
- Refreshed selector-based GameplayScene PlayMode run `r_dydbkl61` exposed a visible-band capture defect: `6` total, `3` passed, `3` failed. Root cause: the capture hit test checked only the straight anchor-to-anchor chord, while the visible band is two segments through the rest point.
- Added regression test `SlingshotControllerTests.PointerPressed_AtRestPointAwayFromAnchorChord_StartsActivePull`; RED run `r_ylnafewa`: `13` total, `12` passed, `1` failed.
- Fixed `SlingshotController.IsInsideBandTouchTarget()` to check both visible band segments: left anchor to rest point, and rest point to right anchor.
- Rider reformat and file problem checks passed for `SlingshotController.cs` and `SlingshotControllerTests.cs`.
- Targeted Slingshot controller GREEN run `r_b8z0h4nx`: `13` total, `13` passed, `0` failed, `0` warnings.
- Targeted GameplayScene PlayMode GREEN run `r_xd84fzra`: `6` total, `6` passed, `0` failed, `0` warnings.
- Broad project run `r_llruxoos`: `79` total, `75` passed, `4` failed. Failures were stale test expectations after prior architecture decisions, not runtime slingshot regressions:
  - Gameplay State tests still expected subscriber exceptions to propagate, but the agreed behavior is guarded event invocation with logging.
  - Installer tests still expected concrete controller resolution, but the agreed behavior is interface-only/entrypoint registration.
- Updated stale tests to assert guarded Gameplay State event invocation and interface/entrypoint VContainer registration.
- Rider reformat and file problem checks passed for the updated Gameplay State, Slingshot installer, Gameplay Flow, and Gameplay LifetimeScope test files.
- Targeted stale-expectation verification run `r_ebxu5i52`: `26` total, `26` passed, `0` failed, `0` warnings.
- Latest broad project test run `r_9acz50zl`: `79` total, `79` passed, `0` failed, `0` warnings.
- `git diff --check` passed after the latest code/test cleanup.
- `GameplayScene.unity` contains authored Slingshot anchors, Launch Frame, Band LineRenderer, Pull Hint, Touch Indicator, GameplayLifetimeScope references, Gameplay State config references, Slingshot config reference, and Player RigidbodyLaunchTarget reference.
- Direct Enhanced Touch usage remains isolated behind `UnityInputBackend`.
- Direct `InputSystem` device calls outside `UnityInputBackend` are limited to the PlayMode scene interaction smoke test.
- No event bus, MonoBehaviour injection, public static class, static member, or const declaration was found in the new gameplay/input code paths.

## Manual Play Mode Checklist

Run in `Assets/Scenes/GameplayScene.unity` with editor mouse first.

- [ ] Start Pulls near the visually thin Band from center, slightly left, and slightly right.
- [ ] Confirm touches outside the intended finger-sized radius do not capture.
- [ ] Pull backward with weak, medium, and strong distances and confirm launch power scales clearly.
- [ ] Pull forward and confirm it does not create launch energy.
- [ ] Pull laterally and confirm launch direction rotates while power remains based on backward distance.
- [ ] Release below the minimum Pull distance and confirm no launch occurs.
- [ ] Confirm Pull Hint communicates the gesture without obstructing Player, Band, or Surface.
- [ ] Confirm Touch Indicator appears at the interpreted pull point and remains readable during Pull.
- [ ] Confirm Band Shape is visually understandable in idle and Active Pull states.
- [ ] Select Slingshot authoring objects and confirm gizmos make anchors, Launch Frame, Pull Plane, Pull limits, touch target, and lateral angle easy to inspect.
- [ ] Check the Console for missing-reference errors, assertion failures, and unexpected warnings.

## Touch Device Checklist

Run on a touch-capable target when available.

- [ ] Confirm a finger-sized touch target feels reliable and not surprising.
- [ ] Confirm first-touch-only behavior while additional fingers are ignored.
- [ ] Confirm touch release/cancel returns Band, Pull Hint, and Touch Indicator to the expected state.

## Manual Result Log

- Editor mouse pass result: pending.
- Touch-device pass result: pending.
- Accepted tuning changes after manual pass: pending.
- Follow-up issue docs created after manual pass: pending; only needed if manual validation finds out-of-scope concerns.

## Follow-Up Findings

- None recorded from automated authoring checks.
- Human feel acceptance is still pending because the checklist requires a person to judge interaction reliability, readability, and tuning in the Gameplay Scene.
- Touch-device validation remains pending until a device is available.
