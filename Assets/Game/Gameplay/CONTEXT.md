# Gameplay

Gameplay covers the runtime concepts that define how a run begins, progresses, ends, and returns to preparation.

## Language

**Run**:
A gameplay attempt that begins with **Launch** and ends by crash, lost momentum, falling out, or reaching **Run Finish**.
_Avoid_: Session, attempt

**Level Session**:
The currently loaded level instance that can contain multiple **Runs**.
_Avoid_: App session, Run

**Gameplay State**:
The single current phase that gates what gameplay systems may do.
_Avoid_: Tag, mode flag, animation state

**Run Preparation**:
The default **Gameplay State** where the player prepares for the next **Run**.
_Avoid_: Shop state, menu, pre-game

**Continue**:
The player confirmation action in **Run Preparation** that accepts the prepared setup and proceeds toward **Pre-Launch**.
_Avoid_: Launch, start run, acknowledge result

**Pre-Launch**:
The **Gameplay State** where the **Slingshot** can accept a **Pull** before a **Run** begins.
_Avoid_: Run Preparation, aiming mode

**Launch**:
The accepted release that starts a **Run**.
_Avoid_: Pull release, shot preview, impulse value

**Running**:
The **Gameplay State** where the **Run Body** is moving through the **Run Course**.
_Avoid_: Sliding state, animation run

**Run Body**:
The gameplay role of the controlled object while it travels through a **Run** after **Launch**.
_Avoid_: Launch Target, Character, player model

**Run Body Contact Collider**:
The gameplay-owned contact shape used by the **Run Body** for movement support and default run-ending contact authority during **Running**.
_Avoid_: Launch Target Collider Root, Animated Contact Sensor, Character collider, band silhouette, visual hitbox, single-collider assumption

**Run Steering Control**:
The player touch control used during **Running** to steer the **Run Body**.
_Avoid_: Virtual joystick, screen steering, Pull

**Run Steering Affordance**:
The visible presentation that helps the player perceive an active **Run Steering Control** gesture.
_Avoid_: movement joystick, 2D joystick, Pull indicator

**Run Steering Origin**:
The touch position that acts as neutral for one active **Run Steering Control** gesture.
_Avoid_: Screen center, joystick center, Pull Point

**Run Steering Range**:
The maximum physical touch displacement from the **Run Steering Origin** that represents full steering.
_Avoid_: Deadzone, screen half-width, raw pixel distance, invalid drag distance

**Run Steering Deadzone**:
The neutral portion of **Run Steering Range** near the **Run Steering Origin** that produces no steering.
_Avoid_: Screen center deadzone, input delay, response smoothing

**Run Steering Responsiveness**:
How quickly the **Run Body** responds to requested **Run Steering Control** during **Running**.
_Avoid_: Input polling rate, touch sampling rate

**Run Steering Frame**:
The gameplay orientation used by **Run Steering Control** to steer the **Run Body** during **Running**.
_Avoid_: Ground normal, support frame, locomotion frame, physics normal

**Run Air Steering Control**:
The weaker **Run Steering Control** behavior used while the **Run Body** is unsupported by a valid **Run Surface** during **Running**.
_Avoid_: Launch steering, air driving, Character Presentation Mode

**Run Steering Mode Selector**:
The **Running** control boundary that chooses between grounded **Run Steering Control** and **Run Air Steering Control** from current support and motion facts.
_Avoid_: Post-Launch Steering Gate, launch speed bypass, speed recovery, launch cap grace, no-takeoff timeout

**Launch Landing Stabilization**:
The post-launch contact correction that removes unwanted lift away from the first real **Run Surface** landing while preserving surface-tangent **Run Body** speed.
_Avoid_: downforce, speed recovery, bounce damping, slowdown

**Run Body Speed Sanity Guard**:
A defensive validation limit for impossible **Run Body** velocities during **Running**, not a player-facing movement cap.
_Avoid_: PlayerMaxSpeed, steering cap, balance speed limit

**Run Ended**:
The **Gameplay State** after one **Run Result** is accepted and before **Acknowledge Run Result** returns to **Run Preparation**.
_Avoid_: Game Over, RunEnd, auto-reset

**Acknowledge Run Result**:
The player confirmation action in **Run Ended** that accepts the shown **Run Result** and returns to **Run Preparation**.
_Avoid_: Continue, Start Run, auto-retry

**Run End Reason**:
The single cause that ended a **Run**.
_Avoid_: Death reason, fail type, result title

**Run Result**:
The immutable summary captured when a **Run** ends.
_Avoid_: Score state, outcome flag, live progress

**Run Result Notifier**:
The notification boundary that publishes an accepted **Run Result**.
_Avoid_: Gameplay State inference, result polling, UI event bus

**Run End Flow**:
The gameplay flow that accepts run-ending signals and produces one **Run Result**.
_Avoid_: Game over manager, result screen controller

**Best Run Distance**:
The largest accepted **Run Result** distance reached earlier in the current **Level Session**.
_Avoid_: Previous run distance, all-time best, saved best

**Run Distance Display**:
The UI-facing whole-meter presentation of distance reached by a **Run Result**.
_Avoid_: Raw meters, rounded-up progress, world position

**Run Air Time**:
The duration during **Running** when the **Run Body** is not supported by a **Run Surface**.
_Avoid_: Airborne animation, jump state, total scene time

**Run Ended UI**:
The UI surface shown during **Run Ended** to present the accepted **Run Result** and receive **Acknowledge Run Result**.
_Avoid_: Upgrade screen, runtime-built result screen, Run Preparation UI

**Run Ended UI Timing**:
The moment when **Run Ended UI** appears relative to the accepted **Run Result**.
_Avoid_: Animation wait, delayed reward reveal

**Run Reward Reveal**:
The presentation sequence that shows an already accepted **Run Reward Breakdown** during **Run Ended**.
_Avoid_: Currency grant, payout timing, wallet animation

**Run Reward Source Row**:
A displayed line in **Run Reward Reveal** for one aggregate **Run Reward Source**.
_Avoid_: Reward rule, reward event, currency grant

**Run Reward Source Order**:
The stable sequence in which **Run Reward Sources** are displayed during **Run Reward Reveal**.
_Avoid_: UI label sort, scene hierarchy order, source-specific view logic

**Run Reward Reveal Fast-Forward**:
The player input during **Run Reward Reveal** that immediately shows the fully revealed result without acknowledging it.
_Avoid_: Acknowledge Run Result, skip reward, auto-continue

**Run Ended Acknowledge Guard**:
The input gate that prevents **Acknowledge Run Result** until **Run Reward Reveal** is complete.
_Avoid_: Auto-continue delay, global input debounce

**Run End Pose Lock**:
The hold applied to the **Run Body** during **Run Ended** so it stays at the accepted **Run Result** pose.
_Avoid_: Visual-only freeze, continued physics drift

**Run Progress Frame**:
The course reference that defines forward downhill progress, lateral direction, and up direction during a **Run**.
_Avoid_: World forward, launch direction, camera forward

**Run Contact Category**:
The gameplay meaning of a contact or region encountered during a **Run**.
_Avoid_: Collider type, physics layer, tag

**Animated Contact Sensor**:
A trigger-only **Run** contact role whose pose follows copied **Character** body-part pose to report body-part-specific contact facts.
_Avoid_: Run Body collider, Launch Target collider, Character collider, movement collider, physics hitbox

**Animated Contact Sensor Physics Root**:
The gameplay-owned kinematic Rigidbody root that hosts copied-pose **Animated Contact Sensor** trigger Colliders for Unity trigger delivery.
_Avoid_: Character Visual Anchor Rigidbody, pickup Rigidbody, animated bone Rigidbody, pickup-side physics owner

**Animated Contact Sensor Pose Sync**:
The explicit update boundary that copies finalized animated **Character** body-part poses into **Animated Contact Sensor Physics Root** sensor transforms.
_Avoid_: FixedUpdate movement, Animator, Character Visual Follower, Physics.SyncTransforms, hierarchy parenting side effect

**Player Body Part Layer**:
The dedicated Unity physics layer for pose-following **Animated Contact Sensors** on the player character.
_Avoid_: Player Layer, Player Tag, Pickup Layer, body-part enum

**Animated Sensor Identity**:
The authoring identity attached to an **Animated Contact Sensor** report so consumers can distinguish which sensor produced the contact.
_Avoid_: Player Tag, hardcoded body-part enum, collider instance id, gameplay tag

**Run Obstacle Sensor Source**:
The dedicated aggregate source component used when a feature exposes configured obstacle-eligible **Animated Contact Sensor** reports as body-part **Run Obstacle** notices.
_Avoid_: Pickup Sensor Source, Run Body Contact Collider, Run Contact Classifier, Run End Flow, every TriggerNotifier, Character hierarchy scan

**Run Surface**:
A **Run Contact Category** that allows traversal without ending the **Run**.
_Avoid_: Ground, floor, ramp

**Run Surface Contact Slowdown**:
The ordinary loss of **Run Body** speed caused by contact with a **Run Surface**.
_Avoid_: steering speed cap, hidden drag, PlayerMaxSpeed

**Run Obstacle**:
A **Run Contact Category** that can end a **Run** through **Obstacle Impact**.
_Avoid_: Wall tag, hard mesh, visual prop

**Obstacle Impact**:
A **Run Obstacle** contact strong enough to end a **Run**.
_Avoid_: Any collision, light scrape, speed loss

**Lost Momentum**:
A **Run End Reason** caused by sustained lack of meaningful progress.
_Avoid_: Stopped, idle, one-frame low speed

**Run Safety Net**:
A **Run Contact Category** that ends a **Run** when the **Run Body** falls below the playable course.
_Avoid_: Boundary, guard rail, invisible wall

**Run Finish**:
A **Run Contact Category** that ends a **Run** by completion.
_Avoid_: Goal trigger, checkpoint, finish marker

**Run Preparation Camera**:
The camera behavior that presents the character before **Pre-Launch**.
_Avoid_: Idle camera, shop camera

**Pre-Launch Camera**:
The camera behavior that frames the **Slingshot** before **Launch**.
_Avoid_: Run Preparation Camera, aiming overlay

**Run Camera**:
The camera behavior that follows the **Run Body** during **Running** and **Run Ended**.
_Avoid_: Main camera, level camera

**Run Camera Anchor**:
The camera-facing reference point derived from the **Run Body** for **Run Camera** framing.
_Avoid_: Character pivot, camera target, follow transform

**Gameplay Pickups Scene Composition Installer**:
The scene composition boundary that owns scene-authored pickup references and pickup contact-source wiring for a **Gameplay** scene.
_Avoid_: GameplayLifetimeScope pickup fields, Pickup Collection Controller, Character hierarchy scan

## Relationships

- A **Level Session** may contain multiple **Runs**.
- A **Run** is governed by one current **Gameplay State**.
- **Run Preparation** occurs before **Pre-Launch**.
- **Continue** occurs during **Run Preparation**.
- **Pre-Launch** accepts a **Pull** and may lead to **Launch**.
- **Launch** starts **Running**.
- The slingshot-facing **Launch Target** becomes the **Run Body** when **Launch** starts **Running**.
- **Run Body Contact Collider** belongs to the **Run Body**, not the **Character** or **Character Visual Anchor**.
- **Run Body Contact Collider** provides movement support, **Run Surface** contact, **Run Finish**, **Run Safety Net**, and default **Obstacle Impact** contact authority.
- **Run Body Contact Collider** is distinct from **Launch Target Collider Root** and **Animated Contact Sensors**.
- **Run Body Contact Collider** is a gameplay role; it may be represented by one contact shape or by a compound contact shape.
- **Run Body Contact Collider** should not be driven by current **Character** animation; animation-following body-part contacts belong to **Animated Contact Sensors**.
- **Run Steering Control** belongs to **Running**, not **Pre-Launch**.
- **Run Steering Control** steers the **Run Body** during **Running**.
- **Run Steering Affordance** presents **Run Steering Control**; it does not redefine movement axes or steering authority.
- **Run Steering Affordance** may show **Run Steering Origin**, active displacement, **Run Steering Range**, and **Run Steering Deadzone**.
- **Run Steering Affordance** presents the same **Run Steering Range** and **Run Steering Deadzone** used by **Run Steering Control**.
- **Run Steering Affordance** follows the **Run Steering Control** lifecycle during **Running**.
- **Run Steering Affordance** appears from the active **Run Steering Origin**, not from a fixed screen-side home position.
- **Run Steering Affordance** is presented in screen space from touch positions, not as a world-space visual tied to the **Run Body**.
- **Run Steering Affordance** is visible only while a **Run Steering Control** gesture is active.
- **Run Steering Affordance** presents horizontal **Run Steering Control** displacement; it does not present vertical displacement as steering.
- **Run Steering Affordance** presents active displacement with a knob that moves horizontally, not with a continuous horizontal rail or track.
- **Run Steering Affordance** may hint at **Run Steering Range** endpoints without rendering a continuous movement track.
- **Run Steering Affordance** knob directly follows current horizontal **Run Steering Control** displacement while the gesture is active.
- **Run Steering Affordance** animation does not smooth or lag active **Run Steering Control** displacement.
- **Run Steering Affordance** knob appears at the current horizontal **Run Steering Control** displacement when the gesture starts.
- **Run Steering Affordance** appearance animation may affect opacity or scale, but not the initial active knob position.
- **Run Steering Affordance** disappears from the final knob position when the **Run Steering Control** gesture ends.
- **Run Steering Affordance** gesture-end animation may affect opacity or scale, but does not return the knob to **Run Steering Origin**.
- **Run Steering Affordance** clamps visible horizontal displacement at **Run Steering Range** when full steering is reached.
- **Run Steering Affordance** may show **Run Steering Deadzone** as a neutral area around **Run Steering Origin**.
- **Run Steering Affordance** may show horizontal touch movement inside **Run Steering Deadzone** while **Run Steering Control** remains neutral.
- **Run Steering Affordance** observes **Run Steering Control** input state; it is not an input-emitting UI control.
- **Run Steering Affordance** is not interactive UI and does not consume touches or block **Run Steering Control**.
- **Run Steering Control** does not depend on **Run Steering Affordance** being present.
- **Run Steering Control** may begin from any touch during **Running**.
- **Run Steering Control** does not begin from a touch that begins on interactive UI during **Running**.
- **Run Steering Affordance** does not introduce a left-side or screen-region start requirement for **Run Steering Control**.
- One active **Run Steering Control** gesture has one **Run Steering Origin**.
- Only one **Run Steering Control** gesture is active at a time.
- Additional touches do not change the active **Run Steering Control** gesture.
- The active **Run Steering Control** gesture ends when its touch ends.
- Interactive UI during **Running** does not take over an already active **Run Steering Control** gesture.
- **Run Steering Range** defines full steering from the **Run Steering Origin** as a physical touch distance.
- **Run Steering Range** maps physical horizontal displacement directly to steering amount until full steering is reached.
- **Run Steering Range** remains a strict physical distance when **Run Steering Origin** is near a screen edge.
- **Run Steering Affordance** preserves the true **Run Steering Origin** and **Run Steering Range** near screen edges; it does not shift inward to stay fully visible.
- **Run Steering Deadzone** ignores a small initial portion of **Run Steering Range** near the **Run Steering Origin**.
- **Run Steering Deadzone** does not reduce full steering; the remaining **Run Steering Range** still reaches full steering.
- **Run Steering Responsiveness** affects how quickly the **Run Body** responds to requested **Run Steering Control**.
- **Run Steering Responsiveness** does not affect how often input is sampled.
- **Run Steering Frame** defines the orientation used by **Run Steering Control**.
- **Run Steering Frame** is a gameplay control concept, not a **Run Progress Frame** metric reference.
- **Run Steering Frame** may be derived from **Run Surface** traversal facts without being identical to a raw contact normal.
- **Run Steering Control** uses horizontal displacement; vertical displacement has no steering meaning.
- **Run Steering Control** preserves player-facing **Run Body** speed while changing steering direction.
- **Run Air Steering Control** belongs to **Running**, not **Slingshot** or **Character Presentation**.
- **Run Air Steering Control** applies when the **Run Body** is unsupported by a valid **Run Surface**.
- **Run Air Steering Control** applies during any unsupported **Running** motion before **Run Ended**.
- **Run Air Steering Control** may also apply during stale post-launch support samples when the **Run Body** is moving away from the reported **Run Surface**.
- **Run Air Steering Control** uses the **Run Steering Frame** rather than raw support normals or **Character Presentation Mode**.
- **Run Air Steering Control** uses the active **Run Steering Control** gesture with separately tuned, weaker turn authority than grounded steering.
- **Run Air Steering Control** shares **Run Steering Responsiveness** with grounded **Run Steering Control**.
- **Run Air Steering Control** does nothing without an active **Run Steering Control** gesture.
- **Run Air Steering Control** rotates current air-frame planar velocity direction while preserving planar speed and vertical velocity.
- **Run Air Steering Control** rotates the **Run Body** to face the steered air-frame planar velocity when steering is applied.
- **Run Air Steering Control** must not add forward speed, reduce launch speed, apply player-facing speed caps, or drive toward a target velocity.
- **Run Air Steering Control** does not add pitch, roll, banking, or **Character Presentation Mode** rotation.
- **Run Air Steering Control** is not selected from **Launch Flight**, **Airborne**, or any other **Character Presentation Mode**.
- **Run Air Steering Control** stops when **Running** ends.
- **Run Steering Mode Selector** chooses between grounded **Run Steering Control** and **Run Air Steering Control** during **Running**.
- **Run Steering Mode Selector** selects steering behavior; it does not rotate velocity, write **Run Body** pose, or change **Run Body** speed.
- **Run Steering Mode Selector** has no blocked or inactive selection; low speed, invalid velocity, and missing input are separate **Run Steering Control** gates.
- **Run Steering Mode Selector** selects **Run Air Steering Control** when current support is not a valid **Run Surface**.
- **Run Steering Mode Selector** selects **Run Air Steering Control** when the **Run Body** is moving away from a reported **Run Surface** above the accepted lift tolerance.
- **Run Steering Mode Selector** may delay grounded steering selection after **Launch**, but it must not change **Run Body** speed.
- **Run Steering Mode Selector** must not disable **Run Air Steering Control** during unsupported post-launch motion.
- **Run Steering Mode Selector** selects grounded steering on the first valid post-launch **Run Surface** landing.
- **Run Steering Mode Selector** selects grounded steering immediately for a weak grounded **Launch** when current support is a valid **Run Surface** and **Run Body** velocity has no positive surface-normal lift.
- **Run Steering Mode Selector** must not use a no-takeoff timeout to enable grounded steering.
- **Launch Landing Stabilization** can remove positive surface-normal lift after first real post-launch landing, but it must preserve surface-tangent **Run Body** speed.
- **Launch Landing Stabilization** may correct post-launch lift before grounded **Run Steering Control** resumes.
- First-pass **Animated Contact Sensors** report pickup contacts only.
- Body-part-specific interaction or **Run Obstacle** contact facts require an explicit feature policy beyond first-pass pickup collection.
- **Animated Contact Sensor** obstacle notices should reuse existing **Run Contact Category** authoring; they should not introduce sensor-specific obstacle tags or layers.
- **Animated Contact Sensor** obstacle notices should report contacted **Run Obstacles** through `RunContactCategory.Obstacle` authoring.
- **Animated Contact Sensor** obstacle notices should resolve `RunContact` from the exact contacted Collider GameObject when the obstacle notice feature is enabled.
- **Animated Contact Sensor** obstacle notices should not use parent `RunContact` lookup unless the collider-local **Run Contact Category** authoring contract is explicitly changed.
- Every **Run Obstacle** collider that should be reportable by **Animated Contact Sensors** should carry its own collider-local `RunContactCategory.Obstacle`.
- **Animated Contact Sensors** may copy animated **Character** body-part poses, but gameplay consumes aggregated sensor reports rather than raw character hierarchy objects.
- **Animated Contact Sensors** stay trigger-only and should not add per-sensor Rigidbody ownership to animated **Character** bones or **Character Visual Anchor**.
- **Animated Contact Sensor Physics Root** owns the single gameplay-side kinematic Rigidbody used by copied-pose **Animated Contact Sensors** for first-pass Unity trigger delivery.
- **Animated Contact Sensor Physics Root** belongs to the gameplay-owned Player / **Run Body** side, not **Character Visual Anchor**.
- **Animated Contact Sensor Physics Root** should be a separate gameplay-owned sibling/root driven explicitly, not a child of the **Run Body** Rigidbody root and not under **Character Visual Anchor**.
- **Animated Contact Sensor Physics Root** copies animated body-part poses from the **Character** hierarchy without making **Character Visual Anchor** gameplay physics authority.
- **Animated Contact Sensor Pose Sync** copies body-part poses after the **Character** presentation pose is finalized for the rendered frame.
- **Animated Contact Sensor Pose Sync** should be explicitly ordered after **Character Visual Follower** and animation pose evaluation; it must not rely on Unity transform parenting to inherit the correct timing.
- **Animated Contact Sensor Pose Sync** should not run from the **Run Body** `FixedUpdate` movement path because `FixedUpdate` can run zero or multiple times per rendered frame while the copied pose depends on render-frame presentation state.
- First-pass **Animated Contact Sensor Pose Sync** should let Unity consume copied sensor transforms on the next physics simulation step for trigger delivery.
- **Animated Contact Sensor Pose Sync** should not call `Physics.SyncTransforms` every rendered frame by default; same-frame physics queries after pose copy require an explicit measured feature policy.
- **Animated Contact Sensor Pose Sync** assumes the **Character** Animator update mode is part of the authoring contract; `Animate Physics` requires an explicit timing policy instead of silently reusing render-frame sync assumptions.
- Sensor-driven pickup collection should use **Animated Contact Sensor Physics Root** for trigger delivery, not pickup-side Rigidbodies or Rigidbody ownership on **Character Visual Anchor**.
- **Animated Contact Sensors** should use the **Player Body Part Layer** in physics authoring, named `PlayerBodyPart` in the first implementation.
- **Animated Contact Sensors** should not use Player Tag in the first implementation; source membership gives them gameplay meaning.
- Player Layer and Player Tag stay reserved for **Run Body** and **Launch Target** authority, not pose-following body-part sensors.
- The **Player Body Part Layer** should not interact with Player Layer in the Unity Layer Collision Matrix unless an explicit self-contact feature requires it.
- **Animated Contact Sensors** should not report contacts with **Run Body Contact Collider** or **Launch Target Collider Root** through ordinary layer self-interaction.
- Player self-contact between **Animated Contact Sensors** and **Run Body** authority should be opt-in feature policy, not default physics authoring.
- First-pass **Player Body Part Layer** interaction should be limited to **Pickup Layer**; it should not interact with `CameraObstacle` until body-part obstacle notices become an explicit feature.
- **Animated Contact Sensor** reports may carry **Animated Sensor Identity** derived from GameObject name or hierarchy path in the first implementation.
- **Animated Sensor Identity** should not be hardcoded as a gameplay body-part enum in the first implementation.
- Future stable **Animated Sensor Identity** should use an asset-backed id before sensor contracts need to survive model hierarchy or object-name changes.
- Pickup-eligible **Animated Contact Sensor** configuration should live in a dedicated aggregate source, not in pickup collection rules or unchecked visual hierarchy scans.
- Pickup collection contacts should originate from the pickup-eligible **Animated Contact Sensor** source, which resolves contacted **Pickups** before collection rules run.
- Body-part **Run Obstacle** notices should originate from **Run Obstacle Sensor Source**, not from **Pickup Sensor Source**.
- **Run Obstacle Sensor Source** is the configured source of obstacle-eligible **Animated Contact Sensors** accepted by body-part **Run Obstacle** notice consumers when that feature is enabled.
- **Pickup Sensor Source** should live on the gameplay-owned Player / **Run Body** side, not under **Character Visual Anchor**; **Run Obstacle Sensor Source** should follow the same ownership rule if body-part obstacle notices are enabled.
- **Pickup Sensor Source** should reference **Animated Contact Sensors** hosted by **Animated Contact Sensor Physics Root**; those sensors copy pose from **Character Visual Anchor** body-part objects.
- **Run Obstacle Sensor Source** should follow the same **Animated Contact Sensor Physics Root** reference policy if body-part obstacle notices are enabled.
- Source component placement should follow ownership of gameplay eligibility, target resolution, and typed contact events, not the visual pose provider.
- **Run Obstacle Sensor Source** owns obstacle-eligible sensor configuration through explicit serialized `TriggerNotifier` entries when body-part obstacle notices are enabled.
- First-pass **Pickup Sensor Source** configuration should use explicit serialized `TriggerNotifier` entries, not broad runtime hierarchy scans.
- **Run Obstacle Sensor Source** should also use explicit serialized `TriggerNotifier` entries if body-part obstacle notices are enabled later.
- An explicit first-pass sensor entry should reference exactly one `TriggerNotifier` and derive **Animated Sensor Identity** from that notifier's GameObject name or hierarchy path.
- Sensor sources should not discover eligibility by scanning every `TriggerNotifier` under **Character Visual Anchor**.
- A separate `AnimatedContactSensor` component should not be introduced unless source entries stop carrying identity, validation, and authoring clearly.
- Setup validation should flag missing sensor references and duplicate sensor entries inside one source.
- **Pickup Sensor Source** should own its raw `TriggerNotifier` subscription lifecycle; **Run Obstacle Sensor Source** should own the same lifecycle if body-part obstacle notices are enabled.
- Sensor sources should subscribe to configured `TriggerNotifier` entries when enabled and unsubscribe when disabled.
- Source-owned subscriptions should avoid duplicate or dangling raw trigger callbacks across disable/enable cycles and scene reset.
- `TriggerNotifier` should remain a dumb Unity adapter; it should not know source eligibility, target resolution, or gameplay meaning.
- Sensor source target-resolution misses should be treated as non-matching contacts, not authoring errors.
- **Pickup Sensor Source** should emit no **Pickup Contact** when the contacted collider does not resolve to a **Pickup**.
- **Run Obstacle Sensor Source** should emit no body-part **Run Obstacle** notice when the contacted collider does not resolve to collider-local `RunContactCategory.Obstacle`.
- Runtime non-matching contacts should not log warnings by default; setup validation owns missing source entries and duplicate configured sensors.
- **Pickup Sensor Source** should not own pickup reward idempotency when multiple eligible sensors touch the same **Pickup**.
- **Pickup Sensor Source** may emit multiple **Pickup Contact** facts for the same **Pickup** when multiple eligible sensors contact it.
- **Pickup Collection Controller** and **Level Pickup State** own duplicate pickup prevention and accepted collection idempotency.
- **Pickup Sensor Source** should be state-agnostic contact resolution; it should not check **Gameplay State** before emitting **Pickup Contact** facts.
- **Pickup Collection Controller** owns **Gameplay State** gating for pickup collection and decides whether a **Pickup Contact** can become a **Pickup Collection Event**.
- First-pass **Pickup Sensor Source** should report new trigger enters only after its source subscriptions are active.
- First-pass **Pickup Sensor Source** should not query or replay sensors that are already overlapping **Pickups** when the source becomes enabled.
- Source/component enable order should make **Pickup Sensor Source** active before pickup collection contacts matter.
- A first-pass **Pickup Contact** should carry lightweight contact facts: **Pickup**, **Animated Sensor Identity**, sensor/source reference, and contacted Collider.
- A **Pickup Contact** should not carry reward, **Gameplay State**, collection result, accepted/rejected status, or currency grant details.
- Reward and collection outcome belong to **Pickup Collection Controller**, **Level Pickup State**, and **Pickup Definition**, not **Pickup Sensor Source**.
- **Run Obstacle Sensor Source** should resolve the contacted collider to collider-local `RunContactCategory.Obstacle` before emitting a body-part **Run Obstacle** notice.
- **Run Obstacle Sensor Source** should ignore **Run Surface**, **Run Finish**, and **Run Safety Net** contacts when body-part obstacle notices are enabled.
- **Run Obstacle Sensor Source** should not collect pickups, own movement support, classify **Obstacle Impact**, or end the **Run** by itself.
- **Run Obstacle Sensor Source** should be state-agnostic contact resolution; it should not check **Gameplay State** before emitting body-part **Run Obstacle** notices.
- Consumers of **Run Obstacle Sensor Source** own **Gameplay State** gating and decide whether an obstacle notice matters during **Running**, **Run Ended**, or another state.
- **Run Obstacle Sensor Source** should emit enter-only body-part **Run Obstacle** notices when first enabled, matching `TriggerNotifier.TriggerEntered`.
- **Run Obstacle Sensor Source** should report new trigger enters only after its source subscriptions are active.
- **Run Obstacle Sensor Source** should not query or replay sensors that are already overlapping **Run Obstacles** when the source becomes enabled.
- Source/component enable order should make **Run Obstacle Sensor Source** active before body-part obstacle contacts matter if that feature is enabled.
- **Run Obstacle Sensor Source** should emit each resolved body-part **Run Obstacle** notice when multiple eligible sensors touch the same **Run Obstacle**.
- Consumers of **Run Obstacle Sensor Source** own deduplication, cooldowns, and meaning for repeated or multi-body-part **Run Obstacle** notices.
- **Run Obstacle Sensor Source** should not collapse hand/head/body obstacle notices into one obstacle-level notice.
- `TriggerNotifier.TriggerEntered` is an internal Unity adapter input for sensor sources, not the gameplay contact API for consumers.
- **Pickup Sensor Source** should expose typed, resolved pickup reports instead of leaking raw `TriggerNotifier` callbacks; **Run Obstacle Sensor Source** should do the same for body-part obstacle notices if enabled.
- Consumers should subscribe to source-level **Pickup Contact** or enabled body-part **Run Obstacle** notices, not to raw sensor `TriggerNotifier` components directly.
- **Run Obstacle Sensor Source** should not track **Run Obstacle** stay/exit contact lifetime until a feature needs sustained body-part contact state.
- **Run Obstacle Sensor Source** should preserve enough **Animated Sensor Identity** context to debug which configured sensor touched the **Run Obstacle**.
- A body-part **Run Obstacle** notice should carry lightweight trigger facts: **Animated Sensor Identity**, sensor/source reference, contacted Collider, and resolved `RunContact` / `RunContactCategory.Obstacle`.
- A body-part **Run Obstacle** notice should not carry physics impact details such as contact point, contact normal, relative velocity, or impact speed.
- Physics impact details belong to the **Run Body Contact Collider** collision path and **Obstacle Impact** classification, not trigger-only **Animated Contact Sensor** notices.
- Pickup collection and body-part **Run Obstacle** notices should not share one source because they resolve contacted colliders through different gameplay contracts.
- The same **Animated Contact Sensor** may be configured in both **Pickup Sensor Source** and **Run Obstacle Sensor Source** only if a later feature makes it eligible for both purposes.
- Sensor source membership means purpose eligibility, not ownership of the **Animated Contact Sensor**.
- Each sensor source applies its own target resolution and filtering contract to the same low-level trigger report.
- Duplicate registration of the same **Animated Contact Sensor** inside one source should be rejected or flagged by setup validation to avoid duplicate notices.
- **Animated Contact Sensors** do not replace the **Run Body**, movement support, **Launch Target Collider Root**, band contact, **Run Finish**, **Run Safety Net**, or core run-ending contact authority.
- **Animated Contact Sensor** **Run Obstacle** reports do not become **Obstacle Impact** by default.
- **Obstacle Impact** is owned by the **Run Body** obstacle contact path unless an explicit feature policy converts selected **Animated Contact Sensor** reports into a run-ending signal.
- **Run Finish** and **Run Safety Net** contact should come from the **Run Body** contact path, not from **Animated Contact Sensors**.
- **Animated Contact Sensor** pickup reports should reach **Pickup Collection Controller** through a configured pickup-eligible sensor source, not through **Player Tag** or raw character hierarchy `TriggerNotifier` scans.
- **Pickup Sensor Source** determines sensor meaning from configured source entries, not from Player Tag; **Run Obstacle Sensor Source** should follow the same rule if body-part obstacle notices are enabled.
- **Gameplay Pickups Scene Composition Installer** owns scene-specific pickup authoring, pickup source registration, and pickup setup validation.
- **Gameplay Pickups Scene Composition Installer** is parallel to **GameplayPhysicsSceneCompositionMonoInstaller**: it owns scene references, registers runtime sources, and validates scene setup.
- **GameplayLifetimeScope** remains the root gameplay composition scope but should not own scene-authored pickup lists or player pickup contact collider lists.
- First-pass **Gameplay Pickups Scene Composition Installer** should use explicit scene-authored **Pickup** references instead of broad runtime scene pickup discovery.
- First-pass **Gameplay Pickups Scene Composition Installer** may use an empty explicit **Pickup** set for a level authored without pickups.
- No-pickup levels should not require **Pickup Sensor Source** authoring; pickup sensor setup may be omitted or no-op when the explicit **Pickup** set is empty.
- No-pickup levels should keep **Pickup Collection Controller** and pickup collection notification composition available through empty/no-op pickup sources.
- No-pickup levels may keep authored **Pickup Sensor Source** entries without warning or failure; the explicit **Pickup** set controls whether pickup-bearing validation runs.
- Pickup levels with at least one explicit **Pickup** reference should require and validate **Pickup Sensor Source** authoring.
- Pickup levels with at least one explicit **Pickup** reference should reject an empty **Pickup Sensor Source** or zero configured pickup sensor entries.
- Pickup-bearing validation should reject configured **Pickup Sensor Source** entries without an enabled trigger Collider on the same `TriggerNotifier` GameObject and on `PlayerBodyPart` layer.
- Pickup-bearing validation should not require Player Tag or a Rigidbody on configured **Pickup Sensor Source** sensor entries.
- Pickup-bearing validation should reject configured **Pickup Sensor Source** entries whose `TriggerNotifier` component is disabled or whose sensor GameObject is inactive in hierarchy.
- Disabled or inactive authored **Pickup Sensor Source** entries should be tolerated without warning only when the explicit **Pickup** set is empty.
- `PlayerBodyPart` to **Pickup Layer** collision-matrix validation should run only when the explicit **Pickup** set is non-empty.
- No-pickup levels should not fail pickup physics validation because `PlayerBodyPart` and **Pickup Layer** interaction is absent.
- Pickup auto-discovery should be treated as optional editor authoring support, not as the first-pass runtime composition source of truth.
- Sensor-driven pickup collection should require exactly one enabled trigger Collider under each explicit **Pickup** hierarchy on **Pickup Layer**.
- The single pickup-side trigger Collider for an explicit **Pickup** should stay under that **Pickup** hierarchy, be enabled, be marked trigger, and be on **Pickup Layer**.
- The single pickup-side trigger Collider for an explicit **Pickup** may be any Unity Collider subtype; pickup collection should not whitelist primitive Collider classes.
- Single-collider pickup validation should count only Colliders on **Pickup Layer** under the explicit **Pickup** hierarchy.
- Colliders under an explicit **Pickup** hierarchy on non-**Pickup Layer** layers should not participate in pickup collection single-collider validation.
- Additional Colliders on **Pickup Layer** under one explicit **Pickup** hierarchy should be invalid first-pass pickup authoring, not merged into one volume.
- Sensor-driven pickup collection should not require pickup-side Rigidbody authoring; trigger delivery is supplied by **Animated Contact Sensor Physics Root**.
- The pickup-side trigger Collider remains the pickup target volume, not the trigger-delivery Rigidbody owner.
- Sensor-driven pickup collection should not require a pickup-side `TriggerNotifier`; pickup-side `TriggerNotifier` authoring is legacy compatibility.
- First-pass **Gameplay Pickups Scene Composition Installer** should reject null, duplicate, or inactive explicit **Pickup** references during setup validation when references are present.
- Initial pickup unavailability should require an explicit future policy, not a disabled referenced **Pickup**.
- **Gameplay Pickups Scene Composition Installer** should not own pickup collection rules, rewards, **Gameplay State** gating, or **Level Pickup State** idempotency.
- Ordinary **Run Body** slowdown comes from **Run Surface Contact Slowdown**, not from **Run Steering Control**.
- **Run Surface Contact Slowdown** should be tuned through authored **Run Surface** behavior before adding a separate gameplay resistance owner.
- **Run Body Speed Sanity Guard** catches impossible velocities without shaping normal launch distance or sliding feel.
- **Run Body Speed Sanity Guard** must be unreachable by normal **Launch**, upgrades, and authored **Run Surface** traversal.
- **Run Body Speed Sanity Guard** is allowed to log or clamp impossible velocities, not to define expected run distance.
- Removing player-facing **Run Steering Control** speed caps removes the need for launch speed bypass or speed recovery.
- **Running** ends with one **Run Result**.
- **Run End Flow** produces one accepted **Run Result** per ended **Run**.
- **Run Result** may include a **Run Reward Breakdown** that explains its run-earned currency.
- **Run Ended** persists until **Acknowledge Run Result**.
- **Run Reward Reveal** does not change the accepted **Run Result** or grant currency over time.
- **Run Reward Source Rows** render **Run Reward Sources** without knowing source-specific reward rules.
- **Run Reward Source Rows** follow **Run Reward Source Order** supplied before reaching the view.
- **Run Reward Reveal Fast-Forward** completes **Run Reward Reveal** but does not perform **Acknowledge Run Result**.
- **Acknowledge Run Result** returns to **Run Preparation**.
- **Run Result** has one **Run End Reason**.
- **Best Run Distance** belongs to the current **Level Session**.
- **Run Distance Display** presents **Run Result** distance without changing it.
- **Run Air Time** is measured from traversal facts, not from **Character Presentation Mode**.
- **Run Surface** does not end a **Run**.
- **Run Obstacle**, **Run Safety Net**, and **Run Finish** can end a **Run**.
- **Run Progress Frame** owns progress, lateral, and up directions for run metrics.
- **Run Preparation Camera**, **Pre-Launch Camera**, and **Run Camera** belong to different **Gameplay States**.

## Example dialogue

> **Dev:** "Can a finish contact and obstacle contact both publish separate results?"
> **Domain expert:** "No - **Run End Flow** accepts one ending and produces one **Run Result**."

> **Dev:** "Does **Run Ended** automatically reset the player?"
> **Domain expert:** "No - **Run Ended** holds the accepted result until **Acknowledge Run Result** returns to **Run Preparation**."

> **Dev:** "Does the result animation pay coins as each row appears?"
> **Domain expert:** "No - **Run Reward Reveal** explains the already accepted **Run Result**."

> **Dev:** "Does a result row know how distance becomes coins?"
> **Domain expert:** "No - a **Run Reward Source Row** only displays a **Run Reward Source**."

> **Dev:** "Should the result view sort reward rows by label?"
> **Domain expert:** "No - it follows **Run Reward Source Order**."

> **Dev:** "If the player taps while reward rows are still revealing, should that continue to preparation?"
> **Domain expert:** "No - that is **Run Reward Reveal Fast-Forward**; the next tap can **Acknowledge Run Result**."

> **Dev:** "Is **Continue** the same input as **Acknowledge Run Result**?"
> **Domain expert:** "No - **Continue** leaves **Run Preparation**, while **Acknowledge Run Result** leaves **Run Ended**."

> **Dev:** "Is the **Run Steering Control** the same thing as a **Pull**?"
> **Domain expert:** "No - a **Pull** prepares **Launch** during **Pre-Launch**, while **Run Steering Control** steers the **Run Body** during **Running**."

> **Dev:** "Are we adding a virtual joystick Vector2?"
> **Domain expert:** "No - add a **Run Steering Affordance** for existing horizontal **Run Steering Control**; vertical displacement still has no steering meaning."

> **Dev:** "Should the **Run Steering Affordance** use a fixed left-corner joystick base?"
> **Domain expert:** "No - it appears from the active **Run Steering Origin** because **Run Steering Control** begins from the player's touch."

> **Dev:** "Should the **Run Steering Affordance** use a UI joystick control to send input?"
> **Domain expert:** "No - **Run Steering Control** stays owned by the existing project input path; **Run Steering Affordance** only presents it."

> **Dev:** "Since the **Run Steering Affordance** is serialized UI, should its graphics block touches like other UI?"
> **Domain expert:** "No - it is not interactive UI; it presents **Run Steering Control** without consuming touches."

> **Dev:** "If the **Run Steering Affordance** view is missing, should **Run Steering Control** stop working?"
> **Domain expert:** "No - **Run Steering Control** stays usable; the missing affordance is a presentation/setup issue."

> **Dev:** "Should the **Run Steering Affordance** require touches to start on the left side of the screen?"
> **Domain expert:** "No - **Run Steering Control** may begin from any touch during **Running**; the affordance appears from that touch."

> **Dev:** "Should the **Run Steering Affordance** stay visible during **Running** before the player touches?"
> **Domain expert:** "No - it appears only while an active **Run Steering Control** gesture exists."

> **Dev:** "Should the **Run Steering Affordance** knob follow the finger up and down like a circular joystick?"
> **Domain expert:** "No - it presents horizontal **Run Steering Control** displacement only; vertical displacement still has no steering meaning."

> **Dev:** "Should the **Run Steering Affordance** use a horizontal rail or line for the knob path?"
> **Domain expert:** "No - it uses a knob that moves horizontally, with subtle **Run Steering Range** end hints and no continuous track."

> **Dev:** "Should the **Run Steering Affordance** knob spring or ease toward the active touch displacement?"
> **Domain expert:** "No - while **Run Steering Control** is active, the knob directly follows current horizontal displacement."

> **Dev:** "Should the **Run Steering Affordance** knob animate out from **Run Steering Origin** when the gesture starts?"
> **Domain expert:** "No - it appears at the current horizontal displacement immediately; appearance animation may affect opacity or scale only."

> **Dev:** "Should the **Run Steering Affordance** knob animate back to **Run Steering Origin** when the gesture ends?"
> **Domain expert:** "No - it disappears from the final knob position; gesture-end animation may affect opacity or scale only."

> **Dev:** "Should the **Run Steering Affordance** keep following the finger after full steering is reached?"
> **Domain expert:** "No - visible horizontal displacement clamps at **Run Steering Range** because that is already full steering."

> **Dev:** "Should **Run Steering Deadzone** be invisible in the **Run Steering Affordance**?"
> **Domain expert:** "No - it may appear as a neutral area around **Run Steering Origin** while **Run Steering Control** remains neutral inside it."

> **Dev:** "Should the **Run Steering Affordance** stay centered while the touch is still inside **Run Steering Deadzone**?"
> **Domain expert:** "No - it may show horizontal touch movement inside **Run Steering Deadzone**, but **Run Steering Control** remains neutral there."

> **Dev:** "Should the **Run Steering Affordance** have separate visual-only range or deadzone values?"
> **Domain expert:** "No - it presents the same **Run Steering Range** and **Run Steering Deadzone** used by **Run Steering Control**."

> **Dev:** "Should the **Run Steering Affordance** be attached to the **Run Body** in the world?"
> **Domain expert:** "No - it is presented in screen space from touch positions, because **Run Steering Origin** and **Run Steering Range** are touch concepts."

> **Dev:** "Should the **Run Steering Affordance** shift inward near a screen edge so the whole visual stays visible?"
> **Domain expert:** "No - it preserves the true **Run Steering Origin** and **Run Steering Range**; decorative visuals may clip or fade without changing those facts."

> **Dev:** "If the player taps a future pause button during **Running**, should that also start **Run Steering Control**?"
> **Domain expert:** "No - touches that begin on interactive UI during **Running** belong to that UI and do not start **Run Steering Control**."

> **Dev:** "If an active **Run Steering Control** gesture slides over UI, should the UI take it over?"
> **Domain expert:** "No - the active **Run Steering Control** gesture remains active until that touch ends or cancels."

> **Dev:** "Does **Run Steering Control** use screen center as neutral?"
> **Domain expert:** "No - the **Run Steering Origin** is the initial touch position for that gesture."

> **Dev:** "Is the **Run Steering Frame** the same as the **Run Progress Frame**?"
> **Domain expert:** "No - **Run Steering Frame** orients player control, while **Run Progress Frame** defines run metrics."

> **Dev:** "Should **Run Steering Control** cap how fast the **Run Body** can slide?"
> **Domain expert:** "No - it steers direction; **Run Surface Contact Slowdown** slows the **Run Body**, with only a defensive **Run Body Speed Sanity Guard** for impossible values."

> **Dev:** "Do we still need launch speed recovery after removing the steering speed cap?"
> **Domain expert:** "No - **Run Steering Mode Selector** and **Launch Landing Stabilization** may still exist, but neither is a speed recovery system."

> **Dev:** "Can the player steer while the slingshot shot is still flying?"
> **Domain expert:** "Yes - unsupported fired motion uses **Run Air Steering Control**, while grounded steering waits for valid **Run Surface** support."

> **Dev:** "Should a weak grounded launch start steering because a timer expired?"
> **Domain expert:** "No - **Run Steering Mode Selector** should be event or condition driven, not timeout driven."

> **Dev:** "Can a head **Animated Contact Sensor** touching a **Run Obstacle** end the run?"
> **Domain expert:** "No - it is an obstacle contact report; **Obstacle Impact** still comes from the **Run Body** path unless an explicit feature policy says otherwise."

> **Dev:** "Should body-part obstacle sensors use their own obstacle tags?"
> **Domain expert:** "No - they reuse **Run Contact Category** authoring, such as `RunContactCategory.Obstacle`, and only change the source of the report."

> **Dev:** "Can an obstacle root carry one `RunContact` for all child colliders?"
> **Domain expert:** "No - if body-part obstacle notices are enabled, `RunContact` stays collider-local; every reportable obstacle collider carries its own **Run Contact Category** metadata."

> **Dev:** "Can pickup collection and body-part obstacle notices share the same sensor source?"
> **Domain expert:** "No - pickups resolve to **Pickup** aggregates, while obstacles resolve collider-local **Run Contact Category** metadata."

> **Dev:** "Can one hand sensor be listed in both pickup and obstacle sources?"
> **Domain expert:** "Only if a later body-part obstacle feature explicitly makes it eligible for both; first-pass animated sensors are pickup-only."

> **Dev:** "What owns configured head/body obstacle sensors?"
> **Domain expert:** "If that feature is enabled, **Run Obstacle Sensor Source** owns them and emits body-part **Run Obstacle** notices only; it does not collect pickups or end the **Run**."

> **Dev:** "Should sensor source components live under **Character Visual Anchor** because the sensors follow animation?"
> **Domain expert:** "No - sources live on the gameplay-owned Player / **Run Body** side and reference sensors hosted by **Animated Contact Sensor Physics Root**."

> **Dev:** "Should copied body-part sensors sync in `FixedUpdate` because trigger delivery is physics?"
> **Domain expert:** "No - **Animated Contact Sensor Pose Sync** copies finalized **Character** poses after presentation/animation, then Unity consumes the copied sensor transforms on the next physics step."

> **Dev:** "Can a source just scan every `TriggerNotifier` under **Character Visual Anchor**?"
> **Domain expert:** "No - first implementation uses explicit sensor entries so eligibility is authored and validated."

> **Dev:** "Do we need a separate `AnimatedContactSensor` component on every hand or head sensor?"
> **Domain expert:** "No - first-pass source entries reference `TriggerNotifier` directly; add a dedicated component only when source entries stop carrying the authoring contract clearly."

> **Dev:** "Should `TriggerNotifier` manage which gameplay source listens to it?"
> **Domain expert:** "No - sensor sources subscribe and unsubscribe from their configured notifiers through their own lifecycle."

> **Dev:** "Should a pickup sensor warning-log when it touches a non-pickup?"
> **Domain expert:** "No - that is a non-matching contact; the source emits no typed report."

> **Dev:** "Should **Pickup Sensor Source** suppress the second hand touching the same pickup?"
> **Domain expert:** "No - it may emit another **Pickup Contact** fact; **Pickup Collection Controller** and **Level Pickup State** prevent duplicate rewards."

> **Dev:** "Should **Pickup Sensor Source** check whether the run is currently collecting pickups?"
> **Domain expert:** "No - it reports **Pickup Contact** facts; **Pickup Collection Controller** owns **Gameplay State** gating."

> **Dev:** "Should **Pickup Contact** include reward amount or collection result?"
> **Domain expert:** "No - it is a contact fact; reward and accepted collection outcome are downstream."

> **Dev:** "If a hand sensor is already inside a pickup when the pickup source enables, should the source replay that contact?"
> **Domain expert:** "No - first pass reports new trigger enters only; source enable order should make it active before collection matters."

> **Dev:** "Should **Run Obstacle Sensor Source** only emit while **Running**?"
> **Domain expert:** "No - it reports contact facts; consumers decide through **Gameplay State** whether to act on them."

> **Dev:** "Should **Run Obstacle Sensor Source** deduplicate hand and head sensors touching the same obstacle?"
> **Domain expert:** "No - it emits each body-part contact fact; consumers own deduplication, cooldowns, and gameplay meaning."

> **Dev:** "If a hand sensor is already inside an obstacle when the source enables, should the source replay that contact?"
> **Domain expert:** "No - if body-part obstacle notices are enabled, the source reports new trigger enters only; source enable order should make it active before those contacts matter."

> **Dev:** "Should obstacle sensors report stay and exit events?"
> **Domain expert:** "No - when first enabled, it emits enter-only body-part **Run Obstacle** notices, matching `TriggerNotifier.TriggerEntered`."

> **Dev:** "Should gameplay consumers subscribe directly to sensor `TriggerNotifier`s?"
> **Domain expert:** "No - sensor sources keep raw trigger callbacks private and publish typed, resolved contact reports."

> **Dev:** "Should animated hand sensors use Player Tag or Player Layer?"
> **Domain expert:** "No - they use the `PlayerBodyPart` physics layer and no Player Tag; source membership gives gameplay meaning."

> **Dev:** "Should `PlayerBodyPart` sensors trigger against the **Run Body Contact Collider**?"
> **Domain expert:** "No - disable Player Layer and **Player Body Part Layer** interaction unless a self-contact feature explicitly needs it."

> **Dev:** "Should `PlayerBodyPart` sensors trigger against `CameraObstacle` in the first pass?"
> **Domain expert:** "No - first-pass `PlayerBodyPart` interaction is pickup-only; body-part obstacle notices require a later explicit feature."

> **Dev:** "Should a head obstacle notice include contact normal and impact speed?"
> **Domain expert:** "No - it carries lightweight trigger facts; impact physics belongs to the **Run Body Contact Collider** collision path."

> **Dev:** "If a weak launch stays grounded, when can steering start?"
> **Domain expert:** "When the **Run Body** is supported by a valid **Run Surface** and has no positive lift away from that surface."

## Flagged ambiguities

- "State" resolves to one current **Gameplay State**, not tags or simultaneous flags.
- "Player" and "target" resolve to **Launch Target** during **Pre-Launch** slingshot discussion, and to **Run Body** during **Running** movement discussion.
- "Virtual joystick" resolves to **Run Steering Control** when discussing **Running**, not to **Pull** or a visible joystick asset.
- "Visible joystick" resolves to **Run Steering Affordance** when discussing rendered feedback for **Run Steering Control**, not a new 2D movement input.
- "Fixed joystick base" conflicts with **Run Steering Origin** unless **Run Steering Control** semantics are explicitly changed.
- "UI joystick control" conflicts with **Run Steering Affordance** if it emits gameplay input instead of presenting **Run Steering Control** state.
- "Raycast-blocking joystick UI" conflicts with **Run Steering Affordance** because the affordance is not interactive UI and does not consume touches.
- "Missing joystick view means no steering" conflicts with **Run Steering Control** because steering does not depend on **Run Steering Affordance** being present.
- "Joystick zone" and "left-side joystick" conflict with **Run Steering Control** unless touch eligibility is explicitly changed.
- "Idle joystick" and "persistent joystick" conflict with **Run Steering Affordance** unless its active-gesture lifecycle is explicitly changed.
- "Circular joystick", "2D knob", and "vertical joystick movement" conflict with **Run Steering Affordance** unless **Run Steering Control** gains vertical steering meaning.
- "Horizontal rail", "track line", and "knob path line" conflict with **Run Steering Affordance** because the knob moves horizontally without a continuous track.
- "Spring-follow knob", "smoothed active knob", and "laggy joystick animation" conflict with **Run Steering Affordance** because active displacement is shown directly.
- "Position tween from origin", "animate knob out from center", and "delayed first knob position" conflict with **Run Steering Affordance** because gesture-start position is shown immediately.
- "Return-to-origin animation", "snap back to center", and "recenter on gesture end" conflict with **Run Steering Affordance** because gesture-end disappearance happens from the final knob position.
- "Center" resolves to **Run Steering Origin** for **Run Steering Control**, not screen center.
- "Size limit" resolves to **Run Steering Range**, where physical displacement beyond the range clamps to full steering.
- "Joystick travel beyond the range" conflicts with **Run Steering Affordance** because visible horizontal displacement clamps at **Run Steering Range**.
- "Deadzone" resolves to **Run Steering Deadzone** for **Run Steering Control**, not screen center distance.
- "Invisible deadzone" conflicts with **Run Steering Affordance** when neutral-area feedback is needed; **Run Steering Deadzone** may be shown without changing control behavior.
- "Deadzone means no visual movement" conflicts with **Run Steering Affordance**; inside-deadzone movement may be visible while steering remains neutral.
- "Visual-only range" and "visual-only deadzone" conflict with **Run Steering Affordance** because it presents the actual **Run Steering Range** and **Run Steering Deadzone**.
- "World-space joystick" conflicts with **Run Steering Affordance** because the affordance presents screen-space touch concepts, not **Run Body** world position.
- "Edge-safe joystick center" and "fully visible joystick" conflict with **Run Steering Affordance** if they shift the apparent **Run Steering Origin** or **Run Steering Range** near a screen edge.
- "Any touch during Running" excludes touches that begin on interactive UI during **Running**.
- "UI stealing steering" conflicts with **Run Steering Control** once a gesture is already active.
- "Responsiveness" resolves to **Run Steering Responsiveness**, not input polling or touch sampling.
- "Ground normal", "support frame", and "locomotion frame" resolve to **Run Steering Frame** only when discussing player steering orientation.
- "Steering frame" resolves to **Run Steering Frame**, not **Run Progress Frame**.
- "In-flight steering", "air control", and "launch steering" resolve to **Run Air Steering Control** during **Running** movement discussion.
- "Falling steering" resolves to **Run Air Steering Control** while the **Gameplay State** is still **Running**.
- "Air stabilization" resolves to no **Run Air Steering Control** behavior unless an active **Run Steering Control** gesture exists.
- "Stale grounded support" resolves to **Run Air Steering Control** when the **Run Body** is moving away from the reported **Run Surface** above the accepted lift tolerance.
- "Post-launch steering gate" resolves to **Run Steering Mode Selector**; the selector is not a rule that disables air steering.
- "Blocked steering mode" resolves to a **Run Steering Control** gate, not a **Run Steering Mode Selector** selection.
- "Launch speed bypass", "burst cap", and "speed recovery" resolve to obsolete speed-cap workarounds once **Run Steering Control** no longer caps normal **Run Body** speed.
- "No-takeoff timeout" should not be used to enable grounded steering after **Launch**.
- "Landing stabilization" resolves to **Launch Landing Stabilization**, not slowdown, downforce, or speed recovery.
- "Speed cap" resolves to **Run Body Speed Sanity Guard** only when discussing impossible velocity validation; normal sliding speed should not be capped by **Run Steering Control**.
- "Friction", "drag", and "slowdown" resolve first to **Run Surface Contact Slowdown** for ordinary sliding speed loss.
- "Joystick" does not imply two-dimensional movement for **Run Steering Control**.
- "Stops" resolves to **Lost Momentum**, not arbitrary one-frame low velocity.
- "Boundary" resolves to **Run Safety Net** only when it catches the below-course failure case.
- "Forward", "downhill", "distance", and "progress" resolve to **Run Progress Frame** for run metrics.
- "Air time" resolves to **Run Air Time** for run metrics and rewards, not to airborne animation.
- "Reward row order" resolves to **Run Reward Source Order**, not UI label sorting.
- "Skip result" resolves to **Run Reward Reveal Fast-Forward** while the reveal is in progress and **Acknowledge Run Result** after it is complete.
- "Idle camera" resolves to **Run Preparation Camera** before preparation ends and **Pre-Launch Camera** while the slingshot can accept a pull.
