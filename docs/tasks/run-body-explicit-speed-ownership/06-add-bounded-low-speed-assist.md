# Add Bounded Low-Speed Assist

Type: AFK

## Parent

[PRD: Run Body Explicit Speed Ownership](../../prd/prd-run-body-explicit-speed-ownership.md)

## What to build

Add **Run Body Low-Speed Assist** for recoverable supported incidents such as a small seam loss or soft scrape, while preserving the ability of blocking geometry and **Lost Momentum** to end a run.

When valid grounded support, positive **Course Forward Alignment**, below-target tangent speed, and a usable existing tangent direction first make a slowdown episode eligible, the movement controller opens one assist attempt. Its total requested velocity budget equals the initial deficit to the effective assist target, where the effective target is the lower of the raw assist target and the resolved soft envelope. The authored assist acceleration controls how quickly that fixed budget is requested.

Spend requested assist delta before Rigidbody resolution. Solver cancellation therefore consumes the attempt instead of granting free retries. Support loss pauses the same attempt without replenishing it. Exact-zero or directionless motion neither invents heading nor spends budget. Rearm only after sampled tangent speed independently exceeds the target by a small numeric tolerance or a new run begins.

Keep the first slice observation-based. Do not add obstruction raycasts, persistent blocking-contact tracking, or a dependency on **Lost Momentum** internals.

## Acceptance criteria

- [x] Designers can tune low-speed assist target and low-speed assist acceleration independently.
- [x] The effective assist target is the lower of the raw target and resolved soft envelope.
- [x] Assist starts only with valid grounded support, below-target speed, positive course-forward alignment, and a usable existing tangent direction.
- [x] One eligible low-speed episode opens one attempt whose total requested velocity budget equals its initial deficit.
- [x] Requested assist delta spends budget before physics resolution.
- [x] Repeated solver cancellation cannot replenish or extend the attempt budget.
- [x] Assist approaches the effective target at the authored rate without crossing the target or soft envelope.
- [x] Support loss pauses the current attempt and does not replenish it.
- [x] Exact-zero and directionless states do not synthesize heading, open propulsion in an arbitrary direction, or spend budget.
- [x] Lateral and reversed movement receive no positive assist.
- [x] Independent sampled recovery above the effective target plus numeric tolerance rearms assistance.
- [x] Starting a new run rearms assistance and leaving **Running** clears attempt state.
- [x] Blocking geometry can exhaust one attempt and still stop the Run Body.
- [x] **Lost Momentum** remains an independent observer and can end a genuine stall.
- [x] No obstruction raycast, persistent blocking-contact tracker, or dependency on **Lost Momentum** internals is introduced.
- [x] Diagnostic state exposes whether assist is eligible, active, paused, exhausted, or rearmed and how much requested budget remains.

## Completion evidence

- Assist state, orchestration, lifecycle, and diagnostic slice: 141/141 passed (`r_bdkwi22u`).
- Focused movement PlayMode slice: 9/9 passed (`r_7rc81e91`), including recoverable motion, solver-cancelled budget exhaustion, and unsupported pause/resume.
- Static inspection found no obstruction sensing, persistent blocking-contact tracker, or dependency on Lost Momentum internals.

## Verification

- EditMode tests:
  - Eligible near-stall movement approaches the effective target at the authored acceleration.
  - A raw target above the envelope resolves to the envelope without mutating config.
  - Total requested assist cannot exceed the initial deficit.
  - Solver-cancelled requested delta still spends the budget.
  - Support loss pauses without replenishment, and resumed support continues the same attempt.
  - Exact zero, missing direction, lateral travel, and reversed travel do not receive or spend assist.
  - Independent recovery above target plus tolerance and a new run each rearm the attempt.
  - Run exit clears attempt state.
  - One blocked low-speed episode cannot receive continuous propulsion.
- PlayMode tests:
  - A recoverable seam or soft scrape can receive limited assistance.
  - A blocking obstacle can exhaust assistance and still reach the existing **Lost Momentum** outcome.
  - Unsupported transitions do not gain airborne assist and do not reset the attempt.
- Static checks:
  - Rider reformat and problem inspection for changed code and tests.
  - Unity connector compile before tests.
  - Confirm no obstruction sensing or **Lost Momentum** implementation dependency was added.
- Manual Unity smoke check:
  - Compare a small seam loss, soft scrape, exact stop, and blocked obstacle; confirm limited help only for the recoverable moving cases.
- Package version/changelog:
  - Not required.

## Blocked by

- [05 - Make PlayerMaxSpeed the Soft Speed Envelope](05-make-player-max-speed-soft-speed-envelope.md)
