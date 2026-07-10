# Make PlayerMaxSpeed the Soft Speed Envelope

Type: AFK

## Parent

[PRD: Run Body Explicit Speed Ownership](../../prd/prd-run-body-explicit-speed-ownership.md)

## What to build

Give PlayerMaxSpeed one explicit player-facing meaning: it modifies the **Run Body Speed Envelope** used by intentional valid-grounded tangent-speed policy.

Resolve PlayerMaxSpeed through the existing immutable active-run gameplay-stat snapshot and resolver cache while building each active fixed-pass context. A neutral stat preserves the configured base soft maximum; an upgrade raises the effective envelope through the existing stat semantics. Do not add a second scalar cache or read mutable upgrade state directly during a run.

Below the envelope, model-authored positive acceleration may approach but not cross it. Above the envelope, direction-independent resistance grows with normalized excess and reaches full authored resistance at twice the effective envelope. Launch, gravity, collision, or solver-produced overspeed must remain initially visible and settle through resistance rather than an immediate clamp. The separate **Run Body Speed Sanity Guard** continues to contain only impossible values.

## Acceptance criteria

- [x] The effective soft envelope is derived from the configured base soft maximum and PlayerMaxSpeed resolved from the immutable active-run snapshot.
- [x] A neutral PlayerMaxSpeed result preserves the configured base soft envelope.
- [x] An upgraded PlayerMaxSpeed result raises the sustainable useful speed in the expected direction.
- [x] Repeated resolution uses the existing centralized resolver cache and adds no second scalar stat cache.
- [x] Active-run upgrade changes do not alter an already captured run snapshot.
- [x] Non-finite, non-positive, or sanity-range-invalid resolved envelope values prevent movement from starting or continuing into fixed-step policy.
- [x] Model-authored positive acceleration approaches but does not cross the effective soft envelope.
- [x] Pre-existing overspeed is not immediately clamped to the envelope.
- [x] Direction-independent resistance begins only above the envelope and scales by normalized excess.
- [x] Resistance reaches full authored strength at twice the effective envelope and does not require course-forward alignment.
- [x] Launch-, gravity-, collision-, and solver-produced overspeed remains observable before settling.
- [x] The **Run Body Speed Sanity Guard** remains far above every valid base and upgraded envelope and never acts as balance tuning.
- [x] Unsupported and airborne motion receives neither model acceleration gating nor envelope resistance.
- [x] No upgrade save format, ownership semantics, or mid-run mutation behavior changes.

## Completion evidence

- Envelope, stat-snapshot, resolver-cache, overspeed, and preparation slice: 171/171 passed (`r_hkchmypq`).
- The existing resolver allocation test remains green; static inspection found no second scalar cache or direct mutable-upgrade read.
- The authored base envelope is 20 m/s, the maximum supported PlayerMaxSpeed factor is 2x, and the defensive sanity guard remains separate at 250 m/s.

## Verification

- EditMode tests:
  - Neutral and upgraded PlayerMaxSpeed values produce the expected effective envelopes.
  - Invalid resolved values fail at active-run preparation rather than being repaired in fixed-step policy.
  - Repeated resolution on an unchanged active snapshot is stable, centralized, and allocation-free.
  - Model acceleration stops at the envelope without reducing pre-existing overspeed immediately.
  - Resistance is zero at or below the envelope, grows with normalized excess, and reaches full strength at twice the envelope.
  - Resistance is equal for forward, lateral, and reversed overspeed.
  - Unsupported contexts remain neutral.
- PlayMode tests:
  - Neutral and upgraded runs sustain visibly different useful speeds under equivalent grounded conditions.
  - Launch overspeed and collision-produced overspeed are not hard-clamped on the first movement pass.
  - Existing upgrade snapshot semantics remain unchanged during an active run.
- Static checks:
  - Rider reformat and problem inspection for changed code and tests.
  - Unity connector compile before tests.
  - Confirm stat reads go through the existing active-run resolver and no duplicate cache was introduced.
  - Confirm the gameplay envelope and defensive sanity threshold remain distinct names and values.
- Manual Unity smoke check:
  - Compare neutral and upgraded runs on the same course, including a collision overspeed case, and confirm a higher sustainable speed without a hard wall.
- Package version/changelog:
  - Not required; no package or save-format change.

## Blocked by

- [04 - Add Direction-Independent Surface Slowdown](04-add-direction-independent-surface-slowdown.md)
