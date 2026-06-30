# PRD: Run Preparation Gameplay Upgrades

## Problem Statement

The current gameplay loop starts directly in the pre-launch/slingshot state. That keeps the first playable slice simple, but it leaves no state-owned place for the player to inspect app-session coins, buy upgrades, and intentionally start the next run.

The game now has coin collection work merged, but the gameplay economy still needs to be shaped for upgrade spending. The existing collection model can grant and display collected amounts, while upgrades require an atomic spend-and-level operation, clear naming that does not collide with Unity `Resources`, and a clean distinction between currency definitions, collectible pickup definitions, and grants of currency from pickup collection.

The upgrade system also needs to scale beyond a handful of hand-authored rows. Designers should be able to author many levels for upgrades such as `SlingshotLaunchPower`, `PlayerMaxSpeed`, `PlayerSteeringResponsiveness`, and `CoinPickupMultiplier` using curves, min/max projections, rounding rules, and exact overrides for standout levels. Gameplay code should not calculate upgraded values ad hoc in different controllers.

There is an architectural constraint around Slingshot: Slingshot is a feature leaf. It must not know about upgrades, economy, game state, or higher gameplay orchestration. Slingshot should expose pull data. Gameplay should resolve stats, calculate the final launch impulse, and apply it to the run target.

The goal of this PRD is to define the first implementation slice for run preparation upgrades, keep the code testable in plain C# services, and preserve the current dependency direction and VContainer composition style.

## Solution

Add a new `Run Preparation` gameplay state before the existing pre-launch state. It becomes the default initial gameplay state when the scene starts. In this state, the player sees their coin balance, a list of upgrade cards with icons, current level, current/next effect preview, next cost, and a `Continue` command. Buying upgrades can only happen in this state. Pressing `Continue` creates an immutable run modifier snapshot from the current upgrade levels, transitions to the pre-launch state, and enables the existing slingshot capture flow.

Create or extract a `Game.Gameplay.Economy` domain for app-session currency concepts. It should own currency definitions, balances, grants, storage, spending, and per-run currency accumulation. Do not introduce a gameplay namespace, assembly, or folder named `Resources`. The target glossary is:

- `CurrencyDefinition`: the identity of a spendable/grantable currency, initially coins.
- `CurrencyGrant`: an immutable value object describing a currency and a positive amount to add.
- `CurrencyBalance`: a read model for UI display.
- `ICurrencyStorage` / `CurrencyStorage`: app-session balances with `GetAmount`, `Grant`, and `TrySpend`.
- `RunCurrencyAccumulator` / `RunCurrencySnapshot`: per-run earned totals for the run summary.

Keep pickup collectibles separate from currencies. A `PickupDefinition` describes a collectible pickup. A pickup can contain a `CurrencyGrant`, initially coin grants. The pickup collection flow applies pickup-specific modifiers, grants currency to app-session storage, records final earned amounts in the run accumulator, and emits collection events with both base and final applied amounts.

Add an upgrades/stat resolution domain around asset-authored stat ids and upgrade definitions. The first four upgrade stat ids are:

- `SlingshotLaunchPower`
- `PlayerMaxSpeed`
- `PlayerSteeringResponsiveness`
- `CoinPickupMultiplier`

Each upgrade is represented by an `UpgradeDefinition` ScriptableObject containing an icon, stable id, target stat id, max level, effect progression, cost progression, formatting metadata, and validation. The upgrade catalog is a ScriptableObject referenced by `GameplayLifetimeScope`. It owns the ordered upgrade list and one catalog-wide purchase currency. It does not own player balances or upgrade levels.

Resolve gameplay values through a central `GameplayStatResolver`. Callers provide a stat id and a base value. The resolver applies the active modifier sources in a deterministic order and returns the final value. The first upgrade slice uses upgrade-driven multiplicative factors, but the resolver must support future modifier sources such as run ramps, temporary buffs, or event modifiers without forcing individual gameplay controllers to duplicate stacking logic.

Keep MonoBehaviours shallow. Evaluation, purchasing, preview building, stat resolving, launch impulse calculation, and coin multiplier rounding should live in plain C# classes. Scene objects and UI views forward input, render immutable view models, and expose Unity references through VContainer.

## Unity Surfaces

Runtime assemblies and modules:

- Add a `Game.Gameplay.Economy` runtime module for currency definitions, grants, storage, spending, and run accumulation.
- Add an upgrades/stat module under gameplay for upgrade definitions, upgrade progress, progression evaluation, purchase flow, preview flow, stat modifiers, and run modifier snapshots.
- Update the pickup module so pickup collection depends on Economy types instead of owning generic resource storage concepts.
- Keep the Slingshot feature assembly as a leaf. It may expose pull strength, pull offset, final pull point, and related pull-frame data, but it must not depend on gameplay upgrades, economy, or game state orchestration.
- Keep root gameplay composition responsible for connecting state flow, run preparation UI, upgrade services, slingshot launch calculation, player movement stat resolution, pickup grant resolution, and scene references.

Scene and asset surfaces:

- Add a `Run Preparation` gameplay state id asset.
- Update the gameplay state config so `Run Preparation` is the initial state.
- Add valid transitions from `Run Preparation` to `Pre-Launch`, from `Pre-Launch` to `Running`, from `Running` to `Run Ended`, and from `Run Ended` back to `Run Preparation`.
- Add a run preparation UI surface in the gameplay scene or prefab hierarchy. It displays coin balance, upgrade entries, buy buttons, maxed state, insufficient funds state, and the continue command.
- Add a coin `CurrencyDefinition` asset.
- Add `GameplayStatId` assets for the four first upgrade stats.
- Add four `UpgradeDefinition` assets with assigned UI icons: slingshot launch power, player max speed, player steering responsiveness, and coin pickup multiplier.
- Add an `UpgradeCatalog` asset that references the four upgrade definitions and the coin purchase currency.
- Add a `GameplaySlingshotLaunchConfig` asset for gameplay-owned launch impulse tuning.
- Update `GameplayLifetimeScope` serialized references for state ids, economy services, upgrade catalog/configs, run preparation view, stat ids/configs, and launch impulse services.

Editor surfaces:

- Add `OnValidate` validation for upgrade definitions, upgrade catalogs, progression data, gameplay stat ids, currency definitions, and gameplay launch config where invalid authoring can break runtime behavior.
- Validation should warn for duplicate ids, missing icons, missing target stats, missing purchase currency, negative/zero costs where not allowed, invalid level ranges, missing curves, malformed exact overrides, and conflicting definitions.
- No custom editor window is required for the first slice.

Testing and tooling surfaces:

- Use Unity EditMode tests for pure C# economy, upgrade, stat resolver, launch calculation, and pickup grant resolution behavior.
- Use PlayMode tests only where scene state, VContainer composition, MonoBehaviour lifecycle, UI binding, or physics integration require Unity runtime behavior.
- Use the Unity AI Agent Connector compile gate before running tests after implementation changes.

## User Stories

Player-facing stories:

- As a player, when I open the gameplay scene, I start in `Run Preparation`, not directly in the slingshot launch state.
- As a player, I can see my current coin balance before launching a run.
- As a player, I can see four upgrade options before a run: slingshot launch power, player max speed, player steering responsiveness, and coin pickup multiplier.
- As a player, each upgrade option has a recognizable UI icon.
- As a player, each upgrade option shows its current level.
- As a player, each upgrade option shows its maximum level or clearly indicates when it is maxed.
- As a player, each upgrade option shows the next coin cost when another level is available.
- As a player, each upgrade option shows the current gameplay effect in readable form.
- As a player, each upgrade option shows the next gameplay effect before I buy it.
- As a player, I can buy an upgrade when I have enough coins.
- As a player, buying an upgrade immediately subtracts the required coins from my visible balance.
- As a player, buying an upgrade immediately increments that upgrade level.
- As a player, after buying an upgrade, the next cost and next effect preview update immediately.
- As a player, I cannot buy an upgrade if I do not have enough coins.
- As a player, the UI makes an unaffordable upgrade non-actionable or clearly disabled.
- As a player, I cannot buy an upgrade beyond its maximum level.
- As a player, a maxed upgrade no longer shows a next purchase cost.
- As a player, I can press `Continue` from run preparation to proceed to the slingshot launch setup.
- As a player, pressing `Continue` does not start the run yet.
- As a player, after pressing `Continue`, I can interact with the slingshot as before.
- As a player, launching from the slingshot starts the run as before.
- As a player, bought upgrade levels affect the next run after I press `Continue`.
- As a player, bought upgrade levels remain available across multiple runs in the same app session.
- As a player, app-session upgrade progress and coin balances reset after an app restart in the first implementation slice.
- As a player, coin pickup multiplier increases coins earned from coin pickups during a run.
- As a player, coin pickup multiplier does not increase non-pickup coin grants.
- As a player, coin pickup multiplier does not affect launch strength, speed, or steering.
- As a player, slingshot launch power makes the launch impulse stronger according to authored tuning.
- As a player, player max speed changes the maximum movement speed during the running state.
- As a player, steering responsiveness changes how quickly steering input affects movement during the running state.
- As a player, upgrade choices made after a run end are available before the next run starts.
- As a player, the run end flow returns me to run preparation rather than straight back to slingshot setup.

Designer-facing stories:

- As a designer, I can create a new upgrade definition without changing gameplay code.
- As a designer, I can assign a sprite icon to each upgrade definition.
- As a designer, I can choose which gameplay stat an upgrade modifies.
- As a designer, I can set the maximum level of an upgrade.
- As a designer, I can author upgrade costs using min/max values and an AnimationCurve.
- As a designer, I can author upgrade effects using min/max values and an AnimationCurve.
- As a designer, I can use exact overrides for specific upgrade levels that should break from the curve.
- As a designer, exact overrides take precedence over curve-projected values.
- As a designer, I can make one upgrade much more expensive than a linear progression without adding every level by hand.
- As a designer, I can define rounding behavior for upgrade costs.
- As a designer, I can define rounding or step behavior for displayed upgrade effects.
- As a designer, I can format an effect as a multiplier such as `x1.1`, `x1.5`, or `x10`.
- As a designer, I can format an effect as a percent or plain number when needed by future upgrades.
- As a designer, level 0 represents the baseline unpurchased state.
- As a designer, cost level 1 is the cost to buy the first upgrade level.
- As a designer, effect level 0 is the unpurchased effect value.
- As a designer, effect level N is the effect after N purchased levels.
- As a designer, a maxed upgrade has no next cost.
- As a designer, all upgrade definitions in a catalog use one catalog-wide purchase currency for the first slice.
- As a designer, the upgrade catalog controls display order in the run preparation UI.
- As a designer, invalid upgrade authoring produces warnings before runtime where possible.
- As a designer, duplicate upgrade ids are flagged as invalid.
- As a designer, duplicate gameplay stat ids are flagged as invalid.
- As a designer, missing icons are flagged before a build-quality slice ships.
- As a designer, invalid curves or negative costs are flagged before runtime.
- As a designer, I can tune gameplay launch impulse separately from the Slingshot feature's pull presentation.
- As a designer, I can tune minimum and maximum forward launch impulse.
- As a designer, I can tune how normalized pull strength maps to launch impulse.
- As a designer, I can tune lateral launch angle from pull offset.
- As a designer, I can tune upward impulse separately from forward impulse.
- As a designer, optional total impulse clamps prevent impossible authored launch values.
- As a designer, I can tune player movement base values separately from upgrade multipliers.
- As a designer, I can tune coin pickup base grants separately from coin pickup multiplier.

Developer-facing stories:

- As a developer, I can resolve any gameplay stat through one stat resolver instead of recalculating upgrade math in controllers.
- As a developer, a caller provides the base value to the stat resolver.
- As a developer, the stat resolver does not read Slingshot, movement, pickup, or economy configs directly.
- As a developer, modifier source order is explicit and deterministic.
- As a developer, modifier operations apply in this order: `FlatAdd`, `AdditivePercent`, `MultiplicativeFactor`, `ClampMin`, `ClampMax`.
- As a developer, the first four upgrades use `MultiplicativeFactor` modifiers.
- As a developer, future sources such as run ramps can be added as explicit modifier sources without scene search or static registries.
- As a developer, run modifiers are snapshotted when the player presses `Continue`.
- As a developer, runtime changes in upgrade levels after `Continue` do not mutate the active run snapshot unless explicitly designed later.
- As a developer, purchase flow is a single atomic operation.
- As a developer, purchase flow spends coins and increments upgrade level together.
- As a developer, purchase flow does not increment level if spending fails.
- As a developer, purchase flow returns structured results for success, max level, insufficient currency, missing definition, and invalid definition.
- As a developer, `ICurrencyStorage.TrySpend` returns false for insufficient balance.
- As a developer, invalid arguments to currency storage fail fast instead of returning false.
- As a developer, upgrade progress storage exposes levels without depending on UI.
- As a developer, only the run preparation purchase service mutates upgrade progress in the first slice.
- As a developer, upgrade preview building is pure and testable.
- As a developer, the run preparation UI receives immutable preview models.
- As a developer, run preparation UI views do not own purchase, balance, or progression rules.
- As a developer, the Slingshot feature does not reference upgrade definitions, stat ids, economy storage, or gameplay state ids.
- As a developer, Slingshot exposes pull strength and pull offset data needed by gameplay.
- As a developer, gameplay calculates a `LaunchImpulse` from pull data, launch config, and resolved `SlingshotLaunchPower`.
- As a developer, launch impulse calculation is pure and unit tested.
- As a developer, launch impulse application is the narrow physics mutation boundary.
- As a developer, player steering resolves `PlayerMaxSpeed` and `PlayerSteeringResponsiveness` during the running state.
- As a developer, pickup collection resolves `CoinPickupMultiplier` only for coin pickup grants.
- As a developer, coin multiplier fractional carry is per-run and resets with run currency accumulation.
- As a developer, run currency snapshots store final earned totals only.
- As a developer, pickup collection events include both base and final applied amounts.
- As a developer, root gameplay composition wires services through VContainer.
- As a developer, scene views are registered as interfaces rather than injected as concrete MonoBehaviours.
- As a developer, tests can instantiate the core economy and upgrade services without Unity lifecycle callbacks.

Tester-facing stories:

- As a tester, I can verify the gameplay scene starts in run preparation.
- As a tester, I can verify slingshot input is disabled during run preparation.
- As a tester, I can verify slingshot input is enabled after pressing `Continue`.
- As a tester, I can verify the run starts only after slingshot launch, not after `Continue`.
- As a tester, I can verify buying with exact required coins succeeds.
- As a tester, I can verify buying with one coin less than required fails.
- As a tester, I can verify buying a maxed upgrade fails without spending coins.
- As a tester, I can verify duplicate or invalid catalog authoring is detected.
- As a tester, I can verify upgrade preview values match progression evaluator values.
- As a tester, I can verify exact override levels beat curve-projected levels.
- As a tester, I can verify modifier operation order with multiple modifier sources.
- As a tester, I can verify the active run snapshot remains stable after state transition to pre-launch.
- As a tester, I can verify slingshot launch power changes the final impulse but not Slingshot pull reporting.
- As a tester, I can verify coin pickup multiplier uses base grant, fractional carry, and floor rules.
- As a tester, I can verify run summary earned totals use final applied coin grants.
- As a tester, I can verify app-session state resets when storage instances are recreated.

Maintainer-facing stories:

- As a maintainer, I can find economy concepts under gameplay economy naming, not `Resources`.
- As a maintainer, I can change future persistence behind upgrade progress and currency storage without changing UI widgets.
- As a maintainer, I can add future upgrade types by adding stat ids and definitions rather than editing a switch in every gameplay controller.
- As a maintainer, I can reason about dependency direction: leaf features emit data, root gameplay composes behavior.
- As a maintainer, I can keep MonoBehaviours small because rules live in injectable services.
- As a maintainer, I can split future persistence, analytics, or remote economy work into separate PRs.

## Implementation Decisions

State flow:

- `Run Preparation` is a gameplay state, not a modal layered over pre-launch.
- `Run Preparation` is the initial state in the gameplay state config.
- `Continue` transitions from `Run Preparation` to `Pre-Launch`.
- Slingshot launch transitions from `Pre-Launch` to `Running`.
- Run completion transitions from `Running` to `Run Ended`.
- Run restart transitions from `Run Ended` back to `Run Preparation`.
- Slingshot capture is enabled only while the current state is `Pre-Launch`.
- Upgrade buying is accepted only while the current state is `Run Preparation`.

Economy naming and ownership:

- Use `Game.Gameplay.Economy` for gameplay currency concepts.
- Do not use `Resource` or `Resources` as the domain term for currency, balances, grants, or storage.
- Current `Resource*` code should be migrated to the target currency glossary as part of or immediately before the upgrade implementation slice.
- Do not use `Currency` for collectible pickup definitions. Pickups are collectibles; currencies are balances.
- Use `CurrencyGrant` for grants from pickups or other gameplay sources.
- `CurrencyStorage` is app-session storage in the first slice.
- `ICurrencyStorage` exposes `GetAmount`, `Grant`, and `TrySpend`.
- `TrySpend` returns false only for insufficient balance; invalid definitions or invalid amounts fail fast.
- There is no separate wallet, payment port, marketplace port, SKU, or IAP abstraction in the first slice.

Upgrade authoring:

- `UpgradeCatalog` is a ScriptableObject.
- `UpgradeCatalog` contains ordered upgrade definitions and one catalog-wide purchase currency.
- `UpgradeCatalog` does not store balances or player upgrade levels.
- `UpgradeDefinition` is a ScriptableObject.
- Every `UpgradeDefinition` has a stable id, icon, display metadata, target `GameplayStatId`, max level, cost progression, and effect progression.
- Progression supports min value, max value, normalized AnimationCurve, rounding/step rules, exact level overrides, and formatting metadata.
- Exact overrides win over curve projection.
- Level 0 is always baseline/unpurchased.
- Cost values are defined for purchasing levels 1 through max level.
- Effect values are defined for levels 0 through max level.
- A maxed upgrade has no next cost and should produce a maxed preview state.
- The first upgrade operation type is `MultiplicativeFactor` for all four upgrade stats.

Progress and purchasing:

- Use `IUpgradeProgressStorage` or `IUpgradeProgressModel` naming for upgrade levels; prefer storage if it mutates and model if it is read-model-only.
- Upgrade progress is app-session only in the first slice.
- Upgrade progress survives multiple runs while the app remains open.
- Upgrade progress resets when the app restarts in the first slice.
- Purchase service is the only mutator for upgrade levels in the first slice.
- Purchase service performs cost lookup, currency spend, and level increment as one operation.
- Purchase service returns structured results: success, max level, insufficient currency, missing definition, and invalid definition.
- UI renders results but does not implement purchase rules.

Stat resolution:

- Use a central `GameplayStatResolver` for upgraded gameplay values.
- Callers pass base values to the resolver.
- The resolver accepts explicit modifier sources through composition.
- The resolver does not search the scene and does not use static registries.
- Modifier source ordering is deterministic and visible in composition.
- Operation order is `FlatAdd`, `AdditivePercent`, `MultiplicativeFactor`, `ClampMin`, then `ClampMax`.
- Resolver output should be deterministic for the same base value, stat id, and modifier source set.
- The first active source is upgrade levels from the run modifier snapshot.
- Future sources such as run ramps or temporary buffs should plug in without changing gameplay controllers.

Run modifier snapshot:

- Pressing `Continue` builds an immutable run modifier snapshot.
- The active snapshot is used by gameplay systems for the upcoming run.
- The snapshot is built from current upgrade levels and definitions.
- Runtime upgrade purchases after snapshot creation do not affect the active run unless a future PR explicitly changes that behavior.
- Snapshot creation should fail clearly if required definitions are invalid.

Slingshot and launch:

- Slingshot remains a feature leaf.
- Slingshot reports pull strength and pull offset; it does not calculate final upgraded push impulse.
- Gameplay owns final launch impulse calculation.
- Use `GameplaySlingshotLaunchConfig` for gameplay-owned launch tuning.
- `GameplaySlingshotLaunchConfig` contains minimum forward impulse, maximum forward impulse, normalized pull-strength curve, maximum lateral launch angle, normalized lateral-angle curve, upward impulse, and optional total impulse clamps.
- `SlingshotLaunchPower` modifies the gameplay-calculated launch impulse.
- A pure `LaunchImpulseCalculator` returns an immutable launch impulse value.
- A narrow launch application component mutates physics using the calculated impulse.
- Existing Slingshot config remains responsible only for Slingshot-local pull/presentation behavior after migration.

Movement and pickups:

- `PlayerMaxSpeed` is resolved during running movement.
- `PlayerSteeringResponsiveness` is resolved during running movement.
- Movement controllers should not read upgrade levels directly.
- `CoinPickupMultiplier` applies only to coin grants from collectible pickups.
- `CoinPickupMultiplier` does not apply to mission payouts, ads, IAP grants, debug grants, or non-coin pickup types.
- Coin pickup multiplier uses per-run fractional carry plus flooring so fractional value is not lost across pickups in the same run.
- Fractional carry resets with the run currency accumulator.
- Fractional carry is not persisted to app-session currency storage.
- Pickup collection events include base amount and final applied amount.
- Run currency snapshots store final earned totals only.

UI:

- Run preparation UI is a view over immutable preview models.
- The UI does not calculate upgrade effects, costs, affordability, or stat modifiers.
- The UI can send buy commands and continue commands to injected controllers/services.
- UI icon sprites come from upgrade definitions.
- UI should handle affordable, unaffordable, maxed, missing/invalid definition, and purchase-success states.
- The first slice may use simple production-ready layout and placeholder art if final icons are not yet available, but each upgrade definition must expose an icon reference.

Validation:

- Invalid authoring should be caught in `OnValidate` where practical and fail composition where runtime safety requires it.
- Catalog validation covers duplicate definitions, duplicate ids, null definitions, missing purchase currency, and ordering issues.
- Upgrade validation covers missing id, missing icon, missing stat id, invalid max level, invalid progression curves, invalid overrides, and invalid cost/effect values.
- Launch config validation covers negative impulses, invalid angle ranges, null curves, and impossible clamps.
- Runtime services should fail fast for invalid definitions that escaped editor validation.

## Testing Decisions

EditMode tests:

- Test currency storage grant, get amount, exact spend, insufficient spend, and invalid argument behavior.
- Test upgrade progress storage default level, set/increment behavior, max-level boundaries, and invalid definition handling.
- Test progression evaluation for level 0, first level, max level, nonlinear curve values, rounding, step behavior, and exact overrides.
- Test upgrade preview creation for affordable, unaffordable, maxed, missing definition, current effect, next effect, and next cost states.
- Test purchase service atomicity: spend and level increment both happen on success, neither happens on failure.
- Test purchase result values: success, max level, insufficient currency, missing definition, and invalid definition.
- Test gameplay stat resolver operation ordering with multiple modifier sources.
- Test gameplay stat resolver determinism for identical inputs.
- Test run modifier snapshot creation from upgrade levels.
- Test that changing upgrade progress after snapshot creation does not mutate the snapshot.
- Test `LaunchImpulseCalculator` for min/max pull strength, lateral angle mapping, upward impulse, launch power multiplier, and optional clamps.
- Test player movement stat resolution through a fake modifier source without using MonoBehaviour lifecycle.
- Test coin pickup grant resolution for x1, fractional multiplier, large multiplier, fractional carry, run reset, non-coin grant bypass, and final amount flooring.
- Test run currency accumulator records final earned totals only.
- Test pickup collection event data includes base and final amounts.
- Test catalog and definition validation logic where it can be expressed without Unity scene lifecycle.

PlayMode tests:

- Test the gameplay scene starts in `Run Preparation` using the configured gameplay state asset.
- Test slingshot capture is disabled in `Run Preparation`.
- Test pressing `Continue` transitions to `Pre-Launch` and enables slingshot capture.
- Test pressing `Continue` creates an active run modifier snapshot.
- Test slingshot launch transitions from `Pre-Launch` to `Running` and applies gameplay-calculated launch impulse.
- Test run completion returns from `Run Ended` to `Run Preparation` for the next run.
- Test run preparation UI renders upgrade previews from the catalog.
- Test run preparation UI buy command updates balance and preview state through services.
- Test VContainer scene composition resolves required economy, upgrade, stat, state, and launch services.
- Test any runtime MonoBehaviour boundary that cannot be tested deterministically in EditMode.

Regression coverage:

- Existing slingshot behavior should remain covered by Slingshot feature tests for pull reporting and view behavior.
- Existing gameplay flow tests should be updated for the new initial state and additional transition.
- Existing pickup collection tests should be updated from resource terminology to currency terminology once the rename lands.
- Existing run end flow tests should be updated so restart returns to run preparation.
- Existing lifetime scope tests should cover new registrations and serialized references.

Test constraints:

- Prefer plain C# EditMode tests for all rules that do not require Unity runtime behavior.
- Do not rely on `Awake`, `Start`, or `OnDestroy` in EditMode tests for runtime scripts.
- Use PlayMode tests for scene composition and runtime view/controller lifecycle.
- Use NUnit constraint-style assertions in tests.
- Run compile through the Unity AI Agent Connector before targeted tests after implementation changes.

## Release and Compatibility

- This is a gameplay feature PRD for the current Unity project, not a reusable package release.
- No new Unity package dependency is required for the first slice. Existing UGUI and VContainer dependencies are sufficient unless implementation proves otherwise.
- The first slice changes gameplay scene flow by making `Run Preparation` the initial state.
- The first slice changes run restart flow by returning from `Run Ended` to `Run Preparation`.
- The first slice likely requires serialized scene and asset reference updates in `GameplayLifetimeScope` and gameplay state config assets.
- The first slice intentionally uses app-session storage only. There is no save-file migration because no app-restart persistence is introduced.
- If persistence is added later, currency balances and upgrade progress need explicit save schema decisions and migration rules.
- Existing coin/resource implementation naming may need a refactor from current `Resource*` names to the target `Currency*` glossary. That refactor should be kept precise and should avoid broad unrelated churn.
- Existing code, tests, and asset names that still use `Resource` for gameplay currency should be renamed when the economy module is introduced, but Unity `Resources` APIs or folders should not be used for this feature.
- Slingshot public contracts may need to change from final launch speed/impulse reporting to pull data reporting. That is a deliberate dependency-direction correction and should be tested at the feature boundary.
- Balancing values in ScriptableObjects are content data and can change without code changes after the architecture lands.

## Out of Scope

- Persistent save/load across app restarts.
- Cloud sync, remote config, live economy, A/B testing, or server-authoritative balances.
- IAP, ads, missions, daily rewards, or non-pickup coin grant systems.
- Multiple purchase currencies in one upgrade catalog.
- Per-upgrade purchase currencies.
- SKU or store product modeling.
- Analytics events, telemetry dashboards, or economy fraud prevention.
- Final visual polish for the run preparation UI beyond a usable production-aligned first slice.
- Custom editor windows for upgrade balancing.
- Addressables schema changes.
- New packages.
- Full rebalance of player movement, slingshot, or pickup values.
- General-purpose formula scripting for designers.
- Applying `CoinPickupMultiplier` to non-pickup rewards.
- Making Slingshot depend on gameplay upgrades or economy.
- Saving fractional coin carry beyond the active run.
- A generalized marketplace/domain commerce abstraction.

## Further Notes

Requirements are largely answered. The remaining implementation-slice assumption is that the first PRD targets a thin vertical slice: run preparation UI, app-session economy spending, upgrade authoring, purchase/preview services, stat resolver, run snapshot, and the first gameplay integrations. If the desired first implementation PR is domain-only, trim the UI and scene PlayMode stories into a follow-up PR.

The currently agreed naming is `CoinPickupMultiplier`, `SlingshotLaunchPower`, `PlayerMaxSpeed`, and `PlayerSteeringResponsiveness`. After the coin implementation is merged into this branch, class names should be updated precisely to the target glossary: `CurrencyDefinition`, `CurrencyGrant`, `CurrencyStorage`, `RunCurrencyAccumulator`, and related Economy names.

`GameplaySlingshotLaunchConfig` is the preferred name for gameplay-owned slingshot launch tuning because it makes the ownership explicit: gameplay calculates the final launch impulse, while the Slingshot feature remains a leaf that reports pull data.

Open balancing inputs still needed before production tuning:

- Initial coin prices and max levels for each upgrade.
- Initial effect ranges and curve shapes for each upgrade.
- Final icon art for each upgrade.
- UI copy and formatting strings for each effect type.
- Exact level overrides for any intentionally standout levels.
