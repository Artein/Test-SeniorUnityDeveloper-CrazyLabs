# Add Asset-Backed Gameplay State

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

## Type

AFK

## User stories covered

38-43, 55, 57, 60

## What to build

Add the reusable Gameplay State feature using ScriptableObject state ids and transition assets. A runtime service should keep the current state in a model, validate authored config, expose transition/query APIs, and raise typed changing/changed events.

This slice should also create the initial Pre-Launch, Running, and Run Ended state assets plus the allowed transition assets/config needed by the Slingshot flow. The result should be usable by later slices without hardcoded state enums or strings.

## Acceptance criteria

- [x] Gameplay State has its own runtime assembly and EditMode test assembly.
- [x] State ids are ScriptableObject assets compared by reference identity.
- [x] Transition assets declare one allowed from/to state pair.
- [x] A config asset provides initial state and allowed transitions, with `OnValidate` diagnostics.
- [x] A pure validator reports nulls, self-transitions, and duplicate transitions with stable typed errors.
- [x] The service exposes current state, `IsCurrent`, `TryTransitionTo`, `GameplayStateChanging`, and `GameplayStateChanged`.
- [x] Valid transitions raise changing, mutate model, then raise changed.
- [x] Same-state transitions return false with no events and no warning.
- [x] Invalid transitions return false and warn with existing Unity logging.
- [x] Changing subscriber exceptions are logged and isolated so transition mutation can complete.
- [x] A VContainer installer registers config, model, service, and public service interface.

## Verification

- EditMode tests: initial state construction; valid transition event order; same-state no-op; invalid transition warning; changing exception blocks mutation; validator null/self/duplicate errors.
- PlayMode tests: none required for this slice.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: inspect the authored state/config assets and confirm references are explicit and valid.
- Package version/changelog: no package/changelog change beyond the VContainer dependency from issue 01.

## Blocked by

- 01 - Add Gameplay Composition Spine
