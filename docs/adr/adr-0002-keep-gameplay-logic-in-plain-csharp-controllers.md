---
id: ADR-0002
number: 2
title: "Keep Gameplay Logic In Plain C# Controllers"
status: approved
date: 2026-06-25
deciders: []
tags: ["gameplay", "testability", "monobehaviour"]
components: ["Gameplay"]
supersedes: []
superseded_by: []
code_refs: []
test_refs: []
issue_refs: []
summary: "Keep gameplay rules in plain C# controllers and services, with MonoBehaviours limited to shallow Unity adapter responsibilities."
---

# ADR-0002: Keep Gameplay Logic In Plain C# Controllers

## Summary

Gameplay behavior should live in plain C# controllers and services. MonoBehaviours should stay shallow and handle Unity callbacks, serialized scene
data, and visual references.

## Context

Gameplay rules need deterministic tests that do not rely on Unity lifecycle callbacks. At the same time, scenes and prefabs remain useful for authored
composition, visual references, and Unity-specific entry points.

## Decision

We will put gameplay rules in plain C# controllers and services, and use MonoBehaviours as shallow Unity adapters around those objects.

## Alternatives considered

- **Put behavior directly in MonoBehaviours:** Familiar in Unity, but it ties rules to lifecycle order and makes EditMode tests harder.
- **Use pure data-only scenes with no adapters:** Keeps logic pure, but loses practical Unity authoring and callback integration points.
- **Do nothing:** Would preserve short-term speed but increase coupling between rules, scene objects, and runtime lifecycle behavior.

## Consequences

- Positive: Gameplay rules can be tested without relying on runtime MonoBehaviour callbacks.
- Positive: Unity scenes and prefabs remain available for composition and visuals.
- Negative: Adapters and composition roots must be kept thin and explicit.
- Neutral: Some PlayMode tests may still be needed for engine behavior and scene integration.

## Validation

- Code paths: Not implemented in the current tree.
- Tests or checks: ADR validation and index generation; implementation should prefer EditMode tests for plain C# controllers.
- Review trigger: Revisit if gameplay behavior starts accumulating inside MonoBehaviours.

## Supersession

- Supersedes: None
- Superseded by: None
