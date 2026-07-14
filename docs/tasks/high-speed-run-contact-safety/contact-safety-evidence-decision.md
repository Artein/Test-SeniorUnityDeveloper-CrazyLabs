# Contact-Safety Evidence and Remediation Decision

## Decision

The narrowly scoped runtime correction remains selected, and the supported 40 m/s contact-safety contract is accepted. The final compile, focused, related-regression, and full-suite gates are clean. The Continuous Dynamic product path, resolver, notifier, classifier, and trigger scenarios pass. The former Discrete negative control is rejected as an invalid acceptance oracle after the owner-authorized synchronization experiment made it detect the collision in the isolated paired run.

The adversarial Continuous Dynamic obstacle scenario exposed a deterministic classification failure, not tunneling: Unity delivered a solid collision callback and physically stopped the production Run Body, but the production-scene callback reported zero relative velocity after resolution. The existing classifier therefore rejected the contact because its contact-normal speed was below the configured Obstacle Impact threshold.

The owner approved the local issue plan and full implementation. [Issue 05](05-preserve-static-obstacle-approach-velocity.md) is the selected remedy. Rigidbody Contact Notifier now preserves approach velocity for this exact static-obstacle data-loss case by reconstructing:

```text
preSolveBodyVelocity = postSolveBodyVelocity - collisionImpulse / bodyMass
```

A swept query is not authorized or implemented because collision detection and solid response already succeeded. Existing finite nonzero callback relative velocity remains authoritative. Moving Rigidbody and ArticulationBody obstacles remain outside the fallback scope.

## Environment and supported envelope

- Unity: 6000.3.18f1.
- Physics: built-in Unity 3D physics.
- Fixed timestep: 0.02 seconds.
- Production Run Body: authored non-trigger SphereCollider, radius 0.35 meters.
- Production collision mode: Continuous Dynamic, with gravity and interpolation.
- Supported acceptance speed: 40 m/s; 0.8 meters per fixed step.
- Diagnostic stress speed: 80 m/s; 1.6 meters per fixed step. This is headroom, not a product guarantee.

## Boundary evidence

| Boundary | Contract | Geometry | Result | Classification |
|---|---|---|---|---|
| Thin solid Obstacle, Discrete, 40 m/s | Diagnostic only; rejected negative control | 0.01 m obstacle; 0.71 m raw overlap span; 0.75 m contact-offset envelope; 0.8 m step; 0.025 m planned clearance at each sampled pose | Unsynchronized isolated runs missed; full-suite and synchronized isolated runs detected collision | Not an acceptance oracle |
| Thin solid Obstacle, Continuous Dynamic, 40 m/s | Solid collision + classifier + Run End | Production body/mode; same 0.01 m obstacle and mathematical phase | Collision delivered; exactly one ObstacleHit; Run Ended | Proved safe after Issue 05 |
| Band 5 Run Finish, 40 m/s | Sampled trigger overlap | 2.2 m overlap span; +1.4 m fixed-step margin | Exactly one Finished; Run Ended | Proved safe |
| Band 5 Run Finish, 80 m/s | Sampled trigger overlap stress | 2.2 m overlap span; +0.6 m fixed-step margin | Exactly one Finished; Run Ended | Stress passed |
| Run Safety Net, 40 m/s | Sampled trigger overlap | 2.7 m overlap span; +1.9 m fixed-step margin | Exactly one OutOfBounds; Run Ended | Proved safe |
| Run Safety Net, 80 m/s | Sampled trigger overlap stress | 2.7 m overlap span; +1.1 m fixed-step margin | Exactly one OutOfBounds; Run Ended | Stress passed |

The trigger results prove sampled overlap through the authored volumes. They do not claim that Continuous Dynamic guarantees trigger delivery.

## Diagnostic evidence and selected boundary

Before remediation:

- Corrected production-scene run `r_ja2c6exc`: 5/6; only Continuous Dynamic obstacle classification failed.
- Diagnostic run `r_iaw1vous`: one collision, one contact, zero relative velocity, classifier rejected, zero results, state remained Running.
- Instrumented run `r_5aqd1s7g`: one authoritative collision; total impulse `(0, 0, -40)` kg·m/s; mass 1 kg; callback Run Body z velocity 0 m/s; relative velocity 0 m/s.
- Exact acceptance failure: `Expected: property Count equal to 1 But was: 0`.

This evidence rejects two broader remedies:

- FixedUpdate history cannot recover the approach velocity because the available snapshot is already post-impact.
- A sweep is unnecessary because Unity found and physically resolved the collision.

The Discrete negative control has a separate test-design problem:

- Original 0.05 m obstacle full suite `r_2qp681qv`: 1081/1082; two contacts and one ObstacleHit.
- Revised 0.01 m obstacle isolated runs `r_n6c4q4zc` and `r_ztvuqibo`: 6/6 each.
- Revised combined focused run `r_vr8r8cve`: 25/25.
- Revised full suite `r_jc2o4qxs`: 1081/1082; two contacts and one ObstacleHit.
- No test assignment to global Physics contact/simulation settings or `Time.fixedDeltaTime` was found.
- Owner-authorized synchronized paired run `r_xixzvr0o`: after assigning Discrete in PreLaunch and waiting exactly one fixed step, the Continuous Dynamic positive passed and Discrete produced two collision notifications, two contact points, one ObstacleHit, and Run Ended.

The experiment falsifies the permanent requirement that Discrete must miss. Earlier isolated misses depended on an unsynchronized collision-mode transition and cannot establish CCD dependence. Per the agreed stop condition, no retry or further geometry reduction is accepted. The synchronization probe and mutable-mode control are removed from the committed fixture; the production-mode result and mathematical danger-case preconditions remain authoritative.

Issue 05 therefore preserves callback evidence only when:

- reported relative velocity is finite and effectively zero;
- the contacted collider has no attached Rigidbody or ArticulationBody;
- callback-time body velocity and collision impulse are finite;
- impulse is nonzero;
- body mass is finite and positive;
- reconstructed velocity is finite and nonzero.

Run Contact Classifier is unchanged and continues to project the resulting velocity onto each finite contact normal before applying the configured threshold. The high-tangent/below-normal-threshold test protects scrape semantics.

## Verification evidence

- Compile gates: clean through the project-local Unity AI Agent Connector.
- Final production-scene fixture `r_xb9yt4vl`: 5/5 passed.
- Final resolver, notifier, classifier, and production-scene focused run `r_nrlm1cd4`: 24/24 passed.
- Final related Run End, fixed-step pipeline, production scene composition/speed ownership, course scene, and contact-physics run `r_zglu5o9e`: 32/32 passed.
- Final full project suite `r_eutc2p41`: 1081/1081 passed on its first run.
- Focused calculator `r_0dtilny2`: 8/8 passed.
- Focused notifier `r_2ok87hdh`: 4/4 passed.
- Focused classifier `r_oqrq1027`: 7/7 passed.
- First production-scene GREEN `r_c5dgxemn`: 6/6 passed.
- Probe-free production-scene repetition `r_v59y86r5`: 6/6 passed.
- Post-refactor combined focused run `r_cjwk5ky3`: 25/25 passed.
- Related Run End, fixed-step pipeline, production scene composition/speed ownership, course scene, and contact-physics run `r_spwq0olx`: 32/32 passed.
- Revised production-scene runs `r_n6c4q4zc` and `r_ztvuqibo`: 6/6 passed independently.
- Revised combined focused run `r_vr8r8cve`: 25/25 passed.
- Revised related regression run `r_yn2p0lte`: 32/32 passed.
- Full project suite before rejecting the control `r_jc2o4qxs`: 1081/1082; only the invalid Discrete negative control failed.
- Authorized synchronized diagnostic `r_xixzvr0o`: 1/2; the Continuous Dynamic positive passed, while synchronized Discrete detected two contacts and accepted one ObstacleHit, falsifying the negative-control requirement.
- Manual Unity smoke: not run; deterministic production-scene PlayMode evidence is the accepted oracle.
- Jobs/Burst guard: not applicable; no Jobs or Burst code changed.

No retries are used as an oracle. The earlier isolated repetitions and the single synchronized experiment are retained as diagnostic evidence, not acceptance.

## Compatibility and follow-up audit

- No public API, serialized field, save format, scene, prefab, ScriptableObject, package, ProjectSettings, fixed-timestep, layer-matrix, or global physics change.
- No trigger widening, obstacle conversion, sweep, per-fixed-step query, static service, new dependency, or VContainer composition change.
- Production sphere authority, solid collision response, explicit Run Contact Category, contact-normal threshold, Run End candidate boundary, priority `Finished > ObstacleHit > OutOfBounds > LostMomentum`, and one-result latch remain unchanged.
- The runtime cost is limited to reading callback data and a small pure calculation only when collision callbacks occur; mobile sweep profiling is not applicable.
- No package version or save migration.
- Release-note follow-up: record the shipped static-obstacle approach-velocity correction in the project's next gameplay release notes if that release process requires player-visible behavior fixes.
- Revalidate these assumptions after a Unity/PhysX upgrade, physics-backend change, moving-obstacle authorization, fixed-timestep change, or supported-speed change.

## Sources

- [Unity Continuous Collision Detection manual](https://docs.unity3d.com/6000.3/Documentation/Manual/ContinuousCollisionDetection.html)
- [Unity sweep-based CCD manual](https://docs.unity3d.com/6000.3/Documentation/Manual/sweep-based-ccd.html)
- [Unity fixed updates manual](https://docs.unity3d.com/6000.3/Documentation/Manual/fixed-updates.html)
- [Unity Rigidbody collision mode guidance](https://docs.unity3d.com/6000.3/Documentation/Manual/physics-optimization-cpu-rigidbody-collision-modes.html)
- [PhysX pair flags and trigger-shape CCD limitation](https://nvidia-omniverse.github.io/PhysX/physx/5.2.1/_build/physx/latest/struct_px_pair_flag.html)
- [Shared ChatGPT discussion supplied as secondary context](https://chatgpt.com/share/6a5619c1-75d8-83eb-a147-cc1bb338541e)

Repository assets, executable tests, and the Unity/PhysX primary documentation are the decision authorities. The shared discussion is context only.
