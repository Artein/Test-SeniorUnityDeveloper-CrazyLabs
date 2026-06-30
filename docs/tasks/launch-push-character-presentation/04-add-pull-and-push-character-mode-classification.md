## Parent

[Launch Push Character Presentation PRD](../../prd/prd-launch-push-character-presentation.md)

## What to build

Add Pull Anticipation and Launch Push to the pure Character presentation mode classification model. This slice should expand the Character presentation mode enum, classification input, and tuning so slingshot-specific modes have explicit precedence before normal locomotion can take over.

The classifier should keep terminal modes highest priority, then select Pull Anticipation while a validated active pull exists, then select Launch Push while a launch-push fact is active and its elapsed time is below the configured minimum guard. Existing pre-launch, Slide, Run, Airborne, Victory, Defeat, slope, speed, hysteresis, and airborne behavior should continue to apply after slingshot-specific modes no longer own presentation.

This slice should be pure C# and should not require Animator, prefab, scene, slingshot context source, or Unity object changes.

## Acceptance criteria

- [ ] Character presentation modes include PullAnticipation and LaunchPush with stable integer values suitable for Animator mode selection.
- [ ] Classification input includes active-pull presence, launch-push presence, and launch-push elapsed seconds.
- [ ] Character presentation tuning includes a launch-push minimum duration guard.
- [ ] Victory and Defeat outrank PullAnticipation and LaunchPush.
- [ ] PullAnticipation outranks LaunchPush and all pre-launch or locomotion decisions.
- [ ] LaunchPush outranks Idle, Slide, Run, and Airborne while launch push is active and elapsed time is below the minimum guard.
- [ ] Normal pre-launch and locomotion classification resumes once launch-push elapsed time reaches the minimum guard.
- [ ] Normalized pull, normalized launch power, normalized pull offset, and normalized launch offset do not decide mode in this slice.
- [ ] Existing Slide, Run, Airborne, terminal, hysteresis, and minimum locomotion mode duration behavior remains covered and unchanged outside the new precedence rules.
- [ ] No Animator Controller, Character view, presenter dependency, slingshot event subscription, scene, prefab, package, save-format, physics, or band behavior change is introduced in this slice.

## Verification

- EditMode tests:
  - PullAnticipation is selected over downhill Slide facts.
  - PullAnticipation is selected over flat forward Run facts.
  - LaunchPush is selected over downhill Slide before the minimum guard elapses.
  - LaunchPush is selected over flat forward Run before the minimum guard elapses.
  - LaunchPush hands off to normal locomotion when elapsed time reaches the minimum guard.
  - Victory and Defeat outrank PullAnticipation and LaunchPush.
  - PullAnticipation outranks LaunchPush if both facts are present.
  - Existing classifier tests for Slide, Run, Airborne, hysteresis, and terminal behavior remain green.
- PlayMode tests:
  - None expected; this slice should be pure EditMode-testable.
- Static checks:
  - Rider reformat and file-problem checks for changed C# files.
  - Unity compile through the connector before tests.
  - `git diff --check`.
- Manual Unity smoke check:
  - None expected beyond reviewing default tuning values for readability.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
