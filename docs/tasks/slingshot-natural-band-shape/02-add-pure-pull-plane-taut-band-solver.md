# Add Pure Pull Plane Taut Band Solver

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md), [Slingshot Target-Coupled Band Implementation Issues](../slingshot-target-coupled-band/index.md), and [ADR-0008](../../adr/adr-0008-use-deterministic-taut-band-shape-solver-instead-of-rope-physics.md).

## Type

AFK

## User stories covered

61-63

## What to build

Add the pure C# taut Band Shape solver that works entirely in 2D Pull Plane coordinates. Given anchors, pull direction data, a sampled Launch Target Silhouette, padding, and output sample counts, the solver should reduce samples to a convex silhouette, inflate it, find valid tangent Band Contact Points, choose the contour containing the Pulled-Side Center, reject free spans that cross the silhouette, and write a fixed-size ordered Band Shape path.

The solver should not know about Unity `Collider`, `Transform`, `Rigidbody`, scene objects, or `Vector3` world mapping. It should be deterministic, allocation-conscious, and covered primarily by EditMode tests.

## Acceptance criteria

- [x] Solver operates on Pull Plane `float2` data and does not depend on Unity scene or physics object lifetimes.
- [x] Raw silhouette samples are reduced to an ordered convex outer silhouette before tangent and contour selection.
- [x] `BandContactPadding` inflates the convex silhouette by offsetting hull edges outward in Pull Plane space.
- [x] Tangent Band Contact Points are selected from valid external tangent candidates from each Band anchor to the inflated silhouette.
- [x] The selected Band Wrap contour contains the Pulled-Side Center derived from the actual pull direction, including lateral Pull Offset.
- [x] Odd `BandWrapSampleCount` places the middle wrap sample exactly at the Pulled-Side Center.
- [x] If multiple candidate pairs contain the Pulled-Side Center, the solver chooses the shortest taut path whose free spans do not cross the inflated silhouette.
- [x] Valid solver output has stable order and exactly `BandWrapSampleCount + 4` points: left anchor, left contact, wrap samples, right contact, right anchor.
- [x] Runtime geometry failures are represented as failed solve results, not hot-path exceptions.
- [x] No LINQ or normal-path allocations are introduced inside repeated solve paths.

## Verification

- EditMode tests: pulled-side contour for center/left/right pulls; middle wrap sample at Pulled-Side Center; free-span crossing rejection; fixed point count and stable order; invalid or degenerate silhouettes fail solve; even `BandWrapSampleCount` validation through config/validator where introduced.
- PlayMode tests: none required for the pure solver.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: none required for this pure solver slice.
- Package version/changelog: no package/changelog change.

## Blocked by

- 01 - Align Natural Band Shape Docs And Contracts
