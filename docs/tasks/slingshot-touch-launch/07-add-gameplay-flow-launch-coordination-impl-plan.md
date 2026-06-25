# Implement Issue 07: Gameplay Flow Launch Coordination

## Summary

- Add the game-specific `GameplayFlowController` in the root gameplay assembly.
- Listen to Slingshot `LaunchRequested`, ask `IGameplayStateService` to transition to the configured Running `GameplayStateId`, and invoke `ISlingshotLauncher` only when the transition succeeds.
- Keep this slice coordination-only: no launch payload validation, Rigidbody physics, player hold logic, Slingshot state mutation, or `GameplayScene.unity` wiring.

## Key Changes

- Add `GameplayFlowController : IInitializable, IDisposable`:
  - Constructor dependencies: `ISlingshotLaunchNotifier`, `IGameplayStateService`, `ISlingshotLauncher`, and Running `GameplayStateId`.
  - `Initialize()` subscribes to `LaunchRequested`.
  - `Dispose()` unsubscribes only.
  - The launch-request handler calls `TryTransitionTo(_runningStateId)` before launching.
  - If transition returns `false`, skip launch silently and rely on Gameplay State service warnings for invalid transitions.
  - If transition returns `true`, call `_slingshotLauncher.Launch(request)`.
  - Launcher exceptions propagate; do not roll back Gameplay State.
- Add composition support through the gameplay composition spine:
  - Register `GameplayFlowController` as its concrete instance plus VContainer lifecycle interfaces.
  - Keep registration in gameplay composition code or a small `GameplayFlowInstaller` if that matches the implemented installer pattern.
  - Accept the Running state id as an explicit configured dependency.
  - Do not inject MonoBehaviours and do not perform scene searches.
- Preserve ownership boundaries:
  - Slingshot remains an intent producer through `ISlingshotLaunchNotifier`.
  - Gameplay State remains the source of transition legality.
  - `ISlingshotLauncher` remains the launch application boundary.
  - Issue 08 owns final scene assignment and full playable smoke wiring.

## Test Plan

- Add EditMode tests for `GameplayFlowController` with local fakes:
  - `Initialize_WhenLaunchRequested_TransitionsToRunningBeforeLaunching`.
  - `LaunchRequested_WhenTransitionFails_DoesNotLaunch`.
  - `LaunchRequested_WhenLauncherThrows_PropagatesAfterSuccessfulTransition`.
  - `Dispose_WhenLaunchRequestedAfterDispose_DoesNotTransitionOrLaunch`.
  - `LaunchRequested_DoesNotRollback_WhenLauncherThrows`.
  - `LaunchRequested_UsesConfiguredRunningStateId`.
- Add a composition test if practical without scene loading:
  - Container resolves and initializes `GameplayFlowController` from fake Slingshot notifier, fake Gameplay State service, fake launcher, and Running state id.
  - No MonoBehaviour injection required.
- Verification order for implementation:
  - Rider reformat/problems on changed C# and asmdef files.
  - Unity compile through Unity AI Agent Connector.
  - Targeted Gameplay Flow EditMode tests after compile is clean.
  - No PlayMode tests required for this slice.

## Assumptions

- Issues 02, 05, and 06 already provide `IGameplayStateService`, `GameplayStateId`, `ISlingshotLaunchNotifier`, `SlingshotLaunchRequest`, and `ISlingshotLauncher`.
- The controller lives under the root gameplay assembly because Gameplay Flow is game-specific orchestration rather than reusable Slingshot or Gameplay State logic.
- The Running state id is configured explicitly by composition; no state enum, string lookup, asset path lookup, or scene search is introduced.
- Failed transition means "do not launch"; it is not an error at the flow-controller level.

## References

- [Issue 07: Add Gameplay Flow Launch Coordination](07-add-gameplay-flow-launch-coordination.md)
- [Issue 02 Implementation Plan](02-add-asset-backed-gameplay-state-impl-plan.md)
- [Issue 05 Implementation Plan](05-add-pull-release-to-launchrequested-impl-plan.md)
- [Issue 06 Implementation Plan](06-add-launch-target-and-slingshot-launcher-impl-plan.md)
- [Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)
- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
