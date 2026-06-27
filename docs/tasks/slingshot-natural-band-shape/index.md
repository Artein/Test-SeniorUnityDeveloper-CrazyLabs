# Slingshot Natural Band Shape Implementation Issues

Parent PRD: [Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

Related follow-up task set: [Slingshot Target-Coupled Band Implementation Issues](../slingshot-target-coupled-band/index.md)

Related ADR: [ADR-0008: Use Deterministic Taut Band Shape Solver Instead Of Rope Physics](../../adr/adr-0008-use-deterministic-taut-band-shape-solver-instead-of-rope-physics.md)

These local implementation issues are ordered by dependency. They are follow-up tracer-bullet slices for replacing the current closest-point/arc-based Band approximation with a deterministic natural Band Shape around the Launch Target Silhouette.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Align Natural Band Shape Docs And Contracts](01-align-natural-band-shape-docs-and-contracts.md) | AFK | None | 60-63 |
| 02 | [Add Pure Pull Plane Taut Band Solver](02-add-pure-pull-plane-taut-band-solver.md) | AFK | 01 | 61-63 |
| 03 | [Add Collider Silhouette Source And Band Shape Provider](03-add-collider-silhouette-source-and-band-shape-provider.md) | AFK | 02 | 61-63 |
| 04 | [Integrate Natural Band Shape Into Pull And Recoil](04-integrate-natural-band-shape-into-pull-and-recoil.md) | AFK | 03 | 10, 23-24, 61-63 |
| 05 | [Wire GameplayScene Natural Band Shape](05-wire-gameplayscene-natural-band-shape.md) | AFK | 04 | 1-15, 20-24, 58, 61-63 |
| 06 | [Run Natural Band Human Feel And Authoring QA](06-run-natural-band-human-feel-and-authoring-qa.md) | HITL | 05 | 2-4, 7-12, 16-22, 61-63 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Do not create a separate PRD for this follow-up unless the product scope changes.
- Keep MonoBehaviours shallow and register existing scene views/adapters through composition.
- Keep Unity Collider access at an adapter boundary; pure Band Shape geometry should remain EditMode-testable.
- Run Unity compile before tests on implementation slices.
- Prefer EditMode tests for pure geometry/controller behavior and PlayMode only for Collider, Rigidbody, LineRenderer, and scene-boundary behavior.
- No Burst/job implementation is required in these slices; optimize later only if profiling proves plain C# too slow.
