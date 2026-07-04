## Parent

[Finish Line Presentation And Celebration PRD](../../prd/prd-finish-line-presentation-and-celebration.md)

## What to build

Add a visual-only **Finish Threshold Visual** aligned with the authoritative **Run Finish** so the exact crossing moment is readable at speed. The threshold should use the generated transparent checkered texture as first-pass art, read strongest near the run surface, fade upward to transparency, and look like a crossing marker rather than a physical wall.

This slice should carry the threshold all the way through asset import, material/view authoring, scene placement, reset behavior, and scene validation without granting run completion or changing contact classification.

## Acceptance criteria

- [ ] The generated checkered texture imports with alpha transparency suitable for the threshold visual.
- [ ] A threshold material/view supports runtime opacity changes for fade-out and reset.
- [ ] The threshold is aligned with the authoritative course-owned **Run Finish**.
- [ ] The threshold is most opaque near the run surface and fades upward toward full transparency.
- [ ] The threshold object has no collider, **RunContact**, reward logic, or contact classification behavior.
- [ ] Returning to **Run Preparation** restores the threshold to its ready-to-cross state.

## Verification

- EditMode tests:
  - If fade interpolation is implemented outside a MonoBehaviour view, test fade progression and reset behavior through a passive view boundary.
- PlayMode tests:
  - Scene assertion proves the **Finish Threshold Visual** has no collider and no **RunContact**.
  - Scene assertion proves the threshold visual is present, enabled before crossing, and serialized where the finish presentation view expects it.
  - Scene assertion proves the threshold is spatially aligned with the authoritative **Run Finish** within an implementation-defined tolerance.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
  - Asset import metadata is present for the generated transparent texture.
- Manual Unity smoke check:
  - Inspect the threshold in Scene/Game views and confirm there is no visible chroma-key fringe or wall-like read.
- Package version/changelog:
  - No package manifest, package version, or changelog update expected.

## Blocked by

- 01 - Separate Finish Presentation From Run Finish Contacts
