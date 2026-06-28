# Run Natural Band Human Feel And Authoring QA

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md), [Slingshot Target-Coupled Band Implementation Issues](../slingshot-target-coupled-band/index.md), and [ADR-0008](../../adr/adr-0008-use-deterministic-taut-band-shape-solver-instead-of-rope-physics.md).

## Type

HITL

## User stories covered

2-4, 7-12, 16-22, 61-63

## What to build

Run a human feel and authoring QA pass for the natural Band Shape in Gameplay Scene. The goal is to confirm the mechanic reads as a physical slingshot: the target follows the pull, the Band wraps around the Launch Target Silhouette instead of cutting through the mesh, off-center pulls steer in the opposite direction of the pull, and designer-facing tuning/gizmos are understandable.

This slice should produce a short QA report with checked scenarios, observations, and any follow-up issues if the feel is not acceptable.

QA report: [Natural Band Human Feel And Authoring QA Report](06-natural-band-human-feel-and-authoring-qa-report.md)

## Acceptance criteria

- [ ] Center Pull visually stretches the Band around the target and launches forward on release.
- [ ] Left-corner and right-corner Pulls wrap around the pulled side of the Launch Target Silhouette instead of flipping to the forward/short side.
- [ ] Lateral Pull steering launches opposite the lateral pull direction.
- [ ] Weak Pulls cancel naturally and return to rest without launch.
- [ ] Forward-only Pull movement is clamped and does not create launch energy.
- [ ] Band does not visibly cut through the Launch Target mesh during Active Pull or post-shot Band Release Recoil.
- [ ] Band Release Recoil reads as the Band pushing the Launch Target before returning to rest.
- [ ] Pull Hint, Touch Indicator, Band Shape, and selected-object gizmos are readable in the Gameplay Scene.
- [ ] Slingshot config tuning fields are understandable enough for designer iteration.
- [ ] QA report is saved alongside this issue and lists any follow-up bugs or tuning tasks.

## Verification

- EditMode tests: none required for this HITL slice beyond ensuring existing tests remain green if any tuning changes are made.
- PlayMode tests: run relevant existing PlayMode composition/scene tests if scene wiring or tuning changes are made.
- Static checks: Unity compile via Unity AI Agent Connector if any files are changed during QA.
- Manual Unity smoke check: perform center, left, right, weak, and forward-only Pulls in Gameplay Scene; inspect selected-object gizmos; capture notes in the QA report.
- Package version/changelog: no package/changelog change.

## Blocked by

- 05 - Wire GameplayScene Natural Band Shape
