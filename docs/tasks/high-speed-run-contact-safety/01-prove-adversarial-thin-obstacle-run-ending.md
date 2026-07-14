# 01 — Prove Adversarial Thin-Obstacle Run Ending

## Parent

[High-Speed Run Contact Safety PRD](../../prd/prd-high-speed-run-contact-safety.md) — user stories 1–2, 5, 7–8, 10–13, 19–26, 29–33, 35–38, 49, 53, 56, 59–60.

## What to build

Add a deterministic PlayMode tracer bullet that drives the production Gameplay scene Run Body through a deliberately thin, solid Run Obstacle at the supported 40 m/s tier. Exercise the authored 0.35-meter Run Body sphere, production Rigidbody Contact Notifier, Run Contact Classifier, Run End candidate receiver, Run End Flow, Gameplay State, and accepted Run Result. Derive the fixed-step phase and contact envelope mathematically so both planned sampled endpoints are clear of the obstacle. Keep arrangement geometry and diagnostics in the PlayMode test assembly.

The owner-authorized diagnostic that assigned Discrete during PreLaunch, waited exactly one fixed step, then launched still detected the collision. Therefore Discrete-must-miss is rejected as an acceptance criterion rather than normalized through retries or thinner geometry.

## Acceptance criteria

- [ ] The positive scenario uses the production Gameplay scene and authoritative Run Body Contact Collider rather than a synthetic larger body.
- [ ] The test asserts that the production contact collider is a sphere with a 0.35-meter authored radius and that the production Rigidbody uses Continuous Dynamic collision detection.
- [ ] The temporary Run Obstacle uses a non-trigger collider and explicit Obstacle Run Contact Category.
- [ ] At 40 m/s and the project fixed timestep, fixed-step displacement exceeds the projected sphere-plus-obstacle overlap span along the crossing axis.
- [ ] A helper derives the planned pre-step and post-step centers from live collider geometry, contact offsets, obstacle thickness, speed, and fixed timestep; neither sampled pose lies inside the calculated contact envelope.
- [ ] Acceptance does not depend on mutating the production Rigidbody collision mode or requiring Discrete to miss.
- [ ] The Continuous Dynamic scenario produces an obstacle collision observation and exactly one accepted Run Result with reason ObstacleHit.
- [ ] Gameplay State reaches Run Ended through the existing Run End Flow.
- [ ] The approach velocity's contact-normal component exceeds the configured Obstacle Impact threshold without weakening scrape semantics.
- [ ] The scenario preserves solid physical collision response; it does not convert the obstacle into a trigger.
- [ ] The existing 80 m/s scenario remains separate diagnostic headroom and is reported independently from 40 m/s acceptance.
- [ ] Production-scene setup uses scene reload, isolated save state, condition-based bounded fixed-step polling, and no time-based sleeps.
- [ ] Failure output includes speed, fixed timestep, displacement, body radius, obstacle thickness, collision mode, start/end positions, contact count, result count, and final Gameplay State.
- [ ] A nondeterministic or flaky result stops the work for investigation rather than being normalized with retries.
- [ ] No runtime code, scene, prefab, ScriptableObject, package, or ProjectSettings changes are introduced solely to arrange the proof.

## Verification

- Compile gate: Compile the exact project Unity version through the Unity AI Agent Connector; resolve every compile error before running tests.
- PlayMode tests: Run the filtered production Continuous Dynamic scenario `given_MaxUpgradedSpeedProductionRunBody_when_CrossingAdversarialThinObstacle_then_ObstacleHitEndsRun`; retain the rejected synchronized Discrete result in the evidence decision, not as a permanent test.
- PlayMode regression: Run the existing production high-speed obstacle and isolated thin-obstacle notifier scenarios.
- EditMode tests: Not required unless a pure geometry helper is placed in an EditMode-accessible test assembly.
- Static checks: Confirm the obstacle remains non-trigger, explicitly categorized, and absent from shipped scene or prefab content.
- Manual Unity smoke check: Not required when deterministic production-scene PlayMode evidence passes.
- Package version/changelog: Not required; this is test-only safety evidence.

## Blocked by

None — can start immediately.
