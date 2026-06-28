# Add Camera Geometry Layers And Decollider Safety

## Parent

[Run Camera Cinemachine PRD](../../prd/prd-run-camera-cinemachine.md) and [ADR-0009](../../adr/adr-0009-use-cinemachine-for-run-camera.md).

## Type

AFK

## User stories covered

6-7, 12, 16-18, 22, 27, 40, 49, 55-60

## What to build

Make the **Run Camera** respect intentional camera-blocking geometry. Add explicit camera geometry layers for terrain and obstacles, assign the current slope/surface and blocking objects appropriately, and configure the **Run Camera** with Cinemachine Third Person Follow plus Decollider so it stays outside terrain and solid object interiors.

This slice should keep the first implementation conservative: solve camera-object and camera-terrain intersection first, and do not enable Deoccluder unless a real line-of-sight blocker has been proven during playtesting.

## Acceptance criteria

- [ ] Project layer names include explicit camera geometry layers such as `CameraTerrain` and `CameraObstacle`.
- [ ] Terrain/slope surfaces that the camera must stay above are assigned to the camera terrain layer.
- [ ] Walls and solid props that should block the camera are assigned to the camera obstacle layer.
- [ ] The **Launch Target**, **Band**, UI, triggers, collectibles, and non-blocking decoration are excluded from camera collision filters.
- [ ] The **Run Camera** uses Cinemachine Third Person Follow for run composition.
- [ ] The **Run Camera** uses Cinemachine Decollider for terrain/object safety.
- [ ] Decollider filters include only intended camera geometry layers.
- [ ] Deoccluder is not required in this first slice and remains deferred unless playtesting proves target occlusion is a problem.
- [ ] Camera correction recovers smoothly enough that temporary collision adjustment does not permanently trap the view.

## Verification

- EditMode tests: layer policy/config validation where testable without scene loading.
- PlayMode tests: Gameplay Scene has expected camera geometry layers; **Run Camera** includes Third Person Follow and Decollider; current surface is assigned to camera terrain where practical; simple slope/object smoke case keeps camera outside camera geometry where practical.
- Static checks: Unity compile; `git diff --check`.
- Manual Unity smoke check: launch, slide down slopes, pass near solid objects, and verify the camera does not show below slope surfaces or enter object interiors.
- Package version/changelog: no additional package dependency beyond Cinemachine; no project changelog entry required unless the repo already requires one for ProjectSettings changes.

## Blocked by

- [Add Launch-Gated Run Camera Handoff](02-add-launch-gated-run-camera-handoff.md)
