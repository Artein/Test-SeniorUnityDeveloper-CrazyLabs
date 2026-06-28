# Add Arc-Based Band Wrap Visuals

> Superseded behavior note: this issue records the completed arc-wrap approximation. Current natural Band Shape work is tracked in
> `docs/tasks/slingshot-natural-band-shape/` and replaces arc-only wrap selection with ADR-0008's inflated silhouette, tangent contacts, and
> pulled-side contour solver.

## Parent

[Slingshot Target-Coupled Band PRD](../../prd/prd-slingshot-target-coupled-band.md)

## Type

AFK

## User stories covered

3, 9-11, 29, 32-37, 43

## What to build

Extend the Band contact provider and Band Shape construction so the visible Band includes a collider-aligned Band Wrap between Band Contact Points. The wrap should use presentation-only samples generated from an arc on the Pull Plane around the Pull Point, projected to the assigned Collider, padded outward, and inserted into the ordered Band Shape. The goal is a smooth, readable wrap that avoids drawing a straight chord through the Launch Target.

This is a best-effort visual approximation for one explicit Collider, not rope physics or exact arbitrary-collider tangent solving.

## Acceptance criteria

- [x] Band Wrap samples are included between left and right Band Contact Points in the ordered Band Shape.
- [x] Band Wrap sample origins are generated from a Pull Plane arc around the Pull Point, not linear interpolation through the target.
- [x] Asymmetric contact cases prefer the backward/pulled side aligned with negative Launch Frame forward instead of blindly using the shortest arc.
- [x] Wrap samples are projected to the assigned Collider through generic Collider surface queries.
- [x] Wrap samples use the same outward padding rule and deterministic fallback behavior as contact points.
- [x] Rendered wrap samples are constrained to the Pull Plane/Band height after Collider queries.
- [x] Band Wrap visual quality is configured through a clamped, validated segment-count setting.
- [x] Changing Band Wrap segment count changes visual sample density only.
- [x] Pull distance, Pull Offset, normalized power, launch direction, launch speed, and final Pull Point are unchanged by Band Wrap sample count.
- [x] No Burst, rope physics, joints, or exact mesh/collider silhouette solving is introduced.

## Verification

- EditMode tests: Band Shape includes ordered wrap samples; sample count follows config; launch request data is unchanged by sample count; asymmetric arc selection logic is covered with deterministic geometry fakes where practical.
- PlayMode tests: representative Collider wrap behavior; no straight visual segment through the target; wrap samples stay on Pull Plane/Band height; base Collider contract works for more than the current target shape where practical.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: pull center and off-center, then inspect whether the Band reads as wrapping behind/around the Launch Target without obvious mesh intersection.
- Package version/changelog: no package/changelog change.

## Blocked by

- 03 - Add Single-Collider Band Contact Provider
