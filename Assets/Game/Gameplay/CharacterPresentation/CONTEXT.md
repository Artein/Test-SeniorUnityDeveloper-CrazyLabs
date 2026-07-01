# Character Presentation

Character Presentation covers how the visible character appears in response to gameplay lifecycle, motion, surface facts, and slingshot presentation facts.

## Language

**Character**:
The visible avatar presented for the controlled **Launch Target**.
_Avoid_: Player, Launch Target, skin

**Source Character Asset**:
An imported model, skeleton, material, texture, audio, effect, or animation source used to build a project-owned **Character**.
_Avoid_: Character, gameplay object, composition root

**Character Visual Anchor**:
The transform role that mounts the **Character** onto the controlled **Launch Target**.
_Avoid_: Collider root, Band Center, model root

**Character Model Root**:
The internal alignment role that absorbs imported model offset, rotation, and scale.
_Avoid_: Character Visual Anchor, gameplay root, collider root

**Character Presentation Mode**:
The selected appearance state for the **Character**.
_Avoid_: Gameplay State, animation trigger, run phase

**Idle**:
The neutral **Character Presentation Mode** used when no stronger presentation state is active.
_Avoid_: Held, asleep, inactive

**Pull Anticipation**:
The **Character Presentation Mode** used while an **Active Pull** is preparing a launch.
_Avoid_: Pull Hint, drag state, Launch Push

**Launch Push**:
The **Character Presentation Mode** used immediately after an accepted **Launch**.
_Avoid_: Band recoil, Running, Pull Anticipation

**Slide**:
The grounded locomotion **Character Presentation Mode** for downhill sliding.
_Avoid_: Gameplay State, Run Surface, skid effect

**Run**:
The grounded locomotion **Character Presentation Mode** for forward running movement.
_Avoid_: Gameplay Run, Running state

**Airborne**:
The **Character Presentation Mode** used when the character is not supported by a **Run Surface**.
_Avoid_: Jump state, falling result, Lost Momentum

**Victory**:
The terminal **Character Presentation Mode** for a successful **Run Result**.
_Avoid_: UI title, Run Finish, reward state

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
Continuous facts about the **Run Surface** currently supporting or near the **Launch Target**.
_Avoid_: Contact event, ground tag, raw hit

**Run Surface Context Source**:
The probe boundary that provides current **Run Surface Context**.
_Avoid_: Run Contact Category, collision event, classifier

**Run Surface Slope Calculator**:
The math boundary that derives forward-downhill slope from surface and course axes.
_Avoid_: Animator transition, surface probe, character view

**Course Planar Speed**:
The unsigned planar speed magnitude across the **Run Progress Frame** plane.
_Avoid_: Forward speed, vertical speed, animation speed

**Course Forward Speed**:
The signed speed along the **Run Progress Frame** forward axis.
_Avoid_: Planar speed, launch speed, world speed

**Playback Speed Multiplier**:
The presentation value that scales locomotion playback.
_Avoid_: Global animation speed, movement speed, stat modifier

**Lateral Lean**:
A future visual left-right presentation value if steering needs to affect the **Character**.
_Avoid_: Steer, Pull Offset, gameplay input

**Character Presentation Cue**:
A future one-shot presentation event that is not itself a sustained **Character Presentation Mode**.
_Avoid_: Gameplay State, animation mode, run result

## Relationships

- A **Character** visually represents the **Launch Target** but does not replace it.
- **Character Visual Anchor** belongs to visual mounting, not gameplay contact or band alignment.
- **Character Model Root** handles source-asset alignment under a **Character**.
- **Character Presentation Mode** is appearance language, not **Gameplay State**.
- **Character Presenter** gathers facts and applies one **Character Presentation Frame** to the **Character Presentation View**.
- **Character Presentation Mode Classifier** returns one **Character Presentation Classification Result**.
- **Run Surface Context** helps choose grounded, sliding, running, and airborne presentation.
- **Course Planar Speed** is useful for playback scaling; **Course Forward Speed** is useful for forward-run eligibility.
- **Victory** and **Defeat** come from the accepted **Run Result**, not from **Run Ended** alone.

## Example dialogue

> **Dev:** "Is Ladybug the new **Launch Target**?"
> **Domain expert:** "No - Ladybug is the **Character**; the controlled gameplay object remains the **Launch Target**."

> **Dev:** "Should sliding and running be separate **Gameplay States**?"
> **Domain expert:** "No - they are **Character Presentation Modes** derived from run context."

> **Dev:** "Does **Run Ended** alone tell us whether to play victory or defeat?"
> **Domain expert:** "No - terminal character presentation uses the accepted **Run Result**."

## Flagged ambiguities

- "Ladybug", "avatar", "skin", and "player model" resolve to **Character** for appearance language.
- "Slide", "run", "airborne", "victory", and "death" resolve to **Character Presentation Mode** when discussing appearance.
- "Air time" resolves to gameplay **Run Air Time** when discussing metrics or rewards.
- "Held" resolves to **Idle** unless an **Active Pull** requires **Pull Anticipation**.
- "Ground probe", "surface probe", and "slope source" resolve to **Run Surface Context Source**.
- "Slope math" resolves to **Run Surface Slope Calculator**.
- "Move speed" and raw speed values resolve to presenter facts unless a view-facing term is explicitly needed.
- "Steer" resolves to gameplay control language; use **Lateral Lean** only for visual character tilt.
- "Root motion" means visual-only authored motion, not movement of the **Launch Target**.
