# Gameplay

Gameplay covers the runtime concepts that define how a run begins, moves, and ends.

## Language

**Slingshot**:
The launch device that starts a run.
_Avoid_: Launcher, rope input

**Run**:
A gameplay attempt that begins with **Launch** and ends by crash, lost momentum, or reaching the level end.
_Avoid_: Session, attempt

**Level Session**:
The currently loaded level instance that can contain multiple **Runs**.
_Avoid_: App session, Run

**Run End Reason**:
The single cause that ended a **Run**.
_Avoid_: Game over type, fail reason, death reason

**Run Result**:
The immutable summary captured when a **Run** ends.
_Avoid_: Score state, end payload, outcome data

**Run Progress Frame**:
The course reference that defines forward downhill progress, lateral direction, and up direction during a **Run**.
_Avoid_: World forward, launch direction, camera forward

**Run End Flow**:
The gameplay flow that accepts run-ending signals and produces one **Run Result**.
_Avoid_: Game over manager, result controller, death handler

**Run Contact Category**:
The gameplay meaning of a contact or region encountered during a **Run**.
_Avoid_: Collider type, physics layer, tag

**Run Surface**:
A **Run Contact Category** that allows the **Launch Target** to slide, jump, or redirect without ending the **Run**.
_Avoid_: Ground, floor, ramp

**Run Obstacle**:
A **Run Contact Category** that can end a **Run** by `ObstacleHit`.
_Avoid_: Tree collider, wall tag

**Obstacle Impact**:
A **Run Obstacle** contact strong enough to end a **Run** by `ObstacleHit`.
_Avoid_: Any collision, speed drop

**Lost Momentum**:
A **Run End Reason** caused by sustained lack of meaningful **Run** progress.
_Avoid_: Stopped, idle, one-frame low speed

**Run Safety Net**:
A **Run Contact Category** that ends a **Run** when the **Launch Target** falls below the playable course.
_Avoid_: Boundary, kill plane, infinite fall detector

**Run Finish**:
A **Run Contact Category** that ends a **Run** by `Finished`.
_Avoid_: Goal trigger, finish line

**Currency Definition**:
The authored identity of a spendable currency balance bucket that pickups can grant and upgrades can spend.
_Avoid_: Resource Definition, SKU, currency enum, Coin Definition

**Pickup Definition**:
The authored collection definition for a **Pickup**, including its **Currency Grant**.
_Avoid_: Coin Definition, prefab value

**Currency Grant**:
The authored positive amount of one **Currency Definition** granted by gameplay content.
_Avoid_: Currency Reward, Currency Modifier, Currency Change

**Final Pickup Grant Amount**:
The integer amount actually added to **Currency Storage** and **Run Currency Accumulator** for one accepted **Pickup**.
_Avoid_: base grant, preview amount, fractional carry

**Pickup**:
The scene object collected during a **Run** when touched by a collider with the configured **Player Tag**.
_Avoid_: Coin Pickup, collectible

**Pickup Collection Event**:
The notification that one **Pickup** was accepted and its **Final Pickup Grant Amount** was applied.
_Avoid_: Coin collected event, feedback trigger

**Player Tag**:
The Unity tag applied to every collider GameObject that may collect **Pickups**.
_Avoid_: LaunchTarget tag, PickupCollector, layer-as-identity

**Player Layer**:
The Unity physics layer assigned to player colliders that may overlap **Pickup** triggers.
_Avoid_: Player identity, Player Tag

**Pickup Layer**:
The Unity physics layer assigned to **Pickup** trigger colliders for broad-phase filtering.
_Avoid_: Pickup identity, currency type

**Level Pickup State**:
The level-session owner of which **Pickups** are available or consumed.
_Avoid_: Pickup local flag, save registry, scene search

**Currency Balance**:
The current app-session total held for one **Currency Definition**.
_Avoid_: Resource Balance, Coin Balance, run score

**Currency Storage**:
The gameplay boundary that owns app-session **Currency Balances** for granted and spent **Currency Definitions**.
_Avoid_: Resource Storage, wallet, Resources folder, coin save

**Economy**:
The gameplay area that owns shared currency-balance and upgrade-purchase concepts.
_Avoid_: Resources, wallet feature, pickup collection

**Run Currency Accumulator**:
The run-scoped holder that accumulates accepted **Currency Grant** amounts during the current **Run**.
_Avoid_: Run Resource Collection, Run Resource Tally, CurrencyStorage

**Run Currency Snapshot**:
The immutable final currency amounts earned during one ended **Run**.
_Avoid_: Live accumulator view, CurrencyStorage

**Run Camera**:
The camera behavior that follows the controlled **Launch Target** during a **Run**.
_Avoid_: Player camera, target camera, follow camera

**Run Camera Anchor**:
The camera-facing reference point derived from the **Launch Target** for **Run Camera** framing during a **Run**.
_Avoid_: Camera target, camera pivot, follow target

**Pre-Launch Camera**:
The camera behavior used during **Pre-Launch** before the **Run Camera** takes over.
_Avoid_: Idle camera, static camera

**Run Preparation**:
The default gameplay state where the player prepares for the next **Run** before the **Slingshot** can accept a **Pull**.
_Avoid_: Shop, upgrade state, lobby

**Continue**:
The player confirmation action in **Run Preparation** that accepts the prepared upgrades and proceeds toward **Pre-Launch**.
_Avoid_: Start Run, Launch, Play

**Upgrade**:
A player-owned progression item that improves one gameplay-facing stat across future **Runs**.
_Avoid_: Temporary boost, run purchase, modifier

**Upgrade Id**:
The authored identity used to refer to a purchased **Upgrade** without hardcoding it.
_Avoid_: Gameplay Stat Id, display name, asset name

**Upgrade Level**:
The owned rank of an **Upgrade**.
_Avoid_: Modifier value, temporary stack

**Upgrade Progress Storage**:
The injected boundary used by plain C# upgrade services to read and update app-session owned **Upgrade Levels** by **Upgrade Id**.
_Avoid_: Upgrade Preview, save format, UI model

**Upgrade Definition**:
The authored configuration that describes one **Upgrade** and how its cost and effect scale by **Upgrade Level**.
_Avoid_: Upgrade save data, runtime modifier

**Upgrade Catalog**:
The authored ScriptableObject asset that owns the ordered set of **Upgrade Definitions** and their **Purchase Currency** for **Run Preparation**.
_Avoid_: UI list, scene lookup, wallet state

**Purchase Currency**:
The **Currency Definition** used by an **Upgrade Catalog** to price **Upgrade** purchases.
_Avoid_: per-upgrade wallet, SKU, shop product

**Upgrade Authoring Validation**:
The checks that detect invalid **Upgrade Catalog** and **Upgrade Definition** content before gameplay services are used.
_Avoid_: Runtime balancing, purchase validation

**Upgrade Progression**:
The authored curve-plus-overrides rule that maps an **Upgrade Level** to a cost or effect value.
_Avoid_: Hardcoded formula, mandatory level table

**Upgrade Purchase Service**:
The application boundary that atomically spends a **Currency Balance** and increases an owned **Upgrade Level**.
_Avoid_: UI purchase logic, wallet helper, direct UI storage mutation

**Upgrade Purchase Result**:
The immutable outcome returned by **Upgrade Purchase Service** after a purchase attempt.
_Avoid_: Boolean result, UI state, exception control flow

**Upgrade Preview**:
The immutable read model shown for one **Upgrade** during **Run Preparation**.
_Avoid_: Upgrade Definition, Upgrade save data, UI state

**Modifier Source**:
An owned or temporary cause that contributes one or more **Runtime Modifiers**.
_Avoid_: Upgrade

**Live Modifier Source**:
A **Modifier Source** evaluated from current runtime conditions instead of frozen into the **Run Modifier Snapshot**.
_Avoid_: Upgrade level, snapshot mutation

**Runtime Modifier**:
A resolved gameplay stat adjustment applied for a specific calculation or interval.
_Avoid_: Upgrade, upgrade level

**Modifier Operation**:
The authored math operation used by a **Runtime Modifier** when resolving a gameplay stat.
_Avoid_: Formula script, source order

**Run Modifier Snapshot**:
The frozen set of **Runtime Modifiers** resolved from owned **Upgrade Levels** when leaving **Run Preparation** for **Pre-Launch**.
_Avoid_: Live upgrade state, mutable upgrade config

**Active Run Modifier Snapshot**:
The current **Run Modifier Snapshot** stored after **Continue** and used until the following **Run** ends.
_Avoid_: Upgrade storage, snapshot factory, live modifier source

**Run Modifier Snapshot Factory**:
The plain C# boundary that builds a **Run Modifier Snapshot** from owned **Upgrade Levels** and authored **Upgrade Definitions**.
_Avoid_: Gameplay Flow, UI builder, purchase service

**Gameplay Stat Resolver**:
The deterministic boundary that resolves a final gameplay stat value from a base value and active **Runtime Modifiers**.
_Avoid_: Upgrade calculator, stat service locator

**Gameplay Stat Id**:
The authored identity used to refer to a gameplay stat without hardcoding it.
_Avoid_: Enum value, string key, property name

**Coin Pickup Reward**:
The final integer coin amount accepted from a **Pickup** during a **Run** after `CoinPickupMultiplier` is resolved.
_Avoid_: Collectible Reward, mission reward, ad reward, purchase grant

**Coin Pickup Fractional Carry**:
The run-scoped fractional remainder kept while resolving `CoinPickupMultiplier` for coin **Currency Grants** from **Pickups**.
_Avoid_: persistent balance, hidden currency, per-level reward debt

**Pre-Launch**:
The phase before a **Run** begins, when the **Slingshot** can accept a **Pull**.
_Avoid_: Waiting, ready state

**Gameplay State**:
The single current phase of gameplay flow.
_Avoid_: Gameplay tag, flag

**Gameplay State Id**:
The authored identity used to refer to a **Gameplay State** without hardcoding it.
_Avoid_: Enum value, string key

**Gameplay State Transition**:
The authored identity of an allowed change from one **Gameplay State** to another.
_Avoid_: State edge, transition rule

**Gameplay Flow**:
The coordinator that changes the current **Gameplay State** in response to gameplay events.
_Avoid_: Orchestrator, state controller

**Launch Sequence**:
The designer-tweakable presentation workflow around a valid **Launch**.
_Avoid_: Launch logic, state transition

**Unity Input**:
The shared adapter that turns Unity input sources into project input concepts.
_Avoid_: Touch manager, input singleton

**Pointer Input**:
A source-agnostic screen contact or pointer event.
_Avoid_: Touch, mouse event

**Band**:
The draggable elastic segment of a **Slingshot** that the player touches to prepare a launch.
_Avoid_: Rope, string

**Band Touch Target**:
The generous invisible input area used to start a **Pull** on the **Band**.
_Avoid_: Rope collider, hitbox

**Band Shape**:
The taut visual path of the **Band** from anchor to anchor while idle, pulled, or recoiling.
_Avoid_: Rope physics, band simulation

**Band Release Recoil**:
The post-shot visual phase where the **Band** follows the moving **Launch Target** contact points while returning from the loaded **Band Shape** to
the rest/idle/default shape. When the **Band** reaches that rest shape, it detaches and stops following the **Launch Target**.
_Avoid_: Snap back, leash

**Band Wrap**:
The visible part of a **Band Shape** that follows the held **Launch Target** silhouette between **Band Contact Points**.
_Avoid_: Rope wrap, physics wrap

**Band Contact Point**:
A visible tangent point where a taut **Band Shape** meets the held **Launch Target**.
_Avoid_: Pull point, middle point

**Pulled Side**:
The side of the **Launch Target Silhouette** facing the backward direction of the current **Active Pull**, including lateral **Pull Offset**.
_Avoid_: Nearest side, shortest side

**Pulled-Side Center**:
The central point of a **Band Wrap** on the **Pulled Side** of the current **Launch Target Silhouette**.
_Avoid_: Wrap midpoint, contact midpoint

**Rest Point**:
The **Slingshot**-owned position where the **Band** and held **Launch Target** return when no **Active Pull** is loaded.
On a valid **Launch**, the **Launch Target** launches from the loaded **Pull Point** first; the **Band** returns toward the **Rest Point** only as
post-shot **Band Release Recoil**. During that recoil, the **Band** keeps following the moving **Launch Target** contact points until it reaches
the rest/idle/default shape, then detaches.
_Avoid_: Player initial position, spawn point

**Touch Indicator**:
The visual marker that follows the captured touch during an **Active Pull**.
_Avoid_: Pointer, cursor

**Pull Hint**:
The idle tutorial visual that suggests starting a **Pull** on the **Band**.
_Avoid_: Pointer, nudge

**Pull**:
A touch gesture that begins on the **Band** and draws it backward before release.
_Avoid_: Drag, stretch

**Pull Strength**:
The **Slingshot**-owned scalar describing how strongly a **Pull** was drawn backward.
_Avoid_: SlingshotLaunchPower, Launch Impulse, upgrade value

**Pull Release**:
The input moment when an **Active Pull** ends before it is judged as a **Launch**.
_Avoid_: Launch, fire

**Active Pull**:
The single **Pull** currently owned by one captured touch while the **Launch Target** is held.
_Avoid_: Current drag, selected finger

**Pull Offset**:
The lateral displacement of a **Pull** from the **Band** center.
_Avoid_: Aim offset, side drag

**Pull Point**:
The interpreted **Pull Plane** position that holds the **Launch Target** during an **Active Pull**.
_Avoid_: Band middle, contact point

**Launch Frame**:
The **Slingshot**-owned orthonormal orientation that defines forward, backward, lateral, and up directions for a **Pull** and **Launch**.
_Avoid_: Camera forward, world forward

**Pull Plane**:
The **Slingshot**-owned plane where touch positions are interpreted as backward and lateral **Pull** movement.
_Avoid_: Ground raycast, surface input

**Launch**:
The transition from a released **Pull** into forward motion for the **Launch Target**.
_Avoid_: Fire, shoot

**Launch Impulse**:
The resolved immutable gameplay-owned physical push value for the **Launch Target** after a valid **Launch** is accepted.
_Avoid_: Pull power, band force, Slingshot calculation

**Gameplay Slingshot Launch Config**:
The gameplay-owned authored ScriptableObject config used to turn **Pull Strength** and **Pull Offset** from a **Slingshot** into a **Launch Impulse**.
_Avoid_: SlingshotConfig, Band tuning, pull interpretation

**Launch Impulse Calculator**:
The gameplay-owned boundary that resolves a **Launch Impulse** from **Pull Strength**, **Pull Offset**, **Gameplay Slingshot Launch Config**, and active
gameplay stat rules.
_Avoid_: Slingshot, physics applier, upgrade storage

**Launch Impulse Application**:
The boundary that applies a resolved **Launch Impulse** to the **Launch Target**.
_Avoid_: Launch Impulse Calculator, upgrade resolution, pull interpretation

**Launch Target**:
The scene object that is held by an **Active Pull**, receives launch energy from a **Slingshot**, and is controlled during a **Run**.
_Avoid_: Player, projectile, payload

**Launch Target Silhouette**:
The inflated convex band-height **Pull Plane** outline of the held **Launch Target** used by a taut **Band Shape**.
_Avoid_: Collider mesh, 3D wrap shape, concave collider outline

## Relationships

- A **Slingshot** has one **Band**.
- A **Slingshot** has one **Rest Point**.
- A **Slingshot** is a feature leaf.
- A **Slingshot** reports interpreted **Pull** data to gameplay.
- A **Slingshot** reports **Pull Strength** to gameplay.
- A **Slingshot** does not know about **Upgrades**, **Gameplay Stat Resolver**, **Gameplay Flow**, or higher gameplay systems.
- A **Slingshot** does not calculate the final **Launch Impulse**.
- A **Slingshot** does not own **Gameplay Slingshot Launch Config**.
- **Slingshot**-local tuning controls **Pull** interpretation and **Band** presentation.
- **Pull Strength** is not `SlingshotLaunchPower`.
- **Pull Strength** does not include **Upgrades** or **Runtime Modifiers**.
- **Gameplay Slingshot Launch Config** controls the base **Launch Impulse** shape before gameplay stat modifiers are applied.
- `GameplaySlingshotLaunchConfig` is the code-facing name for **Gameplay Slingshot Launch Config**.
- **GameplayLifetimeScope** references **Gameplay Slingshot Launch Config** and provides it to **Launch Impulse Calculator**.
- **Gameplay Slingshot Launch Config** does not own **Upgrade Levels**, **Runtime Modifiers**, **Gameplay Stat Resolver**, or **Launch Impulse Application**.
- **Launch Impulse Calculator** calculates the final **Launch Impulse** from **Pull Strength**, **Pull Offset**, **Gameplay Slingshot Launch Config**,
  and active gameplay stat rules.
- **Launch Impulse Calculator** returns a resolved **Launch Impulse**.
- **Launch Impulse Calculator** uses **Gameplay Stat Resolver** for `SlingshotLaunchPower`.
- **Launch Impulse Calculator** does not apply the **Launch Impulse** to the **Launch Target**.
- **Launch Impulse** does not apply itself to the **Launch Target**.
- `LaunchImpulse` is the code-facing name for **Launch Impulse**.
- **Launch Impulse Application** applies the already-resolved **Launch Impulse** to the **Launch Target**.
- **Launch Impulse Application** translates **Launch Impulse** into physics application.
- **Launch Impulse Application** does not interpret **Pulls**, evaluate **Upgrades**, or resolve **Runtime Modifiers**.
- A **Band** has one **Band Touch Target**.
- A **Band** has one visible **Band Shape**.
- A **Band Shape** is a taut visual path, not gameplay launch math.
- A **Band Shape** may have one **Band Wrap**.
- A **Band Shape** may have **Band Contact Points**.
- A **Band Wrap** is bounded by **Band Contact Points**.
- A **Band Wrap** follows the held **Launch Target** silhouette.
- A rest/idle **Band Shape** passes through the **Rest Point**.
- **Run Preparation** is the default current **Gameplay State** when the game launches.
- **Run Preparation** occurs before **Pre-Launch**.
- **Continue** occurs during **Run Preparation**.
- **Continue** creates the **Run Modifier Snapshot** before **Gameplay Flow** enters **Pre-Launch**.
- **Continue** does not begin a **Run**.
- A player may buy **Upgrades** during **Run Preparation**.
- An **Upgrade** has one **Upgrade Definition**.
- An **Upgrade Definition** has one **Upgrade Id**.
- An **Upgrade Id** identifies purchase and ownership state.
- An **Upgrade Id** is separate from **Gameplay Stat Id**.
- An **Upgrade Definition** owns the player-facing icon for its **Upgrade**.
- An **Upgrade Catalog** owns the ordered list of **Upgrade Definitions** shown during **Run Preparation**.
- An **Upgrade Catalog** has one **Purchase Currency**.
- An **Upgrade Definition** cost **Upgrade Progression** evaluates amounts in the **Upgrade Catalog** **Purchase Currency**.
- An **Upgrade Catalog** does not own **Currency Balances** or **Upgrade Levels**.
- **GameplayLifetimeScope** references the **Upgrade Catalog** and registers it for plain C# upgrade services.
- **Upgrade Authoring Validation** checks **Upgrade Catalogs** and **Upgrade Definitions**.
- **Upgrade Authoring Validation** reports duplicate **Upgrade Ids**, null **Upgrade Definitions**, missing icons, missing **Gameplay Stat Ids**,
  invalid max levels, invalid **Upgrade Progressions**, and invalid **Modifier Operations**.
- **Upgrade Authoring Validation** may surface editor warnings from `OnValidate`.
- **GameplayLifetimeScope** or an upgrade installer fails composition when **Upgrade Authoring Validation** reports errors.
- A player may own **Upgrade Levels** for **Upgrades**.
- **Upgrade Level** `0` means the baseline owned state with no purchased upgrade level.
- An owned **Upgrade Level** is clamped from `0` through the **Upgrade Definition** maximum level.
- An **Upgrade** is maxed when its owned **Upgrade Level** is greater than or equal to its **Upgrade Definition** maximum level.
- The current **Upgrade** effect is evaluated at the current **Upgrade Level**.
- The next purchase cost is evaluated for the current **Upgrade Level** plus one.
- A maxed **Upgrade** has no next purchase cost.
- **Upgrade Progress Storage** provides owned **Upgrade Levels** to **Upgrade Preview** builders, **Upgrade Purchase Service**, and **Run Modifier Snapshot** creation.
- Owned **Upgrade Levels** span multiple **Runs** during the current app session.
- Owned **Upgrade Levels** reset when the app session ends in the first upgrade slice.
- **Upgrade Progress Storage** does not define the save format.
- `IUpgradeProgressStorage` is the code-facing name for **Upgrade Progress Storage**.
- An **Upgrade Definition** has one cost **Upgrade Progression**.
- An **Upgrade Definition** has one effect **Upgrade Progression**.
- An **Upgrade Progression** uses a broad curve and may have exact per-level overrides.
- An **Upgrade Progression** evaluates a requested **Upgrade Level** against the **Upgrade Definition** maximum level.
- An exact per-level **Upgrade Progression** override wins over curve evaluation.
- When no exact override exists, **Upgrade Progression** normalizes the requested **Upgrade Level** against the maximum level, evaluates the authored
  curve, maps that result between min and max, applies rounding or step policy, then clamps.
- Cost **Upgrade Progressions** are queried for purchase levels from `1` through the maximum level.
- Effect **Upgrade Progressions** are queried for levels from `0` through the maximum level.
- A multiplicative level `0` effect may be authored or defaulted to neutral, such as `x1`.
- An **Upgrade Preview** is built from an **Upgrade Definition**, owned **Upgrade Level**, the **Currency Balance** for the **Purchase Currency**, and
  **Upgrade Progressions**.
- An **Upgrade Preview** may include icon, display name, current level, next cost, current effect, next effect, affordability, and maxed state.
- An **Upgrade Preview** shows next effect from the current **Upgrade Level** plus one when the **Upgrade** is not maxed.
- **Upgrade Purchase Service** checks the cost, spends a **Currency Balance** in the **Purchase Currency**, and increments an **Upgrade Level** as one
  operation.
- **Upgrade Purchase Service** is the only **Run Preparation** purchase mutator of `IUpgradeProgressStorage`.
- **Upgrade Purchase Service** depends on **Currency Storage** for balance and spend operations.
- `ICurrencyStorage` is the code-facing name for **Currency Storage**.
- `Game.Gameplay.Economy` is the code-facing namespace and assembly name for shared **Economy** code.
- **Economy** owns **Currency Definition**, **Currency Grant**, **Currency Balance**, **Currency Storage**, **Run Currency Accumulator**, and **Run Currency Snapshot**.
- **Currency Storage** grants, reads, and spends **Currency Balances** by **Currency Definition**.
- **Currency Balances** span multiple **Runs** during the current app session.
- **Currency Balances** reset when the app session ends in the first upgrade slice.
- App-restart persistence is outside **Currency Storage** and **Upgrade Progress Storage** in the first upgrade slice.
- `ICurrencyStorage` exposes `GetAmount`, `Grant`, and `TrySpend`.
- `TrySpend` returns false only when a **Currency Balance** is insufficient.
- Invalid **Currency Definitions** and non-positive currency amounts are invalid storage usage, not gameplay purchase results.
- **Upgrade Purchase Service** returns one **Upgrade Purchase Result** for each purchase attempt.
- **Upgrade Purchase Result** may report `Success`, `MaxLevel`, `InsufficientCurrency`, `MissingDefinition`, or `InvalidDefinition`.
- **Upgrade Purchase Service** maps failed `TrySpend` calls to `InsufficientCurrency`.
- **Upgrade Purchase Service** reports purchase failure without partial **Currency Balance** or **Upgrade Level** changes.
- **Run Preparation** UI renders **Upgrade Previews** and sends purchase or **Continue** commands.
- **Run Preparation** UI does not evaluate **Upgrade Progressions** or mutate **Upgrade Levels** directly.
- An **Upgrade Level** may contribute one or more **Runtime Modifiers**.
- A **Modifier Source** may contribute one or more **Runtime Modifiers**.
- A **Live Modifier Source** may contribute **Runtime Modifiers** after the **Run Modifier Snapshot** is created.
- A **Live Modifier Source** may start, stop, or expire during **Running**.
- A **Live Modifier Source** does not mutate **Upgrade Levels** or the **Run Modifier Snapshot**.
- A **Live Modifier Source** may return no **Runtime Modifiers** while inactive.
- **Live Modifier Sources** are provided to **Gameplay Stat Resolver** through explicit composition.
- A **Runtime Modifier** may affect **Launch**, movement during a **Run**, or rewards from a **Run**.
- A **Runtime Modifier** has one **Modifier Operation**.
- A **Run Modifier Snapshot** is resolved when **Gameplay Flow** changes from **Run Preparation** to **Pre-Launch**.
- A **Run Modifier Snapshot** remains stable through **Pre-Launch** and the following **Run**.
- **Active Run Modifier Snapshot** is set when **Continue** is accepted.
- **Active Run Modifier Snapshot** is cleared or replaced when returning to **Run Preparation**.
- `IRunModifierSnapshotProvider` is the read-facing code name for accessing the **Active Run Modifier Snapshot**.
- Write access to **Active Run Modifier Snapshot** belongs to gameplay flow or **Run Preparation** transition code.
- **Run Modifier Snapshot Factory** builds the **Run Modifier Snapshot** when **Continue** is accepted.
- **Run Modifier Snapshot Factory** reads **Upgrade Catalog** and `IUpgradeProgressStorage`.
- **Run Modifier Snapshot Factory** evaluates owned **Upgrade Levels** through **Upgrade Definitions** and **Upgrade Progressions**.
- **Run Modifier Snapshot Factory** does not read **Currency Balances**, mutate **Upgrade Levels**, or know about UI.
- **Gameplay Stat Resolver** combines a base value with **Runtime Modifiers** from the **Run Modifier Snapshot** and active **Live Modifier Sources**.
- **Gameplay Stat Resolver** receives the base value from the gameplay system requesting a stat resolution.
- **Gameplay Stat Resolver** reads the **Active Run Modifier Snapshot** through `IRunModifierSnapshotProvider`.
- **Gameplay Stat Resolver** does not find **Live Modifier Sources** through scene searches or static registries.
- **Gameplay Stat Resolver** resolves values for one **Gameplay Stat Id** at a time.
- **Gameplay Stat Resolver** applies **Modifier Operations** in this fixed order: `FlatAdd`, `AdditivePercent`, `MultiplicativeFactor`,
  `ClampMin`, then `ClampMax`.
- **Gameplay Stat Resolver** does not read **Slingshot**, movement, reward, or economy configs to discover base values.
- **Gameplay Stat Resolver** does not own **Upgrade Levels**, economy state, **Slingshot**, movement, or reward behavior.
- The first four **Upgrade Definitions** use `MultiplicativeFactor` as their default **Modifier Operation**.
- A multiplicative **Upgrade** effect at **Upgrade Level** `0` is neutral, such as `x1`.
- `SlingshotLaunchPower`, `PlayerMaxSpeed`, `PlayerSteeringResponsiveness`, and `CoinPickupMultiplier` initially use `MultiplicativeFactor`.
- **Upgrade Definition** formatting metadata decides whether multiplicative effects appear as `xN` or equivalent percentage labels.
- An **Upgrade** targets one **Gameplay Stat Id**.
- A **Runtime Modifier** targets one **Gameplay Stat Id**.
- Authored upgrade-facing **Gameplay Stat Ids** include `SlingshotLaunchPower`, `PlayerMaxSpeed`, `PlayerSteeringResponsiveness`, and
  `CoinPickupMultiplier`.
- `SlingshotLaunchPower` modifies gameplay's **Launch Impulse** calculation after **Slingshot** has interpreted the **Pull**.
- `SlingshotLaunchPower` does not modify **Pull Strength**.
- `PlayerMaxSpeed` affects player movement during **Running**.
- `PlayerSteeringResponsiveness` affects player steering during **Running**.
- `CoinPickupMultiplier` affects **Coin Pickup Reward** amounts only.
- `CoinPickupMultiplier` is resolved before coin pickup amounts are written to **Currency Storage** and **Run Currency Accumulator**.
- `CoinPickupMultiplier` uses **Coin Pickup Fractional Carry** for fractional multiplied results.
- **Coin Pickup Reward** is the floored whole-number result of the base coin **Currency Grant** times `CoinPickupMultiplier` plus current
  **Coin Pickup Fractional Carry**.
- The remaining fractional part becomes the next **Coin Pickup Fractional Carry**.
- **Coin Pickup Fractional Carry** resets with **Run Currency Accumulator** for each **Run**.
- **Coin Pickup Fractional Carry** does not persist across **Runs** or into **Currency Storage**.
- **Band Contact Points** are tangent points from **Slingshot** anchors to the **Launch Target Silhouette**.
- A taut **Band Shape** does not pass through the **Launch Target Silhouette**.
- During an **Active Pull**, a **Band Wrap** follows the **Pulled Side** of the current **Launch Target Silhouette**.
- A **Band Wrap** curves through the **Pulled-Side Center**.
- A **Band Wrap** follows the **Launch Target Silhouette** contour between **Band Contact Points** that contains the **Pulled-Side Center**.
- **Pulled-Side Center** follows the actual **Active Pull** direction, including **Pull Offset**.
- A **Slingshot** may show one **Pull Hint** while idle.
- A **Slingshot** accepts a **Pull** during **Pre-Launch**.
- A **Run** is governed by one current **Gameplay State**.
- A **Run** has one **Run Progress Frame**.
- A **Run** ends with one **Run End Reason**.
- A **Run Result** summarizes one ended **Run**.
- A **Run Result** has one **Run End Reason**.
- A **Run Result** has one **Run Currency Snapshot**.
- A **Run Result** uses the **Run Progress Frame** to describe forward downhill progress.
- **Run End Flow** produces one **Run Result** for an ended **Run**.
- A **Run Contact Category** determines how a **Run** contact or region affects the **Run**.
- A **Run Surface** does not end a **Run**.
- A **Run Obstacle** may end a **Run** with `ObstacleHit`.
- An **Obstacle Impact** ends a **Run** with `ObstacleHit`.
- **Lost Momentum** ends a **Run** with `LostMomentum`.
- **Lost Momentum** is measured against the **Run Progress Frame**.
- A **Run Safety Net** ends a **Run** with `OutOfBounds`.
- A **Run Finish** ends a **Run** with `Finished`.
- A **Level Session** may contain multiple **Runs**.
- A **Level Session** has one **Level Pickup State**.
- A **Level Pickup State** resets when a **Level Session** starts.
- A **Pickup Definition** has one **Currency Grant**.
- A **Pickup Definition** belongs to pickup collection, not **Economy**.
- A **Currency Grant** references one **Currency Definition**.
- A **Currency Grant** has a positive amount.
- A **Currency Grant** amount is the base amount before runtime reward modifiers.
- A **Pickup Definition** without a **Currency Grant** is invalid authoring.
- A **Pickup** has one **Pickup Definition**.
- A **Pickup** without a **Pickup Definition** is invalid authoring.
- A **Pickup** uses a trigger collider for collection.
- A **Pickup** trigger collider uses the **Pickup Layer**.
- A **Pickup** collection collider that is not a trigger is invalid authoring.
- A **Pickup** collection collider outside the **Pickup Layer** is invalid authoring.
- A **Pickup** forwards Unity trigger-enter contacts; it does not decide collectability or grant currency.
- A **Pickup** publishes trigger-enter contacts for the **Pickup Collection Controller** to interpret.
- A **Pickup** trigger contact passes the contacted Unity `Collider`.
- A **Pickup** does not use trigger-stay polling for collection.
- A **Pickup Collection Controller** subscribes to **Pickup** trigger contacts from explicit scene references.
- A **Pickup Collection Controller** accepts only contacted colliders with the configured **Player Tag**.
- A **Pickup Collection Controller** compares the configured **Player Tag** with Unity `CompareTag`.
- Physics layers filter pickup/player trigger overlap before **Player Tag** identity is checked.
- The **Pickup Layer** overlaps the **Player Layer** for collection.
- The **Pickup Layer** avoids unnecessary overlaps with non-player layers.
- The **Player Layer** keeps required world and run-contact interactions.
- A collider can collect a **Pickup** only if that collider's own GameObject is on the **Player Layer**.
- A collider can collect a **Pickup** only if that collider's own GameObject has the configured **Player Tag**.
- A collecting collider without both the **Player Layer** and configured **Player Tag** is invalid authoring.
- Player pickup-contact colliders are serialized explicitly for setup validation.
- Player pickup-contact collider references are manually assigned in the first slice.
- Serialized player pickup-contact colliders are not a collector mechanism.
- A player root may also have the configured **Player Tag**, but root tagging does not make untagged child colliders collect **Pickups**.
- Invalid pickup collection authoring fails fast instead of silently skipping **Pickups** or granting fallback currency.
- Pickup collection setup is validated during runtime composition before the first **Run**, not lazily on first trigger contact.
- A **Level Pickup State** tracks **Pickups** from explicit scene references.
- A **Level Pickup State** tracks each **Pickup** at most once.
- Duplicate explicit **Pickup** references are invalid authoring.
- A **Level Pickup State** determines whether a **Pickup** is available or consumed.
- A **Pickup** applies availability by enabling or disabling its scene root GameObject.
- A consumed **Pickup** disables its scene root GameObject.
- A **Pickup** is marked consumed before currency totals are mutated.
- A **Pickup** may increase a **Currency Balance** during a **Run**.
- A **Pickup** may increase the **Run Currency Accumulator** during a **Run**.
- A **Pickup** may increase a **Currency Balance** at most once per **Level Session**.
- A **Pickup Collection Event** follows an accepted **Currency Grant**.
- A **Pickup Collection Event** identifies the **Pickup**, **Currency Definition**, base **Currency Grant** amount, **Final Pickup Grant Amount**, and
  collection position.
- A **Currency Balance** belongs to one **Currency Definition**.
- A **Currency Balance** spans multiple **Runs** during the current app session.
- A **Currency Balance** resets when the app session ends in the first upgrade slice.
- A **Run Currency Accumulator** is keyed by **Currency Definition**.
- A **Run Currency Accumulator** resets when gameplay enters **Pre-Launch** for the next **Run**.
- A **Run Currency Accumulator** produces a **Run Currency Snapshot** when a **Run** ends.
- A **Run Currency Snapshot** contains final earned currency collected during its **Run**.
- A **Run Currency Snapshot** does not include base **Currency Grant** totals separately.
- A **Run Currency Snapshot** does not include **Coin Pickup Fractional Carry**.
- A missing **Currency Definition** in a **Run Currency Snapshot** means zero collected for that **Run**.
- A **Run Currency Snapshot** is read by end-of-run UI.
- Pickup collection depends on **Economy** for currency concepts.
- **Economy** does not depend on pickup collection.
- During a **Run**, the user controls the **Launch Target**.
- A **Run Camera** follows the **Launch Target** during a **Run**.
- A **Run Camera** uses one **Run Camera Anchor** to frame the **Launch Target**.
- A **Run Camera Anchor** is derived from the **Launch Target**.
- A **Pre-Launch Camera** frames the **Slingshot** before a **Run**.
- A **Run Camera** takes over from a **Pre-Launch Camera** after **Launch**.
- A **Gameplay State Id** identifies one **Gameplay State**.
- A **Gameplay State Transition** identifies one allowed change between two **Gameplay States**.
- **Gameplay Flow** changes the current **Gameplay State**.
- A **Launch Sequence** presents a valid **Launch**.
- **Unity Input** produces **Pointer Input**.
- A **Slingshot** consumes **Pointer Input** during **Pre-Launch**.
- A **Pull** belongs to one active **Slingshot**.
- A **Pull Release** may become a **Launch**.
- A **Slingshot** may have one **Active Pull**.
- An **Active Pull** may show one **Touch Indicator**.
- An **Active Pull** may move one held **Launch Target** during **Pre-Launch**.
- An **Active Pull** has one **Pull Point**.
- An ended **Active Pull** may return a held **Launch Target** to the **Rest Point**.
- An accepted **Pull Release** keeps the **Band Shape** loaded through launch handoff, then enters **Band Release Recoil**.
- During **Band Release Recoil**, the **Band** updates **Band Contact Points** and **Band Wrap** from the current **Launch Target Silhouette** until
  it reaches the rest/idle/default shape; after that, it detaches and must not keep chasing the **Launch Target**.
- A **Pull** may have a **Pull Offset**.
- A **Slingshot** owns one **Launch Frame**.
- A **Launch Frame** has perpendicular unit right, forward, and up axes.
- A **Slingshot** owns one **Pull Plane**.
- A **Launch** affects one **Launch Target**.
- A **Launch Target** has one **Launch Target Silhouette** for the current **Pull Plane**.
- A taut **Band Shape** wraps around the **Launch Target Silhouette**.
- A **Launch Target Silhouette** is an outer outline; a taut **Band Shape** bridges over concave details.

## Example dialogue

> **Dev:** "Can the **Launch Target** move before the **Pull** is released?"
> **Domain expert:** "Yes — during an **Active Pull**, the held **Launch Target** follows the interpreted **Pull Point**, but the **Run** still begins only on **Launch**."

> **Dev:** "Where does the held **Launch Target** return after a weak or canceled **Pull**?"
> **Domain expert:** "To the **Rest Point**, because the **Band** and held **Launch Target** share the same unloaded position."

> **Dev:** "Does a valid **Pull Release** reset the **Band** before the **Launch** is applied?"
> **Domain expert:** "No — the loaded **Band Shape** stays through launch handoff; after the shot, the **Band** moves back toward the **Rest Point** alongside the **Launch Target** leaving the **Slingshot**."

> **Dev:** "During post-shot recoil, does the **Band** keep wrapping around the launched **Launch Target**?"
> **Domain expert:** "Yes, briefly — after the shot, the **Band** follows the moving **Launch Target** contact points during **Band Release Recoil** so it feels like the **Band** pushed the target forward. Once the **Band** reaches its rest/idle/default shape, it detaches and stops following."

> **Dev:** "Can the **Slingshot** accept another **Pull** after **Launch**?"
> **Domain expert:** "No — after **Launch**, the **Run** has started and **Pre-Launch** is over."

> **Dev:** "Is the thing that stops the **Run** the same as the score shown after it?"
> **Domain expert:** "No — **Run End Reason** says why the **Run** ended, while **Run Result** is the summary captured for the ended **Run**."

> **Dev:** "Can a finish trigger and obstacle contact both publish separate results?"
> **Domain expert:** "No — **Run End Flow** accepts one run-ending signal and produces one **Run Result**."

> **Dev:** "Does every collider hit during a **Run** end the **Run**?"
> **Domain expert:** "No — **Run Surface** contacts are traversal, while **Run Obstacle** and **Run Finish** contacts can end the **Run**."

> **Dev:** "Does any touch against a **Run Obstacle** end the **Run**?"
> **Domain expert:** "No — only an **Obstacle Impact** ends the **Run** by `ObstacleHit`; light obstacle contact can be bearable."

> **Dev:** "Does the **Run** end the moment the **Launch Target** has one low-speed frame?"
> **Domain expert:** "No — `LostMomentum` means **Lost Momentum**, a sustained lack of meaningful **Run** progress."

> **Dev:** "What catches the **Launch Target** if it falls below the course?"
> **Domain expert:** "A **Run Safety Net** ends the **Run** by `OutOfBounds`."

> **Dev:** "Is forward progress the same as launch direction?"
> **Domain expert:** "No — **Launch** direction can include **Pull Offset**, while the **Run Progress Frame** defines forward downhill progress."

> **Dev:** "Should the game start in **Pre-Launch**?"
> **Domain expert:** "No — the game starts in **Run Preparation**; **Pre-Launch** begins after the player leaves preparation and the **Slingshot** can accept a **Pull**."

> **Dev:** "Is a temporary speed ramp an **Upgrade**?"
> **Domain expert:** "No — an **Upgrade** is owned run-to-run progression; a ramp is a **Modifier Source** that contributes **Runtime Modifiers**."

> **Dev:** "Do designers need to author every **Upgrade Level** value by hand?"
> **Domain expert:** "No — an **Upgrade Progression** uses a curve for the broad shape and exact overrides only where needed."

> **Dev:** "Does **Upgrade Level** one mean the baseline?"
> **Domain expert:** "No — **Upgrade Level** `0` is the baseline; level one is the first purchased level."

> **Dev:** "Which level does the next purchase cost use?"
> **Domain expert:** "The next purchase cost uses the current **Upgrade Level** plus one, unless the **Upgrade** is maxed."

> **Dev:** "Does a per-level **Upgrade Progression** override still go through rounding?"
> **Domain expert:** "No — the override is the final authored value; validation should catch invalid values."

> **Dev:** "Does cost progression evaluate level zero?"
> **Domain expert:** "No — cost progression is queried for purchase levels one through max level; level zero is baseline."

> **Dev:** "Does a **Gameplay Stat Id** own the UI icon for an **Upgrade**?"
> **Domain expert:** "No — the player-facing icon belongs to the **Upgrade Definition**."

> **Dev:** "Can Run Preparation UI spend coins directly?"
> **Domain expert:** "No — UI requests a purchase from **Upgrade Purchase Service**, which owns the atomic currency-spend-and-level update."

> **Dev:** "Should each **Upgrade Definition** choose its own purchase currency?"
> **Domain expert:** "No — the **Upgrade Catalog** owns the **Purchase Currency**, while **Upgrade Definitions** own amount-only cost progressions."

> **Dev:** "Should insufficient balance be reported by throwing from **Currency Storage**?"
> **Domain expert:** "No — `TrySpend` returns false for insufficient **Currency Balance**, and **Upgrade Purchase Service** maps that to `InsufficientCurrency`."

> **Dev:** "Can buying an **Upgrade** change launch power during an **Active Pull**?"
> **Domain expert:** "No — owned **Upgrade Levels** are resolved into a **Run Modifier Snapshot** before **Pre-Launch**, so pull and launch calculations use stable values."

> **Dev:** "Should **Slingshot** calculate upgrade stacking for launch speed?"
> **Domain expert:** "No — **Slingshot** reports interpreted **Pull** data, while gameplay resolves modifiers and calculates the final **Launch Impulse**."

> **Dev:** "Can later-added **Runtime Modifiers** change the stacking order?"
> **Domain expert:** "No — **Gameplay Stat Resolver** uses fixed **Modifier Operation** order, not source order."

> **Dev:** "Does `SlingshotLaunchPower` change how far the player can pull the **Band**?"
> **Domain expert:** "No — **Slingshot** interprets the **Pull** first; `SlingshotLaunchPower` modifies gameplay's **Launch Impulse** calculation."

> **Dev:** "Is **Pull Strength** the same thing as `SlingshotLaunchPower`?"
> **Domain expert:** "No — **Pull Strength** comes from the **Slingshot** interpreting the **Pull**; `SlingshotLaunchPower` is a gameplay upgrade stat used later."

> **Dev:** "Should **Launch Impulse Calculator** push the **Launch Target**?"
> **Domain expert:** "No — **Launch Impulse Calculator** resolves the value; **Launch Impulse Application** applies it to the **Launch Target**."

> **Dev:** "Should **Launch Impulse Calculator** mutate physics objects directly?"
> **Domain expert:** "No — it returns a resolved **Launch Impulse**; **Launch Impulse Application** translates that value into physics application."

> **Dev:** "Should final launch tuning live in the **Slingshot**?"
> **Domain expert:** "No — **Slingshot** owns **Pull** interpretation and **Band** presentation; **Gameplay Slingshot Launch Config** owns final **Launch Impulse** tuning."

> **Dev:** "Is `GameplaySlingshotLaunchConfig` part of the **Slingshot** feature?"
> **Domain expert:** "No — it is gameplay-owned config for **Slingshot**-derived launch, not **Slingshot** feature config."

> **Dev:** "Should `GameplaySlingshotLaunchConfig` be referenced by **GameplayLifetimeScope**?"
> **Domain expert:** "Yes — it is authored gameplay config used by **Launch Impulse Calculator**, so gameplay composition owns the reference."

> **Dev:** "Should the first four upgrade effects all use the same operation?"
> **Domain expert:** "Yes — use `MultiplicativeFactor` first for `SlingshotLaunchPower`, `PlayerMaxSpeed`, `PlayerSteeringResponsiveness`, and `CoinPickupMultiplier`."

> **Dev:** "Does `PlayerMaxSpeed` change the initial **Launch** velocity?"
> **Domain expert:** "No — `PlayerMaxSpeed` and `PlayerSteeringResponsiveness` affect player movement during **Running**, not **Slingshot** launch math."

> **Dev:** "Should upgrade definitions target raw code properties?"
> **Domain expert:** "No — upgrade definitions target authored **Gameplay Stat Ids** such as `SlingshotLaunchPower` and `PlayerMaxSpeed`."

> **Dev:** "Does `CoinPickupMultiplier` affect mission rewards, ad rewards, or non-coin pickups?"
> **Domain expert:** "No — it affects **Coin Pickup Reward** amounts only."

> **Dev:** "Should `x1.1` on one-coin pickups round every pickup up to two coins?"
> **Domain expert:** "No — **Coin Pickup Reward** uses floor plus **Coin Pickup Fractional Carry**, so fractional value pays out over multiple pickups in the same **Run**."

> **Dev:** "Should a **Run Currency Snapshot** include both base and final pickup amounts?"
> **Domain expert:** "No — **Pickup Collection Event** carries both for per-pickup feedback and diagnostics; **Run Currency Snapshot** stores final earned currency totals only."

> **Dev:** "Do **Currency Balances** and **Upgrade Levels** survive app restart in the first upgrade slice?"
> **Domain expert:** "No — they persist across **Runs** only within the current app session; app-restart persistence is deferred behind storage interfaces."

> **Dev:** "Can **Pre-Launch** and running be active at the same time?"
> **Domain expert:** "No — there is one current **Gameplay State**."

> **Dev:** "Can **Gameplay Flow** switch to any **Gameplay State** asset?"
> **Domain expert:** "No — the change must match an authored **Gameplay State Transition**."

> **Dev:** "Does the **Slingshot** switch the game from **Pre-Launch** to running?"
> **Domain expert:** "No — **Slingshot** reports launch activity, and **Gameplay Flow** changes the **Gameplay State**."

> **Dev:** "Is every **Pull Release** a **Launch**?"
> **Domain expert:** "No — a weak, forward-only, or canceled **Pull Release** does not become a **Launch**."

> **Dev:** "Does the **Launch Sequence** decide whether a **Launch** is allowed?"
> **Domain expert:** "No — **Launch Sequence** presents a valid **Launch** after **Gameplay Flow** accepts it."

> **Dev:** "Does the **Slingshot** care whether input came from touch or mouse?"
> **Domain expert:** "No — **Unity Input** provides **Pointer Input**, and the **Slingshot** interprets that as a **Pull**."

> **Dev:** "Does a **Pull Offset** matter when the **Pull** is released?"
> **Domain expert:** "Yes — it changes the **Launch** direction while the backward part of the **Pull** changes launch strength."

> **Dev:** "Does **Band Shape** decide how much force the **Launch Target** receives?"
> **Domain expert:** "No — **Band Shape** shows the **Pull**, while gameplay launch rules decide the **Launch Impulse**."

> **Dev:** "Can the **Band Shape** use the **Pull Point** as a point inside the **Launch Target**?"
> **Domain expert:** "No — the **Pull Point** is where the held **Launch Target** is positioned; **Band Contact Points** are where the **Band Shape** visually meets it."

> **Dev:** "Does adding more points to the **Band Wrap** change launch power?"
> **Domain expert:** "No — **Band Wrap** is visual detail, while **Launch** rules use **Pull** distance and **Pull Offset**."

> **Dev:** "Does moving the camera change **Pull** direction?"
> **Domain expert:** "No — the **Launch Frame** belongs to the **Slingshot**, not the camera."

> **Dev:** "Does the **Run Camera** follow the player?"
> **Domain expert:** "It follows the **Launch Target** during a **Run**; player is not the current gameplay term for that object."

> **Dev:** "Does the **Run Camera** use the **Launch Target** transform directly?"
> **Domain expert:** "No — it frames the **Launch Target** through a **Run Camera Anchor** so camera framing can stay stable during physics motion."

> **Dev:** "Which camera is active before the **Run** begins?"
> **Domain expert:** "The **Pre-Launch Camera** frames the **Slingshot** until the **Run Camera** takes over after **Launch**."

> **Dev:** "Can two touches create two **Pulls** at the same time?"
> **Domain expert:** "No — a **Slingshot** can have only one **Active Pull**."

> **Dev:** "Does the level **Surface** decide where a **Pull** is measured?"
> **Domain expert:** "No — the **Pull Plane** belongs to the **Slingshot**."

> **Dev:** "Is the hand animation the same thing as the user's touch?"
> **Domain expert:** "No — **Pull Hint** teaches the gesture while **Touch Indicator** follows the real **Active Pull**."

## Flagged ambiguities

- "Rope" was used for the draggable launch element, but the gameplay concept is **Band**; rope may still describe a visual asset if needed.
- "Rope physics" was used for **Band Shape**, but current gameplay treats deformation as visual rather than physical simulation.
- "Natural rubber band behavior" means a taut **Band Shape** around the held **Launch Target**, not runtime rope physics.
- "Pointer" was used for both tutorial and touch feedback; resolved as **Pull Hint** for idle guidance and **Touch Indicator** for live input.
- "State" was used for gameplay phase identity, not tags or simultaneous flags; resolved as one current **Gameplay State**.
- "Touch" and "mouse" were used as input sources, but gameplay consumes **Pointer Input** from **Unity Input**.
- "Before launch movement" refers to held **Launch Target** positioning during **Active Pull**, not the start of a **Run**.
- "Rest position" means **Rest Point** owned by the **Slingshot**, not the **Launch Target** object's initial scene transform.
- "Pull point" and "band middle" were used interchangeably, but the **Pull Point** positions the held **Launch Target** while **Band Contact Points** shape the visible **Band** around it.
- "Closest contact point" is not a **Band Contact Point** unless it is also tangent to the **Launch Target Silhouette** from a **Slingshot** anchor.
- "Resources" is avoided as a feature, folder, assembly, or namespace name because Unity treats `Resources` folders specially.
- "Economy" names the shared gameplay area for currency balances and purchases; **Pickup** collection remains separate and depends on **Economy** for currency concepts.
- "More segments" means visual points in the **Band Wrap**, not physics bodies or gameplay samples.
- "Collider outline" means the **Launch Target Silhouette** in the **Pull Plane**, not the whole 3D collider volume.
- "Silhouette" means the inflated convex outer outline of the **Launch Target** for **Band Shape** purposes, not every concave collider detail.
- "3D wrap" is not a **Band Shape** concern; the **Band** wraps the band-height **Launch Target Silhouette**.
- "Back side" and "pulled side" resolve to **Pulled Side**; do not choose **Band Wrap** by nearest or shortest contour side.
- "Player" and "target" were used ambiguously for the controlled object during a **Run**; resolved as **Launch Target**.
- "Camera target", "camera pivot", and "follow target" resolve to **Run Camera Anchor** when discussing **Run Camera** framing.
- "Idle camera" and "static camera" resolve to **Pre-Launch Camera** when discussing the camera before a **Run**.
- "Stops" resolves to the `LostMomentum` **Run End Reason**, not to arbitrary one-frame low velocity.
- "Ramp" resolves to **Run Surface** for first-slice run-ending logic unless future ramp-specific gameplay events need a separate term.
- "Boundary" is not a first-slice **Run Contact Category**; use **Run Safety Net** for the below-map infinite-fall catch volume.
- "Forward", "downhill", "distance", and "progress" resolve to **Run Progress Frame** when discussing **Run** metrics and momentum.
- "Camera obstacle" is not the same concept as **Run Obstacle**; camera safety and **Run End Reason** use separate meanings.
- Collider shape does not define **Run Contact Category**.
- "Meaningful impact" and "hard collision" resolve to **Obstacle Impact**, not raw speed loss after contact.
