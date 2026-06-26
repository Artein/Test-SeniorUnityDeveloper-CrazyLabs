# Migrate Band Shape To Ordered Polyline

## Parent

[Slingshot Target-Coupled Band PRD](../../prd/prd-slingshot-target-coupled-band.md)

## Type

AFK

## User stories covered

2, 16-17, 28, 41

## What to build

Replace the stale three-point Band Shape model with an ordered world-space polyline model that the view can render with every supplied point. This makes the Band renderer ready for contact points and wrap samples while keeping this slice narrow: it may still use a simple rest/pull path until the contact provider is added, but no public behavior should require exactly three positions or a middle point equal to the Pull Point.

This slice should update stale tests and smoke expectations that assert a fixed three-point LineRenderer path.

## Acceptance criteria

- [x] Band Shape data can represent an ordered world-space polyline with variable point count.
- [x] Band Shape validates or fails fast for invalid/insufficient point data.
- [x] Rest and Active Pull visuals can still be represented before collider contact/wrap is added.
- [x] Slingshot view applies every ordered Band Shape point to the Band LineRenderer.
- [x] Slingshot view does not assume a middle point is the Pull Point.
- [x] Existing three-point assumptions in tests are removed or migrated to ordered-polyline assertions.
- [x] Existing Pull Hint and Touch Indicator behavior remains unchanged.
- [x] Launch math remains based on Pull distance and Pull Offset, not Band Shape point count.

## Verification

- EditMode tests: Band Shape rejects invalid point data; view receives and applies full ordered polyline; controller launch request data is unchanged by visual point count.
- PlayMode tests: smoke test verifies the Band LineRenderer uses the complete ordered polyline rather than exactly three points.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: enter Play Mode and confirm rest/pull Band visuals still render after the representation migration.
- Package version/changelog: no package/changelog change.

## Blocked by

- 01 - Add Held Target Pull Positioning And Launch Handoff
