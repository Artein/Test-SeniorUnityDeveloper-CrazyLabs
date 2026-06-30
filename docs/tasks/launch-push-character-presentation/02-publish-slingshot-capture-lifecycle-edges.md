## Parent

[Launch Push Character Presentation PRD](../../prd/prd-launch-push-character-presentation.md)

## What to build

Publish slingshot capture lifecycle edges as a separate slingshot-owned contract. Character presentation needs to know when capture has become enabled again and when capture has been disabled for launch or release handling, but those edges should not be mixed into active pull value events.

This slice should add a capture lifecycle notifier with payload-free capture enabled and capture disabled events. Events should be emitted only on real capture state edges, after the relevant slingshot state changes have been committed, and through safe invocation.

This slice should not add Character presentation modes, launch-push timers, Animator parameters, scene references, or gameplay physics changes.

## Acceptance criteria

- [ ] A slingshot-owned capture lifecycle notifier contract exposes capture enabled and capture disabled events without payload.
- [ ] Capture enabled is emitted only after capture setup is committed and consumers can observe stable rest geometry and target hold state.
- [ ] Capture disabled is emitted exactly once when capture actually transitions to disabled.
- [ ] Capture disabled covers launch handoff and release-recoil early-exit paths that genuinely disable capture.
- [ ] Same-state capture calls do not emit lifecycle events.
- [ ] Capture lifecycle events stay separate from active pull changed and active pull cleared events.
- [ ] New lifecycle events use the project safe-invocation extension.
- [ ] Existing launch validation, launch request application, target hold, band recoil, and touch indicator behavior remain unchanged.
- [ ] No Character presenter, Character view, Animator Controller, scene wiring, package, or save-format change is introduced in this slice.

## Verification

- EditMode tests:
  - Capture lifecycle notifier tests cover enable, disable, launch disable, early-exit disable, and same-state no-op behavior.
  - Event ordering tests prove capture enabled is observed after setup is committed and capture disabled is not duplicated.
  - Subscriber-failure tests, if there is an existing safe-invocation testing pattern, prove one throwing subscriber does not stop later subscribers.
- PlayMode tests:
  - None expected unless existing capture lifecycle can only be proven through Unity object composition.
- Static checks:
  - Rider reformat and file-problem checks for changed C# files.
  - Unity compile through the connector before tests.
  - `git diff --check`.
- Manual Unity smoke check:
  - Pull, cancel, weak-release, and valid-release flows still look identical before later Character animation work is added.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
