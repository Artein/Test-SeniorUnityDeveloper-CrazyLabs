# Run Human Recoil Feel And Authoring QA

## Parent

[Slingshot Band Release Recoil Clearance PRD](../../prd/prd-slingshot-band-release-recoil-clearance.md)

## Type

HITL

## User stories covered

1-7, 10-13, 40, 55

## What to build

Run a human Unity smoke pass after the implementation slices are complete. The goal is to verify that the Band no longer passes through the Launch Target on low-impulse launches while still detaching naturally and preserving existing Slingshot feel.

Record any follow-up tuning or authoring concerns separately instead of expanding this bug-fix scope unless they block the recoil-clearance behavior.

## Acceptance criteria

- [ ] A shallow valid Pull Release just above MinimumPullDistance shows no visible Band pass-through from the player camera.
- [ ] A deeper Pull Release still reads as the Band pushing the Launch Target and returning naturally.
- [ ] Left and right lateral launches preserve expected steering feel and do not reveal Band clipping.
- [ ] The Band detaches after the Launch Target clears the rest/idle/default path and does not appear tethered during the Run.
- [ ] Existing Band Recoil Duration and Band Contact Padding still feel usable without a broad balance pass.
- [ ] Any remaining visual or authoring concerns are recorded as follow-up work, not hidden in the implementation slices.

## Verification

- EditMode tests: none beyond the already green implementation-slice tests.
- PlayMode tests: targeted Slingshot PlayMode regression suite from implementation slices remains green before manual QA.
- Static checks: Unity compile is clean before manual QA.
- Manual Unity smoke check: shallow valid launch, deeper launch, left lateral launch, right lateral launch, detach timing, and transition into Run from the player camera.
- Package version/changelog: no package/changelog change.

## Blocked by

- 04 - Harden Detach Lifecycle And Regression Suite
