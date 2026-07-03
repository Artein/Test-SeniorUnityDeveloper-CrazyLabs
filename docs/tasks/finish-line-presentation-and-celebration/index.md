# Finish Line Presentation And Celebration Implementation Issues

Parent PRD: [Finish Line Presentation And Celebration PRD](../../prd/prd-finish-line-presentation-and-celebration.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
- [ADR-0009: Use Cinemachine For Run Camera](../../adr/adr-0009-use-cinemachine-for-run-camera.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for turning the finish approach into a readable **Finish Presentation** with one authoritative **Run Finish**, a visual-only **Finish Threshold Visual**, accepted-success **Finish Celebration**, and **Victory Facing** during successful **Victory**.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Separate Finish Presentation From Run Finish Contacts](01-separate-finish-presentation-from-run-finish-contacts.md) | AFK | None | 1, 4, 15-18, 24, 28, 33-34, 44-45, 54-55 |
| 02 | [Add Finish Threshold Visual](02-add-finish-threshold-visual.md) | AFK | 01 | 2-7, 19-20, 25-27, 46, 53-55 |
| 03 | [Play Finish Celebration From Accepted Success](03-play-finish-celebration-from-accepted-success.md) | AFK | 02 | 8-11, 21-23, 29-32, 38-41, 47-48, 55 |
| 04 | [Blend Victory Facing Toward Run Camera](04-blend-victory-facing-toward-run-camera.md) | AFK | None | 12-14, 35-37, 39, 42-43, 55 |
| 05 | [Close End-To-End Finish Presentation Flow](05-close-end-to-end-finish-presentation-flow.md) | AFK | 03, 04 | 1-14, 24, 38, 44-48, 51-52, 55 |
| 06 | [Review Finish Camera Framing And VFX Feel](06-review-finish-camera-framing-and-vfx-feel.md) | HITL | 05 | 5-13, 21-23, 49-53 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep **Finish Presentation** visual-only and separate from **Run Finish** contact behavior.
- Keep **Run End Flow** as the only authority that accepts **Run Result**.
- Drive **Finish Celebration** and **Victory Facing** from accepted successful **Run Result**, not raw trigger entry.
- Keep scene-authored finish views registered through VContainer without making MonoBehaviours own service lifetimes.
- Treat `FinishThresholdCheckered.png` as first-pass replaceable art.
- Run Unity compile before implementation tests on code or scene-changing slices.
