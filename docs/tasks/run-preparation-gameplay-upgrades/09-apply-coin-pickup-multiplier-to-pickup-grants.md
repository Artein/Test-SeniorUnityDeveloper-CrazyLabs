## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Apply `CoinPickupMultiplier` to coin currency grants from collectible pickups only. The completed slice should resolve the active multiplier through `GameplayStatResolver`, apply it to base coin pickup grants with per-run fractional carry and floor behavior, grant the final integer amount to app-session currency storage, record final earned totals in the run currency accumulator, and report both base and final amounts in pickup collection events.

This slice should not apply the multiplier to non-coin currencies, mission payouts, ads, IAP grants, debug grants, or future non-pickup sources.

## Acceptance criteria

- [ ] Coin pickup grants pass through a multiplier resolver before writing to currency storage.
- [ ] Non-coin pickup grants bypass `CoinPickupMultiplier`.
- [ ] Non-pickup currency grants bypass `CoinPickupMultiplier`.
- [ ] Fractional multiplied coin amounts carry within the current run and floor the current payout.
- [ ] Fractional carry resets with the run currency accumulator for the next run.
- [ ] Fractional carry is not stored in app-session currency storage or run currency snapshots.
- [ ] App-session currency storage receives final applied integer amounts.
- [ ] Run currency accumulator records final applied integer amounts only.
- [ ] Pickup collection events include base currency grant amount and final applied amount.
- [ ] Existing x1 multiplier behavior preserves current pickup collection totals.

## Verification

- EditMode tests:
  - x1 multiplier grants the authored base amount.
  - Fractional multiplier carries fractional remainder across pickups in one run.
  - Large multiplier grants the expected final integer amount.
  - Run reset clears fractional carry.
  - Non-coin pickup grants bypass the coin multiplier.
  - Non-pickup grants bypass the coin multiplier.
  - Run currency snapshot stores final earned totals only.
  - Pickup collection event payload contains base and final amounts.
- PlayMode tests:
  - Gameplay pickup collection in Running applies the active coin pickup multiplier and records final totals.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Author a temporary coin multiplier above x1, collect several coin pickups, and confirm visible balance/earned totals match floor-plus-carry behavior.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Extract Economy Currency Grant/Spend Path
- 04 - Add Run Modifier Snapshot And Gameplay Stat Resolver
