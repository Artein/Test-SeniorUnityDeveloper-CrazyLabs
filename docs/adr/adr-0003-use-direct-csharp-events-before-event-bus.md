---
id: ADR-0003
number: 3
title: "Use Direct C# Events Before Event Bus"
status: approved
date: 2026-06-25
deciders: []
tags: ["events", "coordination", "gameplay"]
components: ["Gameplay"]
supersedes: []
superseded_by: []
code_refs: []
test_refs: []
issue_refs: []
summary: "Use direct C# events for local gameplay coordination before introducing a global event bus."
---

# ADR-0003: Use Direct C# Events Before Event Bus

## Summary

Feature controllers should coordinate local gameplay behavior through direct C# event subscriptions before the project introduces a global message bus
or event bus.

## Context

The current gameplay coordination scope is local to the slingshot and gameplay-flow slice. A global bus would add indirection and project-wide policy
before the codebase has enough coordination complexity to justify it.

## Decision

We will use direct C# events for local gameplay coordination. A broader event bus remains a later project-wide architecture decision if coordination
complexity grows.

## Alternatives considered

- **Global event bus now:** Could decouple publishers and subscribers, but adds invisible coupling and architecture surface before it is needed.
- **Direct method calls only:** Very explicit, but less flexible for notification-style coordination among peer controllers.
- **Do nothing:** Risks inconsistent coordination patterns as features are added.

## Consequences

- Positive: Direct subscriptions keep coordination visible and easy to trace.
- Positive: The project avoids premature event-bus policy.
- Negative: Local coupling is intentional and must be reviewed as gameplay coordination grows.
- Follow-up: Revisit when multiple independent systems need shared, cross-cutting event flow.

## Validation

- Code paths: Not implemented in the current tree.
- Tests or checks: ADR validation and index generation; implementation should test event subscription and unsubscription behavior.
- Review trigger: Revisit if direct event chains become hard to trace or span unrelated gameplay systems.

## Supersession

- Supersedes: None
- Superseded by: None
