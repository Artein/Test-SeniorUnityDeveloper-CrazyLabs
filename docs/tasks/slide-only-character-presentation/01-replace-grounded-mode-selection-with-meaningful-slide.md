## Parent

[Slide-Only Character Presentation PRD](../../prd/prd-slide-only-character-presentation.md)

## What to build

Replace normal grounded **Character Presentation Mode** selection so **Meaningful Grounded Movement** produces **Slide** instead of splitting between **Slide** and **Run**.

This slice should make grounded locomotion depend on **Course Planar Speed** and the presentation movement threshold, not **Course Forward Speed** or downhill slope thresholds. **Idle** remains the grounded stopped or stalled mode. Existing higher-priority presentation states still win: accepted **Run Result** terminal modes, **Pull Anticipation**, **Launch Push**, inactive/pre-launch **Idle**, and confirmed **Airborne**.

This slice should be focused on classifier behavior and tests. It may keep old tuning member names temporarily if that keeps the slice pure; the clean serialized authoring rename is covered by a later issue.

## Acceptance criteria

- [ ] Grounded **Course Planar Speed** at or above the presentation movement threshold selects **Slide**.
- [ ] Grounded **Course Planar Speed** below the presentation movement threshold selects **Idle**.
- [ ] Flat, uphill, mild downhill, steep downhill, banked, sideways, and backward grounded movement all select **Slide** when planar movement is meaningful.
- [ ] **Course Forward Speed** no longer decides whether grounded movement is **Slide**.
- [ ] Downhill slope thresholds no longer decide canonical **Character Presentation Mode** selection.
- [ ] Existing terminal **Victory** and **Defeat** precedence is unchanged.
- [ ] Existing **Pull Anticipation** and **Launch Push** precedence is unchanged.
- [ ] Inactive and pre-launch states remain **Idle** unless pull presentation owns the mode.
- [ ] Confirmed ungrounded state still selects **Airborne**.
- [ ] Short ungrounded gaps preserve **Slide** and do not introduce **Run**.
- [ ] Existing minimum locomotion duration behavior cannot preserve **Run** as normal grounded locomotion.
- [ ] Normal classifier paths do not return **Run**.
- [ ] No Rigidbody, collider, slingshot launch, run progress, run end, scene, prefab, Animator Controller, package, save-format, or Addressables behavior changes are introduced in this slice.

## Verification

- EditMode tests:
  - Grounded flat forward movement above threshold selects **Slide**.
  - Grounded downhill movement above threshold selects **Slide**.
  - Grounded uphill movement above threshold selects **Slide**.
  - Grounded sideways movement above threshold selects **Slide**.
  - Grounded backward movement above threshold selects **Slide**.
  - Grounded movement below threshold selects **Idle**.
  - Zero planar speed selects **Idle**.
  - Changing only slope while speed and grounded state remain stable does not produce **Run**.
  - **Victory** and **Defeat** override grounded **Slide**.
  - **Pull Anticipation** and **Launch Push** override grounded **Slide**.
  - Short ungrounded gaps preserve **Slide**.
  - Long ungrounded gaps select **Airborne**.
  - Parameterized normal-path guard proves classifier does not emit **Run**.
- PlayMode tests:
  - None expected; this slice should be pure EditMode-testable.
- Static checks:
  - Rider reformat and file-problem checks for changed C# files.
  - Unity compile through the connector before tests.
  - `git diff --check`.
- Manual Unity smoke check:
  - None expected for this pure classifier slice.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
