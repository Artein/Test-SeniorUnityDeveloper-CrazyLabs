# Character Presentation

Character Presentation covers how the visible character appears in response to gameplay lifecycle, motion, surface facts, and slingshot presentation facts.

## Language

**Character**:
The visible avatar presented for the controlled gameplay object.
_Avoid_: Player, Launch Target, Run Body, skin

**Source Character Asset**:
An imported model, skeleton, material, texture, audio, effect, or animation source used to build a project-owned **Character**.
_Avoid_: Character, gameplay object, composition root

**Character Visual Anchor**:
The presentation-space transform role that positions and orients the **Character** relative to the controlled gameplay object.
_Avoid_: Launch Target child, Collider root, Band Center, model root

**Character Visual Follower**:
The presentation coordinator that keeps the **Character Visual Anchor** visually following the controlled gameplay object's render pose with bounded smoothing.
_Avoid_: Movement controller, Rigidbody follower, gameplay pose source, camera smoother

**Character Visual Follow Tuning**:
The serialized **Character Presentation Tuning** values controlling **Character Visual Follower** response, lag clamps, and snap thresholds.
_Avoid_: Movement tuning, steering frame tuning, Run Surface Context smoothing

**Smoothed Character Pose**:
The presentation-only pose applied to the **Character Visual Anchor** by the **Character Visual Follower**.
_Avoid_: Launch Target pose, Rigidbody pose, gameplay position, camera source pose

**Character Model Root**:
The internal alignment role that absorbs imported model offset, rotation, and scale.
_Avoid_: Character Visual Anchor, gameplay root, collider root

**Character Presentation Mode**:
The selected appearance state for the **Character**.
_Avoid_: Gameplay State, animation trigger, run phase

**Idle**:
The neutral or stalled **Character Presentation Mode** used when no stronger presentation state or meaningful grounded movement is active.
_Avoid_: Held, asleep, inactive, slow locomotion

**Pull Anticipation**:
The **Character Presentation Mode** used while an **Active Pull** is preparing a launch.
_Avoid_: Pull Hint, drag state, Launch Push

**Launch Push**:
The **Character Presentation Mode** used immediately after an accepted **Launch**.
_Avoid_: Band recoil, Running, Pull Anticipation

**Launch Flight**:
The **Character Presentation Mode** used for fired-by-slingshot unsupported motion before first real post-launch landing.
_Avoid_: Airborne, falling, speed recovery, launch movement state

**Slide**:
The grounded locomotion **Character Presentation Mode** for momentum-driven movement across a **Run Surface**, including downhill sliding and flat coasting.
_Avoid_: Gameplay State, Run Surface, skid effect, downhill-only mode, stalled movement

**Coast**:
A descriptive flavor of **Slide** used for flatter or slower grounded movement.
_Avoid_: Run, separate Character Presentation Mode, powered movement

**Slide Flavor**:
An optional visual variation inside **Slide** based on presentation facts such as slope or speed.
_Avoid_: Character Presentation Mode, gameplay state, Run replacement

**Slide Flavor Tuning**:
Future **Character Presentation Tuning** for visual variation inside **Slide**, not for canonical mode selection.
_Avoid_: SlideEnterDownhillDegrees, SlideExitDownhillDegrees, RunFlatMaximumAbsSlopeDegrees

**Run**:
A reserved **Character Presentation Mode** kept for compatibility, not a normal visible locomotion choice.
_Avoid_: Gameplay Run, Running state, flat locomotion

**Reserved Presentation Mode**:
A serialized or asset-backed **Character Presentation Mode** retained to avoid compatibility churn while normal runtime selection stops using it.
_Avoid_: Active mode, cleanup target, gameplay state

**Airborne**:
The **Character Presentation Mode** used for real falling after normal support loss and presentation fall-intent gating.
_Avoid_: Launch Flight, jump state, falling result, Lost Momentum

**Victory**:
The terminal **Character Presentation Mode** for a successful **Run Result**.
_Avoid_: UI title, Run Finish, reward state

**Victory Facing**:
A presentation-only orientation override that turns the visible **Character** toward the **Run Camera** during successful **Victory**.
_Avoid_: Run Body rotation, Rigidbody rotation, camera source rotation

**Defeat**:
The terminal **Character Presentation Mode** for a failed **Run Result**.
_Avoid_: Death reason, failure title, obstacle hit

**Character Presentation Tuning**:
The authored values that shape mode selection and playback feel.
_Avoid_: Gameplay balance, movement config, upgrade tuning

**Character Presentation Mode Classifier**:
The decision logic that selects one **Character Presentation Mode** from presentation-facing facts.
_Avoid_: Animator, gameplay state machine, surface probe

**Character Presentation Classification Input**:
The immutable fact bundle supplied to the **Character Presentation Mode Classifier** for one decision.
_Avoid_: View state, raw physics state, event payload

**Character Presentation Classification Result**:
The selected **Character Presentation Mode** returned by the classifier.
_Avoid_: Diagnostic reason, animation frame, controller state

**Character Presentation Frame**:
The view-facing presentation data applied to the **Character** for one update.
_Avoid_: Classifier input, raw surface context, gameplay state

**Character Presenter**:
The presentation coordinator that gathers facts, owns presentation memory, classifies mode, and applies frames.
_Avoid_: View, classifier, animation controller

**Character Presentation View**:
The passive view boundary that receives **Character Presentation Frames**.
_Avoid_: Classifier, presenter, gameplay controller

**Run Surface Context**:
Continuous facts about the **Run Surface** currently supporting or near the controlled gameplay object.
_Avoid_: Contact event, ground tag, raw hit

**Run Surface Context Source**:
The probe boundary that provides current **Run Surface Context**.
_Avoid_: Run Contact Category, collision event, classifier

**Run Surface Slope Calculator**:
The math boundary that derives forward-downhill slope from surface and course axes for presentation flavor or diagnostics.
_Avoid_: Animator transition, surface probe, character view, canonical mode boundary

**Course Planar Speed**:
The unsigned planar speed magnitude across the **Run Progress Frame** plane.
_Avoid_: Forward speed, vertical speed, progress speed

**Course Forward Speed**:
The signed speed along the **Run Progress Frame** forward axis.
_Avoid_: Planar speed, launch speed, presentation locomotion eligibility

**Meaningful Grounded Movement**:
Grounded **Course Planar Speed** above the presentation locomotion threshold, independent of signed forward progress.
_Avoid_: Forward progress, downhill-only movement, powered movement

**Meaningful Grounded Movement Threshold**:
The **Character Presentation Tuning** value that defines the minimum **Course Planar Speed** for **Slide** instead of **Idle**.
_Avoid_: RunMinimumForwardSpeed, forward-speed threshold, migration shim

**Playback Speed Multiplier**:
The presentation value that scales locomotion playback.
_Avoid_: Global animation speed, movement speed, stat modifier

**Slide Reference Speed**:
The **Character Presentation Tuning** value used to scale **Slide** playback for all normal grounded locomotion.
_Avoid_: RunReferenceSpeed, separate run playback speed, compatibility playback tuning

**Lateral Lean**:
A future visual left-right presentation value if steering needs to affect the **Character**.
_Avoid_: Steer, Pull Offset, gameplay input

**Character Presentation Cue**:
A future one-shot presentation event that is not itself a sustained **Character Presentation Mode**.
_Avoid_: Gameplay State, animation mode, run result

## Relationships

- A **Character** visually represents the controlled gameplay object but does not replace it.
- **Character Visual Anchor** belongs to visual mounting, not transform hierarchy, gameplay contact, or band alignment.
- **Character Visual Follower** smooths only the **Character Visual Anchor**; it must not move the controlled gameplay object, Rigidbody, colliders, camera source, progress source, or surface probes.
- **Character Visual Follower** samples the controlled gameplay object's Transform render pose in the presentation late update phase, not raw Rigidbody position, so the visible **Character** follows Unity's interpolated render pose.
- Camera and motion presentation sources may align to the same controlled-object Transform render pose, but they must not read the **Smoothed Character Pose** as gameplay truth.
- **Smoothed Character Pose** is presentation-only and must not become gameplay truth or an input to physics, steering, progress, collision, or camera systems.
- An **Animated Contact Sensor** may copy a **Character** body-part pose for contact reporting, but this does not make **Smoothed Character Pose** gameplay truth.
- **Animated Contact Sensor** reports should cross into gameplay through a dedicated aggregation boundary, not through direct gameplay dependency on the **Character** hierarchy.
- Raw `TriggerNotifier` callbacks on **Animated Contact Sensors** should stay private to gameplay sensor sources; gameplay consumers should receive typed source reports.
- Pose-following **Animated Contact Sensors** may copy **Character** body-part poses without being parented under **Character Visual Anchor**.
- **Animated Contact Sensors** should remain trigger-only; they must not make **Character Visual Anchor**, animated bones, or per-sensor objects own gameplay Rigidbodies.
- **Animated Contact Sensor Physics Root** belongs to gameplay ownership, not **Character Visual Anchor**, even when it copies **Character** body-part poses.
- **Animated Contact Sensor Physics Root** should be a separate gameplay-owned sibling/root driven explicitly, not a child of the **Run Body** Rigidbody root and not under **Character Visual Anchor**.
- **Animated Contact Sensor Pose Sync** copies finalized **Character** body-part poses into **Animated Contact Sensor Physics Root** after presentation and animation have produced the rendered-frame pose.
- **Animated Contact Sensor Pose Sync** should be ordered after **Character Visual Follower**; it must not run from the gameplay movement `FixedUpdate` path.
- **Animated Contact Sensor Pose Sync** may read **Character** body-part transforms without making **Smoothed Character Pose** a general gameplay truth source.
- **Animated Contact Sensor Pose Sync** should not make **Character Visual Anchor** own a standalone physics or trigger-delivery loop.
- A **Character** using `Animate Physics` Animator timing needs an explicit sensor sync policy; first-pass render-frame sync assumes normal presentation timing.
- Pickup trigger delivery physics-body ownership belongs to **Animated Contact Sensor Physics Root**, not **Character Visual Anchor**, animated bones, or per-pickup Rigidbodies.
- Pose-following **Animated Contact Sensors** may use the `PlayerBodyPart` physics layer, but layer and tag policy remains gameplay contact authoring, not character presentation state.
- Pose-following **Animated Contact Sensors** should not trigger against Player Layer just because they follow the **Character**.
- First-pass pose-following **Animated Contact Sensors** should interact with **Pickups** only; `CameraObstacle` interaction requires an explicit body-part obstacle feature.
- **Character** GameObject names or hierarchy paths may provide first-pass **Animated Sensor Identity**, but they should not become a long-lived gameplay API when stable sensor content is needed.
- Pickup-eligible **Animated Contact Sensor** authoring may reference **Character** hierarchy objects as pose inputs, but gameplay source entries should point at copied sensors hosted by **Animated Contact Sensor Physics Root**.
- Future obstacle-eligible **Animated Contact Sensor** authoring may use the same pose-input pattern, but **Run Obstacle Sensor Source** owns what is obstacle-eligible if that feature is enabled.
- Gameplay sensor source components should not live under **Character Visual Anchor** just because their referenced **Animated Contact Sensors** follow **Character** animation.
- **Character Visual Anchor** supplies animated pose inputs; gameplay-owned sensor sources own eligibility, target resolution, and typed contact event publication.
- Gameplay sensor sources should reference explicit **Animated Contact Sensor** entries; **Character Visual Anchor** should not be treated as a scan root for all `TriggerNotifier` children.
- First-pass gameplay sensor source entries should reference copied sensor `TriggerNotifier` components directly; a separate `AnimatedContactSensor` component is not required on every sensor object.
- Gameplay sensor sources own their raw `TriggerNotifier` subscription lifecycle; **Character Visual Anchor** and animated bones do not own gameplay listener state.
- Gameplay sensor sources own target-resolution filtering; non-matching sensor contacts should not make **Character Visual Anchor** log or decide gameplay meaning.
- The same **Animated Contact Sensor** may be referenced by multiple gameplay sensor sources only when it is explicitly eligible for multiple contact purposes.
- Duplicate references to the same **Animated Contact Sensor** inside one gameplay sensor source should be treated as setup error or validation warning.
- **Character Visual Follow Tuning** belongs to the **Character Presentation View** scene or prefab authoring; it is not gameplay movement, upgrade, or economy tuning.
- **Character Visual Follower** is owned by the composition root as a presentation entry point; the view remains passive and should not own a standalone `LateUpdate` loop.
- **Character Visual Follower** keeps position tight, heading fairly tight, and smooths up/tilt more strongly; explicit snaps handle run resets and terminal state changes, while automatic snap thresholds are for stationary drift rather than continuous high-speed target motion.
- **Character Model Root** handles source-asset alignment under a **Character**.
- **Character Presentation Mode** is appearance language, not **Gameplay State**.
- **Character Presentation Mode** changes must not change **Run Body** velocity, steering state, speed recovery, contact response, or run distance.
- **Character Presenter** gathers facts and applies one **Character Presentation Frame** to the **Character Presentation View**.
- **Character Presentation Mode Classifier** returns one **Character Presentation Classification Result**.
- **Run Surface Context** helps choose grounded **Slide**, **Idle**, and **Airborne** presentation.
- **Launch Flight** describes slingshot-fired unsupported appearance; it must not clamp or recover **Run Body** speed when it starts, ends, or transitions to **Airborne**.
- **Airborne** describes real falling appearance; entering **Airborne** must not change **Run Body** speed.
- **Slide** covers both downhill sliding and flatter **Coast** flavor; slope may affect **Slide Flavor** but not the canonical mode.
- The first **Slide** implementation uses the existing **Slide** presentation for all **Meaningful Grounded Movement**; **Coast** does not require a separate clip or blend.
- **Idle** covers true stopped or stalled grounded presentation; **Slide** requires **Meaningful Grounded Movement**.
- **Run** remains a **Reserved Presentation Mode**, not normal appearance language.
- Normal runtime classification must not emit **Run**; view compatibility may map unexpected **Run** frames to **Slide** until asset cleanup removes the legacy state.
- Character presentation regression tests should broadly protect that normal classifier paths do not return **Run**, while still covering special modes such as **Launch Push**, **Airborne**, **Victory**, and **Defeat**.
- **Course Planar Speed** is useful for playback scaling and **Meaningful Grounded Movement**; **Course Forward Speed** belongs to progress, failure, and reward logic.
- Character presentation tuning should be renamed in place to **Meaningful Grounded Movement Threshold** language; no `FormerlySerializedAs` or migration shim is required for the old run-forward name.
- **Slide Reference Speed** is the normal locomotion playback reference; old run-reference tuning should be removed or treated as compatibility-only fallback to **Slide Reference Speed**.
- **Run Surface Slope Calculator** must not decide between **Slide** and **Run**; it can only support **Slide Flavor** or diagnostics.
- Old slope threshold mode-selection tuning should be removed from the classifier path; any future slope tuning must be explicitly named as **Slide Flavor Tuning** and left unwired from canonical mode selection until a flavor pass needs it.
- **Victory** and **Defeat** come from the accepted **Run Result**, not from **Run Ended** alone.
- **Victory Facing** belongs to successful **Victory** presentation, not **Run Finish** contact handling or **Run End Pose Lock**.
- **Victory Facing** is driven by the accepted successful **Run Result**, not by raw **Run Finish** contact entry.
- **Victory Facing** may rotate the visible **Character** or **Character Visual Anchor** toward the **Run Camera**, but must not rotate the **Run Body**, Rigidbody, colliders, camera source, progress source, or surface probes.
- **Victory Facing** should face the actual rendered **Run Camera** lens, not the Cinemachine tracking anchor or camera source transform.
- **Victory Facing** should blend through the start of **Victory** presentation, so the turn reads as part of the victory animation rather than a separate correction.
- **Victory Facing** must reset when returning to **Run Preparation** so the next **Run** starts from normal **Character Visual Follower** orientation.

## Example dialogue

> **Dev:** "Is Ladybug the new **Run Body**?"
> **Domain expert:** "No - Ladybug is the **Character**; the controlled gameplay object remains separate."

> **Dev:** "Does the **Character Visual Anchor** need to be parented under the controlled gameplay object?"
> **Domain expert:** "No - it is the presentation pose for the **Character**, not a gameplay hierarchy requirement."

> **Dev:** "Can gameplay systems read the **Smoothed Character Pose**?"
> **Domain expert:** "No - gameplay reads the controlled gameplay object and its physics facts; the smoothed pose is visual-only."

> **Dev:** "Does a hand sensor that follows animation make the **Character** a gameplay collider?"
> **Domain expert:** "No - an **Animated Contact Sensor** can copy a body-part pose for narrow contact reporting, but it does not replace **Run Body** collision authority."

> **Dev:** "Should a hand sensor use Player Tag because it is under the **Character**?"
> **Domain expert:** "No - visual hierarchy placement only supplies pose; gameplay contact authoring uses `PlayerBodyPart` and source configuration."

> **Dev:** "Should animated sensors touch the player collider because both represent the same character?"
> **Domain expert:** "No - visual representation does not create player self-contact; gameplay sensor sources only need external contact facts."

> **Dev:** "Can gameplay subscribe directly to a hand `TriggerNotifier`?"
> **Domain expert:** "No - the notifier is a low-level adapter; gameplay should consume typed reports from a sensor source."

> **Dev:** "Should every animated hand or head sensor get a Rigidbody?"
> **Domain expert:** "No - the sensor stays trigger-only; pickup collection uses gameplay-owned **Animated Contact Sensor Physics Root** for trigger delivery, and other contact features need their own explicit physics-body policy."

> **Dev:** "Should `CharacterVisualAnchor` get one Rigidbody for all body-part sensors?"
> **Domain expert:** "No - use gameplay-owned **Animated Contact Sensor Physics Root**; **Character Visual Anchor** remains presentation-only."

> **Dev:** "Should copied body-part sensors read the visual pose before **Character Visual Follower** finishes smoothing?"
> **Domain expert:** "No - **Animated Contact Sensor Pose Sync** runs after the visible **Character** pose is finalized for that rendered frame."

> **Dev:** "Does a head obstacle sensor make the Character hierarchy own obstacle logic?"
> **Domain expert:** "No - if body-part obstacle notices are enabled, **Run Obstacle Sensor Source** owns obstacle eligibility and reporting; the Character hierarchy only supplies animated sensor poses."

> **Dev:** "Should `Pickup Sensor Source` be a child of **Character Visual Anchor**?"
> **Domain expert:** "No - **Character Visual Anchor** supplies pose inputs; source ownership stays with gameplay."

> **Dev:** "Can gameplay infer all sensors by walking the whole character hierarchy?"
> **Domain expert:** "No - source eligibility must be explicit and validated."

> **Dev:** "Should every copied sensor also have an `AnimatedContactSensor` component?"
> **Domain expert:** "No - first pass keeps the copied sensor as trigger plus `TriggerNotifier`; source entries provide gameplay meaning."

> **Dev:** "Should the animated sensor remember which gameplay source subscribed to it?"
> **Domain expert:** "No - lifecycle belongs to the gameplay sensor source; the sensor notifier only reports trigger enters."

> **Dev:** "Should the visual hierarchy warn when a sensor touches something that is not a pickup or obstacle?"
> **Domain expert:** "No - target filtering belongs to gameplay sensor sources; non-matching contacts are ignored."

> **Dev:** "Can the same hand sensor be pickup-eligible and obstacle-eligible?"
> **Domain expert:** "Yes - that is gameplay source configuration; the visual hierarchy only supplies pose inputs."

> **Dev:** "Should we hardcode head and hand sensor ids in gameplay code?"
> **Domain expert:** "No - first use authored object name or hierarchy path; graduate to asset-backed ids when those contacts need stable content contracts."

> **Dev:** "Should the **Character Visual Follower** smooth raw Rigidbody position?"
> **Domain expert:** "No - it follows the controlled gameplay object's Transform render pose during presentation late update, after Rigidbody interpolation has produced the visible pose."

> **Dev:** "Should sliding and running be separate **Gameplay States**?"
> **Domain expert:** "No - **Slide** is a **Character Presentation Mode**, while **Run** is gameplay language or reserved presentation compatibility."

> **Dev:** "Should flat coasting switch from **Slide** to **Run**?"
> **Domain expert:** "No - flat coasting is still **Slide** presentation, possibly with a calmer **Coast** flavor."

> **Dev:** "Should ending **Launch Flight** slow down the gameplay object?"
> **Domain expert:** "No - **Launch Flight** is visual presentation only; **Run Body** speed changes come from gameplay physics and contact behavior."

> **Dev:** "Should we delete the old **Run** enum and Animator state now?"
> **Domain expert:** "No - keep **Run** as a **Reserved Presentation Mode** for compatibility, but normal runtime classification should not produce it."

> **Dev:** "Should a barely moving grounded character keep sliding?"
> **Domain expert:** "No - once grounded movement is no longer meaningful, presentation returns to **Idle**."

> **Dev:** "Should a grounded character sliding sideways or briefly backward switch out of **Slide**?"
> **Domain expert:** "No - presentation uses **Meaningful Grounded Movement**, not signed forward progress."

> **Dev:** "Should downhill thresholds decide whether the character is in **Slide**?"
> **Domain expert:** "No - grounded movement decides **Slide**; slope can only influence **Slide Flavor**."

> **Dev:** "Should `SlideEnterDownhillDegrees`, `SlideExitDownhillDegrees`, or `RunFlatMaximumAbsSlopeDegrees` stay in the classifier?"
> **Domain expert:** "No - remove old slope thresholds from canonical mode selection; reintroduce only explicitly named **Slide Flavor Tuning** when needed."

> **Dev:** "Do we need a new **Coast** animation before removing visible running?"
> **Domain expert:** "No - first route all meaningful grounded movement to the existing **Slide** presentation."

> **Dev:** "Should the old run-forward tuning name stay serialized for migration safety?"
> **Domain expert:** "No - rename it in place to **Meaningful Grounded Movement Threshold** language as a clean authoring update."

> **Dev:** "Should old run playback keep its own reference speed?"
> **Domain expert:** "No - use **Slide Reference Speed** for normal locomotion and compatibility fallback."

> **Dev:** "Does **Run Ended** alone tell us whether to play victory or defeat?"
> **Domain expert:** "No - terminal character presentation uses the accepted **Run Result**."

> **Dev:** "Can the finish victory turn rotate the gameplay object toward the camera?"
> **Domain expert:** "No - **Victory Facing** rotates only the visible **Character** presentation, and it resets before the next **Run Preparation**."

> **Dev:** "Should **Victory Facing** be a separate snap before the victory animation?"
> **Domain expert:** "No - it should blend through the start of **Victory** so the turn and animation read as one presentation."

> **Dev:** "Should touching the finish contact rotate the character toward the camera?"
> **Domain expert:** "No - **Victory Facing** waits for the accepted successful **Run Result**."

## Flagged ambiguities

- "Ladybug", "avatar", "skin", and "player model" resolve to **Character** for appearance language.
- "Visual mount", "visual child", and "character parent" resolve to **Character Visual Anchor**; the term does not imply Unity transform parenting under the controlled gameplay object.
- "Visual smoothing", "presentation follower", "visual follower", and "character smoothing" resolve to **Character Visual Follower**.
- "Smoothed pose", "visual pose", and "follower pose" resolve to **Smoothed Character Pose** and must not be treated as gameplay truth.
- "Visual follow tuning", "anchor smoothing values", and "jitter smoothing settings" resolve to **Character Visual Follow Tuning**.
- "Slide", "coast", "flat sliding", and "grounded locomotion" resolve to **Slide** when discussing normal active grounded appearance.
- "Stopped", "stalled", and "barely moving" resolve to **Idle** when grounded movement is not meaningful.
- "Moving", "sliding sideways", and "sliding backward" resolve by **Meaningful Grounded Movement** for presentation and by **Course Forward Speed** for progress/failure.
- "Run minimum speed", "forward locomotion threshold", and `RunMinimumForwardSpeed` resolve to **Meaningful Grounded Movement Threshold** for character presentation.
- "Run reference speed" and `RunReferenceSpeed` resolve to **Slide Reference Speed** or compatibility fallback, not normal tuning.
- "Slope threshold", "downhill threshold", "flat threshold", `SlideEnterDownhillDegrees`, `SlideExitDownhillDegrees`, and `RunFlatMaximumAbsSlopeDegrees` resolve to **Slide Flavor Tuning** or diagnostics, not **Character Presentation Mode** selection.
- "Run" resolves to gameplay **Run** unless explicitly discussing the **Reserved Presentation Mode** kept for compatibility.
- "Launch flight", "Airborne", "victory", and "death" resolve to **Character Presentation Mode** when discussing appearance.
- "Face the camera", "turn toward camera", and "victory pose rotation" resolve to **Victory Facing** when discussing successful terminal character presentation.
- "Beginning of victory animation", "victory entry", and "opening victory blend" resolve to **Victory Facing** when discussing camera-facing orientation.
- "Launch Flight ended" and "Airborne started" do not imply any gameplay velocity clamp, speed recovery, or movement-state transition.
- "Air time" resolves to gameplay **Run Air Time** when discussing metrics or rewards.
- "Held" resolves to **Idle** unless an **Active Pull** requires **Pull Anticipation**.
- "Ground probe", "surface probe", and "slope source" resolve to **Run Surface Context Source**.
- "Slope math" resolves to **Run Surface Slope Calculator**.
- "Move speed" and raw speed values resolve to presenter facts unless a view-facing term is explicitly needed.
- "Steer" resolves to gameplay control language; use **Lateral Lean** only for visual character tilt.
- "Root motion" means visual-only authored motion, not movement of the controlled gameplay object.
