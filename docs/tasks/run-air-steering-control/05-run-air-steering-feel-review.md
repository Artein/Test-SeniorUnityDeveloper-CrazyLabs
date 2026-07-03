# Run Air Steering Feel Review

Type: HITL

## Parent

`docs/prd/prd-run-air-steering-control.md`

## What to build

Run a manual Unity feel review for **Run Air Steering Control** after the AFK implementation slices are complete.

The review should validate that air steering gives useful agency without feeling like grounded driving, without reducing launch distance, and without adding hidden speed. It should also decide final authored values for air turn authority and accepted lift tolerance if implementation used conservative defaults.

This is intentionally HITL because the exact feel values are product tuning decisions. The implementation should already be technically correct before this slice starts.

## Acceptance criteria

- [ ] Small pull still produces a short launch.
- [ ] Maximum pull still produces a far launch.
- [ ] Air steering during launch flight gently bends direction without visible speed loss.
- [ ] Air steering after a side bump feels consistent with launch-flight air steering.
- [ ] Air steering while falling before **RunEnded** feels useful but not overpowered.
- [ ] Grounded steering feels stronger and more responsive than air steering.
- [ ] No-touch airborne motion has no visible hidden guidance.
- [ ] Landing transitions back to grounded steering without a jarring control change.
- [ ] Repeated win/fail/reset cycles do not leak air steering state into the next run.
- [ ] Final air turn authority and accepted lift tolerance values are either accepted or recorded for a follow-up tuning issue.

## Verification

- EditMode tests:
  - Existing implementation tests remain green after tuning changes.
- PlayMode tests:
  - Scene composition tests remain green after tuning changes.
- Static checks:
  - Unity connector compile before tests if any serialized or code tuning changes are made.
  - Rider reformat/problems only if files change.
- Manual Unity smoke check:
  - Required HITL playtest covering small pull, max pull, launch air steering, later bump air steering, falling air steering, landing, and repeated run reset.
- Package version/changelog:
  - Not required.

## Blocked by

- Scene Contract And Air Steering Tuning
