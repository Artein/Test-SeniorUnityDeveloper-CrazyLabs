# Add Launch-Gated Run Camera Handoff

## Parent

[Run Camera Cinemachine PRD](../../prd/prd-run-camera-cinemachine.md) and [ADR-0009](../../adr/adr-0009-use-cinemachine-for-run-camera.md).

## Type

AFK

## User stories covered

1, 3-5, 11, 14-15, 24-29, 32-39, 42-48

## What to build

Add the first complete **Run Camera** handoff path. When the **Launch Target** receives launch velocity and the **Gameplay State** is Running, project-owned lifecycle code should make the **Run Camera** live and drive a **Run Camera Anchor** from the **Launch Target**. Leaving Running should deactivate the **Run Camera** and reset the launch-gated lifecycle.

The slice should include narrow source, anchor, config, and camera rig boundaries so controller tests can use fakes while scene code remains shallow. The **Run Camera** only needs enough anchor motion to follow the **Launch Target** in this slice; detailed velocity-facing yaw and smoothing are completed in the next slice.

## Acceptance criteria

- [ ] A **Run Camera** exists in the scene and is inactive or lower priority during **Pre-Launch**.
- [ ] A **Run Camera Anchor** exists and is the Tracking Target used by the **Run Camera**.
- [ ] **Run Camera** activation requires both `LaunchApplied` and the Running **Gameplay State**.
- [ ] Running without `LaunchApplied` does not activate the **Run Camera**.
- [ ] `LaunchApplied` outside Running does not activate the **Run Camera** until Running is reached.
- [ ] Leaving Running deactivates the **Run Camera** and clears launch-gated state.
- [ ] Repeated launch/state events are idempotent.
- [ ] `RunCameraConfig` owns only project-controlled anchor/lifecycle values and does not duplicate Cinemachine component settings.
- [ ] Gameplay composition validates required config, source, anchor, and rig references.
- [ ] Existing steering and **Slingshot** behavior remain independent of camera lifecycle.

## Verification

- EditMode tests: lifecycle gating, idempotence, deactivation/reset, config validation, and fake rig/source/anchor contracts.
- PlayMode tests: after a valid launch the active camera changes from **Pre-Launch Camera** to **Run Camera**; scene-resolved camera references are assigned exactly once.
- Static checks: Unity compile; `git diff --check`.
- Manual Unity smoke check: launch from the **Slingshot** and verify the camera handoff occurs only after motion begins.
- Package version/changelog: no additional package dependency beyond the first slice; no project changelog entry required unless the repo already requires one.

## Blocked by

- [Convert Pre-Launch Camera To Cinemachine Brain](01-convert-pre-launch-camera-to-cinemachine-brain.md)
