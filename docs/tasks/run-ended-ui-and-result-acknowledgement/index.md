# Run Ended UI And Result Acknowledgement Implementation Issues

Parent PRD: [Run Ended UI And Result Acknowledgement PRD](../../prd/prd-run-ended-ui-and-result-acknowledgement.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for turning **Run Ended** into a player-acknowledged result
phase with scene-authored UI, current-run stats, session best-distance feedback, held end pose, and terminal character presentation.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Add Acknowledge Run Result Flow](01-add-acknowledge-run-result-flow.md) | AFK | None | 1, 7, 8, 17, 23, 25 |
| 02 | [Add Run Result Stats And Session Best Distance](02-add-run-result-stats-and-session-best-distance.md) | AFK | None | 2, 3, 4, 14, 15, 18, 19, 20, 21, 26, 27 |
| 03 | [Add Scene-Authored Run Ended UI](03-add-scene-authored-run-ended-ui.md) | AFK | 01, 02 | 1-8, 11-13, 16, 18-21, 24, 28-30 |
| 04 | [Hold Launch Target During Run Ended](04-hold-launch-target-during-run-ended.md) | AFK | 01 | 9, 22 |
| 05 | [Verify Terminal Character Presentation In Run Ended](05-verify-terminal-character-presentation-in-run-ended.md) | AFK | 01 | 10, 16 |
| 06 | [Close End-To-End Run Ended Gameplay Loop](06-close-end-to-end-run-ended-gameplay-loop.md) | AFK | 03, 04, 05 | 1-10, 23, 28-30 |
| 07 | [Review Run Ended UI Copy Timing And Feel](07-review-run-ended-ui-copy-timing-and-feel.md) | HITL | 06 | 5, 6, 11-13, 16, 30 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep **Acknowledge Run Result** separate from **Continue** in **Run Preparation**.
- Keep **Run End Flow** as the owner of the **RunEnded -> RunPreparation** transition.
- Keep **Run Ended UI** scene-authored; runtime should not create the UI hierarchy.
- Keep **Best Run Distance** scoped to the current **Level Session** and out of save data.
- Keep tests focused on technical contracts and behavior, not exact designer-owned layout or final copy.
- Run Unity compile before implementation tests on code or scene-changing slices.
