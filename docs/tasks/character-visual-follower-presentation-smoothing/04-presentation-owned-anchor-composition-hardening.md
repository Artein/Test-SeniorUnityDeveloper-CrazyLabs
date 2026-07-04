## Parent

[Character Visual Follower Presentation Smoothing PRD](../../prd/prd-character-visual-follower-presentation-smoothing.md)

## What to build

Harden GameplayScene composition so the **Character Visual Anchor** is clearly presentation-owned and not accidentally treated as part of gameplay physics truth.

The implementation should update scene authoring and composition assertions around the visual hierarchy. The **Character Visual Anchor** may still be physically located near or under the launch target for authoring convenience, but tests should not require it to be a child of **Launch Target**. The follower owns the anchor's world pose; gameplay systems continue to use the Rigidbody-backed **Launch Target** directly.

This slice should make the composition contract explicit before the feature goes into manual feel review.

## Acceptance criteria

- [ ] GameplayScene authoring supports the **Character Visual Anchor** as a presentation-space role, not a required child of **Launch Target**.
- [ ] Scene composition tests no longer assert `CharacterVisualAnchor.transform.IsChildOf(LaunchTarget.transform)` as a requirement.
- [ ] Scene composition tests still assert the visible character/model hierarchy is under the **Character Visual Anchor** as presentation output.
- [ ] Scene composition tests still assert no Collider, Rigidbody, Joint, or CharacterController exists under the visible character/model presentation hierarchy.
- [ ] Composition tests verify the scene-authored **CharacterPresentationView** exposes the same visual anchor and tuning consumed by the follower.
- [ ] Composition tests verify the agreed serialized tuning defaults remain authored in GameplayScene.
- [ ] VContainer composition resolves one shared follower entry point with the expected launch target, visual anchor, tuning, and presentation lifecycle dependencies.
- [ ] No gameplay system uses **Character Visual Anchor** as a camera follow target, physics body, support probe origin, progress sample source, run-end input, slingshot target, or steering source.
- [ ] Existing launch, sliding, run-end, camera, and animation presentation behavior remains functionally unchanged except for visual smoothing.

## Verification

- EditMode tests:
  - Not expected unless composition is factored behind pure helper code.
- PlayMode tests:
  - GameplayScene composition passes with the presentation-owned anchor contract.
  - Visual character/model remains under **Character Visual Anchor**.
  - Visible hierarchy contains no physical gameplay components.
  - Scene-authored tuning values match the expected defaults.
  - Follower dependencies resolve through GameplayLifetimeScope without duplicate follower instances.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Source search confirms gameplay systems still reference **Launch Target** or their existing data sources, not **Character Visual Anchor**.
- Manual Unity smoke check:
  - Start a run in GameplayScene and confirm launch, camera follow, sliding, animation, run-end, and reset still work after hierarchy/composition changes.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

[03 - Soft Up-Tilt Smoothing And Safe Pose Composition](03-soft-up-tilt-smoothing-and-safe-pose-composition.md)
