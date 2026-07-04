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

**Run Steering Control**:
The player touch control used during **Running** to steer the **Run Body**.
_Avoid_: Virtual joystick, screen steering, Pull

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

## Relationships

- A **Level Session** may contain multiple **Runs**.
- A **Run** is governed by one current **Gameplay State**.
- **Run Preparation** occurs before **Pre-Launch**.
- **Continue** occurs during **Run Preparation**.
- **Pre-Launch** accepts a **Pull** and may lead to **Launch**.
- **Launch** starts **Running**.
- The slingshot-facing **Launch Target** becomes the **Run Body** when **Launch** starts **Running**.
- **Run Steering Control** belongs to **Running**, not **Pre-Launch**.
- **Run Steering Control** steers the **Run Body** during **Running**.
- **Run Steering Control** may begin from any touch during **Running**.
- One active **Run Steering Control** gesture has one **Run Steering Origin**.
- Only one **Run Steering Control** gesture is active at a time.
- Additional touches do not change the active **Run Steering Control** gesture.
- The active **Run Steering Control** gesture ends when its touch ends.
- **Run Steering Range** defines full steering from the **Run Steering Origin** as a physical touch distance.
- **Run Steering Range** maps physical horizontal displacement directly to steering amount until full steering is reached.
- **Run Steering Range** remains a strict physical distance when **Run Steering Origin** is near a screen edge.
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

> **Dev:** "If a weak launch stays grounded, when can steering start?"
> **Domain expert:** "When the **Run Body** is supported by a valid **Run Surface** and has no positive lift away from that surface."

## Flagged ambiguities

- "State" resolves to one current **Gameplay State**, not tags or simultaneous flags.
- "Player" and "target" resolve to **Launch Target** during **Pre-Launch** slingshot discussion, and to **Run Body** during **Running** movement discussion.
- "Virtual joystick" resolves to **Run Steering Control** when discussing **Running**, not to **Pull** or a visible joystick asset.
- "Center" resolves to **Run Steering Origin** for **Run Steering Control**, not screen center.
- "Size limit" resolves to **Run Steering Range**, where physical displacement beyond the range clamps to full steering.
- "Deadzone" resolves to **Run Steering Deadzone** for **Run Steering Control**, not screen center distance.
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
