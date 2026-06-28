---
id: ADR-0007
number: 7
title: "Centralize Unity Input Behind UnityInput"
status: approved
date: 2026-06-25
deciders: []
tags: ["input", "unity-input-system", "enhanced-touch"]
components: ["Input", "Gameplay"]
supersedes: []
superseded_by: []
code_refs: []
test_refs: []
issue_refs: []
summary: "Centralize Unity Enhanced Touch enablement and pointer event translation behind a root-scoped UnityInput service."
---

# ADR-0007: Centralize Unity Input Behind UnityInput

## Summary

Centralize Unity Enhanced Touch enablement and generic pointer event translation behind a root-scoped `UnityInput` service. Features must not call
`EnhancedTouchSupport.Enable()` or `EnhancedTouchSupport.Disable()` directly.

## Context

Unity Enhanced Touch is controlled through process-wide static APIs. Multiple consumers can need input at the same time, so direct feature-level
enable/disable calls risk disabling shared input out from under another consumer.

## Decision

We will expose narrow interfaces such as `IEnhancedTouchSupportApi`, `IEnhancedTouchPointerInput`, and the composed `IUnityInput` through a
root-scoped `UnityInput` service. Enhanced Touch enablement will use ref-counted `IDisposable` handles so multiple consumers can share input safely.

## Alternatives considered

- **Let features call Enhanced Touch static APIs directly:** Simple, but creates process-wide ownership conflicts between consumers.
- **Wrap only pointer translation:** Reduces API surface, but still leaves enablement policy scattered.
- **Use one global input flag without handles:** Centralizes state, but makes ownership and disposal less explicit.

## Consequences

- Positive: Enhanced Touch process-wide state has one owner.
- Positive: Multiple consumers can share input through ref-counted handles.
- Negative: Input consumers must go through the project-owned input abstraction.
- Neutral: Static Unity APIs remain wrapped behind testable interfaces.

## Validation

- Code paths: Not implemented in the current tree.
- Tests or checks: ADR validation and index generation; implementation should test ref-counted enablement and pointer event translation.
- Review trigger: Revisit before adding direct feature calls to Enhanced Touch static APIs or changing root input ownership.

## Supersession

- Supersedes: None
- Superseded by: None
