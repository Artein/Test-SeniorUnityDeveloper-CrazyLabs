---
id: ADR-0010
number: 10
title: "Use Explicit Run Body Speed Model With Rigidbody Contact Physics"
status: approved
date: 2026-07-10
deciders: ["Artem Sukhliak"]
tags: ["gameplay", "movement", "physics", "rigidbody", "speed-ownership"]
components: ["Gameplay", "Physics", "Composition"]
supersedes: []
superseded_by: []
code_refs:
  - "Assets/Game/Gameplay/RunBodyMovementController.cs"
  - "Assets/Game/Gameplay/DefaultRunBodySpeedEvaluator.cs"
  - "Assets/Game/Gameplay/RunBodyMovementConfig.cs"
  - "Assets/Game/Gameplay/RunSurfaceContext.cs"
  - "Assets/Game/Gameplay/RunBodyVelocitySanityGuard.cs"
  - "Assets/Game/Gameplay/RunGameplayStatResolver.cs"
  - "Assets/Game/Gameplay/GameplayLifetimeScope.cs"
test_refs:
  - "Assets/Game/Gameplay/Tests/EditMode/DefaultRunBodySpeedEvaluatorTests.cs"
  - "Assets/Game/Gameplay/Tests/EditMode/RunBodyMovementControllerTests.cs"
  - "Assets/Game/Gameplay/Tests/EditMode/RunBodyVelocitySanityGuardTests.cs"
  - "Assets/Game/Gameplay/Tests/EditMode/RunGameplayStatResolverTests.cs"
  - "Assets/Game/Gameplay/Tests/PlayMode/RunBodyContactPhysicsPlayModeTests.cs"
  - "Assets/Game/Gameplay/Tests/PlayMode/RunBodySpeedModelPlayModeTests.cs"
  - "Assets/Game/Gameplay/Tests/PlayMode/GameplaySceneRunBodySpeedOwnershipTests.cs"
issue_refs:
  - "docs/diagrams/run-body-speed-model.md"
  - "docs/prd/prd-run-body-explicit-speed-ownership.md"
  - "docs/prd/prd-run-body-natural-speed-ownership.md"
  - "docs/tasks/run-body-natural-speed-ownership/06-player-max-speed-product-decision.md"
summary: "Use an explicit gameplay model for intentional grounded Run Body tangent speed while Unity Rigidbody physics retains gravity, contacts, and collision response."
---

# ADR-0010: Use Explicit Run Body Speed Model With Rigidbody Contact Physics

## Summary

We will use an explicit gameplay model for intentional grounded **Run Body** tangent-speed outcomes while Unity Rigidbody physics retains gravity, contacts, collision response, and separation. This makes speed tunable and testable without replacing the contact physics that support obstacle gameplay.

## Context

Before this change, the steering flow sanitized and rotated existing Rigidbody velocity but largely preserved its magnitude. Player-facing speed therefore emerged from launch impulse, gravity, contact geometry, physics materials, collisions, and solver behavior.

That emergent model does not provide a clear gameplay authority for slope acceleration, ordinary slowdown, recoverable near-stalls, the upgraded `PlayerMaxSpeed` envelope, or high-speed readability. Changing geometry, materials, launch tuning, or solver behavior can alter speed feel through unrelated side effects.

Rigidbody physics remains valuable for gravity, contacts, collision response, and surface separation. The architectural gap is intentional gameplay speed ownership, not Unity physics itself. ADR-0002 and ADR-0005 further constrain the solution toward plain C# gameplay policy composed through VContainer.

This decision preserves direction-only steering, airborne Rigidbody behavior, corrected surface-normal velocity, and the defensive **Run Body Speed Sanity Guard**. It does not decide exact formulas, first-slice API shapes, presentation behavior, or future surface-specific profiles.

## Decision

We will introduce a **Run Body Speed Model** that owns intentional surface-tangent speed outcomes while the **Run Body** is supported by a valid grounded **Run Surface**.

Unity Rigidbody physics will continue to own gravity, contacts, collision response, separation, and externally produced surface-normal velocity. The speed model will not choose travel heading, add airborne speed behavior, or become a full custom physics engine.

One movement orchestration boundary will compose speed policy, direction-only steering, landing correction, and defensive velocity sanitation into one final Rigidbody target-state write per fixed step. Multiple active movement writers are forbidden.

`PlayerMaxSpeed` will define a soft gameplay speed envelope rather than an immediate hard velocity clamp. The **Run Body Speed Sanity Guard** will remain a separate defensive boundary for impossible physics values.

Designer-authored movement values will use validated configuration. Gameplay policy will remain plain C# and be composed through VContainer. Exact formulas, tuning fields, first-slice APIs, and deferred surface profiles belong in the replacement PRD and the engineering diagram.

## Alternatives considered

- **Keep speed emergent from Rigidbody physics:** Lowest implementation cost, but speed remains coupled to launch tuning, gravity, geometry, materials, contacts, and solver behavior. Designer intent and deterministic tests remain weak.
- **Tune speed only through physics materials and course geometry:** Provides surface variation but cannot cleanly own upgrades, bounded recovery, or a global soft envelope. Geometry and contact tuning retain unrelated side effects.
- **Add a hard velocity cap to steering or the Rigidbody adapter:** Simple and makes `PlayerMaxSpeed` immediately visible, but recreates the hidden speed wall and mixes speed ownership with steering or infrastructure.
- **Replace Rigidbody movement with fully custom or kinematic physics:** Gives maximum control but unnecessarily takes ownership of collision response, gravity, separation, and obstacle interaction.
- **Use an explicit tangent-speed model with Rigidbody contact physics:** Chosen because it provides intentional, testable speed outcomes while retaining Unity contact and collision behavior.

## Consequences

- Positive: Designers gain explicit controls for grounded acceleration, slowdown, recovery, and the soft speed envelope.
- Positive: `PlayerMaxSpeed` gains a testable gameplay meaning without becoming a hidden hard wall.
- Positive: Steering, speed policy, and defensive velocity sanitation have distinct ownership.
- Positive: Speed behavior becomes suitable for focused EditMode tests while retaining Rigidbody obstacle interaction.
- Negative: Gameplay-authored slowdown can combine with physics friction, so surface and model tuning must avoid unintended double damping.
- Negative: A composed target velocity can fight contact resolution if ordering or supported-surface validity is wrong.
- Negative: The architecture adds policy, orchestration, validation, diagnostics, and bounded recovery state that must be maintained.
- Neutral: Airborne movement, gravity, collision response, and launch impulse remain Rigidbody-driven.
- Neutral: Surface-specific profiles remain deferred until distinct surfaces require separate authored behavior.
- Follow-up: Implement focused behavior and integration tests defined by the replacement PRD.
- Migration: Move existing steering-owned Rigidbody writes behind the single movement orchestration boundary; do not register the legacy and replacement movement writers together.

## Validation

- Code paths: Review the movement orchestrator, speed evaluator, steering evaluator, Rigidbody target adapter, Run Surface validity mapping, configuration validation, stat resolution, and VContainer composition.
- Tests or checks: Cover slope acceleration, slowdown, soft-envelope resistance, neutral unsupported behavior, bounded low-speed assistance, upgraded `PlayerMaxSpeed`, preserved corrected surface-normal velocity, one final movement write, Editor and startup validation, launch and landing transitions, airborne behavior, collision overspeed, and high-speed obstacle approach. Solver-backed PlayMode coverage must observe both the controller target write and the Rigidbody velocity after the following PhysX step so contact ownership is tested rather than mocked.
- Review trigger: Revisit this ADR before introducing a hard gameplay speed cap, airborne speed policy, a second movement writer, surface-specific speed profiles, or replacement of Rigidbody contact physics.

## Supersession

- Supersedes: None
- Superseded by: None
