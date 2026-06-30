# PRD: Local Persistent Economy State

## Problem Statement

The game currently has an in-session economy and upgrade loop, but currency balances and upgrade progress do not survive an app restart. Coins are held by an in-memory currency storage keyed by `CurrencyDefinition` asset references. Upgrade levels are held by an in-memory upgrade progress storage keyed by `UpgradeDefinition.StableId`. This mismatch means upgrades already have a persistence identity, while currencies do not.

The player-facing issue is simple: earned coins and purchased upgrades disappear between sessions. The engineering issue is deeper: economy persistence must be reliable on mobile devices, survive app updates, tolerate app process kills, support future schema changes, and avoid corrupting a player's progress when a save write fails halfway through an economy transaction.

The current pickup flow also grants coins directly into the wallet during a run. Product direction now requires coins to be rewarded only when a run ends. Upgrade purchases, by contrast, must be committed immediately. This makes economy persistence a transaction problem, not just a "serialize each class" problem.

This feature is local-only. There is no hard currency, no IAP, no receipt validation, and no cloud sync in scope. The implementation still needs enterprise-grade boundaries so future server authority or cloud sync can be added without rewriting gameplay rules.

## Solution

Introduce a centralized, versioned local economy save for the player's soft-currency wallet and upgrade progress.

`CurrencyDefinition` assets gain an editor-generated stable save ID. The ID is serialized into the asset, exposed as a readonly runtime property, and displayed readonly in the Editor. It must not change when the asset is renamed or moved. Duplicate or missing currency save IDs are invalid authoring.

Currency balances and upgrade levels move behind a single `PlayerEconomyState` aggregate. Runtime services continue to expose narrow economy and upgrade interfaces to existing gameplay code, but all durable mutations flow through one economy transaction boundary. This prevents partial saves such as "coins spent but upgrade level not advanced" or "run reward shown but not committed".

Local persistence is implemented as compact DTO JSON stored under Unity's `Application.persistentDataPath`, using a primary file, temporary write file, backup file, schema version, migration pipeline, and corruption recovery. The first implementation should use Unity's built-in `JsonUtility` with simple list DTOs rather than dictionaries. Serialization and file I/O should run on a serialized background `Task` queue over immutable DTO snapshots, while Unity objects and gameplay state are read or mutated only on the main thread.

During a run, pickup collection updates only the run currency accumulator and pickup consumed state. When `RunResultAccepted` publishes the accepted run result, a run reward committer applies the run currency snapshot to the wallet and requests an immediate save commit. Upgrade purchase attempts become economy transactions that spend currency, set the upgrade level, and commit the save before returning a successful purchase result.

## Unity Surfaces

- Runtime assemblies and asmdefs
  - Extend the gameplay economy runtime module with stable currency identity, economy state, local save DTOs, save repository, migration, and save scheduling boundaries.
  - Extend the gameplay upgrades runtime module only where upgrade progress needs to read and write through the centralized economy state.
  - Update the pickup runtime module so accepted pickups during a run do not grant wallet currency directly.
  - Update root gameplay runtime composition so VContainer wires the economy state, local save store, save scheduler, run reward committer, and compatibility adapters.
  - Keep gameplay rules in plain C# services and controllers, consistent with ADR-0002.
  - Keep composition through VContainer, consistent with ADR-0005.
  - Keep local notifications as direct C# events where required, consistent with ADR-0003.

- Editor assemblies, windows, inspectors, importers, or menu items
  - Add editor-only generation for missing `CurrencyDefinition` save IDs.
  - Add readonly editor display for currency save IDs.
  - Add or extend authoring validation for currency definitions and currency catalogs to reject missing or duplicate save IDs.
  - Add repair tooling only if needed for existing assets with missing IDs or duplicates. Any destructive regeneration must be deliberate, not automatic.
  - No custom editor window is required for the first slice.

- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings
  - Existing coin currency definition asset receives a stable save ID.
  - Existing upgrade definition assets continue to use their stable IDs.
  - Existing gameplay scene composition references stay explicit through `GameplayLifetimeScope`.
  - Existing pickup definitions continue to define base currency grants.
  - Existing run preparation UI continues to read wallet balance and upgrade previews, but now reads loaded persisted state.
  - No new Unity package dependency is expected for the first slice.
  - Do not add Unity's `com.unity.serialization` package.
  - Do not intentionally depend on a transitive Newtonsoft package unless the project chooses to add it as a direct dependency later.

- RPC/helper commands, hooks, or shell wrappers
  - Use the Unity AI Agent Connector compile gate after implementation changes.
  - Use targeted Unity EditMode tests first.
  - Use PlayMode tests only for lifecycle, scene composition, app pause/quit hooks, or Unity engine integration behavior.
  - No new RPC helper command or shell wrapper is required.

- Package versioning, changelog, and installation/sync behavior
  - This is project gameplay work, not a distributable package release.
  - No package manifest change is expected.
  - Save schema versioning and migration behavior are required because this feature introduces durable player data.
  - Changelog impact is internal unless the project maintains a player-facing release note stream.

## User Stories

1. As a player, I want my coin balance to remain after closing and reopening the game, so that collected rewards feel permanent.
2. As a player, I want my purchased upgrades to remain after closing and reopening the game, so that progression is not lost.
3. As a player, I want coins earned in a run to be added only when the run ends, so that failed or unfinished run reward behavior is clear.
4. As a player, I want an accepted run result to commit its coin reward once, so that the same run cannot double-grant currency.
5. As a player, I want upgrade purchases to apply immediately, so that spending coins has an immediate visible effect.
6. As a player, I want a purchased upgrade to remain purchased if the game is closed right after buying, so that important progress is durable.
7. As a player, I want a purchase to fail cleanly if it cannot be saved, so that I do not lose coins without receiving the upgrade.
8. As a player, I want my balance and upgrades to load before I interact with run preparation, so that the UI shows real progress.
9. As a player, I want the game to recover from a damaged save when possible, so that one interrupted write does not wipe my progress.
10. As a player, I want the game to start with a sane default state if no save exists, so that first launch works normally.
11. As a player, I want soft-currency progress to be local to this install, so that the current feature does not depend on account sign-in.
12. As a player, I want app updates to preserve my economy progress, so that upgrading the game does not reset me.
13. As a player, I want renamed or moved content assets to keep my saved balances and upgrade levels, so that content organization changes do not reset progress.
14. As a player, I want removed or temporarily unavailable content not to destroy my saved progress silently, so that future content changes are safe.
15. As a player, I want invalid saved values to be repaired rather than crash the game, so that I can keep playing.
16. As a player, I want save work to avoid gameplay hitches, so that collecting and running stay responsive on mobile devices.
17. As a player, I want purchases to prevent rapid duplicate taps while a commit is pending, so that the UI cannot create ambiguous transactions.
18. As a player, I want the game to save important economy commits before backgrounding where possible, so that mobile app switching does not commonly lose progress.
19. As a player, I want backup restoration behavior to be acceptable for soft local progress, so that reinstall or device restore may recover data when the platform supports it.
20. As a designer, I want every currency definition to have a stable save ID, so that balances can be serialized without relying on asset path or name.
21. As a designer, I want a new currency definition to receive an ID automatically in the Editor, so that I do not hand-type persistence identifiers.
22. As a designer, I want currency save IDs to be visible in the Editor, so that I can inspect and discuss authored identities.
23. As a designer, I want currency save IDs to be readonly in normal inspector workflows, so that accidental edits do not reset player balances.
24. As a designer, I want renaming a currency asset to keep the same save ID, so that asset naming remains safe.
25. As a designer, I want moving a currency asset to another folder to keep the same save ID, so that project organization remains safe.
26. As a designer, I want duplicating a currency asset to be caught if it duplicates the save ID, so that content copies do not create ambiguous balances.
27. As a designer, I want missing currency save IDs to be flagged before entering Play Mode or building, so that broken assets are fixed early.
28. As a designer, I want duplicate currency save IDs to be flagged clearly, so that I know which assets need repair.
29. As a designer, I want the existing coin currency definition to receive a stable ID through migration or validation, so that current content becomes persistent.
30. As a designer, I want upgrade definitions to keep using stable IDs, so that existing upgrade authoring remains compatible.
31. As a designer, I want removed upgrade definitions to leave saved unknown records preserved, so that content can be restored or migrated later.
32. As a designer, I want explicit migration mappings for renamed or replaced economy content, so that player progress can move intentionally.
33. As a designer, I want run pickup definitions to remain base grants, so that persistence does not change authored reward values.
34. As a designer, I want the coin pickup multiplier to affect the run snapshot before run-end commit, so that rewarded coins match gameplay tuning.
35. As a designer, I want run-end reward timing to be product-owned, so that future fail-state reward policies can be changed in one place.
36. As a developer, I want one versioned economy aggregate, so that currency balances and upgrade levels are persisted atomically.
37. As a developer, I want a central transaction boundary for economy mutations, so that related changes cannot save independently.
38. As a developer, I want existing `ICurrencyStorage` read/write semantics to be adapted carefully, so that call sites can migrate incrementally.
39. As a developer, I want existing `IUpgradeProgressStorage` semantics to be backed by the same economy state, so that upgrades and wallet share one source of truth.
40. As a developer, I want upgrade purchase to spend and level-up through one service, so that the purchase operation remains atomic.
41. As a developer, I want a failed save commit to prevent purchase success, so that UI state and durable state do not diverge.
42. As a developer, I want run reward commit to consume a `RunCurrencySnapshot`, so that end-of-run persistence reads immutable result data.
43. As a developer, I want pickup collection to avoid wallet writes during `Running`, so that per-pickup I/O never happens.
44. As a developer, I want current-run currency accumulation to stay in memory, so that gameplay performance is independent of file writes.
45. As a developer, I want save DTOs to use lists rather than dictionaries, so that Unity `JsonUtility` can serialize them directly.
46. As a developer, I want runtime state to expose dictionaries or maps internally where useful, so that lookup remains efficient.
47. As a developer, I want DTOs separated from runtime domain objects, so that migrations do not leak into gameplay logic.
48. As a developer, I want the save file to include `schemaVersion`, so that app updates can migrate old saves.
49. As a developer, I want the save file to include a revision or monotonic commit counter, so that debugging and future conflict handling have ordering data.
50. As a developer, I want the loader to parse a save envelope before current DTO deserialization, so that unknown schema versions can be handled intentionally.
51. As a developer, I want stepwise migrations, so that old save versions can upgrade through stable intermediate transformations.
52. As a developer, I want old DTO classes preserved when breaking fields change, so that migration code can still read older saves.
53. As a developer, I want unknown currency IDs to be preserved during load/save, so that temporarily missing content does not erase balances.
54. As a developer, I want unknown upgrade IDs to be preserved during load/save, so that temporarily missing content does not erase levels.
55. As a developer, I want known invalid negative balances to be repaired deterministically, so that corrupted saves do not poison runtime state.
56. As a developer, I want known upgrade levels above current max to be clamped or migrated by policy, so that content tuning changes remain safe.
57. As a developer, I want all save repairs to be observable through logs, so that unexpected data issues can be diagnosed.
58. As a developer, I want the save store to write a temporary file first, so that interrupted writes do not replace a good primary save with a partial file.
59. As a developer, I want a backup save file, so that primary corruption has a recovery path.
60. As a developer, I want save load order to try primary, then backup, then fresh defaults, so that recovery is deterministic.
61. As a developer, I want corrupt save files to be retained or renamed for diagnostics when practical, so that failures are not invisible.
62. As a developer, I want file paths hidden behind an injected path provider, so that tests do not depend on the real device path.
63. As a developer, I want local save I/O hidden behind an interface, so that future cloud/server authority can wrap or replace it.
64. As a developer, I want serialization hidden behind an interface, so that Newtonsoft or another serializer can be introduced later if schema needs outgrow `JsonUtility`.
65. As a developer, I want no direct dependency on PlayerPrefs for economy state, so that storage is explicit, versioned, and recoverable.
66. As a developer, I want no dependency on Unity's deprecated Serialization package, so that the save system starts on a supported baseline.
67. As a developer, I want no new UniTask dependency for the first slice, so that async file I/O does not force a package decision.
68. As a developer, I want no Unity Jobs path for JSON/file I/O, so that managed strings and file operations stay outside data-oriented job constraints.
69. As a developer, I want a serialized background save queue, so that overlapping commits cannot race each other.
70. As a developer, I want important commits to await queue completion when gameplay needs durable success, so that purchase and reward semantics are honest.
71. As a developer, I want coalescing for non-critical flushes, so that repeated state changes do not create excessive disk writes.
72. As a developer, I want app pause and quit hooks to request a flush, so that mobile lifecycle events have a best-effort durability path.
73. As a developer, I want Unity object access to remain on the main thread, so that background serialization does not touch ScriptableObjects or scene objects.
74. As a developer, I want immutable DTO snapshots passed to the worker, so that the background thread cannot observe mutated runtime state.
75. As a developer, I want save errors to surface as structured results, so that UI and services can choose retry, fail, or default behavior.
76. As a developer, I want run preparation UI to render save loading, purchase pending, purchase failed, and loaded states, so that transaction states are visible.
77. As a developer, I want services to remain plain C# where possible, so that EditMode tests cover most behavior.
78. As a developer, I want VContainer to compose persistence services in the gameplay lifetime scope, so that dependencies stay explicit.
79. As a developer, I want no static global save service, so that tests and scene lifetime stay isolated.
80. As a developer, I want save file names and schema constants centralized in the save module, so that storage policy is not scattered.
81. As a developer, I want a checksum or equivalent corruption signal considered for the save payload, so that truncated or malformed data is detected earlier.
82. As a developer, I want no anti-cheat promises for local soft currency, so that the implementation stays proportional to current product scope.
83. As a developer, I want future hard currency or IAP to require a separate authority design, so that local editable files are not mistaken for secure value storage.
84. As a tester, I want EditMode tests for stable currency ID behavior, so that asset rename/move safety is protected.
85. As a tester, I want EditMode tests for duplicate currency ID validation, so that ambiguous balances cannot ship.
86. As a tester, I want EditMode tests for save/load of balances and upgrade levels, so that persistence behavior is covered without Play Mode.
87. As a tester, I want EditMode tests for missing save files, so that first-launch defaults are deterministic.
88. As a tester, I want EditMode tests for corrupt primary and valid backup saves, so that recovery behavior is covered.
89. As a tester, I want EditMode tests for corrupt primary and corrupt backup saves, so that fresh fallback behavior is explicit.
90. As a tester, I want EditMode tests for old schema migration, so that app updates preserve progress.
91. As a tester, I want EditMode tests for unknown currency and upgrade IDs, so that preservation behavior is locked.
92. As a tester, I want EditMode tests for invalid negative balances, so that repair policy is deterministic.
93. As a tester, I want EditMode tests for upgrade levels above current max, so that clamp or migration policy is deterministic.
94. As a tester, I want EditMode tests for purchase commit failure, so that coins and level do not partially change.
95. As a tester, I want EditMode tests for purchase commit success, so that balance and level persist together.
96. As a tester, I want EditMode tests for no per-pickup wallet grant, so that run-end-only reward policy is protected.
97. As a tester, I want EditMode tests for run-result reward commit, so that accepted run snapshots update wallet exactly once.
98. As a tester, I want EditMode tests for save queue ordering, so that later commits cannot be overwritten by earlier worker completions.
99. As a tester, I want PlayMode tests only where Unity lifecycle is required, so that the test suite stays fast and deterministic.
100. As a tester, I want a PlayMode smoke test for gameplay scene composition after persistence wiring, so that serialized scene references and VContainer registration are valid.
101. As a tester, I want a PlayMode or lifecycle-focused check for pause/quit flush hooks if they rely on Unity callbacks, so that mobile lifecycle integration is not untested.
102. As a maintainer, I want a deep local save module with a small interface, so that persistence complexity is hidden behind stable contracts.
103. As a maintainer, I want economy data migration isolated from UI and gameplay controllers, so that schema changes do not spread through the project.
104. As a maintainer, I want platform storage behavior documented in the PRD, so that future agents do not reintroduce PlayerPrefs or per-class saves.
105. As a maintainer, I want future cloud sync to be out of scope but not blocked, so that local save design remains replaceable.
106. As a maintainer, I want future hard currency and IAP to be out of scope, so that the local save is not treated as a secure economy authority.
107. As a maintainer, I want implementation tasks to preserve current module boundaries, so that Economy, Upgrades, Pickups, and root Gameplay composition remain understandable.
108. As a maintainer, I want no unrelated scene, prefab, package, or UI redesign in this persistence slice, so that the diff stays reviewable.

## Implementation Decisions

- Use a centralized `PlayerEconomyState` aggregate for persisted soft economy state.
- Persist currency balances and upgrade levels in the same save transaction.
- Keep `CurrencyStorage` and `UpgradeProgressStorage` style access as compatibility surfaces only if that reduces migration risk; their durable state must come from the central economy aggregate.
- Use stable string save IDs for currencies and upgrades. Currency IDs are added now; upgrade IDs already exist.
- Generate missing currency save IDs in the Editor using a random stable identifier serialized into the asset.
- Do not derive runtime save IDs from asset names, folders, Resources paths, Addressables addresses, or Unity object instance IDs.
- Do not rely solely on Unity meta GUIDs for runtime identity. The asset may use editor metadata as an input if desired, but the stable save ID must be serialized into the asset and available in player builds.
- Treat duplicated currency IDs as invalid authoring. Duplicating an asset may duplicate serialized fields, so validation is mandatory.
- Expose currency save ID as a readonly property for runtime and test code.
- Display currency save ID readonly in normal inspector workflows.
- Avoid an automatic "regenerate ID" path in normal workflows because it would orphan existing saved balances.
- If repair tooling is added, it must be explicit and should warn about save compatibility consequences.
- Continue using `UpgradeDefinition.StableId` for upgrade persistence.
- Add catalog-level or project-level authoring validation for currency save IDs before any persistence implementation depends on them.
- Use one local save file family for the economy state: primary, temporary, and backup.
- Store the save under `Application.persistentDataPath` through an injected path provider.
- Store player economy data as compact JSON DTOs.
- Use Unity `JsonUtility` for the first implementation because the save shape can be simple, field-based, and list-oriented.
- Use list DTO entries for balances and levels because Unity serialization does not support dictionaries.
- Keep runtime lookup maps separate from serialized DTO layout.
- Do not add Unity's `com.unity.serialization` package.
- Do not rely on transitive Newtonsoft availability. If later needed, add Newtonsoft as a direct package dependency in a separate decision.
- Do not add UniTask for the first implementation. Use standard `Task` and cancellation where async work is needed.
- Do not use Unity Jobs or Burst for JSON serialization or file I/O. Jobs are for blittable data-oriented work, while save JSON and file operations use managed strings and system I/O.
- Perform Unity object reads, catalog resolution, and mutable gameplay state changes on the main thread.
- Create immutable DTO snapshots on the main thread before sending work to the background save queue.
- Let the background save queue serialize DTOs and write files.
- Use a single-writer queue to preserve commit order.
- Purchases wait for the important save commit to finish before reporting success.
- Run-end reward commits should be immediate and durable from the gameplay service perspective, while UI can represent pending commit state if needed.
- Pickup collection during `Running` updates current-run state only. It must not grant to wallet storage directly.
- `RunResultAccepted` is the first-choice hook for committing run rewards because it already carries the accepted run's immutable currency snapshot.
- Run reward commit must be idempotent per accepted result or protected by run-end flow latching, so duplicate notifications cannot double grant.
- Upgrade purchase should mutate balance and upgrade level in one transaction.
- If save commit fails during purchase, return a non-successful purchase result and leave runtime state consistent with durable state.
- If save commit fails during run reward commit, keep the run-end flow from silently treating the reward as permanently granted. The UI should receive an error or pending state rather than misreport durable progress.
- Include `schemaVersion` in every save.
- Include a revision or commit counter in every save.
- Keep old DTO shapes as long as they are needed for migration.
- Migrations are stepwise and deterministic.
- Unknown future schema versions should fail safely rather than being loaded as current data.
- Missing fields in older saves should receive explicit migration defaults.
- Unknown currency and upgrade IDs are preserved unless a deliberate migration tombstones them.
- Known negative balances are repaired according to policy, with the first policy expected to clamp soft currency to zero and log a warning.
- Known upgrade levels above current max are handled by explicit policy. The default expectation is clamp to current max unless a migration mapping says otherwise.
- Unknown upgrade levels for unknown IDs are preserved as saved data, not interpreted against current catalog max.
- Use backup recovery: try primary, then backup, then defaults.
- Keep save payload small enough that Android Auto Backup quota and iOS backup cost are irrelevant for normal use.
- Accept that local soft-currency files can be edited by advanced users. This slice is reliability-focused, not anti-cheat or fraud-proof.
- Keep hard currency, IAP, receipt validation, and server authority separate from this local save feature.

## Testing Decisions

- Good tests should assert external behavior at module boundaries: loaded balances, loaded upgrade levels, transaction results, durable commit success or failure, run reward timing, migration output, corruption fallback, and authoring validation. Tests should not assert private fields or use reflection for nonpublic access.
- Prefer EditMode tests for deep plain C# modules:
  - currency save ID validation
  - economy state transactions
  - currency and upgrade compatibility adapters
  - purchase transaction behavior
  - run reward committer behavior
  - save DTO serialization
  - migration pipeline
  - file store primary/backup fallback using temporary test paths
  - save queue ordering
- Use existing test patterns around economy storage, run currency accumulation, run-end flow, run preparation, upgrade purchase flow, and authoring validation.
- Use PlayMode tests only for Unity lifecycle or scene integration:
  - gameplay scene VContainer composition after persistence registration
  - run preparation UI binding to loaded state if not testable through view models alone
  - pause/background flush hook if implemented through MonoBehaviour lifecycle
  - any AssetDatabase/editor-only ID generation behavior that cannot be covered with normal EditMode tests
- Run compile before tests after code changes.
- Run targeted tests for changed modules before broader regression.
- Editor ID generation tests must not depend on hardcoded asset paths or GUIDs. Create temporary test assets or use explicit test hooks where appropriate.
- File store tests must use injected temporary paths, not the real player `persistentDataPath`.
- Migration tests should use frozen JSON fixtures representing older schemas.
- Corruption tests should use malformed or truncated JSON fixtures.
- Save queue tests should control completion ordering deterministically rather than relying on sleeps.
- Transaction tests should inject a failing save store to prove runtime rollback or non-success behavior.
- Run reward tests should prove pickup collection does not touch wallet state before run result acceptance.
- Run reward tests should prove accepted run result commits wallet state exactly once.
- Purchase tests should prove insufficient currency, max level, invalid definition, commit failure, and success cases.
- Validation tests should prove duplicate currency save IDs are invalid.
- Validation tests should prove missing currency save IDs are invalid after editor generation has had a chance to run.
- Performance checks should focus on no file I/O per pickup and no Unity object access from background threads.

## Release and Compatibility

- Unity version assumption: Unity 6 project, currently on Unity 6000.3.x in project settings.
- Mobile target assumption: iOS and Android behavior matters.
- `Application.persistentDataPath` is the correct Unity-provided root for durable local app data, but the platform path differs by OS.
- On iOS, Unity maps persistent data to the app container `Documents` directory. This data is backed up by default. Economy save data is small user progress, so backup is acceptable.
- On Android, Unity maps persistent data to app-specific external files on most devices. App-specific files are removed on uninstall, and Android Auto Backup may include them by default.
- Android Auto Backup has quota and timing constraints. The economy save must remain small and must not rely on backup as guaranteed cloud sync.
- App updates should preserve data when the bundle identifier/package name remains stable.
- Changing bundle identifier/package name is a compatibility break for the save location unless a platform-specific migration is designed.
- The first save schema should be versioned from day one.
- This feature creates durable player data, so future changes to field names, ID formats, currency definitions, upgrade definitions, max levels, and reward policy require migration review.
- No package dependency change is expected.
- Unity's `com.unity.serialization` package should not be introduced because it has been marked deprecated in Unity's 2026 serialization update.
- PlayerPrefs must not be used for economy state because it is unencrypted preference storage, not a versioned transactional save.
- Backward compatibility risk: existing players before this feature have no local economy save. First launch after update should initialize from current in-memory defaults or authored defaults.
- Backward compatibility risk: existing `CurrencyDefinition` assets lack save IDs. Editor generation and validation must handle them before runtime persistence ships.
- Backward compatibility risk: current pickup behavior grants wallet currency during the run. Changing to run-end commit changes reward timing and tests must lock this new behavior.
- Backward compatibility risk: purchase API may need to represent pending or failed save commit states, which can affect UI assumptions.

## Out of Scope

- Hard currency.
- In-app purchases.
- Receipt validation.
- Server authority.
- Cloud sync.
- Cross-device conflict resolution.
- Account login.
- Anti-cheat or tamper-proof local currency.
- Encryption for economy security.
- Analytics pipelines.
- Remote config.
- Addressables schema changes.
- New Unity packages unless separately approved.
- Full UI redesign of run preparation or run-end screens.
- Persisting pickup consumed state across app restarts unless explicitly added as a separate requirement.
- Persisting current in-progress run state.
- Persisting settings, audio preferences, input preferences, cosmetics, character selection, achievements, or other non-economy data.
- Save import/export UI.
- Player-facing save slot management.

## Further Notes

- Centralizing saves is the recommended design because this economy has cross-domain invariants: purchase couples wallet balance and upgrade level, and run-end rewards couple run result and wallet balance. Per-class saves would only be appropriate for independent preference-like state with no shared invariants, no shared migrations, and no transaction boundaries.
- `JsonUtility` is acceptable for the first slice because the DTO shape can be intentionally simple. Its limitations, especially fields-only serialization and no dictionaries, should shape the DTOs rather than leak into gameplay state.
- Background serialization is safe only for plain DTO snapshots. Do not serialize live gameplay services, ScriptableObjects, MonoBehaviours, catalogs, or mutable dictionaries on a worker thread.
- Mobile operating systems can suspend or kill apps quickly after backgrounding. The implementation should save immediately after important economy commits instead of relying on quit-time flushing.
- Backup behavior is not cloud sync. Android and iOS may back up app data under user/platform conditions, but the product promise remains local persistent save only.
- Relevant references reviewed while drafting this PRD:
  - Unity `Application.persistentDataPath`: https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
  - Unity PlayerPrefs: https://docs.unity3d.com/ScriptReference/PlayerPrefs.html
  - Unity JSON serialization: https://docs.unity3d.com/Manual/json-serialization.html
  - Unity Job System overview: https://docs.unity3d.com/Manual/job-system-overview.html
  - Android Auto Backup: https://developer.android.com/identity/data/autobackup
  - Android app-specific files: https://developer.android.com/training/data-storage/app-specific
  - Apple file system guide: https://developer.apple.com/library/archive/documentation/FileManagement/Conceptual/FileSystemProgrammingGuide/FileSystemOverview/FileSystemOverview.html
  - Unity Serialization update: https://discussions.unity.com/t/coreclr-scripting-and-serialization-update-june-2026/1723299
