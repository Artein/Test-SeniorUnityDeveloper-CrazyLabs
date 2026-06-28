# Stabilize Velocity-Facing Run Camera Anchor

## Parent

[Run Camera Cinemachine PRD](../../prd/prd-run-camera-cinemachine.md) and [ADR-0009](../../adr/adr-0009-use-cinemachine-for-run-camera.md).

## Type

AFK

## User stories covered

8-10, 19-21, 30-32, 36-38, 46

## What to build

Make the **Run Camera Anchor** stable enough for downhill sliding, jumps, and steering. The anchor should derive position from the **Launch Target**, apply configured height/look offsets, face planar velocity, and ignore raw physics body rotation. When planar velocity is below the minimum yaw speed, the anchor should keep the last valid yaw instead of spinning or snapping.

This slice completes the project-owned camera behavior that Cinemachine follows. Cinemachine still owns final camera composition and damping; the project owns only anchor pose generation and lifecycle timing.

## Acceptance criteria

- [ ] Anchor position follows **Launch Target** position plus configured project-owned offsets.
- [ ] Anchor yaw follows planar **Launch Target** velocity.
- [ ] Anchor yaw ignores **Launch Target** body rotation, visual rotation, pitch/roll, and slope normal.
- [ ] Low planar velocity preserves the last valid yaw.
- [ ] Invalid, zero, or degenerate velocity input does not produce invalid rotations.
- [ ] Position and yaw smoothing respond to configured rates and delta time.
- [ ] The anchor update phase is chosen deliberately and verified against physics-driven motion jitter.
- [ ] Invalid `RunCameraConfig` values clamp or fail visibly according to the project config pattern.
- [ ] Cinemachine Third Person Follow offsets, Decollider radius, Decollider damping, and Brain blend settings remain out of `RunCameraConfig`.

## Verification

- EditMode tests: offset following, planar velocity yaw, low-speed fallback, invalid velocity handling, smoothing, config validation, and independence from body rotation.
- PlayMode tests: smoke coverage that the **Run Camera Anchor** tracks current **Launch Target** motion after launch without obvious frame-lag jitter.
- Static checks: Unity compile; `git diff --check`.
- Manual Unity smoke check: slide, steer, and jump while verifying the camera looks along travel direction without inheriting physics wobble.
- Package version/changelog: no package or changelog change expected.

## Blocked by

- [Add Launch-Gated Run Camera Handoff](02-add-launch-gated-run-camera-handoff.md)
