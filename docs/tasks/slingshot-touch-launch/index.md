# Slingshot Touch Launch Implementation Issues

Parent PRD: [Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

These local implementation issues are ordered by dependency. They use tracer-bullet slices: each issue should leave a narrow, verifiable path through code, assets, tests, and documentation where that surface is relevant.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Add Gameplay Composition Spine](01-add-gameplay-composition-spine.md) | AFK | None | 53, 54, 57, 60 |
| 02 | [Add Asset-Backed Gameplay State](02-add-asset-backed-gameplay-state.md) | AFK | 01 | 38-43, 55, 57, 60 |
| 03 | [Add UnityInput Pointer Stream](03-add-unityinput-pointer-stream.md) | AFK | 01 | 30-37, 55-57 |
| 04 | [Add Band Capture And Pull Visualization](04-add-band-capture-and-pull-visualization.md) | AFK | 02, 03 | 1, 2, 6, 10-16, 20-23, 25-29, 55-57 |
| 05 | [Add Pull Release To LaunchRequested](05-add-pull-release-to-launchrequested.md) | AFK | 04 | 3-9, 15, 47, 48, 59 |
| 06 | [Add Launch Target And Slingshot Launcher](06-add-launch-target-and-slingshot-launcher.md) | AFK | 02 | 49-52, 55, 57 |
| 07 | [Add Gameplay Flow Launch Coordination](07-add-gameplay-flow-launch-coordination.md) | AFK | 02, 05, 06 | 44-46, 53-55, 57 |
| 08 | [Wire GameplayScene Playable Slingshot](08-wire-gameplayscene-playable-slingshot.md) | AFK | 01-07 | 1-15, 20-24, 51-54, 58, 60 |
| 09 | [Run Human Feel And Authoring QA Pass](09-run-human-feel-and-authoring-qa-pass.md) | HITL | 08 | 2-4, 7-12, 16-22, 58 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep MonoBehaviours shallow and register existing scene views through composition.
- Run Unity compile before tests on implementation slices.
- Prefer EditMode tests for pure controllers/services and PlayMode only for engine/scene behavior.
- VContainer is a new package dependency and must be pinned through the package manifest and lock file during implementation.
