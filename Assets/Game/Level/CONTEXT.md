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

## Example dialogue

> **Dev:** "Is a **Run Progression Band** a gameplay state?"
> **Domain expert:** "No - it is a distance range used for reach, economy, and pressure tuning."

> **Dev:** "Does **Soft Containment** end the **Run**?"
> **Domain expert:** "No - it shapes the **Run Surface** to redirect the **Launch Target** back into the **Run Course**."

> **Dev:** "Is the **Risk Line** required for completion?"
> **Domain expert:** "No - the **Main Playable Line** should remain the expected route to the finish."

> **Dev:** "Should high side riding end the **Run**?"
> **Domain expert:** "No - **Side Bank** riding remains **Run Surface** traversal; cresting the **Course Lip** can set up escape from **Soft Containment**."

## Flagged ambiguities

- "Chunk" describes asset packaging, while **Run Course Section** describes gameplay-design purpose.
- "Edge" resolves to **Side Bank** when still inside **Soft Containment** and **Course Lip** when discussing the upper escape edge.
- "Progression zone", "upgrade zone", and "reach band" resolve to **Run Progression Band**.
- "Ramp" resolves to **Run Surface** unless a ramp-specific gameplay term becomes necessary.
- "Finish marker" resolves to readable level dressing; **Run Finish** is the gameplay contact term.
- "Ladybug level" resolves to **Ladybug Rooftop Half-Tube** only when discussing this specific run-course theme.
