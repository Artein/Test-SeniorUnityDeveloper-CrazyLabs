## Parent

[Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

Supersession note: [Slide-Only Character Presentation](../slide-only-character-presentation/index.md) supersedes acceptance wording in this slice that treats flat grounded movement as normal visible **Run**. **Run** remains reserved compatibility.

## What to build

Wire the generic Character presentation system and the project-owned Ladybug Character prefab into Gameplay Scene. The Launch Target should continue to be the only gameplay physics authority while Character presentation follows it through Idle, Slide, reserved Run compatibility, Airborne, Victory, and Defeat presentation modes.

This slice should make the game playable with Ladybug presentation end-to-end before collider fit and final visual calibration are treated as HITL tasks.

## Acceptance criteria

- [ ] Gameplay composition registers Character Presentation Presenter and its required contracts through the existing lifetime-scope pattern.
- [ ] Gameplay composition registers or exposes the passive Character Presentation View and tuning without injecting concrete MonoBehaviours into domain logic.
- [ ] The Ladybug Character prefab is mounted under Character Visual Anchor.
- [ ] Launch Target Rigidbody, Band Center, LaunchTargetColliderRoot, gameplay collider, slingshot, steering, run progress, run contacts, and run camera remain authoritative.
- [ ] Pre-launch/slingshot hold maps to Idle presentation.
- [ ] Active grounded downhill movement maps to Slide presentation.
- [ ] Active grounded flat forward movement maps to Slide presentation when movement is meaningful.
- [ ] Sustained ungrounded movement maps to Airborne presentation after debounce.
- [ ] Accepted successful run result maps to Victory presentation.
- [ ] Accepted failed run result maps to Defeat presentation.
- [ ] Character presentation does not add root-motion movement or imported physics behavior to the Launch Target.
- [ ] Existing slingshot, band, steering, run-end, contact, and camera tests remain green except where later collider tuning intentionally changes gameplay-contact behavior.

## Verification

- EditMode tests:
  - Lifetime-scope/composition tests verify presenter dependencies can be resolved and required serialized references are validated.
  - Presenter tests from earlier slices remain green with real registration assumptions.
- PlayMode tests:
  - Gameplay Scene composition test verifies Ladybug Character is mounted under Character Visual Anchor.
  - Scene test verifies one authoritative Rigidbody and one gameplay collider ownership path.
  - Scene test verifies visual anchor changes do not move Band Center, LaunchTargetColliderRoot, Rigidbody root, or camera anchor.
  - Scenario smoke tests cover pre-launch Idle, active Slide for meaningful grounded movement, reserved Run compatibility where practical, Airborne after debounce, Victory, and Defeat presentation signals.
  - Existing band/contact/camera PlayMode tests remain green.
- Static checks:
  - `git diff --check`.
  - Unity compile through the connector before tests.
  - AssetDatabase refresh/reimport if scene or prefab serialization changed.
- Manual Unity smoke check:
  - Play the Gameplay Scene and confirm the player visually appears as Ladybug while slingshot pull, launch, downhill motion, and run end remain usable.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 Add Launch Target Presentation Anchors
- 07 Add Character Presentation Presenter
- 08 Compose Ladybug Character Prefab And Controller
