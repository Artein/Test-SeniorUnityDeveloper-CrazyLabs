## Parent

[Ladybug Half-Tube Run Course PRD](../../prd/prd-ladybug-half-tube-run-course.md)

## What to build

Extend the **Run Course** through the required centered tutorial ramp, covering the 150-200m ramp setup, takeoff, airtime cue, and broad landing. This slice should make the first ramp a forgiving core mechanic introduction: centered approach coins, clear sightline, ramp authored as **Run Surface**, visible coin arc, no blocker before/on/immediately after landing, and **Run Safety Net** coverage below the takeoff and landing area.

The completed slice should be playable and testable as the first major airtime beat. It should not add a new obstacle pattern in the same demand window.

## Acceptance criteria

- [ ] The **Run Course** is continuous from Launch through at least 200m.
- [ ] The 150-170m ramp setup section points the player toward the centered required ramp with readable approach coins.
- [ ] The 170-200m tutorial ramp is authored as **Run Surface**, not **Run Obstacle**.
- [ ] The ramp takeoff uses a short steeper pitch target and the landing returns to a gentler pitch target.
- [ ] The ramp has a broad, readable landing that returns the player toward the half-tube.
- [ ] No blocking obstacle exists immediately before the ramp, on the ramp, on the landing, or in the first recovery window after landing.
- [ ] A visible coin arc communicates the intended ramp line before takeoff.
- [ ] Ramp collision is simple enough to launch and land predictably.
- [ ] **Run Safety Net** coverage exists below the ramp takeoff, airborne gap/fall area, landing, and side exits.
- [ ] The **Run Camera** frames the approach, takeoff, airtime, and landing clearly enough for manual validation.
- [ ] Under-upgraded first Runs should still usually fail before this required ramp after final tuning; this slice must not force first-run completion.

## Verification

- EditMode tests:
  - Pure validation tests for ramp classification or section continuity, if a validation module exists.
- PlayMode tests:
  - Required ramp collider/contact authoring is verified as **Run Surface**.
  - No required-ramp object introduces unsupported **Run Contact Categories** such as **Ramp** or **Boundary**.
  - Representative ramp setup, takeoff, and landing slope samples match intended directional expectations.
  - **Run Safety Net** coverage is verified under ramp-related fall areas.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector after any code, scene, prefab, or test change.
- Manual Unity smoke check:
  - Play the ramp setup repeatedly and confirm the approach coins, takeoff, airtime, landing, and camera framing are readable.
  - Confirm there is no immediate blocker or unfair landing punishment.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 - Add Band 2 Early Obstacle And Reach Slice
