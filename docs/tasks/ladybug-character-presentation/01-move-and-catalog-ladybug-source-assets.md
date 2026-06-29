## Parent

[Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

## What to build

Move the imported Ladybug source content into the third-party plugin bucket and leave the project with a clear catalog of which imported Ladybug assets are intended for first-slice Character presentation. The move should preserve Unity meta GUIDs so imported clips, avatar references, materials, textures, sounds, VFX, and future prefab composition keep stable references.

This slice should make asset ownership obvious without wiring Ladybug into gameplay yet. It should also record the first-slice animation mapping: Idle, Slide, Run, Airborne, Victory, and Defeat.

## Acceptance criteria

- [ ] Imported Ladybug source content is grouped under the third-party plugin bucket and no duplicate source bucket remains in the old location.
- [ ] Unity `.meta` files are preserved as renames/moves rather than regenerated replacements.
- [ ] The imported T-pose Ladybug model/avatar source remains available and is documented as the first-slice character basis.
- [ ] First-slice clips are cataloged for Idle, Slide, Run, Airborne, Victory, and Defeat.
- [ ] Deferred imported content is cataloged separately, including yoyo, transition wipe, slide center rotation, VFX, and audio candidates.
- [ ] No gameplay scene, runtime Character presentation code, Animator Controller, or project-owned Ladybug prefab is introduced in this slice.
- [ ] The asset move does not require save data, Addressables, Unity version, package manifest, or signing changes.

## Verification

- EditMode tests:
  - None expected unless an existing asset-reference validation helper already exists.
- PlayMode tests:
  - None expected.
- Static checks:
  - `git diff --check`.
  - Confirm moved Ladybug assets are represented as renames where possible and `.meta` files are not recreated with new GUIDs.
  - Search for stale references to the old Ladybug source bucket.
  - Unity compile through the connector after AssetDatabase refresh/reimport settles.
- Manual Unity smoke check:
  - Open the moved Ladybug source assets in the Project window and confirm primary model, clips, VFX, and audio are visible without missing-script or broken-reference warnings.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
