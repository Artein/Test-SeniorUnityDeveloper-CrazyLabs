## Parent

[Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

## What to build

Add explicit presentation anchors to the existing Launch Target composition without changing the player physics authority. The Launch Target should have separate responsibilities for Band Center, gameplay collider ownership, and Character visual mounting.

This slice should make the hierarchy ready for a future Ladybug Character child while proving that the existing slingshot, band, contact, and camera contracts still point at the correct gameplay surfaces.

## Acceptance criteria

- [ ] The Launch Target keeps its existing authoritative Rigidbody and gameplay movement components.
- [ ] Band Center remains a dedicated slingshot/band reference separate from collider and character visual roots.
- [ ] A dedicated LaunchTargetColliderRoot owns the gameplay collider or collider silhouette role.
- [ ] A dedicated CharacterVisualAnchor exists as the only intended mount point for Character presentation.
- [ ] Existing slingshot, band shape, steering, contact, and run camera references continue to target gameplay roles, not future character visuals.
- [ ] The hierarchy change does not add Ladybug assets, Animator, Character presentation code, or imported physics components.
- [ ] Existing visual placeholder behavior remains acceptable until the Ladybug Character prefab is composed in a later slice.

## Verification

- EditMode tests:
  - Existing composition or lifetime-scope tests updated only if serialized references require coverage.
- PlayMode tests:
  - Gameplay Scene composition test verifies authoritative Rigidbody, Band Center, LaunchTargetColliderRoot, CharacterVisualAnchor, and gameplay collider ownership.
  - Existing band visibility/natural band shape tests remain green.
  - Existing contact and camera scene smoke tests remain green where they already exist.
- Static checks:
  - `git diff --check`.
  - Unity compile through the connector before tests.
- Manual Unity smoke check:
  - Open Gameplay Scene and inspect the Launch Target hierarchy for clear role separation.
  - Pull and release the current placeholder target to confirm band alignment and launch readability did not obviously regress.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
