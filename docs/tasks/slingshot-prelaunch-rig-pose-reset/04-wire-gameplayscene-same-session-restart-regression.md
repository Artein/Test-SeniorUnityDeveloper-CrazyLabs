# Wire GameplayScene Same-Session Restart Regression

## Parent

[Slingshot Pre-Launch Rig Pose Reset PRD](../../prd/prd-slingshot-prelaunch-rig-pose-reset.md)

## Type

AFK

## User stories covered

1-10, 26, 31-34, 38

## What to build

Wire the Gameplay Scene and tests so the full same-session restart path is covered without reloading the scene.

The scene should provide explicit authored **Pre-Launch Rig Pose** references for the **Slingshot** rig and **Launch Target**. The regression should
start from one loaded Gameplay Scene session, disturb or launch the **Launch Target**, return to **Pre-Launch**, and prove the visible setup has been
restored: **Band Center** aligns with **Rest Point**, target rotation and held Rigidbody state are restored, and idle **Band Shape** comes from the
aligned current geometry.

## Acceptance criteria

- [ ] Gameplay Scene has explicit assigned **Pre-Launch Rig Pose** references for the **Slingshot** rig and **Launch Target**.
- [ ] Gameplay composition registers the reset boundary, pose provider, and target reset adapter through VContainer.
- [ ] Missing required pose references fail loud through scene/composition validation.
- [ ] A PlayMode regression runs a same-session restart without reloading the Gameplay Scene.
- [ ] After restart, **Launch Target** **Band Center** aligns with **Slingshot** **Rest Point** within tolerance.
- [ ] After restart, **Launch Target** rotation matches the authored **Pre-Launch Rig Pose**.
- [ ] After restart, the target Rigidbody is held with clean motion state.
- [ ] After restart, idle **Band Shape** uses the aligned refreshed rest geometry.
- [ ] Existing first-load Gameplay Scene composition tests remain green.

## Verification

- EditMode tests:
  - Lifetime-scope or composition validation catches missing reset/pose dependencies where practical.
  - Relevant orchestration and adapter tests from issues 01-03 remain green.
- PlayMode tests:
  - Same-session Gameplay Scene restart regression.
  - Existing Gameplay Scene first-load composition and **Slingshot** scene tests remain green.
- Static checks:
  - Rider reformat/problems on changed code files.
  - Unity compile through the Unity AI Agent Connector before tests.
  - `git diff --check`.
- Manual Unity smoke check:
  - Launch, let the **Run** end, and verify return to centered **Pre-Launch** without scene reload.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Pre-Launch Rig Pose Reset Boundary
- 02 - Restore Launch Target Pose And Held Rigidbody Baseline
- 03 - Refresh Slingshot Geometry On Capture Enable
