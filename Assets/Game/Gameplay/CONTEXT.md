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
The **Gameplay State** where the **Launch Target** is moving through the **Run Course**.
_Avoid_: Sliding state, animation run

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
The duration during **Running** when the **Launch Target** is not supported by a **Run Surface**.
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
The hold applied to the **Launch Target** during **Run Ended** so it stays at the accepted **Run Result** pose.
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
A **Run Contact Category** that ends a **Run** when the **Launch Target** falls below the playable course.
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
The camera behavior that follows the **Launch Target** during **Running** and **Run Ended**.
_Avoid_: Main camera, level camera

**Run Camera Anchor**:
The camera-facing reference point derived from the **Launch Target** for **Run Camera** framing.
_Avoid_: Character pivot, camera target, follow transform

## Relationships

- A **Level Session** may contain multiple **Runs**.
- A **Run** is governed by one current **Gameplay State**.
- **Run Preparation** occurs before **Pre-Launch**.
- **Continue** occurs during **Run Preparation**.
- **Pre-Launch** accepts a **Pull** and may lead to **Launch**.
- **Launch** starts **Running**.
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

## Flagged ambiguities

- "State" resolves to one current **Gameplay State**, not tags or simultaneous flags.
- "Player" and "target" resolve to **Launch Target** when discussing the controlled run object.
- "Stops" resolves to **Lost Momentum**, not arbitrary one-frame low velocity.
- "Boundary" resolves to **Run Safety Net** only when it catches the below-course failure case.
- "Forward", "downhill", "distance", and "progress" resolve to **Run Progress Frame** for run metrics.
- "Air time" resolves to **Run Air Time** for run metrics and rewards, not to airborne animation.
- "Reward row order" resolves to **Run Reward Source Order**, not UI label sorting.
- "Skip result" resolves to **Run Reward Reveal Fast-Forward** while the reveal is in progress and **Acknowledge Run Result** after it is complete.
- "Idle camera" resolves to **Run Preparation Camera** before preparation ends and **Pre-Launch Camera** while the slingshot can accept a pull.
