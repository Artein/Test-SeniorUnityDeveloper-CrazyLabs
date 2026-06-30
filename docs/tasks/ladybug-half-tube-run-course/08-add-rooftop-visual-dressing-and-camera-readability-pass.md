## Parent

[Ladybug Half-Tube Run Course PRD](../../prd/prd-ladybug-half-tube-run-course.md)

## What to build

Add Ladybug-themed Paris rooftop visual dressing to the full static graybox while preserving gameplay collision clarity, **Run Camera** readability, and project-owned gameplay wrappers. This slice should use rooftop chunks and obstacle meshes as theme and silhouette assets without making imported meshes the authoritative **Run Surface** or unfair **Run Obstacle** collision source.

This is HITL because visual density, obstacle silhouette readability, camera framing, material fit, and theme acceptance require human review in Unity.

## Acceptance criteria

- [ ] The course reads as a Ladybug-themed Paris rooftop gutter/chute rather than a generic prototype strip.
- [ ] Imported rooftop chunks and props are used as visual dressing around the project-owned half-tube where appropriate.
- [ ] Rooftop obstacle visuals match gameplay blockers without adding unfair collision detail.
- [ ] Imported visual meshes do not replace project-owned **Run Surface** geometry as the gameplay source of truth.
- [ ] Visual-only props outside the playable line do not introduce unintended **Run Contact Categories**.
- [ ] Prop density does not hide safe lines, coin lines, ramp approaches, ramp landings, side lips, or **Run Finish**.
- [ ] The **Run Camera** keeps obstacles, ramps, coins, and **Run Finish** readable across representative Band 1-Band 5 sections.
- [ ] Camera obstacle/terrain layer authoring remains intentional and does not treat every decorative collider as a camera obstacle.
- [ ] The final finish marker is visually distinct.
- [ ] Moving/rotating obstacle gameplay remains deferred; rotating-gate assets, if used, are static dressing only in this slice.
- [ ] Human reviewer accepts the visual theme fit and readability pass before final progression tuning.

## Verification

- EditMode tests:
  - None expected unless a validation module checks visual-only vs gameplay-contact authoring rules.
- PlayMode tests:
  - Gameplay Scene composition verifies decorative props do not introduce unsupported or unintended **Run Contact Categories**.
  - Camera layer/composition checks remain green after visual dressing.
  - Existing collision/contact tests remain green.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector after any code, scene, prefab, or test change.
- Manual Unity smoke check:
  - HITL visual review across all five **Run Progression Bands** for theme fit, obstacle silhouette clarity, coin readability, ramp readability, finish visibility, camera occlusion, and prop/collider confusion.
  - Use camera captures when needed to inspect framing and occlusion.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 06 - Add Band 5 Finish Approach And Run Finish Slice
