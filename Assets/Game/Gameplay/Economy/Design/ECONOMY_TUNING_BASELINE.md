# Economy Tuning Baseline

This document records the accepted numeric baseline for run-earned currency and upgrade pacing. It complements the Economy and Upgrades `CONTEXT.md` glossary files; it is the tuning reference, not terminology guidance.

## Accepted Rules

- Run income should combine meaningful one-time **Coin Pickup Reward** payouts with repeatable **Distance Bonus** income.
- **Distance Bonus** should support later runs after nearby pickups have already been collected, but should not dominate pickup income.
- **Coin Pickups** should grant enough currency to justify route choices.
- **Upgrades** should use 10 meaningful levels, not 20-25 micro-levels.
- Every purchased **Upgrade Level** should visibly advance the **Upgrade Preview**.
- Level 10 should produce a clearly felt gameplay difference.
- A decent first failed run should usually afford one useful reach/control upgrade, not several upgrades at once.
- **CoinPickupMultiplier** should be useful but delayed enough that it is not the obvious first purchase every run.

## Run Income Baseline

| Source | Baseline |
| --- | ---: |
| Distance Bonus | 0.5 coins per meter |
| Air Time Bonus | 5 coins per second |
| Regular Coin Pickup | 5 coins |
| Big Coin Pickup | 25 coins |

Expected first-run affordability target:

- A decent 85-125m failed run should usually earn enough for one early Slingshot, Speed, or Steering upgrade if the player collects safe early pickups.
- Early reach/control upgrade level 1 costs target 150 coins.
- CoinPickupMultiplier level 1 costs target 300 coins.

## Upgrade Effect Baseline

| Level | SlingshotLaunchPower | PlayerMaxSpeed | PlayerSteeringResponsiveness | CoinPickupMultiplier |
| ---: | ---: | ---: | ---: | ---: |
| 1 | x1.1 | x1.1 | x1.1 | x1.2 |
| 2 | x1.2 | x1.2 | x1.2 | x1.4 |
| 3 | x1.3 | x1.3 | x1.3 | x1.6 |
| 4 | x1.4 | x1.4 | x1.4 | x1.8 |
| 5 | x1.5 | x1.5 | x1.5 | x2.0 |
| 6 | x1.6 | x1.6 | x1.6 | x2.2 |
| 7 | x1.7 | x1.7 | x1.7 | x2.4 |
| 8 | x1.8 | x1.8 | x1.8 | x2.6 |
| 9 | x1.9 | x1.9 | x1.9 | x2.8 |
| 10 | x2.0 | x2.0 | x2.0 | x3.0 |

## Upgrade Cost Baseline

Use the same reach/control cost curve for SlingshotLaunchPower, PlayerMaxSpeed, and PlayerSteeringResponsiveness.

| Level | Reach/Control Cost | CoinPickupMultiplier Cost |
| ---: | ---: | ---: |
| 1 | 150 | 300 |
| 2 | 250 | 500 |
| 3 | 350 | 700 |
| 4 | 500 | 1000 |
| 5 | 700 | 1400 |
| 6 | 950 | 1900 |
| 7 | 1250 | 2500 |
| 8 | 1600 | 3200 |
| 9 | 2000 | 4000 |
| 10 | 2500 | 5000 |

## Validation Targets

- First useful failed run: 85-125m, one useful early reach/control purchase possible with safe pickup collection.
- Runs 2-3: first obstacle/ramp region becomes more reliable after early reach upgrades.
- Runs 5-10: first completion should be plausible with upgrades and improving control.
- Risk/reward pickups should speed progression, not gate basic reach upgrades.
- Income multiplier should shorten later progression without replacing reach/control upgrades.
- Air Time Bonus should make ramps feel rewarding without becoming a main income source.

## Open Questions

- Exact scene pickup placement may need adjustment after regular/big pickup values change from 1/5 to 5/25.
- Final validation needs playtest or simulation data for reachable pickups per run band.
