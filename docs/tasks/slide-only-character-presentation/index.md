# Slide-Only Character Presentation Implementation Issues

Parent PRD: [Slide-Only Character Presentation PRD](../../prd/prd-slide-only-character-presentation.md)

Related PRDs:

- [Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md) - superseded only where it treats flat forward movement as normal Run presentation.

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for making **Slide** the only normal grounded locomotion **Character Presentation Mode**, while keeping **Run** as a **Reserved Presentation Mode** and preserving the Rigidbody-backed **Launch Target** as gameplay truth.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Replace Grounded Mode Selection With Meaningful Slide](01-replace-grounded-mode-selection-with-meaningful-slide.md) | AFK | None | 1-16, 21-28, 39-47, 53-63, 66-70 |
| 02 | [Normalize Reserved Run And Slide Playback](02-normalize-reserved-run-and-slide-playback.md) | AFK | 01 | 17-18, 29-31, 34-37, 44, 46-47, 59, 64, 68 |
| 03 | [Update Ladybug Authoring And Scene Composition](03-update-ladybug-authoring-and-scene-composition.md) | AFK | 01, 02 | 19-20, 24-38, 48-52, 65, 67-69 |
| 04 | [Clean Legacy Run Presentation Documentation](04-clean-legacy-run-presentation-documentation.md) | AFK | 01, 02, 03 | 21, 25-30, 39, 48, 66-70 |
| 05 | [Run Slide-Only Visual QA And Calibration](05-run-slide-only-visual-qa-and-calibration.md) | HITL | 03, 04 | 1-20, 32-33, 37-38, 67-70 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep **Launch Target** physics authoritative throughout all slices.
- Keep **Character** runtime classes generic. Reserve **Ladybug** for concrete prefab, Animator, material, and imported asset surfaces.
- Keep **Run** in the **Character Presentation Mode** enum as a **Reserved Presentation Mode**. Do not reorder enum values.
- Do not add a **Coast** mode, new animation dependency, package change, Unity version change, save migration, Addressables change, or broad physics change for this feature.
- Prefer EditMode tests for classifier, presenter, playback, and compatibility behavior.
- Use PlayMode tests for scene, prefab, Animator, and serialized tuning composition.
- Run Unity compile before implementation tests on each code or asset-changing slice.
- Slice 05 requires human review because the final answer is visual feel: flat coasting, downhill sliding, true stalls, launch handoff, and absence of visible running.
