# Align Natural Band Shape Docs And Contracts

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md), [Slingshot Target-Coupled Band Implementation Issues](../slingshot-target-coupled-band/index.md), and [ADR-0008](../../adr/adr-0008-use-deterministic-taut-band-shape-solver-instead-of-rope-physics.md).

## Type

AFK

## User stories covered

60-63

## What to build

Align the existing Slingshot planning artifacts and runtime contracts with the refined natural Band Shape design. The accepted behavior is no longer the closest-point/arc approximation: the Band Shape should be a deterministic taut path around an inflated Launch Target Silhouette in the Pull Plane, using tangent Band Contact Points, pulled-side contour selection, fixed odd point counts, and caller-owned output buffers.

This slice should update existing docs and contract naming without adding the full solver behavior yet. It should leave implementation slices with a clear source of truth for names, config fields, validation rules, failure boundaries, and test expectations.

## Acceptance criteria

- [x] Existing PRD references that describe closest-point contact and arc-based wrap as the target behavior are updated to describe the deterministic taut Band Shape design.
- [x] Existing task references or implementation plans that would mislead future work about three-point, closest-point, or arc-only Band Shape behavior are updated or clearly superseded by this follow-up issue set.
- [x] ADR-0008 captures the concrete refined decisions: 2D Pull Plane solver, inflated convex Launch Target Silhouette, tangent contacts, pulled-side contour, caller-owned buffers, no unconditional `Physics.SyncTransforms`, and Burst deferred.
- [x] Runtime contract names move toward Band Shape terminology rather than contact-only terminology.
- [x] Config expectations are documented: `BandSilhouetteSampleCount`, odd `BandWrapSampleCount`, non-negative `BandContactPadding`, immutable config during provider/controller lifetime.
- [x] Validation expectations are documented: even `BandWrapSampleCount` is invalid, setup/programmer errors fail fast, runtime geometry solve failures return false on the hot path.

## Verification

- EditMode tests: not required for this docs/contracts slice unless contract changes are compiled here.
- PlayMode tests: none required.
- Static checks: Unity compile if runtime contracts or config names are changed; markdown review for stale contradictory Band Shape descriptions.
- Manual Unity smoke check: none required.
- Package version/changelog: no package/changelog change.

## Blocked by

None - can start immediately.
