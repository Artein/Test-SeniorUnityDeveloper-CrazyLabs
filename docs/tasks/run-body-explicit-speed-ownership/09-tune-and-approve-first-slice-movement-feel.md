# Tune and Approve First-Slice Movement Feel

Type: HITL

## Parent

[PRD: Run Body Explicit Speed Ownership](../../prd/prd-run-body-explicit-speed-ownership.md)

## What to build

Use the completed diagnostics and production-scene regression path to select and approve the initial **Run Body Movement Tuning** values for the first explicit-speed release.

A designer or product owner must compare weak, neutral, and upgraded runs across representative downhill, flat, seam, soft-scrape, blocking-obstacle, collision-overspeed, and high-speed obstacle cases. Tune the global movement asset so course shape, ordinary slowdown, limited recovery, and PlayerMaxSpeed progression are visible and understandable without creating a hard speed wall, automatic heading, endless obstacle pushing, or a defensive sanity-guard dependency.

This gate approves the first-slice movement feel for merge. It is not a redesign of launch, physics materials, obstacle layouts, upgrade ownership, **Lost Momentum**, camera, presentation, or future surface-specific behavior. Any architectural or product-scope failure returns to its owning implementation issue rather than being hidden by tuning.

## Acceptance criteria

- [x] A human reviewer compares weak, neutral, and upgraded runs on the same representative course conditions.
- [x] Downhill acceleration is noticeable and predictable during course-aligned travel.
- [x] Flat traversal slowdown is understandable without feeling like hidden steering brake.
- [x] Lateral and reversed travel receive no positive downhill or low-speed assistance.
- [x] Stronger launch retains an observable advantage.
- [x] Unsupported flight and first landing preserve expected physical behavior and tangent momentum.
- [x] Neutral and upgraded PlayerMaxSpeed values produce a visible difference in sustainable useful speed without a hard cap.
- [x] Collision overspeed remains initially visible and settles at an acceptable rate.
- [x] A recoverable seam or soft scrape receives useful but limited assistance.
- [x] Exact stop does not invent a heading, and a blocking obstacle can exhaust assistance and remain stopped.
- [x] **Lost Momentum** still ends a genuine stalled run.
- [x] High-speed obstacle approach remains readable and produces reliable collision or run-end behavior.
- [x] The defensive **Run Body Speed Sanity Guard** remains unreachable throughout valid authored scenarios.
- [x] The approved raw values are stored in the single movement-tuning asset with no hidden config resolution.
- [x] Diagnostics explain observed speed changes using support, slope, alignment, envelope, contributors, and assist budget.
- [x] The reviewer records approval or returns concrete failed scenarios to the owning issue.
- [x] No surface profile, terrain/material inference, unrelated system retuning, or architecture expansion is included.

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

## Human review record

Status: **Approved by Artem Sukhliak**

Production tuning under review:

- Downhill acceleration: `8 m/s per second` at full downhill contribution.
- Ordinary supported-surface slowdown: `0.5 m/s per second`.
- Low-speed assist: `5 m/s` raw target, `8 m/s per second` acceleration, one bounded attempt.
- Soft speed envelope: `20 m/s` base; PlayerMaxSpeed upgrades reach a `2x` factor (`40 m/s`).
- Above-envelope resistance: `12 m/s per second` at twice the effective envelope.
- Defensive sanity threshold: `250 m/s`; valid authored envelopes remain well below it.

Automated evidence prepared for review:

- Production-scene high-speed collision/run-end and clean upgraded relaunch proof: 2/2 passed (`r_njwedred`).
- Issue 08 production-scene ownership and lifecycle suite: 50/50 passed (`r_ipa77xph`).
- Changed regression suite: 696/696 passed (`r_c9stm43x`).
- Full Unity project suite: 970/970 passed (`r_1mhs0tfi`).
- Static checks confirm one production velocity writer, no active legacy writer, unchanged migrated values, valid ADR metadata, and no forbidden project or package drift.

Manual comparison procedure:

1. Inspect `Assets/Game/Gameplay/RunBodyMovementConfig.asset`. Confirm the Inspector presents distinct speed, movement-validity, defensive-sanity, launch-landing, steering, screen-metrics, direction, and steering-frame groups with readable labels, units, tooltips, and bounded numeric controls.
2. Open `Assets/Scenes/GameplayScene.unity`; on **Gameplay Composition**, temporarily enable **Run Diagnostics Overlay** without saving that diagnostic toggle.
3. Keep the course and steering path comparable across a weak launch, a strong launch with neutral PlayerMaxSpeed, and a strong launch after upgrading PlayerMaxSpeed.
4. Observe downhill, flat, lateral/reversed, seam or soft scrape, exact stop, blocking obstacle, collision overspeed, high-speed obstacle, run end, and relaunch behavior against the acceptance criteria above. Confirm diagnostics agree with the visible behavior and the sanity guard is never a contributor.
5. Record `Approved` or list each failed scenario with launch strength, PlayerMaxSpeed level, location, observed diagnostics, and expected behavior.

Verdict: **Approved**

Reviewer notes: Movement feel is approved for this change. Small bumps still cause excessive repeated slight lift, making the character appear to jump continually over parts of the run. This behavior predates the explicit-speed refactor and does not block its approval.

Follow-up observation: Investigate small-bump repeated lift separately at the Run Surface/contact, landing, and presentation boundaries. Do not retune the explicit speed model to hide the behavior without first identifying its owner.

## Blocked by

- [07 - Expose Speed-Decision Diagnostics](07-expose-speed-decision-diagnostics.md)
- [08 - Prove Gameplay Scene Speed Ownership End to End](08-prove-gameplay-scene-speed-ownership-end-to-end.md)
