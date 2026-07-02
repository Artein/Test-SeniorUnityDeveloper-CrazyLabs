## Parent

[Launch Push Character Presentation PRD](../../prd/prd-launch-push-character-presentation.md)

Supersession note: [Slide-Only Character Presentation](../slide-only-character-presentation/index.md) supersedes wording in this slice that treats flat grounded movement as normal visible **Run**. **Run** remains reserved compatibility.

## What to build

Wire the concrete Ladybug Character Animator and Gameplay Scene presentation path so the new Character presentation modes can be observed in Play Mode. This slice should add authored PullAnticipation and LaunchPush Animator states or blend trees, add the required Animator parameters to the Ladybug controller, and connect the Character view fields to those parameters on the project-owned Ladybug Character prefab.

The runtime route should remain generic: PresentationMode selects Idle, PullAnticipation, LaunchPush, Slide, reserved Run compatibility, Airborne, Victory, and Defeat, while normalized pull and launch values tune animation intensity and lateral variation. Imported animation root motion must not move the gameplay Rigidbody, and the Ladybug visual must remain a child presentation object under the existing Character visual anchor.

This slice is wiring and composition work. It should not change launch physics, slingshot band behavior, collider fit, camera rules, save format, packages, or broad gameplay timing.

## Acceptance criteria

- [ ] The Ladybug Animator Controller contains PullAnticipation and LaunchPush states or blend trees selected through PresentationMode.
- [ ] PresentationMode remains the stable integer control parameter for Character mode state selection.
- [ ] Animator float parameters exist for normalized pull strength, normalized launch power, normalized pull offset, and normalized launch offset.
- [ ] PlaybackSpeedMultiplier remains available for locomotion playback scaling.
- [ ] PullAnticipation enters quickly from eligible modes so validated pull feedback feels responsive.
- [ ] LaunchPush enters immediately after accepted launch and does not route through normal locomotion.
- [ ] LaunchPush hands off through a short clean transition after the classifier stops selecting LaunchPush.
- [ ] Normal locomotion cannot appear during the launch-push guard window when the runtime mode is LaunchPush.
- [ ] Animator root motion stays disabled and imported animations do not move the authoritative Rigidbody.
- [ ] The Ladybug Character prefab has its view parameter fields wired to valid Animator parameters.
- [ ] Gameplay Scene composition resolves the same runtime path used by the prefab and container.
- [ ] No Launch Target collider, Band Center, camera anchor, band shape, launch physics, package, save-format, or Addressables behavior changes are introduced in this slice.

## Verification

- EditMode tests:
  - Animator parameter-name validation tests if an existing asset validation pattern is already available.
  - Existing presenter, classifier, and view tests from earlier slices remain green.
- PlayMode tests:
  - Composition test verifies the Ladybug Character prefab view references a valid Animator and the expected mode and float parameter names.
  - Gameplay Scene composition test verifies the Character visual is mounted under the visual anchor and the Launch Target remains physics-authoritative.
  - Runtime smoke test verifies PullAnticipation and LaunchPush modes can be selected through the real scene path without missing Animator parameter errors.
- Static checks:
  - Rider reformat and file-problem checks for changed C# files if any code changes are needed.
  - Unity compile through the connector before tests.
  - `git diff --check`.
  - Confirm prefab and controller serialization churn is limited to intended Animator and Character view wiring.
- Manual Unity smoke check:
  - In Play Mode, pull the slingshot and confirm PullAnticipation appears while the validated pull is held.
  - Release a valid launch and confirm LaunchPush appears before normal locomotion or Airborne can take over.
  - Confirm the previous early Slide and awkward locomotion handoff moment is no longer visible during the slingshot push interval.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 05 Carry Slingshot Values Through Character Frames
