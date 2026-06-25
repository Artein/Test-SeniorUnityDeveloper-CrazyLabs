# Wire GameplayScene Playable Slingshot

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

## Type

AFK

## User stories covered

1-15, 20-24, 51-54, 58, 60

## What to build

Wire the complete Slingshot mechanic into the Gameplay Scene. The scene should have authored Slingshot anchors, Launch Frame, Band visual, UI Pull Hint, UI Touch Indicator, Gameplay State config/assets, Slingshot config, gameplay `LifetimeScope`, and Rigidbody Launch Target adapter connected to the existing Player.

The completed slice should be playable in the Editor using mouse simulation and ready for touch-device validation in the follow-up human QA slice.

## Acceptance criteria

- [ ] Gameplay Scene contains explicit Slingshot left/right/rest anchor references and Launch Frame.
- [ ] Band visual is assigned and renders through the Slingshot view.
- [ ] Pull Hint and Touch Indicator are scene-authored UI objects under a Canvas and controlled by the Slingshot view.
- [ ] Gameplay State assets/config and Slingshot config are assigned through explicit serialized references.
- [ ] Gameplay `LifetimeScope` registers UnityInput, Gameplay State, Slingshot controller, Slingshot launcher, Gameplay Flow, scene view interfaces, and launch target adapter.
- [ ] Existing Player Rigidbody is assigned to the Rigidbody Launch Target adapter.
- [ ] Missing required scene references fail fast through validation/assertions instead of null-reference drift.
- [ ] Starting in Pre-Launch shows the Pull Hint and holds the Player.
- [ ] Pulling from the Band updates the Band Shape and Touch Indicator.
- [ ] Releasing a valid Pull transitions to Running and launches the Player forward.
- [ ] After launch, Slingshot capture is disabled and accidental re-pulls do not relaunch.
- [ ] A PlayMode composition smoke test is added if practical and deterministic.

## Verification

- EditMode tests: any composition code that can be tested without scene loading.
- PlayMode tests: Gameplay Scene composition smoke test if practical; otherwise document why it is deferred.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: open Gameplay Scene, enter Play Mode, pull/release with editor mouse, confirm Player launches and no missing reference/errors appear.
- Package version/changelog: no package/changelog change.

## Blocked by

- 01 - Add Gameplay Composition Spine
- 02 - Add Asset-Backed Gameplay State
- 03 - Add UnityInput Pointer Stream
- 04 - Add Band Capture And Pull Visualization
- 05 - Add Pull Release To LaunchRequested
- 06 - Add Launch Target And Slingshot Launcher
- 07 - Add Gameplay Flow Launch Coordination
