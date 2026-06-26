# Run Human Feel And Authoring QA Pass

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

## Type

HITL

## User stories covered

2-4, 7-12, 16-22, 58

## What to build

Run the human-in-the-loop tuning and authoring validation pass for the playable Slingshot. This should validate that the finger-sized Band Touch Target, Pull limits, speed curve, lift, lateral steering, Band visual, Pull Hint, Touch Indicator, and gizmos feel correct in the actual Gameplay Scene.

The output of this slice is accepted tuning values and any follow-up implementation issues discovered during real use. It should avoid broad new feature work such as haptics, audio, launch sequencing, or rope physics unless those are explicitly split into new tasks.

## Acceptance criteria

- [x] Designer/player can reliably start Pulls by touching near the visually thin Band.
- [x] Touch target radius feels finger-sized without causing surprising captures far from the Band.
- [x] Backward Pull distance produces readable power scaling from weak to strong launches.
- [x] Weak Pulls cancel naturally and do not accidentally launch.
- [x] Forward Pull movement does not create launch energy.
- [x] Lateral Pull Offset rotates launch direction predictably without changing launch power.
- [ ] Pull Hint communicates the interaction without obstructing gameplay.
- [x] Touch Indicator appears at the interpreted pull point and remains readable.
- [x] Band Shape is visually understandable in idle and Active Pull states.
- [ ] Slingshot gizmos make anchors, Launch Frame, Pull Plane, Pull limits, touch target, and lateral angle easy to inspect.
- [x] Accepted config values are saved in scene/config assets.
- [x] Any tuning or design concerns outside the first-slice scope are recorded as follow-up issues, not hidden in this issue.

## Verification

- EditMode tests: none expected unless tuning exposes a code defect that needs regression coverage.
- PlayMode tests: rerun the composition smoke test after tuning if it exists.
- Static checks: Unity compile via Unity AI Agent Connector if any assets/scripts changed; Rider checks only for script changes.
- Manual Unity smoke check: required; validate in Play Mode with editor mouse and, when available, on touch device.
- Package version/changelog: no package/changelog change.

## Blocked by

- 08 - Wire GameplayScene Playable Slingshot
