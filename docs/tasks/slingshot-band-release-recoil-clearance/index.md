# Slingshot Band Release Recoil Clearance Implementation Issues

Parent PRD: [Slingshot Band Release Recoil Clearance PRD](../../prd/prd-slingshot-band-release-recoil-clearance.md)

Related follow-up task set: [Slingshot Natural Band Shape Implementation Issues](../slingshot-natural-band-shape/index.md)

Related ADR: [ADR-0008: Use Deterministic Taut Band Shape Solver Instead Of Rope Physics](../../adr/adr-0008-use-deterministic-taut-band-shape-solver-instead-of-rope-physics.md)

These local implementation issues are ordered by dependency. They refine Band Release Recoil so the Band detaches only once the rest/idle/default Band Shape is clear of the current Launch Target Silhouette, including visible Band thickness.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Add Pull Plane Rest-Shape Clearance Gate](01-add-pull-plane-rest-shape-clearance-gate.md) | AFK | None | 11, 27-30, 33-36, 47, 51, 54-56 |
| 02 | [Use Clearance Gate In Low-Impulse Release Recoil](02-use-clearance-gate-in-low-impulse-release-recoil.md) | AFK | 01 | 1-10, 12-23, 27-29, 31-40, 42-46, 48, 50-56 |
| 03 | [Fail Closed On Recoil Solve Failure](03-fail-closed-on-recoil-solve-failure.md) | AFK | 02 | 21-26, 30-34, 47, 51, 54-56 |
| 04 | [Harden Detach Lifecycle And Regression Suite](04-harden-detach-lifecycle-and-regression-suite.md) | AFK | 02, 03 | 13, 15, 35-41, 48-56 |
| 05 | [Run Human Recoil Feel And Authoring QA](05-run-human-recoil-feel-and-authoring-qa.md) | HITL | 04 | 1-7, 10-13, 40, 55 |

## Notes

- HITL QA run sheet: [05-hitl-qa-run-sheet.md](05-hitl-qa-run-sheet.md).
- Do not publish these remotely unless explicitly requested.
- Keep Band Shape visual-only; launch power, launch direction, Pull Offset, and Rigidbody launch semantics should not change.
- Respect ADR-0008: keep deterministic Pull Plane taut solving and do not introduce runtime rope physics, joints, or Unity-physics-driven Band simulation.
- During Band Release Recoil, virtual Pull Point drives Pulled Side; do not use the launched target midpoint, bounds center, or Rigidbody position as the solver query point.
- Keep MonoBehaviours shallow and keep Unity Collider access at the existing silhouette adapter boundary.
- Run Unity compile before tests on implementation slices.
- Prefer EditMode tests for pure policy/controller behavior and PlayMode only for Collider, Rigidbody, LineRenderer, and scene-boundary behavior.
- No package version, changelog, manifest, save format, or ProjectSettings change is expected.
