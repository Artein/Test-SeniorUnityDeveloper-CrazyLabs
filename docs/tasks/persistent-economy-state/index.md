# Persistent Economy State Implementation Issues

Parent PRD: [Local Persistent Economy State PRD](../../prd/prd-local-persistent-economy-state.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for adding stable currency save identity, a centralized local economy aggregate, versioned JSON persistence under Unity persistent data, mobile-safe save scheduling, durable upgrade purchase commits, run-end-only reward commits, and one integrated gameplay scene smoke path.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Record Persistence Failure Policy And API Contract](01-record-persistence-failure-policy-and-api-contract.md) | AFK | None | US 7, 18-19, 41, 56, 75-76, 81, 104 |
| 02 | [Add Stable Currency Save IDs](02-add-stable-currency-save-ids.md) | AFK | None | US 13, 20-29, 84-85 |
| 03 | [Centralize Economy State In Memory](03-centralize-economy-state-in-memory.md) | AFK | 02 | US 36-47, 77-80, 102-103, 107 |
| 04 | [Persist Schema V1 Local Save](04-persist-schema-v1-local-save.md) | AFK | 02, 03 | US 1-2, 8, 10-12, 45, 48-49, 58, 62-65, 86-87 |
| 05 | [Add Migration And Recovery](05-add-migration-and-recovery.md) | AFK | 04 | US 9, 14-15, 31-32, 50-57, 59-61, 88-93 |
| 06 | [Add Serialized Save Queue And Lifecycle Flush](06-add-serialized-save-queue-and-lifecycle-flush.md) | AFK | 01, 04, 05 | US 16, 18, 69-75, 98, 101 |
| 07 | [Commit Upgrade Purchases Durably](07-commit-upgrade-purchases-durably.md) | AFK | 01, 03, 06 | US 5-7, 17, 30, 40-41, 94-95 |
| 08 | [Commit Run Rewards At Run End](08-commit-run-rewards-at-run-end.md) | AFK | 01, 03, 06 | US 3-4, 33-35, 42-44, 96-97 |
| 09 | [Wire Gameplay Composition And UI States](09-wire-gameplay-composition-and-ui-states.md) | AFK | 01, 04, 06, 07, 08 | US 8, 17, 76, 78-79, 99-101, 107-108 |
| 10 | [Document Release Guardrails](10-document-release-guardrails.md) | AFK | 01, 04-09 | US 11-12, 19, 65-68, 82-83, 104-106 |

## Notes

- All slices are AFK after the approved failure policy: log errors, continue gracefully, avoid rollback machinery for this version, and make UI re-read and display the actual economy state.
- Use `Application.persistentDataPath` through an injected path provider. Do not use PlayerPrefs for economy state.
- Do not add Unity `com.unity.serialization`, UniTask, or a direct Newtonsoft dependency in these slices.
- Keep JSON/file I/O off the gameplay path through immutable DTO snapshots and a serialized save queue. Do not use Unity Jobs or Burst for managed JSON/file work.
- Keep Economy as the central persistence owner. Upgrades and Pickups can depend on Economy, but Economy should not depend on those feature modules.
- Preserve unknown currency and upgrade save IDs unless an explicit future migration tombstones them.
- Run Unity compile before implementation tests in each slice. Prefer EditMode tests for plain C# behavior and PlayMode tests only for scene composition, UI, lifecycle hooks, or Unity engine integration.
- Do not publish these remotely unless explicitly requested.
