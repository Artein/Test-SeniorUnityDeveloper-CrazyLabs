# Implement Issue 02: Asset-Backed Gameplay State

## Summary

- Add reusable Gameplay State under `Assets/Game/Gameplay/GameplayState`.
- Use ScriptableObject state ids and transition assets compared by reference identity.
- Add pure model/service/validator logic with typed changing/changed events.
- Add initial state/config assets under `Assets/Game/Gameplay/States`.
- Assume issue 01 is complete: VContainer is installed and the root gameplay composition spine exists.

## Key Changes

- Create runtime/test asmdefs:
  - `Game.Gameplay.GameplayState.asmdef`
  - `Game.Gameplay.GameplayState.Tests.EditMode.asmdef`
- Add public state assets/types:
  - `GameplayStateId : ScriptableObject`, no secondary id fields, no equality override.
  - `GameplayStateTransition : ScriptableObject`, serialized `FromStateId` and `ToStateId`.
  - `GameplayStateConfig : ScriptableObject, IGameplayStateConfig`, serialized initial state and transition list, with `OnValidate` warnings.
- Add runtime API:
  - `IGameplayStateService` exposes `CurrentStateId`, `IsCurrent(GameplayStateId)`, `TryTransitionTo(GameplayStateId)`, `GameplayStateChanging`, and `GameplayStateChanged`.
  - Use delegates:
    `public delegate void GameplayStateChangingHandler(GameplayStateId nextStateId, GameplayStateId previousStateId);`
    `public delegate void GameplayStateChangedHandler(GameplayStateId nextStateId, GameplayStateId previousStateId);`
  - Add internal `IGameplayStateModel` / `GameplayStateModel`; only the service mutates current state.
- Add validation:
  - `IGameplayStateValidator` / `GameplayStateValidator`.
  - Stable typed errors via `GameplayStateValidationErrorCode` and `GameplayStateValidationError`.
  - Catch null config, missing initial state, null transition, missing from/to, self-transition, and duplicate from/to pairs.
  - Do not validate reachability or complete flow coverage.
- Add behavior:
  - Service construction validates config and initializes model from config initial state.
  - Valid transition raises `GameplayStateChanging`, mutates model, then raises `GameplayStateChanged`.
  - Same-state transition returns `false`, emits no events, and logs no warning.
  - Invalid transition, including null target, returns `false`, keeps current state, and logs a Unity warning.
  - `GameplayStateChanging` exceptions propagate and prevent mutation/changed event.
- Add VContainer feature installer:
  - `GameplayStateInstaller : IInstaller`, constructed with `IGameplayStateConfig`.
  - Registers config, validator, model, service, and `IGameplayStateService`.
  - Do not inject MonoBehaviours and do not wire the Gameplay Scene in this issue.
- Add authored assets:
  - `PreLaunchStateId.asset`
  - `RunningStateId.asset`
  - `RunEndedStateId.asset`
  - `PreLaunchToRunningTransition.asset`
  - `RunningToRunEndedTransition.asset`
  - `RunEndedToPreLaunchTransition.asset`
  - `GameplayStateConfig.asset`

## Test Plan

- EditMode tests for validator:
  - reports missing initial state.
  - reports null transition entries.
  - reports missing from/to state ids.
  - reports self-transition.
  - reports duplicate transition pair.
- EditMode tests for service:
  - initializes current state from config.
  - valid transition emits changing before mutation and changed after mutation.
  - same-state transition is a silent no-op.
  - invalid transition logs warning and preserves current state.
  - changing subscriber exception propagates and blocks mutation.
- EditMode test for installer:
  - build a VContainer container with transient test config and resolve `IGameplayStateService`.
- Use transient ScriptableObject test instances and explicit `#if UNITY_INCLUDE_TESTS` partial test hooks; no reflection and no hardcoded asset paths in tests.
- Verification order: Rider reformat/problems, Unity compile via Unity AI Agent Connector, then targeted EditMode tests.
- Manual smoke: inspect authored state/config assets and confirm all references are explicit and valid.

## Assumptions

- Issue 01 has already added VContainer and the root gameplay assembly.
- This issue creates the reusable Gameplay State feature and assets only; scene LifetimeScope wiring can happen in later flow/scene issues.
- Existing Unity logging is used; structured logging remains deferred.
- No PlayMode tests or package/changelog changes are required for this slice.

## References

- [Issue 02: Add Asset-Backed Gameplay State](02-add-asset-backed-gameplay-state.md)
- [Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)
- [ADR-0001: Use Asset-Backed Gameplay State Ids](../../adr/adr-0001-use-asset-backed-gameplay-state-ids.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
