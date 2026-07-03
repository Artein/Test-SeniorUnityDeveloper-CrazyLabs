# Slingshot

Slingshot covers pull interpretation, band presentation, launch-target holding, and the handoff that can become a run launch.

## Language

**Slingshot**:
The launch device that prepares and reports a potential **Launch**.
_Avoid_: Launcher, rope input

**Band**:
The visible elastic element stretched by a **Pull**.
_Avoid_: Rope, string, trajectory line

**Rest Point**:
The unloaded position where the **Band** and held **Launch Target** align before an **Active Pull**.
_Avoid_: Level start, player spawn, reset pose

**Pull**:
The interpreted player drag that moves the held **Launch Target** through the **Slingshot**.
_Avoid_: Swipe, launch, camera drag

**Active Pull**:
A currently captured **Pull** that is controlling the held **Launch Target**.
_Avoid_: Held state, touch down, pull hint

**Pull Point**:
The interpreted position that holds the **Launch Target** during an **Active Pull**.
_Avoid_: Band midpoint, touch point, contact point

**Pull Plane**:
The plane where **Pull** positions are interpreted for the **Slingshot**.
_Avoid_: Screen plane, ground plane, run surface

**Pull Strength**:
The normalized backward depth of a **Pull** before gameplay modifiers are applied.
_Avoid_: Launch power, upgrade power, impulse

**Pull Offset**:
The lateral component of a **Pull** within the **Pull Plane**.
_Avoid_: Steering, camera offset, side swipe

**Normalized Pull**:
The presentation-facing amount of the current **Active Pull**.
_Avoid_: Raw pull distance, launch power

**Normalized Pull Offset**:
The presentation-facing lateral amount of the current **Active Pull**.
_Avoid_: Raw Pull Offset, steering input

**Pull Release**:
The end of an **Active Pull**.
_Avoid_: Launch, cancel, tap up

**Launch Impulse**:
The resolved physical push applied to the **Launch Target** for an accepted **Launch**.
_Avoid_: Pull Strength, launch config, band force

**Launch Impulse Calculator**:
The gameplay boundary that resolves a **Launch Impulse** from accepted launch facts.
_Avoid_: Slingshot, physics applier

**Launch Impulse Application**:
The boundary that applies a resolved **Launch Impulse** to the **Launch Target**.
_Avoid_: Launch math, pull interpreter

**Launch Target**:
The slingshot-facing role of the gameplay object while held by the **Slingshot** and receiving **Launch Impulse**.
_Avoid_: Run Body, Character, avatar, player model

**Band Center**:
The alignment point on the **Launch Target** used by the **Band** while held.
_Avoid_: Collider center, character center, mesh pivot

**Launch Target Collider Root**:
The collision-shape role under the **Launch Target** used for gameplay contact and band silhouette.
_Avoid_: Character visual anchor, model root

**Launch Target Silhouette**:
The pull-plane outline of the held **Launch Target** used by a taut **Band Shape**.
_Avoid_: Mesh outline, whole volume, visual bounds

**Band Shape**:
The visible path of the **Band**.
_Avoid_: Launch math, physics rope, force curve

**Band Contact Point**:
A tangent point where the visible **Band Shape** meets the **Launch Target Silhouette**.
_Avoid_: Closest point, Pull Point, collision contact

**Band Wrap**:
The curved part of a **Band Shape** that follows the **Launch Target Silhouette**.
_Avoid_: Rope simulation, launch guide

**Pulled Side**:
The side of the **Launch Target Silhouette** facing the current **Pull** direction.
_Avoid_: Back side, nearest side, direct path

**Pull Hint**:
The idle guidance that teaches the player where and how to pull.
_Avoid_: Touch Indicator, tutorial text

**Touch Indicator**:
The live feedback that follows the real **Active Pull** input.
_Avoid_: Pull Hint, cursor, aiming reticle

**Pre-Launch Rig Pose**:
The authored alignment of **Slingshot**, **Launch Target**, and **Rest Point** before a new **Pull**.
_Avoid_: Level start position, camera pose, respawn point

**Slingshot Presentation Context**:
The presentation-facing facts derived from **Active Pull** and accepted launch activity.
_Avoid_: Slingshot view state, gameplay state

**Launch Push**:
The character-presentation handoff that represents an accepted **Launch** pushing the character into motion.
_Avoid_: Band Release Recoil, Gameplay State, Pull Anticipation

**Band Release Recoil**:
The post-release band motion back toward rest.
_Avoid_: Launch Push, active pull, run movement

## Relationships

- A **Slingshot** has one **Band** and one **Rest Point**.
- A **Slingshot** may have one **Active Pull**.
- An **Active Pull** has **Pull Strength**, **Pull Offset**, and a **Pull Point**.
- **Pull Strength** and **Pull Offset** describe interpreted pull facts, not final launch results.
- **Pull Release** may become an accepted **Launch**, but not every release launches.
- **Launch Impulse Calculator** resolves the push; **Launch Impulse Application** applies it.
- The **Launch Target** becomes the Gameplay **Run Body** after **Launch** starts **Running**.
- **Band Shape** is presentation, not launch force.
- A taut **Band Shape** may include **Band Contact Points** and **Band Wrap**.
- **Pulled Side** follows the current **Pull** direction.
- **Pull Hint** is idle guidance; **Touch Indicator** is live feedback.
- **Pre-Launch Rig Pose** aligns the **Slingshot** and **Launch Target** before input begins.

## Example dialogue

> **Dev:** "Does adding more points to the **Band Wrap** increase launch power?"
> **Domain expert:** "No - **Band Shape** is visual, while gameplay resolves the **Launch Impulse** from accepted launch facts."

> **Dev:** "Is every **Pull Release** a **Launch**?"
> **Domain expert:** "No - a weak or invalid release can end the **Active Pull** without starting a **Run**."

> **Dev:** "Does the camera decide where a **Pull** goes?"
> **Domain expert:** "No - **Pull Plane** and slingshot axes define pull interpretation."

## Flagged ambiguities

- "Rope" resolves to **Band** for the launch elastic unless discussing a purely visual asset.
- "Natural rubber band behavior" resolves to taut **Band Shape**, not physical rope simulation.
- "Pointer" resolves to **Pull Hint** for idle guidance and **Touch Indicator** for live feedback.
- "Rest position" resolves to **Rest Point**, not the level start or app reset pose.
- "Pull point" and "band middle" are not interchangeable; **Pull Point** holds the **Launch Target** while **Band Contact Points** shape the visible band.
- "Back side" resolves to **Pulled Side** when choosing the wrap side.
- "Launch power" is not **Pull Strength** once gameplay modifiers or impulse resolution are involved.
