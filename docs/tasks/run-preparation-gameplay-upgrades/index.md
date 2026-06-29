# Run Preparation Gameplay Upgrades Implementation Issues

Parent PRD: [Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

Related ADRs:

- [ADR-0001: Use Asset-Backed Gameplay State IDs](../../adr/adr-0001-use-asset-backed-gameplay-state-ids.md)
- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for adding **Run Preparation**, app-session **Economy** spending, authored upgrades, stat resolution, Slingshot launch-power integration, movement upgrades, coin pickup multiplier, and one integrated gameplay scene smoke path.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Extract Economy Currency Grant/Spend Path](01-extract-economy-currency-grant-spend-path.md) | AFK | None | Currency naming, pickup grants, app-session balances, `TrySpend`, no `Resource` domain |
| 02 | [Add Upgrade Catalog, Definitions, Progressions, And Previews](02-add-upgrade-catalog-definitions-progressions-and-previews.md) | AFK | 01 | Designer-authored upgrades, icons, curves, overrides, level 0 semantics, previews |
| 03 | [Add Upgrade Progress And Atomic Purchase Flow](03-add-upgrade-progress-and-atomic-purchase-flow.md) | AFK | 01, 02 | Buy, insufficient, maxed, atomic spend and level, purchase results |
| 04 | [Add Run Modifier Snapshot And Gameplay Stat Resolver](04-add-run-modifier-snapshot-and-gameplay-stat-resolver.md) | AFK | 02, 03 | Central stat resolution, modifier order, frozen run snapshot |
| 05 | [Add Run Preparation State And Continue Handoff](05-add-run-preparation-state-and-continue-handoff.md) | AFK | 04 | Initial Run Preparation state, Continue to Pre-Launch, slingshot disabled before Continue |
| 06 | [Add Run Preparation UI Buy/Continue Path](06-add-run-preparation-ui-buy-continue-path.md) | AFK | 03, 05 | Visible coins, upgrade icons, buy buttons, maxed/disabled states, shallow UI |
| 07 | [Move Slingshot Launch Impulse Calculation To Gameplay](07-move-slingshot-launch-impulse-calculation-to-gameplay.md) | AFK | 04, 05 | Slingshot leaf boundary, `GameplaySlingshotLaunchConfig`, `SlingshotLaunchPower` |
| 08 | [Apply Player Movement Upgrade Stats](08-apply-player-movement-upgrade-stats.md) | AFK | 04, 05 | `PlayerMaxSpeed`, `PlayerSteeringResponsiveness` during Running |
| 09 | [Apply Coin Pickup Multiplier To Pickup Grants](09-apply-coin-pickup-multiplier-to-pickup-grants.md) | AFK | 01, 04 | `CoinPickupMultiplier`, fractional carry, base/final event amounts, final run totals |
| 10 | [Wire Gameplay Scene Upgrade Smoke Path](10-wire-gameplay-scene-upgrade-smoke-path.md) | AFK | 05, 06, 07, 08, 09 | End-to-end scene path: Run Preparation, buy, Continue, launch, run, end, Run Preparation |
| 11 | [Review Upgrade Tuning And Icons](11-review-upgrade-tuning-and-icons.md) | HITL | 02, 06, 10 | Final prices, max levels, curves, exact overrides, readable UI values, icon approval |

## Notes

- Keep **Slingshot** as a feature leaf. It reports Pull data; higher Gameplay calculates and applies final Launch Impulse.
- Keep **Run Preparation** as a gameplay state, not a modal layered over Pre-Launch.
- Keep first-slice balances and owned upgrade levels app-session only. Save format and app-restart persistence stay out of scope.
- Keep purchase currency catalog-wide on **Upgrade Catalog** for the first slice.
- Keep **CoinPickupMultiplier** scoped to coin **Currency Grants** from **Pickups** only.
- Keep MonoBehaviours shallow. Put progression, purchasing, stat resolution, launch impulse calculation, and pickup multiplier math in plain C# services.
- Run Unity compile before implementation tests in each slice. Prefer EditMode tests for plain C# rules and PlayMode tests only for state, scene, UI, physics, or VContainer composition behavior.
- Do not publish these remotely unless explicitly requested.
