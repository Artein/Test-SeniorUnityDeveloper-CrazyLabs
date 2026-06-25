# Add Gameplay Flow Launch Coordination

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

## Type

AFK

## User stories covered

44-46, 53-55, 57

## What to build

Add the game-specific Gameplay Flow controller that listens to Slingshot launch intent, requests the transition to Running, and calls the Slingshot launcher only when that transition succeeds. The Slingshot controller should remain an intent producer, and Gameplay State should remain the owner of transition legality.

This slice should establish the transition-before-launch behavior that prevents physics launch outside an accepted gameplay state.

## Acceptance criteria

- [ ] Gameplay Flow controller subscribes to `LaunchRequested` and unsubscribes on disposal.
- [ ] Controller is configured with the Running state id and asks Gameplay State service to transition there.
- [ ] Launch application happens only after `TryTransitionTo(Running)` returns true.
- [ ] Failed transitions skip launcher invocation without duplicating Gameplay State invalid-transition warnings.
- [ ] Launcher exceptions after successful transition propagate and do not trigger rollback.
- [ ] Dispose unsubscribes without mutating Gameplay State or canceling Slingshot state.
- [ ] Dependencies are registered through the gameplay composition spine without injecting MonoBehaviours.

## Verification

- EditMode tests: valid request transitions before launch; failed transition skips launch; launcher exception propagates after transition; dispose unsubscribes and does not mutate state.
- PlayMode tests: none required for this slice.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: temporary scene/subscriber check that a launch request changes state to Running before physics launch.
- Package version/changelog: no package/changelog change.

## Blocked by

- 02 - Add Asset-Backed Gameplay State
- 05 - Add Pull Release To LaunchRequested
- 06 - Add Launch Target And Slingshot Launcher
