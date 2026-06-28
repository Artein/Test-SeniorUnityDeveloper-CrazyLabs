# Pickup Resource Collection Implementation Issues

Parent PRD: [Pickup Resource Collection PRD](../../prd/prd-pickup-resource-collection.md)

Related ADRs:

- [ADR-0001: Use Asset-Backed Gameplay State IDs](../../adr/adr-0001-use-asset-backed-gameplay-state-ids.md)
- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for generic **Pickup** resource collection, first with
regular and big coin content, without adding persistence, economy, VFX/SFX, end-screen presentation, runtime discovery, parent lookup, or per-frame
pickup scans.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Add Generic Resource Grant Data Path](01-add-generic-resource-grant-data-path.md) | AFK | None | 12-18, 31-37, 61-66, 72, 74 |
| 02 | [Attach Run Resource Snapshot To RunResult](02-attach-run-resource-snapshot-to-runresult.md) | AFK | 01 | 7-8, 34-38, 73 |
| 03 | [Add Pickup Adapter And Level Pickup State](03-add-pickup-adapter-and-level-pickup-state.md) | AFK | 01 | 25-26, 39-44, 67, 70 |
| 04 | [Add PickupCollectionController Transaction Flow](04-add-pickupcollectioncontroller-transaction-flow.md) | AFK | 01, 03 | 3-6, 45-56, 68 |
| 05 | [Add Pickup Physics Setup And Trigger Integration](05-add-pickup-physics-setup-and-trigger-integration.md) | AFK | 03, 04 | 27-30, 42, 48, 57-58, 69-70 |
| 06 | [Wire GameplayLifetimeScope Composition And Validation](06-wire-gameplaylifetimescope-composition-and-validation.md) | AFK | 02, 04, 05 | 20, 25, 57-59, 71, 73 |
| 07 | [Add Editor Authoring Helpers For Pickups And Tags](07-add-editor-authoring-helpers-for-pickups-and-tags.md) | AFK | 06 | 21-24, 60, 74 |
| 08 | [Author Regular And Big Coin Pickup Prefabs](08-author-regular-and-big-coin-pickup-prefabs.md) | HITL | 01, 03, 05 | 1-2, 9-19, 26-28 |
| 09 | [Wire Gameplay Scene Pickup Smoke Path](09-wire-gameplay-scene-pickup-smoke-path.md) | HITL | 06, 08 | 1-8, 20, 27-30, 69-71 |

## Notes

- Keep runtime code generic. Coin-specific names belong only to authored asset and prefab content.
- Keep **Pickup Definition** values as base authored grants. Multipliers remain outside the first slice.
- Keep **ResourceStorage** app-session scoped and keyed by **Resource Definition** asset reference.
- Keep **RunResourceAccumulator** current-run scoped and reset it on **Pre-Launch** for the next **Run**.
- Keep **LevelPickupState** scoped to the current **Level Session**; pickups are one-shot across runs in that level session.
- Use trigger-enter collection only, with **Player Layer** / **Pickup Layer** broad-phase filtering and configured **Player Tag** identity checks.
- Do not add `PickupCollector`, parent hierarchy lookup, `OnTriggerStay`, runtime scene discovery, or a central pickup catalog.
- Run Unity compile before implementation tests. Prefer EditMode tests for plain C# behavior and PlayMode tests only for physics, scene, and Unity
  composition behavior.
