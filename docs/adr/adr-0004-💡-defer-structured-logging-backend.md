---
id: ADR-0004
number: 4
title: "Defer Structured Logging Backend"
status: proposed
date: 2026-06-25
deciders: []
tags: ["logging", "observability", "unity"]
components: ["Gameplay", "Tooling"]
supersedes: []
superseded_by: []
code_refs: []
test_refs: []
issue_refs: []
summary: "Use the existing Unity logging path for warnings for now and defer project-owned structured logging to a separate architecture change."
---

# ADR-0004: Defer Structured Logging Backend

## Summary

Gameplay services should use the existing Unity logging path for warnings for now. Project-owned structured logging is still desirable later, but it
should be handled as a separate architecture change.

## Context

Gameplay systems will eventually benefit from consistent event names and structured fields. However, adding `com.unity.logging` or a custom backend
now would expand dependency and observability policy while `com.unity.logging` is deprecated beyond Unity 6.2 and this repository appears to target a
newer Unity version.

## Decision

We will defer a structured logging backend. For now, gameplay services should use the existing Unity logging path for warnings.

## Alternatives considered

- **Add `com.unity.logging` now:** Provides structured logging, but conflicts with the package deprecation concern for newer Unity versions.
- **Build a custom structured backend now:** Gives project control, but it is a separate architecture decision and premature for the current slice.
- **No logging policy:** Avoids work now, but leaves gameplay warnings inconsistent.

## Consequences

- Positive: The project avoids committing to a deprecated or premature logging dependency.
- Negative: Gameplay logging remains less structured until a later architecture decision.
- Neutral: Existing Unity warning logs remain acceptable for current gameplay services.
- Follow-up: Define a project-owned structured logging approach when observability requirements are clearer.

## Validation

- Code paths: Not implemented in the current tree.
- Tests or checks: ADR validation and index generation.
- Review trigger: Revisit before adding a logging package, custom logging backend, or cross-system observability policy.

## Supersession

- Supersedes: None
- Superseded by: None
