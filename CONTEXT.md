# Context Entry

This repository uses multiple project glossaries.

Start with `CONTEXT-MAP.md`, then open the context that matches the topic you are discussing.

`CONTEXT.md` files define project language only: terms, relationships, example dialogue, and flagged ambiguities.

## Repository Language

**Authoring Guidance**:
Reviewer-facing explanation attached to tweakable serialized values so their purpose, impact, units, and safe adjustment range are understandable in the Unity Inspector.
_Avoid_: Tooltip, inline docs, tuning docs

## Relationships

- **Authoring Guidance** may be implemented with Unity tooltips, headers, validation attributes, or editor-only presentation.
- **Authoring Guidance** explains authored values; it does not define gameplay behavior.
- **Authoring Guidance** belongs on tweakable or non-obvious serialized values, not on every serialized reference.
- Substantive **Authoring Guidance** explains what a value controls, the expected impact of changing it, and its typical or safe adjustment range.

## Example dialogue

> **Dev:** "Should this serialized field have a tooltip?"
> **Domain expert:** "Only if it needs **Authoring Guidance** because a reviewer or designer would not immediately know whether to tweak it."

> **Dev:** "How much should the guidance say?"
> **Domain expert:** "Enough to understand what the value controls, what changes when it is adjusted, and what range is reasonable."

## Flagged ambiguities

- "Tooltip" resolves to Unity's display mechanism; **Authoring Guidance** resolves to the project intent behind the explanation.
- "Document every serialized field" is not **Authoring Guidance**; prefer signal on values whose adjustment meaning is not obvious.
