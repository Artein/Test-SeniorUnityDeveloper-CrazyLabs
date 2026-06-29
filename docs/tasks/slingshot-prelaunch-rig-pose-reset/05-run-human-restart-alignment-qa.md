# Run Human Restart Alignment QA

## Parent

[Slingshot Pre-Launch Rig Pose Reset PRD](../../prd/prd-slingshot-prelaunch-rig-pose-reset.md)

## Type

HITL

## User stories covered

1-5, 31-34

## What to build

Run a human-in-the-loop QA pass for same-session restart alignment once automated coverage is green.

The pass should verify that repeated **Run** restarts visually return the **Launch Target** to the center of the **Band**, that the **Band** idle shape
does not start offset, and that normal **Pull** interaction still feels unchanged after several restarts.

## Acceptance criteria

- [ ] First Gameplay Scene load starts with **Band Center** aligned to **Rest Point**.
- [ ] One same-session restart returns to centered **Pre-Launch** without scene reload.
- [ ] Several repeated restarts do not accumulate visible offset, rotation drift, or stale motion.
- [ ] Shallow valid launch returns cleanly to centered **Pre-Launch** after the **Run** ends.
- [ ] A collision-heavy or awkward rotation **Run** returns cleanly to authored target rotation.
- [ ] Pulling after restart still moves the held **Launch Target** position-only and preserves the reset rotation.
- [ ] Idle **Band Shape** after restart reads as centered from the player camera.
- [ ] No launch power, launch direction, steering, **Pull Offset**, or **Band Release Recoil** behavior regression is observed.
- [ ] Any discovered tuning or authoring issue is recorded separately from the reset implementation.

## Verification

- EditMode tests:
  - All targeted EditMode tests from issues 01-03 remain green before manual QA.
- PlayMode tests:
  - Same-session Gameplay Scene restart regression remains green before manual QA.
- Static checks:
  - Unity compile through the Unity AI Agent Connector before manual QA.
  - `git diff --check`.
- Manual Unity smoke check:
  - Start Gameplay Scene, inspect first-load centered state.
  - Launch and let **Run End Flow** return to **Pre-Launch** once.
  - Repeat several restarts in the same play session.
  - Try shallow launch, deeper launch, lateral launch, and collision-heavy launch.
  - Verify **Band Center**, **Rest Point**, target rotation, and idle **Band Shape** read correctly from Game view.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 04 - Wire GameplayScene Same-Session Restart Regression
