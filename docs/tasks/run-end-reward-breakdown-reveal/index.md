# Run End Reward Breakdown Reveal Implementation Issues

Parent PRD: [Run End Reward Breakdown Reveal PRD](../../prd/prd-run-end-reward-breakdown-reveal.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for adding a source-agnostic **Run Reward Breakdown**, reward-source rows, distance and air-time contributors, coroutine reveal sequencing, and final feel review for the scene-authored **Run Ended UI**.

## Issues

| ID | Title | Type | Status | Blocked by | User stories covered |
| --- | --- | --- | --- | --- | --- |
| 01 | [Picked-Up Coins Run Reward Source Tracer Bullet](01-picked-up-coins-run-reward-source-tracer-bullet.md) | AFK | Implemented and verified | None | 3, 7, 12, 15-18, 23-26, 29, 35, 38, 40 |
| 02 | [Distance Bonus Run Reward Source](02-distance-bonus-run-reward-source.md) | AFK | Implemented and verified | 01 | 4, 7, 12, 19-21, 24, 27, 35-36, 40 |
| 03 | [Air Time Bonus Run Reward Source](03-air-time-bonus-run-reward-source.md) | AFK | Implemented and verified | 01 | 5, 7, 12, 19-21, 24, 28, 35-36, 40 |
| 04 | [Run Reward Reveal And Fast-Forward Gate](04-run-reward-reveal-and-fast-forward-gate.md) | AFK | Implemented and verified | 01 | 1-2, 7-12, 21-22, 29-34, 37, 39 |
| 05 | [End-To-End Multi-Source RunEnd Reward Flow](05-end-to-end-multi-source-runend-reward-flow.md) | AFK | Implemented and verified | 02, 03, 04 | 6-14, 23-26, 35-40 |
| 06 | [RunEnd Reward Reveal Feel Review](06-runend-reward-reveal-feel-review.md) | HITL | Open - human review required | 04, 05 | 1-14, 21, 39 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep **Run Reward Source Row** source-agnostic; the **Run Ended UI** must not know pickup, distance, air-time, or future mechanic rules.
- Keep **Run Reward Contributor** modules plain C# and testable in EditMode.
- Keep **Run End Flow** as the owner of accepted **Run Result** and **Acknowledge Run Result**.
- Keep the **Run Ended UI** scene-authored; runtime may instantiate generic row views from the serialized row prefab, but must not create the whole panel.
- Keep reward commit based on the accepted result total; the reveal explains rewards and does not grant them.
- No UniTask dependency and no row pooling in this slice set.
- Run Unity compile before implementation tests on code or scene-changing slices.
