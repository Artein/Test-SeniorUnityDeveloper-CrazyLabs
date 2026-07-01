## Parent

[Ladybug Half-Tube Run Course PRD](../../prd/prd-ladybug-half-tube-run-course.md)

## What to build

Author the first 0-70 meters of the Ladybug rooftop half-tube **Run Course** as a complete playable onboarding slice. This slice should replace the prototype start surface with a project-owned **Run Surface** that establishes downhill direction, center-trough steering, first light bank touches, early safe coins, and **Run Safety Net** coverage underneath and outside the side lips.

The completed slice should be playable from **Launch** through Band 1 without blocking obstacles. It should prove the half-tube cross-section, **Soft Containment**, **Run Progress Frame** orientation, representative **ForwardDownhillDegrees**, basic camera framing, and scene composition before later slices add pressure.

## Acceptance criteria

- [ ] The first 0-70m of the **Run Course** is one continuous **Run Progress Frame**-relative half-tube aligned with the current course-forward direction.
- [ ] The Band 1 surface includes sections for 0-20m shallow settle, 20-45m gentle center S steering, and 45-70m low bank touch returning to center.
- [ ] The authored surface uses project-owned geometry/colliders as the gameplay source of truth.
- [ ] The center trough is approximately 6m wide with recoverable side banks contributing to about 10m total playable width.
- [ ] Side banks create visible **Soft Containment** without hidden lateral clamps, invisible side walls, or forced recenter volumes.
- [ ] Excessive side escape has a clear path toward **Run Safety Net** instead of invisible correction.
- [ ] Band 1 contains no blocking **Run Obstacles**.
- [ ] Visual-only edge props, if present, do not participate in **Run Contact Category** classification.
- [ ] Early safe **Pickups** establish the center line and first gentle side movement without requiring risk.
- [ ] **Run Safety Net** coverage exists below the Band 1 trough, side lips, and likely escape areas.
- [ ] Representative center, left-bank, and right-bank samples produce useful downhill **Run Surface Context** values for the intended pitch ranges.
- [ ] The Gameplay Scene still loads, resolves gameplay composition, and keeps the **Launch Target**, **Run Camera**, **Run Surface Context Source**, and **Run Progress Frame Source** functional.

## Verification

- EditMode tests:
  - Pure validation tests for any new course-section or authoring validation module, if one is introduced in this slice.
  - No EditMode tests expected for raw scene mesh placement alone.
- PlayMode tests:
  - Gameplay Scene composition verifies the Band 1 **Run Surface** is classified as **Run Surface**.
  - Representative slope samples cover center trough, left bank, and right bank in Band 1.
  - **Run Safety Net** presence is verified under Band 1 and outside side-lip escape areas.
  - Existing scene DI, **Run Progress Frame**, **Run Surface Context Source**, and camera composition tests remain green.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector after any code, scene, prefab, or test change.
- Manual Unity smoke check:
  - Launch into Band 1 and confirm the player can settle, steer gently, ride a low bank, recover to center, collect early safe coins, and fall into **Run Safety Net** if escaping the side lip.
  - Confirm camera framing keeps the trough and early coin line readable.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately.
