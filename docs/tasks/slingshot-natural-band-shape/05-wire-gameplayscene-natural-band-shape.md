# Wire GameplayScene Natural Band Shape

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md), [Slingshot Target-Coupled Band Implementation Issues](../slingshot-target-coupled-band/index.md), and [ADR-0008](../../adr/adr-0008-use-deterministic-taut-band-shape-solver-instead-of-rope-physics.md).

## Type

AFK

## User stories covered

1-15, 20-24, 58, 61-63

## What to build

Wire the natural Band Shape provider into the Gameplay Scene composition. The scene should keep explicit Slingshot anchors, Launch Frame, Band visual, Pull Hint, Touch Indicator, Gameplay State, Slingshot config, gameplay LifetimeScope, and Rigidbody Launch Target adapter wiring, while using the natural Band Shape provider and explicit Launch Target Collider silhouette source.

This slice should make the existing playable Slingshot use the natural taut Band Shape in the actual scene.

## Acceptance criteria

- [x] Gameplay Scene composition registers the natural Band Shape provider and Collider silhouette source explicitly through VContainer.
- [x] Existing Player/Launch Target has the single assigned Collider needed for silhouette sampling.
- [x] Slingshot config asset exposes and validates `BandSilhouetteSampleCount`, odd `BandWrapSampleCount`, and non-negative `BandContactPadding`.
- [x] Missing required scene references fail fast through validation/assertions instead of null-reference drift.
- [x] Starting in Pre-Launch shows the Pull Hint, holds the Player, and renders the fixed-size rest Band Shape.
- [x] Pulling from the Band moves the Player with the Pull Point and updates the natural Band Shape.
- [x] Releasing a valid Pull transitions to Running and launches opposite the pull direction, including lateral steering.
- [x] After launch, Slingshot capture remains disabled according to Gameplay State gating.
- [x] Scene gizmos remain useful for anchors, Rest Point, Pull Plane, Launch Frame axes, Pull limits, touch target behavior, and Band Shape tuning.

## Verification

- EditMode tests: any composition code that can be tested without scene loading.
- PlayMode tests: Gameplay Scene composition smoke test loads/resolves required dependencies; pulling updates target and Band Shape; same-frame Collider sampling boundary is covered if not already covered by issue 03.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: open Gameplay Scene, enter Play Mode, pull/release with editor mouse, confirm Player launches and no missing reference/errors appear.
- Package version/changelog: no package/changelog change.

## Blocked by

- 04 - Integrate Natural Band Shape Into Pull And Recoil
