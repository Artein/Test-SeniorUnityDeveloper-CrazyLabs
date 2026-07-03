# Scene Contract And Terminology Cleanup

Type: AFK

## Parent

`docs/prd/prd-run-body-natural-speed-ownership.md`

## What to build

Update the protected scene/config/test/docs contract so it matches the new speed ownership model.

The final contract should say: **Launch Impulse** creates fired energy, **Run Steering Control** changes direction without player-facing speed caps, **Run Surface Contact Slowdown** owns ordinary speed loss, **Launch Landing Stabilization** only removes first-landing lift, and **Character Presentation Mode** changes never alter **Run Body** velocity.

This slice should remove or retire obsolete launch burst/recovery assertions and terminology from tests, fakes, scene composition checks, and local context docs. It should keep naming cleanup scoped: broad code/asset renames are not required unless they are needed to remove a misleading active contract.

## Acceptance criteria

- [ ] Scene composition tests no longer protect obsolete launch burst grace, launch burst recovery, or launch burst speed multiplier as active movement behavior.
- [ ] Scene/config assertions protect the current ownership model, including any defensive sanity guard if serialized.
- [ ] Test fakes no longer expose obsolete speed-cap or launch-recovery fields unless required for compatibility with untouched interfaces.
- [ ] Local context docs use **Run Body** for post-launch physical movement and **Launch Target** only for slingshot-facing role.
- [ ] Documentation states that **Launch Flight** and **Airborne** are presentation-only and must not change velocity.
- [ ] Documentation states that ordinary slowdown belongs to **Run Surface Contact Slowdown**.
- [ ] No unrelated scene, prefab, art, UI, economy, or run-end behavior is changed.

## Verification

- EditMode tests:
  - All targeted tests changed by issues 1-4 remain green.
- PlayMode tests:
  - Gameplay scene composition tests cover the final authored contract.
  - Focused slingshot input smoke remains green.
- Static checks:
  - Unity connector compile before tests.
  - Rider reformat/problems for changed code/test files.
  - Markdown review for issue/PRD/context terminology consistency.
- Manual Unity smoke check:
  - Strong launch should no longer hit a speed wall when **Launch Flight** ends or **Airborne** begins.
- Package version/changelog:
  - Not required unless the project maintains a local gameplay changelog for this branch.

## Blocked by

- Post-Launch Steering Gate.
- Direction-Only Run Steering.
- Run Body Speed Sanity Guard.
- Launch Energy Regression Guards.
