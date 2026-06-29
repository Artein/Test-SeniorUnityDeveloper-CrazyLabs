## Problem Statement

The game currently has a **Run** loop, **Run Result** data, and authored gameplay state flow, but it does not have a generic way for the player to collect resources during a run. The first use case is coins: a regular small coin and a big coin. The player should collect coins during a **Run**, the run should end, a new run should start, and the player should continue collecting remaining coins and accumulating total resources.

The feature cannot be coin-specific. The same architecture should support later resources or SKUs without introducing a new collection controller for each resource type. Designers need to author collectible pickup variants in Unity with ScriptableObject-backed values, not hardcoded numbers on prefabs. Engineers need deterministic state boundaries so pickups are collected once per **Level Session**, resource totals persist across runs in the current app session, and end-of-run UI can read the amount collected during the just-ended **Run**.

The feature also has important Unity performance and authoring constraints. Pickup collection should be trigger-enter driven, use physics layers for broad-phase filtering, avoid per-frame scene scans, and fail fast on invalid authoring instead of silently skipping bad pickups or granting fallback resources.

## Solution

Implement a generic pickup/resource collection slice.

The solution introduces authored **Resource Definition** assets for resource balance buckets, authored **Pickup Definition** assets for resource grants, and reusable pickup prefab variants for the first two coin types. A neutral pickup base prefab owns the common shell, while regular and big coin variants assign their specific **Pickup Definition**, collider, visual, and optional presentation overrides. Pickup definitions contain one resource reference and a positive base amount. Future modifiers, such as a coin multiplier, apply outside the definition before writing to storage.

Runtime collection is owned by plain C# gameplay services and controllers. A shallow **Pickup** MonoBehaviour forwards `OnTriggerEnter` contacts while enabled and can apply availability through `SetAvailable(bool)` by enabling or disabling the whole pickup scene root GameObject. It does not decide collectability, mutate resources, check gameplay state, or own consumed state.

`PickupCollectionController` subscribes to explicit scene pickup references from `GameplayLifetimeScope`, accepts contacts only while the current **Gameplay State** is `Running`, compares the contacted collider against the configured **Player Tag**, consumes the pickup through **Level Pickup State**, grants resources to `ResourceStorage`, adds the same grant to `RunResourceAccumulator`, disables the pickup root, and publishes a generic **Pickup Collection Event** after the grant is applied.

The first slice separates three lifetimes:

- **Level Pickup State** tracks which explicit scene pickups are available or consumed for the current **Level Session**. It resets only when a **Level Session** starts.
- `ResourceStorage` stores app-session **Resource Balance** totals keyed by **Resource Definition** asset reference. It carries forward across **Runs** during the current app session.
- `RunResourceAccumulator` stores current-run resource deltas. It resets when gameplay enters **Pre-Launch** for the next **Run** and produces a sparse immutable **Run Resource Snapshot** when a **Run** ends.

`RunResult` gains the **Run Resource Snapshot** so end-of-run UI can read the resources collected during the ended **Run** without racing the next run's accumulator reset.

## Unity Surfaces

- Runtime assemblies and asmdefs:
  - Add a pickup/resource runtime assembly named `Game.Gameplay.Pickups`.
  - Place pickup/resource runtime code under `Assets/Game/Gameplay/Pickups`.
  - Update the root `Game.Gameplay` assembly to reference `Game.Gameplay.Pickups` for `GameplayLifetimeScope` composition and `RunResult` integration.
  - Add pickup/resource EditMode and PlayMode test assemblies under the pickup feature.
  - Keep core gameplay rules in plain C# services/controllers, consistent with ADR-0002.
  - Use direct C# events for local pickup notification, consistent with ADR-0003.
  - Compose runtime services through VContainer, consistent with ADR-0005.
- Editor assemblies, windows, inspectors, importers, or menu items:
  - Add an editor-only tag selector attribute/property drawer for serialized tag-name fields.
  - Add a `GameplayLifetimeScope` custom inspector button named `Refresh Pickup References From Scene`.
  - The refresh helper finds same-scene **Pickup** objects, includes inactive pickups, orders references deterministically, records Undo, and marks the scope dirty.
  - Do not use `OnValidate` to auto-populate scene pickup references.
  - Existing validation-warning patterns may remain in `OnValidate`, but runtime setup validation must fail hard during composition.
- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:
  - Add **Resource Definition** and **Pickup Definition** assets under `Assets/Game/Gameplay/Pickups/Definitions`.
  - Add reusable pickup prefabs under `Assets/Game/Gameplay/Pickups/Prefabs`.
  - Add a neutral `PickupBase.prefab`.
  - Add regular small coin and big coin pickup prefab variants.
  - Legacy meshes, materials, and visual dependencies stay under `Assets/Ladybug/Content/...` and may be referenced by pickup prefab variants.
  - Add Unity tag `Player` or reuse an equivalent existing player tag if one appears before implementation.
  - Add Unity physics layers named `Player` and `Pickup`.
  - Pickup trigger colliders use **Pickup Layer**.
  - Player pickup-contact colliders use **Player Layer** and the configured **Player Tag** on their own GameObjects.
  - Configure the 3D Layer Collision Matrix so **Pickup Layer** overlaps **Player Layer** for collection while avoiding unnecessary non-player overlaps.
  - Serialize all level pickups into `GameplayLifetimeScope`.
  - Serialize explicit player pickup-contact collider references into `GameplayLifetimeScope` for setup validation only.
  - Manually assign player pickup-contact colliders in the first slice; do not add a second refresh helper for them.
- RPC/helper commands, hooks, or shell wrappers:
  - Use Unity AI Agent Connector compile and targeted test commands for implementation verification.
  - No new RPC helper command or shell wrapper is required for this feature.
- Package versioning, changelog, and installation/sync behavior:
  - This is project gameplay work, not a distributable package release.
  - No package dependency change is expected.
  - No save-data migration is expected in the first slice.

## User Stories

1. As a player, I want to collect regular coins during a **Run**, so that the run contains collectible rewards.
2. As a player, I want to collect big coins during a **Run**, so that higher-value pickups feel distinct.
3. As a player, I want collected coins to add to my total resource amount, so that collecting has persistent value during the app session.
4. As a player, I want collected coins to remain collected after the run ends, so that I cannot farm the same pickup by restarting the run.
5. As a player, I want a new run to start with already-consumed pickups still gone, so that the level has a known finite pickup set.
6. As a player, I want to continue collecting remaining pickups in later runs of the same level, so that progress through the level session continues.
7. As a player, I want the end-of-run UI to know how many coins I collected in the just-ended run, so that the result screen can show current-run rewards.
8. As a player, I want current-run collected coins to be separate from total coins, so that the result screen is not confused by previous runs.
9. As a designer, I want a regular small coin prefab variant, so that I can place normal rewards in levels.
10. As a designer, I want a big coin prefab variant, so that I can place higher-value rewards in levels.
11. As a designer, I want a neutral pickup base prefab, so that shared pickup setup is not duplicated across coin types.
12. As a designer, I want coin amounts to come from **Pickup Definition** assets, so that prefab visuals do not hardcode reward values.
13. As a designer, I want **Pickup Definition** assets to reference **Resource Definition** assets, so that the same pickup system can grant future resources.
14. As a designer, I want **Pickup Definition** amount to be a positive integer, so that invalid zero or negative rewards are caught.
15. As a designer, I want missing **Resource Definition** references to fail validation, so that broken pickup definitions are not silently accepted.
16. As a designer, I want missing **Pickup Definition** references on pickup variants to fail validation, so that placed pickups always have explicit reward data.
17. As a designer, I want pickup definition assets grouped under the pickup feature's Definitions folder, so that reward config is easy to find.
18. As a designer, I want reusable pickup prefabs grouped under the pickup feature's Prefabs folder, so that gameplay prefab variants are easy to find.
19. As a designer, I want legacy visuals and materials to stay in the existing content tree, so that art assets do not move unnecessarily.
20. As a designer, I want all level pickups serialized into `GameplayLifetimeScope`, so that runtime composition uses explicit references.
21. As a designer, I want a `Refresh Pickup References From Scene` button, so that I can populate level pickup references from the current scene.
22. As a designer, I want the refresh helper to include inactive pickups, so that disabled or staged pickups are not missed.
23. As a designer, I want the refresh helper to order pickups deterministically, so that serialized diffs stay stable.
24. As a designer, I want the refresh helper to record Undo and mark the scope dirty, so that editor workflow follows Unity expectations.
25. As a designer, I want duplicate pickup references to fail validation, so that one pickup cannot produce confusing state behavior.
26. As a designer, I want consumed pickups to disable their whole scene root GameObject, so that visuals and trigger participation stop together.
27. As a designer, I want pickup trigger colliders on **Pickup Layer**, so that collision filtering is visible in authoring.
28. As a designer, I want player pickup-contact colliders on **Player Layer**, so that collection filtering is explicit.
29. As a designer, I want every collecting collider GameObject to carry the configured **Player Tag**, so that child colliders collect correctly.
30. As a designer, I want root-only player tagging to be rejected for pickup collection, so that pickup behavior is not dependent on hidden parent lookup.
31. As a developer, I want generic **Resource Definition** identities, so that future resource types do not require new collection code.
32. As a developer, I want `ResourceStorage` to own app-session **Resource Balance** totals, so that resource totals have one service boundary.
33. As a developer, I want `ResourceStorage` keyed by **Resource Definition** asset reference in the first slice, so that persistence concerns do not add premature IDs.
34. As a developer, I want `RunResourceAccumulator` to own current-run resource deltas, so that end-of-run data is not read from app-session totals.
35. As a developer, I want `RunResourceAccumulator` to reset on **Pre-Launch** for the next **Run**, so that every run starts with empty current-run deltas.
36. As a developer, I want `RunResourceAccumulator` to produce an immutable **Run Resource Snapshot**, so that UI can read ended-run data safely.
37. As a developer, I want **Run Resource Snapshot** to be sparse, so that missing resources naturally mean zero collected for that run.
38. As a developer, I want **Run Resource Snapshot** inside **Run Result**, so that run-end data has one immutable payload.
39. As a developer, I want `LevelPickupState` to own available/consumed pickup state, so that state is not hidden in scene GameObject active flags.
40. As a developer, I want `LevelPickupState` to reset only at **Level Session** start, so that pickups are one-shot across runs in the same level.
41. As a developer, I want the **Pickup** MonoBehaviour to be a shallow adapter, so that gameplay rules stay testable outside Unity callbacks.
42. As a developer, I want **Pickup** to publish trigger-enter contacts only, so that collection is event-driven and not a per-frame scan.
43. As a developer, I want **Pickup** contact events to include the raw contacted `Collider`, so that the controller can compare the configured tag.
44. As a developer, I want no first-slice `PickupContact` struct, so that the event payload stays minimal until more data is needed.
45. As a developer, I want `PickupCollectionController` to own contact subscriptions, so that subscription lifecycle is centralized.
46. As a developer, I want `PickupCollectionController` to ignore contacts outside `Running`, so that pre-launch and run-ended overlaps cannot grant rewards.
47. As a developer, I want `PickupCollectionController` to use `CompareTag` with the configured **Player Tag**, so that player identity is explicit and fast.
48. As a developer, I want physics layers to filter pickup/player overlap before tag identity checks, so that pickup physics cost stays bounded.
49. As a developer, I want no `PickupCollector` component in the first slice, so that collection eligibility stays simple and does not require hierarchy lookup.
50. As a developer, I want no parent lookup from contacted collider to player root, so that every collecting collider must be authored correctly.
51. As a developer, I want the accepted pickup transaction to consume pickup state before mutating resource totals, so that duplicate contacts cannot double grant.
52. As a developer, I want accepted collection to grant both `ResourceStorage` and `RunResourceAccumulator`, so that totals and current-run deltas stay in sync.
53. As a developer, I want accepted collection to disable the pickup root after the grant, so that the scene reflects consumed state immediately.
54. As a developer, I want a generic **Pickup Collection Event** after grant application, so that future feedback can observe accepted pickups.
55. As a developer, I want the **Pickup Collection Event** payload to include resource, amount, and world position, so that future VFX/UI do not read live state.
56. As a developer, I want direct C# events instead of an event bus, so that local pickup coordination follows current ADR guidance.
57. As a developer, I want runtime setup validation during composition, so that invalid pickup authoring fails before the first run.
58. As a developer, I want validation to include pickup definitions, resource definitions, positive amounts, layers, tags, trigger setup, serialized pickup refs, and player pickup-contact collider refs, so that setup failures are actionable.
59. As a developer, I want `GameplayLifetimeScope` to pass the configured **Player Tag** into `PickupCollectionController`, so that the tag string is not hardcoded.
60. As a developer, I want a tag selector drawer for serialized tag names, so that authoring avoids typo-prone free text.
61. As a developer, I want no maximum collectible amount calculator in the first slice, so that future multipliers do not force premature level-total APIs.
62. As a developer, I want future reward multipliers outside **Pickup Definition**, so that definitions remain base authored grants.
63. As a developer, I want no app-restart persistence in the first slice, so that save identifiers can be designed when persistence is actually needed.
64. As a tester, I want EditMode tests for `ResourceStorage`, so that resource totals are added and queried by **Resource Definition**.
65. As a tester, I want EditMode tests for `RunResourceAccumulator`, so that current-run deltas reset, accumulate, and snapshot correctly.
66. As a tester, I want EditMode tests for sparse **Run Resource Snapshot** behavior, so that missing resources read as zero.
67. As a tester, I want EditMode tests for `LevelPickupState`, so that one-shot consumption and duplicate-reference validation are deterministic.
68. As a tester, I want EditMode tests for `PickupCollectionController`, so that transaction order and state gating are covered without real physics where possible.
69. As a tester, I want PlayMode tests for trigger-enter pickup collection, so that Unity physics integration works with layers, tags, and colliders.
70. As a tester, I want PlayMode tests for consumed pickup root deactivation, so that visuals and colliders stop together.
71. As a tester, I want composition tests for `GameplayLifetimeScope`, so that invalid serialized pickup/player setup fails loudly.
72. As a maintainer, I want the feature isolated in a pickup/resource asmdef, so that gameplay responsibilities stay modular.
73. As a maintainer, I want root gameplay to reference the pickup feature only where composition and run-result integration need it, so that dependency direction stays clear.
74. As a maintainer, I want generated PRD and future tasks to preserve glossary vocabulary, so that future implementation agents do not reintroduce coin-specific names.

## Implementation Decisions

- Use generic pickup/resource architecture, not coin-specific collection architecture.
- Use **Resource Definition** for the authored identity of a resource bucket.
- Use **Pickup Definition** for the authored pickup grant.
- First-slice **Pickup Definition** data contains exactly one **Resource Definition** reference and one positive integer amount.
- Do not add a type enum, string ID, stable save ID, prefab reference, icon, display name, VFX, SFX, or feedback settings to **Pickup Definition** in the first slice.
- `PickupDefinition.Amount` is the base authored grant amount. Future modifiers such as coin multipliers apply outside the definition before writing to storage.
- Missing **Resource Definition**, missing **Pickup Definition**, and non-positive amount are invalid authoring.
- Keep authored resource identity asset-backed by reference in the first slice, matching the project's existing asset-backed gameplay-state identity style.
- `ResourceStorage` is the service/class name for app-session totals. **Resource Balance** remains the domain term for the held amount.
- `ResourceStorage` keys balances by **Resource Definition** asset reference until app-restart persistence is in scope.
- `RunResourceAccumulator` is the run-scoped holder for current-run resource deltas.
- `RunResourceAccumulator` resets when gameplay enters **Pre-Launch** for the next **Run**, not on launch.
- `RunResourceAccumulator` produces **Run Resource Snapshot** when a **Run** ends.
- **Run Resource Snapshot** is immutable and sparse. Missing **Resource Definition** means zero collected for that **Run**.
- **Run Resource Snapshot** belongs inside **Run Result** because **Run Result** is the immutable summary captured when a **Run** ends.
- `LevelPickupState` owns available/consumed pickup state for the current **Level Session**.
- `LevelPickupState` resets only at **Level Session** start.
- Pickups are one-shot per **Level Session**, not one-shot per **Run**.
- Do not calculate maximum collectible coins/resources for the level in the first slice.
- Use a neutral pickup base prefab and separate regular small coin and big coin variants.
- Each pickup prefab variant directly references its **Pickup Definition**. Do not add a central pickup catalog in the first slice.
- Put definition assets under the pickup feature's Definitions folder.
- Put reusable pickup prefabs under the pickup feature's Prefabs folder.
- Keep legacy art, meshes, and materials in the existing content tree; pickup variants may reference them.
- **Pickup** MonoBehaviour is a shallow Unity adapter only.
- **Pickup** forwards trigger-enter contacts while enabled.
- **Pickup** exposes a contact event/callback shaped around the pickup and the raw contacted `Collider`.
- **Pickup** may expose its assigned **Pickup Definition**.
- **Pickup** applies availability by enabling/disabling the whole pickup scene root GameObject through `SetAvailable(bool)`.
- **Pickup** does not own collection rules, resource grants, gameplay-state gating, consumed state, or score/UI updates.
- `PickupCollectionController` owns pickup contact subscriptions and unsubscriptions.
- `GameplayLifetimeScope` passes the explicit serialized pickup list into `PickupCollectionController`.
- Accepted pickup collection order is: trigger contact, `Running` gate, `CompareTag` against the configured **Player Tag**, `LevelPickupState.TryConsume`, `ResourceStorage` grant, `RunResourceAccumulator` grant, pickup root deactivation, then **Pickup Collection Event** publication.
- Consuming pickup state before mutating resource totals prevents duplicate trigger contacts from double-granting.
- Publish a generic **Pickup Collection Event** after the resource grant is applied.
- **Pickup Collection Event** payload includes **Resource Definition**, grant amount, and collection world position.
- Do not implement VFX, SFX, HUD animation, floating counters, or end-screen presentation in this first slice unless the implementation task explicitly expands scope.
- Use configured **Player Tag** for collection identity, not `LaunchTarget`.
- Serialize the **Player Tag** name on `GameplayLifetimeScope` and pass it into `PickupCollectionController`.
- Add an editor-only tag selector attribute/property drawer for serialized tag fields.
- Every player collider GameObject that can enter pickup triggers must carry the configured **Player Tag**.
- Root-only player tagging is not sufficient without parent lookup.
- Do not add `PickupCollector` in the first slice.
- Do not resolve contacted colliders by searching parent hierarchy for a collector component in the first slice.
- Use **Player Layer** and **Pickup Layer** for broad-phase physics filtering.
- Use trigger colliders and `OnTriggerEnter` only for first-slice pickup collection.
- Do not use `OnTriggerStay` polling or per-frame pickup scans.
- Configure the Layer Collision Matrix so **Pickup Layer** overlaps **Player Layer** and avoids unnecessary non-player pickup overlaps.
- Layers are filtering only. **Player Tag** remains the gameplay identity check.
- Runtime setup validation happens during `GameplayLifetimeScope` composition or controller construction before the first **Run**.
- Validation covers assigned and valid **Pickup Definition**, positive amount, required **Resource Definition**, pickup trigger collider setup, pickup trigger on **Pickup Layer**, collecting collider on **Player Layer**, collecting collider with configured **Player Tag**, non-empty configured tag, serialized pickup references, duplicate pickup references, and explicit player pickup-contact collider references.
- `GameplayLifetimeScope` serializes explicit player pickup-contact collider references for setup validation only.
- Player pickup-contact collider references stay manually assigned in the first slice.
- Add one `GameplayLifetimeScope` custom inspector button named `Refresh Pickup References From Scene`.
- The refresh helper finds same-scene **Pickup** objects, includes inactive pickups, orders deterministically, records Undo, and marks the scope dirty.
- Do not auto-populate pickup references from `OnValidate`.
- Use VContainer composition for services and controllers.
- Register scene adapters as narrow interfaces where useful. Do not inject services into pickup MonoBehaviours.
- Use direct C# events for local pickup notifications before introducing any bus.
- Add or extend a run-result notification path only as needed for consumers to observe **Run Result**; direct C# notification is the expected first choice if no existing result delivery surface exists.

## Testing Decisions

- Good tests should assert external behavior at module boundaries: stored balances, run deltas, immutable snapshots, one-shot consumption, accepted collection transaction results, validation failures, event publication, and Unity trigger integration. Tests should not assert private fields or rely on reflection.
- Prefer EditMode tests for deep plain C# modules:
  - `ResourceStorage` adds and queries amounts by **Resource Definition**.
  - `ResourceStorage` treats missing balances as zero.
  - `RunResourceAccumulator` accumulates grants by resource.
  - `RunResourceAccumulator` resets for the next **Run**.
  - `RunResourceAccumulator` creates immutable sparse snapshots.
  - **Run Resource Snapshot** returns zero for missing resources.
  - `LevelPickupState` marks pickups consumed once.
  - `LevelPickupState` rejects duplicate explicit pickup references.
  - `LevelPickupState` reset restores pickup availability for a new **Level Session**.
  - `PickupCollectionController` ignores contacts outside `Running`.
  - `PickupCollectionController` ignores colliders without the configured **Player Tag**.
  - `PickupCollectionController` consumes state before resource grants.
  - `PickupCollectionController` grants both app-session storage and run accumulator.
  - `PickupCollectionController` publishes the **Pickup Collection Event** after the grant.
  - `PickupCollectionController` unsubscribes from pickup contact events when disposed.
- Use PlayMode tests where Unity engine behavior is part of the contract:
  - `OnTriggerEnter` on **Pickup** forwards one trigger-enter contact.
  - Trigger setup with **Pickup Layer** and **Player Layer** collects as expected.
  - Contacting collider GameObject must have the configured **Player Tag**.
  - Consumed pickup disables the whole pickup scene root GameObject.
  - Scene composition fails loudly when required pickup/player references are invalid.
- Editor tests or manual editor checks should cover the custom inspector refresh helper:
  - same-scene pickups are found;
  - inactive pickups are included;
  - ordering is deterministic;
  - Undo is recorded;
  - the target scope is marked dirty.
- Existing prior art:
  - Gameplay config and controller behavior is covered by EditMode tests.
  - Gameplay scene composition and Unity physics callback behavior is covered by PlayMode tests.
  - Existing `GameplayLifetimeScope` tests cover serialized reference validation patterns.
- Verification sequence for implementation:
  - Reformat changed files through Rider MCP where available.
  - Check file problems through Rider MCP where available.
  - Run Unity compile through Unity AI Agent Connector.
  - Run targeted EditMode tests for new pickup/resource modules after compile is clean.
  - Run targeted PlayMode tests only for Unity trigger/scene/composition behavior after compile is clean.
  - Run broader changed tests if the integration touches shared gameplay state or run-end result behavior.

## Release and Compatibility

- Unity version assumption: the project targets Unity 6000.3.x based on local docs and project instructions.
- Existing third-party dependency assumption: VContainer remains the DI framework.
- No new package dependency is expected.
- No package version or changelog update is expected unless the project later treats gameplay docs as release artifacts.
- No app-restart persistence or save-data migration is included in the first slice.
- **Resource Definition** asset-reference keying is intentionally first-slice only; stable save identifiers should be designed before persistence ships.
- ProjectSettings compatibility risk: adding tags, layers, and Layer Collision Matrix changes can affect scene physics if misconfigured. Implementation must verify current collision behavior for non-pickup gameplay still works.
- Scene compatibility risk: player pickup-contact colliders must be tagged/layered on their own GameObjects. Existing root-only tags will not be enough.
- Run-result compatibility risk: adding **Run Resource Snapshot** to **Run Result** changes constructor/call-site shape and requires targeted tests for existing run-end behavior.

## Out of Scope

- App-restart persistence for **Resource Balance** totals.
- Save IDs, GUID-backed resource IDs, or externally addressable resource identifiers.
- Central pickup catalog.
- Maximum collectible amount calculation for a level.
- Coin multiplier or any other reward multiplier implementation.
- Economy, shop, SKU purchase, inventory, analytics, or backend integration.
- Addressables schema changes.
- VFX, SFX, floating pickup counters, HUD animation, or pickup presentation feedback implementation.
- Final end-of-run UI design and layout. The data contract for end-of-run UI is in scope through **Run Result** and **Run Resource Snapshot**.
- Pickup pooling, magnet collection, proximity attraction, or collection radius tuning beyond trigger collider authoring.
- Runtime scene discovery of pickups.
- Automatic player pickup-contact collider discovery or refresh helper.
- Parent hierarchy lookup for pickup collection eligibility.
- `OnTriggerStay` collection.
- Per-frame pickup scanning.
- Runtime fallback grants for invalid pickup authoring.

## Further Notes

- Deep modules for implementation should be `ResourceStorage`, `RunResourceAccumulator`, **Run Resource Snapshot**, `LevelPickupState`, and `PickupCollectionController`. These carry most behavior behind small testable interfaces.
- Shallow Unity surfaces should be **Pickup**, ScriptableObject definitions, `GameplayLifetimeScope` serialized references, the custom inspector refresh helper, and the tag selector drawer.
- The current codebase logs **Run Result** inside **Run End Flow** and does not yet have a run-end UI subscription surface. The PRD assumes direct C# result notification can be added when a consumer needs it, consistent with ADR-0003.
- The current codebase does not yet formalize a level-loading lifecycle. Until a dedicated level-session owner exists, the practical first-slice assumption is that the current `GameplayLifetimeScope` composition represents **Level Session** start.
- The first implementation should keep coin-specific naming to asset names and prefab variant names only. Runtime modules should stay generic.
