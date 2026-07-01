# Economy

Economy covers currency identity, balances, grants, run-earned currency, and spendable amounts.

## Language

**Currency Definition**:
The authored identity of a spendable currency bucket.
_Avoid_: Resource Definition, SKU, coin type

**Currency Grant**:
The authored positive amount of one **Currency Definition** granted by gameplay content.
_Avoid_: Currency reward, balance change, payout event

**Currency Balance**:
The current amount held for one **Currency Definition**.
_Avoid_: Wallet, inventory amount, run earnings

**Currency Storage**:
The boundary that grants, reads, and spends **Currency Balances** by **Currency Definition**.
_Avoid_: Save file, upgrade store, pickup collector

**Purchase Currency**:
The **Currency Definition** used to buy a set of **Upgrades**.
_Avoid_: Upgrade cost type, shop coin

**Run Currency Accumulator**:
The run-scoped holder that accumulates accepted **Currency Grant** amounts during the current **Run**.
_Avoid_: Currency Storage, reward preview, pickup counter

**Run Currency Snapshot**:
The immutable final currency amounts earned during one ended **Run**.
_Avoid_: Total balance, live run counter, wallet snapshot

**Final Pickup Grant Amount**:
The integer amount actually added after pickup reward rules are applied.
_Avoid_: Base grant, preview amount, fractional amount

**Coin Pickup Reward**:
The whole-number coin payout accepted from a coin pickup after reward modifiers and carry are applied.
_Avoid_: Currency Grant, total coins, visual coin value

**Coin Pickup Fractional Carry**:
The run-scoped fractional remainder retained between coin pickups when reward modifiers produce fractions.
_Avoid_: Saved fraction, balance remainder, rounding error

## Relationships

- A **Currency Grant** references one **Currency Definition**.
- A **Currency Balance** belongs to one **Currency Definition**.
- **Currency Storage** owns **Currency Balances**.
- **Purchase Currency** is a **Currency Definition** used by upgrade purchase rules.
- **Run Currency Accumulator** records accepted run earnings, not total holdings.
- **Run Currency Snapshot** is captured from **Run Currency Accumulator** when a **Run** ends.
- **Coin Pickup Reward** may differ from the base **Currency Grant**.
- **Coin Pickup Fractional Carry** stays scoped to one **Run**.

## Example dialogue

> **Dev:** "Should the run-ended screen show total coins?"
> **Domain expert:** "No - it shows the **Run Currency Snapshot**, while **Currency Balance** is the spendable total."

> **Dev:** "Does a one-coin pickup with a multiplier always become two coins?"
> **Domain expert:** "No - **Coin Pickup Reward** uses whole coins and **Coin Pickup Fractional Carry** keeps the remainder for later pickups in the same **Run**."

## Flagged ambiguities

- "Resource" is avoided for gameplay currency because it collides with Unity folder language.
- "Reward" resolves to **Coin Pickup Reward** only after modifiers are applied; the authored amount is **Currency Grant**.
- "Coins collected" during a result screen resolves to **Run Currency Snapshot**, not **Currency Balance**.
- "Wallet" resolves to **Currency Balance** when discussing spendable currency.
