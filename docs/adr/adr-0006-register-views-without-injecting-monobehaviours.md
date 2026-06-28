---
id: ADR-0006
number: 6
title: "Register Views Without Injecting MonoBehaviours"
status: approved
date: 2026-06-25
deciders: []
tags: ["vcontainer", "monobehaviour", "views", "composition"]
components: ["Gameplay", "Composition"]
supersedes: []
superseded_by: []
code_refs: []
test_refs: []
issue_refs: []
summary: "Register existing scene views as interface instances so controllers can use them without injecting dependencies into MonoBehaviours."
---

# ADR-0006: Register Views Without Injecting MonoBehaviours

## Summary

Scene MonoBehaviours should remain shallow views and adapters. Composition roots may register existing scene views as interface instances for plain C#
controllers to consume.

## Context

The project wants MonoBehaviours to avoid service references, dependency injection responsibilities, and VContainer lifecycle ownership. Controllers
still need access to scene-authored views through narrow interfaces.

## Decision

We will register existing scene views as interface instances in composition roots. Plain C# controllers own subscriptions and behavior coordination,
while scene MonoBehaviours remain shallow views/adapters without injected dependencies.

## Alternatives considered

- **Inject dependencies into MonoBehaviours:** Convenient, but expands view responsibilities and couples scene objects to DI lifecycle behavior.
- **Have views perform scene lookup:** Avoids DI wiring, but hides dependencies and spreads lookup policy through scene scripts.
- **Make controllers find views directly:** Reduces composition code, but couples controllers back to Unity scene structure.

## Consequences

- Positive: Views stay shallow and focused on Unity-facing responsibilities.
- Positive: Controllers can depend on narrow view interfaces and own behavior coordination.
- Negative: Composition roots must explicitly register scene views.
- Neutral: Scene-authored references remain part of composition.

## Validation

- Code paths: Not implemented in the current tree.
- Tests or checks: ADR validation and index generation; implementation should test controller behavior through view interfaces.
- Review trigger: Revisit if MonoBehaviours start receiving injected services or owning controller subscriptions.

## Supersession

- Supersedes: None
- Superseded by: None
