# Gameplay

Gameplay covers the runtime concepts that define how a run begins, moves, and ends.

## Language

**Slingshot**:
The launch device that starts a run.
_Avoid_: Launcher, rope input

**Run**:
A gameplay attempt that begins with **Launch** and ends by crash, lost momentum, or reaching the level end.
_Avoid_: Session, attempt

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
The visible deformation of the **Band** during a **Pull**.
_Avoid_: Rope physics, band simulation

**Touch Indicator**:
The visual marker that follows the captured touch during an **Active Pull**.
_Avoid_: Pointer, cursor

**Pull Hint**:
The idle tutorial visual that suggests starting a **Pull** on the **Band**.
_Avoid_: Pointer, nudge

**Pull**:
A touch gesture that begins on the **Band** and draws it backward before release.
_Avoid_: Drag, stretch

**Pull Release**:
The input moment when an **Active Pull** ends before it is judged as a **Launch**.
_Avoid_: Launch, fire

**Active Pull**:
The single **Pull** currently owned by one captured touch.
_Avoid_: Current drag, selected finger

**Pull Offset**:
The lateral displacement of a **Pull** from the **Band** center.
_Avoid_: Aim offset, side drag

**Launch Frame**:
The **Slingshot**-owned orientation that defines forward, backward, lateral, and up directions for a **Pull** and **Launch**.
_Avoid_: Camera forward, world forward

**Pull Plane**:
The **Slingshot**-owned plane where touch positions are interpreted as backward and lateral **Pull** movement.
_Avoid_: Ground raycast, surface input

**Launch**:
The transition from a released **Pull** into forward motion for the **Launch Target**.
_Avoid_: Fire, shoot

**Launch Target**:
The scene object that receives launch energy from a **Slingshot**.
_Avoid_: Projectile, payload

## Relationships

- A **Slingshot** has one **Band**.
- A **Band** has one **Band Touch Target**.
- A **Band** has one visible **Band Shape**.
- A **Slingshot** may show one **Pull Hint** while idle.
- A **Slingshot** accepts a **Pull** during **Pre-Launch**.
- A **Run** is governed by one current **Gameplay State**.
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
- A **Pull** may have a **Pull Offset**.
- A **Slingshot** owns one **Launch Frame**.
- A **Slingshot** owns one **Pull Plane**.
- A **Launch** affects one **Launch Target**.

## Example dialogue

> **Dev:** "Can the **Launch Target** move before the **Pull** is released?"
> **Domain expert:** "No — the **Slingshot** prepares the run, and the **Launch** begins the run."

> **Dev:** "Can the **Slingshot** accept another **Pull** after **Launch**?"
> **Domain expert:** "No — after **Launch**, the **Run** has started and **Pre-Launch** is over."

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
> **Domain expert:** "No — **Band Shape** shows the **Pull**, while **Launch** rules decide the resulting motion."

> **Dev:** "Does moving the camera change **Pull** direction?"
> **Domain expert:** "No — the **Launch Frame** belongs to the **Slingshot**, not the camera."

> **Dev:** "Can two touches create two **Pulls** at the same time?"
> **Domain expert:** "No — a **Slingshot** can have only one **Active Pull**."

> **Dev:** "Does the level **Surface** decide where a **Pull** is measured?"
> **Domain expert:** "No — the **Pull Plane** belongs to the **Slingshot**."

> **Dev:** "Is the hand animation the same thing as the user's touch?"
> **Domain expert:** "No — **Pull Hint** teaches the gesture while **Touch Indicator** follows the real **Active Pull**."

## Flagged ambiguities

- "Rope" was used for the draggable launch element, but the gameplay concept is **Band**; rope may still describe a visual asset if needed.
- "Rope physics" was used for **Band Shape**, but current gameplay treats deformation as visual rather than physical simulation.
- "Pointer" was used for both tutorial and touch feedback; resolved as **Pull Hint** for idle guidance and **Touch Indicator** for live input.
- "State" was used for gameplay phase identity, not tags or simultaneous flags; resolved as one current **Gameplay State**.
- "Touch" and "mouse" were used as input sources, but gameplay consumes **Pointer Input** from **Unity Input**.
