## Parent

[Finish Line Presentation And Celebration PRD](../../prd/prd-finish-line-presentation-and-celebration.md)

## What to build

Play **Finish Celebration** as a one-shot presentation reward only after **Run End Flow** accepts a successful **Run Result**. The celebration should use existing Ladybug confetti assets first, drive a passive scene-authored finish presentation view, and fade the **Finish Threshold Visual** out during the celebration.

This slice should keep contact classification and **Run End Flow** free of particle playback. The presenter/listener should subscribe to the accepted-result boundary already exposed by the run-end flow and should reset cleanly for **Run Preparation**.

## Acceptance criteria

- [ ] **Finish Celebration** starts only after an accepted successful **Run Result**.
- [ ] Failed accepted results, obstacle hits, out-of-bounds, lost momentum, and raw contact entry do not start celebration.
- [ ] Duplicate accepted success notifications do not spawn duplicate one-shot celebration playback for the same terminal run.
- [ ] The scene-authored celebration view can play, stop/clear, fade out the threshold, and reset for **Run Preparation**.
- [ ] Existing Ladybug confetti assets are used as the first-pass VFX source.
- [ ] Finish presentation view references are registered through VContainer without making MonoBehaviours own controller lifetimes.

## Verification

- EditMode tests:
  - Presenter/controller test proves accepted successful result starts celebration.
  - Presenter/controller test proves failed accepted result does not start celebration.
  - Presenter/controller test proves raw contact/classification input is not required or consumed by celebration playback.
  - Presenter/controller test proves duplicate accepted success does not create duplicate one-shot playback.
  - Presenter/controller test proves **Run Preparation** reset restores threshold visibility and clears celebration state.
- PlayMode tests:
  - Scene assertion proves finish confetti references are serialized and inactive before accepted success.
  - Scene assertion proves finish presentation view references resolve through gameplay composition.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Finish a successful run and verify confetti starts after crossing and the threshold fades during celebration.
  - Trigger a failed run and verify confetti does not play.
- Package version/changelog:
  - No package manifest, package version, or changelog update expected.

## Blocked by

- 02 - Add Finish Threshold Visual
