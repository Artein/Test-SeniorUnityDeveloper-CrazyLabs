# Slingshot Pre-Launch Rig Pose Reset Implementation Issues

Parent PRD: [Slingshot Pre-Launch Rig Pose Reset PRD](../../prd/prd-slingshot-prelaunch-rig-pose-reset.md)

Related ADRs:

- [ADR-0001: Use Asset-Backed Gameplay State Ids](../../adr/adr-0001-use-asset-backed-gameplay-state-ids.md)
- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for restoring the authored **Pre-Launch Rig Pose** before
same-session **Pre-Launch** capture and proving that the **Launch Target** **Band Center** returns to the **Slingshot** **Rest Point** without scene
reload.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Add Pre-Launch Rig Pose Reset Boundary](01-add-pre-launch-rig-pose-reset-boundary.md) | AFK | None | 10-15, 20-23, 27-28, 35-38 |
| 02 | [Restore Launch Target Pose And Held Rigidbody Baseline](02-restore-launch-target-pose-and-held-rigidbody-baseline.md) | AFK | 01 | 5, 16-19, 25-30, 33 |
| 03 | [Refresh Slingshot Geometry On Capture Enable](03-refresh-slingshot-geometry-on-capture-enable.md) | AFK | None | 1-4, 12-13, 24, 29, 34 |
| 04 | [Wire GameplayScene Same-Session Restart Regression](04-wire-gameplayscene-same-session-restart-regression.md) | AFK | 01-03 | 1-10, 26, 31-34, 38 |
| 05 | [Run Human Restart Alignment QA](05-run-human-restart-alignment-qa.md) | HITL | 04 | 1-5, 31-34 |

## Notes

- Do not publish these remotely unless explicitly requested.
- **Pre-Launch Rig Pose** is the source of truth for same-session restart alignment.
- The **Rest Point** remains **Slingshot** geometry, not a whole-level spawn point.
- Reset must happen before `GameplayStateChanged` observers see **Pre-Launch**.
- **Slingshot** capture consumes already-reset geometry and refreshes its snapshot on capture enable.
- **Launch Target** restart restores authored rotation, but held **Pull** positioning remains position-only.
- Keep **Slingshot** state-agnostic and keep Unity-facing reset work behind shallow adapters.
- Run Unity compile before tests on implementation slices.
- Prefer EditMode tests for orchestration and adapter contracts; use PlayMode for the same-session Gameplay Scene regression.
- No ADR, package version, changelog, manifest, save format, or ProjectSettings change is expected.
