# Upgrades

Upgrades cover player-owned progression, upgrade levels, stat modifiers, and run-scoped stat resolution.

## Language

**Upgrade**:
A player-owned progression item that improves one gameplay-facing stat across future **Runs**.
_Avoid_: Powerup, temporary boost, shop card

**Upgrade Definition**:
The authored identity, display data, cost progression, and effect progression for one **Upgrade**.
_Avoid_: Upgrade state, UI card, stat id

**Upgrade Id**:
The stable identity used to store ownership for one **Upgrade**.
_Avoid_: Display name, Gameplay Stat Id, asset name

**Upgrade Level**:
The owned progression value for one **Upgrade**.
_Avoid_: Rank text, purchase count, stat value

**Upgrade Catalog**:
The authored ordered set of **Upgrade Definitions** and their shared **Purchase Currency**.
_Avoid_: UI list, wallet, save data

**Upgrade Progression**:
The authored rule that evaluates cost or effect values for an **Upgrade Level**.
_Avoid_: Animation curve, balance table, level formula

**Upgrade Preview**:
The immutable read model shown for one **Upgrade** during **Run Preparation**.
_Avoid_: Upgrade Definition, purchase result, UI widget

**Upgrade Purchase Service**:
The boundary that attempts to spend currency and increase an **Upgrade Level** as one operation.
_Avoid_: Button handler, Currency Storage, Upgrade Preview

**Upgrade Purchase Result**:
The outcome of one attempted upgrade purchase.
_Avoid_: Exception, UI state, balance value

**Gameplay Stat Id**:
The authored identity of a gameplay stat that modifiers can affect.
_Avoid_: Upgrade Id, field name, display label

**Modifier Source**:
An owned or temporary cause that contributes one or more **Runtime Modifiers**.
_Avoid_: Upgrade, stat resolver, config

**Live Modifier Source**:
A **Modifier Source** evaluated from current conditions instead of frozen into a **Run Modifier Snapshot**.
_Avoid_: Upgrade Level, saved modifier, catalog entry

**Runtime Modifier**:
A resolved stat change targeting one **Gameplay Stat Id**.
_Avoid_: Upgrade effect, raw level value, balance rule

**Modifier Operation**:
The authored math operation used by a **Runtime Modifier** when resolving a gameplay stat.
_Avoid_: Sort order, source priority, stat type

**Run Modifier Snapshot**:
The frozen set of **Runtime Modifiers** resolved from owned **Upgrade Levels** for one upcoming **Run**.
_Avoid_: Upgrade save data, live modifier list, currency state

**Active Run Modifier Snapshot**:
The current **Run Modifier Snapshot** used by gameplay until the following **Run** ends.
_Avoid_: Upgrade catalog, stat cache, live source

**Run Modifier Snapshot Factory**:
The boundary that builds a **Run Modifier Snapshot** from owned **Upgrade Levels** and **Upgrade Definitions**.
_Avoid_: Purchase service, stat resolver, UI builder

**Gameplay Stat Resolver**:
The boundary that combines a base stat value with applicable **Runtime Modifiers**.
_Avoid_: Upgrade system, config reader, source registry

## Relationships

- An **Upgrade** has one **Upgrade Definition**.
- An **Upgrade Definition** has one **Upgrade Id**.
- An **Upgrade Id** is separate from **Gameplay Stat Id**.
- A player may own one **Upgrade Level** per **Upgrade**.
- An **Upgrade Catalog** contains **Upgrade Definitions** and has one **Purchase Currency**.
- An **Upgrade Definition** has cost and effect **Upgrade Progressions**.
- **Upgrade Preview** is read-only and does not mutate **Upgrade Levels**.
- A purchased **Upgrade Level** should change the player-facing **Upgrade Preview** instead of becoming hidden micro-progression.
- **Upgrade Purchase Service** is the purchase mutator for **Upgrade Levels**.
- A **Runtime Modifier** targets one **Gameplay Stat Id** and has one **Modifier Operation**.
- **Run Modifier Snapshot** is created before **Pre-Launch** and remains stable for the following **Run**.
- **Gameplay Stat Resolver** reads the **Active Run Modifier Snapshot** and active **Live Modifier Sources**.

## Example dialogue

> **Dev:** "Does buying an **Upgrade** change launch power during an active pull?"
> **Domain expert:** "No - owned **Upgrade Levels** are resolved into a **Run Modifier Snapshot** before **Pre-Launch**."

> **Dev:** "Is **Upgrade Id** the same as **Gameplay Stat Id**?"
> **Domain expert:** "No - **Upgrade Id** identifies ownership, while **Gameplay Stat Id** identifies the stat being modified."

> **Dev:** "Can the UI spend coins directly?"
> **Domain expert:** "No - it requests **Upgrade Purchase Service**, which owns the currency-spend and level-change operation."

> **Dev:** "Can an upgrade purchase leave the shown effect unchanged because the effect rounded to the same value?"
> **Domain expert:** "No - a purchased **Upgrade Level** should make the **Upgrade Preview** visibly advance."

## Flagged ambiguities

- "Powerup" resolves to **Upgrade** only for owned run-to-run progression; temporary effects resolve to **Modifier Source**.
- "Level one" is not the baseline; **Upgrade Level** zero is the baseline owned state.
- "Upgrade value" may mean cost, effect, preview text, or owned level; use the precise term.
- "Meaningful upgrade" resolves to an **Upgrade Level** that changes both **Upgrade Preview** and future **Run Modifier Snapshot** outcomes.
- "Stat" resolves to **Gameplay Stat Id** when authoring modifier targets.
- "Snapshot" in upgrade language resolves to **Run Modifier Snapshot**, not save data.
