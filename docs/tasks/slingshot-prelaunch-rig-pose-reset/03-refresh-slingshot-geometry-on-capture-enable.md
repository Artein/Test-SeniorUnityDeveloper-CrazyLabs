# Refresh Slingshot Geometry On Capture Enable

## Parent

[Slingshot Pre-Launch Rig Pose Reset PRD](../../prd/prd-slingshot-prelaunch-rig-pose-reset.md)

## Type

AFK

## User stories covered

1-4, 12-13, 24, 29, 34

## What to build

Update **Slingshot** capture so enabling capture refreshes its geometry from current view transforms before positioning the held **Launch Target** and
showing idle **Band Shape**.

The initial geometry snapshot can still validate setup and allocate internal buffers, but it must not be the only source of geometry for the lifetime
of the controller. This lets a same-session **Pre-Launch Rig Pose** reset move the **Slingshot** rig and have capture use the current **Rest Point**,
anchors, and **Launch Frame**.

## Acceptance criteria

- [ ] `ISlingshotCapture.EnableCapture()` refreshes **Slingshot** geometry from current view transforms before aligning the held target.
- [ ] Capture idle **Band Shape** is built from the refreshed **Rest Point**, anchors, and **Launch Frame**.
- [ ] Initialization still validates required geometry and setup failures fail loud.
- [ ] Existing capture enable/disable idempotency remains intact.
- [ ] Existing active **Pull**, weak release, canceled release, and launch request behavior remain unchanged.
- [ ] The **Slingshot** controller remains state-agnostic.

## Verification

- EditMode tests:
  - Capture enable refreshes geometry before held target rest alignment.
  - Capture idle uses the refreshed rest geometry after authored geometry changes.
  - Existing **Slingshot** controller tests for capture, pull, weak release, valid release, and launch handoff remain green.
- PlayMode tests:
  - Covered later by the same-session Gameplay Scene regression.
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
