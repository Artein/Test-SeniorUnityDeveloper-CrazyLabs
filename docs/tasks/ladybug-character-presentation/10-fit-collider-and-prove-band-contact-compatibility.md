## Parent

[Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

## What to build

Tune the gameplay collider shape, size, and offset to fit the Ladybug Character presentation while proving that slingshot band wrapping, launch behavior, run contacts, and run camera assumptions remain acceptable. This is a HITL slice because collider fit requires visual judgment against the real Ladybug model and changes gameplay-contact behavior.

This slice should avoid opportunistic threshold or run-end rule changes unless tests or human review show a concrete need.

## Acceptance criteria

- [ ] Ladybug visual scale and orientation are locked well enough to tune the gameplay collider.
- [ ] LaunchTargetColliderRoot collider shape, size, and offset are adjusted to fit the Ladybug avatar more fairly than the prototype cylinder.
- [ ] Collider tuning preserves clear separation between gameplay collider, Band Center, Character Visual Anchor, and Ladybug Character Model Root.
- [ ] Slingshot band wrapping and held target alignment remain readable and do not visibly intersect the character in normal pull poses.
- [ ] Launch behavior remains physics-based and does not use root motion or imported character physics.
- [ ] Run contact and obstacle/failure behavior remain fair with the tuned collider.
- [ ] Run camera framing remains acceptable with the tuned collider and visual scale.
- [ ] Any run-end threshold changes are explicitly justified by tests or review, not bundled as incidental tuning.
- [ ] Human reviewer signs off that collider fit is good enough for the first slice.

## Verification

- EditMode tests:
  - Existing collider-independent classifier, presenter, run-end, and band solver tests remain green.
  - Any new pure collider sizing helper tests if implementation introduces one.
- PlayMode tests:
  - Gameplay Scene composition verifies one gameplay collider ownership path after tuning.
  - Band shape tests remain green with the tuned collider.
  - Contact/run-end tests remain green or are intentionally updated with documented behavior changes.
  - Camera scene smoke tests remain green.
- Static checks:
  - `git diff --check`.
  - Unity compile through the connector before tests.
  - AssetDatabase refresh/reimport if prefab or scene serialization changed.
- Manual Unity smoke check:
  - Human review of slingshot pull, launch, slide, flat run, airborne, obstacle contact, victory, defeat, and camera framing with the tuned collider.
  - Capture screenshots or a short note for any accepted visual compromises.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 02 Add Launch Target Presentation Anchors
- 08 Compose Ladybug Character Prefab And Controller
- 09 Wire Character Presentation Into GameplayScene
