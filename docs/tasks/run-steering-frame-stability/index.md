# Run Steering Frame Stability Implementation Issues

Parent PRD: [Run Steering Frame Stability PRD](../../prd/prd-run-steering-frame-stability.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for stabilizing the **Run Steering Frame** on U-shaped **Run Surface** traversal while preserving the Rigidbody-backed **Launch Target**, raw **RunSurfaceContext**, and **Soft Containment** through authored **Run Surface** shape.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Resettable Fixed-Tick Run Steering Frame Baseline](01-resettable-fixed-tick-run-steering-frame-baseline.md) | AFK | None | 20-26, 31-37, 45-51, 56-57, 66-69, 72, 74-75 |
| 02 | [Slew Ordinary Grounded Run Surface Normal Changes](02-slew-ordinary-grounded-run-surface-normal-changes.md) | AFK | 01 | 1, 3-4, 12-16, 21, 31, 38-40, 54, 58-60, 70-72, 75 |
| 03 | [Preserve Steering Frame Through Short Support Misses](03-preserve-steering-frame-through-short-support-misses.md) | AFK | 01 | 5-6, 23, 27-28, 40-42, 52-53, 61-62 |
| 04 | [Confirm Or Reject Large Steering Normal Discontinuities](04-confirm-or-reject-large-steering-normal-discontinuities.md) | AFK | 02 | 7-8, 16, 22, 24, 31, 38-44, 60, 63-65, 71, 75 |
| 05 | [Run Side-Bank Feel Review And Tuning](05-run-side-bank-feel-review-and-tuning.md) | HITL | 02, 03, 04 | 1-19, 25, 29-30, 52-55, 69-73 |

## Notes

- Keep **PhysicsRunSurfaceContextSource** raw; do not smooth **RunSurfaceContext** for progress, presentation, run-end, camera, or physics systems.
- Keep stability inside **RunSurfaceSteeringFrameSource** and expose only narrow read/reset seams.
- Keep the stability tuning in the existing player steering config asset.
- Do not add hidden walls, forced recentering, level geometry changes, Rigidbody architecture changes, or new packages.
- Run Unity compile before implementation tests in each AFK slice. Prefer EditMode tests for state-machine behavior; use PlayMode only when scene/runtime behavior cannot be expressed deterministically.
- The HITL slice is a feel and tuning review after deterministic behavior slices land.
- Do not publish these remotely unless explicitly requested.
