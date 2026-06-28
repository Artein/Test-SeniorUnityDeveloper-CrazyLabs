## Parent

[Run End Flow First Slice PRD](../../prd/prd-run-end-flow-first-slice.md)

## What to build

Add the first vertical slice for **Run Progress Frame** support. The game should be able to snapshot a designer-authored downhill progress frame at
`LaunchApplied`, use the **Launch Target** position as the progress origin, expose current and maximum forward progress through
`RunProgressService`, and fail loud when progress-frame authoring is missing or invalid.

This slice should establish the runtime progress contract used later by **Run End Flow**, **Lost Momentum**, camera fallback, and future steering
constraints. It should also add the gameplay-facing motion source contract so run-ending logic does not depend on camera-named interfaces.

## Acceptance criteria

- [ ] A designer-authored `RunProgressFrameSource` can provide valid forward/right/up axes for a **Run Progress Frame**.
- [ ] `RunProgressFrameSnapshot` is immutable and captured at `LaunchApplied`.
- [ ] `RunProgressService` / `IRunProgressService` owns snapshot validity, current forward progress, and maximum forward progress.
- [ ] Progress origin is the **Launch Target** position at `LaunchApplied`, not the authored frame object's position.
- [ ] `DistanceTravelled`-ready max progress is based on downhill-forward projection, not path length, raw world displacement, camera direction, or slingshot aim direction.
- [ ] Invalid or missing progress-frame authoring fails loud and does not silently fall back to world forward or launch direction.
- [ ] A gameplay-facing `IRunMotionSource` exposes position and linear velocity for run-end services.
- [ ] Gameplay composition can register the progress-frame source, progress service, and run-motion source through VContainer.

## Verification

- EditMode tests:
  - `RunProgressService` snapshots normalized valid axes at `LaunchApplied`.
  - Invalid progress-frame data is rejected loudly.
  - Progress origin is the **Launch Target** position at `LaunchApplied`.
  - Max forward progress increases with downhill motion.
  - Backward bounce does not reduce max progress.
  - Sideways movement and vertical jump height do not inflate max progress.
  - `SlingshotLaunchRequest.LaunchDirection` lateral aim does not define progress.
- PlayMode tests:
  - Rigidbody-backed run-motion source exposes current position and linear velocity.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Assign a temporary progress frame and confirm logs/errors are clear when it is missing or invalid.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
