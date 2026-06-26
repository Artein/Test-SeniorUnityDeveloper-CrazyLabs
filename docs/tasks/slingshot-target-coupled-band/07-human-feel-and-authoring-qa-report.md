# Human Feel And Authoring QA Report

Parent issue: [Run Human Feel And Authoring QA](07-run-human-feel-and-authoring-qa.md)

## Status

Implementation is verified by automated tests. Human feel, designer authoring, and physical touch-device QA are still pending human sign-off.

## Automated Evidence

- Unity compile: succeeded via `.unity-ai-agent-connector/bin/uaiac compile`, with connector warning `editor_session_degraded`.
- Targeted selector run `r_lzgje7qr`: 79 total, 79 passed, 0 failed.
- Editor mouse simulation: verified by Gameplay Scene PlayMode smoke coverage and UnityInput EditMode coverage.
- Gameplay Scene smoke coverage verifies target-coupled pull, ordered Band LineRenderer usage, valid launch, off-center opposite steering, weak/forward pull behavior, and scene wiring.

## Current Authored Tuning

- Touch Target Radius Pixels: `80`
- Minimum Pull Distance: `0.25`
- Maximum Pull Distance: `3`
- Maximum Lateral Pull: `1.5`
- Maximum Launch Angle Degrees: `35`
- Minimum Launch Speed: `4`
- Maximum Launch Speed: `12`
- Launch Up Speed: `1.5`
- Band Contact Padding: `0.05`
- Band Wrap Sample Count: `12`
- Band Recoil Duration: `0.18`
- Launch Speed Curve: linear `0..1`
- Band Recoil Curve: linear `0..1`

## Human QA Checklist

- [ ] Launch Target feels directly pulled during Active Pull.
- [ ] Band Contact Points and Band Wrap do not visibly cut through the target mesh in normal play.
- [ ] Off-center Pulls steer in the expected opposite direction.
- [ ] Band Release Recoil reads as the Band pushing the Launch Target forward before detaching.
- [ ] Band Release Recoil does not make the target feel tethered after rest.
- [ ] Designer-facing config fields are understandable and practical to tune.
- [ ] Slingshot gizmos are useful enough to inspect anchors, Pull limits, and target-coupled Band behavior.
- [x] Editor mouse simulation remains usable for local iteration.
- [x] Physical touch-device validation was not run because no touch device is accessible from the current Codex automation environment.
- [ ] Accepted tuning values and follow-up issues are recorded after human/device QA.

## Human QA Notes

- Pending human tester.
- No tuning values are accepted yet; values above are current authored defaults.
- No follow-up issues are recorded yet.
