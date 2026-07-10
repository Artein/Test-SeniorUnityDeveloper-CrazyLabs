# Expose Speed-Decision Diagnostics

Type: AFK

## Parent

[PRD: Run Body Explicit Speed Ownership](../../prd/prd-run-body-explicit-speed-ownership.md)

## What to build

Extend run diagnostics so an engineer or designer can observe why the **Run Body Speed Model** changed tangent speed during a fixed pass.

Expose current tangent speed, effective soft envelope, valid-support state, forward-downhill angle, **Course Forward Alignment**, active speed-decision contributors, effective low-speed assist target, assist-attempt state, and remaining requested assist budget. Diagnostics must read the same normalized context, decision, and movement state used by production orchestration rather than recomputing policy from scene objects.

Present contributors as a flags-style list because acceleration, slowdown, envelope resistance, and low-speed assist may contribute together. Diagnostics explain behavior only: enabling, disabling, recording, or displaying them must not alter movement output or become an input to gameplay decisions.

Use the standalone actor-aware movement diagram as the high-level documentation entry point. Update linked engineering notes only if implementation terminology or observation boundaries changed.

## Acceptance criteria

- [x] Diagnostics expose sampled tangent speed and the effective soft envelope for the active fixed pass.
- [x] Diagnostics expose valid grounded support, forward-downhill angle, and course-forward alignment used by the evaluator.
- [x] Diagnostics expose every active speed-decision contributor as a combinable flags-style list.
- [x] Diagnostics distinguish downhill acceleration, ordinary slowdown, above-envelope resistance, and low-speed assist.
- [x] Diagnostics expose effective assist target, attempt state, and remaining requested assist budget.
- [x] Inactive, unsupported, directionless, reset, and unavailable states are represented explicitly rather than as misleading stale values.
- [x] Values come from the production movement context and decision boundary rather than a second policy implementation.
- [x] Diagnostic capture and display do not change evaluator output, movement integration, lifecycle state, or Rigidbody writes.
- [x] Contributor flags remain descriptive metadata and are never used as gameplay branch conditions.
- [x] Labels use the approved **Run Body**, **Run Surface**, speed-envelope, sanity-guard, and low-speed-assist terminology.
- [x] The actor-aware Mermaid artifact remains the canonical high-level overview and is referenced rather than duplicated.

## Completion evidence

- Diagnostics implementation plus all then-changed tests: 694/694 passed (`r_xmy0m4x5`).
- PlayMode equivalence proves observed and unobserved diagnostics produce identical movement output.
- Static inspection confirms contributor flags are only produced by policy and consumed by diagnostics/tests, never by gameplay branching.
- The standalone actor-aware Mermaid artifact remains the linked high-level system overview.

## Verification

- EditMode tests:
  - Combined policy effects produce the expected contributor flags.
  - Adding or removing diagnostic observation does not change numerical speed decisions.
  - Assist states and remaining budget reflect start, spend, pause, exhaustion, rearm, and reset.
  - Unsupported and inactive snapshots clear or mark values unavailable instead of retaining stale active data.
- PlayMode tests:
  - Scene diagnostics display or publish values that match the active movement context during downhill acceleration, slowdown, overspeed resistance, and low-speed assist.
  - Enabling and disabling visible diagnostics does not change measured movement output.
- Static checks:
  - Rider reformat and problem inspection for changed code and tests.
  - Unity connector compile before tests.
  - Confirm no gameplay service branches on contributor flags or diagnostic display state.
  - Confirm documentation links resolve and terminology matches the glossary.
- Manual Unity smoke check:
  - Observe a downhill run, flat slowdown, upgraded overspeed, recoverable seam, blocked assist attempt, unsupported flight, and run reset; verify the diagnostic explanation matches what is visible on screen.
- Package version/changelog:
  - Not required.

## Blocked by

- [06 - Add Bounded Low-Speed Assist](06-add-bounded-low-speed-assist.md)
