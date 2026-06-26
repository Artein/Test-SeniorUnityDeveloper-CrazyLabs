# Add Post-Shot Band Release Recoil

## Parent

[Slingshot Target-Coupled Band PRD](../../prd/prd-slingshot-target-coupled-band.md)

## Type

AFK

## User stories covered

6-8, 12

## What to build

Add the post-shot visual phase where the Band follows the moving Launch Target contact/wrap points while returning from the loaded Band Shape to the rest/idle/default shape. Valid Pull Release should keep the loaded Band through the synchronous launch handoff. After the launch is applied, Band Release Recoil begins. During recoil, the Band should keep querying the moving target's contact/wrap data until the visual interpolation reaches rest, then detach and stop following the Launch Target.

This slice should treat recoil duration/curve as intended visual tuning, not as a safety timeout. There is no independent max-duration guard in this slice.

## Acceptance criteria

- [x] Valid Pull Release keeps the loaded Band Shape through launch request notification and accepted launch handoff.
- [x] Band Release Recoil starts only after launch application.
- [x] Recoil begins from the final loaded Band Shape.
- [x] During recoil, Band Contact Points and Band Wrap are still computed from the moving Launch Target Collider.
- [x] Recoil visually returns toward the rest/idle/default Band Shape using designer-tuned timing or curve data.
- [x] Recoil completion is defined by reaching the rest/idle/default Band Shape.
- [x] Once rest is reached, the Band detaches from the Launch Target and stops querying target contact/wrap.
- [x] No separate max-duration safety guard is added.
- [x] Weak, canceled, and invalid Pulls still reset directly to idle/rest instead of entering post-shot recoil.
- [x] Slingshot capture remains disabled after Launch according to existing Gameplay State gating.

## Verification

- EditMode tests: valid release does not reset Band before launch notification; accepted launch starts recoil after launch application; recoil requests moving contact/wrap data; recoil detaches at rest; weak/canceled/invalid Pulls do not start recoil.
- PlayMode tests: if deterministic, exercise a launch and verify the LineRenderer follows target contact/wrap during recoil and stops following after rest.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: launch from a loaded Pull and confirm the Band reads as pushing the Launch Target forward before returning to idle/rest.
- Package version/changelog: no package/changelog change.

## Blocked by

- 01 - Add Held Target Pull Positioning And Launch Handoff
- 04 - Add Arc-Based Band Wrap Visuals
