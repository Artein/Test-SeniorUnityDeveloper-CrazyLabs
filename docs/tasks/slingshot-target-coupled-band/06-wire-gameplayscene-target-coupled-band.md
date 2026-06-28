# Wire GameplayScene Target-Coupled Band

## Parent

[Slingshot Target-Coupled Band PRD](../../prd/prd-slingshot-target-coupled-band.md)

## Type

AFK

## User stories covered

1-15, 28, 30-31, 39-41

## What to build

Wire the target-coupled Band behavior into the playable Gameplay Scene. The scene should explicitly assign the Launch Target Collider used by the Band contact/wrap provider, expose the new config tuning needed by contact/wrap/recoil, register the Launch Target adapter through the required narrow interfaces, and update scene smoke coverage away from stale three-point Band assumptions.

The completed slice should be playable in the Editor using mouse simulation and ready for touch-device/human feel validation.

## Acceptance criteria

- [x] Gameplay Scene has one explicitly assigned Launch Target Collider for Band contact/wrap calculation.
- [x] Scene composition registers launch, held-positioning, and Band contact/wrap interfaces for the Launch Target adapter.
- [x] Slingshot config asset contains validated tuning for Band Wrap visual quality, contact padding, and recoil behavior as needed by prior slices.
- [x] Slingshot view renders the complete ordered Band Shape polyline in the scene.
- [x] Pulling from the Band moves the Launch Target with the Pull Point.
- [x] Band visuals meet and wrap around the assigned Launch Target Collider instead of passing through the target mesh.
- [x] Releasing a valid Pull launches from the pulled position.
- [x] Post-shot Band Release Recoil is visible after launch and detaches at rest.
- [x] Existing Gameplay State gating still disables Slingshot capture after Launch.
- [x] Missing scene references fail fast through validation/assertions instead of null-reference drift.
- [x] PlayMode smoke coverage is updated so it no longer asserts exactly three Band LineRenderer positions.
- [x] PlayMode smoke coverage verifies the target-coupled path enough to catch broken scene wiring.

## Verification

- EditMode tests: composition registration and validation where testable without scene loading.
- PlayMode tests: Gameplay Scene composition smoke test covering assigned Collider, ordered LineRenderer path, Launch Target movement during Pull, valid launch from pulled position, and recoil presence if deterministic.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: open Gameplay Scene, enter Play Mode, pull/release with editor mouse, confirm target movement, collider-aligned Band Shape, launch, recoil, and no missing reference/errors.
- Package version/changelog: no package/changelog change.

## Blocked by

- 01 - Add Held Target Pull Positioning And Launch Handoff
- 02 - Migrate Band Shape To Ordered Polyline
- 03 - Add Single-Collider Band Contact Provider
- 04 - Add Arc-Based Band Wrap Visuals
- 05 - Add Post-Shot Band Release Recoil
