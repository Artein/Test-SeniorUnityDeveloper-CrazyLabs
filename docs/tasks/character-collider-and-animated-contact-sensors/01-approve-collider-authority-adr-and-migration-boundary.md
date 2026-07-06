## Parent

[Character Collider Split And Animated Contact Sensors PRD](../../prd/prd-character-collider-and-animated-contact-sensors.md)

## What to build

Create and approve the collider-authority implementation decision before code or scene migration starts. The decision should lock the split between **Launch Target Collider Root**, **Run Body Contact Collider**, pickup-focused **Animated Contact Sensors**, and future body-part obstacle notices.

This is a HITL gate because the migration changes physics-body ownership, scene authoring, layer policy, trigger direction, and timing expectations. If the team decides not to create a formal ADR, record that waiver in the issue outcome before unblocking implementation slices.

## Acceptance criteria

- [ ] Decision record confirms **Launch Target Collider Root** remains the slingshot launch target and band-contact surface.
- [ ] Decision record confirms **Run Body Contact Collider** owns movement support, **Run Finish**, **Run Safety Net**, and default **Obstacle Impact**.
- [ ] Decision record confirms first-pass **Run Body Contact Collider** is a single `SphereCollider` with no animation-driven size or shape switching.
- [ ] Decision record confirms **Animated Contact Sensors** are copied-pose trigger sensors under gameplay-owned physics hierarchy, not gameplay authority for movement or run end.
- [ ] Decision record confirms first-pass animated sensors are pickup-only and body-part obstacle notices require a separate future **Run Obstacle Sensor Source**.
- [ ] Decision record confirms `PlayerBodyPart` to **Pickup Layer** interaction is the first-pass external interaction, with no camera-obstacle interaction here.
- [ ] Decision record confirms the timing policy: pose copy after **Character Visual Follower**, then next-physics-step trigger delivery, with no default per-frame `Physics.SyncTransforms`.
- [ ] Decision record captures the first-pass **Animated Sensor Identity** policy and the TODO to replace hierarchy path or GameObject name with asset-backed IDs before identity becomes a stable content contract.
- [ ] User/team approval is recorded, or a written waiver explicitly allows implementation to continue from the PRD alone.

## Verification

- EditMode tests:
  - Not required for this HITL documentation gate.
- PlayMode tests:
  - Not required for this HITL documentation gate.
- Static checks:
  - `git diff --check`.
  - ADR or decision-note links resolve if a new document is created.
- Manual Unity smoke check:
  - Not required; no Unity assets or scene data should change in this issue.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
