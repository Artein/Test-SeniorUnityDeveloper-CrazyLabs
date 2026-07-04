## Parent

[Run Steering Frame Stability PRD](../../prd/prd-run-steering-frame-stability.md)

## What to build

Filter implausible large steering-normal jumps without hiding confirmed real geometry transitions.

When the **Launch Target** is continuously grounded and a valid raw support normal jumps beyond the configured snap angle, treat that normal as suspect first. If it disappears before the configured confirmation duration, continue using the last stable **Run Steering Frame** and reject the spike as contact noise. If the large change persists through confirmation, accept it as real support and snap or reinitialize the steering frame to it instead of slowly slewing across the full angle.

This slice is complete when one-frame side-edge spikes no longer redirect steering, while real sharp transitions still update promptly and predictably.

## Acceptance criteria

- [ ] Large normal jumps beyond the configured snap angle are identified while continuously grounded.
- [ ] A one-frame suspect normal does not rotate the stable **Run Steering Frame**.
- [ ] Suspect confirmation time uses the configured seconds value and fixed delta time.
- [ ] A suspect normal that persists through confirmation is accepted as real support.
- [ ] Confirmed large discontinuities snap or reinitialize the steering frame instead of slow-slewing across the full angle.
- [ ] Suspect state clears when the candidate disappears, becomes invalid, matches the stable frame again, or reset occurs.
- [ ] Ordinary below-threshold normal changes still use the slew behavior from the previous slice.
- [ ] Invalid normals remain rejected and cannot be confirmed.
- [ ] Raw **RunSurfaceContext** remains unchanged.

## Verification

- EditMode tests:
  - Large one-frame normal spike is ignored while the last stable frame remains in use.
  - Suspect candidate accumulates confirmation time only while the candidate remains valid and consistent enough to represent the same large change.
  - Suspect candidate disappearing before confirmation clears suspect state.
  - Candidate persisting through confirmation snaps or reinitializes the frame.
  - Confirmed large discontinuity does not slew across the full angle.
  - Below-threshold normal changes still follow angular slew behavior.
  - Invalid normals cannot enter or confirm suspect state.
  - Reset clears suspect state.
- PlayMode tests:
  - Not expected for this slice unless deterministic unit coverage cannot represent the edge-contact pattern.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Targeted EditMode tests for steering-frame source.
- Manual Unity smoke check:
  - Ride high on the **Side Bank** and confirm one-frame edge noise no longer produces a sudden steering-frame flip.
  - Cross an intentionally sharp support transition and confirm steering reorients after confirmation rather than feeling permanently stuck.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 - Slew Ordinary Grounded Run Surface Normal Changes
