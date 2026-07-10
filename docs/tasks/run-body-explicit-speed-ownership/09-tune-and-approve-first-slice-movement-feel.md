# Tune and Approve First-Slice Movement Feel

Type: HITL

## Parent

[PRD: Run Body Explicit Speed Ownership](../../prd/prd-run-body-explicit-speed-ownership.md)

## What to build

Use the completed diagnostics and production-scene regression path to select and approve the initial **Run Body Movement Tuning** values for the first explicit-speed release.

A designer or product owner must compare weak, neutral, and upgraded runs across representative downhill, flat, seam, soft-scrape, blocking-obstacle, collision-overspeed, and high-speed obstacle cases. Tune the global movement asset so course shape, ordinary slowdown, limited recovery, and PlayerMaxSpeed progression are visible and understandable without creating a hard speed wall, automatic heading, endless obstacle pushing, or a defensive sanity-guard dependency.

This gate approves the first-slice movement feel for merge. It is not a redesign of launch, physics materials, obstacle layouts, upgrade ownership, **Lost Momentum**, camera, presentation, or future surface-specific behavior. Any architectural or product-scope failure returns to its owning implementation issue rather than being hidden by tuning.

## Acceptance criteria

- [ ] A human reviewer compares weak, neutral, and upgraded runs on the same representative course conditions.
- [ ] Downhill acceleration is noticeable and predictable during course-aligned travel.
- [ ] Flat traversal slowdown is understandable without feeling like hidden steering brake.
- [ ] Lateral and reversed travel receive no positive downhill or low-speed assistance.
- [ ] Stronger launch retains an observable advantage.
- [ ] Unsupported flight and first landing preserve expected physical behavior and tangent momentum.
- [ ] Neutral and upgraded PlayerMaxSpeed values produce a visible difference in sustainable useful speed without a hard cap.
- [ ] Collision overspeed remains initially visible and settles at an acceptable rate.
- [ ] A recoverable seam or soft scrape receives useful but limited assistance.
- [ ] Exact stop does not invent a heading, and a blocking obstacle can exhaust assistance and remain stopped.
- [ ] **Lost Momentum** still ends a genuine stalled run.
- [ ] High-speed obstacle approach remains readable and produces reliable collision or run-end behavior.
- [ ] The defensive **Run Body Speed Sanity Guard** remains unreachable throughout valid authored scenarios.
- [ ] The approved raw values are stored in the single movement-tuning asset with no hidden config resolution.
- [ ] Diagnostics explain observed speed changes using support, slope, alignment, envelope, contributors, and assist budget.
- [ ] The reviewer records approval or returns concrete failed scenarios to the owning issue.
- [ ] No surface profile, terrain/material inference, unrelated system retuning, or architecture expansion is included.

## Verification

- EditMode tests:
  - Re-run the complete targeted evaluator, config, stat, movement-composition, and bounded-assist suites after final authored-value changes.
- PlayMode tests:
  - Re-run the complete focused Gameplay scene movement suite, including high-speed obstacle and lifecycle-reset coverage.
- Static checks:
  - Unity connector compile before tests.
  - Rider problem inspection for any changed code or tests.
  - Validate authored config and resolved PlayerMaxSpeed ranges against the defensive sanity threshold.
  - Run git diff whitespace and documentation-link checks.
- Manual Unity smoke check:
  - Required HITL review of downhill, flat, seam, soft scrape, exact stop, blocking obstacle, collision overspeed, high-speed obstacle, run reset, and weak/neutral/upgraded comparisons.
- Package version/changelog:
  - Not required.

## Blocked by

- [07 - Expose Speed-Decision Diagnostics](07-expose-speed-decision-diagnostics.md)
- [08 - Prove Gameplay Scene Speed Ownership End to End](08-prove-gameplay-scene-speed-ownership-end-to-end.md)
