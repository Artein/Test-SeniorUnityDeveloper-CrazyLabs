# Add Pull Plane Rest-Shape Clearance Gate

## Parent

[Slingshot Band Release Recoil Clearance PRD](../../prd/prd-slingshot-band-release-recoil-clearance.md)

## Type

AFK

## User stories covered

11, 27-30, 33-36, 47, 51, 54-56

## What to build

Add a production detach gate that answers whether the rest/idle/default Band Shape is clear of the current Launch Target Silhouette in the Pull Plane, including visible Band radius. The gate should let Band Release Recoil ask whether detachment is safe without moving launch, recoil lifecycle, or Unity Collider responsibilities into the geometry policy.

The visible Band radius should come from a narrow runtime surface that reflects rendered Band thickness. Prefer an existing view/config boundary if it can expose the value cleanly. If the implementation needs a new serialized authoring field, public API, save format, package dependency, or scene migration, stop and ask before proceeding.

## Acceptance criteria

- [ ] The detach candidate is the rest/idle/default Band Shape, not the current virtual recoil Pull Point by itself.
- [ ] Clearance is measured in the Pull Plane against the current Launch Target Silhouette.
- [ ] Visible Band radius is included in the clearance margin.
- [ ] The controller can ask for clear/blocked clearance without depending on LineRenderer internals.
- [ ] Unity Collider access remains behind the existing Launch Target Silhouette boundary.
- [ ] If a pure clearance policy module is introduced, its clear, blocked, and boundary cases are directly EditMode-testable.
- [ ] Existing Band Shape point count, Band Wrap sample count, launch power, launch direction, and launch handoff semantics are unchanged.
- [ ] No public API, serialization, package, save format, ProjectSettings, or scene migration is added without explicit approval.

## Verification

- EditMode tests: clearance policy clear case, blocked case, visible-radius boundary case, and any visible Band radius source contract if a new pure policy/runtime surface is introduced.
- PlayMode tests: not required for this slice unless the radius source can only be validated through rendered scene behavior.
- Static checks: Rider reformat/problems on changed code files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: not required for this slice unless a serialized authoring surface is changed.
- Package version/changelog: no package/changelog change.

## Blocked by

None - can start immediately
