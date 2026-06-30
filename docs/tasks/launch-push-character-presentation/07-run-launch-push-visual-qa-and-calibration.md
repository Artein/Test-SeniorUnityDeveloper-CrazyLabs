## Parent

[Launch Push Character Presentation PRD](../../prd/prd-launch-push-character-presentation.md)

## What to build

Run human visual QA and final calibration for Pull Anticipation and Launch Push in the real Gameplay Scene. This slice accepts that animation feel, transition timing, lateral pose readability, pull-strength response, and the absence of awkward leg poses require in-editor judgement after the deterministic runtime path is already built.

Calibration should focus on authored Animator transitions, launch-push minimum duration, normalized pull and launch blend response, and verifying that slingshot-specific modes hand off cleanly into normal Slide, Run, or Airborne. Any runtime code changes discovered during QA should stay narrowly scoped to presentation behavior and should keep launch physics, band visuals, and collider geometry stable unless a separate issue is opened.

## Acceptance criteria

- [ ] Pull Anticipation appears only during validated active pull and clears on canceled, invalid, weak, or recaptured states.
- [ ] Pull Anticipation visibly responds to pull strength without looking jittery at small input changes.
- [ ] Pull Anticipation visibly responds to lateral pull direction where the authored animation supports it.
- [ ] Launch Push starts immediately after accepted launch.
- [ ] Launch Push preserves accepted launch strength and lateral intent through its normalized Animator inputs.
- [ ] Slide and Run do not appear during the intended launch-push guard window.
- [ ] Launch Push hands off cleanly into Slide, Run, or Airborne after the guard, depending on real surface and motion facts.
- [ ] The previously observed early sliding pose during slingshot push is no longer visible.
- [ ] The previously observed awkward Slide-to-Run leg-split transition is no longer visible during the slingshot push handoff.
- [ ] Inactive slingshot Animator channels return to zero and do not leak stale pull or launch values into locomotion or terminal modes.
- [ ] Victory and Defeat still override slingshot presentation.
- [ ] Animator root motion remains disabled and the Ladybug visual does not move Band Center, collider, Rigidbody, or camera anchor transforms.
- [ ] QA output records final accepted tuning values, screenshots or captures if useful, and any follow-up issues that are intentionally left out of this feature.

## Verification

- EditMode tests:
  - Run changed EditMode tests from the implementation slices if any tuning changes touch runtime code.
- PlayMode tests:
  - Run changed PlayMode tests from the implementation slices if any scene, prefab, Animator parameter, or container wiring changes are made.
- Static checks:
  - Rider reformat and file-problem checks for changed C# files if any code changes are made.
  - Unity compile through the connector before tests if any runtime, prefab, Animator, scene, or asmdef changes are made.
  - `git diff --check`.
  - Confirm final serialized asset changes are limited to accepted Animator, prefab, or tuning adjustments.
- Manual Unity smoke check:
  - Record at least one valid pull-and-launch from the default starting area and review Pull Anticipation, Launch Push, and locomotion handoff.
  - Repeat with weak pull, canceled pull, strong centered pull, left-biased pull, right-biased pull, downhill handoff, flat-forward handoff, and airborne handoff where level geometry allows it.
  - Confirm no console errors or missing Animator parameter warnings are emitted during the checked flows.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 06 Wire Ladybug Animator And GameplayScene Presentation Path
