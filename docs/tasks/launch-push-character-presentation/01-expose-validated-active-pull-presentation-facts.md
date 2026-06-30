## Parent

[Launch Push Character Presentation PRD](../../prd/prd-launch-push-character-presentation.md)

## What to build

Expose validated active-pull presentation facts from the slingshot layer. This slice should add a slingshot-owned active pull notifier, a small active pull context, and a shared pull-offset normalizer so later Character presentation slices can react to live pull state without reading raw pointer input or slingshot view state.

The notifier should publish only after slingshot validation has accepted a live pull as presentation-worthy. The first-slice context should carry normalized pull strength and signed normalized pull offset. The pull-offset normalizer should own the side-specific effective lateral normalization used by both live pull facts and later accepted-launch facts.

This slice should not add Character presentation modes, presenter dependencies, Animator parameters, launch-push timers, prefab wiring, or gameplay physics changes.

## Acceptance criteria

- [ ] A slingshot-owned active pull notifier contract exposes changed and cleared events for validated active pull presentation facts.
- [ ] The active pull context carries normalized pull strength and signed normalized pull offset, and does not expose raw pointer input, band shape, touch indicator position, or view geometry.
- [ ] Active pull changed is raised only after the slingshot has validated a live pull enough to update active slingshot presentation.
- [ ] Active pull cleared is raised when a previously live validated pull is canceled, becomes invalid, or releases too weakly to launch.
- [ ] Ignored pointer input with no live active pull does not produce redundant clear events.
- [ ] Valid launch release does not clear pull anticipation through the weak-or-invalid clear path before launch-push facts can be produced by later slices.
- [ ] New notifier events use the project safe-invocation extension.
- [ ] A shared pull-offset normalizer maps centered pull to 0, full effective left to -1, and full effective right to 1.
- [ ] Pull-offset normalization respects side-specific effective limits and depth-based lateral ramping instead of dividing by raw maximum lateral pull alone.
- [ ] Normalization handles zero or near-zero effective lateral range without producing invalid floating point values.
- [ ] No Character presenter, Character view, Animator Controller, scene wiring, launch physics, or band recoil behavior changes are introduced in this slice.

## Verification

- EditMode tests:
  - Pull-offset normalizer covers centered, left, right, asymmetric limits, lateral ramping, clamped extremes, shallow pull depth, and zero-range guard cases.
  - Active pull notifier tests cover changed, cleared, no-op ignored input, invalid pull, weak release, and valid release ordering behavior through the public slingshot-facing contract.
- PlayMode tests:
  - None expected unless existing slingshot behavior can only be observed through Unity object lifecycle.
- Static checks:
  - Rider reformat and file-problem checks for changed C# files.
  - Unity compile through the connector before tests.
  - `git diff --check`.
- Manual Unity smoke check:
  - Pull the slingshot in Play Mode and confirm existing pull visuals and release behavior are not visibly changed by event publication.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
