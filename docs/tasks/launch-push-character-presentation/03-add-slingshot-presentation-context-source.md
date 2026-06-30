## Parent

[Launch Push Character Presentation PRD](../../prd/prd-launch-push-character-presentation.md)

## What to build

Add the slingshot presentation read model consumed by Character presentation. This slice should introduce a slingshot presentation context source that subscribes to active pull facts, capture lifecycle edges, and accepted launch notification, then exposes one immutable current context value.

The context source owns launch-push lifetime. It should start launch push at elapsed zero when an accepted launch is applied, tick elapsed time through injected time, preserve normalized launch power and launch offset from the accepted release, and clear inactive channels deterministically. It should not choose Character presentation modes.

This slice should register the slingshot-owned services through the slingshot composition boundary and avoid adding pure slingshot references to the gameplay lifetime scope.

## Acceptance criteria

- [ ] A slingshot presentation context source contract exposes one immutable current context value.
- [ ] The current context carries active-pull presence, normalized pull strength, normalized pull offset, launch-push presence, launch-push elapsed seconds, normalized launch power, and normalized launch offset.
- [ ] Active pull events update live pull fields without starting launch push.
- [ ] Active pull cleared and capture disabled clear live pull fields.
- [ ] Accepted launch starts launch push at elapsed zero and freezes normalized launch power and normalized launch offset from the accepted release.
- [ ] Launch-push elapsed time advances through injected time while launch push is active.
- [ ] Capture enabled clears both live pull and launch-push context.
- [ ] Capture disabled does not end launch push by itself.
- [ ] Inactive pull and launch channels are zeroed in the exposed current context.
- [ ] The context source does not select Character presentation mode and does not reference Animator, Character view, or Character presenter concepts.
- [ ] Slingshot service registrations are owned by the slingshot installer or equivalent slingshot composition boundary.
- [ ] No launch physics, band recoil lifetime, camera, collider, prefab hierarchy, package, or save-format behavior is changed in this slice.

## Verification

- EditMode tests:
  - Context source tests cover active pull changed, active pull cleared, capture disabled, capture enabled, accepted launch, ticking elapsed time, inactive-channel zeroing, and immutable current values.
  - Accepted launch tests prove launch power and launch offset are frozen from the accepted release and remain distinct from live pull values.
  - Time tests use injected time and do not rely on Unity frame waits.
- PlayMode tests:
  - Composition test resolves the slingshot presentation context source contract from the container and verifies it is registered by the slingshot composition boundary.
- Static checks:
  - Rider reformat and file-problem checks for changed C# and asmdef files.
  - Unity compile through the connector before tests.
  - `git diff --check`.
- Manual Unity smoke check:
  - Pull, release, and recapture flows remain visually unchanged until Character presentation consumes the new context.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 Expose Validated Active Pull Presentation Facts
- 02 Publish Slingshot Capture Lifecycle Edges
