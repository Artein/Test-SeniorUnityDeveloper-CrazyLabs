# Gameplay

Gameplay covers the runtime concepts that define how a run begins, moves, and ends.

## Language

**Slingshot**:
The launch device that starts a run.
_Avoid_: Launcher, rope input

**Run**:
A gameplay attempt that begins with **Launch** and ends by crash, lost momentum, or reaching the level end.
_Avoid_: Session, attempt

<<<<<<< HEAD
**Run Camera**:
The camera behavior that follows the controlled **Launch Target** during a **Run**.
_Avoid_: Player camera, target camera, follow camera

**Run Camera Anchor**:
The camera-facing reference point derived from the **Launch Target** for **Run Camera** framing during a **Run**.
_Avoid_: Camera target, camera pivot, follow target

**Pre-Launch Camera**:
The camera behavior used during **Pre-Launch** before the **Run Camera** takes over.
_Avoid_: Idle camera, static camera

=======
>>>>>>> refs/heads/main
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

**Launch Target**:
<<<<<<< HEAD
The scene object that is held by an **Active Pull**, receives launch energy from a **Slingshot**, and is controlled during a **Run**.
_Avoid_: Player, projectile, payload
=======
The scene object that is held by an **Active Pull** and receives launch energy from a **Slingshot**.
_Avoid_: Projectile, payload
>>>>>>> refs/heads/main

**Launch Target Silhouette**:
The inflated convex band-height **Pull Plane** outline of the held **Launch Target** used by a taut **Band Shape**.
_Avoid_: Collider mesh, 3D wrap shape, concave collider outline

## Relationships

- A **Slingshot** has one **Band**.
- A **Slingshot** has one **Rest Point**.
- A **Band** has one **Band Touch Target**.
- A **Band** has one visible **Band Shape**.
- A **Band Shape** is a taut visual path, not gameplay launch math.
- A **Band Shape** may have one **Band Wrap**.
- A **Band Shape** may have **Band Contact Points**.
- A **Band Wrap** is bounded by **Band Contact Points**.
- A **Band Wrap** follows the held **Launch Target** silhouette.
- A rest/idle **Band Shape** passes through the **Rest Point**.
- **Band Contact Points** are tangent points from **Slingshot** anchors to the **Launch Target Silhouette**.
- A taut **Band Shape** does not pass through the **Launch Target Silhouette**.
- During an **Active Pull**, a **Band Wrap** follows the **Pulled Side** of the current **Launch Target Silhouette**.
- A **Band Wrap** curves through the **Pulled-Side Center**.
- A **Band Wrap** follows the **Launch Target Silhouette** contour between **Band Contact Points** that contains the **Pulled-Side Center**.
- **Pulled-Side Center** follows the actual **Active Pull** direction, including **Pull Offset**.
- A **Slingshot** may show one **Pull Hint** while idle.
- A **Slingshot** accepts a **Pull** during **Pre-Launch**.
- A **Run** is governed by one current **Gameplay State**.
<<<<<<< HEAD
- During a **Run**, the user controls the **Launch Target**.
- A **Run Camera** follows the **Launch Target** during a **Run**.
- A **Run Camera** uses one **Run Camera Anchor** to frame the **Launch Target**.
- A **Run Camera Anchor** is derived from the **Launch Target**.
- A **Pre-Launch Camera** frames the **Slingshot** before a **Run**.
- A **Run Camera** takes over from a **Pre-Launch Camera** after **Launch**.
=======
>>>>>>> refs/heads/main
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

> **Dev:** "Can the **Band Shape** use the **Pull Point** as a point inside the **Launch Target**?"
> **Domain expert:** "No — the **Pull Point** is where the held **Launch Target** is positioned; **Band Contact Points** are where the **Band Shape** visually meets it."

> **Dev:** "Does adding more points to the **Band Wrap** change launch power?"
> **Domain expert:** "No — **Band Wrap** is visual detail, while **Launch** rules use **Pull** distance and **Pull Offset**."

> **Dev:** "Does moving the camera change **Pull** direction?"
> **Domain expert:** "No — the **Launch Frame** belongs to the **Slingshot**, not the camera."

<<<<<<< HEAD
> **Dev:** "Does the **Run Camera** follow the player?"
> **Domain expert:** "It follows the **Launch Target** during a **Run**; player is not the current gameplay term for that object."

> **Dev:** "Does the **Run Camera** use the **Launch Target** transform directly?"
> **Domain expert:** "No — it frames the **Launch Target** through a **Run Camera Anchor** so camera framing can stay stable during physics motion."

> **Dev:** "Which camera is active before the **Run** begins?"
> **Domain expert:** "The **Pre-Launch Camera** frames the **Slingshot** until the **Run Camera** takes over after **Launch**."

=======
>>>>>>> refs/heads/main
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
- "More segments" means visual points in the **Band Wrap**, not physics bodies or gameplay samples.
- "Collider outline" means the **Launch Target Silhouette** in the **Pull Plane**, not the whole 3D collider volume.
- "Silhouette" means the inflated convex outer outline of the **Launch Target** for **Band Shape** purposes, not every concave collider detail.
- "3D wrap" is not a **Band Shape** concern; the **Band** wraps the band-height **Launch Target Silhouette**.
- "Back side" and "pulled side" resolve to **Pulled Side**; do not choose **Band Wrap** by nearest or shortest contour side.
<<<<<<< HEAD
- "Player" and "target" were used ambiguously for the controlled object during a **Run**; resolved as **Launch Target**.
- "Camera target", "camera pivot", and "follow target" resolve to **Run Camera Anchor** when discussing **Run Camera** framing.
- "Idle camera" and "static camera" resolve to **Pre-Launch Camera** when discussing the camera before a **Run**.
=======
>>>>>>> refs/heads/main
