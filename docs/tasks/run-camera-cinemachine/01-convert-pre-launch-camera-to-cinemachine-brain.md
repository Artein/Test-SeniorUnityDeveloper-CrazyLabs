# Convert Pre-Launch Camera To Cinemachine Brain

## Parent

[Run Camera Cinemachine PRD](../../prd/prd-run-camera-cinemachine.md) and [ADR-0009](../../adr/adr-0009-use-cinemachine-for-run-camera.md).

## Type

AFK

## User stories covered

2, 13, 23, 33-34, 50

## What to build

Introduce Cinemachine without changing **Pre-Launch** behavior. The real Main Camera remains the render camera and the **Slingshot** input camera, but it is driven through a Cinemachine Brain. A scene-authored **Pre-Launch Camera** should match the current fixed camera shot so existing **Pull** projection remains stable before **Launch**.

This slice should be demoable on its own: entering the scene should look and behave like the current **Pre-Launch** setup, while the project now has the package and scene foundation needed for later **Run Camera** slices.

## Acceptance criteria

- [ ] Unity Cinemachine is added as an explicit project dependency and package resolution is captured.
- [ ] The Main Camera remains the authoritative render/input camera and has a Cinemachine Brain.
- [ ] A scene-authored **Pre-Launch Camera** matches the existing fixed camera pose and lens closely enough that **Slingshot** input projection does not change.
- [ ] The **Pre-Launch Camera** is live before **Launch** without requiring **Run Camera** objects or lifecycle code.
- [ ] No **Slingshot**, **Pull**, **Pull Plane**, **Launch Frame**, launch force, or steering behavior is changed by this slice.
- [ ] Scene composition validation catches a missing Brain or missing **Pre-Launch Camera**.

## Verification

- EditMode tests: composition or validation coverage for required camera references where practical.
- PlayMode tests: Gameplay Scene has a Main Camera with Cinemachine Brain and a live **Pre-Launch Camera**; existing **Pre-Launch** projection smoke coverage still passes.
- Static checks: Unity compile; `git diff --check`.
- Manual Unity smoke check: start scene and verify the initial **Slingshot** framing and **Pull** behavior match the previous fixed shot.
- Package version/changelog: manifest and lock file include Cinemachine; no project changelog entry required unless the repo already requires one for dependency changes.

## Blocked by

None - can start immediately.
