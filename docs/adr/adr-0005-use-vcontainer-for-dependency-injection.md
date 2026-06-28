---
id: ADR-0005
number: 5
title: "Use VContainer For Dependency Injection"
status: approved
date: 2026-06-25
deciders: []
tags: ["dependency-injection", "vcontainer", "composition"]
components: ["Gameplay", "Composition"]
supersedes: []
superseded_by: []
code_refs: []
test_refs: []
issue_refs: []
summary: "Use VContainer for dependency injection so gameplay services, controllers, and Unity adapters are composed through explicit LifetimeScopes."
---

# ADR-0005: Use VContainer For Dependency Injection

## Summary

Use VContainer as the project dependency injection package. Gameplay features should compose plain C# services, controllers, and shallow Unity
adapters through explicit LifetimeScopes.

## Context

The architecture favors plain C# gameplay logic and shallow Unity adapters. Those dependencies need explicit composition without service locators or
scene lookup, and the third-party dependency source should stay explicit.

## Decision

We will use VContainer for dependency injection. The package should be added early via a direct UPM Git URL pinned to a release tag rather than
through an OpenUPM scoped registry for now.

## Alternatives considered

- **Service locators or scene lookup:** Easy to start, but hides dependencies and weakens testability.
- **Manual composition only:** Keeps dependencies explicit, but becomes repetitive as gameplay controllers and adapters grow.
- **OpenUPM scoped registry:** Useful for package distribution, but introduces a project-wide registry decision not needed yet.

## Consequences

- Positive: Dependencies are explicit and composition is centralized in LifetimeScopes.
- Positive: Plain C# gameplay objects can be wired without direct scene lookup.
- Negative: The project takes on VContainer as a third-party dependency.
- Neutral: Package sourcing remains explicit through a pinned UPM Git URL.

## Validation

- Code paths: Not implemented in the current tree.
- Tests or checks: ADR validation and index generation; implementation should verify composition roots and controller injection behavior.
- Review trigger: Revisit before changing dependency injection framework, package source policy, or composition root ownership.

## Supersession

- Supersedes: None
- Superseded by: None
