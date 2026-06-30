# Ladybug Half-Tube Run Course Implementation Issues

Parent PRD: [Ladybug Half-Tube Run Course PRD](../../prd/prd-ladybug-half-tube-run-course.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
- [ADR-0009: Use Cinemachine For Run Camera](../../adr/adr-0009-use-cinemachine-for-run-camera.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for building one continuous Ladybug rooftop half-tube **Run Course** with project-owned **Run Surface** geometry, static-first **Run Obstacles**, safe/risk **Pickups**, **Run Safety Net** coverage, and a visible **Run Finish**. The first implementation should validate the 420m graybox and repeated-Run progression loop before moving obstacle gameplay or final art polish.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Author Band 1 Half-Tube Onboarding Slice](01-author-band-1-half-tube-onboarding-slice.md) | AFK | None | 1-10, 41-43, 48-52, 56, 66-70, 73-79, 87-92 |
| 02 | [Add Band 2 Early Obstacle And Reach Slice](02-add-band-2-early-obstacle-and-reach-slice.md) | AFK | 01 | 11-16, 25-29, 44, 56-59, 73-79, 87-97 |
| 03 | [Add Required Center Tutorial Ramp Slice](03-add-required-center-tutorial-ramp-slice.md) | AFK | 02 | 17-20, 27-29, 45, 54-55, 68-69, 75, 89, 92, 95 |
| 04 | [Add Band 3 Post-Ramp Bank Pressure Slice](04-add-band-3-post-ramp-bank-pressure-slice.md) | AFK | 03 | 13-16, 21, 37, 45, 56-57, 64, 70, 96 |
| 05 | [Add Band 4 Reach Pressure And Optional Ramp Slice](05-add-band-4-reach-pressure-and-optional-ramp-slice.md) | AFK | 04 | 22-24, 27-30, 46, 52-58, 76-80, 93-97, 101 |
| 06 | [Add Band 5 Finish Approach And Run Finish Slice](06-add-band-5-finish-approach-and-run-finish-slice.md) | AFK | 05 | 31-35, 47, 72-76, 87-89, 94 |
| 07 | [Wire Course-Wide Pickup Distribution And Level Pickup State](07-wire-course-wide-pickup-distribution-and-level-pickup-state.md) | AFK | 01-06 | 4, 14-15, 21, 25-30, 35, 56-59, 81-82, 90, 97, 101 |
| 08 | [Add Rooftop Visual Dressing And Camera Readability Pass](08-add-rooftop-visual-dressing-and-camera-readability-pass.md) | HITL | 06 | 36-40, 60-72, 98 |
| 09 | [Run Progression Tuning And Graybox Acceptance Pass](09-run-progression-tuning-and-graybox-acceptance-pass.md) | HITL | 01-08 | 24-32, 41-65, 87-102 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep **Run Course**, **Run Course Section**, **Run Progression Band**, **Run Surface**, **Soft Containment**, **Run Safety Net**, and **Run Finish** terminology aligned with the project glossary.
- Keep imported Ladybug rooftop assets visual-first. Project-owned geometry and wrappers own gameplay colliders and **Run Contact Category** authoring.
- Keep ramps authored as **Run Surface**. Do not add a **Ramp** or **Boundary** **Run Contact Category**.
- Keep first-slice obstacles static. Moving or rotating obstacle gameplay is deferred until the full static graybox validates.
- Keep under-upgraded failure primarily tied to **Lost Momentum** and reach pressure, not forced blockers.
- Run Unity compile before implementation tests on code/scene slices.
- Use PlayMode tests for Gameplay Scene composition, contact categories, pickup wiring, safety-net coverage, ramp classification, and representative slope samples.
- Use manual Unity smoke checks for human-readable obstacle spacing, camera readability, first-run reach, upgraded completion timing, coin distribution, and visual theme fit.
