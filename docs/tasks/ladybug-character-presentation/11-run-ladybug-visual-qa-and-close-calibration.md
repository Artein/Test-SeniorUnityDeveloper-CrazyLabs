## Parent

[Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

## What to build

Run the final Ladybug Character presentation QA pass and close first-slice calibration. This is a HITL slice because it depends on full Ladybug assets, in-editor visual review, and human judgment around animation feel, scale, material appearance, camera framing, and whether any minimal VFX/audio should ship with the first slice.

This slice should produce a concise QA result that either accepts the first slice as shippable or lists the specific follow-up issues needed before shipping.

## Acceptance criteria

- [ ] Full Ladybug asset payload is available, not only pointer-sized source files.
- [ ] Ladybug visual scale, local offset, local rotation, and Model Root setup are reviewed in Gameplay Scene.
- [ ] Idle, Slide, Run, Airborne, Victory, and Defeat presentation modes are reviewed in representative gameplay contexts.
- [ ] Slide is confirmed as the active downhill default.
- [ ] Run is confirmed as acceptable for flat grounded forward movement.
- [ ] Airborne debounce feels stable over small terrain gaps and responsive over sustained gaps.
- [ ] Playback speed reference values and clamps are calibrated enough for the first slice.
- [ ] Materials and shaders render acceptably in the active render pipeline.
- [ ] Camera framing remains acceptable through pull, launch, slide, flat run, airborne, victory, and defeat moments.
- [ ] Deferred items remain explicitly deferred unless a tiny VFX/audio addition is approved and wired through presentation-facing boundaries.
- [ ] QA result records any accepted compromises and any follow-up issues required before production polish.

## Verification

- EditMode tests:
  - Existing Character presentation, run result, slope/speed, and presenter tests remain green.
- PlayMode tests:
  - Gameplay Scene composition, Animator parameter, band/contact, camera, and run-end tests remain green after final calibration.
  - Any visual smoke/capture tests added for this pass are deterministic enough to keep, or removed if they are diagnostic-only.
- Static checks:
  - `git diff --check`.
  - Unity compile through the connector before tests.
  - AssetDatabase refresh/reimport if final prefab, material, controller, or scene data changed.
- Manual Unity smoke check:
  - Human QA pass covers pre-launch hold, pull, release, downhill slide, flat run, short contact gaps, sustained airborne, obstacle failure, finish victory, and restart/pre-launch return.
  - Human QA pass records whether first-slice VFX/audio are included or explicitly deferred.
- Package version/changelog:
  - No package manifest update expected.
  - Add local release note or task note only if the project has an established changelog/release-note surface for gameplay presentation changes.

## Blocked by

- 09 Wire Character Presentation Into GameplayScene
- 10 Fit Collider And Prove Band Contact Compatibility
