# Run Body Speed Model Rationale

Focused rationale and deferred-extension notes supporting the [high-level actor and runtime diagram](./run-body-speed-model.md).

## Direction-Preserving Speed Rationale

The first slice separates heading from speed magnitude:

```text
Corrected tangent velocity
-> current tangent direction + current tangent speed
-> Run Steering Decision optionally changes tangent direction
-> project selected steering intent onto current SurfaceNormal plane
-> Run Body Speed Decision changes tangent speed
-> resolve target facing from final tangent direction + SteeringUp
-> Run Body Movement Controller recombines selected direction and resolved speed
```

For a corrected tangent velocity of `8 m/s` course-forward plus `6 m/s` right, current tangent speed is `10 m/s`. If one fixed pass adds `0.08 m/s`, direction-preserving composition produces approximately `8.064 m/s` forward plus `6.048 m/s` right. Adding `0.08 m/s` only to the course-forward component would instead produce `8.08 m/s` forward plus `6 m/s` right and would gradually auto-rotate the trajectory toward course-forward.

Direction-preserving composition keeps that automatic guidance out of **Run Body Speed Model**. Steering input, current momentum, gravity, and Rigidbody contacts remain the sources that can change travel direction. The next fixed movement pass observes any direction change produced by Unity collision/contact response instead of silently steering it back.

At exactly zero tangent speed, there is no current heading to preserve. The first slice therefore does not synthesize course-forward motion, cache a hidden recovery heading, or use low-speed assist as an automatic restart. A future restart mechanic would need an explicit direction source, eligibility rule, and player-facing rationale.

`SurfaceNormal` and `SteeringUp` intentionally remain separate. `SurfaceNormal` is the accepted contact-plane fact used by slope, support validity, and final grounded tangent composition. `SteeringUp` is a more heavily filtered control orientation that can slew toward support, reject a brief spike, or survive a short unsupported interval. Using `SteeringUp` as the final movement plane could scale a component that is actually moving into or away from the current support; using `SurfaceNormal` for final projection preserves the accepted physics boundary.

## Deferred Surface Profiles

Surface profiles are intentionally deferred because the reviewed finding only requires explicit speed ownership. Add **Run Surface Speed Profiles** later when product requirements call for multiple authored surface behaviors, such as ice accelerating less, snow adding stronger drag, or roof tiles preserving speed differently.

Until then, the first slice should keep one default/global **Run Body Speed Tuning** source and make **Run Body Speed Model** consume that tuning plus traversal facts.

## Soft Maximum Speed Rationale

`SoftMaximumSpeed` exists so designers can tune readability, upgraded speed, and high-speed fairness without creating a hidden wall in the player's velocity.

It should answer this question:

> "Around what surface-tangent speed should this Run Body stop being actively accelerated by gameplay tuning?"

The first-slice implementation should treat it as a speed envelope:

- If current surface-tangent speed is below the envelope, slope acceleration, surface effects, and low-speed assist can contribute normally.
- If current surface-tangent speed is near or above the envelope, normal positive acceleration should be gated off.
- If designers need stronger settling, the model can add envelope resistance through `TangentDrag` and mark `RunBodySpeedDecisionContributors.SpeedEnvelope`.
- It should not instantly replace current velocity with a capped value.
- It should not be used for defensive impossible-speed protection; that remains the job of **Run Body Speed Sanity Guard**.

## Low-Speed Assist Rationale

Low-speed assist exists only for the gap between normal sliding and **Lost Momentum** failure: the **Run Body** is still on a valid **Run Surface**, but a recoverable slowdown has dropped speed below useful control.

The first slice cannot reliably classify blocking geometry before attempting recovery. `IRigidbodyContactNotifier` exposes collision entry rather than persistent blocking-contact state, and `LostMomentumDetector` owns a run-ending observation rather than a movement-policy obstruction signal. Do not add a speculative forward raycast, persistent-contact tracker, or dependency on lost-momentum state for this slice.

Useful cases:

- Surface seams that unexpectedly consume surface-tangent velocity.
- Soft obstacle scrapes that do not qualify as **Obstacle Impact** but leave the player nearly stalled.
- Exiting a slow surface where designers want controllable speed to return.
- Valid post-launch landings that absorb more speed than intended.
- Short flat or uphill patches that should feel heavy without becoming an immediate fail.

Invalid cases:

- Airborne travel without valid support.
- Accepted **Obstacle Impact**, **Run Safety Net**, **Run Finish**, or **Lost Momentum**.
- Continuous assistance that keeps replacing velocity removed by blocking geometry.
- A hidden hard minimum speed that prevents real stalls.

Any recovery-related property or tooltip should state that it supports recoverable valid-surface traversal; it is not a speed floor and not a way to bypass run-ending rules.

The target-speed field should be named `LowSpeedAssistTargetSpeed`, not `MinimumRecoverySpeed`, because it describes an assist target rather than a hard minimum or broad recovery rule.

### Bounded Attempt Policy

Low-speed assist is one bounded recovery attempt per low-speed episode, not a continuously maintained speed floor:

```text
attemptVelocityBudget =
    Max(0, effectiveLowSpeedAssistTarget - speedAtAttemptStart)

requestedAssistDelta = Min(
    effectiveLowSpeedAssistAcceleration * fixedDeltaTime,
    Max(0, effectiveLowSpeedAssistTarget - naturallyIntegratedSpeed),
    remainingAttemptVelocityBudget)

remainingAttemptVelocityBudget -= requestedAssistDelta
```

- The budget is charged by requested model-authored velocity before Rigidbody contact resolution. A solver that removes velocity against a wall therefore cannot refill the attempt.
- A new run starts with an assist attempt available. The attempt begins only with valid support, speed below the target, a usable non-zero tangent heading, and positive course-forward eligibility.
- Reaching zero tangent speed does not synthesize a heading and does not spend more budget.
- Losing valid support pauses speed effects without replenishing the attempt.
- After an attempt reaches its target or spends its budget, another attempt is unavailable until sampled tangent speed independently rises above the assist target by a small numeric tolerance, or a new run begins.
- Because assist itself cannot cross the target, observed speed above that threshold demonstrates recovery from launch, gravity, slope, collision response, or another non-assist source.
- At a recoverable seam, requested velocity survives contact resolution and approaches the target. Against blocking geometry, requested velocity is consumed from a finite budget and then stops. On a persistently slow surface, the attempt may be spent and ordinary slowdown may still lead to **Lost Momentum**.
- Target and acceleration remain the only first-slice designer knobs. Attempt budget is derived from the speed deficit, so designers do not tune another arbitrary recovery quantity.
