# PRD: Ladybug Character Presentation

Supersession note: [Slide-Only Character Presentation](prd-slide-only-character-presentation.md) supersedes every requirement in this PRD that treats flat grounded forward movement as normal visible **Run** presentation. The architecture remains current: the Rigidbody-backed **Launch Target** is gameplay truth, and the **Character** remains presentation only.

## Problem Statement

The current player-facing Launch Target is represented by a cylinder-like gameplay object. That shape is useful for early physics and slingshot iteration, but it is not acceptable as the main character presentation for the game. The game needs the player to control a Ladybug character while preserving the existing physics-based downhill movement, slingshot launch behavior, steering, contact handling, run camera assumptions, and run-end flow.

The core product problem is separation of responsibility. The Launch Target is the authoritative gameplay body. It owns the Rigidbody, collision behavior, slingshot interaction, band center, steering, and run progress relationships. The Ladybug should not replace that authority with imported prefab physics, root motion, or vendor scripts. The Ladybug should be a presentation child that reads gameplay state and motion data, then drives animation, visual scale, VFX, and audio in a way that matches the game loop.

The game is mostly about sliding forward downhill. That means the default active grounded locomotion should be Slide, not Run. Earlier versions of this PRD allowed visible Run for flat forward sections; that behavior is now superseded by slide-only presentation, where Run remains reserved compatibility. Airborne, victory, and defeat presentation must also be supported without leaking gameplay-specific result concepts into the view.

This PRD defines the first implementation slice for a generic Character presentation system using the imported Ladybug assets as the first concrete character.

## Solution

Build a generic Character presentation layer around the existing Launch Target.

The existing physics root remains authoritative. A project-owned Ladybug Character prefab is mounted under a dedicated Character Visual Anchor. The gameplay collider is split away from the visual character hierarchy so collider tuning, band wrapping, camera tracking, and run contacts stay explicit and testable.

Runtime presentation is driven by a plain C# presenter. The presenter samples current gameplay, motion, surface, and run-result state once per rendered tick. It passes a small classification input to a pure Character Presentation Mode Classifier. The classifier returns a Character Presentation Classification Result containing the selected Character Presentation Mode. The presenter then builds a Character Presentation Frame and applies it to a passive Unity view.

The Unity view owns Animator references, SaintsField-backed Animator parameter fields, and prefab-owned presentation tuning. It does not decide what a launch, result, or gameplay state means. It only applies presentation frames to the Animator and enforces view-level safety such as disabling root motion.

The first slice uses one Animator base layer with these parameters:

1. `PresentationMode` as `int`.
2. `PlaybackSpeedMultiplier` as `float`.

The current presentation mode enum includes slingshot-specific modes from later presentation work. The original Ladybug slice used these user-facing modes, with Run now reserved for compatibility:

1. `Idle = 0`
2. `PullAnticipation = 1`
3. `LaunchPush = 2`
4. `Slide = 3`
5. `Run = 4`
6. `Airborne = 5`
7. `Victory = 6`
8. `Defeat = 7`

Slide is the default active grounded locomotion for meaningful grounded movement, including downhill and flat coasting. Run remains a reserved compatibility mode rather than a normal visible locomotion result. Airborne is used only after a short ungrounded debounce to avoid flicker on tiny terrain gaps. Victory and Defeat are terminal presentation modes sourced from accepted run results.

The implementation should use generic names for reusable runtime code. `Ladybug` belongs in concrete asset names, prefab names, controller names, and imported asset buckets. Runtime classes should use `Character*` names unless they are truly Ladybug-specific.

## Unity Surfaces

Runtime gameplay surfaces:

1. Character presentation presenter.
2. Character presentation mode classifier.
3. Character presentation classification input and result.
4. Character presentation frame.
5. Character presentation mode enum.
6. Character presentation view interface.
7. Character presentation tuning interface.
8. Unity-facing Character Presentation View MonoBehaviour.
9. Continuous run surface context source.
10. Run surface context value type.
11. Run surface slope calculator.
12. Signed course-forward speed helper on the run progress snapshot.
13. Run result notifier contract exposed by the run-end flow.
14. VContainer registration in the gameplay lifetime scope.

Prefab and scene surfaces:

1. Launch Target root remains the authoritative gameplay body.
2. Band Center remains a dedicated slingshot/band reference.
3. Launch Target Collider Root owns the gameplay collider.
4. Character Visual Anchor owns the character visual prefab instance.
5. Ladybug Character prefab is project-owned composition, not a raw vendor prefab.
6. Ladybug Character prefab contains Character Presentation View and Animator.
7. Ladybug Character prefab contains an internal Model Root for imported mesh/avatar/skeleton offset, rotation, and scale quirks.
8. Ladybug Character Animator Controller is project-owned.
9. Imported Ladybug source assets live under the third-party plugins bucket while preserving meta GUIDs.

Animator surfaces:

1. One full-body base layer for the first slice.
2. Idle state using `Ilde_Breathing`.
3. Slide state using the plain `Slide` clip.
4. Run state using `RunLoop` or `RunLoop2`.
5. Airborne state using `JumpFall`.
6. Victory state using `Victory`.
7. Defeat state using `Death2`.
8. Persistent `PresentationMode` integer parameter.
9. Persistent `PlaybackSpeedMultiplier` float parameter.
10. Authored transition clips only when they support reserved compatibility or future polish; normal grounded locomotion must not depend on Run-to-Slide or Slide-to-Run transitions.

Imported Ladybug asset surfaces:

1. `LadyBug@TPose.FBX` is the primary source character/model/avatar asset because key gameplay clips reference it as their avatar source.
2. UI Ladybug variants are not used as the primary player source.
3. `LadyBug_Avatar.FBX` is not used as the primary player source for this slice.
4. Yoyo clips, transition wipe, slide center rotation, and other specialized clips are cataloged but not required for the first slice.

Package and tooling surfaces:

1. SaintsField Animator parameter fields are used on serialized Unity-facing view fields when the package is already available in the project.
2. Unity AI Agent Connector remains the compile and test gate.
3. Rider reformat and file-problem checks apply to code changes in the implementation slice.
4. No save data, Addressables schema, Unity version, signing, or remote service surfaces are expected for this PRD.

## User Stories

1. As a player, I want the playable target to look like a Ladybug character so that the game has a recognizable main character instead of a prototype cylinder.

2. As a player, I want the Ladybug to slide when moving downhill so that the default animation matches the main fantasy of the game.

3. Superseded by slide-only presentation: flatter forward-moving sections now remain visible Slide when movement is meaningful.

4. As a player, I want the Ladybug to continue moving with the current physics feel so that changing the character does not change the core controls.

5. As a player, I want the Ladybug to react when airborne so that jumps, bumps, and terrain gaps feel intentional.

6. As a player, I want short terrain gaps not to instantly flicker the Ladybug into an airborne animation so that locomotion feels stable.

7. As a player, I want victory to show a positive character animation so that finishing a run feels complete.

8. As a player, I want defeat to show a failure animation so that collisions and run loss have clear feedback.

9. As a player, I want animation speed to broadly match movement speed so that fast and slow motion do not look disconnected from physics.

10. As a player, I want the Ladybug to stay visually attached to the gameplay body so that slingshot launches and downhill sliding remain readable.

11. As a player, I want the band and target interaction to remain predictable so that pulling and launching still feel like the same game.

12. As a player, I want the character collider to fit the Ladybug better than the prototype cylinder so that contacts feel fair.

13. As a player, I want collider changes not to cause confusing band wrapping or launch behavior so that the character still feels controllable.

14. As a player, I want the run camera to continue framing the gameplay body correctly so that the character change does not break camera readability.

15. As a designer, I want Slide to be the active downhill default so that levels can be authored around the main sliding loop.

16. Superseded by slide-only presentation: flatter forward-moving surfaces should remain visible Slide when movement is meaningful.

17. Superseded by slide-only presentation: slope changes should not switch normal grounded locomotion between Slide and Run.

18. As a designer, I want a minimum locomotion mode duration so that locomotion transitions remain visually intentional.

19. As a designer, I want Airborne to have a configurable enter delay so that tiny contact losses do not look like jumps.

20. As a designer, I want Airborne to exit immediately when grounded so that landing feels responsive.

21. As a designer, I want a Slide reference speed so that grounded locomotion playback can be calibrated consistently.

22. As a designer, I want playback speed clamps so that animations do not become absurdly slow or fast.

23. As a designer, I want imported Ladybug VFX and sounds cataloged so that later slices can add polish without redoing asset discovery.

24. As a designer, I want yoyo, launch, and lateral lean presentation deferred until their gameplay meaning is concrete so that the first slice stays focused.

25. As a designer, I want the first slice to avoid root motion so that physics remains the only source of gameplay movement.

26. As a designer, I want the raw vendor prefab not to be used directly so that project gameplay composition remains stable even if imported assets change.

27. As a designer, I want Ladybug source assets grouped as third-party plugin content so that ownership is clear in the project tree.

28. As a technical artist, I want a project-owned Ladybug Character prefab so that scale, offsets, materials, and Animator wiring can be controlled safely.

29. As a technical artist, I want a Model Root inside the prefab so that imported skeleton orientation and scale quirks do not leak into gameplay anchors.

30. As a technical artist, I want a Character Visual Anchor under the Launch Target so that visual alignment can be tuned independently of the band center.

31. As a technical artist, I want the gameplay collider root separated from the character visual root so that presentation edits do not accidentally change collisions.

32. As a technical artist, I want the Animator Controller to use stable parameter names so that clips and transitions can be audited.

33. As a technical artist, I want SaintsField Animator parameter fields in the view so that serialized parameter references are less error-prone.

34. As a technical artist, I want Animator state names not to be serialized into gameplay logic so that controller internals can evolve.

35. As a technical artist, I want the imported Slide clip to be the first downhill default so that the character uses available authored content.

36. As a technical artist, I want `Slide_CenterRotation` deferred so that it is not wired before the lateral presentation problem is defined.

37. As a technical artist, I want full LFS assets available before final visual calibration so that mesh bounds and material appearance are judged from real assets.

38. As a gameplay engineer, I want reusable runtime classes to use Character terminology so that the same system can support future characters.

39. As a gameplay engineer, I want the classifier to be pure C# so that most transition rules are tested without scene setup.

40. As a gameplay engineer, I want the presenter to be plain C# and lifecycle-managed by VContainer so that orchestration is testable and not hidden in MonoBehaviour callbacks.

41. As a gameplay engineer, I want the Unity view to be passive so that it does not interpret gameplay state or run results.

42. As a gameplay engineer, I want the presenter to pass only a Character Presentation Frame to the view so that the view contract stays shallow.

43. As a gameplay engineer, I want the frame to contain only mode and playback speed for the first slice so that raw slope and gameplay data do not leak into Animator code.

44. As a gameplay engineer, I want a signed course-forward speed helper so that diagnostics and future presentation flavor can reason about forward motion rather than only planar magnitude.

45. As a gameplay engineer, I want course-planar speed to remain available so that playback speed can use stable magnitude data.

46. As a gameplay engineer, I want a continuous surface context source so that slope classification is not derived from one-shot contact-enter events.

47. As a gameplay engineer, I want run surface context to expose grounded state, ground normal, and forward downhill degrees so that consumers get semantic terrain data.

48. As a gameplay engineer, I want raw collider, raycast, tag, and category details to stay out of the surface context payload so that presentation stays decoupled from probe implementation.

49. As a gameplay engineer, I want terminal Victory and Defeat modes to outrank locomotion so that run completion cannot be overwritten by movement state.

50. As a gameplay engineer, I want pre-launch slingshot hold to map to Idle so that no launch-specific one-shot command is needed in the first slice.

51. As a gameplay engineer, I want one-shot commands excluded from the first slice so that the view does not need gameplay vocabulary like launch or run-ended.

52. As a gameplay engineer, I want run-result notification exposed as a direct C# event so that the presenter can react without a global event bus.

53. As a gameplay engineer, I want the view never to receive Run Result or Run End Reason so that success and failure mapping stays outside the view.

54. As a gameplay engineer, I want the classifier result to contain the mode and not expose reason strings as part of the core contract so that debug diagnostics do not become runtime API.

55. As a gameplay engineer, I want optional classifier diagnostics to log only when the calculated mode changes so that debugging is useful without polluting normal contracts.

56. As a gameplay engineer, I want root motion disabled on the Animator so that imported animation cannot move the Launch Target Rigidbody.

57. As a gameplay engineer, I want imported Rigidbody, Collider, Joint, CharacterController, ragdoll, and hitbox components stripped or disabled in the first-slice character prefab so that there is one gameplay physics authority.

58. As a gameplay engineer, I want mode timing and ungrounded timing owned by the presenter so that the classifier remains deterministic and stateless except for explicit input.

59. As a gameplay engineer, I want the presenter to reset ungrounded timing on grounded, pre-launch, and new-run boundaries so that Airborne debounce starts from meaningful states.

60. As a gameplay engineer, I want the presenter to use rendered tick delta time for presentation so that Animator updates align with visual frames.

61. As a gameplay engineer, I want FixedUpdate physics to remain separate from visual presentation sampling so that physics behavior does not change.

62. As a gameplay engineer, I want VContainer registration to follow existing gameplay lifetime patterns so that dependency direction remains consistent.

63. As a QA engineer, I want EditMode tests for classifier priority so that terminal, pre-launch, airborne, slide, reserved Run compatibility, and fallback order is locked.

64. As a QA engineer, I want EditMode tests for slope calculation so that downhill, uphill, flat, and banked surfaces classify consistently.

65. As a QA engineer, I want tests for Slide mode hold and reserved Run normalization so that animation presentation does not chatter.

66. As a QA engineer, I want tests for Airborne debounce so that short gaps preserve locomotion and sustained gaps switch to Airborne.

67. As a QA engineer, I want tests for playback speed calculation so that reference speeds and clamps behave predictably.

68. As a QA engineer, I want tests that the presenter unsubscribes from result events so that scene teardown does not leave stale callbacks.

69. As a QA engineer, I want tests that accepted run results map to Victory or Defeat so that terminal presentation is deterministic.

70. As a QA engineer, I want PlayMode checks that the composed Launch Target has one authoritative Rigidbody and gameplay collider owner so that imported physics does not interfere.

71. As a QA engineer, I want PlayMode checks that the Ladybug visual anchor does not move the Band Center, gameplay collider, or Rigidbody so that presentation stays isolated.

72. As a QA engineer, I want existing slingshot band and run contact tests to keep passing after collider changes so that the player swap does not regress core play.

73. As a QA engineer, I want an Animator parameter consistency test so that the serialized view fields match the project-owned controller.

74. As a QA engineer, I want a visual smoke pass in Unity using full Ladybug assets so that scale, orientation, collider fit, and camera framing are validated in-editor.

75. As a producer, I want the first slice to be shippable without character abilities so that the visual upgrade does not block on future progression systems.

76. As a producer, I want explicit out-of-scope boundaries for VFX, audio, yoyo, launch one-shots, and abilities so that the implementation remains controllable.

77. As a maintainer, I want the PRD to preserve agreed terminology so that future issues and code reviews use the same words.

78. As a maintainer, I want the third-party asset move to preserve meta GUIDs so that animation clip references and avatar links survive the reorganization.

79. As a maintainer, I want no save migration for this slice so that release risk stays limited to scene, prefab, animation, and presentation code.

80. As a maintainer, I want no Addressables schema change for this slice so that build content routing is not expanded before needed.

## Implementation Decisions

The first implementation slice is a presentation-only character swap. It must not introduce Ladybug gameplay abilities, new player physics ownership, or root-motion movement.

Reusable runtime code uses Character terminology. Ladybug appears only in concrete asset names, prefab names, Animator Controller names, imported asset folders, and any future asset-specific authoring notes.

The existing Launch Target remains the authoritative gameplay body. It continues to own physics movement, slingshot behavior, band-center semantics, steering, contact handling, run progress, and camera relationships.

The Ladybug Character is mounted as a presentation child under a dedicated Character Visual Anchor. The gameplay collider is mounted under a separate Launch Target Collider Root. Band Center remains separate from both of those roots.

The project uses a project-owned Ladybug Character prefab instead of a raw vendor prefab. The project-owned prefab contains the Character Presentation View, Animator, and a Model Root. The Model Root contains imported mesh, avatar, skeleton, material, offset, rotation, and scale details.

Imported Ladybug source content is treated as third-party plugin content. Moving it into the plugin bucket must preserve meta GUIDs, especially the GUIDs used by imported clips and avatar references.

The implementation should start from the imported T-pose Ladybug character asset because the relevant gameplay clips reference that asset as their avatar source. UI-oriented Ladybug variants and the separate avatar file are not the primary player source for this slice.

The first-slice clip mapping is:

1. Idle uses `Ilde_Breathing`.
2. Slide uses plain `Slide`.
3. Run uses `RunLoop` or `RunLoop2` only as a reserved compatibility state when authored controller compatibility requires it.
4. Airborne uses `JumpFall`.
5. Victory uses `Victory`.
6. Defeat uses `Death2`.

The first-slice Animator Controller uses one full-body base layer. The controller has persistent `PresentationMode` and `PlaybackSpeedMultiplier` parameters. It does not require gameplay trigger parameters or one-shot commands.

The view uses SaintsField Animator parameter references for `PresentationMode` and `PlaybackSpeedMultiplier`. The first slice does not serialize Animator state references or state names into gameplay code.

The Character Presentation View is a shallow Unity adapter. It owns serialized Animator references, serialized Animator parameter references, prefab-owned tuning values, and a root-motion guard. It exposes frame application to the presenter and does not classify gameplay state.

The view may provide presentation tuning through an interface because the tuning belongs to the prefab and character authoring surface. This avoids a separate config asset until multiple characters, abilities, or shared authoring workflows create a real need for one.

The Character Presentation Presenter is a plain C# object managed by the gameplay lifetime scope. It samples dependencies once per rendered tick, tracks current mode timing, tracks ungrounded elapsed time, listens for accepted run results, builds Character Presentation Frames, and applies them to the view.

The presenter uses injected frame delta time for presentation timing. It does not move physics, does not call physics stepping APIs, and does not rely on runtime MonoBehaviour lifecycle callbacks for its core orchestration.

The Character Presentation Mode Classifier is a pure module. It accepts a Character Presentation Classification Input and returns a Character Presentation Classification Result containing the selected Character Presentation Mode.

The classification input includes current mode, current mode elapsed seconds, ungrounded elapsed seconds, pre-launch state, active-run state, run-ended state, finished state, run surface context, course-planar speed, course-forward speed, and linear velocity.

The classification result contains the Character Presentation Mode. It does not expose reason strings as required product API. Optional diagnostics may log transition reasons when the calculated mode changes.

Classification priority is:

1. Terminal Victory or Defeat.
2. Pre-launch Idle.
3. Active-run Airborne after debounce.
4. Active grounded Slide.
5. Reserved Run compatibility normalized to Slide before view application.
6. Fallback Idle.

Idle represents pre-launch hold and non-active fallback. The first slice does not use a Held mode.

Slide is the default active grounded locomotion for meaningful grounded movement. Flat grounded coasting no longer selects visible Run.

Normal grounded presentation uses a meaningful grounded movement threshold plus a minimum locomotion mode duration. Slope thresholds no longer switch normal locomotion between Slide and Run.

Airborne switching uses an enter delay. During short ungrounded gaps, the presenter preserves visible Slide for active locomotion. Once ungrounded time exceeds the delay, the classifier selects Airborne. Airborne exits immediately once grounded.

The first slice does not include a Landing mode. Landing can be added later if the game needs a visible landing anticipation or impact beat.

The Character Presentation Frame contains only selected mode and playback speed multiplier. It does not carry raw slope, grounded state, vertical speed, steering values, gameplay state IDs, run-end reasons, or trigger commands.

Playback speed is driven by an Animator float parameter rather than global Animator speed. This allows clip playback to be controlled without changing the Animator component's global timing semantics.

Playback speed multiplier is calculated from course-planar speed divided by Slide Reference Speed, then clamped by tuning. Non-locomotion modes default to `1`.

The initial Slide Reference Speed is seeded from the middle of the current launch speed range. This is a calibration starting point, not an imported clip truth.

The signed course-forward speed helper is added beside the existing course-planar speed helper for diagnostics and future presentation flavor. Normal grounded slide-only classification uses course-planar speed.

The continuous run surface context source is separate from contact-enter classification. It exposes stable semantic terrain data for presentation: grounded state, ground normal, and forward downhill degrees.

The run surface slope calculator is pure math. It derives forward downhill degrees from ground normal, course forward direction, and course up direction.

The run-result notifier exposes accepted run results through a direct C# event. The presenter maps success to Victory and failure to Defeat. The view never receives run result objects or run-end reason values.

Imported physics-like components on the character prefab are stripped or disabled for the first slice. This includes imported Rigidbody, Collider, Joint, CharacterController, ragdoll, hitbox, and trigger components. Only the existing gameplay physics body remains authoritative.

Animator root motion is disabled and guarded. Imported clips may animate bones and visual children only. They must not move the Launch Target Rigidbody, Band Center, gameplay collider, or camera anchor.

Collider shape, size, and offset are tuned only after the visual scale is locked. Collider changes are treated as gameplay-affecting because they can change band wrapping, obstacle contacts, and lost-momentum behavior.

Ladybug VFX and sounds are cataloged for later slices. Candidate assets include slide dust, cartoon slide effects, ouch vocals, jump, land, duck, footsteps, death, woosh, yoyo spin, and transition wipe assets.

The first slice may ship without VFX or audio if animation presentation, physics isolation, and collider fit are complete. Any VFX or audio added in the first slice should be wired through presentation-facing facades and must not couple slingshot, steering, or run-end logic to concrete Ladybug assets.

## Testing Decisions

Testing starts with pure EditMode tests for deep modules. PlayMode tests are used where Unity scene, prefab, Animator, physics, or serialized component behavior is the actual risk.

The Character Presentation Mode Classifier gets EditMode tests for terminal priority, pre-launch Idle, active grounded Slide, reserved Run normalization, fallback Idle, Airborne debounce, Airborne immediate exit on grounded, and minimum locomotion mode duration.

Classifier tests should cover edge cases where course-planar speed is non-zero but course-forward speed is zero or negative. Those cases should still produce visible Slide when grounded movement is meaningful, and they must not expose visible Run merely because the body is moving sideways or backward.

Run Surface Slope Calculator gets EditMode tests for flat surface, downhill surface, uphill surface, banked surface, normalized and non-normalized input, and expected sign convention for forward downhill degrees.

Run Progress Frame Snapshot gets EditMode tests for signed course-forward speed. Tests cover forward movement, backward movement, pure lateral movement, zero velocity, and non-unit forward direction if relevant to the API contract.

Playback speed calculation gets EditMode tests. Tests cover Slide reference speed, clamping, zero reference speed guard behavior, zero movement, fast movement, and non-locomotion default playback.

Character Presentation Presenter gets EditMode tests using test doubles for motion, surface context, run-result notifier, time, classifier, tuning, and view. Tests cover applying one frame per tick, preserving mode elapsed time, resetting ungrounded time, mapping accepted run results to terminal presentation, and unsubscribing on disposal.

Presenter tests should assert externally visible calls to the view, not private fields. If test hooks are needed, they should follow the project's explicit test-hook conventions and avoid reflection.

Run Result Notifier behavior gets focused tests around accepted result publication. Tests should verify that the event is emitted only when the run result is accepted by the run-end flow and that success/failure values remain intact for the presenter.

Character Presentation View gets Unity-level tests for Animator parameter application. The test should verify that applying a frame writes the configured integer and float Animator parameters.

Character Presentation View gets a root-motion guard test. The test should verify that root motion is disabled or corrected according to the view contract.

Animator parameter consistency gets a serialized asset or PlayMode test when practical. It should verify that the Ladybug Character prefab's serialized parameter references point at actual parameters in the project-owned Animator Controller.

Prefab composition gets PlayMode tests. Tests verify that the Launch Target has one authoritative Rigidbody, one gameplay collider ownership path, a separate Band Center, a separate Character Visual Anchor, and a Ladybug Character child.

Prefab composition tests verify that the Ladybug Character child does not contain active imported Rigidbody, Collider, Joint, CharacterController, ragdoll, hitbox, or trigger components that can interfere with gameplay physics.

Prefab isolation tests verify that visual anchor transform changes do not move the Band Center, gameplay collider root, Launch Target Rigidbody root, or camera anchor.

Existing slingshot band, launch, run contact, and run-end tests remain gates. They should be run after collider hierarchy changes because the character swap can affect gameplay contacts even when the code change is presentation-focused.

Visual QA is required after full LFS assets are available. Manual or captured PlayMode checks should inspect Ladybug scale, orientation, material appearance, collider fit, slingshot pull readability, Slide/Idle/Airborne transitions, reserved Run compatibility, victory, defeat, and camera framing.

No test should hardcode imported asset paths or GUIDs unless the project already has a typed test asset provider for that purpose. If tests need concrete assets, expose them through the project's typed test asset provider pattern.

No production implementation should be considered done until Unity compilation is clean. Targeted tests added or changed for this slice should run only after compile is clean.

## Release and Compatibility

This is a Unity gameplay presentation change. It is not a save-data feature and should not require a player migration.

Moving imported Ladybug content under the third-party plugin bucket is compatibility-sensitive. Meta files must be preserved so existing clip, avatar, material, texture, and prefab references survive the move.

The implementation should avoid package manifest changes if SaintsField is already available in the project. If SaintsField is missing, adding it is a package change and requires explicit approval before implementation.

The first slice should not require Addressables schema changes, Unity version changes, signing changes, analytics changes, remote config changes, or backend changes.

Scene and prefab serialization are the main release risks. Review diffs carefully to ensure only the intended Launch Target hierarchy, Ladybug Character prefab, Animator Controller, and presentation wiring changed.

Physics compatibility is a release gate. Existing slingshot, steering, run progress, contact, camera, and run-end behavior should remain the same except for intentional collider shape and offset tuning.

Collider tuning is gameplay-affecting. If the collider is resized or offset, run-end thresholds should not be changed opportunistically unless tests or design review show a concrete need.

Animation compatibility depends on the real imported assets. The current checkout may contain Git LFS pointer files for binary FBX content, so final visual validation requires the real asset payload.

Mobile performance should remain stable because the first slice uses a single character Animator and one presentation tick path. Any added VFX or audio should be reviewed separately if enabled in the first slice.

## Out of Scope

1. Character abilities.
2. Multiple selectable characters.
3. Character ability configs.
4. Splitting presentation config from ability config.
5. Save data or progression changes.
6. Addressables schema changes.
7. Remote config.
8. Analytics.
9. New monetization or economy hooks.
10. Result screen UI changes.
11. New level design.
12. Root-motion gameplay movement.
13. Vendor prefab physics as gameplay authority.
14. Full character customization.
15. A full animation blend tree.
16. Landing mode.
17. Launch one-shot commands.
18. Run-ended one-shot commands.
19. Yoyo presentation.
20. Lateral lean presentation.
21. `Slide_CenterRotation` integration.
22. Transition wipe integration.
23. Full VFX pass.
24. Full audio pass.
25. Material overhaul beyond what is needed to make the character render correctly.
26. Camera system replacement.
27. Slingshot system redesign.
28. Run-end flow redesign.
29. Contact classification redesign.
30. Public API or package release.

## Further Notes

Assumptions:

1. Full Ladybug LFS assets will be available before final prefab, material, collider, and visual QA calibration.
2. The imported T-pose Ladybug source asset remains the correct avatar source after the third-party asset move.
3. The existing Launch Target physics feel is product-correct and should be preserved.
4. The current launch speed range is a reasonable first calibration source for Slide playback reference speed.
5. The first slice is allowed to be animation-only if VFX and audio wiring would broaden scope too much.
6. SaintsField is already available or will be approved separately before implementation if it is missing.

Unresolved but non-blocking calibration questions:

1. Exact Ladybug visual scale, local offset, and local rotation require full asset inspection in Unity.
2. Exact gameplay collider shape, size, and offset require visual alignment plus band/contact regression checks.
3. Exact meaningful-grounded-movement threshold and Slide hold duration require in-editor playtesting against representative downhill and flat level sections.
4. Exact Slide reference speed requires visual calibration against authored clips.
5. Final material and shader fixes depend on how the imported assets render in the active pipeline.
6. First-slice VFX and audio inclusion can be decided after the animation-only path is validated.

No blocking product questions remain for writing implementation issues from this PRD.
