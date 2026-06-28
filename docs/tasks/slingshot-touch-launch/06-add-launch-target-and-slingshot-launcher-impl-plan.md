# Implement Issue 06: Launch Target And Slingshot Launcher

## Summary

- Add the Slingshot launch-application boundary: `ISlingshotLauncher` consumes a valid `SlingshotLaunchRequest`, computes final velocity, and delegates physics application to `ILaunchTarget`.
- Add a shallow `RigidbodyLaunchTarget` MonoBehaviour adapter that handles hold/release mechanics around an explicitly assigned `Rigidbody`.
- Keep Gameplay Flow authorization out of this slice. Issue 07 will decide when launch is allowed and when Slingshot capture is enabled; this slice applies launch when called.

## Key Changes

- Add launch APIs in the Slingshot runtime assembly:
  - `ISlingshotLauncher` with `void Launch(SlingshotLaunchRequest request)`.
  - `ILaunchTarget` with `void Hold()` and `void Launch(Vector3 velocity)`.
  - Follow repo interface/file rule: keep each interface in the same file as its single production implementation until a second production implementation exists.
- Add `SlingshotLaunchController : ISlingshotLauncher, IInitializable, IDisposable`:
  - Depends on `ILaunchTarget`/`IHeldLaunchTarget`.
  - Does not subscribe to Gameplay State and does not hold the target on state changes.
  - Does not call `TryTransitionTo`, does not inspect Running state, and does not duplicate Gameplay Flow authorization.
- Implement `Launch(SlingshotLaunchRequest request)`:
  - Validate launch-relevant request data: finite normalized direction/up direction, approximately unit direction/up direction, finite non-negative launch speeds, and finite non-zero final velocity.
  - Compute final velocity as `request.LaunchDirection * request.LaunchSpeed + request.LaunchUpDirection * request.LaunchUpSpeed`.
  - On invalid request or invalid final velocity, log a warning with existing Unity logging and skip `ILaunchTarget.Launch`.
  - On valid request, call `ILaunchTarget.Launch(finalVelocity)` and let target exceptions propagate.
- Add `RigidbodyLaunchTarget : MonoBehaviour, ILaunchTarget`:
  - Serialized explicit `Rigidbody _rigidbody`; no dynamic lookup and no `Reset()` auto-wiring.
  - `OnValidate` reports missing reference diagnostics.
  - Before dereferencing serialized references, use fully-qualified `UnityEngine.Assertions.Assert`.
  - `Hold()` is idempotent: on first hold, save previous `isKinematic` and `constraints`; set `isKinematic = true`; clear `linearVelocity` and `angularVelocity`.
  - Repeated `Hold()` while already held must not overwrite the saved previous state.
  - `Launch(Vector3 velocity)` restores saved `isKinematic` and `constraints` if currently held, clears `linearVelocity` and `angularVelocity`, marks the target unheld, then applies `AddForce(velocity, ForceMode.VelocityChange)`.
  - If `Launch()` is called without a prior `Hold()`, do not restore state; still clear stale velocities and apply the velocity change.
- Extend Slingshot composition:
  - Register `SlingshotLaunchController` as the concrete instance behind `ISlingshotLauncher`, plus its VContainer lifecycle interfaces.
  - Register the scene-provided `ILaunchTarget` as an existing interface instance later through the gameplay LifetimeScope.
  - Do not mutate `GameplayScene.unity` in this issue; Player adapter assignment belongs to issue 08.

## Test Plan

- Add Slingshot EditMode tests with local fakes for `ILaunchTarget`/`IHeldLaunchTarget`:
  - Initialize does not hold the target; capture owns hold behavior.
  - Dispose does not mutate gameplay state or launch target.
  - Valid request calls target launch with the exact computed final velocity.
  - Invalid direction, invalid speed, zero/non-finite final velocity warn and skip target launch.
  - Launcher has no Gameplay State dependency and never authorizes transitions.
  - Target launch exception propagates for a valid request.
- Add focused Rigidbody adapter tests:
  - Prefer PlayMode tests for the real `RigidbodyLaunchTarget` because it exercises Unity physics APIs.
  - Verify hold saves/restores `isKinematic` and `constraints`, sets kinematic true, and clears linear/angular velocity.
  - Verify repeated hold does not overwrite the originally saved state.
  - Verify launch after hold restores saved state, clears stale velocity, and applies `ForceMode.VelocityChange`.
  - Verify launch without hold is deterministic: no saved-state restore, stale velocity cleared, velocity change applied.
- Verification order for implementation:
  - Rider reformat/problems for changed files.
  - Unity compile through Unity AI Agent Connector.
  - Targeted Slingshot EditMode tests.
  - Targeted Slingshot PlayMode adapter tests if added.

## Assumptions

- Implement after issue 05 so `SlingshotLaunchRequest` already exists; do not create a duplicate launch request type.
- `SlingshotLaunchRequest` directions are normalized by producer contract, but the launcher still validates them defensively.
- Unity 6 Rigidbody API uses `linearVelocity`; mass-agnostic launch uses `Rigidbody.AddForce(Vector3, ForceMode.VelocityChange)`.
- Existing Unity logging is used for warnings; no structured logging package is introduced.
- Scene wiring, Player assignment, and full playable smoke testing stay in issue 08.
