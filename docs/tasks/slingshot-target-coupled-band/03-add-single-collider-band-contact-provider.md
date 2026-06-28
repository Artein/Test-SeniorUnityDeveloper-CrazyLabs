# Add Single-Collider Band Contact Provider

## Parent

[Slingshot Target-Coupled Band PRD](../../prd/prd-slingshot-target-coupled-band.md)

## Type

AFK

## User stories covered

2, 11, 13-14, 17, 19-20, 30-31, 35-37, 40

## What to build

Add a narrow Band contact provider for the Launch Target so the visible Band meets the target surface instead of using the Pull Point as a renderer point. The provider should use one explicitly assigned Unity Collider of any Collider type, calculate left and right Band Contact Points from the current Collider pose, apply non-negative visual padding, and constrain rendered contact data to the Slingshot Pull Plane/Band height.

This slice should integrate contact points into the ordered Band Shape but does not need the full arc-based Band Wrap yet.

## Acceptance criteria

- [x] A narrow Band contact provider interface exists for Launch Target visual contact data.
- [x] The Launch Target adapter exposes one explicitly assigned Collider for contact calculation.
- [x] Missing Collider or invalid padding is reported through authoring validation and fails fast before use.
- [x] The provider supports the base Unity Collider contract rather than hardcoding the current target shape.
- [x] The provider does not auto-discover child Colliders or choose among multiple Colliders.
- [x] Contact points are calculated from the assigned Collider's current Transform after held positioning.
- [x] Left and right Band Contact Points are derived from the relevant Band anchors and assigned Collider.
- [x] Contact padding offsets outward from the Collider toward the contact query origin.
- [x] Degenerate padding direction fallback is deterministic and never produces invalid vectors.
- [x] Rendered contact points are projected to the Pull Plane/Band height after Collider queries.
- [x] Active Pull Band Shape uses Band Contact Points instead of the Pull Point as the visible middle.
- [x] Launch math remains unchanged by contact-point calculation.

## Verification

- EditMode tests: controller requests contact data after held positioning; Band Shape uses provider contact points; launch request data remains unchanged; padding and fallback math are stable where testable without Unity engine behavior.
- PlayMode tests: representative Collider contact-point behavior; assigned Collider is used; child Colliders are not auto-selected; contact points are constrained to Pull Plane/Band height; same-frame held-position-to-contact boundary is verified if needed.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: pull the Slingshot and confirm the Band visually meets the Launch Target surface instead of passing through the mesh.
- Package version/changelog: no package/changelog change.

## Blocked by

- 01 - Add Held Target Pull Positioning And Launch Handoff
- 02 - Migrate Band Shape To Ordered Polyline
