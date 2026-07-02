## Parent

[Slide-Only Character Presentation PRD](../../prd/prd-slide-only-character-presentation.md)

## What to build

Make **Run** compatibility-only at the presenter/view boundary and make **Slide Reference Speed** the normal playback reference for grounded locomotion.

After issue 01, normal classifier paths should no longer emit **Run**. This slice handles the remaining compatibility edges: existing mode memory, test doubles, legacy Animator-facing frames, and any unexpected **Run** frame should not produce visible running in normal play. The chosen boundary should be explicit and tested. A practical first implementation is to normalize unexpected **Run** to **Slide** before applying the frame, and to use **Slide Reference Speed** for any compatibility playback fallback.

This slice should not delete the **Run** enum value or reorder Animator mode integers.

## Acceptance criteria

- [ ] **Run** remains in the **Character Presentation Mode** enum with stable integer value and ordering.
- [ ] Normal runtime frame application cannot leak visible **Run** when classification or compatibility input unexpectedly provides **Run**.
- [ ] Unexpected **Run** is normalized to **Slide** at a single explicit boundary.
- [ ] Mode elapsed memory does not preserve **Run** as a locomotion hold.
- [ ] Short ungrounded debounce with previous **Run** normalizes to **Slide** or eventually **Airborne**, never visible **Run**.
- [ ] **Slide Reference Speed** is used for normal grounded locomotion playback.
- [ ] **Run Reference Speed** is removed from normal playback behavior or treated only as a compatibility fallback to **Slide Reference Speed**.
- [ ] Playback speed remains clamped by the existing minimum and maximum playback multipliers.
- [ ] **Pull Anticipation**, **Launch Push**, **Idle**, **Airborne**, **Victory**, and **Defeat** playback behavior remains unchanged.
- [ ] No Animator Controller state deletion, enum reordering, imported clip deletion, scene hierarchy change, physics change, package change, or save-format change is introduced in this slice.

## Verification

- EditMode tests:
  - Presenter or view boundary maps unexpected **Run** frame to **Slide**.
  - Compatibility **Run** playback uses **Slide Reference Speed** fallback.
  - **Slide** playback still uses **Course Planar Speed** divided by **Slide Reference Speed**.
  - Playback speed still clamps to configured minimum and maximum multipliers.
  - Non-locomotion modes still use playback speed `1` unless their existing behavior says otherwise.
  - Mode memory cannot hold **Run** through minimum locomotion duration.
  - Short ungrounded preservation with previous **Run** does not emit visible **Run**.
- PlayMode tests:
  - None expected unless the chosen boundary requires Animator-backed verification.
- Static checks:
  - Rider reformat and file-problem checks for changed C# files.
  - Unity compile through the connector before tests.
  - `git diff --check`.
- Manual Unity smoke check:
  - None expected for this compatibility/playback slice.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Replace Grounded Mode Selection With Meaningful Slide
