---
id: ADR-0001
number: 1
title: "Use Asset-Backed Gameplay State Ids"
status: approved
date: 2026-06-25
deciders: []
tags: ["gameplay-state", "scriptableobject", "flow"]
components: ["Gameplay"]
supersedes: []
superseded_by: []
code_refs: []
test_refs: []
issue_refs: []
summary: "Use ScriptableObject assets as gameplay state and transition identities so authored flow stays reference-based and decoupled from hardcoded identifiers."
---

# ADR-0001: Use Asset-Backed Gameplay State Ids

## Summary

Use `GameplayStateId` ScriptableObject assets to identify gameplay states and `GameplayStateTransition` ScriptableObject assets to identify allowed
state changes. Runtime validation compares authored assets by reference identity instead of names, GUIDs, enums, or duplicate serialized identifiers.

## Context

Gameplay features need stable authored identities for states and transitions without hardcoding flow details into feature components. The game has one
current Gameplay State at a time, starts from an explicit initial state provided by composition, and needs validation of allowed transitions.

## Decision

We will use asset-backed `GameplayStateId` values for gameplay states and separate asset-backed `GameplayStateTransition` values for allowed changes.
`GameplayStateService` stores and publishes the current state, and `GameplayFlowController` owns when transitions are requested.

## Alternatives considered

- **Enums or strings:** Simple to code, but they couple flow to hardcoded identifiers and make authored content less explicit.
- **GUID or secondary serialized identifiers:** Stable in theory, but they duplicate identity and add another value that can drift from the asset.
- **Simultaneous state tags:** More flexible, but unnecessary for the current one-state-at-a-time flow and harder to validate.

## Consequences

- Positive: Feature components can depend on authored state identities without knowing gameplay-flow details.
- Positive: Reference identity keeps runtime validation direct and avoids duplicate identifier fields.
- Negative: State and transition assets must be authored and wired correctly by composition.
- Neutral: The decision assumes one current Gameplay State at a time.

## Validation

- Code paths: Not implemented in the current tree.
- Tests or checks: ADR validation and index generation; implementation should add gameplay-state transition tests when code exists.
- Review trigger: Revisit if gameplay needs multiple simultaneous states, save-format state identifiers, or externally addressable state IDs.

## Supersession

- Supersedes: None
- Superseded by: None
