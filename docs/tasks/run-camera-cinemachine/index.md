# Run Camera Cinemachine Implementation Issues

Parent PRD: [Run Camera Cinemachine PRD](../../prd/prd-run-camera-cinemachine.md)

Related ADR: [ADR-0009: Use Cinemachine For Run Camera](../../adr/adr-0009-use-cinemachine-for-run-camera.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for adding a launch-gated Cinemachine **Run Camera** that follows the **Launch Target** through a stable **Run Camera Anchor** while preserving **Pre-Launch Camera** behavior.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Convert Pre-Launch Camera To Cinemachine Brain](01-convert-pre-launch-camera-to-cinemachine-brain.md) | AFK | None | 2, 13, 23, 33-34, 50 |
| 02 | [Add Launch-Gated Run Camera Handoff](02-add-launch-gated-run-camera-handoff.md) | AFK | 01 | 1, 3-5, 11, 14-15, 24-29, 32-39, 42-48 |
| 03 | [Stabilize Velocity-Facing Run Camera Anchor](03-stabilize-velocity-facing-run-camera-anchor.md) | AFK | 02 | 8-10, 19-21, 30-32, 36-38, 46 |
| 04 | [Add Camera Geometry Layers And Decollider Safety](04-add-camera-geometry-layers-and-decollider-safety.md) | AFK | 02 | 6-7, 12, 16-18, 22, 27, 40, 49, 55-60 |
| 05 | [Playtest And Tune Run Camera Feel](05-playtest-and-tune-run-camera-feel.md) | HITL | 03, 04 | 8, 11-12, 16-18, 36, 49, 53-54, 59-60 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep **Run Camera** terminology aligned with `CONTEXT.md`.
- Keep **Slingshot** input and launch math independent from camera handoff work.
- Keep Cinemachine settings on Cinemachine scene components; keep project-owned anchor and lifecycle tuning in `RunCameraConfig`.
- Use explicit camera geometry layers such as `CameraTerrain` and `CameraObstacle`; do not treat every collider as a camera obstacle.
- Run Unity compile before targeted tests on implementation slices.
- Prefer EditMode tests for controller, config, and anchor behavior. Use PlayMode tests for scene composition, Cinemachine components, layer authoring, and engine-level camera safety smoke checks.
