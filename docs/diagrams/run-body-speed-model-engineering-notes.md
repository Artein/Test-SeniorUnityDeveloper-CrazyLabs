# Run Body Speed Model Engineering Notes

Detailed decisions supporting the [high-level actor and runtime diagram](./run-body-speed-model.md). Product behavior and acceptance scope live in the [replacement PRD](../prd/prd-run-body-explicit-speed-ownership.md).

## Reading The Diagram

- **Player** provides pull and steering input, then judges the visible feel.
- **Designer** tunes authored values: launch tuning, **Run Body Movement Tuning**, recovery behavior, the soft speed envelope, and movement-validity thresholds. Inspector validation reports invalid authored combinations before Play.
- **Engineer** owns the **Run Body Movement Controller** orchestration, **Run Body Speed Model** policy, **Run Body Movement Target** adapter boundary, authoring validation, tests, and diagnostics.
- **Runtime System** executes the fixed-step loop.
- **Run Body Movement Controller** is the fixed-step orchestrator; `RunBodyMotor` is avoided as the canonical name because it is not established repo vocabulary and can imply a Rigidbody replacement.
- First implementation should introduce `RunBodyMovementController` as the VContainer fixed-step entry point that owns the single final movement write.
- Do not keep `PlayerSteeringController` as the fixed-step entry point after speed becomes explicit; it would make a steering-named class own speed, steering, surface facts, and Rigidbody writes.
- During migration, do not register both `PlayerSteeringController` and `RunBodyMovementController` as active fixed-step movement writers.
- Existing steering math should move into a narrower steering collaborator; **Run Body Movement Controller** remains responsible for composing speed, steering, correction, and final target writes.
- **Run Body Movement Controller** should normalize current movement facts before speed and steering evaluation.
- **Run Body Movement Controller** owns low-speed-assist attempt state across fixed passes; the pure speed evaluator supplies the candidate target and rate but does not maintain or replenish recovery budget.
- **Run Body Movement Controller** should compute `HasValidGroundedRunSurface` from **Run Surface Context** plus corrected movement velocity before creating `RunBodySpeedContext`.
- **Run Body Movement Controller** owns movement-side launch-applied facts needed for movement composition, including fallback `LaunchUpDirection`.
- **Run Body Movement Controller** should call `IRunSteeringFrameResetter.Reset(launchUpDirection)` when run movement becomes active for the current launch.
- **Run Body Movement Controller** should use `IRunSteeringFrameSource.GetUpDirection(fallbackLaunchUpDirection)` when building `RunSteeringContext`.
- `RunSteeringInputController` must not store fallback launch-up direction or reset the steering frame; that is movement context, not input intent.
- `RunSurfaceSteeringFrameSource` remains the first-slice steering frame filter/source/reset target.
- Do not add a separate steering-frame coordinator in the first slice.
- The first correction order should be raw Rigidbody velocity, then **Run Body Speed Sanity Guard**, then **Launch Landing Stabilization**, then speed and steering evaluation.
- **Run Body Speed Sanity Guard** remains defensive only; it must not become `SoftMaximumSpeed` or a player-facing speed limit.
- **Launch Landing Stabilization** remains a stateful post-launch lift correction; it is not speed gameplay and not steering.
- `LaunchLandingMaximumLiftSpeed` answers the launch-landing stabilization question: how much post-launch lift should be clamped or tolerated after landing.
- Do not reuse `LaunchLandingMaximumLiftSpeed` as the public name for the general supported-surface validity gate; that gate should use movement-validity naming such as `MaximumSupportedSurfaceNormalLiftSpeed`.
- A narrow `RunLaunchLandingStabilizer` collaborator can own stabilization state and timing so **Run Body Movement Controller** does not absorb the whole old steering controller.
- `RunLaunchLandingStabilizer.Stabilize(...)` should consume corrected velocity plus the minimum current support facts needed by the correction and return corrected velocity; it should not evaluate slope acceleration, surface slowdown, or steering direction.
- `RunLaunchLandingStabilizer` should be stateful but driven by **Run Body Movement Controller**.
- `RunLaunchLandingStabilizer` should not subscribe directly to `ISlingshotLaunchAppliedNotifier` or **Gameplay State** in the first slice.
- **Run Body Movement Controller** should call `ArmForLaunch()` when **Launch Applied** arrives, `Reset()` when the run lifecycle clears, and `Stabilize(...)` at the established point in the fixed movement pass.
- This keeps fixed-step correction ordering under one orchestrator while keeping launch-landing state and timing out of the movement controller itself.
- First-slice API:

```csharp
public interface IRunLaunchLandingStabilizer
{
    void ArmForLaunch();
    void Reset();
    Vector3 Stabilize(RunLaunchLandingStabilizationContext context);
}
```

- `ArmForLaunch` is intentional wording: it prepares the stabilizer to observe post-launch flight and the first valid landing; it does not claim that stabilization is already active.
- `RunLaunchLandingStabilizationContext` should carry only the corrected velocity, current support facts needed by the existing correction, and fixed-step elapsed time.
- Do not pass `LaunchUpDirection` to the stabilizer. The current lift correction uses the landed **Run Surface** normal; launch-up direction belongs to **Run Steering Frame** lifecycle.
- Stabilization tuning remains constructor-injected through its narrow config view rather than repeated in the per-step context.
- **Run Body Speed Model** and **Run Steering Evaluator** should receive corrected velocity after sanity and landing stabilization.
- The final composed output should be named `RunBodyMovementTargetState`.
- `RunBodyMovementTargetState` is data-only resolved target state, not a Command-pattern object and not a policy decision.
- Avoid `RunBodyMovementCommand` because `Command` can imply behavior/execution ownership.
- Avoid `RunBodyMovementDecision` because `Decision` is reserved for evaluator outputs such as `RunBodySpeedDecision` and `RunSteeringDecision`.
- First-slice `RunBodyMovementTargetState` shape:

```csharp
public readonly struct RunBodyMovementTargetState
{
    public Vector3 LinearVelocity { get; }
    public bool HasRotation { get; }
    public Quaternion Rotation { get; }
}
```

- **Run Body Movement Controller** resolves one `RunBodyMovementTargetState` per fixed movement step before writing through **Run Body Movement Target**.
- The Rigidbody write boundary should be named `IRunBodyMovementTarget`, replacing steering-specific target vocabulary such as `IPlayerSteeringTarget`.
- `IRunBodyMovementTarget` is adapter-only; it reads current Rigidbody movement state and applies the resolved target state.
- `IRunBodyMovementTarget` must not evaluate speed, steering, correction, input, or surface facts.
- First-slice target application should directly set the resolved Rigidbody `linearVelocity` and use `MoveRotation` when `RunBodyMovementTargetState.HasRotation` is true.
- First-slice target application should not use `AddForce`, force modes, Rigidbody drag, or mass as gameplay speed tuning paths.
- First-slice `IRunBodyMovementTarget` shape:

```csharp
public interface IRunBodyMovementTarget
{
    Vector3 LinearVelocity { get; }
    void ApplyTargetState(RunBodyMovementTargetState targetState);
}
```

- Prefer `ApplyTargetState(...)` over `Apply(...)` because the method name stays clear at call sites and avoids implying a broader command-style execution boundary.
- This preserves the current deterministic velocity-write style while moving the decision-making out of the steering target and into **Run Body Movement Controller** plus evaluators.
- Split the steering collaborator into `RunSteeringInputController` and `IRunSteeringEvaluator`.
- `RunSteeringInputController` owns stateful input lifecycle: input enable/disable, pointer gesture state, desired steer, and smoothed current steer.
- `RunSteeringInputController` owns smoothing state and smoothing math.
- `RunSteeringInputController` should keep `IInitializable` / `IDisposable` style ownership for input event subscription and cleanup.
- `RunSteeringInputController` owns input enable/disable, active pointer gesture state, and reset-on-exit behavior around **Running**.
- `RunSteeringInputController` should enable active steering input only after both **Running** and `ISlingshotLaunchAppliedNotifier.LaunchApplied` have been observed for the current run.
- `LaunchApplied` may be observed before **Running**; in that case, steering input stays inactive and returns a neutral state until **Running** is reached.
- If **Running** is reached before `LaunchApplied`, steering input stays inactive and returns a neutral state until the launch-applied event arrives.
- Leaving **Running** clears launch readiness for steering input and resets pointer/smoothing state.
- First slice should not introduce a shared launch-readiness query service only for this steering gate.
- `RunSteeringInputController` should track its own current-run launch-applied readiness from `ISlingshotLaunchAppliedNotifier`.
- Existing consumers such as `RunCameraController`, `RunEndFlow`, `LostMomentumDetector`, and `PlayerSteeringController` already pair launch-applied readiness with their own activation and reset rules; preserve that local ownership until a shared current-run lifecycle invariant is designed.
- Steering input tests should cover `LaunchApplied` before **Running**, **Running** before `LaunchApplied`, and leaving **Running** clearing readiness.
- `RunSteeringInputController` implements `IRunSteeringInputSource`, but should not implement `IFixedTickable` in the first slice.
- **Run Body Movement Controller** should not own pointer subscription, input enable handles, or gesture reset behavior.
- `RunSteeringInputController` must not write Rigidbody velocity or rotation.
- `RunSteeringInputController` should expose a single `RunSteeringInputState` snapshot to **Run Body Movement Controller** instead of several read-only mutable-state properties.
- **Run Body Movement Controller** should depend on `IRunSteeringInputSource`, not the concrete `RunSteeringInputController`.
- **Run Body Movement Controller** owns fixed-step ordering: it should advance and read `RunSteeringInputState` once per fixed movement pass, then use that snapshot to build `RunSteeringContext`.
- Do not register `RunSteeringInputController` as an independent fixed-tick movement entry point in the first slice.
- Do not introduce a separate movement lifecycle coordinator in the first slice.
- This avoids relying on VContainer fixed-tick ordering for movement-critical input smoothing.
- First-slice API shape should be `RunSteeringInputState AdvanceAndRead(float fixedDeltaTime)`.
- When inactive, `IRunSteeringInputSource.AdvanceAndRead(...)` should return neutral `RunSteeringInputState`.
- Use `IRunSteeringInputSource` because **Run Body Movement Controller** sees the dependency as the source of current steering input state, while `AdvanceAndRead(...)` makes the smoothing mutation explicit.
- Avoid `IRunSteeringInputStateProvider` and `IRunSteeringInputReader` because they sound passive and hide the smoothing advance.
- Avoid `IRunSteeringInputDriver` because it can imply driving movement or hardware input.
- Avoid `IRunSteeringInputController` because it mirrors the concrete class and makes the consumer depend on too broad a role.
- This avoids partial reads where pointer events could mutate gesture state between property accesses, and it gives tests and diagnostics one coherent input-intent value to inspect.
- `RunSteeringInputState` is not a steering evaluator output, movement target, or Rigidbody state. It is the current input intent snapshot.
- `IRunSteeringEvaluator` is pure fixed-step steering decision logic; it receives steering facts from **Run Body Movement Controller** and returns steering contribution for final **Run Body Movement Target State** composition.
- `IRunSteeringEvaluator` must not subscribe to input, own gesture state, read Rigidbody state directly, or write movement.
- First-slice `RunSteeringInputState` should include gameplay-consumed fields plus lightweight diagnostics:

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

- **Run Body Movement Controller** should use only `IsGestureActive` and `SmoothedSteer` for movement composition.
- `DesiredSteer` explains smoothing lag and responsiveness during tests or diagnostics.
- `CapturedMetrics` explains device-specific steering feel, and should be treated as meaningful only when `HasCapturedMetrics` is true.
- The input to `IRunSteeringEvaluator` should be named `RunSteeringContext`.
- `RunSteeringContext` is per-evaluation steering facts, not config and not output.
- First-slice `RunSteeringContext` shape:

```csharp
public readonly struct RunSteeringContext
{
    public Vector3 CurrentVelocity { get; }
    public Vector3 SteeringUp { get; }
    public RunSteeringMode SteeringMode { get; }
    public float SmoothedSteer { get; }
    public float MaximumTurnDegreesPerSecond { get; }
    public float MinimumSteerSpeed { get; }
    public float FixedDeltaTime { get; }
    public bool IsGestureActive { get; }
}
```

- `RunSteeringContext.MaximumTurnDegreesPerSecond` should already be resolved by **Run Body Movement Controller** from grounded/air steering tuning.
- `RunSteeringContext.CurrentVelocity` is used to find the current planar direction and to apply the steering minimum-speed gate; it does not make steering the speed owner.
- The pure steering output should be named `RunSteeringDecision`.
- First-slice `RunSteeringDecision` shape:

```csharp
public readonly struct RunSteeringDecision
{
    public bool ShouldApplySteering { get; }
    public Vector3 SteeringIntentDirection { get; }
}
```

- `RunSteeringDecision.SteeringIntentDirection` is a normalized pre-projection steering-intent contribution, not a final movement direction or velocity.
- Avoid `SteeredPlanarDirection`: `Steered` sounds final, while `Planar` does not identify whether the plane comes from `SteeringUp`, `SurfaceNormal`, or the **Run Progress Frame**.
- `RunSteeringDecision` should not include target rotation, final speed, or final velocity; **Run Body Movement Controller** composes steering intent with the current support plane and **Run Body Speed Model** output.
- **Run Body Movement Controller** should resolve target facing only after it has projected steering intent onto `SurfaceNormal` and obtained a usable final tangent direction.
- First-slice target facing should look along the final tangent direction while using `SteeringUp` as its stabilized up reference.
- If no usable final tangent direction exists, **Run Body Movement Controller** should not produce a target rotation for that pass.
- Do not add a separate facing evaluator in the first slice; final facing is a direct part of composing `RunBodyMovementTargetState`.
- **Run Body Speed Model** owns intentional surface-tangent speed decisions.
- First-slice **Run Body Speed Model** changes only surface-tangent speed.
- First-slice speed evaluation is direction-preserving: it changes speed magnitude without selecting a preferred travel heading.
- **Run Body Movement Controller** should use `RunSteeringDecision.SteeringIntentDirection` when steering applies; otherwise it should preserve the usable direction of corrected tangent motion.
- **Run Body Speed Model** must not generate a course-forward, steepest-downhill, or fallback travel direction.
- `ForwardDownhillDegrees` controls the amount of scalar downhill acceleration; it is not an instruction to rotate or push the **Run Body** along the **Run Progress Frame** forward direction.
- `RunBodySpeedContext.SurfaceNormal` is the authoritative grounded movement plane for tangent/normal decomposition and final tangent projection.
- `RunSteeringContext.SteeringUp` remains the stabilized control frame used to evaluate steering intent; it must not replace `SurfaceNormal` as the physical support plane.
- After steering evaluation, **Run Body Movement Controller** should project the selected steering-intent direction onto the plane defined by `SurfaceNormal`, normalize it, and apply resolved tangent speed along that projected direction.
- If that projection is non-finite or too small to normalize, **Run Body Movement Controller** should preserve a usable current surface-tangent direction; it must not invent a fallback heading.
- The corrected surface-normal velocity component must be preserved across this composition so speed and steering do not take ownership of contact separation, penetration correction, gravity, or lift.
- If no usable tangent direction exists, **Run Body Movement Controller** must not turn scalar acceleration or low-speed assist into a new velocity direction.
- **Run Body Low-Speed Assist** may strengthen recoverable near-stall motion with an existing heading, but it does not restart a fully stopped **Run Body**.
- Unity gravity or contact response may naturally restart a stopped body on a slope. Otherwise **Lost Momentum** remains the run-ending authority until a separate explicit restart mechanic is designed.
- `CourseForwardAlignment` is the normalized signed alignment between current surface-tangent travel and the **Run Progress Frame** forward direction projected onto `SurfaceNormal`.
- `CourseForwardAlignment` ranges from `-1` for course-reversed travel through `0` for lateral or directionless travel to `+1` for course-forward travel.
- Positive speed effects use `Clamp01(CourseForwardAlignment)` as a proportional factor: downhill acceleration and low-speed assist are full course-forward, reduced while diagonally forward, and absent while lateral, course-reversed, directionless, or invalid.
- Model-authored surface slowdown and above-envelope resistance remain direction-independent; they may reduce tangent speed regardless of heading.
- This scalar controls effect amount only. It does not give **Run Body Speed Model** a direction vector or permission to select a heading.
- First-slice **Run Body Speed Model** preserves corrected surface-normal or vertical velocity; gravity, launch, collisions, sanity correction, and launch-landing stabilization own that component.
- **Run Body Movement Controller** should decompose corrected velocity into surface-tangent and surface-normal components, apply speed and steering to the tangent component, then recombine with the preserved corrected normal component.
- First-slice speed effects apply only when `RunBodySpeedContext.HasValidGroundedRunSurface` is true.
- If `RunBodySpeedContext.HasValidGroundedRunSurface` is false, the speed model should return no slope acceleration, no surface slowdown, no low-speed assist, and no speed-envelope resistance.
- `HasValidGroundedRunSurface` should be false when corrected velocity is meaningfully moving away from the support normal, even if **Run Surface Context** still reports support.
- This excludes ramp-lip takeoff, convex crest separation, and post-launch stale support from grounded speed effects.
- The lift check should tolerate tiny jitter or contact noise; it is a meaningful-lift gate, not a zero-dot-product switch.
- The surface-normal lift threshold belongs to movement and **Run Surface** validity orchestration, not `IRunBodySpeedConfig`.
- Name the general support-validity threshold as a movement-validity value, for example `MaximumSupportedSurfaceNormalLiftSpeed`.
- Keep launch-landing-specific lift clamp/tolerance separate from the general support-validity threshold, even if both start with the same numeric default.
- `IRunBodySpeedConfig` answers what speed effects apply after the surface gate is valid; it should not decide whether the body is still supported by the surface.
- `DefaultRunBodySpeedEvaluator` should not depend on `RunSurfaceContext` or duplicate the rules for deciding whether support is a valid grounded **Run Surface**.
- Neutral unsupported/invalid-surface speed decisions should still echo `SoftMaximumSpeed = context.ResolvedSoftMaximumSpeed` for diagnostics and tests.
- Neutral unsupported/invalid-surface speed decisions should return:

```csharp
TangentAcceleration = 0f;
TangentDrag = 0f;
LowSpeedAssistTargetSpeed = 0f;
LowSpeedAssistAcceleration = 0f;
SoftMaximumSpeed = context.ResolvedSoftMaximumSpeed;
Contributors = RunBodySpeedDecisionContributors.None;
```

- Airborne movement keeps speed-model contribution neutral; Unity physics, collision/contact response, and **Run Air Steering Control** own airborne motion changes.
- First slice uses default/global run speed tuning, not per-surface profiles.
- **Run Surface Speed Profiles** are a future optional extension for distinct authored surface behaviors.
- Future terrain paint, terrain layers, collider authoring, volumes, or course sections may select a **Run Surface Speed Profile**.
- Future terrain materials such as ice or snow should select profiles rather than replacing profiles.
- **Run Surface Context** should remain traversal facts: grounded state, stable support normal, and downhill angle.
- Future **Run Surface Speed Profile** resolution should stay outside **Run Surface Context**, even if the implementation reuses the same support hit internally.
- **Run Body Movement Controller** combines traversal facts and the narrow speed-tuning view of **Run Body Movement Tuning** before evaluating **Run Body Speed Model**.
- Unity physics materials remain contact/solver authoring; they should not be the only source of player-facing speed tuning.
- Tests should fake `IRunBodySpeedConfig` directly in the first slice, without depending on Unity Terrain sampling, asset loading, or profile resolution.
- First-slice code should use one designer-facing `RunBodyMovementConfig : ScriptableObject` asset for related movement settings.
- `RunBodyMovementConfig` should implement narrow runtime interfaces such as `IRunBodySpeedConfig`, `IRunBodyMovementValidityConfig`, `IRunLaunchLandingStabilizationConfig`, `IRunSteeringConfig`, and `IRunSteeringFrameConfig`.
- Runtime services should depend on the narrow interfaces, not on the concrete `RunBodyMovementConfig` asset.
- New movement and steering services should not inject the legacy broad `IPlayerSteeringConfig` contract.
- Keep the broad `IPlayerSteeringConfig` only as a temporary migration adapter if old code still needs it during a staged refactor.
- `IRunBodySpeedConfig` should expose read-only raw authored tuning values only; it should not resolve relationships, expose mutation, load assets, or expose Unity object lifecycle concerns.
- `IRunBodySpeedConfig` should not expose movement/surface-validity thresholds such as the allowed surface-normal lift used by `HasValidGroundedRunSurface`.
- `IRunBodySpeedConfig` should not expose `MaximumSupportedSurfaceNormalLiftSpeed`; that value belongs to movement or **Run Surface** validity orchestration.
- First-slice DI registration should live in `GameplayLifetimeScope`, alongside the existing global gameplay config assets.
- `GameplayLifetimeScope` should serialize the assigned `RunBodyMovementConfig` asset and register it once through the relevant narrow interfaces.
- Prefer the local VContainer style for multi-interface registration, using a generic `RegisterInstance<T1, T2, T3>(...)` overload and `.As<...>()` for any remaining interfaces instead of repeated registration lines for the same asset.
- `GameplayLifetimeScope` should register `DefaultRunBodySpeedEvaluator` as the singleton implementation of `IRunBodySpeedEvaluator`.
- Do not put this first-slice tuning/evaluator registration in `GameplayPhysicsSceneCompositionMonoInstaller`; that installer remains for scene-authored physics/surface services.
- Move speed tuning registration toward scene or surface composition only when speed behavior becomes per-level, per-course-section, or per-surface-profile authored.
- The `IRunBodySpeedConfig` view of `RunBodyMovementConfig` owns default/base speed tuning and envelope-resistance values; it must not read economy state, upgrade levels, or player progress.
- Upgraded `PlayerMaxSpeed` should be resolved by the existing upgrade/stat pipeline before speed evaluation and passed to **Run Body Speed Model** as runtime run/player stats.
- In the first slice, `RunBodyMovementController` owns that composition step and calls the existing resolver directly:

```csharp
var resolvedSoftMaximumSpeed = _runGameplayStatResolver.Resolve(
    _playerMaxSpeedStatId,
    _speedConfig.BaseSoftMaximumSpeed);
```

- `RunBodyMovementController` passes the result to the evaluator as `RunBodySpeedContext.ResolvedSoftMaximumSpeed`; the evaluator must not know about `GameplayStatId`, upgrade levels, or snapshot storage.
- Do not introduce `IRunBodySpeedStatsResolver` for this single existing resolver call. Extract a movement-specific stats collaborator only if multiple movement stats, distinct resolution lifetimes, or shared transformation rules create concrete complexity.
- `RunBodyMovementController` should resolve this value while building `RunBodySpeedContext` on every active fixed movement pass.
- Repeated resolution does not make upgrades mutable during a run: `IRunGameplayStatResolver` reads the immutable active run modifier snapshot and caches its internal resolver while that snapshot is unchanged.
- Do not add a second controller-owned per-run cache for this scalar. If profiling later justifies caching several movement stats, introduce one coherent resolved movement-stats snapshot with an explicit run lifecycle instead of independent cached fields.
- `SoftMaximumSpeed` in `RunBodySpeedDecision` is the resolved envelope value after combining default/base tuning with runtime speed stats.
- First-slice `IRunBodySpeedConfig` names should be designer-readable concepts, not physics-unit names such as `MetersPerSecondSquared`.
- `RunBodyMovementConfig` should communicate units through inspector labels, tooltips, XML docs, and tests.
- Recommended first-slice `IRunBodySpeedConfig` properties:

```csharp
public interface IRunBodySpeedConfig
{
    float DownhillAcceleration { get; }
    float SurfaceSlowdown { get; }
    float LowSpeedAssistTargetSpeed { get; }
    float LowSpeedAssistAcceleration { get; }
    float BaseSoftMaximumSpeed { get; }
    float AboveMaximumSpeedResistance { get; }
}
```

- Suggested designer labels:
  - `DownhillAcceleration`: "Speed added per second while sliding downhill."
  - `SurfaceSlowdown`: "Speed removed per second from baseline surface drag."
  - `LowSpeedAssistTargetSpeed`: "Minimum useful slide speed the model tries to recover toward."
  - `LowSpeedAssistAcceleration`: "How quickly eligible low-speed recovery approaches its target."
  - `BaseSoftMaximumSpeed`: "Default comfort/readability speed before upgrades."
  - `AboveMaximumSpeedResistance`: "Extra slowdown applied when speed exceeds the soft maximum."
- Keep `IRunBodySpeedConfig` flat for DI, tests, and simple fakes.
- Keep `RunBodyMovementConfig` serialized fields flat as well; use inspector headers to group designer controls instead of nested serialized sections.
- Recommended `RunBodyMovementConfig` speed inspector grouping:
  - `Slope`: `DownhillAcceleration`
  - `Slowdown`: `SurfaceSlowdown`
  - `Recovery`: `LowSpeedAssistTargetSpeed`, `LowSpeedAssistAcceleration`
  - `Speed Envelope`: `BaseSoftMaximumSpeed`, `AboveMaximumSpeedResistance`
- First-slice guardrails should use inspector attributes such as `Min` for immediate Editor feedback, backed by explicit validation for invariants that attributes cannot express.
- Prefer `Min` for basic non-negative values; use `Range` only when the upper bound is a real design constraint, not an arbitrary tuning guess.
- Suggested first guardrails:

```csharp
[Min(0f)] private float _downhillAcceleration;
[Min(0f)] private float _surfaceSlowdown;
[Min(0f)] private float _lowSpeedAssistTargetSpeed;
[Min(0f)] private float _lowSpeedAssistAcceleration;
[Min(0.01f)] private float _baseSoftMaximumSpeed;
[Min(0f)] private float _aboveMaximumSpeedResistance;
```

- Add a pure `RunBodyMovementConfigValidator` with EditMode tests. It should validate finite values, required positive values, non-negative rates, and real cross-field invariants across the concrete asset's narrow config views.
- `RunBodyMovementConfig.OnValidate()` should invoke the same validator and report all discovered authoring errors immediately in the Editor. Inspector attributes remain the first guardrail, but they are not the source of truth and do not cover non-finite or cross-field values.
- Validate the asset again once at composition or movement initialization and throw before movement starts when an invariant is violated. This follows the existing Slingshot and Gameplay State config pattern and protects builds in which Editor feedback was missed.
- Do not normalize, clamp, default, or return a neutral decision for malformed authored config inside `DefaultRunBodySpeedEvaluator` or the fixed-step movement loop. Those paths may assume that startup validation succeeded.
- Validate `PlayerMaxSpeed` upgrade authoring in the Editor and validate the resolved value at the active-run preparation boundary, where the immutable modifier snapshot becomes authoritative. Invalid resolved speed prevents the run from reaching movement; it does not fall back to `BaseSoftMaximumSpeed`.
- A valid resolved soft maximum must be finite, positive, and below the **Run Body Speed Sanity Guard** threshold. Normal upgrades must never reach the defensive guard.
- Do not add a new build-preprocessor validation framework in the first slice; the repository's established `OnValidate` plus pre-use fail-fast validation pattern is sufficient.
- The cross-field relationship `BaseSoftMaximumSpeed >= LowSpeedAssistTargetSpeed` should be handled inside **Run Body Speed Model** evaluation, not by first-slice asset validation.
- **Run Body Speed Model** should clamp the effective assist target to the resolved soft maximum:

```text
effectiveLowSpeedAssistTarget = min(config.LowSpeedAssistTargetSpeed, resolvedSoftMaximumSpeed)
```

- This guarantees low-speed assist never fights the soft speed envelope, while keeping `RunBodyMovementConfig` easy to tune.
- This `min` operation resolves two valid gameplay values; it is not recovery from invalid authoring.
- The input to **Run Body Speed Model** should be named `RunBodySpeedContext`.
- `RunBodySpeedContext` is per-evaluation observed/runtime facts, not config and not output.
- First-slice `RunBodySpeedContext` shape:

```csharp
public readonly struct RunBodySpeedContext
{
    public Vector3 CurrentVelocity { get; }
    public bool HasValidGroundedRunSurface { get; }
    public Vector3 SurfaceNormal { get; }
    public float ForwardDownhillDegrees { get; }
    public float CourseForwardAlignment { get; }
    public float ResolvedSoftMaximumSpeed { get; }
}
```

- **Run Body Movement Controller** derives `CourseForwardAlignment` from normalized current surface-tangent travel and the normalized **Run Progress Frame** forward direction projected onto the same `SurfaceNormal` plane.
- If either projected direction is too small to normalize, `CourseForwardAlignment` is `0`; directionless travel is a valid runtime observation.
- `CourseForwardAlignment` is an observed normalized runtime fact, not designer tuning and not a preferred movement direction.
- The evaluator clamps the value before use because `CourseForwardAlignment` is a normalized mathematical input whose proportional effect is defined only on `[-1, 1]`; this is ordinary domain normalization, not authored-config repair.

## Invalid-Value Boundary

- **Authored values** fail fast. Inspector attributes provide immediate bounds, `OnValidate` reports complete validator results, and startup/run preparation rejects invalid config or resolved stats before movement.
- **Runtime movement code** assumes authored config and resolved player stats satisfy those invariants. It contains no silent fallback speed profile and does not repeatedly sanitize config during fixed-step evaluation.
- **Expected runtime absence** is not invalid authoring. Unsupported motion, a missing usable tangent direction, or a degenerate support candidate follows the documented neutral speed-effect behavior.
- **External physics corruption** remains defensively contained. The existing **Run Body Speed Sanity Guard** still handles impossible or non-finite Rigidbody velocity because contacts, solver state, and third-party writes are outside authoring validation.
- Tests should cover the validator, Editor-facing validation adapter, startup rejection, resolved-stat rejection, and the valid production path separately from physics-observation containment.

- `RunSurfaceContext.IsGrounded` remains the filtered traversal-support observation owned by **Run Surface Context**; it is not copied into `RunBodySpeedContext`.
- Diagnostics should observe **Run Surface Context** and `HasValidGroundedRunSurface` separately when they need to distinguish unsupported motion from support rejected by movement-validity rules.
- If diagnostics later need the exact rejection cause, add an explicit movement-validity diagnostic result rather than reintroducing a partly redundant grounded boolean into the speed evaluator input.
- **Run Body Movement Controller** should build `HasValidGroundedRunSurface` from grounded state, a valid support normal, and no meaningful surface-normal lift in corrected velocity before evaluating the speed model.
- Recommended predicate shape:

```text
HasValidGroundedRunSurface =
    IsGrounded
    && HasValidGroundNormal
    && !HasMeaningfulSurfaceNormalLift(CorrectedVelocity, GroundNormal)

HasMeaningfulSurfaceNormalLift =
    Dot(CorrectedVelocity, GroundNormal) > MaximumSupportedSurfaceNormalLiftSpeed
```

- If the validity rule grows beyond a small predicate, extract a narrow surface-facts mapper owned by the movement orchestration layer; keep the speed evaluator on normalized facts.
- `RunBodySpeedContext` should not include steering input in the first slice; steering remains direction/control, while **Run Body Speed Model** owns scalar speed behavior.
- `RunBodySpeedContext` should not include `FixedDeltaTime` in the first slice. The evaluator returns unintegrated acceleration/slowdown rates, and **Run Body Movement Controller** owns fixed-step integration.
- Add elapsed time to speed evaluation only when a concrete time-dependent speed policy requires it; do not carry it for speculative diagnostics.
- Keep **Run Body Speed Model** as the domain/architecture concept.
- The first-slice pure code interface should be named `IRunBodySpeedEvaluator`, because its job is to evaluate context into a decision:

```csharp
public interface IRunBodySpeedEvaluator
{
    RunBodySpeedDecision Evaluate(RunBodySpeedContext context);
}
```

- Avoid `IRunBodySpeedModel` for this pure seam while the method is named `Evaluate`; the interface name and method should describe the same role.
- The first concrete implementation should be named `DefaultRunBodySpeedEvaluator`.
- `DefaultRunBodySpeedEvaluator` means the default/global evaluator for the first slice; avoid `TunedRunBodySpeedEvaluator` because all evaluators are tuned by config.
- `DefaultRunBodySpeedEvaluator` should receive `IRunBodySpeedConfig` through constructor injection.
- `RunBodySpeedContext` should carry per-step runtime facts only; do not put config, assets, or other tuning references into the context.
- `DefaultRunBodySpeedEvaluator` should not depend directly on Rigidbody, time sources, surface-context sources, steering input, Unity asset lifecycle, or scene objects.
- Tests should instantiate `DefaultRunBodySpeedEvaluator` with a fake `IRunBodySpeedConfig` and pass explicit `RunBodySpeedContext` values.

```csharp
public sealed class DefaultRunBodySpeedEvaluator : IRunBodySpeedEvaluator
{
    private readonly IRunBodySpeedConfig _config;

    public DefaultRunBodySpeedEvaluator(IRunBodySpeedConfig config)
    {
        _config = config;
    }

    public RunBodySpeedDecision Evaluate(RunBodySpeedContext context)
    {
        ...
    }
}
```

- Recommended first-slice VContainer shape:

```csharp
builder.RegisterInstance<
    IRunBodySpeedConfig,
    IRunBodyMovementValidityConfig,
    IRunLaunchLandingStabilizationConfig>(_runBodyMovementConfig)
    .As<IRunSteeringConfig, IRunSteeringFrameConfig>();

builder.Register<IRunBodySpeedEvaluator, DefaultRunBodySpeedEvaluator>(Lifetime.Singleton);
builder.Register<IRunSteeringInputMetricsResolver, DefaultRunSteeringInputMetricsResolver>(Lifetime.Singleton);
builder.Register<IRunSteeringEvaluator, DefaultRunSteeringEvaluator>(Lifetime.Singleton);
builder.RegisterEntryPoint<RunBodyMovementController>();
```
