---
id: ADR-0009
number: 9
title: "Use Cinemachine For Run Camera"
status: approved
date: 2026-06-28
deciders: []
tags: ["camera", "cinemachine", "run-camera", "composition"]
components: ["Gameplay", "Camera", "Composition"]
supersedes: []
superseded_by: []
code_refs: []
test_refs: []
issue_refs: []
summary: "Use Cinemachine for Run Camera composition, collision, occlusion, and blending while project code owns Run Camera Anchor motion and launch-gated activation."
---

# ADR-0009: Use Cinemachine For Run Camera

## Summary

Use Cinemachine as the **Run Camera** system. Cinemachine owns camera composition, damping, blending, and obstacle/terrain avoidance, while project
code owns **Run Camera Anchor** motion and activates the **Run Camera** only after `LaunchApplied` while the **Gameplay State** is Running.

## Context

The **Launch Target** slides, jumps, and moves around level objects during a **Run**. The camera must follow it without clipping below slope surfaces,
entering obstacle interiors, or inheriting unstable physics rotation. The project already keeps gameplay behavior in plain C# controllers and uses
shallow Unity adapters for scene-authored behavior.

## Decision

We will add Unity Cinemachine for **Run Camera** behavior. Project code will maintain a stable **Run Camera Anchor** derived from the **Launch Target**
and will gate camera activation on `LaunchApplied` plus the Running **Gameplay State**. Cinemachine components will follow that anchor and own framing,
damping, camera blends, collision resolution, line-of-sight deocclusion, and camera-object decollision where those features are needed.

## Alternatives considered

- **Custom follow and collision controller:** Keeps dependencies smaller, but requires project-owned camera smoothing, collision probing, terrain
  correction, line-of-sight handling, and blend behavior.
- **Follow the Launch Target transform directly:** Simple, but couples camera framing to physics jitter, slope contact, jumps, and rotation changes.
- **Physics-driven camera body:** Familiar as a collision metaphor, but likely to jitter and fight authored camera composition in a casual sliding game.

## Consequences

- Positive: Camera behavior can use Unity-supported Cinemachine tools for follow, blend, collision, deocclusion, and decollision.
- Positive: Gameplay code remains responsible for domain timing and **Run Camera Anchor** ownership instead of camera math.
- Negative: The project takes on a Cinemachine package dependency and scene/component conventions.
- Neutral: Camera obstacle behavior depends on deliberate collider and layer authoring.

## Validation

- Code paths: first implementation should update `Packages/manifest.json`, the gameplay scene camera setup, and gameplay camera lifecycle composition.
- Tests or checks: implementation should cover launch-gated activation, anchor ownership, package/scene composition, and PlayMode camera non-clipping
  scenarios where practical.
- Review trigger: Revisit before replacing Cinemachine with a custom camera collision/occlusion stack or allowing the **Run Camera** to follow the
  **Launch Target** transform directly.

## Supersession

- Supersedes: None
- Superseded by: None
