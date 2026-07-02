# Run Steering Control Implementation Issues

Parent PRD: [Run Steering Control PRD](../../prd/prd-run-steering-control.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
- [ADR-0007: Centralize Unity Input Behind UnityInput](../../adr/adr-0007-centralize-unity-input-behind-unityinput.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for replacing screen-center Running steering with an invisible floating **Run Steering Control** that uses physical touch distance, generic pointer input, and the existing Launch Target movement path.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Add Run Steering Config And Screen Metrics](01-add-run-steering-config-and-screen-metrics.md) | AFK | None | 19-24, 28-29, 33, 39-42, 50-52 |
| 02 | [Add DPI-Aware Run Steering Gesture](02-add-dpi-aware-run-steering-gesture.md) | AFK | 01 | 4-14, 26-27, 35, 38-46, 51 |
| 03 | [Wire Run Steering Control Into Running](03-wire-run-steering-control-into-running.md) | AFK | 02 | 1-3, 10-16, 30-37, 43-47, 52-53 |
| 04 | [Prove Responsiveness And Movement Integration](04-prove-responsiveness-and-movement-integration.md) | AFK | 03 | 17-18, 21, 23, 48-50 |
| 05 | [Close Run Steering Unity Verification](05-close-run-steering-unity-verification.md) | AFK | 04 | Full acceptance pass, compile, targeted tests, scene smoke |
| 06 | [Run Mobile Feel Review And Tuning](06-run-mobile-feel-review-and-tuning.md) | HITL | 05 | 8, 12-14, 24-25 |

## Notes

- Keep Run Steering Control in Gameplay and keep Pre-Launch Pull behavior unchanged.
- Keep Foundation input generic: touch, editor mouse, and tests should continue to flow through the same pointer abstraction.
- Keep Foundation screen services as raw fact adapters. Gameplay configuration owns fallback DPI and validation policy.
- Do not add visible joystick UI, haptics, audio, tutorial copy, two-axis steering, or a display metrics abstraction in these slices.
- Remove outdated steering config fields intentionally. No serialized migration shim or `[FormerlySerializedAs]` is expected.
- Run Unity compile before implementation tests in each slice. Prefer EditMode tests for mapping and controller behavior; use PlayMode only when Unity lifecycle, scene composition, or runtime smoke coverage requires it.
- Do not publish these remotely unless explicitly requested.
