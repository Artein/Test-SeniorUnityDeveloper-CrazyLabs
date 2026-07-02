## Parent

[Run Steering Control PRD](../../prd/prd-run-steering-control.md)

## What to build

Run a human-in-the-loop mobile feel review for the completed Run Steering Control. The goal is to validate the initial physical tuning on a representative phone-class target, especially range, deadzone, edge-origin behavior, and responsiveness feel.

This slice should produce tuning feedback and, if needed, small configuration-only adjustments. It should not redesign the control, add visible UI, or reopen implementation architecture unless testing reveals a blocking problem.

## Acceptance criteria

- [ ] The feature is tested on at least one representative mobile device or device-equivalent target.
- [ ] Starting touch near screen center feels neutral until horizontal movement.
- [ ] Starting touch near left and right screen edges keeps strict physical range behavior.
- [ ] `1.5cm` Run Steering Range is either accepted or adjusted with a recorded reason.
- [ ] `0.15` Run Steering Deadzone is either accepted or adjusted with a recorded reason.
- [ ] Fallback DPI `326` remains accepted or is adjusted with a recorded reason.
- [ ] Run Steering Responsiveness feels like movement response speed, not input lag or polling.
- [ ] Pre-Launch Pull still feels unaffected.
- [ ] Any tuning change is limited to authored configuration unless a defect is found.
- [ ] A short QA note records the tested device/target, result, and final tuning values.

## Verification

- EditMode tests:
  - Re-run affected config or movement tests only if tuning changes alter test expectations.
- PlayMode tests:
  - Re-run affected gameplay scene or controller tests only if tuning changes alter runtime expectations.
- Static checks:
  - `git diff --check` if any tuning files are changed.
  - Unity compile through Unity AI Agent Connector if any Unity asset or code changes are made.
- Manual Unity smoke check:
  - Play a short run using center, left-edge, and right-edge touch starts.
  - Compare small jitter, half-range movement, full-range movement, and beyond-range movement.
  - Confirm release/cancel behavior feels immediate.
  - Confirm Pre-Launch Pull still behaves as expected.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 05 - Close Run Steering Unity Verification
