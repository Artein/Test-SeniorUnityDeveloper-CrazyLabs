# PRD: Slide-Only Character Presentation

## Problem Statement

The game fantasy is momentum-driven sliding down a Run Surface. The current Character Presentation language still treats grounded movement as a split between Slide and Run. That split makes the visible Character switch between two locomotion ideas based on small slope, surface-normal, and speed differences that are not meaningful to the player.

The result is a presentation mismatch. A player can see the Character visibly run during a game about sliding, or flicker between Run and Slide on a surface that feels smooth. Even when the physics are valid, the animation implies powered movement instead of gravity and momentum. On flat or nearly flat sections, visible running also communicates the wrong cause of motion. The Launch Target is coasting or losing speed through physics, not choosing to run.

This PRD changes normal grounded Character Presentation so that meaningful grounded movement is shown as Slide. Idle remains the grounded stopped or stalled presentation. Run remains only as a reserved Character Presentation Mode for compatibility with existing enum values, Animator wiring, and serialized assets.

This PRD supersedes the part of the earlier Ladybug Character Presentation PRD that selected Run for flat forward movement. It does not supersede the architecture: the Rigidbody-backed Launch Target remains gameplay truth, and the Character remains presentation only.

## Solution

Make Slide the only normal grounded locomotion Character Presentation Mode.

The Character Presentation Mode Classifier should select Slide whenever the Character is grounded on a Run Surface and has Meaningful Grounded Movement. Meaningful Grounded Movement is based on Course Planar Speed, not Course Forward Speed. This means downhill, flat, sideways, and briefly backward grounded motion can all remain Slide when the Launch Target is still meaningfully moving across the Run Surface.

The classifier should select Idle when the Character is grounded but movement is not meaningful. This covers true stopped, stalled, or barely crawling grounded cases. Idle should feel like the Character is no longer actively sliding.

The classifier should not use downhill slope thresholds to choose between Slide and Run. Slope can remain available through Run Surface Context and Run Surface Slope Calculator for diagnostics or future Slide Flavor work, but it is not part of canonical Character Presentation Mode selection for this change.

Run should stay in the Character Presentation Mode enum and Animator-compatible assets as a Reserved Presentation Mode. Normal runtime classification must not emit Run. Compatibility handling may map unexpected Run frames to Slide until a later cleanup removes legacy Animator state usage.

No new Coast Character Presentation Mode is introduced. Coast is descriptive flavor inside Slide. The first implementation uses the existing Slide presentation for all Meaningful Grounded Movement.

## Unity Surfaces

Runtime surfaces:

1. Character Presentation Mode Classifier.
2. Character Presentation Classification Input.
3. Character Presentation Classification Result.
4. Character Presentation Tuning owned by the Character Presentation View.
5. Character Presenter mode-memory and playback-speed coordination.
6. Character Presentation Frame creation.
7. Character Presentation View frame application.
8. Character Presentation Mode enum compatibility.
9. Course Planar Speed and Course Forward Speed facts from the run-progress/motion path.
10. Run Surface Context and grounded/ungrounded facts.
11. Run Surface Slope Calculator only as diagnostics or future Slide Flavor support.
12. Launch Push and Pull Anticipation presentation facts.
13. Accepted Run Result notification for Victory and Defeat.

Scene, prefab, and asset surfaces:

1. Gameplay Scene Character wiring.
2. Project-owned Ladybug Character prefab.
3. Character Presentation View serialized tuning fields.
4. Ladybug Character Animator Controller.
5. Existing Slide animation state and clip wiring.
6. Existing Run animation state kept only for compatibility.
7. Animator integer mode parameter values.
8. Animator playback speed parameter wiring.
9. Any prefab or scene YAML that currently stores old run-forward or slope-threshold presentation tuning.
10. Existing Character Visual Anchor and Launch Target hierarchy.

Test surfaces:

1. Character Presentation Mode Classifier EditMode tests.
2. Character Presenter EditMode tests.
3. Character Presentation View Unity-level tests.
4. Gameplay Scene composition PlayMode tests.
5. Animator parameter consistency checks where practical.
6. Any existing tests that currently assert flat forward movement becomes Run.
7. Any existing tests that rely on Run Reference Speed as normal locomotion tuning.

Documentation surfaces:

1. Character Presentation context glossary.
2. Existing Ladybug Character Presentation PRD references that imply flat Run is normal.
3. Future implementation issues generated from this PRD.

## User Stories

1. As a player, I want the Character to look like it is sliding during normal grounded movement so that the animation matches the game fantasy.
2. As a player, I want flat coasting to still look like sliding so that the Character does not appear to power itself forward.
3. As a player, I want the Character not to switch between running and sliding on a smooth-feeling slope so that the motion feels consistent.
4. As a player, I want visible locomotion changes to happen for understandable reasons so that animation state changes feel logical.
5. As a player, I want the Character to become Idle only when it has really stopped or stalled so that slow crawl states do not linger as fake action.
6. As a player, I want a weak launch that barely moves to stop looking active quickly so that failed attempts feel resolved.
7. As a player, I want a successful downhill launch to read as one sliding action so that the run has a clear visual identity.
8. As a player, I want sideways grounded drift to still look like sliding so that bumps and steering corrections do not create strange running.
9. As a player, I want brief backward grounded movement to still look like sliding when the body has momentum so that collisions do not create a false run animation.
10. As a player, I want a true stall to become Idle even on a downhill surface so that lack of movement is communicated honestly.
11. As a player, I want short terrain gaps not to immediately interrupt Slide so that small ground-probe gaps do not create animation noise.
12. As a player, I want real airborne moments to still become Airborne so that jumps and drops remain readable.
13. As a player, I want pull preparation to remain visually distinct so that slingshot aiming is still clear.
14. As a player, I want the launch-push moment to remain visually distinct so that release feedback still feels immediate.
15. As a player, I want victory presentation to remain stronger than locomotion so that finishing a run feels final.
16. As a player, I want defeat presentation to remain stronger than locomotion so that failure feels final.
17. As a player, I want animation speed to scale with how fast I am sliding so that fast and slow motion remain believable.
18. As a player, I want slow coasting to look calmer through playback speed rather than switching to running so that the character still belongs to the sliding game.
19. As a player, I want the visible Character to stay attached to the Launch Target so that the physics body still feels like the object I control.
20. As a player, I want the same controls and physics feel after this change so that only the presentation language changes.
21. As a designer, I want one normal grounded locomotion mode so that authored levels do not need slope thresholds to avoid animation chatter.
22. As a designer, I want Meaningful Grounded Movement to use Course Planar Speed so that visual locomotion matches actual surface movement.
23. As a designer, I want Course Forward Speed to remain available for progress, rewards, and failure logic so that presentation does not corrupt gameplay metrics.
24. As a designer, I want the Meaningful Grounded Movement Threshold to be explicit so that barely moving grounded cases can become Idle.
25. As a designer, I want the old run-forward threshold name removed so that tuning vocabulary matches the new behavior.
26. As a designer, I want slope thresholds removed from canonical mode selection so that tiny normal changes cannot flip the Character between modes.
27. As a designer, I want future slope-based variation to be named Slide Flavor so that it does not recreate Run as normal locomotion.
28. As a designer, I want Coast to stay descriptive rather than become a new mode so that the first change remains small and testable.
29. As a designer, I want Slide Reference Speed to control normal grounded locomotion playback so that there is one calibration point.
30. As a designer, I want Run Reference Speed removed from normal tuning so that old vocabulary does not guide future tuning incorrectly.
31. As a designer, I want Run to remain compatible in existing assets so that this change does not force an Animator cleanup before the behavior improves.
32. As a designer, I want the existing Slide clip to carry the first implementation so that no new animation dependency blocks the design change.
33. As a designer, I want a later visual pass to be able to add calmer Coast flavor inside Slide so that the current decision does not block polish.
34. As a technical artist, I want the Animator mode values preserved so that existing serialized controller wiring does not break.
35. As a technical artist, I want the Run Animator state kept until a dedicated cleanup so that asset churn is controlled.
36. As a technical artist, I want unexpected Run frames to display as Slide when possible so that compatibility does not leak to players.
37. As a technical artist, I want playback speed to remain clamped so that very low or very high physics speeds do not produce ugly animation playback.
38. As a technical artist, I want root motion to remain disabled so that the imported Character never moves the authoritative Launch Target.
39. As a gameplay engineer, I want Character Presentation to remain a pure presentation layer so that Rigidbody physics remains the only movement truth.
40. As a gameplay engineer, I want the classifier to stay pure so that mode-selection behavior is cheap to test.
41. As a gameplay engineer, I want terminal modes to keep highest priority so that Run Result presentation cannot be overwritten by locomotion.
42. As a gameplay engineer, I want Pull Anticipation and Launch Push to keep priority over grounded Slide so that slingshot feedback remains intact.
43. As a gameplay engineer, I want inactive or pre-launch gameplay to remain Idle unless pull presentation owns the mode so that the character does not slide before launch.
44. As a gameplay engineer, I want short ungrounded debounce to preserve Slide rather than Run so that compatibility does not reintroduce visible running.
45. As a gameplay engineer, I want confirmed ungrounded state to select Airborne so that the simplified grounded rule does not hide real flight.
46. As a gameplay engineer, I want current-mode memory not to preserve Run as locomotion so that old hysteresis cannot keep the wrong presentation alive.
47. As a gameplay engineer, I want minimum locomotion duration to apply only where it prevents Slide/Idle chatter so that it does not preserve Run.
48. As a gameplay engineer, I want the view contract to remain frame-based so that the view does not gain gameplay classification logic.
49. As a gameplay engineer, I want scene composition tests to cover new tuning names so that serialized prefabs do not regress to old Run language.
50. As a gameplay engineer, I want no new package or Unity version change so that the implementation stays low-risk.
51. As a gameplay engineer, I want no CharacterController migration so that movement architecture remains unchanged.
52. As a gameplay engineer, I want no save-data migration so that this remains a presentation tuning and serialization cleanup.
53. As a QA engineer, I want tests proving flat grounded movement becomes Slide so that the main behavior is protected.
54. As a QA engineer, I want tests proving downhill grounded movement becomes Slide so that the previous downhill behavior remains.
55. As a QA engineer, I want tests proving sideways grounded movement becomes Slide when planar speed is meaningful so that steering and bump cases are covered.
56. As a QA engineer, I want tests proving backward grounded movement becomes Slide when planar speed is meaningful so that signed forward progress does not drive presentation.
57. As a QA engineer, I want tests proving grounded movement below the threshold becomes Idle so that crawl cutoff behavior is protected.
58. As a QA engineer, I want tests proving slope angle changes alone do not switch modes so that old slide/run chatter cannot return.
59. As a QA engineer, I want tests proving normal classifier paths do not return Run so that reserved compatibility stays reserved.
60. As a QA engineer, I want tests proving Victory and Defeat still override Slide so that terminal presentation stays correct.
61. As a QA engineer, I want tests proving Pull Anticipation and Launch Push still override Slide so that launch presentation stays correct.
62. As a QA engineer, I want tests proving short ungrounded gaps preserve Slide so that ground-probe noise remains stable.
63. As a QA engineer, I want tests proving long ungrounded gaps become Airborne so that real jumps remain visible.
64. As a QA engineer, I want tests proving unexpected Run compatibility maps to Slide or Slide Reference Speed so that old assets cannot leak visible running.
65. As a QA engineer, I want PlayMode composition checks for serialized tuning names so that scenes and prefabs are updated with the clean rename.
66. As a QA engineer, I want existing Run-specific tests either removed or rewritten so that tests describe the new product truth.
67. As a producer, I want this shipped as a focused presentation behavior change so that movement tuning work can remain separate.
68. As a producer, I want compatibility risks called out clearly so that asset cleanup can be planned separately.
69. As a producer, I want no new animation requirement for the first slice so that implementation is not blocked by content production.
70. As a producer, I want future Coast polish left as optional scope so that this change can land before a visual polish pass.

## Implementation Decisions

The authoritative gameplay body remains the Launch Target. This change must not move Rigidbody ownership, collider ownership, slingshot launch behavior, run progress, steering, or run-end logic into Character Presentation.

The Character Presentation Mode enum should keep its existing values and ordering. Run remains a Reserved Presentation Mode. Keeping the value avoids unnecessary Animator and serialized asset churn. Normal runtime classification stops selecting Run.

Grounded classification should use Course Planar Speed against Meaningful Grounded Movement Threshold. Course Forward Speed must not decide whether grounded movement is Slide. Course Forward Speed remains available to progress, failure, reward, or other gameplay logic that needs signed forward movement.

The normal grounded rule is:

1. Grounded with Course Planar Speed at or above Meaningful Grounded Movement Threshold selects Slide.
2. Grounded with Course Planar Speed below Meaningful Grounded Movement Threshold selects Idle.
3. Slope does not select Run, Slide, or Idle.

The classifier priority should remain presentation-specific and deterministic:

1. Accepted terminal Run Result selects Victory or Defeat.
2. Active Pull selects Pull Anticipation.
3. Active Launch Push selects Launch Push while its guard owns presentation.
4. Inactive, pre-launch, or otherwise non-running gameplay without stronger presentation selects Idle.
5. Confirmed ungrounded state selects Airborne.
6. Short ungrounded gaps may preserve Slide to avoid flicker.
7. Grounded movement selects Slide or Idle by Meaningful Grounded Movement.

Existing ungrounded debounce stays useful. During short ungrounded gaps, preserving the previous locomotion mode should preserve Slide. If previous mode is Run, compatibility should normalize it to Slide rather than keep visible running alive.

MinimumLocomotionModeDuration may stay as anti-chatter tuning for visible locomotion, but it must not preserve Run. It should only prevent undesirable Slide/Idle flicker and must not override terminal modes, Pull Anticipation, Launch Push, inactive/pre-launch Idle, or confirmed Airborne behavior.

Slide Reference Speed becomes the normal grounded locomotion playback reference. Playback speed for Slide should continue to use Course Planar Speed divided by Slide Reference Speed, with the existing clamp behavior. Run Reference Speed should be removed from normal tuning or treated only as compatibility fallback to Slide Reference Speed.

The clean tuning rename should happen in place. The old run-forward presentation threshold should become Meaningful Grounded Movement Threshold. No migration shim and no FormerlySerializedAs attribute are required for this change. Existing project assets and scenes must be updated in the same implementation slice.

Old slope threshold tuning should be removed from canonical mode selection. If slope-related fields remain for a future visual variation pass, they must be renamed as Slide Flavor Tuning and left unwired from Character Presentation Mode selection until a dedicated Slide Flavor implementation uses them.

The Character Presenter should continue to own presentation memory and pass one Character Presentation Frame to the Character Presentation View. It should not push raw gameplay state, raw surface details, or Run Result reasons into the view.

The Character Presentation View should remain passive. It applies the selected mode and playback speed to Animator parameters and guards against root motion. It should not decide whether movement is Slide, Idle, Airborne, Victory, Defeat, Pull Anticipation, or Launch Push.

Unexpected Run compatibility can be handled at the presenter or view boundary. The implementation should choose the smallest boundary that prevents visible Run in normal play while keeping tests explicit. A practical first rule is that any unexpected Run frame uses Slide visual behavior and Slide Reference Speed.

The Run Surface Slope Calculator should stay available for diagnostics and future Slide Flavor. This change should not delete slope math just because canonical mode selection no longer uses it.

Documentation should use Character Presentation vocabulary consistently. Gameplay Run, Running state, Run Surface, Run Result, and Run Progress keep their existing gameplay meanings. Only presentation-level visible running changes.

## Testing Decisions

EditMode classifier coverage should be updated first. Existing tests that expect grounded flat forward movement to select Run should be rewritten to expect Slide when Course Planar Speed is meaningful.

Classifier tests should prove all normal grounded movement directions use Course Planar Speed:

1. Flat forward grounded movement above threshold selects Slide.
2. Downhill grounded movement above threshold selects Slide.
3. Uphill grounded movement above threshold selects Slide if the Launch Target is still meaningfully moving on a Run Surface.
4. Sideways grounded movement above threshold selects Slide.
5. Backward grounded movement above threshold selects Slide.
6. Grounded movement below threshold selects Idle.
7. Zero planar speed selects Idle.

Classifier tests should prove slope no longer decides the mode:

1. Mild downhill above movement threshold selects Slide.
2. Steep downhill above movement threshold selects Slide.
3. Flat above movement threshold selects Slide.
4. Banked or noisy surface normal above movement threshold still selects Slide if grounded support is valid.
5. Changing only slope while speed and grounded state stay stable does not produce Run.

Classifier tests should prove priority is preserved:

1. Victory overrides grounded Slide.
2. Defeat overrides grounded Slide.
3. Pull Anticipation overrides grounded Slide.
4. Launch Push overrides grounded Slide during its guard.
5. Inactive or pre-launch state selects Idle unless pull presentation owns the mode.
6. Confirmed ungrounded state selects Airborne.
7. Short ungrounded gap preserves Slide.

Classifier tests should include a broad guard that normal runtime paths do not return Run. This guard should cover grounded flat, grounded downhill, grounded sideways, grounded backward, short ungrounded preservation, and post-hold transitions. Special compatibility tests may still construct Run as current input to verify it normalizes correctly.

Presenter tests should prove mode memory does not preserve Run as normal locomotion. If the classifier or a legacy test double produces Run, the presenter or view compatibility boundary should apply Slide behavior according to the chosen implementation.

Presenter playback tests should prove Slide Reference Speed is used for normal grounded locomotion playback. Existing Run Reference Speed tests should be removed, renamed, or converted to compatibility fallback tests.

View tests should prove Animator parameter application still works after the tuning rename. Root-motion guard tests should remain unchanged.

Scene composition PlayMode tests should verify the project-owned Ladybug Character prefab and Gameplay Scene store the new Meaningful Grounded Movement Threshold and Slide Reference Speed tuning. Tests should not assert old run-forward or slope-threshold tuning as normal mode-selection fields.

Existing PlayMode composition checks for Character Visual Anchor, Launch Target Rigidbody ownership, collider separation, Band Center separation, Animator references, and root-motion safety should remain in force.

Verification should use the Unity AI Agent Connector workflow for implementation work:

1. Run Unity compile first after code or asset changes.
2. Fix compile errors before running tests.
3. Run targeted EditMode Character Presentation classifier and presenter tests.
4. Run targeted PlayMode scene composition tests when scene, prefab, or Animator assets change.
5. Run broader changed tests if the connector reports additional affected tests.

This PRD itself is documentation-only. Creating the PRD does not require a Unity compile.

## Release and Compatibility

This change is intended for the current Unity 6 project setup and current project-owned Ladybug Character assets.

Run remains in the Character Presentation Mode enum as a Reserved Presentation Mode. Do not reorder enum values as part of this change. Reordering creates avoidable risk for Animator parameters, serialized tests, and compatibility assumptions.

The implementation intentionally performs a clean in-place serialized tuning rename. No FormerlySerializedAs attribute and no migration shim are required. This means the implementation must update current project scenes, prefabs, tests, and authored assets in the same change.

Older scene or prefab copies from another branch that still use old field names may require manual authoring updates after merge. That is an accepted cost of the requested clean update.

The existing Run Animator state may stay in the Animator Controller. It is compatibility surface, not product behavior. A later cleanup can remove or repurpose it once no serialized or tooling dependency remains.

This change should not affect save data, Addressables schema, package dependencies, Unity version, input bindings, slingshot launch physics, Rigidbody damping, terrain friction, run-end conditions, coin pickups, rewards, or camera behavior.

Manual playtest should focus on whether flat coasting and downhill movement now read as one consistent sliding fantasy, whether true stalls become Idle quickly enough, and whether launch/pull/airborne/terminal presentation still feel clear.

## Out of Scope

1. Replacing Rigidbody movement with CharacterController.
2. Changing slingshot impulse, terrain friction, damping, lost-momentum thresholds, or upgrade balance.
3. Adding a new Coast Character Presentation Mode.
4. Adding a new Coast animation clip, blend tree, or Animator layer.
5. Adding Slide Flavor based on slope, bank angle, steering, or speed.
6. Removing the Run enum value.
7. Reordering Animator mode integer values.
8. Deleting the Run Animator state or imported Run clips.
9. Redesigning Run Surface Context, support filtering, or ground probing.
10. Changing Run Progress, Run Result, Run End Flow, rewards, or failure detection.
11. Renaming gameplay Run, Running state, Run Surface, Run Progress, or Run Result language.
12. Adding VFX, audio, dust, lean, or one-shot Character Presentation Cues.
13. Changing Character Visual Anchor, Band Center, or Launch Target hierarchy ownership.
14. Changing root-motion policy.
15. Adding packages, changing Unity version, or changing Addressables structure.
16. Adding save-data migration.
17. Publishing remote issues or PR metadata from this PRD.

## Further Notes

Assumptions:

1. The current Slide clip is acceptable as the first visual for all Meaningful Grounded Movement.
2. The Character remains presentation-only and never becomes the physics authority.
3. The existing Character Presentation Mode enum and Animator mode parameter values are compatibility-sensitive.
4. The user explicitly prefers a clean in-place rename with no FormerlySerializedAs and no migration shim.
5. The old flat Run behavior in the Ladybug Character Presentation PRD is superseded by this PRD.
6. Meaningful Grounded Movement Threshold can start from the current run-minimum-speed tuning value unless playtest suggests a different threshold.
7. Slide Reference Speed can keep the current normal locomotion reference value unless visual calibration suggests a different value.

Resolved product decisions:

1. Normal grounded locomotion is Slide, not Run.
2. Coast is a flavor word inside Slide, not a Character Presentation Mode.
3. Idle means grounded stopped or stalled with no meaningful movement.
4. Course Planar Speed drives Slide eligibility.
5. Course Forward Speed does not drive presentation locomotion eligibility.
6. Slope thresholds do not decide canonical mode selection.
7. Run remains reserved for compatibility and should not be emitted by normal runtime classification.
8. No new animation asset is required for the first implementation.

Unresolved implementation details:

1. Whether unexpected Run normalization should live in the Character Presenter or Character Presentation View boundary.
2. Whether MinimumLocomotionModeDuration should still delay Slide-to-Idle transitions, and what exact value feels best after Run removal.
3. Whether the first implementation should fully remove old slope tuning fields or keep renamed, unwired Slide Flavor Tuning for future use.
4. Whether current Gameplay Scene and Ladybug Character prefab serialized values should keep the existing numeric threshold seeds or be retuned in the same implementation slice.
5. Whether a later Animator cleanup should delete the Run state, map it permanently to Slide, or keep it as a debug-only compatibility state.
