# Natural Band Human Feel And Authoring QA Report

Parent issue: [Run Natural Band Human Feel And Authoring QA](06-run-natural-band-human-feel-and-authoring-qa.md)

## Status

Implementation is verified by automated compile and test evidence, including a recovered live Unity Editor focused run. Human Play Mode feel validation, designer authoring sign-off, and physical touch-device validation are still pending.

## Automated Evidence

- Unity compile succeeded via `.unity-ai-agent-connector/bin/uaiac compile --route batch`, with connector warning `editor_session_degraded`.
- Targeted recoil regression run `r_ryyanqgm`: 22 total, 22 passed, 0 failed.
- Focused natural Band Shape run `r_2q59zqjq`: 80 total, 80 passed, 0 failed.
- Recovered live Unity Editor focused run `r_fq6xvudn`: 80 total, 80 passed, 0 failed, 0 warnings; saved result reports clean post-test barrier and graphics capability available.
- PlayMode silhouette source test rename verification run `r_zb85k209`: 3 total, 3 passed, 0 failed, 0 warnings.
- Previous targeted selector run `r_ei1hyk7q`: 78 total, 78 passed, 0 failed.
- Rider reformat and file problem checks passed for `Assets/Game/Gameplay/Slingshot/SlingshotView.cs`, `Assets/Game/Gameplay/Slingshot/SlingshotController.BandShape.cs`, and `Assets/Game/Gameplay/Slingshot/Tests/EditMode/SlingshotControllerTests.cs` after the final natural Band Shape updates.
- Rider reformat and file problem checks passed for `Assets/Game/Gameplay/Slingshot/Tests/PlayMode/LaunchTargetSilhouetteSourceTests.cs` after the PlayMode test asset rename.
- `git diff --check` passed after the latest implementation, task-doc, continuity, and workflow-artifact updates.
- Gameplay Scene PlayMode smoke coverage verifies scene composition, editor-mouse launch flow, weak Pull cancel, forward Pull clamp/no-launch, lateral steering, controller integration, Active Pull Band clearance, and first Band Release Recoil frame Band clearance against the Launch Target Collider.
- Slingshot EditMode coverage verifies the deterministic Pull Plane taut solver, Band Shape provider, controller state transitions, release behavior, and launch request behavior.
- Slingshot PlayMode coverage verifies collider silhouette source and Rigidbody launch target behavior.

## Live Automation Check

- Unity Editor was reopened for `Assets/Scenes/GameplayScene.unity` automation context; `.unity-ai-agent-connector/bin/uaiac status` reports `editor_session.status = live_gui_rpc`, `editor_session.state = ready`, and Unity `6000.3.18f1`.
- Entering Play Mode through `.unity-ai-agent-connector/bin/uaiac play-mode enter` succeeded during the live check.
- Running `tests list` while Play Mode was active was correctly rejected as `play_mode_active`; after returning to Edit Mode, live selector listing succeeded.
- Live focused selector run `r_fq6xvudn` passed 80/80 tests with no warnings. This verifies automated scene mechanics, not human readability or tactile feel.
- Manual Gameplay Scene feel checks remain pending because they require a person to judge visual readability, recoil feel, and designer tuning clarity.

## Current Authored Tuning

Source: `Assets/Game/Gameplay/SlingshotConfig.asset`.

- Touch Target Radius Pixels: `80`
- Minimum Pull Distance: `0.25`
- Maximum Pull Distance: `3`
- Maximum Lateral Pull: `1.5`
- Maximum Launch Angle Degrees: `35`
- Minimum Launch Speed: `4`
- Maximum Launch Speed: `12`
- Launch Up Speed: `1.5`
- Band Contact Padding: `0.05`
- Band Silhouette Sample Count: `32`
- Band Wrap Sample Count: `13`
- Band Recoil Duration: `0.18`
- Launch Speed Curve: linear `0..1`
- Band Recoil Curve: linear `0..1`

## Manual QA Procedure

1. Open `Assets/Scenes/GameplayScene.unity`.
2. Enter Play Mode with the Game view visible.
3. Run the Human QA Checklist below with editor mouse input first.
4. For each Pull scenario, watch the Band during Active Pull, release, and the first post-shot Band Release Recoil frames.
5. Select the Slingshot object in the Scene view and inspect selected-object gizmos against the authored anchors, Pull limits, silhouette samples, wrap samples, and contact padding.
6. Inspect `Assets/Game/Gameplay/SlingshotConfig.asset` and confirm the natural Band Shape tuning fields are understandable enough to adjust without code changes.
7. Record pass/fail notes in Human QA Notes. If any scenario fails, add a follow-up finding with the expected behavior, observed behavior, reproduction gesture, and whether it is a bug or tuning task.
8. Repeat the Touch Device Checklist on a touch-capable target when available.

Pass/fail rule: do not mark an acceptance criterion as passed from automated evidence alone if the criterion depends on visual readability, physical feel, tactile comfort, or designer clarity.

## Human QA Checklist

Run in `Assets/Scenes/GameplayScene.unity` with editor mouse first.

- [ ] Center Pull visually stretches the Band around the target and launches forward on release.
- [ ] Left-corner Pull wraps around the pulled side of the Launch Target Silhouette instead of flipping to the forward/short side.
- [ ] Right-corner Pull wraps around the pulled side of the Launch Target Silhouette instead of flipping to the forward/short side.
- [ ] Lateral Pull steering launches opposite the lateral pull direction.
- [ ] Weak Pulls cancel naturally and return to rest without launch.
- [ ] Forward-only Pull movement is clamped and does not create launch energy.
- [ ] Band does not visibly cut through the Launch Target mesh during Active Pull.
- [ ] Band does not visibly cut through the Launch Target mesh during post-shot Band Release Recoil.
- [ ] Band Release Recoil reads as the Band pushing the Launch Target before returning to rest.
- [ ] Pull Hint, Touch Indicator, Band Shape, and selected-object gizmos are readable in the Gameplay Scene.
- [ ] Slingshot config tuning fields are understandable enough for designer iteration.
- [ ] Console remains free of missing-reference errors, assertion failures, and unexpected warnings during the pass.

## Touch Device Checklist

Run on a touch-capable target when available.

- [ ] Finger-sized Band capture feels reliable and not surprising.
- [ ] First-touch-only behavior remains clear while additional touches are ignored.
- [ ] Touch cancel/release returns Band, Pull Hint, Touch Indicator, and target pose to expected states.

## Human QA Notes

- Pending human tester.
- No tuning values are accepted yet; values above are current authored defaults.
- No follow-up bugs or tuning tasks are recorded yet because human feel QA has not been run.

## Follow-Up Findings

- None recorded from automated evidence.
- Human feel acceptance remains pending because readability, physical feel, and designer-facing tuning clarity require a person to judge the Gameplay Scene in Play Mode.
- Touch-device validation remains pending until a device is available.
