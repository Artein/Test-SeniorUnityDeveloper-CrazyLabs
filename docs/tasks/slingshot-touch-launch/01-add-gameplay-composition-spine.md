# Add Gameplay Composition Spine

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

## Type

AFK

## User stories covered

53, 54, 57, 60

## What to build

Create the minimal gameplay composition spine required for the Slingshot feature to be implemented in later slices. The slice should introduce VContainer as the project DI package, add the root gameplay runtime/test assembly structure, and provide a scene-level gameplay `LifetimeScope` scaffold that can register explicit scene references without injecting MonoBehaviours.

This slice should not add gameplay rules yet. Its value is a compile-clean, testable composition baseline that future issues can extend without repeatedly changing package/bootstrap decisions.

## Acceptance criteria

- [ ] VContainer is added as a pinned package dependency and the package lock is updated.
- [ ] Root gameplay runtime and EditMode test assemblies exist and compile.
- [ ] A scene-level gameplay `LifetimeScope` scaffold exists for explicit serialized references and composition-only registration.
- [ ] The scope validates missing required references through authoring/runtime validation without dynamic scene searches.
- [ ] MonoBehaviour views/adapters are not injected and do not receive VContainer lifecycle responsibilities.
- [ ] The implementation reflects ADR vocabulary around VContainer composition and shallow MonoBehaviours.

## Verification

- EditMode tests: a small composition/bootstrap test where practical for registration behavior that does not require scene loading.
- PlayMode tests: none required for this slice.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: open the Gameplay Scene and confirm the new scope can be assigned without missing-script errors.
- Package version/changelog: package manifest and package lock include the pinned VContainer dependency; no project changelog required.

## Blocked by

None - can start immediately
