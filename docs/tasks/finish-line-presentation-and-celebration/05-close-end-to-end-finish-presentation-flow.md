## Parent

[Finish Line Presentation And Celebration PRD](../../prd/prd-finish-line-presentation-and-celebration.md)

## What to build

Close the complete finish-line path so a successful run reaches the authoritative **Run Finish**, produces one accepted successful **Run Result**, starts **Finish Celebration**, fades the **Finish Threshold Visual**, and enters **Victory** with **Victory Facing** while preserving existing **Run End Flow** authority.

This slice is the integration hardening pass after the independently buildable hierarchy, threshold, celebration, and facing slices are in place. It should prove the feature works as one flow and that failure paths remain quiet.

## Acceptance criteria

- [ ] A successful finish path reaches **Run Ended** through the authoritative course-owned **Run Finish**.
- [ ] The accepted successful **Run Result** starts celebration and **Victory Facing** exactly once for the terminal run.
- [ ] Failed run-end paths do not start celebration or **Victory Facing**.
- [ ] **Run Preparation** reset restores threshold visibility, clears celebration state, and clears **Victory Facing**.
- [ ] **Run End Flow** remains the owner of accepted result authority; presentation code does not grant completion or rewards.
- [ ] The finish billboard, threshold, confetti anchors, and required views are discoverable under **Finish Presentation**.

## Verification

- EditMode tests:
  - Any missing presenter/controller edge cases found during integration are covered with focused tests rather than broad scene-only assertions.
- PlayMode tests:
  - End-to-end successful finish scene test proves **Run Ended** is reached and finish presentation reacts to accepted success.
  - End-to-end failed run scene test proves finish presentation does not react to failed accepted results.
  - Scene tests prove required finish presentation references are serialized and composition resolves without duplicate finish triggers.
  - Scene tests prove **Run Preparation** reset returns finish presentation and character presentation to ready state.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector before tests.
  - Targeted Unity tests through the existing connector after compile is clean.
- Manual Unity smoke check:
  - Complete one successful run and one failed run in the gameplay scene and confirm the visible behavior matches the PRD.
- Package version/changelog:
  - No package manifest, package version, or changelog update expected.

## Blocked by

- 03 - Play Finish Celebration From Accepted Success
- 04 - Blend Victory Facing Toward Run Camera
