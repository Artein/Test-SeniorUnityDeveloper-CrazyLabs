# Playtest And Tune Run Camera Feel

## Parent

[Run Camera Cinemachine PRD](../../prd/prd-run-camera-cinemachine.md) and [ADR-0009](../../adr/adr-0009-use-cinemachine-for-run-camera.md).

## Type

HITL

## User stories covered

8, 11-12, 16-18, 36, 49, 53-54, 59-60

## What to build

Run a human-in-the-loop tuning pass for the completed first **Run Camera** implementation. Tune scene-authored Cinemachine values and project-owned anchor values until **Pre-Launch** framing, launch handoff, downhill sliding, jumping, steering, slope safety, and obstacle recovery feel appropriate for the casual game.

This slice should produce concrete tuned values and a short QA note. It should also decide whether target occlusion is actually a problem worth solving with Deoccluder in a follow-up slice.

## Acceptance criteria

- [ ] **Pre-Launch Camera** framing remains stable and supports **Slingshot** **Pull** interaction.
- [ ] The blend from **Pre-Launch Camera** to **Run Camera** feels intentional and not harsh.
- [ ] **Run Camera** distance, offsets, damping, and anchor smoothing support downhill sliding readability.
- [ ] Jumps remain readable without excessive lag, snap, or physics wobble.
- [ ] Steering left/right remains readable from the camera angle.
- [ ] Slope and obstacle correction avoids internals without distracting pops in normal gameplay.
- [ ] Camera recovery after Decollider correction feels acceptable.
- [ ] A decision is recorded on whether Deoccluder is still deferred or should become a new follow-up issue.
- [ ] A short manual QA/tuning report records the tested scenarios, final values changed, and any known residual risks.

## Verification

- EditMode tests: no new EditMode tests required unless tuning exposes a controller/config bug.
- PlayMode tests: rerun targeted camera scene/composition and smoke tests after tuning.
- Static checks: Unity compile if serialized references or config assets changed; `git diff --check`.
- Manual Unity smoke check: device or editor simulation for launch handoff, sliding follow, jump follow, slope clipping, object interior clipping, anchor smoothness, and steering readability.
- Package version/changelog: no package change expected; add or update a local QA report only if that is the established repo pattern for HITL slices.

## Blocked by

- [Stabilize Velocity-Facing Run Camera Anchor](03-stabilize-velocity-facing-run-camera-anchor.md)
- [Add Camera Geometry Layers And Decollider Safety](04-add-camera-geometry-layers-and-decollider-safety.md)
