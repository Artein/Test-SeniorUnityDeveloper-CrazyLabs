# Add Course-Aligned Downhill Acceleration

Type: AFK

## Parent

[PRD: Run Body Explicit Speed Ownership](../../prd/prd-run-body-explicit-speed-ownership.md)

## What to build

Add the first intentional **Run Body Speed Model** effect: predictable downhill acceleration while moving forward on a valid grounded **Run Surface**.

A pure **Run Body Speed Evaluator** receives normalized support and movement facts plus raw speed tuning and returns a **Run Body Speed Decision** expressed as rates. Downhill strength follows the sine of the positive forward-downhill angle. Positive acceleration scales proportionally with the clamped positive portion of **Course Forward Alignment**, so forward travel receives the full effect, diagonal travel receives a proportional effect, and lateral or reversed travel receives none.

The movement controller integrates the decision using fixed-step elapsed time, prevents model-authored acceleration from crossing the configured base soft envelope, preserves corrected surface-normal velocity, and still performs one final target write. The evaluator contributes speed only; it never chooses heading.

## Acceptance criteria

- [ ] A pure **Run Body Speed Evaluator** can be exercised without Rigidbody, scene lifecycle, or Unity callbacks.
- [ ] The evaluator returns tangent acceleration and diagnostic contributor data rather than a final velocity.
- [ ] Missing, invalid, ungrounded, stale, or meaningfully departing support produces a neutral speed decision.
- [ ] Flat and uphill traversal add no downhill acceleration.
- [ ] Positive forward-downhill angle produces monotonic acceleration based on its sine.
- [ ] Positive downhill acceleration scales proportionally from zero to full strength using positive **Course Forward Alignment**.
- [ ] Lateral, reversed, and directionless travel receives no downhill acceleration.
- [ ] The speed decision never selects, rotates, or repairs travel heading.
- [ ] The downhill contributor appears only when downhill acceleration is active and does not alter numerical output.
- [ ] Decision rates are integrated once by movement orchestration using fixed-step elapsed time.
- [ ] Model-authored downhill acceleration cannot cross the configured base soft envelope.
- [ ] Corrected surface-normal velocity remains unchanged by downhill tangent acceleration.
- [ ] Airborne or unsupported motion remains driven by Rigidbody physics and direction-only air steering.
- [ ] No surface profile or terrain/material lookup is introduced.

## Verification

- EditMode tests:
  - Unsupported and departing-support contexts return neutral output.
  - Flat and uphill angles return zero downhill acceleration.
  - Increasing positive downhill angles produce the expected monotonic sine-scaled result.
  - Forward, diagonal, lateral, reversed, and directionless alignment cases apply the expected proportional gating.
  - Downhill contribution stops at the base soft envelope without clamping pre-existing overspeed.
  - Contributor flags explain active downhill acceleration without affecting output.
  - Movement integration preserves corrected normal velocity and writes once.
- PlayMode tests:
  - A valid grounded downhill section gains more tangent speed than an equivalent flat section.
  - Launch flight remains unaffected before valid landing.
- Static checks:
  - Rider reformat and problem inspection for changed code and tests.
  - Unity connector compile before tests.
  - Confirm the evaluator has no Rigidbody or scene-lifecycle dependency.
- Manual Unity smoke check:
  - Compare forward, diagonal, lateral, and reversed traversal on a downhill section and confirm that only course-aligned travel is rewarded.
- Package version/changelog:
  - Not required.

## Blocked by

- [02 - Install the Single-Writer Movement Baseline](02-install-single-writer-movement-baseline.md)
