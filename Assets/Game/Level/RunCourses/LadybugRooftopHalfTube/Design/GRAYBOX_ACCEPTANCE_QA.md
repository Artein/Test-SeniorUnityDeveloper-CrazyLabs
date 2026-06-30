# Ladybug Half-Tube Graybox Acceptance QA

## Scope

Issue 09 validates the authored Ladybug rooftop half-tube as one complete Run Course: useful first failures, upgrade-driven reach growth, readable hazards, safe/risk coin economy, ramp comfort, finish readability, and a roughly one-minute competent upgraded completion.

## Automated Findings

- Course target metadata is expected on the `Ladybug Rooftop Half-Tube Run Course` root through `LadybugHalfTubeRunCourseAcceptanceProfile`.
- Target course length is 420m with Run Finish at 416m.
- Competent upgraded completion target is 55-65 seconds, implying about 6.5-7.6m/s average forward progress without side-lip shortcuts.
- First useful failure target is Band 2 around 85-125m, before the required ramp at 170m.
- First completion target is runs 5-10 through Run Preparation upgrades and improved control.
- Frozen graybox scope remains: no moving obstacles, no route branching, no hidden containment, no new ramps beyond the required ramp and optional bank ramp already in the layout.
- Existing PlayMode coverage verifies section ranges, slopes, obstacle gaps, required ramp approach/landing, optional ramp fallback, safety net coverage, finish contact, pickup wiring, safe/risk pickup split, visual-only dressing, and camera decollider layers.
- The safe pre-ramp pickup line before the required ramp is expected to fund an early reach-upgrade path across a small number of failed runs without requiring risk/reward pickup lines.
- Continuous PlayMode traversal sampling verifies a buffered player capsule has one supported, obstacle-clear safe route from launch settle through the required ramp, optional-ramp center bypass, final funnel, and Run Finish.
- Scene-level visual prefab fields are wired to Ladybug rooftop FBX assets under `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops`, while PlayMode coverage verifies those instantiated visuals remain render-only children of project-owned gameplay surfaces and blockers.

## Source Asset Wiring Evidence

Static scene YAML plus asset `.meta` GUID checks verify the authored visual fields use the intended Ladybug Rooftops assets:

| Scene visual field | Source asset |
| --- | --- |
| `_rooftopChunk01VisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Environment/Rooftop_Chunk_01.FBX` |
| `_rooftopChunk02VisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Environment/Rooftop_Chunk_02.FBX` |
| `_rooftopChunk03DropVisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Environment/Rooftop_Chunk_03_Drop.FBX` |
| `_rooftopChunk05StepVisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Environment/Rooftop_Chunk_05_Step.FBX` |
| `_obstacleAc1VisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Rooftop_obstacles/Obstacle_AC1.FBX` |
| `_obstacleAc2VisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Rooftop_obstacles/Obstacle_AC2.FBX` |
| `_obstacleSunroofVisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Rooftop_obstacles/Obstacle_SunRoof.FBX` |
| `_obstacleSolarPanelsVisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Rooftop_obstacles/Obstacle_SolarPanels.FBX` |
| `_obstacleBillboardVisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Rooftop_obstacles/Obstacle_Billboard.FBX` |
| `_obstacleWaterTankVisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Rooftop_obstacles/Obstacle_WaterTank.FBX` |
| `_obstacleRoofExitVisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Rooftop_obstacles/Obstacle_RoofExit.FBX` |
| `_obstacleSatDishVisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Rooftop_obstacles/Obstacle_SatDish.FBX` |
| `_rampVisualPrefab` | `Assets/Plugins/Ladybug/Content/Geometry/Levels/Rooftops/Rooftop_obstacles/Ramp.FBX` |

## Manual HITL Smoke Checklist

| Check | Status | Finding |
| --- | --- | --- |
| No-upgrade reach | Pending HITL | Play a decent non-upgraded run; expected useful Band 2 progress around 85-125m and failure before the required ramp. |
| Early-upgrade reach | Pending HITL | Buy early reach/control upgrades from Run Preparation; runs 2-3 should reach the first obstacle/ramp region more consistently. |
| Mid-progression reach | Pending HITL | Runs 4-6 should reach mid-course and start teaching bank/ramp risk lines. |
| Upgraded completion timing | Pending HITL | Competent upgraded run should finish in about 55-65 seconds without side-lip exploits or shortcuts. |
| Under-upgraded failure reason | Pending HITL | Failures should read primarily as Lost Momentum/reach pressure, not mandatory blocker deaths. |
| Obstacle readability | Pending HITL | Every section should expose at least one readable safe or near-safe traversal line. |
| Required ramp safety | Pending HITL | Approach, coin arc, landing, and post-landing recovery should feel clear and forgiving. |
| Optional ramp reward | Pending HITL | Optional ramp should reward confidence while center fallback remains viable. |
| Coin distribution | Pending HITL | Safe/near-safe pickups should feel like the main economy path, with risk/reward pickups optional. |
| Finish readability | Pending HITL | Final 30m should clearly communicate Run Finish and introduce no new hazard type. |
| Camera/theme fit | Pending HITL | Run Camera should keep the tube, obstacles, coins, finish, and Ladybug rooftop theme readable. |

## Acceptance Note

Automated checks can protect the authored structure, progression targets, visual source wiring, and frozen scope. Final Issue 09 acceptance still requires the manual HITL rows above to be executed in the Unity editor and updated with pass/fail findings.
