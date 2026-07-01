# Pickups

Pickups cover collectible scene objects, pickup definitions, collection contacts, and per-level pickup state.

## Language

**Pickup**:
The collectible object touched during a **Run** to request a collection.
_Avoid_: Coin visual, reward, currency object

**Pickup Definition**:
The authored collection definition for a **Pickup**, including its **Currency Grant**.
_Avoid_: Coin Definition, visual value, pickup state

**Pickup Collection Controller**:
The gameplay boundary that decides whether a **Pickup** contact becomes an accepted collection.
_Avoid_: Pickup, trigger, Currency Storage

**Pickup Collection Event**:
The accepted collection fact emitted after a **Pickup** is collected.
_Avoid_: Trigger enter, grant preview, balance update

**Level Pickup State**:
The per-**Level Session** record of which **Pickups** have already been collected.
_Avoid_: Save data, pickup component state, scene search

**Pickup Layer**:
The physics layer used by collectible pickup triggers.
_Avoid_: Run Surface layer, player layer, tag

**Player Tag**:
The Unity tag used to identify the player collider for pickup collection.
_Avoid_: Pickup Layer, Launch Target term, character name

**Pickup Contact**:
The raw contact between a **Pickup** trigger and another collider.
_Avoid_: Accepted collection, reward, balance grant

**Coin Pickup**:
A **Pickup** whose **Pickup Definition** grants coin currency.
_Avoid_: Currency Balance, coin reward text

**Coin Pickup Visual**:
The rendered presentation of a **Coin Pickup**.
_Avoid_: Pickup, Pickup Definition, Currency Grant

**Big Coin Pickup**:
A higher-value **Coin Pickup** variant.
_Avoid_: Different currency, upgrade token

## Relationships

- A **Pickup** has one **Pickup Definition**.
- A **Pickup Definition** has one **Currency Grant**.
- A **Pickup** reports **Pickup Contact**; it does not decide collectability.
- **Pickup Collection Controller** converts valid **Pickup Contact** into **Pickup Collection Event**.
- **Level Pickup State** prevents the same **Pickup** from being collected twice in one **Level Session**.
- **Pickup Layer** and **Player Tag** are authoring contracts for collection contacts.
- **Coin Pickup** and **Big Coin Pickup** differ by grant amount, not by currency concept.
- A **Coin Pickup** may have one **Coin Pickup Visual**.
- A **Big Coin Pickup** may use a larger **Coin Pickup Visual** without changing the meaning of **Big Coin Pickup**.
- **Coin Pickup Visual** does not determine collection, availability, or reward amount.
- **Coin Pickup Visual** may vary its visual phase between instances without changing pickup behavior.
- **Coin Pickup Visual** motion belongs to the visual transform, while the **Pickup** root owns collection contacts.
- External coin meshes are source material for **Coin Pickup Visual**, not gameplay objects.

## Example dialogue

> **Dev:** "Does a pickup grant coins as soon as any collider touches it?"
> **Domain expert:** "No - **Pickup** reports **Pickup Contact**, and **Pickup Collection Controller** decides whether it becomes a **Pickup Collection Event**."

> **Dev:** "Is a collected pickup state saved forever?"
> **Domain expert:** "No - **Level Pickup State** belongs to the current **Level Session**."

> **Dev:** "Should a spinning coin mesh change how many coins the player earns?"
> **Domain expert:** "No - that is **Coin Pickup Visual**; reward meaning belongs to **Pickup Definition**."

## Flagged ambiguities

- "Coin" resolves to **Coin Pickup** for the collectible object and **Currency Definition** for the spendable currency identity.
- "Big coin" resolves to **Big Coin Pickup** for reward value and to scaled **Coin Pickup Visual** for presentation.
- "Coin mesh", "coin material", "coin spin", and "coin phase" resolve to **Coin Pickup Visual**, not **Coin Pickup**.
- "Downloaded coin" resolves to source material for **Coin Pickup Visual**, not a scene-authored **Coin Pickup**.
- "Collected" means an accepted **Pickup Collection Event**, not merely a trigger contact.
- "Pickup state" resolves to **Level Pickup State** when discussing collected/not-collected status.
- "Layer" and "tag" are distinct collection contracts: **Pickup Layer** belongs to pickups and **Player Tag** identifies the collecting collider.
