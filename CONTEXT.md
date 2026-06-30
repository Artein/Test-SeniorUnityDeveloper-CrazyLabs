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

**Run Course**:
The authored downhill playable path for one **Run**.
_Avoid_: Scene, level, map

**Run Course Section**:
A contiguous authored part of a **Run Course** with a clear surface profile and gameplay purpose.
_Avoid_: Chunk, prefab piece

**Run Course Beat**:
A high-level pacing segment of a **Run Course** made from one or more **Run Course Sections**.
_Avoid_: Zone, stage, chapter

**Target Run Duration**:
The expected time from **Launch** to **Run Finish** for a competent player taking the main playable line.
_Avoid_: Speedrun time, retry time, completionist time

**Target First Completion Run Count**:
The expected number of **Runs** before a player reaches **Run Finish** for the first time after using upgrades.
_Avoid_: Session count, level count, retry count

**Run Progression Band**:
A distance range of a **Run Course** used to tune expected reach, coin availability, and upgrade-stage pressure.
_Avoid_: Zone, stage, checkpoint, difficulty tier

**Soft Containment**:
**Run Surface** shaping that usually redirects the **Launch Target** back into the **Run Course** without making escape impossible.
_Avoid_: Wall, boundary, guard rail

**Run End Reason**:
The single cause that ended a **Run**.
_Avoid_: Game over type, fail reason, death reason

**Run Result**:
The immutable summary captured when a **Run** ends.
_Avoid_: Score state, end payload, outcome data

**Run Result Notifier**:
The runtime notification contract that publishes an accepted **Run Result**.
_Avoid_: Gameplay State inference, result polling, UI event bus

First-slice shape:

```csharp
public interface IRunResultNotifier
{
    event Action<RunResult> RunResultAccepted;
}
```

**Run Progress Frame**:
The course reference that defines forward downhill progress, lateral direction, and up direction during a **Run**.
_Avoid_: World forward, launch direction, camera forward

`RunProgressFrameSnapshot` owns course-axis calculations. In the first slice, `GetCoursePlanarSpeed(Vector3 linearVelocity)` returns unsigned planar
speed for playback scaling, and `GetCourseForwardSpeed(Vector3 linearVelocity)` should return signed speed along the frame's forward axis for run
eligibility.

**Run End Flow**:
The gameplay flow that accepts run-ending signals and produces one **Run Result**.
_Avoid_: Game over manager, result controller, death handler

In the first slice, `RunEndFlow` should implement **Run Result Notifier** and publish exactly one accepted **Run Result** per ended **Run**.

**Run Contact Category**:
The gameplay meaning of a contact or region encountered during a **Run**.
_Avoid_: Collider type, physics layer, tag

**Run Surface**:
A **Run Contact Category** that allows the **Launch Target** to slide, jump, or redirect without ending the **Run**.
_Avoid_: Ground, floor, ramp

**Run Surface Context**:
Continuous facts about the **Run Surface** currently supporting or near the **Launch Target**.
_Avoid_: Run Contact Classifier, collision event, ground tag, raw raycast hit

First-slice payload: `bool IsGrounded`, `Vector3 GroundNormal`, and `float ForwardDownhillDegrees`.
`ForwardDownhillDegrees` is positive when the supporting surface descends in run/course-forward direction, near zero on flat, and negative uphill. It
does not treat sideways bank angle as downhill sliding.

**Run Surface Context Source**:
The continuous Unity-facing probe that provides the current **Run Surface Context**.
_Avoid_: Contact-enter notifier, Run Contact Classifier, one-shot collision source

The first-slice source samples the current ground support each rendered/presentation frame. It may use Rigidbody/collider/physics implementation
details internally, but exposes only **Run Surface Context**.

**Run Surface Slope Calculator**:
Pure C# math that derives `ForwardDownhillDegrees` from a ground normal and the current **Run Progress Frame** forward/up axes.
_Avoid_: MonoBehaviour, physics query, Animator logic

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

**Run Preparation**:
The post-run phase where the player can review collected currency and choose upgrades before returning to **Pre-Launch**.
_Avoid_: Pre-Launch, shop, upgrade screen

**Pre-Launch Rig Pose**:
The authored pose where the **Slingshot** and **Launch Target** are aligned before **Pre-Launch** interaction.
_Avoid_: Player initial position, spawn point, reset transform

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

**Launch Push**:
The early character-presentation interval after a valid **Launch** is applied, while the **Launch Target** is still visually being pushed away from
the **Slingshot** before normal grounded or airborne locomotion presentation takes over.
_Avoid_: Slingshot push phase, band phase, launch one-shot

**Pull Anticipation**:
The character-presentation interval while an **Active Pull** holds the **Launch Target** before **Pull Release**.
_Avoid_: Dragged, held, slingshot pull mode

**Normalized Pull**:
The normalized strength of the current **Active Pull**.
_Avoid_: Launch time, animation progress, elapsed time

**Normalized Launch Power**:
The normalized strength of the accepted **Launch**, captured from the final **Pull Release**.
_Avoid_: Launch time, animation progress, elapsed time

**Normalized Pull Offset**:
The signed normalized lateral offset of the current **Active Pull**.
_Avoid_: Raw Pull Offset, world-space offset, steering force

`0` means centered, `-1` means fully clamped to the current effective left pull limit, and `1` means fully clamped to the current effective right pull
limit. The denominator is the effective side-specific pull limit from current slingshot geometry/config at the current pull depth, not raw
`MaximumLateralPull` alone.

**Normalized Launch Offset**:
The signed normalized lateral offset of the accepted **Launch**, captured from the final **Pull Release**.
_Avoid_: Raw Pull Offset, world-space offset, steering force

`0` means centered, `-1` means fully clamped to the effective left pull limit at release, and `1` means fully clamped to the effective right pull
limit at release.

**Slingshot Pull Offset Normalizer**:
The slingshot-owned calculation that maps raw **Pull Offset** and **Pull** depth through current slingshot geometry/config into signed normalized
lateral pull values.
_Avoid_: character offset normalizer, Animator offset normalizer, presentation-only offset math

**Slingshot Presentation Context Source**:
The slingshot-owned read-only source of presentation-facing facts for **Pull Anticipation** and **Launch Push** after slingshot validation.
_Avoid_: Input state, slingshot view state, gameplay state service

**Slingshot Presentation Context**:
The immutable current read model exposed by **Slingshot Presentation Context Source** to **Character Presentation Presenter**.
_Avoid_: mutable slingshot state, animator frame, launch request

First-slice fields are `HasActivePull`, `NormalizedPull`, `NormalizedPullOffset`, `HasLaunchPush`, `LaunchPushElapsedSeconds`,
`NormalizedLaunchPower`, and `NormalizedLaunchOffset`.

**Slingshot Active Pull Notifier**:
The slingshot-owned event source that publishes validated **Active Pull** changes and clear events after input projection, pull clamping, and
slingshot validation.
_Avoid_: pointer input stream, slingshot view state, touch indicator state

First-slice shape is C# events on a notifier interface: `ActivePullChanged` with **Slingshot Active Pull Context** and `ActivePullCleared` without
payload. This is not a `UnityEvent`, global event bus, or **Slingshot View** call.

First-slice event invocation uses the project `InvokeSafely` extension.

**Slingshot Capture Lifecycle Notifier**:
The slingshot-owned event source that publishes capture enabled/disabled lifecycle edges after the **Slingshot** has changed capture readiness.
_Avoid_: gameplay state observer, pre-launch state poller, reset guess

First-slice events are `CaptureEnabled` and `CaptureDisabled` without payload.

**Slingshot Active Pull Context**:
The small value payload published by **Slingshot Active Pull Notifier** for one validated **Active Pull** update.
_Avoid_: SlingshotPullVisual, band shape payload, touch indicator payload

First-slice fields are **Normalized Pull** and **Normalized Pull Offset**. Raw pull distance or raw **Pull Offset** may be retained only for
diagnostics; **Band Shape** and **Touch Indicator** data are not part of this context.

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
the rest/idle/default shape. It detaches only once that rest shape is clear of the current **Launch Target Silhouette**, including visible **Band**
thickness.
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
post-shot **Band Release Recoil**. During that recoil, the **Band** keeps following the moving **Launch Target** contact points until the
rest/idle/default shape is clear of the current **Launch Target Silhouette**, then detaches.
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

**Launch Target Collider Root**:
The **Launch Target** child transform that owns the project gameplay collider used for slingshot silhouette, held clearance, and run contacts.
_Avoid_: VisualRoot, character root, avatar root

If the old collider-bearing `VisualRoot` keeps the gameplay collider after cylinder removal, it should be renamed or split into a **Launch Target
Collider Root** role so it no longer sounds like the visible **Character** alignment root.

**Character**:
The visual, audio, and animated identity shown for a **Launch Target**.
_Avoid_: Player model, avatar, skin, Launch Target Presentation

**Character Visual Anchor**:
The **Launch Target** child transform used only as the stable mount/alignment point for a **Character**.
_Avoid_: Gameplay collider root, Band Center, VisualRoot-as-collider

A **Character Visual Anchor** is separate from the gameplay collider transform and **Band Center**. It exists so character mesh scale, offset, and
rotation authoring cannot accidentally change slingshot wrapping, contact classification, held alignment, or camera semantics.

First-slice launch-target hierarchy:

```text
Launch Target root
  Band Center
  LaunchTargetColliderRoot
    CapsuleCollider
  CharacterVisualAnchor
    LadybugCharacter
```

**Character Model Root**:
The internal child transform inside a concrete **Character** prefab that absorbs imported model/avatar offset, rotation, and scale.
_Avoid_: Character Visual Anchor, prefab root, gameplay collider root

In the first slice, `LadybugCharacter.prefab` has a stable prefab root with `CharacterPresentationView` and `Animator`; `ModelRoot` sits underneath
that root and contains the imported Ladybug model, skeleton, skinned meshes, and local import-alignment tweaks.

**Source Character Asset**:
An imported third-party model, skeleton, material, texture, audio, VFX, or animation asset referenced by a project-owned **Character** prefab.
_Avoid_: project-owned character prefab, runtime character, gameplay prefab

A **Source Character Asset** may live in an imported source-content bucket such as `Assets/Plugins/...`. It is reference content, not the owner of
project-specific Animator wiring, presentation tuning, scene alignment, or gameplay contracts.

**Character Presentation Mode**:
The current appearance mode selected for a **Character** from gameplay and motion context.
_Avoid_: Gameplay State, physics state, Animator state

First-slice values: `Idle`, `PullAnticipation`, `LaunchPush`, `Slide`, `Run`, `Airborne`, `Victory`, `Defeat`.
`PullAnticipation` and `LaunchPush` map to dedicated Animator Controller states or blend trees selected by `PresentationMode`; they are not Animator
triggers and do not route through `Slide` or `Run`.
First-slice transitions into `PullAnticipation` and `LaunchPush` do not wait for Animator clip exit time. `PullAnticipation` may enter from any
eligible mode through a short or immediate transition, `LaunchPush` enters immediately after an accepted **Launch**, and `LaunchPush` hands off to
locomotion through a short fixed transition after classification allows locomotion. Slide/run transition smoothing remains locomotion-specific.

**Character Presentation Mode Classifier**:
The plain C# decision logic that selects one **Character Presentation Mode** from gameplay, motion, and **Run Surface Context** facts.
_Avoid_: Animator state machine, gameplay state flow, diagnostic reason source

In the first slice, the classifier accepts `CharacterPresentationClassificationInput` and returns `CharacterPresentationClassificationResult`.
Diagnostic logging may observe mode changes and explain them, but diagnostic reasons are not part of the classification result, **Character
Presentation Frame**, view, or Animator contract.

First-slice classification priority: terminal victory, terminal defeat, pull anticipation, launch push, pre-launch idle, run-active airborne after
ungrounded debounce, run-active slide, run-active run, fallback idle.

First-slice slide/run switching uses hysteresis and a minimum locomotion mode duration so near-flat slopes and noisy contact normals do not flip the
**Character Presentation Mode** every few frames.
The **Character Presentation Presenter** owns current mode and elapsed mode time, then passes those facts into classification; the classifier does
not hide mutable mode-timer state.
Mode memory is scoped to the current lifecycle: the presenter resets current mode to `Idle` and elapsed mode time to `0f` when entering
**Pre-Launch** or starting a new **Run**, and it resets elapsed mode time whenever the classified mode changes.
Airborne entry is debounced for presentation only: the presenter owns ungrounded elapsed time, passes it into classification, and resets it when the
surface is grounded. The classifier enters `Airborne` only when the run is active, `SurfaceContext.IsGrounded` is false, and ungrounded elapsed time
meets `AirborneEnterDelaySeconds`. During the grace window, current `Slide` or `Run` presentation is preserved instead of falling to `Idle` or
switching immediately to `Airborne`.
`Airborne` exit is immediate in the first slice: once the surface is grounded again, the presenter resets ungrounded elapsed time and normal grounded
classification picks `Slide`, `Run`, or fallback `Idle` on that tick. There is no first-slice `Landing` mode or landing cue.

**Character Presentation Classification Input**:
The immutable fact bundle supplied to a **Character Presentation Mode Classifier** for one classification.
_Avoid_: Animator parameter payload, view frame, long argument list

First-slice payload: `CharacterPresentationMode CurrentMode`, `float CurrentModeElapsedSeconds`, `float UngroundedElapsedSeconds`, `HasActivePull`,
`HasLaunchPush`, `float LaunchPushElapsedSeconds`, `IsPreLaunch`, `IsRunActive`, `HasRunEnded`, `HasFinished`,
`RunSurfaceContext SurfaceContext`, `float CoursePlanarSpeed`, `float CourseForwardSpeed`, and `Vector3 LinearVelocity`. The presenter translates
gameplay state and slingshot presentation context into these presentation-facing facts; raw `GameplayStateId` is not part of the classification input.
`HasRunEnded` comes from gameplay lifecycle context. `HasFinished` is true only when `HasRunEnded` is true, the presenter has an accepted
current-run **Run Result**, and that result is successful; no accepted result means no `Victory`.

**Character Presentation Classification Result**:
The plain C# result returned by a **Character Presentation Mode Classifier**.
_Avoid_: Diagnostic reason, Animator frame, gameplay event

First-slice payload: `CharacterPresentationMode Mode`.

**Course Planar Speed**:
The unsigned planar speed magnitude of the **Launch Target** across the current **Run Progress Frame** plane.
_Avoid_: forward speed, run eligibility, signed progress speed

In the first slice, **Course Planar Speed** is useful for animation playback scaling because lateral, forward, and diagonal planar motion can all need
locomotion timing.
It is calculated by `RunProgressFrameSnapshot.GetCoursePlanarSpeed(Vector3 linearVelocity)`.

**Course Forward Speed**:
The signed speed of the **Launch Target** along the current **Run Progress Frame** forward axis.
_Avoid_: planar speed, animation speed, lateral speed

Positive values mean forward course motion, near-zero values mean lateral-only or stalled course-forward motion, and negative values mean backward
course motion. In the first slice, **Course Forward Speed** helps decide whether grounded flat movement is eligible for `Run`.
It is calculated by `RunProgressFrameSnapshot.GetCourseForwardSpeed(Vector3 linearVelocity)`, not duplicated inside a character presenter.

**Character Presentation Frame**:
The continuous presentation values that can be safely applied to a **Character Presentation View** every rendered frame.
_Avoid_: Trigger payload, event, command, raw surface context

**Playback Speed Multiplier**:
A presentation value used to scale animation playback speed.
_Avoid_: MoveSpeed, MoveSpeed01, meters-per-second speed

In the first slice, **Playback Speed Multiplier** is written as an Animator float parameter, not through global `Animator.speed`.
The **Character Presentation Presenter** computes it from course-planar speed divided by the current mode reference speed, clamped by presentation
tuning. If airborne grace preserves `Slide` or `Run`, playback still uses that preserved locomotion mode's reference speed. Once the classifier
returns `Airborne`, playback defaults to `1f`; idle, victory, and defeat also default to `1f` unless a concrete presentation feature needs scaled
timing. There is no first-slice `IsAirborneGrace` frame field.

**Lateral Lean**:
A view-facing left/right presentation amount for a **Character**.
_Avoid_: Steer, steering input, lateral control

**Character Presentation Cue**:
An optional future one-shot presentation-only effect sent to a **Character Presentation View**.
_Avoid_: Gameplay event, frame flag, repeated bool, stored trigger

**Character Presentation Tuning**:
Authored presentation-only values that help select and drive a **Character Presentation Mode**.
_Avoid_: Character config, gameplay tuning, abilities config

First-slice locomotion tuning includes slide enter/exit downhill thresholds, run enter/exit speed thresholds, minimum locomotion mode duration, and
`AirborneEnterDelaySeconds`.
Launch-presentation tuning includes `LaunchPushMinimumSeconds`, which gates when normal locomotion may resume after **Launch Push** starts.
First-slice playback tuning includes separate run and slide reference speeds plus minimum and maximum playback speed multipliers.
In the first slice, tuning is exposed through `ICharacterPresentationTuning` from the same prefab-owned `CharacterPresentationView` MonoBehaviour
that also implements `ICharacterPresentationView`; the presenter depends on the two read-only contracts, not on a separate config asset.

**Character Presentation Presenter**:
The plain C# coordinator that samples gameplay and motion facts and applies one presentation frame to a **Character Presentation View**.
_Avoid_: View Update, Animator brain, gameplay controller

First-slice VContainer lifecycle: implement `IInitializable`, `ITickable`, and `IDisposable`. `IInitializable` subscribes to presentation-relevant
events, `ITickable` applies one frame per rendered tick, and `IDisposable` unsubscribes.

In the first slice, it may subscribe to **Run Result Notifier**, store the current-run accepted **Run Result** with an explicit has-result flag, clear
that cache when entering **Pre-Launch** or running for a new **Run**, and translate `RunResult.IsSuccess` into `HasFinished` for classification.
It may also inject `IGameplayStateService` plus the configured pre-launch, running, and run-ended `GameplayStateId` assets, then translate them into
`IsPreLaunch`, `IsRunActive`, and `HasRunEnded` lifecycle flags before calling the classifier.
It owns lifecycle-scoped mode memory: entering **Pre-Launch** or starting a new **Run** clears stale terminal/locomotion hysteresis by setting the
current mode to `Idle` and elapsed mode time to `0f`; later mode changes reset only the elapsed timer.
It uses injected `ITime.DeltaTime`, clamped to a non-negative value, for presentation mode elapsed timing because it is an `ITickable`
render-frame presenter. It does not use `FixedDeltaTime` for mode elapsed timing. Each tick builds classification input from the current
mode/elapsed values, classifies, resets elapsed time to `0f` if the mode changed, otherwise advances elapsed time by delta, then applies the
presentation frame.
It also owns ungrounded elapsed time for airborne debounce, resets it when grounded or when entering **Pre-Launch** or a new **Run**, and passes the
current value into classification.

**Character Presentation View**:
The Unity-facing prefab component that writes presentation frames to Animator, VFX, audio, and presentation transforms.
_Avoid_: Gameplay controller, physics owner, mode classifier

In the first slice, a **Character Presentation View** may use SaintsField `AnimatorParam` fields for authoring-safe Animator parameter selection, but
does not expose Animator state names or SaintsField `AnimatorState` fields.
The first-slice view is passive: it implements `ICharacterPresentationView.Apply(CharacterPresentationFrame frame)` and may also implement
`ICharacterPresentationTuning` from serialized prefab fields, but it does not classify modes, tick itself, or consume gameplay state.
It enforces visual-only animation: `Animator.applyRootMotion` is disabled, imported clip root translation must not drive the **Launch Target**
Rigidbody, and any authored victory/defeat/locomotion offsets must stay inside the visual child hierarchy rather than moving the **Band Center**,
gameplay collider, camera anchor, or physics root.
The project-owned character prefab must not keep imported character physics active in the first slice: vendor `Rigidbody`, `Collider`, `Joint`,
`CharacterController`, ragdoll, hitbox, or trigger components are stripped or disabled in the instantiated **Character** hierarchy. The only active
physics contracts remain the existing **Launch Target** Rigidbody and the project-owned gameplay collider or an explicitly retuned replacement for
that same contract.
The first-slice concrete `LadybugCharacter.prefab` is project-owned clean composition. It references imported Ladybug meshes and clips as source
content, but is not a direct vendor prefab or vendor prefab variant that inherits unmanaged components or hierarchy assumptions. It contains only
accepted presentation pieces: skinned meshes/model references, Animator, `CharacterPresentationView`, authored local offset/rotation/scale, and
presentation tuning.
Project-owned character composition assets such as `LadybugCharacter.prefab` and `LadybugCharacter.controller` live under `Assets/Game/...`.
Imported third-party Ladybug source assets live under `Assets/Plugins/Ladybug`.

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
- A **Pre-Launch Rig Pose** aligns one **Slingshot** with one **Launch Target**.
- A **Pre-Launch Rig Pose** places the held **Launch Target** so its **Band Center** aligns with the **Rest Point**.
- A **Run Course** defines the playable path traversed during one **Run**.
- A **Run Course** may have **Run Course Beats**.
- A **Run Course Beat** belongs to one **Run Course**.
- A **Run Course Beat** may have **Run Course Sections**.
- A **Run Course** may have **Run Course Sections**.
- A **Run Course Section** belongs to one **Run Course**.
- A **Run Course** has one **Target Run Duration**.
- A **Run Course** has one **Target First Completion Run Count**.
- A **Run Course** may have **Run Progression Bands**.
- A **Run Progression Band** belongs to one **Run Course**.
- A **Run Progression Band** may contain **Run Course Sections**.
- A **Run Progression Band** describes expected reach and tuning pressure, not gameplay flow state.
- **Soft Containment** is achieved by **Run Surface** shape.
- **Soft Containment** does not end a **Run**.
- A **Run** is governed by one current **Gameplay State**.
- A **Run** has one **Run Progress Frame**.
- A **Run Progress Frame** may compute **Course Planar Speed**.
- A **Run Progress Frame** may compute **Course Forward Speed**.
- A **Run** ends with one **Run End Reason**.
- A **Run Result** summarizes one ended **Run**.
- A **Run Result** has one **Run End Reason**.
- A **Run Result** has one **Run Currency Snapshot**.
- A **Run Result** uses the **Run Progress Frame** to describe forward downhill progress.
- A **Run Result Notifier** publishes accepted **Run Results**.
- **Run End Flow** produces one **Run Result** for an ended **Run**.
- **Run End Flow** may implement **Run Result Notifier**.
- **Run End Flow** publishes one accepted **Run Result** per ended **Run**.
- A **Run Contact Category** determines how a **Run** contact or region affects the **Run**.
- A **Run Surface** does not end a **Run**.
- **Run Surface Context** describes current **Run Surface** support for the **Launch Target**.
- **Run Surface Context** does not end a **Run**.
- **Run Surface Context** may help select a **Character Presentation Mode**.
- **Run Surface Context** contains grounded state, ground normal, and forward-downhill slope in the first slice.
- **Run Surface Context** does not expose collider, hit point, tag, `RunContactCategory`, or raw raycast hit in the first slice.
- A **Run Surface Context Source** provides one current **Run Surface Context**.
- A **Run Surface Context Source** is continuous, not based on contact-enter events.
- A **Run Surface Context Source** may use Rigidbody, collider, and physics queries internally.
- A **Run Surface Slope Calculator** computes `ForwardDownhillDegrees` as pure math.
- A **Run Surface Slope Calculator** uses the current **Run Progress Frame** forward and up axes.
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
- A **Launch Target** may have one **Launch Target Collider Root**.
- A **Launch Target Collider Root** owns the project gameplay collider.
- A **Launch Target Collider Root** may provide the **Launch Target Silhouette** source collider.
- A **Launch Target Collider Root** is separate from **Character Visual Anchor**.
- A **Launch Target Collider Root** is separate from **Band Center**.
- **Band Center**, **Launch Target Collider Root**, and **Character Visual Anchor** are sibling roles under the first-slice **Launch Target** root.
- A **Launch Target** may have one **Character**.
- A **Launch Target** may have one **Character Visual Anchor**.
- A **Character Visual Anchor** is separate from **Band Center**.
- A **Character Visual Anchor** is separate from the gameplay collider transform.
- A **Character Visual Anchor** does not define slingshot wrapping, run contacts, held alignment, or camera framing.
- A **Character** may be mounted under a **Character Visual Anchor**.
- A concrete **Character** prefab has a stable prefab root.
- A concrete **Character** prefab root owns **Character Presentation View**.
- A concrete **Character** prefab root owns the Animator in the first slice.
- A concrete **Character** prefab may have one **Character Model Root**.
- A **Character Model Root** is internal to a concrete **Character** prefab.
- A **Character Model Root** absorbs imported model/avatar offset, rotation, and scale.
- A **Character Model Root** does not define scene placement, gameplay collider, **Band Center**, or **Launch Target** physics.
- A concrete **Character** prefab may reference one or more **Source Character Assets**.
- A **Source Character Asset** does not own project-specific character composition, presentation tuning, scene alignment, or gameplay contracts.
- A **Character** does not define **Launch Target** gameplay behavior.
- A **Character** does not define **Run** abilities in the first slice.
- A **Character** has one current **Character Presentation Mode**.
- A **Character Presentation Mode** is derived from gameplay and motion context.
- A **Character Presentation Mode** is not a **Gameplay State**.
- **Pull Anticipation** is represented as a **Character Presentation Mode**.
- **Launch Push** is represented as a **Character Presentation Mode**.
- **Pull Anticipation** maps to a dedicated Animator Controller state or blend tree.
- **Launch Push** maps to a dedicated Animator Controller state or blend tree.
- **Pull Anticipation** and **Launch Push** are selected through `PresentationMode`, not through Animator triggers.
- **Pull Anticipation** and **Launch Push** do not route through `Slide` or `Run`.
- **Pull Anticipation** may enter from any eligible **Character Presentation Mode** without waiting for Animator clip exit time.
- **Launch Push** enters immediately after an accepted **Launch** without waiting for Animator clip exit time.
- **Launch Push** hands off to locomotion through a short fixed Animator transition after classification allows locomotion.
- Slide/run Animator transition smoothing remains locomotion-specific.
- A **Character Presentation Mode Classifier** selects one **Character Presentation Mode**.
- A **Character Presentation Mode Classifier** consumes one **Character Presentation Classification Input**.
- A **Character Presentation Mode Classifier** returns one **Character Presentation Classification Result**.
- A **Character Presentation Mode Classifier** gives terminal modes priority over **Pull Anticipation**, **Launch Push**, pre-launch, and locomotion
  modes.
- A **Character Presentation Mode Classifier** gives **Pull Anticipation** priority over **Launch Push** and locomotion modes.
- A **Character Presentation Mode Classifier** gives **Launch Push** priority over locomotion modes until its handoff guard is satisfied.
- A **Character Presentation Mode Classifier** gives pre-launch idle priority over stale motion facts.
- A **Character Presentation Mode Classifier** evaluates grounded locomotion only while a run is active.
- A **Character Presentation Mode Classifier** debounces `Airborne` entry through ungrounded elapsed time.
- A **Character Presentation Mode Classifier** preserves current `Slide` or `Run` during short ungrounded grace windows.
- A **Character Presentation Mode Classifier** exits `Airborne` immediately once grounded again.
- A **Character Presentation Mode Classifier** uses hysteresis when switching between `Slide` and `Run`.
- A **Character Presentation Mode Classifier** respects minimum locomotion mode duration before switching between `Slide` and `Run`.
- A **Character Presentation Mode Classifier** does not own hidden mutable mode-timer state.
- A **Character Presentation Mode Classifier** does not consume `IGameplayStateService`.
- A **Character Presentation Mode Classifier** does not consume `GameplayStateId`.
- A **Character Presentation Classification Input** contains presentation-facing lifecycle facts.
- A **Character Presentation Classification Input** contains **Pull Anticipation** and **Launch Push** facts from **Slingshot Presentation Context
  Source**.
- A **Character Presentation Classification Input** uses **Active Pull** presence and **Launch Push** presence/timing for mode selection.
- A **Character Presentation Classification Input** does not use **Normalized Pull**, **Normalized Launch Power**, **Normalized Pull Offset**, or
  **Normalized Launch Offset** for first-slice mode selection.
- A **Character Presentation Classification Input** contains current **Character Presentation Mode** and elapsed mode time.
- A **Character Presentation Classification Input** contains ungrounded elapsed time for airborne debounce.
- A **Character Presentation Classification Input** contains **Course Planar Speed** for playback scaling.
- A **Character Presentation Classification Input** contains **Course Forward Speed** for run eligibility.
- A **Character Presentation Classification Input** does not contain raw `GameplayStateId`.
- A **Character Presentation Classification Result** contains one `CharacterPresentationMode Mode` in the first slice.
- A **Character Presentation Classification Result** does not expose diagnostic reasons as first-slice runtime contract data.
- A **Character Presentation Frame** may include one **Character Presentation Mode**.
- A **Character Presentation Frame** does not include **Character Presentation Cues**.
- A **Character Presentation Frame** does not include raw **Run Surface Context** slope facts in the first slice.
- A **Character Presentation Frame** does not include raw grounded or vertical velocity facts in the first slice.
- A **Character Presentation Frame** does not include raw or normalized movement speed in the first slice.
- A **Character Presentation Frame** does not include `IsAirborneGrace` in the first slice.
- A **Character Presentation Frame** may include **Playback Speed Multiplier**.
- A **Character Presentation Frame** may include **Normalized Pull**, **Normalized Launch Power**, **Normalized Pull Offset**, and
  **Normalized Launch Offset** for slingshot-driven character presentation.
- A **Character Presentation Frame** zeros inactive slingshot-driven presentation values.
- During **Pull Anticipation**, a **Character Presentation Frame** may carry **Normalized Pull** and **Normalized Pull Offset** while
  **Normalized Launch Power** and **Normalized Launch Offset** are zero.
- During **Launch Push**, a **Character Presentation Frame** may carry **Normalized Launch Power** and **Normalized Launch Offset** while
  **Normalized Pull** and **Normalized Pull Offset** are zero.
- Outside **Pull Anticipation** and **Launch Push**, a **Character Presentation Frame** carries zero values for **Normalized Pull**,
  **Normalized Launch Power**, **Normalized Pull Offset**, and **Normalized Launch Offset**.
- A **Character Presentation View** consumes slingshot-driven presentation values from **Character Presentation Frame**.
- A **Character Presentation View** does not read **Slingshot Presentation Context Source** directly.
- **Playback Speed Multiplier** is an Animator parameter in the first slice.
- **Normalized Pull** is a distinct Animator float parameter in the first slice.
- **Normalized Launch Power** is a distinct Animator float parameter in the first slice.
- **Normalized Pull Offset** is a distinct Animator float parameter in the first slice.
- **Normalized Launch Offset** is a distinct Animator float parameter in the first slice.
- **Normalized Pull**, **Normalized Launch Power**, **Normalized Pull Offset**, and **Normalized Launch Offset** are not collapsed into generic
  slingshot strength or offset Animator parameters in the first slice.
- **Playback Speed Multiplier** does not use global `Animator.speed` in the first slice.
- **Playback Speed Multiplier** is computed by the **Character Presentation Presenter** from **Course Planar Speed** and current mode reference speed.
- **Playback Speed Multiplier** keeps locomotion scaling while airborne grace preserves `Slide` or `Run`.
- **Playback Speed Multiplier** defaults to `1f` when the selected mode is `Airborne`.
- **Course Forward Speed** is not the first-slice playback scaling source.
- **Character Presentation Tuning** may define run and slide reference speeds.
- **Character Presentation Tuning** may define minimum and maximum playback speed multipliers.
- A **Character Presentation Frame** does not include `Steer` in the first slice.
- A **Character Presentation Frame** may include **Lateral Lean** only when lateral presentation is implemented.
- **Lateral Lean** does not control the **Launch Target**.
- A **Character Presentation Cue** is optional and presentation-only.
- A **Character** may have **Character Presentation Tuning**.
- **Character Presentation Tuning** does not define **Launch Target** gameplay behavior.
- **Character Presentation Tuning** may influence **Character Presentation Mode** selection.
- **Character Presentation Tuning** may define slide/run hysteresis thresholds and minimum locomotion mode duration.
- **Character Presentation Tuning** may define `AirborneEnterDelaySeconds`.
- A first-slice concrete **Character** prefab is project-owned clean composition.
- A concrete **Character** prefab may reference imported vendor meshes and clips as source content.
- A first-slice concrete **Character** prefab does not inherit unmanaged vendor components or hierarchy assumptions.
- `LadybugCharacter.prefab` is not a direct vendor prefab or vendor prefab variant in the first slice.
- Project-owned concrete **Character** prefabs and Animator Controllers live under `Assets/Game/...`.
- Imported third-party **Character** source assets live under `Assets/Plugins/...`.
- `Assets/Plugins/Ladybug` is an import/source bucket, not the owner of project-specific character composition.
- A **Character** may have one **Character Presentation View**.
- A **Character Presentation View** may implement `ICharacterPresentationView`.
- A **Character Presentation View** may implement `ICharacterPresentationTuning`.
- `ICharacterPresentationView` and `ICharacterPresentationTuning` may be backed by the same MonoBehaviour in the first slice.
- A **Character Presentation Presenter** applies presentation frames to a **Character Presentation View**.
- A **Character Presentation Presenter** may depend on `ICharacterPresentationView`.
- A **Character Presentation Presenter** may depend on `ICharacterPresentationTuning`.
- A **Character Presentation Presenter** is a VContainer entry point in the first slice.
- A **Character Presentation Presenter** initializes event subscriptions through `IInitializable`.
- A **Character Presentation Presenter** applies one presentation frame per rendered tick through `ITickable`.
- A **Character Presentation Presenter** releases event subscriptions through `IDisposable`.
- A **Character Presentation Presenter** applies gameplay edges by changing **Character Presentation Mode** in the first slice.
- A **Character Presentation Presenter** owns current **Character Presentation Mode** and elapsed mode time.
- A **Character Presentation Presenter** scopes current **Character Presentation Mode** and elapsed mode time to the current lifecycle.
- A **Character Presentation Presenter** resets mode memory when entering **Pre-Launch** or starting a new **Run**.
- A **Character Presentation Presenter** resets elapsed mode time when the classified **Character Presentation Mode** changes.
- A **Character Presentation Presenter** uses `ITime.DeltaTime` for mode elapsed timing.
- A **Character Presentation Presenter** does not use `FixedDeltaTime` for presentation mode elapsed timing.
- A **Character Presentation Presenter** builds classification input before advancing elapsed mode time for the current tick.
- A **Character Presentation Presenter** owns ungrounded elapsed time for `Airborne` debounce.
- A **Character Presentation Presenter** resets ungrounded elapsed time when grounded, entering **Pre-Launch**, or starting a new **Run**.
- A **Character Presentation Presenter** resumes grounded `Slide`, `Run`, or fallback `Idle` classification on the same tick that `Airborne` exits.
- A **Character Presentation Presenter** may consume `IGameplayStateService`.
- A **Character Presentation Presenter** may consume configured pre-launch, running, and run-ended `GameplayStateId` assets.
- A **Character Presentation Presenter** maps gameplay state ids to `IsPreLaunch`, `IsRunActive`, and `HasRunEnded`.
- A **Character Presentation Presenter** may consume **Run Result Notifier**.
- A **Character Presentation Presenter** may map `RunResult.IsSuccess` to terminal presentation facts.
- A **Character Presentation Presenter** does not infer victory or defeat from run-ended gameplay state alone.
- A **Character Presentation Presenter** stores accepted **Run Result** with an explicit has-result flag.
- A **Character Presentation Presenter** clears cached **Run Result** when entering **Pre-Launch** or running for a new **Run**.
- A **Character Presentation Presenter** computes `HasFinished` only from the current run-ended state plus the current accepted successful **Run Result**.
- A **Character Presentation Presenter** may send **Character Presentation Cues** only for future presentation-only effects.
- A **Character Presentation View** does not receive **Run End Reason** in the first slice.
- A **Character Presentation View** does not receive **Run Result** in the first slice.
- A **Character Presentation View** does not receive raw **Run Surface Context** slope facts in the first slice.
- A **Character Presentation View** may serialize Animator parameter references for `PresentationMode`, `PlaybackSpeedMultiplier`,
  `NormalizedPull`, `NormalizedLaunchPower`, `NormalizedPullOffset`, and `NormalizedLaunchOffset`.
- A **Character Presentation View** does not serialize concrete Animator state references in the first slice.
- A **Character Presentation View** does not receive landing cues in the first slice.
- A **Character Presentation View** does not own gameplay or physics orchestration.
- A **Character Presentation View** disables Animator root motion in the first slice.
- A **Character Presentation View** does not move the **Launch Target** Rigidbody, **Band Center**, gameplay collider, or camera anchor through animation root motion.
- A **Character Presentation View** does not keep imported character `Rigidbody`, `Collider`, `Joint`, `CharacterController`, ragdoll, hitbox, or trigger components active in the first slice.
- The first-slice **Character** hierarchy does not create additional gameplay contacts or run-ending contacts.
- A project-owned gameplay collider may be retuned to fit the **Character**, but imported character colliders are not automatically gameplay colliders.
- A **Run Camera** follows the **Launch Target** during a **Run**.
- A **Run Camera** uses one **Run Camera Anchor** to frame the **Launch Target**.
- A **Run Camera Anchor** is derived from the **Launch Target**.
- **Run Preparation** happens after an ended **Run** and before **Pre-Launch**.
- A **Pre-Launch Camera** frames the **Slingshot** before a **Run**.
- A **Run Camera** takes over from a **Pre-Launch Camera** after **Launch**.
- A **Gameplay State Id** identifies one **Gameplay State**.
- A **Gameplay State Transition** identifies one allowed change between two **Gameplay States**.
- **Gameplay Flow** changes the current **Gameplay State**.
- A **Launch Sequence** presents a valid **Launch**.
- A **Launch Sequence** may include **Launch Push**.
- **Launch Push** is separate from **Band Release Recoil**.
- **Launch Push** affects **Character Presentation** and does not decide launch physics.
- **Launch Push** lifetime is independent from **Band Release Recoil** lifetime.
- **Launch Push** starts from the applied **Launch** edge and should not stretch or cut itself based on **Band Release Recoil** clearance.
- **Normalized Launch Power** may tune **Launch Push** animation intensity, but it is not the **Launch Push** exit condition.
- `LaunchPushMinimumSeconds` may gate when normal locomotion can resume after **Launch Push** starts.
- An **Active Pull** may drive **Pull Anticipation**.
- **Pull Anticipation** affects **Character Presentation** and does not decide pull geometry or launch physics.
- **Normalized Pull** may tune **Pull Anticipation** animation intensity, but it is not an animation timeline.
- **Pull Anticipation** and **Launch Push** facts should come from slingshot-owned lifecycle/presentation context after slingshot validation.
- **Pull Anticipation** should not be inferred from raw **Pointer Input**.
- **Pull Anticipation** should not be inferred from **Slingshot View** state.
- A **Slingshot Active Pull Notifier** publishes validated **Active Pull** changes and active-pull clear events.
- A **Slingshot Active Pull Notifier** publishes **Slingshot Active Pull Context** on active-pull changes.
- **Slingshot Active Pull Notifier** publishing means raising C# events on the notifier interface.
- **Slingshot Active Pull Notifier** events are raised through the project `InvokeSafely` extension.
- A **Slingshot Active Pull Notifier** may be implemented by `SlingshotController` in the first slice.
- A **Slingshot Capture Lifecycle Notifier** publishes capture readiness/reset edges, not active-pull values.
- A **Slingshot Capture Lifecycle Notifier** is separate from **Slingshot Active Pull Notifier** even if both are implemented by the same slingshot
  component in the first slice.
- A **Slingshot Capture Lifecycle Notifier** publishes first-slice `CaptureEnabled` and `CaptureDisabled` events without payload.
- `CaptureEnabled` is emitted after capture setup is committed, including geometry refresh, target hold/rest alignment, and capture idle view update.
- `CaptureDisabled` is emitted exactly once when capture becomes disabled, including disable paths that return early because launch handoff or release
  recoil is pending.
- Capture lifecycle events are not emitted for no-op same-state capture calls.
- **Slingshot Active Pull Context** contains normalized pull presentation facts, not slingshot view geometry.
- **Slingshot Active Pull Context** does not include **Band Shape** or **Touch Indicator** screen position.
- `SlingshotPullVisual` remains a slingshot view payload.
- A valid **Pull Release** that becomes an accepted **Launch** keeps **Pull Anticipation** context visible until **Launch Push** starts.
- An invalid, weak, forward-only, or canceled **Pull Release** clears **Pull Anticipation** context immediately.
- `LaunchApplied` is the handoff edge that clears live **Active Pull** context and starts **Launch Push** context.
- `CancelActivePullToCaptureIdle()` raises `ActivePullCleared` through `InvokeSafely` when a live validated **Active Pull** stops because of invalid
  pull visual, invalid band shape, weak release, or pointer cancellation.
- A valid **Pull Release** that becomes an accepted **Launch** does not raise `ActivePullCleared` immediately; **Capture Disabled** clears live pull
  context before `LaunchApplied` starts **Launch Push**.
- Ignored pointer input without a live **Active Pull** does not raise `ActivePullCleared`.
- A **Slingshot Presentation Context Source** consumes **Slingshot Active Pull Notifier** events for live **Active Pull** facts.
- A **Slingshot Presentation Context Source** may consume `SlingshotLaunchRequest` at the `LaunchApplied` edge for accepted **Launch** facts in the
  first slice.
- `SlingshotLaunchRequest` is mapped into presentation facts by **Slingshot Presentation Context Source**; it is not exposed to **Character
  Presentation Presenter** or **Character Presentation View**.
- A **Slingshot Presentation Context Source** provides **Pull Anticipation** and **Launch Push** facts to **Character Presentation Presenter**.
- A **Slingshot Presentation Context Source** exposes current facts as one immutable **Slingshot Presentation Context** value.
- A **Slingshot Presentation Context** contains `HasActivePull`, `NormalizedPull`, `NormalizedPullOffset`, `HasLaunchPush`,
  `LaunchPushElapsedSeconds`, `NormalizedLaunchPower`, and `NormalizedLaunchOffset`.
- A **Slingshot Presentation Context Source** zeroes inactive pull and launch channels before exposing **Slingshot Presentation Context**.
- A **Character Presentation Presenter** may copy **Slingshot Presentation Context** facts into **Character Presentation Classification Input** and
  **Character Presentation Frame** without stale-value checks.
- A **Slingshot Presentation Context Source** exposes **Normalized Pull** for live **Active Pull** intensity and **Normalized Launch Power** for frozen
  accepted **Launch** intensity.
- A **Slingshot Presentation Context Source** exposes **Normalized Pull Offset** for live **Active Pull** lateral intent and **Normalized Launch Offset**
  for frozen accepted **Launch** lateral intent.
- **Normalized Pull Offset** and **Normalized Launch Offset** use the effective side-specific allowed pull offset, including slingshot geometry limits
  and depth-based lateral ramp.
- **Normalized Pull Offset** and **Normalized Launch Offset** do not use raw `MaximumLateralPull` as their only denominator.
- A **Slingshot Pull Offset Normalizer** owns the shared slingshot-side calculation for **Normalized Pull Offset** and **Normalized Launch Offset**.
- A **Slingshot Pull Offset Normalizer** belongs to **Slingshot** language, not **Character Presentation** language.
- `SlingshotController` may use **Slingshot Pull Offset Normalizer** for pull clamping and **Slingshot Active Pull Context**.
- A **Slingshot Presentation Context Source** may use **Slingshot Pull Offset Normalizer** when mapping `SlingshotLaunchRequest` into frozen
  **Launch Push** presentation facts.
- A **Slingshot Presentation Context Source** exposes **Launch Push** presence and launch-push elapsed time for the current accepted **Launch**.
- A **Slingshot Presentation Context Source** owns the `LaunchPushElapsedSeconds` timer.
- `LaunchPushElapsedSeconds` starts at `0f` on `LaunchApplied`, increments through injected `ITime` while `HasLaunchPush` is true, and resets on
  `CaptureEnabled`.
- A **Character Presentation Presenter** reads `LaunchPushElapsedSeconds` from **Slingshot Presentation Context Source**; it does not increment that
  timer itself.
- A **Slingshot Presentation Context Source** does not decide whether the selected **Character Presentation Mode** is still `LaunchPush`.
- **Launch Push** presence is reset by pre-launch recapture or new-run lifecycle reset, not by **Band Release Recoil** completion.
- A **Slingshot Presentation Context Source** consumes **Slingshot Capture Lifecycle Notifier** events to reset latched pull/launch presentation context.
- A **Slingshot Presentation Context Source** should not inject gameplay state ids just to reset slingshot-owned presentation context.
- **Capture Enabled** clears live **Active Pull** context and latched **Launch Push** context after the slingshot rig has been reset/recaptured.
- **Capture Disabled** clears live **Active Pull** context but does not end **Launch Push** by itself.
- `SlingshotInstaller` registers slingshot-owned runtime services and notifier interfaces.
- `SlingshotController` is the first-slice implementation behind `ISlingshotCapture`, `ISlingshotLaunchNotifier`, `ISlingshotActivePullNotifier`,
  and `ISlingshotCaptureLifecycleNotifier`.
- `SlingshotPresentationContextSource` is registered as a slingshot-owned entry point/singleton exposed through
  `ISlingshotPresentationContextSource`, and implements VContainer `IInitializable`, `ITickable`, and `IDisposable`.
- `SlingshotPullOffsetNormalizer` is registered as a singleton pure service exposed through `ISlingshotPullOffsetNormalizer`.
- `GameplayLifetimeScope` should not gain scene serialized references for pure slingshot presentation services.
- Slingshot presentation source tests use focused EditMode fakes for slingshot notifiers and `ITime`.
- Composition tests resolve the new slingshot interfaces from the container and verify controller-backed notifier interfaces come from the same
  logical slingshot source.
- A **Slingshot Presentation Context Source** does not select **Character Presentation Mode** directly.
- Terminal **Character Presentation Mode** values outrank **Pull Anticipation**, **Launch Push**, and locomotion modes.
- **Pull Anticipation** outranks **Launch Push** and locomotion modes while a validated **Active Pull** is present.
- **Launch Push** outranks locomotion modes until its minimum handoff guard allows normal presentation to resume.
- **Character Presentation Mode Classifier** regression tests prove **Pull Anticipation** beats downhill `Slide`, **Launch Push** beats downhill
  `Slide` and flat-forward `Run` before `LaunchPushMinimumSeconds`, normal locomotion resumes after that guard, and terminal modes still outrank
  slingshot modes.
- **Character Presentation Presenter** regression tests prove it reads **Slingshot Presentation Context Source**, forwards pull/launch-push facts into
  **Character Presentation Classification Input**, and copies normalized pull/launch values into **Character Presentation Frame**.
- **Slingshot Presentation Context Source** regression tests prove `LaunchApplied` starts **Launch Push** at elapsed `0f`, ticks elapsed time through
  injected `ITime`, `CaptureEnabled` resets pull and launch-push context, and `CaptureDisabled` clears live pull without clearing the launch-push
  latch.
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
  the rest/idle/default **Band Shape** is clear of that silhouette; after that, it detaches and must not keep chasing the **Launch Target**.
- During **Band Release Recoil**, the **Pulled Side** relaxes from the launch **Pull Point** toward the **Rest Point**, while **Band Contact Points**
  follow the current **Launch Target Silhouette**.
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
> **Domain expert:** "Yes, briefly — after the shot, the **Band** follows the moving **Launch Target** contact points during **Band Release Recoil** so it feels like the **Band** pushed the target forward. Once the rest/idle/default **Band Shape** is clear of the current **Launch Target Silhouette**, it detaches and stops following."

> **Dev:** "Can the **Slingshot** accept another **Pull** after **Launch**?"
> **Domain expert:** "No — after **Launch**, the **Run** has started and **Pre-Launch** is over."

> **Dev:** "Is the thing that stops the **Run** the same as the score shown after it?"
> **Domain expert:** "No — **Run End Reason** says why the **Run** ended, while **Run Result** is the summary captured for the ended **Run**."

> **Dev:** "Can a finish trigger and obstacle contact both publish separate results?"
> **Domain expert:** "No — **Run End Flow** accepts one run-ending signal and produces one **Run Result**."

> **Dev:** "Does every collider hit during a **Run** end the **Run**?"
> **Domain expert:** "No — **Run Surface** contacts are traversal, while **Run Obstacle** and **Run Finish** contacts can end the **Run**."

> **Dev:** "Should a character-specific contact classifier decide slide versus run?"
> **Domain expert:** "No — **Run Surface Context** provides continuous surface facts, and a **Character Presentation Mode** classifier decides how the **Character** appears."

> **Dev:** "What belongs in first-slice **Run Surface Context**?"
> **Domain expert:** "`IsGrounded`, `GroundNormal`, and `ForwardDownhillDegrees`. Keep collider, hit point, tag, `RunContactCategory`, and raw raycast hit inside the source implementation or diagnostics."

> **Dev:** "Should **Run Surface Context** come from `RigidbodyContactNotifier` collision-enter events?"
> **Domain expert:** "No — use a continuous **Run Surface Context Source** for stable presentation facts, and keep `ForwardDownhillDegrees` calculation in a pure **Run Surface Slope Calculator** that can be EditMode-tested without Unity physics."

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

> **Dev:** "Is the **Rest Point** the same thing as the level start pose?"
> **Domain expert:** "No — the **Rest Point** is owned by the **Slingshot**, while the **Pre-Launch Rig Pose** aligns the **Slingshot** and **Launch Target** before a new **Pull**."

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

> **Dev:** "Is Ladybug the new **Launch Target**?"
> **Domain expert:** "Ladybug is the **Character**; the controlled gameplay object remains the **Launch Target**."

> **Dev:** "Can we align the character by moving or scaling the collider-bearing `VisualRoot`?"
> **Domain expert:** "No — use a separate **Character Visual Anchor**. Collider, **Band Center**, and character visual alignment are separate contracts."

> **Dev:** "Should the old collider-bearing `VisualRoot` keep that name after the cylinder mesh is removed?"
> **Domain expert:** "No — rename or split it into a **Launch Target Collider Root** role so nobody mistakes it for the **Character** visual alignment root."

> **Dev:** "Should `Band Center`, `LaunchTargetColliderRoot`, and `CharacterVisualAnchor` be siblings under the **Launch Target** root?"
> **Domain expert:** "Yes — the root owns physics, **Band Center** owns held/slingshot alignment, **Launch Target Collider Root** owns collider/silhouette, and **Character Visual Anchor** owns visible character mounting."

> **Dev:** "Should imported Ladybug mesh offset/scale be authored on the `LadybugCharacter` prefab root?"
> **Domain expert:** "No — keep the prefab root stable with `CharacterPresentationView` and Animator. Put imported model alignment quirks under an internal **Character Model Root**."

> **Dev:** "Does picking Ladybug change launch force or steering?"
> **Domain expert:** "No — for the first slice, the **Character** is presentation-only and does not define **Run** abilities."

> **Dev:** "Should sliding and running be separate **Gameplay States**?"
> **Domain expert:** "No — they are **Character Presentation Modes** derived from the **Run** context, not phases of gameplay flow."

> **Dev:** "Should the pre-launch character mode be called Held?"
> **Domain expert:** "No — use `Idle` as the **Character Presentation Mode** so character presentation does not expose slingshot-specific language."

> **Dev:** "Should Ladybug need a separate config asset for slide/run thresholds?"
> **Domain expert:** "No — first-slice **Character Presentation Tuning** can live on the prefab-owned character view and be exposed to plain C# through an interface."

> **Dev:** "Should the character view decide its animation every `Update()`?"
> **Domain expert:** "No — the **Character Presentation Presenter** ticks once per rendered frame and applies a frame to the **Character Presentation View**."

> **Dev:** "Should the presenter be only an `ITickable`?"
> **Domain expert:** "No — because it subscribes to `RunResultAccepted`, make it a VContainer entry point with `IInitializable`, `ITickable`, and `IDisposable`: subscribe, tick, unsubscribe."

> **Dev:** "Should launch and run-ended Animator triggers be booleans in the frame?"
> **Domain expert:** "No — in the first slice, launch and run-ended edges change **Character Presentation Mode**. Add a **Character Presentation Cue** later only for presentation-only one-shots like dust or landing impact."

> **Dev:** "Should the character view receive **Run End Reason**?"
> **Domain expert:** "No — the **Character Presentation Presenter** maps `Finished` to `Victory` and non-finished run endings to `Defeat` in the first slice."

> **Dev:** "How should character presentation know whether a run-ended state means victory or defeat?"
> **Domain expert:** "Subscribe to a **Run Result Notifier** from `RunEndFlow` and use the accepted `RunResult.IsSuccess`. The run-ended gameplay state alone says only that the run ended, not whether the terminal character mode should be `Victory` or `Defeat`."

> **Dev:** "Can the presenter keep the last accepted **Run Result** forever?"
> **Domain expert:** "No — cache the accepted result with an explicit flag, clear it when entering **Pre-Launch** or running for a new **Run**, and allow `Victory` only for a current successful accepted result."

> **Dev:** "Should `CharacterPresentationFrame` include `GroundSlopeDegrees` and `ForwardSlopeDegrees`?"
> **Domain expert:** "No — **Run Surface Context** and the mode classifier consume raw slope facts. The frame carries only the selected **Character Presentation Mode** and Animator/view values."

> **Dev:** "Should `CharacterPresentationFrame` expose `Steer`?"
> **Domain expert:** "No — `Steer` sounds like gameplay control. If lateral character presentation is needed, expose **Lateral Lean** as a view-facing value; otherwise omit it."

> **Dev:** "Should `CharacterPresentationFrame` include `IsGrounded` or `VerticalSpeed`?"
> **Domain expert:** "No — those are classifier inputs. The first-slice frame uses `Airborne` as the presentation result; add presentation-language values later only if jump, fall, or landing visuals need them."

> **Dev:** "Should `CharacterPresentationFrame` include `MoveSpeed`, `MoveSpeed01`, and `MoveSpeedMultiplier`?"
> **Domain expert:** "No — raw and normalized movement speed are presenter-side facts. The first-slice frame needs only **Playback Speed Multiplier** for animation scaling."

> **Dev:** "Should **Playback Speed Multiplier** be applied through global `Animator.speed`?"
> **Domain expert:** "No — write it as an Animator float parameter so locomotion states can use it without speeding up idle, victory, defeat, or transition timing."

> **Dev:** "Who computes **Playback Speed Multiplier**?"
> **Domain expert:** "The presenter computes it as clamped course-planar speed divided by the current mode reference speed. Use separate run and slide reference speeds; keep idle, airborne, victory, and defeat at `1f` until a concrete feature needs scaled timing."

> **Dev:** "During airborne grace, should **Playback Speed Multiplier** switch to `1f`?"
> **Domain expert:** "No — while grace preserves `Slide` or `Run`, it still uses that locomotion mode's playback scaling. Switch to `1f` only when the selected mode is actually `Airborne`."

> **Dev:** "Should the first-slice character view expose SaintsField `AnimatorState` fields?"
> **Domain expert:** "No — use SaintsField `AnimatorParam` fields for frame-owned Animator parameters. The Animator Controller owns states and transitions."

> **Dev:** "Can a Ladybug slide or run clip move the **Launch Target** through root motion?"
> **Domain expert:** "No — the Rigidbody owns world movement. The Animator is visual-only: disable `Animator.applyRootMotion`, keep clips in-place for gameplay movement, and let authored offsets affect only the visual child hierarchy."

> **Dev:** "Can we keep Ladybug prefab colliders or ragdoll bodies active for the first slice?"
> **Domain expert:** "No — the project-owned character prefab should strip or disable imported physics components. Keep the existing **Launch Target** Rigidbody and gameplay collider authoritative unless a specific imported collider is deliberately adopted as that gameplay contract and tested."

> **Dev:** "Should `LadybugCharacter.prefab` be a vendor prefab variant?"
> **Domain expert:** "No — use vendor meshes and clips as source content, but build a clean project-owned prefab with only accepted presentation components and authored local alignment/tuning."

> **Dev:** "Should `LadybugCharacter.prefab` and `LadybugCharacter.controller` live in `Assets/Plugins/Ladybug`?"
> **Domain expert:** "No — put third-party source imports in `Assets/Plugins/Ladybug`, but keep project-owned composition assets under `Assets/Game/...`."

> **Dev:** "Should `CharacterPresentationModeClassifier` return a decision reason?"
> **Domain expert:** "No — return `CharacterPresentationClassificationResult` with `CharacterPresentationMode Mode` only. If debugging needs reasons, use optional transition diagnostics that compare previous and current modes and log why the mode changed."

> **Dev:** "Should `CharacterPresentationClassificationInput` include raw `GameplayStateId`?"
> **Domain expert:** "No — the presenter translates gameplay state into presentation-facing facts like `IsPreLaunch`, `IsRunActive`, `HasRunEnded`, and `HasFinished` before calling the classifier."

> **Dev:** "Do we need a separate lifecycle source wrapper just to compute `IsPreLaunch`, `IsRunActive`, and `HasRunEnded`?"
> **Domain expert:** "No — in the first slice, the **Character Presentation Presenter** can inject `IGameplayStateService` and the configured pre-launch, running, and run-ended `GameplayStateId` assets. The important boundary is that the classifier and view never receive those state ids."

> **Dev:** "Should `CharacterPresentationModeClassifier` have an explicit priority order?"
> **Domain expert:** "Yes — terminal modes win first, then pre-launch idle, then run-active airborne and grounded locomotion, with idle as the fallback."

> **Dev:** "Should slide/run switching use simple thresholds?"
> **Domain expert:** "No — use hysteresis plus minimum locomotion mode duration so slope noise and contact-normal jitter do not cause animation flicker."

> **Dev:** "Where does slide/run hysteresis memory live?"
> **Domain expert:** "The presenter owns current mode and elapsed mode time, then passes `CurrentMode` and `CurrentModeElapsedSeconds` into the classifier. The classifier should not hide mutable timing state, and the view should not own this memory."

> **Dev:** "Can current mode and elapsed mode time carry across runs?"
> **Domain expert:** "No — scope mode memory to the current lifecycle. Reset it to `Idle` with `0f` elapsed when entering **Pre-Launch** or starting a new **Run**, then reset elapsed time normally whenever the classified mode changes."

> **Dev:** "Should the presenter use fixed time for mode timers?"
> **Domain expert:** "No — the presenter is an `ITickable` presentation coordinator, so use injected `ITime.DeltaTime`, clamped non-negative. Build classification input first, then reset elapsed on mode change or advance it when the mode stays stable."

> **Dev:** "Should one missed ground probe immediately switch the character to `Airborne`?"
> **Domain expert:** "No — debounce airborne entry with presenter-owned ungrounded elapsed time and `AirborneEnterDelaySeconds`. During the grace window, preserve current `Slide` or `Run` instead of falling to `Idle` or popping to `Airborne`."

> **Dev:** "Does returning to ground need a `Landing` mode?"
> **Domain expert:** "No — the first slice exits `Airborne` immediately once grounded and resumes normal grounded classification. Add landing dust or audio later as a **Character Presentation Cue** if needed."

> **Dev:** "Should `CoursePlanarSpeed` decide whether flat motion is running?"
> **Domain expert:** "No — keep unsigned `CoursePlanarSpeed` for playback scaling and add signed `CourseForwardSpeed` for run eligibility. Lateral-only flat movement can have planar speed, but it should not automatically become `Run`."

> **Dev:** "Where should `CourseForwardSpeed` be calculated?"
> **Domain expert:** "On `RunProgressFrameSnapshot`, beside `GetCoursePlanarSpeed(...)`. The snapshot owns the validated course axes; the character presenter should consume the resulting fact instead of duplicating vector math."

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
- "Level initial position" resolves to **Pre-Launch Rig Pose** when discussing same-session restart alignment, not to **Rest Point**.
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
- "Chunk" describes asset or prefab packaging, while **Run Course Section** describes gameplay-design purpose.
- "Progression zone", "upgrade zone", and "reach band" resolve to **Run Progression Band** when discussing distance-based upgrade and coin tuning.
- "Ladybug", "avatar", "skin", and "player model" resolve to **Character** when discussing appearance, animation, audio, or VFX.
- "Character anchor", "visual anchor", and "character alignment root" resolve to **Character Visual Anchor**, not the gameplay collider transform or **Band Center**.
- "`VisualRoot`" is not automatically the **Character Visual Anchor** if it carries the gameplay collider contract.
- The old collider-bearing "`VisualRoot`" resolves to **Launch Target Collider Root** after the cylinder renderer is removed or the object is split.
- "`LaunchTargetColliderRoot`", "`LaunchTargetCollider`", and "`GameplayColliderRoot`" resolve to **Launch Target Collider Root** unless a concrete implementation needs finer distinction.
- The first-slice launch-target hierarchy resolves to sibling roles: **Band Center**, **Launch Target Collider Root**, and **Character Visual Anchor** under the **Launch Target** root.
- "`ModelRoot`", "mesh root", "avatar root", and imported-model alignment root resolve to **Character Model Root**, not **Character Visual Anchor** or the `LadybugCharacter` prefab root.
- The `LadybugCharacter` prefab root is the stable presentation contract root; imported model local offsets belong under **Character Model Root**.
- "Slide", "run", "airborne", "victory", and "death" resolve to **Character Presentation Mode** when discussing appearance or animation, not to **Gameplay State**.
- "Held" resolves to `Idle` when no **Active Pull** is changing the character presentation; live pulled presentation resolves to **Pull Anticipation**.
- "Character config", "Ladybug config", and slide/run threshold data resolve to **Character Presentation Tuning** when discussing presentation-only values.
- "`ICharacterPresentationView`" and "`ICharacterPresentationTuning`" are separate read-only contracts; in the first slice both can resolve to the same passive `CharacterPresentationView` MonoBehaviour instance.
- "CharacterRunContactClassifier" and character-specific contact classification resolve to **Run Surface Context** plus **Character Presentation Mode** selection, not to **RunContactClassifier**.
- "Ground probe", "surface probe", and "slope source" resolve to **Run Surface Context Source** when discussing continuous presentation-facing grounded/slope facts.
- "Slope math" resolves to **Run Surface Slope Calculator**, not to a MonoBehaviour or Animator Controller transition.
- "Animator driver", "view Update", and "animation brain" resolve to **Character Presentation Presenter** when discussing presentation orchestration.
- "Presenter lifecycle" resolves to VContainer `IInitializable` + `ITickable` + `IDisposable` when the presenter owns event subscriptions.
- "Animator trigger", "launch trigger", "run-ended trigger", and "reset trigger" resolve to **Character Presentation Mode** changes in the first slice; future presentation-only one-shots resolve to **Character Presentation Cue**.
- "Death reason", "fail reason", and "run-ended animation variant" resolve to **Character Presentation Mode** in the first slice; add a future presentation term only if victory/defeat need multiple visual variants.
- "Run ended state" does not distinguish victory from defeat; use the accepted **Run Result** from **Run Result Notifier** for terminal character presentation.
- "Latest Run Result" means current-run accepted result only, not the last terminal result globally.
- "`GroundSlopeDegrees`", "`ForwardSlopeDegrees`", and raw slope parameters resolve to **Run Surface Context** or classifier diagnostics, not first-slice **Character Presentation Frame** or Animator parameters.
- "Downhill slope" in first-slice **Run Surface Context** resolves to `ForwardDownhillDegrees`, not generic surface tilt or sideways bank angle.
- "Slingshot push phase" resolves to **Launch Push**, not **Band Release Recoil** or a gameplay state.
- "Pull animation", "dragged animation", and "held while dragging" resolve to **Pull Anticipation**, not **Pull Hint** or **Touch Indicator**.
- "Slingshot presentation source", "pull presentation source", and "launch push source" resolve to **Slingshot Presentation Context Source**.
- "`SlingshotPresentationContext`" and "`SlingshotPresentationContextSource.Current`" resolve to **Slingshot Presentation Context**.
- "`ISlingshotActivePullNotifier`", "active-pull notifier", and "pull context notifier" resolve to **Slingshot Active Pull Notifier**.
- "publish", "published", and "publishing" in slingshot notifier context resolve to raising C# events, not `UnityEvent`, a global event bus, or a
  view call.
- "`ISlingshotCaptureLifecycleNotifier`", "capture lifecycle notifier", "slingshot reset notifier", and "recapture notifier" resolve to
  **Slingshot Capture Lifecycle Notifier**.
- "`ISlingshotPresentationContextSource`" resolves to **Slingshot Presentation Context Source**.
- "`ISlingshotPullOffsetNormalizer`" resolves to **Slingshot Pull Offset Normalizer**.
- "`SlingshotActivePullContext`", "active-pull context payload", and "pull presentation payload" resolve to **Slingshot Active Pull Context**.
- `SlingshotPullVisual` resolves to a slingshot visual payload, not **Slingshot Active Pull Context**.
- `ISlingshotView.ShowActivePull` resolves to slingshot visuals only, not the source of **Pull Anticipation** context for character presentation.
- "`SlingshotLaunchRequest`" resolves to the accepted **Launch** payload that **Slingshot Presentation Context Source** may map into frozen **Launch
  Push** presentation facts.
- "Launch presentation payload" is not a separate first-slice term unless `SlingshotLaunchRequest` creates real coupling pressure.
- "`LaunchApplied`" resolves to the character-presentation handoff edge from **Pull Anticipation** to **Launch Push** for an accepted **Launch**.
- "Normalized pull progress" resolves to **Normalized Pull** during **Active Pull** and **Normalized Launch Power** after **Launch**; neither term means elapsed animation time.
- "`NormalizedPower`" on `SlingshotLaunchRequest` resolves to **Normalized Launch Power** in presentation language.
- "`NormalizedPull`" on `SlingshotPullVisual` resolves to **Normalized Pull** in presentation language.
- "`NormalizedPullOffset`" resolves to **Normalized Pull Offset** in presentation language.
- "`NormalizedLaunchOffset`" resolves to **Normalized Launch Offset** in presentation language.
- "lateral offset normalizer" and "normalized lateral offset calculation" resolve to **Slingshot Pull Offset Normalizer**.
- Raw `PullOffset` resolves to slingshot geometry units and must be normalized before it is used as first-slice character presentation data.
- "`SlingshotStrength`" and "`SlingshotOffset`" are too generic for first-slice Animator parameters; use the distinct normalized pull and launch
  parameter names.
- "`LaunchPushDurationSeconds`" resolves to `LaunchPushMinimumSeconds` when discussing a minimum handoff guard for **Launch Push**.
- "`IsLaunchPushActive`" is too close to selected-mode language; use `HasLaunchPush` for classifier input that means a current accepted **Launch** has
  launch-push context available.
- "`Steer`" resolves to gameplay control language; use **Lateral Lean** only for visual left/right character presentation.
- "`IsGrounded`", "`VerticalSpeed`", and raw airborne physics values resolve to classifier inputs, not first-slice **Character Presentation Frame** values.
- "`MoveSpeed`", "`MoveSpeed01`", raw meters-per-second speed, and normalized speed resolve to presenter diagnostics or future feature-specific presentation terms, not first-slice **Character Presentation Frame** values.
- "`MoveSpeedMultiplier`" resolves to **Playback Speed Multiplier** when discussing first-slice animation playback.
- "`Animator.speed`" and global animation speed changes should not be used for first-slice **Playback Speed Multiplier**; use an Animator float parameter.
- "Run speed", "slide speed", and "clip speed" resolve to **Character Presentation Tuning** reference speeds when computing first-slice **Playback Speed Multiplier**.
- "`Airborne grace`" resolves to preserving the current `Slide` or `Run` mode plus its locomotion playback scaling, not to a separate mode, frame flag, or `Airborne` playback timing.
- "Root motion" resolves to visual-only authored skeleton/child motion in the first slice, not **Launch Target** Rigidbody, **Band Center**, collider, or camera-anchor movement.
- "Ladybug colliders", "ragdoll bodies", "hitboxes", and imported character triggers resolve to disabled or stripped vendor physics in the first-slice project-owned **Character** prefab, not active gameplay contacts.
- "Vendor prefab", "vendor prefab variant", and direct imported character prefab use resolve to source/reference content only, not the first-slice `LadybugCharacter.prefab` contract.
- "`Assets/Plugins/Ladybug`" resolves to third-party imported source content, not project-owned `LadybugCharacter.prefab` or `LadybugCharacter.controller`.
- "`Assets/Game/...`" owns project-specific character composition, Animator Controller, view wiring, presentation tuning, and alignment.
- "`LadyBug@TPose.FBX`", "`LadyBug@RunLoop.FBX`", and "`LadyBug@Slide.FBX`" resolve to **Source Character Assets**, not project-owned **Character** prefabs.
- "`AnimatorState`" fields and state-name dropdowns are not first-slice **Character Presentation View** data; add them only if code-driven `CrossFade`/`Play` or explicit state validation becomes a requirement.
- "Classifier input" resolves to **Character Presentation Classification Input**, not `CharacterPresentationModeInput`.
- "`GameplayStateId`" is not part of **Character Presentation Classification Input**; the presenter maps gameplay state into presentation-facing lifecycle facts first.
- "Character presentation lifecycle source" is not needed for generic gameplay-state mapping in the first slice; `CharacterPresentationPresenter` may map `IGameplayStateService` and configured state ids directly. Slingshot-owned pull/launch presentation facts are a separate source because they require slingshot validation, not raw gameplay state ids.
- "Classifier result" resolves to **Character Presentation Classification Result**, whose first-slice payload is `CharacterPresentationMode Mode`.
- "Classifier reason", "mode decision reason", and "why did mode change" resolve to optional diagnostics, not first-slice classification result data passed to the view or Animator.
- "Classification priority" resolves to terminal modes before **Pull Anticipation**, then **Launch Push**, then locomotion modes, not to Animator
  transition priority.
- "Slide/run threshold" resolves to hysteresis enter/exit thresholds plus minimum locomotion mode duration, not a single raw slope check.
- "`Airborne` entry" resolves to presentation debounce using `UngroundedElapsedSeconds` and `AirborneEnterDelaySeconds`, not a one-frame missing-ground probe.
- "`Airborne` exit" resolves to immediate grounded reclassification, not a first-slice `Landing` mode or cue.
- "`Landing`" resolves to a future presentation cue or visual detail if needed, not a first-slice **Character Presentation Mode**.
- "Current mode", "previous mode", and "mode timer" resolve to presenter-owned `CurrentMode` and `CurrentModeElapsedSeconds` facts passed into
  **Character Presentation Classification Input**, not hidden classifier state or view state.
- "Mode memory" means current-lifecycle presentation memory; it must not carry terminal or locomotion hysteresis from a previous **Run**.
- "Mode elapsed time" uses presentation/render-frame `DeltaTime`, not physics `FixedDeltaTime`.
- "`CoursePlanarSpeed`" resolves to unsigned **Course Planar Speed** for playback scaling, not signed forward progress.
- "`CourseForwardSpeed`" resolves to signed **Course Forward Speed** for run eligibility: positive forward, near zero lateral-only, negative backward.
- "`GetCourseForwardSpeed(...)`" belongs on `RunProgressFrameSnapshot`, not on a character presenter or view.
- "Character abilities" are not part of the first-slice **Character** concept; split them from presentation only when characters intentionally change gameplay behavior.
