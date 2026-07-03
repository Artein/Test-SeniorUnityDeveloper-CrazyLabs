# Level

Level covers authored run-course structure, pacing language, progression bands, and containment intent.

## Language

**Run Course**:
The authored downhill playable path for one **Run**.
_Avoid_: Scene, map, level object

**Run Course Section**:
A contiguous authored part of a **Run Course** with a clear surface profile and gameplay purpose.
_Avoid_: Chunk, piece, asset slice

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
_Avoid_: Zone, checkpoint, difficulty tier

**Soft Containment**:
**Run Surface** shaping that usually redirects the **Launch Target** back into the **Run Course** without making escape impossible.
_Avoid_: Wall, hard boundary, guard rail

**Side Bank**:
The banked side of a **Half-Tube Course** that remains traversable **Run Surface**.
_Avoid_: Wall, guard rail, boundary

**Course Lip**:
The upper edge of a **Half-Tube Course** where the **Launch Target** can leave **Soft Containment**.
_Avoid_: Hidden wall, hard boundary, side collider

**Main Playable Line**:
The expected traversable route through a **Run Course** for a competent player.
_Avoid_: Perfect path, center line, speedrun route

**Risk Line**:
An optional route that asks more from the player in exchange for higher reward or faster progress.
_Avoid_: Required path, trap, secret route

**Coin Line**:
A readable pickup placement pattern that invites a player toward a route.
_Avoid_: Pickup count, reward balance, breadcrumb only

**Finish Approach**:
The final readable part of a **Run Course** leading into **Run Finish**.
_Avoid_: End screen, checkpoint, victory state

**Finish Presentation**:
Readable level dressing that communicates the end of the **Finish Approach** without owning **Run Finish** behavior.
_Avoid_: Run Finish, goal trigger, obstacle blocker

**Finish Threshold Visual**:
A visual-only crossing mark aligned with authoritative **Run Finish** so the player can read the exact finish moment.
_Avoid_: Trigger, collider, blocker, finish logic

**Finish Celebration**:
One-shot presentation feedback after an accepted successful **Run Result** from **Run Finish**, such as confetti.
_Avoid_: Approach marker, looping finish hint, reward grant

**Half-Tube Course**:
A **Run Course** shaped with banked sides so lateral steering can redirect downhill motion.
_Avoid_: Flat slope, tunnel, rail corridor

**Ladybug Rooftop Half-Tube**:
The Ladybug-themed **Half-Tube Course** used for the current downhill run level.
_Avoid_: Generic city level, mesh source, asset folder

## Relationships

- A **Run Course** may have **Run Course Beats**.
- A **Run Course Beat** may contain one or more **Run Course Sections**.
- A **Run Course Section** belongs to one **Run Course**.
- A **Run Course** has one **Target Run Duration**.
- A **Run Course** has one **Target First Completion Run Count**.
- A **Run Course** may have **Run Progression Bands**.
- A **Run Progression Band** describes tuning pressure, not gameplay lifecycle state.
- **Soft Containment** is achieved by **Run Surface** shape.
- A **Half-Tube Course** has **Side Banks**.
- **Side Banks** are **Run Surface** used for **Soft Containment**.
- A **Course Lip** marks where **Soft Containment** can be escaped.
- Riding a **Side Bank** should remain recoverable unless the **Launch Target** crests a **Course Lip**.
- **Main Playable Line** should remain understandable without requiring **Risk Line** choices.
- **Coin Line** can communicate route intent without changing the **Run Course** structure.
- **Finish Approach** belongs to the **Run Course**, while **Run Finish** belongs to gameplay contact language.
- A course-authored **Run Finish** contact should live with the **Run Course** it completes, so the **Finish Approach** has one authoritative finish contact.
- **Finish Presentation** belongs to the **Finish Approach**.
- **Finish Presentation** communicates where **Run Finish** is expected but does not define **Run Finish** behavior.
- **Finish Presentation** may reuse visual assets, but inherited **Run Obstacle** contact behavior remains separate.
- **Finish Threshold Visual** belongs to **Finish Presentation**.
- **Finish Threshold Visual** aligns with authoritative **Run Finish** but does not own **Run Finish** behavior.
- **Finish Threshold Visual** may fade out during **Finish Celebration** and reset for **Run Preparation**.
- **Finish Celebration** belongs after an accepted successful **Run Result** from **Run Finish**, not before the player crosses it.
- **Finish Celebration** is driven by the accepted successful **Run Result**, not by raw **Run Finish** contact entry.
- **Finish Celebration** may use particles, audio, or camera-facing presentation cues, but it must not grant rewards or decide the **Run Result**.
- A **Run Obstacle** near **Finish Presentation** remains separate gameplay contact language.

## Example dialogue

> **Dev:** "Is a **Run Progression Band** a gameplay state?"
> **Domain expert:** "No - it is a distance range used for reach, economy, and pressure tuning."

> **Dev:** "Does **Soft Containment** end the **Run**?"
> **Domain expert:** "No - it shapes the **Run Surface** to redirect the **Launch Target** back into the **Run Course**."

> **Dev:** "Is the **Risk Line** required for completion?"
> **Domain expert:** "No - the **Main Playable Line** should remain the expected route to the finish."

> **Dev:** "Should high side riding end the **Run**?"
> **Domain expert:** "No - **Side Bank** riding remains **Run Surface** traversal; cresting the **Course Lip** can set up escape from **Soft Containment**."

> **Dev:** "Can the finish billboard be the **Run Finish**?"
> **Domain expert:** "No - the billboard belongs to **Finish Presentation**; **Run Finish** is the gameplay contact that completes the **Run**."

> **Dev:** "Can an obstacle billboard become **Finish Presentation**?"
> **Domain expert:** "Yes, as visual dressing only; any **Run Obstacle** contact behavior remains separate."

> **Dev:** "Is the checkered crossing texture the **Run Finish**?"
> **Domain expert:** "No - it is the **Finish Threshold Visual** aligned with **Run Finish** so the crossing moment is readable."

> **Dev:** "Can one course keep both a course-root **Run Finish** and a legacy scene-level **Run Finish**?"
> **Domain expert:** "No - the **Finish Approach** should have one authoritative **Run Finish** contact owned with its **Run Course**."

> **Dev:** "Should confetti run before the player reaches the finish?"
> **Domain expert:** "No - use static **Finish Presentation** for approach readability and reserve **Finish Celebration** for the accepted successful **Run Finish**."

> **Dev:** "Should the finish contact itself start confetti?"
> **Domain expert:** "No - **Finish Celebration** waits for the accepted successful **Run Result** from **Run Finish**."

## Flagged ambiguities

- "Chunk" describes asset packaging, while **Run Course Section** describes gameplay-design purpose.
- "Edge" resolves to **Side Bank** when still inside **Soft Containment** and **Course Lip** when discussing the upper escape edge.
- "Progression zone", "upgrade zone", and "reach band" resolve to **Run Progression Band**.
- "Ramp" resolves to **Run Surface** unless a ramp-specific gameplay term becomes necessary.
- "Finish marker", "finish billboard", and "finish sign" resolve to **Finish Presentation** when discussing readable level dressing; **Run Finish** is the gameplay contact term.
- "Finish line texture", "checkered stripe", "chessboard line", and "crossing line" resolve to **Finish Threshold Visual** when discussing the readable crossing mark.
- "Confetti", "finish particles", and "celebration VFX" resolve to **Finish Celebration** when triggered by accepted completion, not approach readability.
- "Blocker" resolves to **Run Obstacle** only when discussing physical contact behavior, not finish visuals.
- "Ladybug level" resolves to **Ladybug Rooftop Half-Tube** only when discussing this specific run-course theme.
