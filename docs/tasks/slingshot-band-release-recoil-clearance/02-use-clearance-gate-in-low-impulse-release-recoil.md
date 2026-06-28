# Use Clearance Gate In Low-Impulse Release Recoil

## Parent

[Slingshot Band Release Recoil Clearance PRD](../../prd/prd-slingshot-band-release-recoil-clearance.md)

## Type

AFK

## User stories covered

1-10, 12-23, 27-29, 31-40, 42-46, 48, 50-56

## What to build

Start with the low-impulse scene regression, then update Band Release Recoil so a valid small Pull Release cannot show the Band passing through the launched Launch Target. During recoil, the virtual Pull Point still relaxes from the final launch Pull Point toward the Rest Point and drives Pulled Side selection. The moving Launch Target Silhouette continues to supply Band Contact Points and Band Wrap geometry until the rest/idle/default Band Shape is clear.

If visual recoil progress reaches its end before clearance is proven, keep the Band in a safe collider-aware visual state and retry clearance on later ticks. If clearance is proven before the visual recoil curve has completed, preserve the intended recoil feel instead of visibly skipping straight to detached idle.

## Acceptance criteria

- [ ] A PlayMode regression pulls just above MinimumPullDistance, releases, samples rendered Band Shape for multiple recoil frames, and fails against the old pass-through behavior.
- [ ] The PlayMode regression asserts rendered Band Shape segments stay outside the real Launch Target Collider by rendered Band radius plus a small margin.
- [ ] Valid Pull Release still keeps the loaded Band Shape through launch request notification and launch handoff.
- [ ] Band Release Recoil starts only after launch application.
- [ ] During unsafe or unknown-clearance recoil, live collider-aware Band Shape solving remains preferred.
- [ ] During recoil, virtual Pull Point drives Pulled Side; the launched target midpoint, bounds center, and Rigidbody position do not become the solver query point.
- [ ] Simple two-span Band Shape and detached rest geometry are blocked while target clearance is unsafe or unknown.
- [ ] Recoil progress reaching the end does not detach the Band unless clearance has also been proven.
- [ ] After visual recoil is ready and rest/idle/default Band Shape clearance is proven, the Band may detach and stop querying Launch Target contact/wrap geometry.
- [ ] Launch power, launch direction, Pull Offset, Pull Distance, MinimumPullDistance, and Rigidbody velocity-change behavior are unchanged.

## Verification

- EditMode tests: existing launch request and Slingshot controller tests for loaded handoff, launch data, weak release, and deep recoil remain green; add controller-level coverage only if the behavior can be observed through public/fake dependencies without reflection.
- PlayMode tests: low-valid-pull multi-frame rendered Band Shape clearance regression; existing Gameplay Scene natural Band Shape coverage remains green.
- Static checks: Rider reformat/problems on changed code files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: shallow valid launch no longer shows Band pass-through; deeper launch still reads normally.
- Package version/changelog: no package/changelog change.

## Blocked by

- 01 - Add Pull Plane Rest-Shape Clearance Gate
