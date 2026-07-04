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

**Run Reward Breakdown**:
The immutable source-by-source explanation of currency earned during one ended **Run**.
_Avoid_: UI animation state, wallet history, balance ledger

**Run Reward Source**:
A player-facing aggregate cause that contributes one amount to a **Run Reward Breakdown**.
_Avoid_: UI row, pickup type, reward event, Currency Definition

**Run Reward Contributor**:
A gameplay rule or mechanic that supplies aggregate **Run Reward Sources** for an ended **Run**.
_Avoid_: Run Ended UI, wallet committer, view presenter

**Distance Bonus**:
A **Run Reward Source** based on the distance reached by an ended **Run**.
_Avoid_: Run Distance Display, Best Run Distance, distance text

**Air Time Bonus**:
A **Run Reward Source** based on **Run Air Time** during an ended **Run**.
_Avoid_: Airborne animation, jump reward, hang-time text

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
- A **Run Reward Breakdown** explains the sources that make up run-earned currency.
- A **Run Reward Source** contributes to one **Run Reward Breakdown** as an aggregate, not as per-event history.
- **Run Reward Contributors** supply **Run Reward Sources** before the **Run Reward Breakdown** is shown.
- **Run Currency Snapshot** remains the total currency amount granted from a **Run Reward Breakdown**.
- **Distance Bonus** converts reached run distance into currency without replacing **Run Distance Display**.
- **Distance Bonus** provides repeatable run-progress income across **Runs** in one **Level Session** without making **Coin Pickup Reward** irrelevant.
- **Air Time Bonus** converts **Run Air Time** into currency without depending on **Character Presentation Mode**.
- **Air Time Bonus** is a secondary reward source for ramps and airtime, not the main run income path.
- **Coin Pickup Reward** may differ from the base **Currency Grant**.
- **Coin Pickup Reward** should be meaningful enough that collected **Coin Pickups** feel worth deliberate route choices.
- **Coin Pickup Fractional Carry** stays scoped to one **Run**.

## Example dialogue

> **Dev:** "Should the run-ended screen show total coins?"
> **Domain expert:** "No - it shows the **Run Currency Snapshot**, while **Currency Balance** is the spendable total."

> **Dev:** "Should picked-up coins and distance bonus become separate balances?"
> **Domain expert:** "No - they are **Run Reward Sources** in one **Run Reward Breakdown**."

> **Dev:** "Should every coin pickup appear as its own result-screen source?"
> **Domain expert:** "No - coin pickups roll up into one aggregate **Run Reward Source**."

> **Dev:** "Should the Run Ended UI know how distance and air time become coins?"
> **Domain expert:** "No - **Run Reward Contributors** supply generic **Run Reward Sources**."

> **Dev:** "Should distance and distance coins be one result line?"
> **Domain expert:** "No - distance is shown as **Run Distance Display**, while **Distance Bonus** is a **Run Reward Source**."

> **Dev:** "Should air-time coins use the character's airborne animation?"
> **Domain expert:** "No - **Air Time Bonus** uses **Run Air Time**."

> **Dev:** "Should airtime replace pickups or distance as the main income path?"
> **Domain expert:** "No - **Air Time Bonus** is a secondary reward source for ramps and air beats."

> **Dev:** "Does a one-coin pickup with a multiplier always become two coins?"
> **Domain expert:** "No - **Coin Pickup Reward** uses whole coins and **Coin Pickup Fractional Carry** keeps the remainder for later pickups in the same **Run**."

> **Dev:** "If earlier runs already collected nearby pickups, should a later failed run earn nothing?"
> **Domain expert:** "No - **Distance Bonus** still gives repeatable run-progress income, while **Coin Pickup Reward** remains the meaningful pickup payout."

## Flagged ambiguities

- "Resource" is avoided for gameplay currency because it collides with Unity folder language.
- "Reward" resolves to **Coin Pickup Reward** only after modifiers are applied; the authored amount is **Currency Grant**.
- "Coins collected" during a result screen resolves to **Run Currency Snapshot**, not **Currency Balance**.
- "Coin sources" during a result screen resolves to **Run Reward Sources**, not separate **Currency Definitions**.
- "Reward source" resolves to an aggregate contribution, not an individual reward event.
- "Reward provider" resolves to **Run Reward Contributor** when discussing source extensibility.
- "Distance reward" resolves to **Distance Bonus**, not **Run Distance Display**.
- "Air reward" resolves to **Air Time Bonus**, not airborne character presentation.
- "Pickup income" resolves to meaningful **Coin Pickup Reward** amounts, not one-coin decorative collectibles.
- "Wallet" resolves to **Currency Balance** when discussing spendable currency.
