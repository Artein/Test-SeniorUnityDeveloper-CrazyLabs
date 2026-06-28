# Add Gameplay Flow Launch Coordination

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

## Type

AFK

## User stories covered

44-46, 53-55, 57

## What to build

Add the game-specific Gameplay Flow controller that enables/disables Slingshot capture from Gameplay State, listens to Slingshot launch intent, requests the transition to Running, and calls the Slingshot launcher only when that transition succeeds. The Slingshot controller should remain an intent producer/capture feature, and Gameplay State should remain the owner of transition legality.

This slice should establish the transition-before-launch behavior that prevents physics launch outside an accepted gameplay state.

## Acceptance criteria

- [x] Gameplay Flow controller subscribes to `LaunchRequested` and unsubscribes on disposal.
- [x] Controller is configured with Pre-Launch and Running state ids.
- [x] Controller enables Slingshot capture when current state is Pre-Launch and disables it when leaving Pre-Launch.
- [x] Controller asks Gameplay State service to transition to Running on launch request.
- [x] Launch application happens only after `TryTransitionTo(Running)` returns true.
- [x] Failed transitions skip launcher invocation without duplicating Gameplay State invalid-transition warnings.
- [x] Launcher exceptions after successful transition propagate and do not trigger rollback.
- [x] Dispose unsubscribes without mutating Gameplay State.
- [x] Dependencies are registered through the gameplay composition spine without injecting MonoBehaviours.

## Verification

- EditMode tests: initial Pre-Launch enables capture; entering/leaving Pre-Launch toggles capture; valid request transitions before launch; failed transition skips launch; launcher exception propagates after transition; dispose unsubscribes and does not mutate state.
- PlayMode tests: none required for this slice.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: temporary scene/subscriber check that a launch request changes state to Running before physics launch.
- Package version/changelog: no package/changelog change.

## Blocked by

- 02 - Add Asset-Backed Gameplay State
- 05 - Add Pull Release To LaunchRequested
- 06 - Add Launch Target And Slingshot Launcher
