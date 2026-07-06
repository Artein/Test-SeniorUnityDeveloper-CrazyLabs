## Parent

[Character Collider Split And Animated Contact Sensors PRD](../../prd/prd-character-collider-and-animated-contact-sensors.md)

## What to build

Add **Animated Contact Sensor Pose Sync** to copy finalized **Character** body-part poses into configured sensor transforms under **Animated Contact Sensor Physics Root**. The sensor root should be a gameplay-owned physics hierarchy with one kinematic Rigidbody and trigger-only sensor colliders, not a child of **Character Visual Anchor** and not a child of the **Run Body** Rigidbody root.

The sync should run after **Character Visual Follower** in the presentation late-update phase so sensors follow the rendered-frame body-part pose while physics trigger delivery happens on the next Unity physics step.

## Acceptance criteria

- [ ] **Animated Contact Sensor Physics Root** is a gameplay-owned root with one kinematic Rigidbody.
- [ ] Copied sensor transforms live under **Animated Contact Sensor Physics Root** and are not under **Character Visual Anchor**.
- [ ] Each mapping explicitly pairs a source **Character** body-part Transform with a copied sensor Transform.
- [ ] Pose sync copies finalized body-part position and rotation into the copied sensor transform.
- [ ] Pose sync runs after **Character Visual Follower** through explicit VContainer entry-point ordering.
- [ ] Disabled or empty sensor mapping sets are valid no-ops.
- [ ] Validation rejects missing source transforms, missing sensor transforms, duplicate sensor targets, or unsupported hierarchy wiring.
- [ ] The implementation does not call `Physics.SyncTransforms` every rendered frame by default.
- [ ] Animator update-mode assumptions are documented as first-pass normal presentation timing, with `Animate Physics` left for an explicit future policy if needed.

## Verification

- EditMode tests:
  - Pose sync copies source transform position and rotation into the configured sensor transform.
  - Empty or disabled mapping sets no-op without errors.
  - Validation rejects null mappings and duplicate sensor targets.
  - Entry-point registration orders pose sync after **Character Visual Follower**.
- PlayMode tests:
  - Deferred to the trigger-delivery integration slice unless Unity transform/physics timing must be proven here.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector before tests.
  - Source search confirms no default per-frame `Physics.SyncTransforms` call was added.
- Manual Unity smoke check:
  - Inspect copied sensor transforms in Play Mode and confirm they follow the intended animated body parts after the visual follower updates.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Approve Collider Authority ADR And Migration Boundary
