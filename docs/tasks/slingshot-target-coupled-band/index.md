# Slingshot Target-Coupled Band Implementation Issues

Parent PRD: [Slingshot Target-Coupled Band PRD](../../prd/prd-slingshot-target-coupled-band.md)

These local implementation issues are ordered by dependency. They are follow-up tracer-bullet slices for the existing Slingshot touch-launch feature, focused on making the Launch Target move with the Pull, making the Band Shape collider-aligned, and adding post-shot Band Release Recoil.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Add Held Target Pull Positioning And Launch Handoff](01-add-held-target-pull-positioning-and-launch-handoff.md) | AFK | None | 1, 4-6, 18, 20-27, 38-40 |
| 02 | [Migrate Band Shape To Ordered Polyline](02-migrate-band-shape-to-ordered-polyline.md) | AFK | 01 | 2, 16-17, 28, 41 |
| 03 | [Add Single-Collider Band Contact Provider](03-add-single-collider-band-contact-provider.md) | AFK | 01, 02 | 2, 11, 13-14, 17, 19-20, 30-31, 35-37, 40 |
| 04 | [Add Arc-Based Band Wrap Visuals](04-add-arc-based-band-wrap-visuals.md) | AFK | 03 | 3, 9-11, 29, 32-37, 43 |
| 05 | [Add Post-Shot Band Release Recoil](05-add-post-shot-band-release-recoil.md) | AFK | 01, 04 | 6-8, 12 |
| 06 | [Wire GameplayScene Target-Coupled Band](06-wire-gameplayscene-target-coupled-band.md) | AFK | 01-05 | 1-15, 28, 30-31, 39-41 |
| 07 | [Run Human Feel And Authoring QA](07-run-human-feel-and-authoring-qa.md) | HITL | 06 | 9-15, 40-41 |

QA report: [Human Feel And Authoring QA Report](07-human-feel-and-authoring-qa-report.md)

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep the original Slingshot touch-launch task set unchanged; this is a follow-up task set.
- Keep MonoBehaviours shallow and register existing scene views/adapters through composition.
- Run Unity compile before tests on implementation slices.
- Prefer EditMode tests for pure controller/service geometry behavior and PlayMode only for Rigidbody, Collider, LineRenderer, and scene-boundary behavior.
- Existing tests that assert a three-point Band Shape are stale for this follow-up and should be migrated by the relevant slices.
