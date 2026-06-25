# Implement Issue 05: Pull Release To LaunchRequested

## Summary

- Extend the Slingshot slice from issue 04 by converting a valid captured Pull Release into a pure `LaunchRequested` event.
- Keep this inside the existing Slingshot feature: no Gameplay Flow, no Rigidbody launch, no scene wiring, no haptics/audio/sequence work.
- Release always clears the Active Pull and restores idle visuals; only valid backward Pulls raise launch intent.

## Key Changes

- Add launch notification API in the Slingshot runtime assembly:
  - `ISlingshotLaunchNotifier` exposes `event SlingshotLaunchRequestedHandler LaunchRequested`.
  - `SlingshotLaunchRequestedHandler` carries one `SlingshotLaunchRequest` payload.
  - `SlingshotLaunchRequest` is immutable pure data and contains normalized power, Pull distance, signed Pull Offset, launch direction, launch speed,
    launch up direction, and launch up speed.
- Extend `SlingshotController`:
  - Implement `ISlingshotLaunchNotifier`.
  - Keep all issue 04 capture, projection, clamping, visual reset, and state-gating behavior.
  - On captured pointer release, recompute the release Pull sample through `ISlingshotInputProjector`.
  - If projection fails, clear Active Pull and return to capture idle without raising `LaunchRequested`.
  - If effective backward Pull distance is below `SlingshotConfig.MinPullDistance`, clear Active Pull and return to capture idle without raising.
  - For valid releases, build one launch request, reset visuals to capture idle, then raise `LaunchRequested`.
- Reuse issue 04 Pull math for request construction:
  - `PullDistance` is clamped backward distance in world units.
  - `PullOffset` is signed lateral displacement in world units, positive to Launch Frame right.
  - Forward-only movement clamps to zero backward distance and therefore cannot produce launch power.
  - Backward distance and lateral offset are clamped independently before request creation.
- Compute request values from config and Launch Frame:
  - `NormalizedPower = InverseLerp(MinPullDistance, MaxPullDistance, PullDistance)`.
  - Evaluate `LaunchSpeedCurve` with normalized power, clamp the curve result to `0..1`, then lerp from min to max launch speed.
  - Normalize lateral steering from `PullOffset / MaxLateralPull`, clamp to `-1..1`, and rotate Launch Frame forward by
    `steering * MaxLaunchAngle` around Launch Frame up.
  - If max lateral Pull is zero, treat steering as zero to avoid invalid math.
  - Normalize `LaunchDirection` and `LaunchUpDirection` by contract.
  - Copy `LaunchUpSpeed` from `SlingshotConfig` without coupling it to forward launch speed.
- Update `SlingshotInstaller`:
  - Register `ISlingshotLaunchNotifier` as the same `SlingshotController` instance.
  - Do not inject MonoBehaviours and do not add Gameplay Flow subscription in this issue.
- Preserve slice boundaries:
  - Canceled Pulls never raise launch requests.
  - Non-captured pointer releases are ignored.
  - Repeated release events after Active Pull is cleared do nothing.
  - Slingshot does not swallow subscriber exceptions; broken launch consumers should remain visible.

## Test Plan

- EditMode tests use local fakes for Slingshot view, Unity Input, Gameplay State, and input projector.
- Cover valid release:
  - A captured pointer release raises exactly one request.
  - Payload includes expected normalized power, Pull distance, Pull Offset, launch direction, launch speed, up direction, and up speed.
  - Band Shape and Touch Indicator return to idle when the release completes.
- Cover invalid release:
  - Weak Pull below the configured minimum raises no event.
  - Forward-only Pull raises no event.
  - Canceled Pull raises no event.
  - Release projection failure raises no event and restores idle.
  - Other pointer release does not affect the active pointer and does not raise a request.
- Cover launch math:
  - Normalized power clamps between min and max Pull distance.
  - Launch speed uses the configured speed curve and speed range.
  - Lateral Pull Offset changes launch direction sign without changing power.
  - Lateral Pull Offset clamps at the configured max launch angle.
  - Launch up direction and launch up speed are included independently from forward speed.
- Cover installer registration:
  - Resolving `ISlingshotLaunchNotifier` returns the same controller instance that owns the release logic.
- Verification order for implementation: Rider reformat/problems, Unity compile via Unity AI Agent Connector, then targeted Slingshot EditMode tests.
- Manual smoke: with a temporary local subscriber, pull/release in Editor and confirm request values are plausible in logs or debugger.

## Assumptions

- Issues 01-04 are implemented first: VContainer, Gameplay State, Unity Input, and Slingshot capture/pull visualization exist.
- `SlingshotConfig` already contains the launch tuning fields planned in issue 04.
- Broken launch tuning is caught by existing config validation; issue 05 does not add a second validation system.
- Gameplay Flow subscription, transition to Running, launch target hold/release, and Rigidbody launch application are deferred to issues 06 and 07.
- No `GameplayScene.unity` mutation is required here; issue 08 owns final scene composition.

## References

- [Issue 05: Add Pull Release To LaunchRequested](05-add-pull-release-to-launchrequested.md)
- [Issue 04 Implementation Plan](04-add-band-capture-and-pull-visualization-impl-plan.md)
- [Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)
- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
