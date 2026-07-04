## Parent

[Character Visual Follower Presentation Smoothing PRD](../../prd/prd-character-visual-follower-presentation-smoothing.md)

## What to build

Establish the **Character Visual Follower** as an end-to-end presentation seam without smoothing behavior yet.

The implementation should add the passive view/tuning surface needed by **CharacterPresentationView**, register a VContainer-owned plain C# follower entry point, sample the **Launch Target** Transform render pose during the presentation late-update phase, and write the **Character Visual Anchor** world pose. In this first slice the output should directly snap to the target pose so the behavior is easy to reason about before smoothing is introduced.

This slice proves ownership and data direction: physical truth stays on the Rigidbody-backed **Launch Target**, the visible character is presentation-only, and no gameplay system reads the smoothed visual pose.

## Acceptance criteria

- [ ] **CharacterPresentationView** exposes an internal visual-follow view/tuning seam without exposing a new public gameplay API.
- [ ] The visual-follow seam provides the **Character Visual Anchor** Transform and serialized tuning values needed by later slices.
- [ ] The initial serialized defaults match the PRD baseline: position response `60`, heading response `45`, up/tilt response `18`, max position lag `0.06`, snap distance `0.75`, and snap angle `45`.
- [ ] A VContainer-owned plain C# **Character Visual Follower** entry point is registered in Gameplay composition.
- [ ] The follower samples the **Launch Target** Transform render pose in the presentation late-update phase, not raw Rigidbody state in fixed update.
- [ ] The follower writes only the **Character Visual Anchor** world position and rotation.
- [ ] In this slice, direct-follow output exactly snaps the visual anchor to the sampled launch target pose each late tick.
- [ ] The follower snaps/reinitializes on startup and on relevant run-state boundaries: run preparation, pre-launch, launch applied, and run ended.
- [ ] **CharacterPresenter** remains the only owner of animation mode/frame application; the follower does not set Animator parameters.
- [ ] The camera, run progress, run end flow, steering, support probing, Rigidbody movement, colliders, and physics materials remain unchanged.
- [ ] Existing visual-only constraints remain true: no Collider, Rigidbody, Joint, or CharacterController is required under the visible character hierarchy.

## Verification

- EditMode tests:
  - Direct-follow controller copies a fake target pose to a fake visual anchor in late tick.
  - The follower samples the target through the configured render-pose source, not through Rigidbody velocity or fixed-tick state.
  - Initialization and run-state boundary events force an immediate snap.
  - Disposing/unsubscribing the follower stops state-boundary callbacks from moving the visual anchor.
  - Character presentation tuning exposes the agreed default values defensively.
- PlayMode tests:
  - Gameplay composition resolves one follower using the scene-authored presentation view, launch target, and visual anchor.
  - Gameplay scene composition still keeps the visible character free of physical gameplay components.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Source search confirms no gameplay system reads the **Character Visual Anchor** as physical truth.
- Manual Unity smoke check:
  - Start a run and confirm the snap-only follower does not visibly change gameplay, camera, launch, or animation behavior.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
