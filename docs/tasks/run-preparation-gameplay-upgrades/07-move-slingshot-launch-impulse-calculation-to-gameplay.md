## Parent

[Run Preparation Gameplay Upgrades PRD](../../prd/prd-run-preparation-gameplay-upgrades.md)

## What to build

Move final Slingshot-derived launch impulse calculation into higher Gameplay while keeping Slingshot as a feature leaf. Slingshot should report pull data such as Pull Strength and Pull Offset. Gameplay should combine that pull data with `GameplaySlingshotLaunchConfig` and the resolved `SlingshotLaunchPower` stat to produce an immutable `LaunchImpulse`, then apply that impulse through a narrow physics mutation boundary.

This slice should preserve the existing playable launch path while correcting dependency direction and making launch power upgradeable through the stat resolver.

## Acceptance criteria

- [ ] Slingshot no longer owns final upgraded launch speed, push impulse, or upgrade-aware launch math.
- [ ] Slingshot emits pull data needed by Gameplay, including Pull Strength and Pull Offset.
- [ ] `GameplaySlingshotLaunchConfig` is an authored ScriptableObject referenced by gameplay composition.
- [ ] Launch config covers min/max forward impulse, normalized pull-strength curve, max lateral launch angle, normalized lateral-angle curve, upward impulse, optional total impulse clamps, and validation.
- [ ] A pure launch impulse calculator returns immutable `LaunchImpulse` values from pull data, launch config, and resolved `SlingshotLaunchPower`.
- [ ] Launch impulse application is the only boundary that mutates Unity physics for the launch target.
- [ ] Existing launch flow transitions from Pre-Launch to Running and applies the gameplay-calculated impulse.
- [ ] Slingshot feature assembly remains independent of gameplay state, economy, upgrades, and stat resolver types.
- [ ] Existing Slingshot pull/band behavior remains covered at the feature boundary.

## Verification

- EditMode tests:
  - Launch impulse calculator handles min pull, max pull, lateral pull, upward impulse, launch power multiplier, and optional clamps.
  - Launch impulse calculator is deterministic and has no Unity physics side effects.
  - Slingshot pull request/data creation reports Pull Strength and Pull Offset without upgrade dependencies.
  - Gameplay launch path uses the stat resolver output for `SlingshotLaunchPower`.
- PlayMode tests:
  - Slingshot launch in the gameplay scene applies a non-zero gameplay-calculated impulse.
  - Increasing `SlingshotLaunchPower` changes final impulse while Slingshot pull reporting stays unchanged.
  - Pre-Launch to Running transition still occurs on launch.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
  - Assembly dependency check or source search confirms Slingshot does not reference upgrades, economy, gameplay state orchestration, or stat resolver types.
- Manual Unity smoke check:
  - Pull and release in the gameplay scene; confirm launch direction/power still feels plausible with authored defaults.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 04 - Add Run Modifier Snapshot And Gameplay Stat Resolver
- 05 - Add Run Preparation State And Continue Handoff
