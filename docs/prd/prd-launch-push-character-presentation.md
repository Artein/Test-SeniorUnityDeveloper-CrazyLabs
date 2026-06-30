# PRD: Launch Push Character Presentation

## Problem Statement

After replacing the prototype cylinder with the Ladybug Character, the current presentation system can show downhill Slide or transition into Run while the Launch Target is still in the early post-release slingshot motion. From the player's perspective this looks wrong: the character appears to be sliding while the slingshot is still pushing it away, and the later Slide-to-Run handoff can briefly expose an awkward pose where the legs split apart.

The slingshot band visuals are not the problem in this feature. The problem is character presentation ownership. The current Character Presentation Mode classification understands pre-launch Idle, locomotion, Airborne, Victory, and Defeat, but it does not yet understand the two slingshot-specific character presentation intervals that happen around a launch:

1. Pull Anticipation while a validated Active Pull holds the Launch Target before Pull Release.
2. Launch Push after a valid Launch is applied while the Launch Target is visually being pushed away from the Slingshot before normal locomotion should take over.

The game needs these intervals to be explicit presentation modes. They must not be inferred from raw pointer input, Slingshot View calls, Band Shape state, or Band Release Recoil lifetime. They should come from slingshot-owned facts after slingshot validation, then flow through the existing Character Presentation Presenter, Character Presentation Mode Classifier, Character Presentation Frame, and passive Character Presentation View.

## Solution

Add first-class Pull Anticipation and Launch Push support to the generic Character presentation system.

The Slingshot publishes validated presentation-facing facts through small slingshot-owned contracts. A Slingshot Presentation Context Source exposes one immutable Slingshot Presentation Context. The Character Presentation Presenter reads that context each rendered tick, forwards only mode-selection facts into Character Presentation Classification Input, and copies normalized pull/launch values into Character Presentation Frame.

The Character Presentation Mode Classifier owns mode precedence. Terminal modes remain highest priority, then Pull Anticipation, then Launch Push, then normal pre-launch and locomotion classification. Pull Anticipation is selected while a validated Active Pull is present. Launch Push is selected after LaunchApplied until a configurable minimum handoff guard allows normal locomotion to resume. Normalized pull and launch values tune animation intensity and lateral variation; they are not elapsed time and they are not mode-exit conditions.

The Character Presentation View stays shallow. It receives a frame and writes Animator parameters. It does not subscribe to slingshot events, inspect gameplay state, read pointer input, or interpret Launch Requests. Pull Anticipation and Launch Push are dedicated Animator states or blend trees selected by PresentationMode. They are not one-shot triggers and they do not route through Slide or Run.

The expected player-facing result is:

1. While the player drags the band with a valid pull, the character shows Pull Anticipation instead of generic Idle or locomotion.
2. On accepted launch, Pull Anticipation hands off immediately to Launch Push.
3. During Launch Push, Slide and Run cannot take over early, even if the support surface is downhill or flat.
4. After the minimum Launch Push guard elapses, normal grounded or airborne locomotion resumes using existing surface and motion classification.
5. Terminal Victory or Defeat still override everything else.

## Unity Surfaces

Runtime assemblies and asmdefs:

1. Gameplay runtime assembly containing Character Presentation and Slingshot runtime code.
2. Gameplay EditMode test assembly for pure classifier, presenter, slingshot context source, normalizer, and notifier tests.
3. Gameplay PlayMode test assembly for scene/container composition and manual-regression coverage where Unity object wiring matters.
4. Existing SaintsField runtime dependency remains available for serialized Animator parameter fields when new frame parameters are exposed on the view.

Character Presentation runtime surfaces:

1. Character Presentation Mode enum gains PullAnticipation and LaunchPush.
2. Character Presentation Classification Input gains slingshot mode-selection facts: HasActivePull, HasLaunchPush, and LaunchPushElapsedSeconds.
3. Character Presentation Tuning gains LaunchPushMinimumSeconds.
4. Character Presentation Mode Classifier gains slingshot presentation precedence.
5. Character Presentation Presenter gains a dependency on a Slingshot Presentation Context Source contract.
6. Character Presentation Frame gains NormalizedPull, NormalizedLaunchPower, NormalizedPullOffset, and NormalizedLaunchOffset.
7. Character Presentation View gains serialized Animator parameter fields for those four float values, using Animator parameter dropdowns for designer safety.
8. Character Presentation View continues to disable Animator root motion and apply frames only.

Slingshot runtime surfaces:

1. Slingshot Active Pull Notifier publishes validated Active Pull changes and clear events.
2. Slingshot Active Pull Context carries Normalized Pull and Normalized Pull Offset for the first slice.
3. Slingshot Capture Lifecycle Notifier publishes CaptureEnabled and CaptureDisabled lifecycle edges.
4. Slingshot Presentation Context Source consumes active pull, launch applied, and capture lifecycle events.
5. Slingshot Presentation Context exposes HasActivePull, NormalizedPull, NormalizedPullOffset, HasLaunchPush, LaunchPushElapsedSeconds, NormalizedLaunchPower, and NormalizedLaunchOffset.
6. Slingshot Pull Offset Normalizer owns signed lateral normalization from raw Pull Offset and Pull Distance through current slingshot geometry/config limits.
7. Existing launch applied notification remains the accepted-launch edge for starting Launch Push.
8. Existing launch request remains internal to slingshot mapping and is not exposed to Character Presentation Presenter or View.

Scene, prefab, Animator, and asset surfaces:

1. The Ladybug Character prefab remains a project-owned visual prefab under the Character Visual Anchor.
2. The Launch Target root remains the gameplay authority for Rigidbody, collider, Band Center, camera anchor, steering, and contacts.
3. No imported Ladybug physics components become authoritative gameplay components as part of this feature.
4. The Ladybug Animator Controller gains PullAnticipation and LaunchPush states or blend trees.
5. PresentationMode drives state selection for Idle, PullAnticipation, LaunchPush, Slide, Run, Airborne, Victory, and Defeat.
6. NormalizedPull, NormalizedLaunchPower, NormalizedPullOffset, and NormalizedLaunchOffset are Animator float parameters.
7. PlaybackSpeedMultiplier remains available for locomotion playback scaling.
8. The Slingshot band rig, Band Shape provider, Band Release Recoil visuals, and touch indicator remain slingshot presentation surfaces, not character presentation surfaces.

Composition and lifecycle surfaces:

1. Slingshot services and notifier contracts are registered by the slingshot installer, not by adding scene references to the gameplay lifetime scope.
2. The slingshot controller can implement multiple notifier interfaces in the first slice if that keeps event ownership local.
3. The Slingshot Presentation Context Source is registered as a slingshot-owned singleton/entry point and exposed through a read-only interface.
4. The Slingshot Pull Offset Normalizer is registered as a pure singleton service.
5. The Character Presentation Presenter consumes the Slingshot Presentation Context Source through DI.
6. New events use direct C# events and the existing safe invocation extension.

Tooling and workflow surfaces:

1. Unity AI Agent Connector remains the compile and test gate for implementation work.
2. Rider reformat and file-problem checks apply to changed C# and asmdef files during implementation.
3. No package manifest, Unity version, Addressables schema, save format, signing, or remote service changes are expected.

## User Stories

1. As a player, I want the character to react while I pull the slingshot, so that the launch preparation feels connected to the character.
2. As a player, I want the character to show a launch push animation right after release, so that the first moment after launch looks intentional.
3. As a player, I want Slide not to appear while the slingshot is still visually pushing the character, so that the launch does not look like downhill locomotion too early.
4. As a player, I want Run not to appear during the initial launch push, so that flat ground under the target does not override the slingshot moment.
5. As a player, I want the character to hand off from launch push into Slide or Run cleanly, so that the transition does not show broken leg poses.
6. As a player, I want Pull Anticipation to respond to pull strength, so that a deeper pull looks more intense than a small pull.
7. As a player, I want Pull Anticipation to respond to lateral pull direction, so that left and right pulls can be visually readable.
8. As a player, I want Launch Push to preserve the accepted launch strength, so that the post-release animation matches the shot that was actually fired.
9. As a player, I want Launch Push to preserve accepted lateral launch intent, so that the character can lean or pose consistently with the release.
10. As a player, I want weak or canceled pulls to stop the anticipation pose, so that the character does not stay in a stale launch-prep animation.
11. As a player, I want invalid pulls to return to pre-launch presentation, so that failed input does not look like a real launch.
12. As a player, I want victory and defeat animations to still override slingshot presentation, so that run-end feedback stays clear.
13. As a player, I want airborne presentation after launch once the launch-push handoff is complete, so that jumps and gaps still read correctly.
14. As a player, I want downhill Slide to remain the normal locomotion default after the launch-push interval, so that the core downhill fantasy remains intact.
15. As a player, I want flat forward sections to use Run after launch-push handoff, so that character motion matches the surface.
16. As a designer, I want PullAnticipation to be a Character Presentation Mode, so that I can author a dedicated Animator state or blend tree.
17. As a designer, I want LaunchPush to be a Character Presentation Mode, so that I can author launch-specific animation independent from Slide and Run.
18. As a designer, I want Pull Anticipation to enter quickly from eligible modes, so that input feels responsive.
19. As a designer, I want Launch Push to enter immediately after accepted launch, so that the launch edge is visually clear.
20. As a designer, I want Launch Push to have a minimum duration, so that locomotion cannot cut it off after a single frame.
21. As a designer, I want Launch Push exit timing to be separate from normalized launch power, so that animation intensity and mode lifetime can be tuned independently.
22. As a designer, I want Launch Push lifetime to be independent from Band Release Recoil, so that character animation is not stretched or cut by band clearance.
23. As a designer, I want normalized pull strength available in Animator, so that Pull Anticipation can blend between subtle and intense poses.
24. As a designer, I want normalized launch power available in Animator, so that Launch Push can vary by shot strength.
25. As a designer, I want normalized pull offset available in Animator, so that Pull Anticipation can vary left and right.
26. As a designer, I want normalized launch offset available in Animator, so that Launch Push can vary left and right from the accepted release.
27. As a designer, I want inactive slingshot channels zeroed, so that Animator blend trees do not keep stale values after a mode changes.
28. As a designer, I want existing PlaybackSpeedMultiplier to continue applying to locomotion, so that Slide and Run calibration is preserved.
29. As a technical artist, I want Animator parameter fields to use dropdowns, so that invalid parameter names are less likely in prefab authoring.
30. As a technical artist, I want PullAnticipation and LaunchPush selected by PresentationMode, so that Animator transitions can be inspected from one stable mode parameter.
31. As a technical artist, I want no Animator state names serialized into gameplay logic, so that controller internals can be refactored without code changes.
32. As a technical artist, I want root motion to stay disabled, so that imported animations cannot move the gameplay Rigidbody.
33. As a technical artist, I want launch-push animation to be tuned without moving Band Center or collider transforms, so that slingshot geometry remains stable.
34. As a gameplay engineer, I want slingshot presentation facts to be produced after slingshot validation, so that character presentation does not duplicate input validation.
35. As a gameplay engineer, I want Pull Anticipation not to read raw Pointer Input, so that input policy stays centralized in the Slingshot.
36. As a gameplay engineer, I want Pull Anticipation not to read Slingshot View state, so that view payloads do not become gameplay contracts.
37. As a gameplay engineer, I want a Slingshot Active Pull Notifier, so that live validated pull facts can be observed without coupling to input or view code.
38. As a gameplay engineer, I want a Slingshot Active Pull Context payload, so that active pull events expose only character-relevant normalized facts.
39. As a gameplay engineer, I want ActivePullChanged raised for live validated pulls, so that Pull Anticipation updates every meaningful pull frame.
40. As a gameplay engineer, I want ActivePullCleared raised for canceled, invalid, or weak pulls, so that Pull Anticipation clears deterministically.
41. As a gameplay engineer, I want valid launch release not to clear pull context too early, so that Pull Anticipation remains until Launch Push starts.
42. As a gameplay engineer, I want CaptureDisabled to clear active pull before LaunchApplied, so that live pull state does not overlap with frozen launch state.
43. As a gameplay engineer, I want CaptureEnabled to clear both pull and launch-push context, so that pre-launch recapture resets all slingshot presentation facts.
44. As a gameplay engineer, I want a Slingshot Capture Lifecycle Notifier separate from active pull events, so that reset/readiness edges are not mixed with pull values.
45. As a gameplay engineer, I want CaptureEnabled emitted after capture setup is committed, so that consumers see stable rest geometry.
46. As a gameplay engineer, I want CaptureDisabled emitted exactly once per real disable, so that consumers do not process duplicate lifecycle edges.
47. As a gameplay engineer, I want no lifecycle events for no-op same-state capture calls, so that subscribers can stay simple.
48. As a gameplay engineer, I want Slingshot Presentation Context Source to own launch-push elapsed time, so that the presenter does not manage slingshot timers.
49. As a gameplay engineer, I want LaunchPushElapsedSeconds to start at zero on LaunchApplied, so that the classifier sees a deterministic handoff window.
50. As a gameplay engineer, I want LaunchPushElapsedSeconds to use injected time, so that tests can be deterministic.
51. As a gameplay engineer, I want Slingshot Presentation Context Source to expose one immutable Current value, so that presenter reads are simple and allocation-free in shape.
52. As a gameplay engineer, I want Slingshot Presentation Context Source not to select Character Presentation Mode, so that all mode precedence remains in the classifier.
53. As a gameplay engineer, I want Character Presentation Presenter to gather facts only, so that it does not hide mode overrides.
54. As a gameplay engineer, I want Character Presentation Mode Classifier to own terminal, pull, push, and locomotion precedence, so that priority is testable in pure C#.
55. As a gameplay engineer, I want terminal modes to outrank Pull Anticipation and Launch Push, so that run results cannot be masked by stale slingshot facts.
56. As a gameplay engineer, I want Pull Anticipation to outrank Launch Push, so that a validated active pull always maps to pre-release presentation.
57. As a gameplay engineer, I want Launch Push to outrank locomotion until its minimum guard elapses, so that surface classification cannot steal the first launch frames.
58. As a gameplay engineer, I want normal locomotion to resume after the Launch Push guard, so that Slide, Run, and Airborne continue using existing physics facts.
59. As a gameplay engineer, I want Normalized Pull Offset to use effective side-specific limits, so that full-left and full-right values reflect the actual clamped pull range.
60. As a gameplay engineer, I want Normalized Launch Offset to use the same effective normalization at release, so that accepted launch pose matches active pull pose.
61. As a gameplay engineer, I want Slingshot Pull Offset Normalizer to be slingshot-owned, so that character presentation does not copy slingshot geometry math.
62. As a gameplay engineer, I want SlingshotController and Slingshot Presentation Context Source to share the normalizer, so that live pull and accepted launch offsets use one rule.
63. As a gameplay engineer, I want Normalized Pull and Normalized Launch Power to stay distinct, so that live pull intensity and accepted shot intensity do not overwrite each other.
64. As a gameplay engineer, I want Pull Anticipation frame values to zero launch channels, so that frozen launch data does not affect pre-release animation.
65. As a gameplay engineer, I want Launch Push frame values to zero pull channels, so that live pull data does not affect post-release animation.
66. As a gameplay engineer, I want locomotion, terminal, and idle frame values to zero all slingshot channels, so that old slingshot facts cannot leak into normal animation.
67. As a gameplay engineer, I want SlingshotInstaller to own new slingshot service registrations, so that GameplayLifetimeScope does not gain pure slingshot references.
68. As a gameplay engineer, I want the Character Presentation View to remain passive, so that MonoBehaviour responsibility stays shallow.
69. As a gameplay engineer, I want new notifier events to use safe invocation, so that one subscriber failure does not break remaining subscribers.
70. As a gameplay engineer, I want no global event bus for this feature, so that local coordination stays explicit.
71. As a QA engineer, I want classifier tests for Pull Anticipation versus downhill Slide, so that pull presentation priority is locked.
72. As a QA engineer, I want classifier tests for Launch Push versus downhill Slide, so that post-launch push cannot be overridden by slope.
73. As a QA engineer, I want classifier tests for Launch Push versus flat forward Run, so that post-launch push cannot be overridden by speed.
74. As a QA engineer, I want classifier tests for Launch Push handoff after the minimum duration, so that normal locomotion resumes deterministically.
75. As a QA engineer, I want classifier tests proving terminal modes outrank slingshot modes, so that run-end presentation remains deterministic.
76. As a QA engineer, I want presenter tests proving slingshot context facts are forwarded into classification input, so that mode decisions have the right facts.
77. As a QA engineer, I want presenter tests proving normalized pull/launch values are copied into frames, so that the view receives animation inputs.
78. As a QA engineer, I want slingshot context source tests for LaunchApplied, ticking, CaptureEnabled, and CaptureDisabled behavior, so that launch-push lifetime is deterministic.
79. As a QA engineer, I want active-pull notifier tests for changed and cleared events, so that Pull Anticipation starts and stops from the correct slingshot paths.
80. As a QA engineer, I want capture lifecycle notifier tests for ordering and no-op behavior, so that reset edges are stable.
81. As a QA engineer, I want pull offset normalizer tests, so that signed lateral values match slingshot limits and ramping.
82. As a QA engineer, I want composition tests resolving new interfaces, so that scene DI wiring matches the runtime design.
83. As a QA engineer, I want manual Unity smoke coverage for Pull Anticipation and Launch Push Animator states, so that authored animations look correct in Play Mode.
84. As a maintainer, I want the PRD to use agreed glossary terms, so that future issues and reviews do not drift back to ambiguous names like slingshot push phase.
85. As a maintainer, I want no public save or package migration in this feature, so that release risk stays limited to runtime presentation and Animator wiring.

## Implementation Decisions

1. Treat PullAnticipation and LaunchPush as Character Presentation Mode enum values.
2. Do not introduce a separate slingshot phase enum for character presentation.
3. Do not model Pull Anticipation or Launch Push as one-shot commands. They are durable modes selected from current facts.
4. Keep the existing Character Presentation Presenter plus pure Character Presentation Mode Classifier architecture.
5. Expand Character Presentation Classification Input with HasActivePull, HasLaunchPush, and LaunchPushElapsedSeconds.
6. Expand Character Presentation Classification Result only if future diagnostics are needed; the core result remains the selected mode.
7. Add LaunchPushMinimumSeconds to Character Presentation Tuning. This is a presentation handoff guard, not a launch physics property.
8. Keep Normalized Pull, Normalized Launch Power, Normalized Pull Offset, and Normalized Launch Offset out of mode selection for the first slice. They tune animation but do not decide presentation mode.
9. Classifier precedence is terminal result first, Pull Anticipation second, Launch Push third, then existing pre-launch/locomotion/airborne rules.
10. Pull Anticipation selection requires a validated active pull fact from slingshot-owned context.
11. Launch Push selection requires a latched launch-push fact and elapsed time below LaunchPushMinimumSeconds.
12. Existing locomotion timing and hysteresis continue to apply only after slingshot-specific modes no longer own presentation.
13. Character Presentation Presenter reads Slingshot Presentation Context Source each tick and builds classification input plus frame values.
14. Character Presentation Presenter does not subscribe directly to active-pull events for mode decisions; the context source owns event subscriptions and current-state caching.
15. Character Presentation Presenter does not increment LaunchPushElapsedSeconds. The slingshot presentation source owns that timer.
16. Character Presentation Frame carries Mode, PlaybackSpeedMultiplier, NormalizedPull, NormalizedLaunchPower, NormalizedPullOffset, and NormalizedLaunchOffset.
17. Pull Anticipation frames carry live pull values and zero launch values.
18. Launch Push frames carry frozen accepted-launch values and zero pull values.
19. Idle, Slide, Run, Airborne, Victory, and Defeat frames zero all slingshot-specific float values.
20. Character Presentation View writes Animator parameters only and remains a shallow MonoBehaviour view.
21. Character Presentation View does not know what pull, launch, run result, or gameplay state means beyond frame values and serialized Animator parameters.
22. Character Presentation View uses Animator parameter dropdown attributes for all serialized Animator parameter names where the project dependency supports it.
23. Animator state selection continues to use PresentationMode as the stable integer control parameter.
24. PullAnticipation and LaunchPush are dedicated Animator Controller states or blend trees selected by PresentationMode.
25. PullAnticipation enters from eligible states with no exit time and a short or immediate transition.
26. LaunchPush enters immediately after accepted LaunchApplied with no exit time.
27. LaunchPush hands off to locomotion through a short fixed transition after the classifier stops selecting LaunchPush.
28. Slide and Run transitions remain locomotion-specific and should not be used as the route into LaunchPush.
29. Slingshot Active Pull Notifier is the event source for validated active-pull changes.
30. Slingshot Active Pull Notifier exposes ActivePullChanged with Slingshot Active Pull Context and ActivePullCleared without payload.
31. Slingshot Active Pull Context first-slice payload contains NormalizedPull and NormalizedPullOffset. Raw pull diagnostics may be added only if useful and kept out of the character frame contract.
32. Slingshot Active Pull Context does not include Band Shape, touch indicator screen position, or view geometry payloads.
33. SlingshotController can implement Slingshot Active Pull Notifier in the first slice because it already owns input projection, pull clamping, validation, and active-pull cancellation paths.
34. ActivePullChanged is raised only after a pull is valid enough to update active slingshot presentation.
35. ActivePullCleared is raised when a live validated pull stops due to invalid pull visual, invalid band shape, weak release, or pointer cancellation.
36. Ignored pointer input with no live active pull does not raise ActivePullCleared.
37. Valid launch release does not clear Pull Anticipation immediately through ActivePullCleared. CaptureDisabled clears live pull before LaunchApplied starts Launch Push.
38. Slingshot Capture Lifecycle Notifier is separate from Slingshot Active Pull Notifier, even when both are implemented by the same component.
39. Slingshot Capture Lifecycle Notifier exposes CaptureEnabled and CaptureDisabled events without payload in the first slice.
40. CaptureEnabled is emitted after capture setup is committed, including geometry refresh, target hold/rest alignment, and capture idle view update.
41. CaptureDisabled is emitted exactly once when capture becomes disabled, including launch handoff and release-recoil early-exit paths.
42. No capture lifecycle event is emitted for no-op same-state capture calls.
43. New slingshot notifier events are raised through the project safe-invocation extension.
44. Slingshot Presentation Context Source is the read model consumed by Character Presentation Presenter.
45. Slingshot Presentation Context Source subscribes to active pull, capture lifecycle, and launch applied notifications.
46. Slingshot Presentation Context Source may map the accepted launch request at LaunchApplied, but it exposes only normalized presentation facts to character presentation.
47. Slingshot Presentation Context Source clears live pull context on ActivePullCleared and CaptureDisabled.
48. Slingshot Presentation Context Source starts Launch Push at elapsed zero on LaunchApplied.
49. Slingshot Presentation Context Source increments LaunchPushElapsedSeconds through injected time while HasLaunchPush is true.
50. Slingshot Presentation Context Source resets active pull and launch-push latch on CaptureEnabled.
51. CaptureDisabled does not end Launch Push by itself.
52. Launch Push presence is not tied to Band Release Recoil completion or clearance.
53. Slingshot Presentation Context Source exposes one immutable current context value with inactive channels zeroed.
54. Slingshot Presentation Context Source does not select Character Presentation Mode.
55. Slingshot Pull Offset Normalizer owns signed effective lateral normalization.
56. Normalized Pull Offset and Normalized Launch Offset use side-specific effective pull limits from current slingshot geometry/config at the relevant pull depth.
57. Effective lateral normalization includes the depth-based lateral ramp.
58. Effective lateral normalization does not divide by raw maximum lateral pull alone.
59. A centered pull maps to 0, full effective left maps to -1, and full effective right maps to 1.
60. SlingshotController and Slingshot Presentation Context Source share the normalizer rather than duplicating lateral math.
61. SlingshotInstaller owns registrations for slingshot notifier interfaces, context source, and pull offset normalizer.
62. GameplayLifetimeScope should not gain serialized scene references for pure slingshot presentation services.
63. Existing launch applied notification remains the accepted-launch edge from the launch controller.
64. Existing slingshot launch physics and band recoil behavior are not changed by this feature.
65. Existing Ladybug collider, Band Center, Character Visual Anchor, and authoritative Launch Target root ownership remain intact.
66. The first implementation slice does not add audio, VFX, root motion, procedural ragdoll, or character abilities.
67. The feature should be implemented as small focused modules with pure C# seams for classifier, context source, and normalizer tests.

## Testing Decisions

Good tests for this feature should assert externally visible behavior at each contract boundary, not private branching details. Tests should verify mode selection, event publication, context read-model state, frame values, container composition, and Animator-facing serialized parameter consistency where practical.

EditMode tests:

1. Character Presentation Mode Classifier returns PullAnticipation when HasActivePull is true, even when the surface would otherwise classify as downhill Slide.
2. Character Presentation Mode Classifier returns LaunchPush when HasLaunchPush is true and LaunchPushElapsedSeconds is below LaunchPushMinimumSeconds, even when downhill slope would otherwise classify as Slide.
3. Character Presentation Mode Classifier returns LaunchPush when HasLaunchPush is true and LaunchPushElapsedSeconds is below LaunchPushMinimumSeconds, even when flat forward motion would otherwise classify as Run.
4. Character Presentation Mode Classifier resumes normal locomotion when LaunchPushElapsedSeconds is at or above LaunchPushMinimumSeconds.
5. Character Presentation Mode Classifier keeps Victory and Defeat above PullAnticipation and LaunchPush.
6. Character Presentation Presenter reads Slingshot Presentation Context Source.Current and forwards HasActivePull, HasLaunchPush, and LaunchPushElapsedSeconds into classification input.
7. Character Presentation Presenter copies NormalizedPull, NormalizedLaunchPower, NormalizedPullOffset, and NormalizedLaunchOffset into Character Presentation Frame.
8. Character Presentation Presenter zeroes inactive slingshot channels according to the selected mode/frame lifecycle.
9. Character Presentation Presenter preserves existing locomotion playback speed behavior.
10. Slingshot Presentation Context Source starts launch push at elapsed 0 on LaunchApplied.
11. Slingshot Presentation Context Source advances launch-push elapsed time through fake injected time.
12. Slingshot Presentation Context Source resets active pull and launch-push latch on CaptureEnabled.
13. Slingshot Presentation Context Source clears active pull on CaptureDisabled without clearing the launch-push latch.
14. Slingshot Presentation Context Source exposes inactive pull and launch channels as zero.
15. Slingshot Active Pull Notifier raises ActivePullChanged for live validated pull updates.
16. Slingshot Active Pull Notifier raises ActivePullCleared through safe invocation for canceled, invalid, or weak pulls that had live validated pull state.
17. Slingshot Active Pull Notifier does not raise clear events for ignored pointer input with no live active pull.
18. Valid launch release preserves pull context until the capture/launch handoff path starts Launch Push.
19. Slingshot Capture Lifecycle Notifier emits CaptureEnabled after enable setup is committed.
20. Slingshot Capture Lifecycle Notifier emits CaptureDisabled once when capture becomes disabled.
21. Slingshot Capture Lifecycle Notifier emits no events for no-op same-state calls.
22. Slingshot Pull Offset Normalizer maps center, full-left, full-right, shallow-ramp, and asymmetric-side-limit cases correctly.
23. Slingshot Pull Offset Normalizer rejects or clamps invalid numeric input according to existing slingshot validation conventions.
24. Container tests resolve Slingshot Active Pull Notifier, Slingshot Capture Lifecycle Notifier, Slingshot Presentation Context Source, and Slingshot Pull Offset Normalizer from the configured container.
25. Container tests verify controller-backed notifier interfaces point to the expected logical slingshot source.
26. Existing Character Presentation and Slingshot tests remain green.

PlayMode tests:

1. Gameplay scene composition resolves the new interfaces without adding extra scene references for pure slingshot presentation services.
2. Ladybug Character view has the required Animator parameter fields wired to the Animator Controller.
3. Scene composition still has one authoritative Launch Target Rigidbody and gameplay collider owner.
4. Character Visual Anchor, Band Center, and gameplay collider ownership remain separated.
5. Existing slingshot band, recoil, run surface, camera, and run-end scene tests remain green unless their expectations explicitly depend on old missing modes.

Manual Unity smoke checks:

1. Pull and hold the band with a valid pull; character enters PullAnticipation and normalized pull values change in the Animator.
2. Pull left and right; NormalizedPullOffset drives the expected lateral blend direction.
3. Release a valid launch; character enters LaunchPush immediately.
4. During LaunchPush, Slide and Run do not take over before the configured minimum guard elapses.
5. After LaunchPush handoff, downhill sections enter Slide and flat forward sections enter Run.
6. Victory and Defeat still override slingshot presentation.
7. No Animator parameter warnings, missing receiver errors, root-motion movement, collider drift, or Band Center movement regressions appear.

Static and tooling checks for implementation:

1. Rider reformat and file-problem checks for changed C# and asmdef files.
2. Unity AI Agent Connector compile before tests.
3. Targeted EditMode tests for classifier, presenter, slingshot context source, active pull notifier, capture lifecycle notifier, and pull offset normalizer.
4. Targeted PlayMode composition tests when scene wiring changes.
5. Changed tests after targeted tests are green.
6. Git diff whitespace check.

## Release and Compatibility

1. Unity version assumption: current Unity 6 project setup remains unchanged.
2. No package version bump is expected unless the repository's release process requires one for gameplay changes.
3. No new third-party package is planned. SaintsField is already part of the current presentation view workflow and can be reused for new Animator parameter fields.
4. No save data migration is needed.
5. No Addressables schema change is needed.
6. No ProjectSettings, render pipeline, input package, signing, authentication, or remote service change is expected.
7. Existing scenes and prefabs will need implementation-time wiring for new Animator parameters and services, but the authoritative gameplay root and physics contracts remain compatible.
8. Backward compatibility risk is mainly serialized enum order and Animator parameter wiring. Add new Character Presentation Mode values intentionally and verify Animator Controller transitions after enum changes.
9. Slingshot notifier additions should be additive interfaces. Existing slingshot launch, capture, and view contracts should continue to behave for existing consumers.
10. Implementation should preserve existing accepted-launch, band recoil, run camera, run surface, and run-end behavior.

## Out of Scope

1. Changing slingshot launch physics, force calculation, or steering.
2. Changing Band Shape solving, Band Release Recoil behavior, or band clearance rules.
3. Retuning the Ladybug gameplay collider, Band Center, camera anchor, or run surface probe.
4. Moving or replacing imported Ladybug assets.
5. Adding audio or VFX playback for Pull Anticipation, Launch Push, or footsteps.
6. Adding character abilities, stats, upgrades, or multiple-character selection.
7. Adding root motion or allowing imported character physics to drive gameplay.
8. Stripping or rewriting imported animation clips.
9. Adding a global event bus.
10. Adding a separate character-specific state machine outside Character Presentation Mode.
11. Exposing raw pointer input, Slingshot View payloads, Band Shape, or touch indicator data to Character Presentation Presenter or View.
12. Using Normalized Pull or Normalized Launch Power as animation elapsed time.
13. Tying Launch Push lifetime to Band Release Recoil completion.
14. Remote issue publishing or implementation work as part of this PRD task.

## Further Notes

The main architectural principle is that the Slingshot owns slingshot interpretation and Character Presentation owns character mode selection. The bridge between them is a small read model, not a view call and not raw input.

The naming is intentionally generic at runtime. Ladybug remains the concrete first character and prefab/asset name, but the runtime system should keep using Character terminology so a future character can reuse the same presentation pipeline.

Animator authoring should be treated as an implementation calibration step. This PRD defines the mode and parameter contract; exact clip choice, blend tree shape, transition durations, and pose offsets should be tuned in Unity against the real Ladybug rig.

The first implementation slice should be narrow: add facts, classification, frame parameters, Animator wiring, and tests. Do not broaden the slice into launch effects, camera changes, or band behavior changes while fixing the launch-push presentation problem.
