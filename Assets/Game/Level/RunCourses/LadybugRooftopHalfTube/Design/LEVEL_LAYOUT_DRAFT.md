# Ladybug Half-Tube Run Course Draft

## Objective

Design one Ladybug-themed downhill **Run Course** that follows Sled Surfers-style mobile level design principles: continuous downhill sliding, readable obstacle dodging, ramps/air beats, coins feeding upgrades, and repeated attempts that improve reach.

Target tuning for the first implemented course:
- Successful upgraded run duration: about 60 seconds from **Launch** to **Run Finish**.
- Target first completion: after about 5-10 **Runs**, assuming the player spends collected coins on upgrades in **Run Preparation**.
- First run expectation: the player should not normally reach **Run Finish**.
- Estimated course length: about 420m forward progress, assuming roughly 7 m/s average speed for a competent upgraded run.
- Initial implementation path: one continuous straight-forward course along the **Run Progress Frame** forward axis; in the current `GameplayScene` that axis is world +Z.
- Main surface profile: banked half-tube with **Soft Containment**, not hard walls.

Reference boundary:
- Sled Surfers public listings describe downhill sledding/sliding, obstacle dodging, ramps/airtime, power-ups, coins, upgrades, and repeated attempts to go farther. Use these principles, not exact copied level layouts.
- Google Play reference: https://play.google.com/store/apps/details?hl=en_US&id=com.sled.surfers.game
- App Store reference: https://apps.apple.com/us/app/sled-surfers/id6744975195

Visual sketch:
- `ladybug-half-tube-layout-sketch.png` is the imagegen planning sketch for this brief. It illustrates the continuous 0-420m rooftop half-tube, five progression bands, 15 sections, safe/risk coin lines, rooftop blockers, required centered ramp, optional risk ramp, safety nets, and half-tube cross-section.

## First-Principles Constraints

A downhill sliding progression course needs these irreducible parts:
- A readable **Launch** and settle area so the player understands forward/downhill direction.
- A continuous **Run Surface** that creates forward movement and does not rely on teleports.
- Lateral steering space so the player can recover, choose coin lines, and dodge obstacles.
- Energy pressure: under-upgraded players should lose reach before the finish, mainly through **Lost Momentum** rather than forced blockers.
- Obstacles with visible safe gaps so failure feels caused by player line choice.
- Coins placed on safe and risky lines so early failures still fund upgrades.
- Ramps as **Run Surface** for airtime and excitement, with safe landings.
- A finish approach that tests control without adding unreadable last-second hazards.
- **Run Safety Net** coverage below the whole playable course, especially side lips and ramp landings.

## Resolved Design Rules

1. Build one continuous **Run Course**, not disconnected stages or branching routes.
2. Use local line choices only: center-safe, side-risk, banked-side coin lines, and optional ramp lines.
3. Shape the course as a half-tube: about 10m playable width, 6m comfortable center band, and 2m banked side surface on each side.
4. Use **Soft Containment** through banked sides; do not use hard guard rails as the primary boundary.
5. Let excessive lateral momentum send the **Launch Target** over the lip into eventual **Run Safety Net** rather than invisible walls.
6. Do not use hidden lateral clamps, invisible side walls, or forced recenter volumes to keep the **Launch Target** inside the half-tube.
7. A competent upgraded clear should be about 60 seconds.
8. Early runs should usually end before the finish, mainly through **Lost Momentum** caused by insufficient reach/upgrades.
9. Obstacles punish steering mistakes; they should not be mandatory fail gates.
10. Coins must be front-loaded enough that failed runs still support upgrade purchases.
11. Ramps are authored as **Run Surface**, never **Run Obstacle**.
12. Each high-level **Run Course Beat** should be built from shorter **Run Course Sections**; target roughly 12-15 sections across the full course.
13. Imported Ladybug assets should stay visual where possible; gameplay colliders and **Run Contact Category** authoring should live on project-owned wrappers later.
14. Start graybox pitch around 7 degrees average, with short flatter and steeper sections for pacing.
15. Use a wide recoverable half-tube cross-section: 6m center trough, 2m bank per side, 25-35 degree banks, and 1.0-1.4m lip height.
16. Validate the graybox with explicit gates before adding moving obstacles, extra ramps, or final art dressing.

## Sled Surfers Principles Applied To Ladybug Theme

Transferable principles:
- Make speed and reach the main progression fantasy.
- Use obstacles as readable steering checks, not hidden traps.
- Use ramps and airtime as emotional peaks.
- Place coins where they teach the line first, then tempt risk later.
- Let upgrades change how far and how cleanly the player can travel.
- Give the player meaningful partial success before first completion.

Ladybug adaptation:
- Replace snow slopes with a Ladybug-themed Paris rooftop gutter/chute: a broad half-tube rain gutter or roofline slide running across Paris rooftops.
- Replace sled cosmetics with the existing **Launch Target** presentation and later upgrade effects.
- Replace snowy obstacles with theme-consistent rooftop props: AC units, sunroofs, satellite dishes, solar panels, roof exits, water tanks, billboards, rotating gates, and rooftop ramps.
- Keep visual language red/black/green/Paris rather than winter/snow.

## Visual Setting

The resolved setting is a Paris rooftop gutter/chute. Treat the half-tube itself as project-owned **Run Surface** geometry, then dress the edges and skyline with existing Ladybug rooftop assets.

Useful asset vocabulary found in `Assets/Plugins/Ladybug`:
- Environment: `Rooftop_Chunk_01.FBX`, `Rooftop_Chunk_02.FBX`, `Rooftop_Chunk_03_Drop.FBX`, `Rooftop_Chunk_04_Drop.FBX`, `Rooftop_Chunk_05_Step.FBX`.
- Obstacles: `Obstacle_AC1.FBX`, `Obstacle_AC2.FBX`, `Obstacle_SunRoof.FBX`, `Obstacle_SunRoof_RunAlong.FBX`, `Obstacle_SunRoof_3L_Blocker.FBX`, `Obstacle_WaterTank.FBX`, `Obstacle_RoofExit.FBX`, `Obstacle_SatDish.FBX`, `Obstacle_Billboard.FBX`, `Obstacle_SolarPanels.FBX`, `Obstacle_Rotating_Gate_Roof.FBX`.
- Ramp: `Ramp.FBX`.

Do not make imported rooftop chunks the gameplay surface source of truth. The gutter/chute should be authored as clean sliding geometry first, then dressed with imported meshes as visuals.

## Downhill Pitch

Resolved first graybox pitch model:
- Average forward downhill pitch: about 7 degrees.
- Launch settle and reach-pressure glide sections: 3-5 degrees.
- Normal sliding sections: 6-8 degrees.
- Short acceleration beats before ramps or late-course excitement: 10-12 degrees.

Pitch rules:
- Treat section pitch values as tuning targets for the main safe line / center trough, not exact values across every bank sample.
- Validate **ForwardDownhillDegrees** on the center trough, left bank, and right bank so side riding does not feel flat/uphill or break presentation.
- Avoid sustained 10-12 degree slope until physics and upgrade tuning prove early runs still fail before the finish.
- Use flatter 3-5 degree spans to create reach pressure where under-upgraded players can enter **Lost Momentum**.
- Do not place a hard obstacle immediately after a steep acceleration beat, ramp takeoff, or ramp landing.
- Vary pitch by **Run Course Section**, not by invisible force zones, so the course remains readable and physically authored.

## Half-Tube Cross-Section

Resolved first graybox cross-section:
- Center trough width: 6m.
- Bank width: 2m per side.
- Total playable width: about 10m.
- Bank angle: about 25-35 degrees.
- Lip height: about 1.0-1.4m above the center trough.

Cross-section rules:
- Center trough should be wide enough for comfortable steering, obstacle reads, and recovery.
- Side banks should be climbable and recoverable at normal speed, not treated as hard walls.
- High lateral speed should make side riding risky enough that the **Launch Target** can crest the lip and eventually hit **Run Safety Net**.
- Side containment must be authored through **Run Surface** geometry, not hidden lateral clamps, invisible side walls, or forced recenter volumes.
- Coins can climb the banks to teach side riding, but the first bank coin lines should return the player to center before the next obstacle.
- Obstacles on banks should be used mostly after the first ramp/air beat, once the player understands side recovery.

## First Graybox Section Layout

Use this as the initial 15-section implementation map. The meter ranges are a starting point for authoring and will need physics validation.

| # | Range | Band | Pitch | Surface / Feature | Obstacles | Coins / Purpose |
| ---: | ---: | --- | --- | --- | --- | --- |
| 1 | 0-20m | Band 1 | 3-4 degrees | Smooth shallow center trough | None | Straight center coins; establish downhill direction |
| 2 | 20-45m | Band 1 | 5-6 degrees | Gentle S line across the center band | None | Safe S coin trail; teach lateral steering |
| 3 | 45-70m | Band 1 | 6 degrees | Low left/right bank touch, returning to center | Visual edge props only | First light bank coins; return player to center before pressure |
| 4 | 70-95m | Band 2 | 6-7 degrees | Clear approach to first choice | One center blocker with wide left/right gaps | Split safe coin trails around blocker |
| 5 | 95-120m | Band 2 | 5-6 degrees | Broad recovery trough | None | Center recovery coins plus a low-bank optional trail |
| 6 | 120-150m | Band 2 | 7-8 degrees | Slightly faster side choice | One offset blocker with one obvious safe side | Bonus coins near the tighter line; safe coins on the obvious line |
| 7 | 150-170m | Band 3 | 8-10 degrees | Required ramp setup and sightline | None | Center approach coins point directly at the ramp |
| 8 | 170-200m | Band 3 | 10-12 degree takeoff, 5-6 degree landing | Required centered tutorial ramp with broad landing | None before, on, or immediately after landing | Visible coin arc; reward airtime without precision demand |
| 9 | 200-225m | Band 3 | 6-7 degrees | Landing recovery and first faster read | Simple staggered low blockers with clear recovery windows | Safe center coins after landing; small reward after clean read |
| 10 | 225-250m | Band 3 | 6-8 degrees | Bank line introduction under pressure | Staggered blocker near one side, center fallback open | Richer bank-side coins; center line stays viable |
| 11 | 250-280m | Band 4 | 6-7 degrees | Parallel center and side transfer lines | Optional bank-side blocker only | Safe center coins; richer side transfer coins |
| 12 | 280-310m | Band 4 | 3-5 degrees | Reach-pressure glide | None | Sparse reachable coins; likely **Lost Momentum** area for under-upgraded runs |
| 13 | 310-350m | Band 4 | 8-10 degree setup, 5-6 degree landing | Optional side/bank risk ramp with center fallback | Familiar static blocker away from landing | Risk coin arc on ramp; safe center coins bypass ramp |
| 14 | 350-390m | Band 5 | 6-7 degrees | Finish approach with wide sightlines | One familiar blocker pattern at most | Coin line starts funneling toward center |
| 15 | 390-420m | Band 5 | 5-6 degrees | Final funnel and visible **Run Finish** | No new hazards | Clean finish coins; no distraction from **Run Finish** |

Section layout rules:
- Treat the table's pitch column as the main safe-line / center-trough target for each **Run Course Section**.
- Keep ranges continuous and non-overlapping in the first graybox.
- Do not add a new mechanic in the same section as a required ramp, optional ramp, or first obstacle of a band.
- Preserve at least one safe or near-safe coin line in every section where coins appear.
- Keep the section table as the source of truth until playtest data proves a range should move.
- If physics tuning changes total course length, scale section ranges proportionally before redesigning obstacle patterns.

## Graybox Validation Gates

Validate the first implementation against these gates before expanding the layout vocabulary.

| Gate | Pass Criteria | Failure Response |
| --- | --- | --- |
| Successful upgraded run duration | A competent upgraded run reaches **Run Finish** in about 55-65 seconds without relying on lip exploits or shortcuts | Adjust pitch, friction, or section length before adding content |
| First-run failure expectation | A decent no-upgrade or starter-upgrade run usually reaches Band 2, roughly 85-125m, and still collects useful coins before the required ramp | Tune reach pressure, safe coin availability, and early upgrade costs with the upgrade branch |
| Completion progression | Player can reasonably reach **Run Finish** after about 5-10 Runs with upgrades and improving control | Rebalance upgrade effects, coin income, or reach-pressure spans |
| Continuous safe line | Every section has at least one readable safe or near-safe traversal line | Move blockers, widen gaps, or reduce simultaneous demands |
| Obstacle readability | First obstacle is understood before impact; later blockers have visible gaps and recovery windows | Increase sightline distance, spacing, or recovery length |
| Ramp safety | Required ramp has clear takeoff, broad landing, visible coin arc, and no blocker before/on/immediately after landing | Reduce ramp demand or clear the landing section |
| Optional ramp fairness | Optional ramp rewards coins or expression but center fallback remains viable | Move reward coins, widen fallback, or reduce ramp penalty |
| Safety net coverage | **Run Safety Net** catches failures below the course, side lips, ramp landings, and finish approach | Expand coverage before visual polish |
| Coin distribution | Reachable coins remain roughly 65-75 percent safe/near-safe and 25-35 percent risk/reward | Reposition coins before tuning final currency values |
| Finish readability | Final 30m clearly communicates **Run Finish** and introduces no new hazard type | Remove late novelty and strengthen finish funnel |
| Theme fit | Gameplay surface stays a clean rooftop gutter/chute while Ladybug rooftop assets dress edges and obstacles | Separate collision wrappers from imported visual meshes |

Validation rules:
- Treat a failed gate as a layout problem first, not an art problem.
- Do not add moving/rotating obstacle behavior until all static gates pass.
- Do not add more ramps until the required and optional ramp gates pass.
- Do not finalize economy values from this brief; only validate placement intent and reachable coin ratios.

## Early-Run Progression Intent

Suggested reach targets before exact upgrade math exists:
- Run 1: reaches Band 2, roughly 85-125m if played decently; earns reliable coins and usually fails before the required ramp at 170m.
- Runs 2-3: reaches the first obstacle/ramp region more consistently, with the required ramp becoming reliable only after early upgrades.
- Runs 4-6: reaches mid-course and learns advanced bank/ramp lines.
- Runs 7-10: reaches the finish approach and can complete after enough upgrades and skill.

This should be tuned by distance reached, average speed, and coin income. The course should not depend on an unavoidable obstacle or scripted death to stop early runs.

Upgrade reach assumptions:
- Early reliability to the required ramp should be tuned primarily around `SlingshotLaunchPower` and `PlayerMaxSpeed` upgrades.
- `PlayerSteeringResponsiveness` should improve line control, recovery, and risk coin access, but should not be mandatory to reach the required ramp.
- `CoinPickupMultiplier` should speed later progression, but should not be required for first ramp reach.
- Safe Band 1 and Band 2 coins collected across failed runs must support at least one early reach-upgrade purchase path.

## Coin Placement Rules

- 65-75 percent of reachable coins should sit on readable safe or near-safe lines.
- 25-35 percent can sit on risk/reward lines: high bank trails, tighter obstacle gaps, ramp arcs, and late side transfers.
- The first 70m should always include enough coins to make a failed first run feel useful.
- After a difficult obstacle or ramp, place a short reward line to confirm success.
- Do not place high-value coins directly after blind crests, ramp landings, or side-lip exits.
- Do not require risky coins to afford basic upgrades.

## Coin Income Structure

Use safe baseline income plus optional risk/reward income.

| Band | Safe Baseline Income | Optional Risk/Reward Income |
| --- | --- | --- |
| Band 1 | Center and gentle side coins; enough to make first failure useful | Very light bank trail only after steering is introduced |
| Band 2 | Safe split trails around the first center blocker | Extra coins close to the obstacle gap or on the outer bank |
| Band 3 | Main line coins through ramp intro and recovery windows | Ramp arc bonus and tighter slalom lines |
| Band 4 | Center fallback coins through reach-pressure spans | Rich bank-side lines near optional blockers |
| Band 5 | Finish funnel coins that guide to **Run Finish** | Small late bonus line only if it does not distract from finishing |

Economy rules:
- Basic upgrades should be affordable from safe/near-safe coins collected across failed runs.
- Risk/reward coins should speed up progression, not gate it.
- Basic reach upgrades should be affordable without relying on risk coins or `CoinPickupMultiplier`.
- Band 1 coin income must be reliable enough that first-run failure still feels productive.
- Later bands may increase reward density, but should not add mandatory coin difficulty.
- Coin values can change later with the upgrade branch; this brief fixes placement intent, not final currency numbers.

## Obstacle Rules

- First obstacle appears only after the player has had time to settle and steer.
- Every obstacle cluster must leave at least one obvious safe line.
- Early obstacle pressure should be lateral, not vertical or timing-based.
- Late obstacles can combine bank angle, speed, and coin temptation, but still need readable gaps.
- Obstacles should usually cost momentum or end through **Obstacle Impact** only when the player clearly hits them at speed.
- Moving or rotating obstacles should wait until the static course validates.

## Obstacle Vocabulary By Progression Band

Use static-first obstacle patterns for the first graybox.

| Band | Range | Allowed Obstacle Patterns | Rooftop Asset Fit |
| --- | ---: | --- | --- |
| Band 1 | 0-70m | No blocking obstacles; visual props outside the playable line only | Edge dressing with rooftop chunks or distant props |
| Band 2 | 70-150m | Single center blocker with left/right safe gaps | `Obstacle_AC1.FBX`, `Obstacle_AC2.FBX`, simple sunroof blocker |
| Band 3 | 150-250m | Staggered slalom pairs, wide ramp-adjacent reads, recovery windows after each pair | AC plus solar panel, satellite dish, low sunroof pieces |
| Band 4 | 250-350m | Bank-side blockers tied to richer coin lines; center fallback remains readable | Water tank, billboard, solar panels, roof exit, sunroof blockers |
| Band 5 | 350-420m | Sparse finish pressure; no new obstacle mechanics | One familiar blocker pattern at most, then clear finish funnel |

Do not introduce moving/rotating obstacle behavior in the first course pass. `Obstacle_Rotating_Gate_Roof.FBX` can remain deferred or be used only as static dressing until static readability, timing, and first-completion pacing are proven.

## Ramp Rules

- Ramps must be **Run Surface** and must keep the player oriented back into the half-tube.
- First playable ramp should be centered, required, and forgiving.
- Later ramps can be optional side/risk lines, but the center fallback must remain safe.
- No obstacle should be placed immediately on a ramp landing.
- Coin arcs should make ramp intent visible before the takeoff.
- Keep the first graybox to one required ramp plus one optional ramp until physics, camera, and upgrade pacing validate.

## Ramp / Air-Beat Vocabulary

Use a small ramp vocabulary for the first graybox.

| Band | Ramp Intent | Placement Rule |
| --- | --- | --- |
| Band 1 | No ramp | Let the player learn downhill direction and lateral steering first |
| Band 2 | Preview only, or no ramp | Keep the first obstacle choice readable; ramp can appear as dressing outside the playable line |
| Band 3 | Required tutorial ramp | Centered takeoff, broad landing, visible coin arc, and no obstacle immediately before or after |
| Band 4 | Optional risk/reward ramp | Side or bank-adjacent ramp tied to richer coins, with a clear center fallback line |
| Band 5 | No new ramp mechanic | Use only a familiar low ramp if it guides into the finish funnel without distracting from **Run Finish** |

Ramp authoring rules:
- Ramps are part of the authored **Run Surface**, not **Run Obstacle**.
- A required ramp should teach airtime and landing, not demand precision.
- Optional ramps should reward confidence with coins or speed expression, not block basic completion.
- Do not combine a new ramp with a new obstacle pattern in the same **Run Course Section**.
- Do not place blockers immediately after takeoff, on the landing, or in the first recovery window after landing.

## Implementation Notes For Later

Later graybox should start with simple project-owned primitives:
- `LevelSurface_*` wrappers with **Run Surface** contacts.
- `LevelObstacle_*` wrappers with **Run Obstacle** contacts.
- `LevelSafetyNet` under the full course and outside the side lips.
- `LevelFinish` at the final 410-420m region.
- Collectible wrappers for coins once the currency branch is merged or its interface is known.

Current open tuning questions:
- Exact friction and upgrade-speed tuning.
- Exact early purchase curve and reach contribution for `PlayerMaxSpeed`, `SlingshotLaunchPower`, `PlayerSteeringResponsiveness`, and `CoinPickupMultiplier`.
- Exact coin economy values per reach band.

## Run Progression Bands

Use **Run Progression Bands** to connect distance reached, coin income, and upgrade pressure without treating each band as a separate stage.

| Band | Range | Expected Role | Tuning Intent |
| --- | ---: | --- | --- |
| Band 1 | 0-70m | First-run onboarding | Reliable coins, low obstacle pressure, teach center/side steering |
| Band 2 | 70-150m | Early upgrade reach | Simple obstacles, first clear choice, enough coins to justify a second upgrade |
| Band 3 | 150-250m | Mid-run skill ramp | Ramp intro, bank riding, alternating obstacles, higher coin rewards |
| Band 4 | 250-350m | Late upgrade pressure | Longer glide sections, richer side-bank lines, likely Lost Momentum area for under-upgraded players |
| Band 5 | 350-420m | Finish-capable run | Finish approach, fewer new mechanics, readable final coin funnel and **Run Finish** |

The bands are not separate levels, scenes, or hard checkpoints. They are tuning ranges for expected reach and economy balance.

## Implementation-Ready Scope

The first graybox brief is implementation-ready. Stop adding level-design scope before graybox work begins.

Implementation should now build and validate the frozen scope:
- One continuous **Run Progress Frame**-relative half-tube **Run Course**.
- 15 continuous **Run Course Sections** across 0-420m.
- Static-first obstacles only.
- One required centered tutorial ramp and one optional risk/reward ramp.
- Safe baseline coins plus optional risk/reward coins.
- **Run Safety Net** coverage below the course, side lips, ramp landings, and finish approach.
- A visible **Run Finish** in the final 410-420m region.

Treat friction, final coin values, and exact upgrade math as graybox/economy tuning inputs, not additional level-layout scope.
