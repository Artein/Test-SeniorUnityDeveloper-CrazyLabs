# Launch Push Character Presentation Implementation Issues

Parent PRD: [Launch Push Character Presentation PRD](../../prd/prd-launch-push-character-presentation.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
- [ADR-0007: Centralize Unity Input Behind UnityInput](../../adr/adr-0007-centralize-unity-input-behind-unityinput.md)
- [ADR-0008: Use Deterministic Taut Band Shape Solver Instead Of Rope Physics](../../adr/adr-0008-use-deterministic-taut-band-shape-solver-instead-of-rope-physics.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for adding explicit **Pull Anticipation** and **Launch Push** Character presentation around slingshot launch, while keeping slingshot validation, launch physics, band visuals, and passive Character views in their existing ownership lanes.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Expose Validated Active Pull Presentation Facts](01-expose-validated-active-pull-presentation-facts.md) | AFK | None | 1, 6-7, 10-11, 34-41, 59-62, 69-70, 79, 81 |
| 02 | [Publish Slingshot Capture Lifecycle Edges](02-publish-slingshot-capture-lifecycle-edges.md) | AFK | None | 42-47, 69-70, 80 |
| 03 | [Add Slingshot Presentation Context Source](03-add-slingshot-presentation-context-source.md) | AFK | 01, 02 | 8-9, 41-43, 48-52, 63, 78 |
| 04 | [Add Pull And Push Character Mode Classification](04-add-pull-and-push-character-mode-classification.md) | AFK | None | 3-4, 12-22, 54-58, 71-75 |
| 05 | [Carry Slingshot Values Through Character Frames](05-carry-slingshot-values-through-character-frames.md) | AFK | 03, 04 | 23-29, 32, 48-53, 63-68, 76-77, 82 |
| 06 | [Wire Ladybug Animator And GameplayScene Presentation Path](06-wire-ladybug-animator-and-gameplayscene-presentation-path.md) | AFK | 05 | 2, 5, 16-33, 82 |
| 07 | [Run Launch Push Visual QA And Calibration](07-run-launch-push-visual-qa-and-calibration.md) | HITL | 06 | 1-15, 18-28, 83-85 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep **Launch Target** physics authoritative throughout all slices.
- Keep runtime presentation classes generic with `Character` terminology; reserve `Ladybug` for concrete assets and authored Animator wiring.
- Treat slingshot active pull, capture lifecycle, and launch-applied facts as slingshot-owned contracts. Character presentation consumes the read model only.
- Do not add a global event bus, serialized Animator state-name gameplay logic, package changes, save migrations, Addressables changes, or broad physics changes for this feature.
- Prefer EditMode tests for normalizer, notifier, context source, classifier, presenter, and view frame application behavior.
- Use PlayMode tests for scene/container composition and Animator/prefab wiring.
- Run Unity compile before implementation tests on each code slice.
- Slice 07 requires human review because launch-push feel, authored animation transitions, and final tuning cannot be fully accepted through deterministic tests alone.
