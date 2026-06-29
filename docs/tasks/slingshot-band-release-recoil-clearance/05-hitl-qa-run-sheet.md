# HITL QA Run Sheet

Parent issue: [Run Human Recoil Feel And Authoring QA](05-run-human-recoil-feel-and-authoring-qa.md)

Use this sheet for the manual Unity smoke pass after the implementation slices are green. Leave items unchecked until a human tester verifies them from the player camera.

## Preflight

- [ ] Unity compile is clean.
- [ ] Targeted Slingshot EditMode implementation tests are green.
- [ ] Targeted natural Band Shape PlayMode regression suite is green.
- [ ] Test is run against the current uncommitted slingshot-band-release-recoil-clearance worktree.

## Scenarios

- [ ] Shallow valid Pull Release just above `MinimumPullDistance`: no visible Band pass-through from the player camera.
- [ ] Deeper Pull Release: Band still reads as pushing the Launch Target and returning naturally.
- [ ] Left lateral launch: steering feel is preserved and no Band clipping is visible.
- [ ] Right lateral launch: steering feel is preserved and no Band clipping is visible.
- [ ] Detach timing: Band detaches after the Launch Target clears the rest/idle/default path and does not appear tethered during Run.
- [ ] Tuning sanity: existing Band Recoil Duration and Band Contact Padding still feel usable without a broad balance pass.

## Result

- Tester:
- Unity scene:
- Pass/Fail:
- Follow-up tuning or authoring concerns:

