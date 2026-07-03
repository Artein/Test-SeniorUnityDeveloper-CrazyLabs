## Parent

[Finish Line Presentation And Celebration PRD](../../prd/prd-finish-line-presentation-and-celebration.md)

## What to build

Add **Victory Facing** so a successful **Victory** presents the visible **Character** toward the **Run Camera** instead of leaving the final view stuck behind the character. The turn should blend with the beginning of **Victory** presentation and should affect only presentation transforms such as the **Character** or **Character Visual Anchor**.

This slice must preserve gameplay truth. It must not rotate the **Run Body**, Rigidbody, colliders, camera source, progress source, surface probes, or **Run End Pose Lock** target, and it must reset when returning to **Run Preparation**.

## Acceptance criteria

- [ ] Accepted successful **Run Result** activates **Victory Facing** during **Victory** presentation.
- [ ] Failed accepted results keep **Defeat** behavior and do not activate **Victory Facing**.
- [ ] The visible **Character** or **Character Visual Anchor** blends toward the **Run Camera** direction instead of snapping as an isolated correction.
- [ ] **Victory Facing** does not mutate **Run Body**, Rigidbody, collider, camera source, progress source, surface probe, or **Run End Pose Lock** transforms.
- [ ] Returning to **Run Preparation** clears **Victory Facing** so normal **Character Visual Follower** orientation owns the next run.
- [ ] The implementation integrates with existing **Character Presenter** / **Character Visual Follower** ownership instead of creating a competing pose loop.

## Verification

- EditMode tests:
  - Character presentation test proves successful accepted result enables **Victory Facing** output.
  - Character presentation test proves failed accepted result does not enable **Victory Facing**.
  - Character presentation test proves **Run Preparation** clears **Victory Facing** state.
  - Character presentation test proves the facing output is presentation-only and does not require mutating gameplay transforms.
- PlayMode tests:
  - Scene/composition test proves character presentation still resolves its view, camera source, and presenter dependencies.
  - If practical, a scene smoke test proves the visible character rotates toward the camera while gameplay body orientation remains unchanged.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Complete a successful run and confirm the visible character turns toward the run camera during the start of **Victory**.
  - Return to **Run Preparation** and confirm the next run starts with normal character-follow orientation.
- Package version/changelog:
  - No package manifest, package version, or changelog update expected.

## Blocked by

None - can start immediately
