## Parent

[Run Steering Frame Stability PRD](../../prd/prd-run-steering-frame-stability.md)

## What to build

Preserve the last stable **Run Steering Frame** briefly when raw support reports a short ungrounded gap.

This is steering-only grace. It should mask one or a few noisy support misses near **Side Banks** without changing true grounded facts for physics, progress, presentation, camera, or run-end systems. If the support miss lasts beyond the configured grace duration, the steering frame should stop pretending that old support is still current and future valid support should reinitialize or snap instead of slewing from stale state.

This slice is complete when brief contact misses no longer jolt steering, but real airborne or escape moments still behave like real ungrounded movement outside the steering-control layer.

## Acceptance criteria

- [ ] When active and previously stable, a brief ungrounded sample keeps returning the last stable steering up direction.
- [ ] Ungrounded grace duration uses the configured seconds value and fixed delta time.
- [ ] Grace expires after the configured duration.
- [ ] After grace expires, reads fall back safely instead of preserving stale support forever.
- [ ] Recontact after a real airborne gap reinitializes or snaps the steering frame rather than slewing from stale support.
- [ ] Raw **RunSurfaceContext.IsGrounded** remains unchanged for all systems.
- [ ] **Run Progress**, **Lost Momentum**, **Run End Flow**, **Character Presentation**, and camera behavior do not consume the grace-smoothed steering frame.
- [ ] Reset clears any active grace timer.

## Verification

- EditMode tests:
  - One missed support sample during active steering returns the last stable frame.
  - Consecutive missed support samples accumulate grace time using fixed delta time.
  - Grace expires after the configured duration.
  - After grace expiry, reads fall back to caller fallback when no valid stable frame should be trusted.
  - Recontact after grace expiry reinitializes or snaps rather than slewing from stale state.
  - Reset clears grace state.
- PlayMode tests:
  - Not expected for this slice unless runtime support-miss behavior cannot be reproduced deterministically in EditMode.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Targeted EditMode tests for steering-frame source.
  - Source search or review confirms no raw grounding consumers were redirected to the steering-frame source.
- Manual Unity smoke check:
  - Ride near a high **Side Bank** where small support misses used to produce twitching and confirm steering remains visually stable through tiny gaps.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Resettable Fixed-Tick Run Steering Frame Baseline
