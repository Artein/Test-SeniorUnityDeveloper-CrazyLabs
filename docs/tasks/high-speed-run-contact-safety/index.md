# High-Speed Run Contact Safety Implementation Issues

Parent PRD: [High-Speed Run Contact Safety](../../prd/prd-high-speed-run-contact-safety.md)

Related architecture decisions:

- [ADR-0002: Keep Gameplay Logic in Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0005: Use VContainer for Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0010: Use an Explicit Run Body Speed Model with Rigidbody Contact Physics](../../adr/adr-0010-use-explicit-run-body-speed-model-with-rigidbody-contact-physics.md)

## Delivery order

| ID | Title | Type | Blocked by | User stories |
|---|---|---|---|---|
| 01 | [Prove Adversarial Thin-Obstacle Run Ending](01-prove-adversarial-thin-obstacle-run-ending.md) | AFK | None | 1–2, 5, 7–8, 10–13, 19–26, 29–33, 35–38, 49, 53, 56, 59–60 |
| 02 | [Prove Authored Run Finish Traversal](02-prove-authored-run-finish-traversal.md) | AFK | None | 4–6, 10–12, 14, 16–20, 27, 29–33, 35–37, 49, 56, 59–60 |
| 03 | [Prove Authored Run Safety Net Traversal](03-prove-authored-run-safety-net-traversal.md) | AFK | 02 | 3, 5, 10–12, 15–20, 28–33, 35–37, 49, 56, 59–60 |
| 04 | [Approve Contact-Safety Evidence and Remediation Boundary](04-approve-contact-safety-evidence-and-remediation-boundary.md) | HITL | 01–03 | 18, 36–38, 50, 55–56, 58, 60 |

## Delivery notes

- The supported acceptance tier is 40 m/s. The 80 m/s tier is diagnostic stress headroom, not a product guarantee; report both separately.
- Solid Run Obstacles and trigger-driven Run Finish or Run Safety Net are independent detection contracts and remain separate proof slices.
- Issues 01–03 are evidence-first and test-only. Passing evidence is a valid outcome and must not cause production, scene, prefab, ScriptableObject, package, or ProjectSettings churn.
- Issue 03 follows Issue 02 so it can reuse the trigger traversal and projected-margin harness without duplicating test infrastructure.
- Issue 04 is the only remediation gate. If evidence is safe, close with no production fix. If evidence reproduces a miss, approve the smallest remedy before drafting additional issues.
- Conditional fallback stories 9, 34, 39–49, 51–54, and 57 are deliberately not claimed by these proof issues. They become input to a new breakdown only if Issue 04 authorizes remediation.
- Every implementation issue must pass the Unity compile gate before targeted tests. Flaky PlayMode physics is a stop condition.
- These are local planning issues only; nothing is published remotely.
