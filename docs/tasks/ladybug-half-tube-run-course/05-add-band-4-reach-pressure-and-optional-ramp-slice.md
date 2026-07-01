## Parent

[Ladybug Half-Tube Run Course PRD](../../prd/prd-ladybug-half-tube-run-course.md)

## What to build

Extend the **Run Course** through Band 4, 250-350 meters, with center/side transfer lines, a reach-pressure glide, and the optional side/bank risk ramp. This slice should create the late-upgrade pressure band where under-upgraded Runs can lose momentum naturally, while upgraded and skilled players can choose richer side-bank lines or bypass the optional ramp through a safe center fallback.

The completed slice should be the main proof that progression pressure comes from course length, slope, coins, and upgrades rather than forced deaths.

## Acceptance criteria

- [ ] The **Run Course** is continuous from Launch through 350m.
- [ ] The 250-280m section supports center and side transfer lines with safe center coins and richer side-transfer coins.
- [ ] Optional bank-side blocker pressure stays tied to reward lines and never blocks the safe center fallback.
- [ ] The 280-310m reach-pressure glide uses flatter pitch targets and sparse reachable coins.
- [ ] The reach-pressure glide can support **Lost Momentum** for under-upgraded Runs after final tuning without adding an unavoidable blocker.
- [ ] The 310-350m optional ramp is authored as **Run Surface** and positioned as a risk/reward line.
- [ ] A safe center fallback bypasses the optional ramp.
- [ ] Optional ramp coin rewards are visible before takeoff and do not distract from the safe bypass.
- [ ] No blockers are placed on the optional ramp landing or immediate recovery window.
- [ ] **Run Safety Net** coverage exists below Band 4, side lips, optional ramp fall paths, and optional ramp landing.
- [ ] The slice remains compatible with upgrade-driven reach assumptions for `SlingshotLaunchPower`, `PlayerMaxSpeed`, `PlayerSteeringResponsiveness`, and `CoinPickupMultiplier`.

## Verification

- EditMode tests:
  - Pure validation tests for optional-ramp fallback presence, section continuity, and safe/risk coin distribution, if represented in a validation module.
- PlayMode tests:
  - Optional ramp is verified as **Run Surface**.
  - Band 4 blockers are verified as **Run Obstacle** only where impact can fairly end the **Run**.
  - Representative samples cover transfer lines, reach-pressure glide, optional ramp takeoff, and landing.
  - **Run Safety Net** coverage is verified under Band 4 and optional ramp escape areas.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector after any code, scene, prefab, or test change.
- Manual Unity smoke check:
  - Confirm under-upgraded movement starts to feel reach-limited in the glide section without a forced obstacle.
  - Confirm the optional ramp reads as rewarding but skippable, with a clear center fallback.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 04 - Add Band 3 Post-Ramp Bank Pressure Slice
