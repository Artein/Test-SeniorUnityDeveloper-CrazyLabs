# Ladybug Character Presentation Implementation Issues

Parent PRD: [Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
- [ADR-0009: Use Cinemachine For Run Camera](../../adr/adr-0009-use-cinemachine-for-run-camera.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for replacing the prototype player cylinder presentation with a generic **Character** presentation layer, using **Ladybug** as the first concrete character while preserving **Launch Target** physics authority.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Move And Catalog Ladybug Source Assets](01-move-and-catalog-ladybug-source-assets.md) | AFK | None | 23, 26-27, 35-37, 76, 78-80 |
| 02 | [Add Launch Target Presentation Anchors](02-add-launch-target-presentation-anchors.md) | AFK | None | 10-14, 28-31, 70-72 |
| 03 | [Add Surface And Course Speed Facts](03-add-surface-and-course-speed-facts.md) | AFK | None | 3, 15-16, 44-48, 63-67 |
| 04 | [Add Character Presentation Mode Classifier](04-add-character-presentation-mode-classifier.md) | AFK | 03 | 2-6, 15-22, 38-39, 42-50, 54-55, 58, 63-67 |
| 05 | [Expose Accepted Run Result Notification](05-expose-accepted-run-result-notification.md) | AFK | None | 7-8, 49, 52-53, 68-69 |
| 06 | [Add Passive Character Presentation View](06-add-passive-character-presentation-view.md) | AFK | None | 9, 25, 32-34, 41-43, 56, 73 |
| 07 | [Add Character Presentation Presenter](07-add-character-presentation-presenter.md) | AFK | 03, 04, 05, 06 | 4-9, 38, 40-43, 49-62, 67-69 |
| 08 | [Compose Ladybug Character Prefab And Controller](08-compose-ladybug-character-prefab-and-controller.md) | AFK | 01, 06 | 1-3, 5, 7-10, 25-37, 56-57, 73-75, 78 |
| 09 | [Wire Character Presentation Into GameplayScene](09-wire-character-presentation-into-gameplayscene.md) | AFK | 02, 07, 08 | 1-11, 14, 38-43, 46-62, 70-73, 75, 79-80 |
| 10 | [Fit Collider And Prove Band Contact Compatibility](10-fit-collider-and-prove-band-contact-compatibility.md) | HITL | 02, 08, 09 | 10-14, 28-31, 70-72, 74 |
| 11 | [Run Ladybug Visual QA And Close Calibration](11-run-ladybug-visual-qa-and-close-calibration.md) | HITL | 09, 10 | 23-24, 36-37, 74-76, 79-80 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep **Launch Target** physics authoritative throughout all slices.
- Keep reusable runtime classes generic with `Character` terminology; reserve `Ladybug` for concrete assets and imported content.
- Preserve `.meta` GUIDs when moving third-party Ladybug assets.
- Prefer EditMode tests for classifier, slope, speed, playback, presenter, and result-notifier behavior.
- Use PlayMode tests for prefab/scene composition, Animator parameter application, Rigidbody/collider authority, and band/contact compatibility.
- Run Unity compile before tests on implementation slices.
- Slices 10 and 11 require human review because scale, collider fit, camera framing, material look, and feel thresholds depend on full Ladybug assets and in-editor visual judgment.
