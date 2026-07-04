## Parent

[Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

## What to build

Compose the project-owned Ladybug Character prefab and Animator Controller from the moved third-party Ladybug source assets. The prefab should be visual-only, stable at the root, and ready to mount under Character Visual Anchor without changing Launch Target physics.

This slice should prove that the Ladybug Character can receive Character Presentation Frames and play the first-slice Idle, Slide, reserved Run compatibility, Airborne, Victory, and Defeat states with the agreed Animator parameters.

## Acceptance criteria

- [ ] A project-owned Ladybug Character prefab exists and is not a raw vendor prefab instance used directly as gameplay composition.
- [ ] The prefab root has the Character Presentation View and Animator surface needed by the presenter/view contract.
- [ ] The prefab contains an internal Model Root for imported mesh/avatar/skeleton offset, rotation, and scale quirks.
- [ ] The Animator Controller has one full-body base layer for the first slice.
- [ ] The Animator Controller has `PresentationMode` integer and `PlaybackSpeedMultiplier` float parameters.
- [ ] Animator states exist for Idle, Slide, reserved Run compatibility, Airborne, Victory, and Defeat using the agreed first-slice Ladybug clips.
- [ ] Slide is the authored default active locomotion state for meaningful grounded movement.
- [ ] Authored transition clips are used where practical; no gameplay code depends on concrete state names.
- [ ] Animator root motion is disabled or guarded.
- [ ] Imported Rigidbody, Collider, Joint, CharacterController, ragdoll, hitbox, and trigger components are absent or disabled on the character prefab.
- [ ] Prefab-owned tuning values are seeded, including Slide reference speed, meaningful movement threshold, and playback clamps.
- [ ] The prefab does not include character abilities, yoyo presentation, lateral lean, launch one-shots, landing mode, full VFX pass, or full audio pass.

## Verification

- EditMode tests:
  - Serialized asset validation test if the project has a suitable provider pattern for the Ladybug Character prefab/controller.
  - Tuning default tests verify seeded reference speeds and clamps are valid.
- PlayMode tests:
  - Applying Character Presentation Frames to the Ladybug Character prefab updates Animator parameters.
  - Prefab composition test verifies root motion guard and absence/disabled state of imported physics-like components.
  - Animator parameter consistency test verifies serialized view parameter fields match controller parameters.
- Static checks:
  - `git diff --check`.
  - Unity compile through the connector before tests.
  - AssetDatabase refresh/reimport if imported assets or controller/prefab serialization changed.
- Manual Unity smoke check:
  - Open the Ladybug Character prefab and preview Idle, Slide, reserved Run compatibility, Airborne, Victory, and Defeat states.
  - Confirm rough visual orientation and scale are plausible before scene wiring.
- Package version/changelog:
  - No package manifest update expected if SaintsField is already installed.
  - No changelog update expected.

## Blocked by

- 01 Move And Catalog Ladybug Source Assets
- 06 Add Passive Character Presentation View
