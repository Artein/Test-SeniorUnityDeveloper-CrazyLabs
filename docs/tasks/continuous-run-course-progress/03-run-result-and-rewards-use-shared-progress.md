# 03 — Make Run Result and rewards consume final shared progress

**Type:** AFK
**User stories covered:** 6–8, 30, 37–38, 60, 71, 75, 77–78

## Parent

[Continuous Run Course Progress PRD](../../prd/prd-continuous-run-course-progress.md)

## What to build

Complete the result path by making run termination request the final authoritative Run Progress Sample from Run Progress Service. Run Result display and distance rewards must consume the same maximum reached progress, including when the Run Body rolls backward before termination. Remove duplicated final-position projection from the run-end path while preserving Run Finish precedence over simultaneous Lost Momentum candidates.

## Acceptance criteria

- [ ] Run termination obtains a final service-owned sample for the final Run Body state without duplicating course projection logic.
- [ ] Run Result distance uses maximum reached progress and does not decrease after rollback.
- [ ] Distance rewards consume the exact same progress value used by Run Result.
- [ ] Run Finish remains authoritative when finish and Lost Momentum become eligible in the same transition.
- [ ] Existing Run Result terminology, UI behavior, rounding, and reward tuning remain unchanged unless explicitly covered by the parent PRD.
- [ ] Invalid final projection follows the approved failure policy and cannot silently replace a valid maximum with fabricated progress.
- [ ] Result and reward behavior survives the real serialized scene lifecycle.

## Verification

- EditMode tests: Final sample updates maximum progress; rollback preserves the result; reward input equals result input; invalid final projection follows policy; finish precedence remains unchanged.
- PlayMode tests: End a run after forward travel and rollback; verify displayed and rewarded distance match; verify finish-versus-Lost-Momentum precedence in scene composition.
- Static checks: Rider problems clean; Unity compile gate clean before tests; no duplicate course calculation remains in the run-end policy.
- Manual Unity smoke check: Complete and fail representative Ladybug runs and compare result/reward behavior with current tuning.
- Package version/changelog: No package change; document any serialized migration or player-visible correction.

## Blocked by

- 02 — Ship the straight Run Course sample with exact Ladybug parity
