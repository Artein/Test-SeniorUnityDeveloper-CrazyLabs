# Run Human Feel And Authoring QA

## Parent

[Slingshot Target-Coupled Band PRD](../../prd/prd-slingshot-target-coupled-band.md)

## Type

HITL

## User stories covered

9-15, 40-41

## What to build

Run a human validation pass for target-coupled Slingshot feel and authoring. This slice should validate that the Band visually reads as pulling and pushing the Launch Target, that the collider-aligned wrap looks acceptable from the Gameplay camera, that tuning values are practical for a designer, and that editor/device input feel remains comfortable.

This slice may produce a short QA report and small tuning-only changes. Larger behavior changes should become new issues rather than being hidden inside QA.

QA report: [Human Feel And Authoring QA Report](07-human-feel-and-authoring-qa-report.md)

## Acceptance criteria

- [ ] Human tester confirms the Launch Target feels directly pulled during Active Pull.
- [ ] Human tester confirms the Band Contact Points and Band Wrap do not visibly cut through the target mesh in normal play.
- [ ] Human tester confirms off-center Pulls still steer in the expected opposite direction.
- [ ] Human tester confirms Band Release Recoil reads as the Band pushing the Launch Target forward before detaching.
- [ ] Human tester confirms Band Release Recoil does not make the target feel tethered after rest.
- [ ] Designer-facing config fields are understandable and practical to tune.
- [ ] Slingshot gizmos are useful enough to inspect anchors, Pull limits, and target-coupled Band behavior.
- [x] Editor mouse simulation remains usable for local iteration.
- [x] Touch-device validation is run if a device is available; otherwise the reason is recorded.
- [ ] QA findings are recorded, including accepted tuning values and any follow-up issues.

## Verification

- EditMode tests: none expected unless QA finds a deterministic regression suitable for automated coverage.
- PlayMode tests: run existing target-coupled Slingshot PlayMode tests after any tuning/code changes.
- Static checks: Unity compile after any code/asset changes; Rider checks if code changes occur.
- Manual Unity smoke check: Gameplay Scene Play Mode with editor mouse; touch-device run when available; inspect normal and off-center Pulls, valid Launch, recoil, reset, and authoring warnings.
- Package version/changelog: no package/changelog change unless the project later adopts gameplay changelog policy.

## Blocked by

- 06 - Wire GameplayScene Target-Coupled Band
