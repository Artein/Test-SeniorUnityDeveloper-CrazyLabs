## Parent

[Slide-Only Character Presentation PRD](../../prd/prd-slide-only-character-presentation.md)

## What to build

Run the human-facing Unity QA and calibration pass for slide-only **Character Presentation**.

The deterministic implementation slices should already prove that normal grounded locomotion becomes **Slide**, that **Run** is reserved compatibility only, and that scene/prefab authoring uses the new tuning language. This slice validates the player-facing result in the real Gameplay Scene: downhill sliding, flat coasting, sideways or backward drift, true stalls, launch/pull handoff, airborne handoff, and terminal presentation.

This slice is HITL because the acceptance question is visual consistency and feel. Automated tests can prove wiring, but they cannot fully decide whether flat coasting, slow sliding, threshold timing, and clip playback look reasonable.

## Acceptance criteria

- [ ] In the Gameplay Scene, normal downhill movement reads as **Slide**.
- [ ] Flat coasting reads as **Slide** or calmer Slide playback, not visible **Run**.
- [ ] Sideways grounded drift does not visibly switch to **Run**.
- [ ] Brief backward grounded movement does not visibly switch to **Run**.
- [ ] True stopped or stalled grounded behavior returns to **Idle** at a reasonable threshold.
- [ ] Weak launch outcomes stop looking active quickly enough to feel resolved.
- [ ] Short ground-probe gaps do not create visible flicker.
- [ ] Real airborne moments still become **Airborne**.
- [ ] **Pull Anticipation** remains readable during aiming.
- [ ] **Launch Push** remains readable immediately after release.
- [ ] **Victory** and **Defeat** remain stronger than locomotion after accepted **Run Result**.
- [ ] Playback speed looks reasonable across slow coasting, normal sliding, and fast sliding.
- [ ] No visible **Run** animation appears in normal play.
- [ ] The **Character** remains visually attached to the **Launch Target**.
- [ ] Root motion, Character Visual Anchor, Band Center, Rigidbody, collider, camera, slingshot launch, and run progress behavior still look unchanged.
- [ ] Any remaining visual issue is recorded as either implementation follow-up or future **Slide Flavor** polish, not hidden in tuning.

## Verification

- EditMode tests:
  - Re-run changed Character Presentation EditMode tests if any calibration requires code or tuning changes.
- PlayMode tests:
  - Re-run changed scene composition and Character Presentation PlayMode tests if any scene, prefab, Animator, or serialized tuning changes are made.
- Static checks:
  - Unity compile through the connector before tests if code or asset changes are made.
  - `git diff --check` if any files change during calibration.
  - Review serialized YAML diffs if prefab, scene, or Animator tuning changes are made.
- Manual Unity smoke check:
  - Play representative launches in the current level.
  - Observe downhill sliding, flat coasting, sideways drift, backward bump recovery, true stall, short terrain gaps, real airborne moments, pull, launch push, victory, and defeat.
  - Confirm no normal gameplay path shows visible **Run**.
- Package version/changelog:
  - No package manifest or changelog update expected unless project release process requires a local note for visual tuning.

## Blocked by

- 03 - Update Ladybug Authoring And Scene Composition
- 04 - Clean Legacy Run Presentation Documentation
