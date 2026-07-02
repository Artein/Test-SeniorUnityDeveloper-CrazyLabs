## Parent

[Run Steering Control PRD](../../prd/prd-run-steering-control.md)

## What to build

Prove that Run Steering Control output still passes through Run Steering Responsiveness before affecting Launch Target movement. The new input mapping should change requested steering, but responsiveness, movement speed limits, and turn-rate limits should remain distinct gameplay concepts.

This slice should preserve the existing responsiveness upgrade path under the corrected terminology and verify that movement behavior still uses resolved stat values where applicable.

## Acceptance criteria

- [ ] Run Steering Responsiveness controls how quickly applied steering moves toward requested steering.
- [ ] Run Steering Responsiveness is not treated as input polling frequency.
- [ ] The existing player steering responsiveness stat/upgrade path still affects steering response speed.
- [ ] Neutral responsiveness settings preserve current smooth steering behavior within expected tolerance.
- [ ] Higher responsiveness settings move applied steering toward requested steering faster.
- [ ] Maximum turn rate remains a separate movement limit.
- [ ] Minimum steer speed remains separate from input mapping.
- [ ] Maximum planar speed remains separate from input mapping.
- [ ] Run Steering Range and Run Steering Deadzone do not bypass responsiveness smoothing.
- [ ] Movement code does not reintroduce a sensitivity multiplier after gesture output.

## Verification

- EditMode tests:
  - Requested steering changes immediately from gesture output while applied steering moves through responsiveness smoothing.
  - Low responsiveness produces slower applied steering convergence.
  - High responsiveness produces faster applied steering convergence.
  - Responsiveness stat resolution still changes convergence speed in the expected direction.
  - Maximum turn rate still limits rotation after input mapping changes.
  - Minimum steer speed and maximum planar speed remain independent from gesture mapping.
  - No sensitivity multiplier is applied after gesture output.
- PlayMode tests:
  - Add only if scene/runtime behavior is required to prove responsiveness integration.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
  - Source search confirms the old response-rate terminology is not the active gameplay API.
  - Source search confirms the responsiveness upgrade/stat id still routes into player steering.
- Manual Unity smoke check:
  - Compare low and boosted responsiveness settings during a short run and confirm the Launch Target response changes without changing touch sampling.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 03 - Wire Run Steering Control Into Running
