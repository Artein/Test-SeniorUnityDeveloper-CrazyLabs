# Run Body Movement Controller Engineering Notes

Detailed orchestration, configuration migration, and steering-input decisions supporting the [high-level actor and runtime diagram](./run-body-speed-model.md).

## Legacy Steering Config Split

The current broad `IPlayerSteeringConfig` should not be carried forward as the runtime dependency for new movement code. It mixes unrelated consumers: steering input, turn control, steering-frame stability, post-launch lift correction, and defensive velocity validity.

Keep the designer experience unified through one `RunBodyMovementConfig` asset, but expose consumer-specific interfaces:

```csharp
public sealed class RunBodyMovementConfig : ScriptableObject,
    IRunBodySpeedConfig,
    IRunBodyMovementValidityConfig,
    IRunLaunchLandingStabilizationConfig,
    IRunSteeringConfig,
    IRunSteeringFrameConfig
{
}
```

| Legacy responsibility | Narrow runtime view | Primary consumer | Example fields |
| --- | --- | --- | --- |
| Steering input metrics | `IRunSteeringConfig` | `IRunSteeringInputMetricsResolver` and `RunSteeringInputController` | `RunSteeringRangeCentimeters`, `RunSteeringDeadzoneFraction`, `RunSteeringResponsiveness`, `FallbackDpi`, `MinimumAcceptedDpi`, `MaximumAcceptedDpi` |
| Turn authority | `IRunSteeringConfig` | `IRunSteeringEvaluator` / `RunBodyMovementController` | `MaximumTurnDegreesPerSecond`, `RunAirSteeringMaximumTurnDegreesPerSecond`, `MinimumSteerSpeed` |
| Steering-frame stability | `IRunSteeringFrameConfig` | `RunSurfaceSteeringFrameSource` or equivalent frame source | normal slew rate, snap angle, ungrounded grace, suspect-normal confirmation |
| Post-launch lift correction | `IRunLaunchLandingStabilizationConfig` | `RunLaunchLandingStabilizer` | `LaunchLandingStabilizationSeconds`, `LaunchLandingMaximumLiftSpeed` |
| Movement validity and defensive sanity | `IRunBodyMovementValidityConfig` | `RunBodyMovementController` and sanity guard | `MaximumSupportedSurfaceNormalLiftSpeed`, `RunBodySpeedSanityGuardMetersPerSecond` |

This keeps designer controls in one inspector while preventing a speed evaluator, frame source, or launch-landing stabilizer from depending on settings it does not own.

First slice intentionally keeps steering input metrics and turn authority together under `IRunSteeringConfig`. Do not add `IRunSteeringInputConfig` and `IRunSteeringControlConfig` until there are separate consumers, separate test fakes, or meaningfully different lifetimes that make the split pay for itself.

`IRunSteeringConfig` should expose raw authored/read-only values only. Do not keep helper methods such as `ResolveRunSteeringDpi` or `ResolveRunSteeringRangePixels` on the config interface when moving to the new movement model. Those methods make the ScriptableObject a hidden logic owner.

The resolution rule belongs in explicit steering input logic:

```csharp
public interface IRunSteeringConfig
{
    float RunSteeringRangeCentimeters { get; }
    float RunSteeringDeadzoneFraction { get; }
    float RunSteeringResponsiveness { get; }
    float FallbackDpi { get; }
    float MinimumAcceptedDpi { get; }
    float MaximumAcceptedDpi { get; }
    float MaximumTurnDegreesPerSecond { get; }
    float RunAirSteeringMaximumTurnDegreesPerSecond { get; }
    float MinimumSteerSpeed { get; }
}

public readonly struct RunSteeringInputMetrics
{
    public float ResolvedDpi { get; }
    public float RangePixels { get; }
    public float DeadzoneFraction { get; }
}

public interface IRunSteeringInputMetricsResolver
{
    RunSteeringInputMetrics Resolve(float rawDpi);
}
```

Keep the first method input this narrow. `IRunSteeringConfig` is constructor-injected, so raw DPI is the only runtime fact needed to resolve first-slice steering input metrics. Do not add `RunSteeringInputMetricsContext` until the resolver genuinely needs multiple runtime facts, such as orientation, screen scale, safe area, input backend, or per-device overrides.

`RunSteeringInputController` can read raw DPI from the existing injected `IScreen` wrapper when a gesture begins and pass that raw value into the resolver. Do not introduce a narrower screen-metrics source only for raw DPI in the first slice; add one later if screen metrics become a broader shared dependency.

Example resolution behavior:

```text
IScreen.Dpi sampled on gesture begin
-> raw DPI + raw steering tuning
-> reject non-finite/out-of-bounds DPI
-> choose fallback DPI when needed
-> convert range centimeters to pixels
-> capture the resolved metrics when a steering gesture begins
```

The first implementation can be named `DefaultRunSteeringInputMetricsResolver` and receive `IRunSteeringConfig` through constructor injection. `RunSteeringInputController` calls the resolver when a gesture begins, then stores the resulting `RunSteeringInputMetrics` for that gesture.

`RunSteeringInputMetrics.ResolvedDpi` is intentionally included even though gesture mapping primarily uses `RangePixels` and `DeadzoneFraction`. It gives tests and diagnostics a direct answer for why a physical steering range resolved differently on a device. It should not become a gameplay branch condition.

The resolved metrics should be captured once per active gesture. Do not recalculate `RangePixels` or `DeadzoneFraction` during pointer movement, because that can make one continuous drag change sensitivity halfway through the gesture. Device DPI refreshes or config changes should affect the next gesture.

Keep this resolver pure:

- It may read injected `IRunSteeringConfig` values.
- It may receive the raw DPI sample as method input.
- It may clamp deadzone and convert centimeters to pixels.
- It must not read `IScreen` or Unity `Screen`, own gesture state, subscribe to input, smooth steering, write Rigidbody motion, or evaluate fixed-step steering.

This keeps designer-authored values in one place while keeping device-metric interpretation directly testable.

## Run Steering Input State Snapshot

`RunSteeringInputController` should expose one `RunSteeringInputState` snapshot to **Run Body Movement Controller**.

Avoid exposing several read-only properties such as `IsGestureActive`, `DesiredSteer`, `SmoothedSteer`, and captured metrics directly from the controller. The properties may look read-only to consumers, but they still describe mutable controller state. A fixed-step movement pass can accidentally read a mixed state if input callbacks update the controller between property reads.

With a snapshot, **Run Body Movement Controller** owns the fixed-step input ordering:

```text
RunBodyMovementController fixed pass
-> IRunSteeringInputSource.AdvanceAndRead(fixedDeltaTime)
-> RunSteeringInputController updates SmoothedSteer from DesiredSteer
-> returns RunSteeringInputState
-> combine with corrected velocity, steering frame, steering mode, and steering tuning
-> build RunSteeringContext
-> evaluate RunSteeringDecision
```

The snapshot is input intent state only. It does not evaluate steering direction, target rotation, final velocity, speed, or Rigidbody writes.

Example:

```text
DesiredSteer jumps to 1.0 during a touch drag
fixed pass 1 -> SmoothedSteer 0.2
fixed pass 2 -> SmoothedSteer 0.4
fixed pass 3 -> SmoothedSteer 0.6
```

Those smoothing values are owned by `RunSteeringInputController`, but the movement step decides when the smoothing advances and when the snapshot is consumed. That makes movement composition deterministic without needing a separate fixed-tick registration for input smoothing.

First-slice interface:

```csharp
public interface IRunSteeringInputSource
{
    RunSteeringInputState AdvanceAndRead(float fixedDeltaTime);
}
```

`AdvanceAndRead(...)` intentionally combines the two operations. Splitting it into `Advance(...)` plus `Current` would let the movement pass accidentally read stale input state, read twice in one movement pass, or advance smoothing after building `RunSteeringContext`.

Lifecycle split:

```text
IInitializable.Initialize
-> RunSteeringInputController subscribes to pointer, gameplay lifecycle, and launch-applied events

LaunchApplied arrives for the current run
-> RunSteeringInputController marks local launch readiness for steering input
-> RunBodyMovementController stores fallback LaunchUpDirection for movement composition
-> RunBodyMovementController calls RunLaunchLandingStabilizer.ArmForLaunch()

Running is current and launch readiness is true
-> RunSteeringInputController enables input and resets pointer/smoothing state

Running is current and movement launch readiness is true
-> RunBodyMovementController resets Run Steering Frame with fallback LaunchUpDirection

RunBodyMovementController fixed pass
-> calls RunLaunchLandingStabilizer.Stabilize(...) after Run Body Speed Sanity Guard
-> calls IRunSteeringInputSource.AdvanceAndRead(fixedDeltaTime)
-> reads Run Steering Frame Source with fallback LaunchUpDirection
-> builds RunSteeringContext.SteeringUp

Running exits, run ends, or controller disposes
-> RunSteeringInputController clears launch readiness, resets pointer/smoothing state, and disables input
-> RunBodyMovementController clears movement-side launch readiness and fallback LaunchUpDirection
-> RunBodyMovementController calls RunLaunchLandingStabilizer.Reset()
```

This readiness is local controller state in the first slice. Do not add a shared `IRunLaunchReadiness` / `ILaunchAppliedState` style service until camera, run-end, lost-momentum, steering input, and movement composition need the same query semantics and the same reset boundary.

First-slice state shape:

```csharp
public readonly struct RunSteeringInputState
{
    public bool IsGestureActive { get; }
    public float DesiredSteer { get; }
    public float SmoothedSteer { get; }
    public bool HasCapturedMetrics { get; }
    public RunSteeringInputMetrics CapturedMetrics { get; }
}
```

`IsGestureActive` and `SmoothedSteer` are the movement-consumed fields. `DesiredSteer` lets diagnostics compare requested input against smoothed input. `CapturedMetrics` lets diagnostics explain physical steering range and deadzone for the active gesture; consumers should treat it as meaningful only when `HasCapturedMetrics` is true.

Inactive neutral values:

- `IsGestureActive = false`
- `DesiredSteer = 0f`
- `SmoothedSteer = 0f`
- `HasCapturedMetrics = false`
- `CapturedMetrics = default`

- In code, the model output should be named `RunBodySpeedDecision`.
- Diagnostics should use `RunBodySpeedDecisionContributors` as a flags-style contributor list, not a singular reason field.
- `RunBodySpeedDecisionContributors` is for tests and diagnostics, not for downstream gameplay branching.
- First-slice `RunBodySpeedDecision` should carry `TangentAcceleration`, `TangentDrag`, `LowSpeedAssistTargetSpeed`, `LowSpeedAssistAcceleration`, `SoftMaximumSpeed`, and `Contributors`.
- First-slice `RunBodySpeedDecision` should not carry final speed or final velocity; **Run Body Movement Controller** integrates the decision with steering, current velocity, and fixed delta time.
- First-slice `RunBodySpeedDecision` should not carry vertical acceleration, normal velocity, downforce, jump, or lift fields.
- `TangentAcceleration` and `TangentDrag` use meters per second squared; **Run Body Movement Controller** applies them over `fixedDeltaTime`.
- **Run Body Speed Model** should not multiply acceleration or drag by `fixedDeltaTime`.
- `LowSpeedAssistTargetSpeed` and `LowSpeedAssistAcceleration` remain separate because a target without a rate would require an instant floor or an implicit reuse of unrelated slope/slowdown tuning.
- `RunBodySpeedDecision.LowSpeedAssistAcceleration` is the effective, eligibility-scaled recovery rate; it is not folded into `TangentAcceleration` because **Run Body Movement Controller** must apply it only while the naturally integrated tangent speed remains below `LowSpeedAssistTargetSpeed`.
- **Run Body Movement Controller** first integrates `TangentAcceleration - TangentDrag`, then rate-limits any eligible recovery toward `LowSpeedAssistTargetSpeed` by `LowSpeedAssistAcceleration * fixedDeltaTime`, capped by the current attempt's remaining velocity budget.
- Recovery is one-way: if naturally integrated speed is at or above the assist target, low-speed assist must not decelerate it back to the target.
- First-slice speed evaluation uses these normalized factors:

```text
forwardFactor = Clamp01(CourseForwardAlignment)
downhillFactor = Clamp01(Sin(Max(0, ForwardDownhillDegrees)))
overspeedFraction = Clamp01(
    (currentTangentSpeed - SoftMaximumSpeed) / SoftMaximumSpeed)

TangentAcceleration =
    DownhillAcceleration * downhillFactor * forwardFactor

TangentDrag =
    SurfaceSlowdown
    + AboveMaximumSpeedResistance * overspeedFraction

LowSpeedAssistAcceleration =
    configuredLowSpeedAssistAcceleration * forwardFactor
```

- Angles in the formula are converted to radians before `Sin`. Because `ForwardDownhillDegrees` is produced from the surface-forward vertical-drop component, the sine reconstructs a dimensionless downhill contribution without another authored reference angle.
- Flat and uphill values contribute zero downhill acceleration. A `30` degree downhill contributes `0.5` of authored `DownhillAcceleration`; `45` degrees contributes approximately `0.707`; `90` degrees contributes `1`.
- Normal positive effects are eligible only while current tangent speed is below `SoftMaximumSpeed`.
- After ordinary acceleration/drag integration and rate-limited assist, movement composition prevents model-added speed from crossing `SoftMaximumSpeed` when the sampled speed began at or below it.
- When sampled speed begins above `SoftMaximumSpeed` because of launch, gravity, collision, contact, or solver response, movement composition does not clamp it to the envelope; positive effects remain disabled and proportional envelope resistance settles it over time.
- `AboveMaximumSpeedResistance` reaches its full authored rate at twice `SoftMaximumSpeed` and remains capped beyond that point.
- Speed and steering are composed into one final `RunBodyMovementTargetState` before **Run Body Movement Target** writes motion.
- **Run Body Movement Target** directly applies that target state; it does not convert the target state back into forces.
- When no valid grounded **Run Surface** is present, **Run Body Movement Controller** should still preserve corrected velocity and apply allowed steering contribution, but **Run Body Speed Model** should contribute neutral speed effects.
- Neutral speed effects still report the resolved `SoftMaximumSpeed`; this is observation metadata, not an applied airborne speed limit.
- **Run Body Speed Model** and **Run Steering Control** should not write Rigidbody velocity through separate paths.
- The old steering entry point should be removed or narrowed when `RunBodyMovementController` is introduced, so the project does not have two systems competing to write movement.
- Unity Rigidbody physics remains the contact, collision, and solver substrate.
- **Run Steering Control** remains direction-only and should not become the speed owner.
- **PlayerMaxSpeed** should behave as a soft envelope, not as a hidden hard velocity wall.
- `SoftMaximumSpeed` is an envelope threshold for positive-acceleration gating and optional envelope resistance, not an immediate velocity clamp.
- Below `SoftMaximumSpeed`, normal slope acceleration and low-speed assist may add speed.
- At or above `SoftMaximumSpeed`, the model should stop adding normal positive gameplay acceleration and may add an envelope contribution to `TangentDrag`.
- Temporary overspeed from launch, drops, collisions, or solver response may exist briefly and settle back into the envelope.
- Impossible or broken velocities still belong to **Run Body Speed Sanity Guard**, not to the gameplay envelope.

