# Run End Flow First Slice Implementation Issues

Parent PRD: [Run End Flow First Slice PRD](../../prd/prd-run-end-flow-first-slice.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0004: Defer Structured Logging Backend](../../adr/adr-0004-💡-defer-structured-logging-backend.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
- [ADR-0009: Use Cinemachine For Run Camera](../../adr/adr-0009-use-cinemachine-for-run-camera.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for ending a **Run** from finish, safety net, obstacle
impact, or **Lost Momentum**, logging one **Run Result**, and returning to **Pre-Launch**.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Add Run Progress Frame And RunProgressService](01-add-run-progress-frame-and-runprogressservice.md) | AFK | None | 7-15, 38-45, 61-62, 70, 73-74, 77 |
| 02 | [Add Run Contact Metadata And Target Notifications](02-add-run-contact-metadata-and-target-notifications.md) | AFK | None | 16-22, 46, 50-53, 65-66, 71-72, 73-75 |
| 03 | [End Runs From Finish Safety Net And Obstacle Impact](03-end-runs-from-finish-safety-net-and-obstacle-impact.md) | AFK | 01, 02 | 1-4, 6, 22, 24-25, 28-37, 54-58, 65-68, 73-76 |
| 04 | [End Runs From Lost Momentum](04-end-runs-from-lost-momentum.md) | AFK | 01, 03 | 5, 9, 26-27, 48, 63-64 |
| 05 | [Harden Candidate Priority And Result Latching](05-harden-candidate-priority-and-result-latching.md) | AFK | 03, 04 | 29-34, 67-68 |
| 06 | [Verify Gameplay Scene Composition And Smoke Paths](06-verify-gameplay-scene-composition-and-smoke-paths.md) | AFK | 01-05 | 16-23, 69-72, 77 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep **Run Contact Category** first-slice values to `Surface`, `Obstacle`, `SafetyNet`, and `Finish`.
- Do not add **Run Boundary** or **Run Ramp** in this first slice.
- Keep MonoBehaviours shallow and register scene adapters through VContainer composition.
- Use direct C# events for local notifications before introducing any event bus.
- Use Unity `Debug.Log` for first-slice **Run Result** output; analytics and structured logging are deferred.
- Run Unity compile before tests on implementation slices.
- Prefer EditMode tests for pure progress, classification, **Lost Momentum**, and **Run End Flow** behavior.
- Use PlayMode tests for Rigidbody/contact adapter behavior and Gameplay Scene composition.
