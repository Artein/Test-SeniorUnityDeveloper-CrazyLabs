## Parent

[Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

## What to build

Add the passive Unity-facing Character Presentation View contract and implementation surface. The view should own Animator references, SaintsField-backed Animator parameter selection, root-motion protection, and prefab-owned tuning values. It should apply Character Presentation Frames without interpreting gameplay states, launch events, run results, or run-end reasons.

This slice should make the view independently testable with a minimal Animator setup before any Ladybug prefab or gameplay presenter is wired.

## Acceptance criteria

- [ ] A view-facing Character Presentation Frame contains selected Character Presentation Mode and playback speed multiplier only.
- [ ] The view interface exposes frame application without launch-specific or run-ended-specific methods.
- [ ] The Unity view writes `PresentationMode` as an Animator integer parameter.
- [ ] The Unity view writes `PlaybackSpeedMultiplier` as an Animator float parameter.
- [ ] Serialized Animator parameter fields use SaintsField Animator parameter support when the package is available.
- [ ] The Unity view disables or corrects Animator root motion according to the PRD contract.
- [ ] Prefab-owned presentation tuning is exposed through a read-only interface for presenter/classifier consumers.
- [ ] The view does not reference Gameplay State IDs, Run Result, Run End Reason, slingshot launch requests, contact categories, or raw slope facts.
- [ ] No Ladybug prefab, Animator Controller asset, scene wiring, or presenter lifecycle logic is introduced in this slice.

## Verification

- EditMode tests:
  - Pure frame/tuning value tests where practical.
  - View contract tests verify no gameplay-specific command methods are required by consumers.
- PlayMode tests:
  - Applying a frame writes the expected Animator integer and float parameters on a minimal test Animator setup.
  - Root motion is disabled or corrected when the view initializes or applies frames.
- Static checks:
  - `git diff --check`.
  - Unity compile through the connector before tests.
  - Static/code review check that the view assembly references presentation contracts, not run-ending or slingshot vocabulary.
- Manual Unity smoke check:
  - Inspect the view component in the Inspector and confirm Animator parameter fields are selectable/readable.
- Package version/changelog:
  - No package manifest update expected if SaintsField is already installed.
  - If SaintsField is missing, stop before adding it because package changes require explicit approval.

## Blocked by

None - can start immediately
