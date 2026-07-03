## Parent

[Finish Line Presentation And Celebration PRD](../../prd/prd-finish-line-presentation-and-celebration.md)

## What to build

Reorganize the final approach so **Finish Presentation** is easy to find and cannot accidentally behave like gameplay contact. The finish billboard should remain a strong destination sign, but it should be visual-only. The course-owned **Run Finish** should remain the single authoritative finish contact, and any legacy or duplicate finish contact should be removed or disabled unless implementation finds a concrete compatibility dependency.

This slice should preserve useful visual reuse while separating roles: **Finish Presentation** for readable dressing, **Run Finish** for successful completion, and **Run Obstacle** only for intentional physical contact behavior.

## Acceptance criteria

- [ ] The finish billboard and related visual dressing are grouped under a dedicated **Finish Presentation** hierarchy.
- [ ] The billboard no longer carries **Run Obstacle** contact behavior, **RunContact**, or a blocking collider as part of the presentation object.
- [ ] The final approach has one authoritative course-owned **Run Finish** contact.
- [ ] Any legacy scene-level duplicate **Run Finish** is removed or disabled, unless a documented compatibility dependency is found.
- [ ] Any still-needed physical blocker/backstop near the finish is authored as a separate clearly named **Run Obstacle**.
- [ ] Existing **RunContactClassifier** and **Run End Flow** result-priority behavior remain unchanged.

## Verification

- EditMode tests:
  - Existing contact classification and run-end priority tests still prove **Run Finish** maps to `Finished`.
- PlayMode tests:
  - Scene assertion proves the finish approach has exactly one authoritative course-owned **Run Finish** contact.
  - Scene assertion proves the billboard presentation object has no **RunContact** obstacle behavior.
  - Scene assertion proves any remaining physical finish-area blocker is separately named/authored as **Run Obstacle**.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Inspect the final approach hierarchy and confirm **Finish Presentation** is discoverable and distinct from **Run Finish**.
- Package version/changelog:
  - No package manifest, package version, or changelog update expected.

## Blocked by

None - can start immediately
