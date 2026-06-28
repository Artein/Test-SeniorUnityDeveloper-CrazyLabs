# Architecture Decision Records

ADR Markdown files in this directory are canonical. This index is for quick discovery by humans and agents.

## Counts

- approved: 7
- proposed: 1
- superseded: 0

## Index

| ID | Status | Title | Date | Components | Tags | Supersedes | Superseded by | File | Summary |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ADR-0001 | approved | Use Asset-Backed Gameplay State Ids | 2026-06-25 | Gameplay | gameplay-state, scriptableobject, flow |  |  | [adr-0001-use-asset-backed-gameplay-state-ids.md](adr-0001-use-asset-backed-gameplay-state-ids.md) | Use ScriptableObject assets as gameplay state and transition identities so authored flow stays reference-based and decoupled from hardcoded identifiers. |
| ADR-0002 | approved | Keep Gameplay Logic In Plain C# Controllers | 2026-06-25 | Gameplay | gameplay, testability, monobehaviour |  |  | [adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md](adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md) | Keep gameplay rules in plain C# controllers and services, with MonoBehaviours limited to shallow Unity adapter responsibilities. |
| ADR-0003 | approved | Use Direct C# Events Before Event Bus | 2026-06-25 | Gameplay | events, coordination, gameplay |  |  | [adr-0003-use-direct-csharp-events-before-event-bus.md](adr-0003-use-direct-csharp-events-before-event-bus.md) | Use direct C# events for local gameplay coordination before introducing a global event bus. |
| ADR-0004 | proposed | Defer Structured Logging Backend | 2026-06-25 | Gameplay, Tooling | logging, observability, unity |  |  | [adr-0004-💡-defer-structured-logging-backend.md](adr-0004-💡-defer-structured-logging-backend.md) | Use the existing Unity logging path for warnings for now and defer project-owned structured logging to a separate architecture change. |
| ADR-0005 | approved | Use VContainer For Dependency Injection | 2026-06-25 | Gameplay, Composition | dependency-injection, vcontainer, composition |  |  | [adr-0005-use-vcontainer-for-dependency-injection.md](adr-0005-use-vcontainer-for-dependency-injection.md) | Use VContainer for dependency injection so gameplay services, controllers, and Unity adapters are composed through explicit LifetimeScopes. |
| ADR-0006 | approved | Register Views Without Injecting MonoBehaviours | 2026-06-25 | Gameplay, Composition | vcontainer, monobehaviour, views, composition |  |  | [adr-0006-register-views-without-injecting-monobehaviours.md](adr-0006-register-views-without-injecting-monobehaviours.md) | Register existing scene views as interface instances so controllers can use them without injecting dependencies into MonoBehaviours. |
| ADR-0007 | approved | Centralize Unity Input Behind UnityInput | 2026-06-25 | Input, Gameplay | input, unity-input-system, enhanced-touch |  |  | [adr-0007-centralize-unity-input-behind-unityinput.md](adr-0007-centralize-unity-input-behind-unityinput.md) | Centralize Unity Enhanced Touch enablement and pointer event translation behind a root-scoped UnityInput service. |
| ADR-0008 | approved | Use Deterministic Taut Band Shape Solver Instead Of Rope Physics | 2026-06-26 | Gameplay, Slingshot | slingshot, band-shape, physics, testability |  |  | [adr-0008-use-deterministic-taut-band-shape-solver-instead-of-rope-physics.md](adr-0008-use-deterministic-taut-band-shape-solver-instead-of-rope-physics.md) | Represent natural slingshot Band Shape as a deterministic taut visual path around the Launch Target Silhouette instead of runtime rope physics. |
