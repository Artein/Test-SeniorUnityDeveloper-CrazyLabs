# Add Pre-Launch Rig Pose Reset Boundary

## Parent

[Slingshot Pre-Launch Rig Pose Reset PRD](../../prd/prd-slingshot-prelaunch-rig-pose-reset.md)

## Type

AFK

## User stories covered

10-15, 20-23, 27-28, 35-38

## What to build

Introduce the gameplay-level reset boundary that applies **Pre-Launch Rig Pose** before **Slingshot** capture can be enabled.

The reset path should run when **Gameplay State** is changing into **Pre-Launch**, before completed-state observers react. It should also run during
initialization when the scene starts already in **Pre-Launch**. By the time any `GameplayStateChanged` listener observes **Pre-Launch**, the rig reset
has already been requested.

Keep this boundary outside **Slingshot**. The **Slingshot** should remain state-agnostic and should only consume the aligned transforms when capture
is enabled.

## Acceptance criteria

- [ ] A gameplay-level reset coordinator or equivalent orchestration boundary exists for **Pre-Launch Rig Pose** reset.
- [ ] Reset is invoked on the `GameplayStateChanging` path when the next state is **Pre-Launch**.
- [ ] Reset is invoked during initialization before capture if the current state is already **Pre-Launch**.
- [ ] `GameplayStateChanged` **Pre-Launch** listeners observe reset having already been requested.
- [ ] **Slingshot** capture is enabled only after the reset boundary has run for **Pre-Launch**.
- [ ] **Run End Flow** remains responsible only for ending a **Run** and returning state toward **Pre-Launch**.
- [ ] The **Slingshot** controller does not gain a **Gameplay State** dependency.
- [ ] The reset boundary is registered through explicit VContainer composition.

## Verification

- EditMode tests:
  - Transition into **Pre-Launch** invokes reset before capture enable.
  - Initialization while already in **Pre-Launch** invokes reset before capture enable.
  - A fake `GameplayStateChanged` listener observes reset already requested when it receives **Pre-Launch**.
  - Leaving **Pre-Launch** still disables capture.
- PlayMode tests:
  - Not required for this slice unless implementation touches scene wiring.
- Static checks:
  - Rider reformat/problems on changed code files.
  - Unity compile through the Unity AI Agent Connector before tests.
  - `git diff --check`.
- Manual Unity smoke check:
  - Not required for this slice.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
