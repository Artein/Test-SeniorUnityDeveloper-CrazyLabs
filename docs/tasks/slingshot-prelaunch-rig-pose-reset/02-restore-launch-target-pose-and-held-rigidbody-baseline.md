# Restore Launch Target Pose And Held Rigidbody Baseline

## Parent

[Slingshot Pre-Launch Rig Pose Reset PRD](../../prd/prd-slingshot-prelaunch-rig-pose-reset.md)

## Type

AFK

## User stories covered

5, 16-19, 25-30, 33

## What to build

Add the **Launch Target** side of **Pre-Launch Rig Pose** reset.

Same-session restart should restore the authored **Launch Target** pose, including rotation, and establish a known held Rigidbody baseline. The reset
must not derive the next held baseline from stale Rigidbody state left by launch, collision, or run-end behavior. Ordinary held **Pull** positioning
after reset remains position-only and should preserve the target rotation established by reset.

Keep Unity Rigidbody manipulation behind a narrow adapter boundary so orchestration tests can use fakes and MonoBehaviours remain shallow.

## Acceptance criteria

- [ ] **Launch Target** reset restores authored position and rotation from **Pre-Launch Rig Pose**.
- [ ] Reset aligns the target so its authored **Band Center** can align with the **Slingshot** **Rest Point** after capture.
- [ ] Reset clears stale linear and angular velocity.
- [ ] Reset establishes the intended held kinematic and constraint baseline without preserving stale post-run values as the next baseline.
- [ ] Held **Pull** positioning remains position-only after reset.
- [ ] Existing launch behavior still restores the intended non-held state on launch.
- [ ] Invalid or missing pose/Rigidbody references fail loud through existing validation style.
- [ ] The adapter remains shallow and does not own **Gameplay State** or **Slingshot** lifecycle decisions.

## Verification

- EditMode tests:
  - Reset restores authored position and rotation.
  - Reset clears linear and angular motion through fake or engine-appropriate seams.
  - Reset establishes known held state instead of preserving stale constraints.
  - Held positioning after reset preserves rotation while moving position.
  - Existing launch and weak/canceled pull tests remain green.
- PlayMode tests:
  - Not required for this slice unless Rigidbody behavior cannot be asserted deterministically in EditMode.
- Static checks:
  - Rider reformat/problems on changed code files.
  - Unity compile through the Unity AI Agent Connector before tests.
  - `git diff --check`.
- Manual Unity smoke check:
  - Not required for this slice.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Pre-Launch Rig Pose Reset Boundary
