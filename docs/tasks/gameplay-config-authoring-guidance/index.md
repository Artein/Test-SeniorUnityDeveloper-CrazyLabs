# Gameplay Config Authoring Guidance Issues

Parent PRD: `docs/prd/prd-gameplay-config-authoring-guidance.md`

Approved local implementation issue set for first-pass gameplay config **Authoring Guidance**. Issues are ordered in dependency order. AFK slices can be implemented independently; the final HITL slice depends on the five implementation slices.

| ID | Title | Type | Blocked by | File |
| --- | --- | --- | --- | --- |
| 01 | Run Camera Authoring Guidance Tracer | AFK | None | `01-run-camera-authoring-guidance-tracer.md` |
| 02 | Slingshot Pull and Band Authoring Guidance | AFK | None | `02-slingshot-pull-and-band-authoring-guidance.md` |
| 03 | Launch Impulse Authoring Guidance | AFK | None | `03-launch-impulse-authoring-guidance.md` |
| 04 | Run Steering Control Authoring Guidance Standardization | AFK | None | `04-run-steering-control-authoring-guidance-standardization.md` |
| 05 | Run End Flow and Reward Authoring Guidance | AFK | None | `05-run-end-flow-and-reward-authoring-guidance.md` |
| 06 | Inspector Readability Smoke Review | HITL | 01-05 | `06-inspector-readability-smoke-review.md` |

## Shared Constraints

- Keep every implementation slice metadata-only.
- Preserve serialized field names, code defaults, validation attributes, validators, runtime fallback behavior, config interfaces, asmdefs, assets, scenes, prefabs, packages, and ProjectSettings.
- Keep guidance inline on serialized fields.
- Use concise guidance for simple fields and `Controls:\n\nImpact:\n\nTypical:` guidance for substantive settings.
- Do not add tests for static tooltip text unless scope expands into behavior, validators, custom editor behavior, defaults, or serialization.
- Run Rider/file problem inspection and Unity compile after C# metadata edits.
