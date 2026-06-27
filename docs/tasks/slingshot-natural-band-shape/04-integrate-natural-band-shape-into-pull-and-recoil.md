# Integrate Natural Band Shape Into Pull And Recoil

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md), [Slingshot Target-Coupled Band Implementation Issues](../slingshot-target-coupled-band/index.md), and [ADR-0008](../../adr/adr-0008-use-deterministic-taut-band-shape-solver-instead-of-rope-physics.md).

## Type

AFK

## User stories covered

10, 23-24, 61-63

## What to build

Integrate the natural Band Shape provider into Slingshot Active Pull and Band Release Recoil. During Active Pull, the held Launch Target should move to the clamped Pull Point first, then the controller should request a natural Band Shape from the current Launch Target Silhouette. During valid release and recoil, the loaded Band Shape should stay stable through launch handoff and then follow the moving Launch Target contact/wrap data while a virtual recoil Pull Point relaxes from the final Pull Point toward rest. If the live target-following solve fails during recoil, the controller can interpolate the last valid live shape toward the fixed rest shape as a fallback.

This slice should preserve existing launch behavior while replacing the visible closest-point/arc approximation with the deterministic natural Band Shape.

## Acceptance criteria

- [x] Active Pull updates held Launch Target position before requesting natural Band Shape data.
- [x] Active Pull renders the provider's complete ordered Band Shape rather than assembling contact and wrap geometry inside the controller.
- [x] Controller owns two active Band Shape buffers and swaps on successful solve so the last valid shape is retained without copying.
- [x] Failed runtime solve during Active Pull keeps the last valid Band Shape visible and blocks release until a valid shape exists again.
- [x] Rest/idle/inactive Band Shape uses the same fixed point count as live shapes, with exactly one Rest Point corner from odd `BandWrapSampleCount`.
- [x] Recoil live-solves the Band Shape from the moving Launch Target as the recoil Pull Point relaxes toward rest, instead of interpolating through the target mesh.
- [x] Recoil keeps target silhouette geometry and recoil-load direction separate: the moving target supplies live contact geometry, and the virtual recoil Pull Point defines the silhouette-relative pulled-side contour.
- [x] If live target-following solve fails during recoil, recoil continues from the last valid live shape toward rest instead of snapping or canceling.
- [x] Once recoil reaches rest, the Band detaches and stops following the Launch Target.
- [x] Launch request data and launch steering remain governed by Pull distance and Pull Offset, not by Band Wrap sample positions.
- [x] Slingshot view remains shallow and only renders the already-computed Band Shape.

## Verification

- EditMode tests: active pull calls held positioning before Band Shape request; failed solve keeps last valid shape and blocks release; valid release keeps loaded shape through launch handoff; recoil follows live target shape rather than interpolating through rest, then detaches at rest; weak/canceled/invalid Pulls reset directly to rest.
- PlayMode tests: launch once and verify LineRenderer follows target contact/wrap during Active Pull and the first Band Release Recoil frame without intersecting the Launch Target Collider.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: pull center/left/right and confirm the Band wraps around the target silhouette without passing through the mesh, then release and confirm recoil reads as the Band pushing the target forward.
- Package version/changelog: no package/changelog change.

## Blocked by

- 03 - Add Collider Silhouette Source And Band Shape Provider
