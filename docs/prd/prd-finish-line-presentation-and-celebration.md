# PRD: Finish Line Presentation and Celebration

## Problem Statement

The current finish-line experience does not clearly communicate the end of the **Run** or reward the player for reaching it. The player sees a large Ladybug billboard near the final approach, but the actual crossing moment is visually weak: the authoritative **Run Finish** reads as a small pale block, and the billboard itself is currently named and authored like an obstacle blocker.

This creates three problems from the player's perspective:

- The destination is visible, but the exact finish threshold is ambiguous.
- Reaching the end does not feel like a strong completion moment because there is no dedicated **Finish Celebration**.
- The terminal character presentation still reads as if the player is seeing the **Character** from behind, which weakens the **Victory** moment.

From the team's perspective, the current setup also mixes responsibilities. A visual billboard uses obstacle language and obstacle contact behavior, a legacy scene-level finish contact competes with the course-root finish contact, and presentation effects risk being triggered from raw contact events instead of accepted **Run Result** state. The finish needs to be reorganized so **Finish Presentation**, **Run Finish**, **Run Obstacle**, **Finish Celebration**, and **Victory Facing** stay separate and testable.

## Solution

Improve the finish line as a presentation-focused pass while preserving the existing **Run End Flow** authority.

The player should approach a readable **Finish Presentation** hierarchy that contains the visual finish sign and a visible **Finish Threshold Visual** aligned with the authoritative **Run Finish**. The threshold uses the generated checkered transparent texture, opaque near the run surface and fading upward, so it marks the crossing moment without looking like a blocker.

When **Run End Flow** accepts a successful **Run Result**, a dedicated presentation listener plays the **Finish Celebration** using existing Ladybug confetti assets, fades out the **Finish Threshold Visual**, and lets the **Character** enter **Victory** with **Victory Facing**: only the visible **Character** or **Character Visual Anchor** turns toward the **Run Camera**. Gameplay physics, colliders, **Run Body**, **Run End Pose Lock**, camera source, progress source, and surface probes remain unchanged.

Returning to **Run Preparation** resets the finish presentation state and clears **Victory Facing**, so the next **Run** starts from normal **Character Visual Follower** orientation.

## Unity Surfaces

- Runtime assemblies and asmdefs:
  - `Game.Gameplay` remains the owner of **Run End Flow**, **Run Result** acceptance, `IRunResultNotifier.RunResultAccepted`, and **Run Preparation** reset state.
  - Character presentation runtime gains or extends presentation-facing behavior for **Victory Facing** without changing gameplay motion ownership.
  - A small finish-celebration presenter/listener should consume accepted **Run Result** events and drive a passive finish presentation view.
  - Any fade timing or facing timing logic should be plain C# where practical, with MonoBehaviours kept as shallow scene views.
- Editor assemblies, windows, inspectors, importers, or menu items:
  - No new editor window, importer, menu item, or custom tool is required.
  - Existing Unity import settings are enough for the generated transparent texture.
  - Scene and prefab validation can be covered by PlayMode tests rather than custom editor tooling unless repeated authoring mistakes justify it later.
- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:
  - `GameplayScene` is the integration surface for the finish-line hierarchy and serialized references.
  - The course-owned **Run Finish** contact remains the authoritative finish trigger.
  - The legacy scene-level finish contact should be removed or disabled unless compatibility evidence proves it is still required.
  - The current billboard visual should move under a dedicated **Finish Presentation** hierarchy and become visual-only.
  - Any physical blocker/backstop near the finish should be authored as a separate clearly named **Run Obstacle** only if still needed.
  - The generated checkered transparent texture becomes the first-pass **Finish Threshold Visual** asset.
  - Existing Ladybug confetti prefabs are the first-pass **Finish Celebration** source assets.
  - `GameplayLifetimeScope` should register any scene-owned finish presentation view without owning the MonoBehaviour lifecycle.
  - No ProjectSettings or package manifest changes are expected.
- RPC/helper commands, hooks, or shell wrappers:
  - Use `.unity-ai-agent-connector/bin/uaiac compile` as the compile gate after implementation.
  - Run targeted EditMode and PlayMode tests through Unity AI Agent Connector after compilation is clean.
  - No new shell wrapper or RPC contract is required.
- Package versioning, changelog, and installation/sync behavior:
  - This is project gameplay/content work, not a package release.
  - No package version, changelog, Addressables schema, or install/sync behavior change is expected.

## User Stories

1. As a player, I want the finish area to be readable before I reach it, so that I understand where the **Run** ends.
2. As a player, I want the exact crossing moment to be visible, so that completion feels intentional rather than accidental.
3. As a player, I want the finish line to look like a visual threshold instead of a physical wall, so that I know I should cross it.
4. As a player, I want the billboard to act as a destination sign, so that the finish approach has a strong visual landmark.
5. As a player, I want the checkered threshold to fade upward, so that it marks the finish without blocking my view down the course.
6. As a player, I want the threshold to be most opaque near the run surface, so that the crossing plane is readable at speed.
7. As a player, I want the finish threshold to align with the actual **Run Finish**, so that the visual and gameplay result agree.
8. As a player, I want the finish to trigger a celebration after I successfully finish, so that completion feels rewarding.
9. As a player, I want confetti to happen after crossing, not before, so that it reads as a reward rather than a navigation hint.
10. As a player, I want the finish threshold to fade out during celebration, so that the scene transitions cleanly into the victory moment.
11. As a player, I want Ladybug-themed confetti to play at the finish, so that the celebration matches the level theme.
12. As a player, I want the **Character** to face the camera during **Victory**, so that the end presentation feels like a real victory pose.
13. As a player, I want the camera-facing turn to blend with the beginning of **Victory**, so that it feels intentional rather than snapped.
14. As a player, I want the next **Run** to start normally after returning to **Run Preparation**, so that victory presentation does not leak into replay.
15. As a designer, I want **Finish Presentation** to be a dedicated hierarchy, so that finish visuals are easy to find and tune.
16. As a designer, I want the billboard to be visual-only, so that finish dressing does not accidentally behave like an obstacle.
17. As a designer, I want any real finish blocker to be a separate **Run Obstacle**, so that physical contact behavior is explicit.
18. As a designer, I want one authoritative course-owned **Run Finish**, so that finish behavior is not split across competing triggers.
19. As a designer, I want **Finish Threshold Visual** to be a reusable term, so that finish-line texture, crossing line, and checkered stripe discussions stay precise.
20. As a designer, I want the threshold texture to be editable as an asset, so that opacity, scale, and material feel can be tuned visually.
21. As a designer, I want confetti anchors to be scene-authored, so that celebration framing can be tuned without code changes.
22. As a designer, I want confetti to be one-shot, so that the finish approach stays readable and does not look permanently completed.
23. As a designer, I want fireworks and looping star effects deferred, so that the first pass stays focused and readable.
24. As a designer, I want the finish billboard, threshold, and confetti to be organized together, so that the full finish presentation is easy to review.
25. As a technical artist, I want the checkered texture to import with alpha transparency, so that it can be placed as a plane or sprite without chroma-key artifacts.
26. As a technical artist, I want the threshold material to support fading, so that the celebration can fade the line out cleanly.
27. As a technical artist, I want the threshold to avoid casting gameplay-looking collision shadows, so that it remains visual-only.
28. As a technical artist, I want the billboard mesh/material reuse to avoid inherited obstacle behavior, so that asset reuse does not imply gameplay reuse.
29. As a gameplay programmer, I want **Run End Flow** to remain the only authority that accepts **Run Result**, so that finish celebration cannot grant completion itself.
30. As a gameplay programmer, I want **Finish Celebration** to listen to accepted **Run Result**, so that rejected or lower-priority contact candidates do not play celebration.
31. As a gameplay programmer, I want celebration to require `result.IsSuccess`, so that obstacle hits, out-of-bounds, and lost momentum never play victory confetti.
32. As a gameplay programmer, I want contact classification to stay free of particle playback, so that collision logic remains deterministic and testable.
33. As a gameplay programmer, I want **RunContactClassifier** to keep mapping **Run Finish** to `Finished`, so that existing end-flow priority behavior remains intact.
34. As a gameplay programmer, I want the legacy finish trigger removed or disabled, so that one crossing produces one authoritative result path.
35. As a gameplay programmer, I want **Victory Facing** to rotate only presentation transforms, so that physics, steering, progress, camera source, and contact shape are not affected.
36. As a gameplay programmer, I want **Victory Facing** reset on **Run Preparation**, so that future runs do not inherit terminal orientation.
37. As a gameplay programmer, I want **Victory Facing** integrated with existing **Character Presenter** and **Character Visual Follower** ownership, so that there is no second competing visual pose loop.
38. As a gameplay programmer, I want scene views registered through VContainer, so that the implementation follows existing composition patterns.
39. As a gameplay programmer, I want any timing logic to be testable without relying on Unity lifecycle callbacks where practical, so that fade and reset behavior is deterministic.
40. As a tester, I want EditMode tests for accepted-result celebration gating, so that only successful accepted results play **Finish Celebration**.
41. As a tester, I want EditMode tests for reset behavior, so that **Run Preparation** restores threshold visibility and stops any active celebration state.
42. As a tester, I want tests that **Victory Facing** does not rotate the **Run Body**, so that visual presentation cannot corrupt gameplay pose.
43. As a tester, I want tests that **Victory Facing** activates only for successful **Run Result**, so that failed results keep **Defeat** behavior.
44. As a tester, I want scene tests that there is exactly one authoritative course-owned **Run Finish**, so that duplicate triggers do not regress.
45. As a tester, I want scene tests that the old billboard presentation object does not carry **Run Obstacle** contact behavior, so that visual dressing stays safe.
46. As a tester, I want scene tests that **Finish Threshold Visual** has no collider or **RunContact**, so that the crossing texture cannot end or block a run.
47. As a tester, I want scene tests that finish confetti references are present, so that missing particle references fail fast.
48. As a tester, I want scene tests that required finish presentation references are serialized, so that VContainer composition is reliable.
49. As a QA engineer, I want a manual camera-framing pass on the finish approach, so that the billboard, threshold, confetti, and victory pose are visible together.
50. As a QA engineer, I want a manual no-finish failure pass, so that confetti and **Victory Facing** do not trigger for failed runs.
51. As a QA engineer, I want a manual successful finish pass, so that the threshold fades, confetti plays, and the character faces camera at the right time.
52. As a producer, I want this pass limited to finish presentation and victory readability, so that it does not reopen level-layout, economy, or progression tuning scope.
53. As a producer, I want the generated texture treated as first-pass art, so that implementation can proceed while still allowing later art replacement.
54. As a maintainer, I want the domain glossary updated, so that future finish-line changes use the same language.
55. As a maintainer, I want no new package dependency, so that the project remains easy to build and test.

## Implementation Decisions

- **Finish Presentation** is the level term for readable finish dressing such as the billboard, threshold visual, and celebration anchors.
- **Run Finish** remains the gameplay contact term and stays separate from **Finish Presentation**.
- The course-owned finish contact is the authoritative **Run Finish** for the final approach.
- The legacy scene-level finish contact should be removed or disabled unless implementation finds a concrete compatibility dependency.
- The billboard currently used near the finish becomes visual-only **Finish Presentation**.
- The billboard may reuse its current mesh and material, but it must not retain inherited **Run Obstacle** collider/contact behavior.
- Any real physical backstop or blocker near the finish must be a separate clearly named **Run Obstacle** object.
- The finish visual objects should be grouped under a dedicated **Finish Presentation** hierarchy root.
- **Finish Threshold Visual** is the visual-only crossing mark aligned with authoritative **Run Finish**.
- The first-pass **Finish Threshold Visual** uses the generated transparent checkered texture.
- The threshold should be strongest near the run surface and fade upward to transparency.
- The threshold must not own **Run Finish** behavior, collision, reward logic, or contact classification.
- The threshold may fade out during **Finish Celebration**.
- The threshold must reset for **Run Preparation**.
- **Finish Celebration** is one-shot presentation feedback after an accepted successful **Run Result** from **Run Finish**.
- **Finish Celebration** must not run before crossing.
- **Finish Celebration** must be driven by accepted **Run Result**, not raw trigger entry or raw contact classification.
- **Finish Celebration** should use existing Ladybug confetti assets first.
- Fireworks, stars, and looping effects remain fallback or later polish only if confetti reads too weak in the run camera.
- **Finish Celebration** playback should be owned by a dedicated scene-authored presentation view/listener under **Finish Presentation**.
- The finish-celebration presenter should subscribe to `IRunResultNotifier.RunResultAccepted` or an equivalent accepted-result boundary already exposed by **Run End Flow**.
- The presenter should play celebration only when the accepted result is successful.
- Contact classification and **Run End Flow** should not directly play particles or fade visuals.
- Scene-owned finish presentation views should be registered through VContainer without treating their MonoBehaviours as owned services.
- Any fade controller should expose a small, stable view interface such as play, fade out, reset, and stop/clear.
- **Victory Facing** is a presentation-only orientation override for successful **Victory**.
- **Victory Facing** may rotate the visible **Character** or **Character Visual Anchor** toward the **Run Camera**.
- **Victory Facing** must not rotate the **Run Body**, Rigidbody, colliders, camera source, progress source, surface probes, or **Run End Pose Lock** target.
- **Victory Facing** must be driven by accepted successful **Run Result**, not raw **Run Finish** contact entry.
- **Victory Facing** should blend through the start of **Victory** presentation instead of snapping as a separate correction.
- **Victory Facing** must reset when returning to **Run Preparation**.
- The implementation should follow ADR-0002 by keeping gameplay/presentation decisions in plain C# controllers where practical and MonoBehaviours shallow.
- The implementation should follow ADR-0003 by using direct C# events rather than introducing an event bus.
- The implementation should follow ADR-0005 by composing services and presenters with VContainer.
- The implementation should follow ADR-0006 by registering scene-owned views without injecting or owning MonoBehaviour lifecycle.
- The implementation should follow ADR-0009 by treating the existing run camera as the camera source for victory-facing calculations.
- No new ADR is required unless implementation introduces a broader finish-presentation framework, save-data changes, or a new scene-authoring pipeline.

## Testing Decisions

- Good tests for this feature assert observable finish-line behavior and authoring contracts, not exact visual polish.
- Tests should protect ownership boundaries: **Run Finish** completes the run, **Finish Presentation** displays visuals, and **Finish Celebration** reacts to accepted success.
- EditMode tests are appropriate for pure presenter/controller behavior that listens to accepted results and drives a passive view.
- EditMode tests should cover successful accepted result starts **Finish Celebration**.
- EditMode tests should cover failed accepted results do not start **Finish Celebration**.
- EditMode tests should cover duplicate accepted results do not create duplicate one-shot celebration playback if the run is already terminal.
- EditMode tests should cover **Run Preparation** reset restores the threshold visual and stops or clears celebration state.
- EditMode tests should cover threshold fade timing if fade interpolation is implemented outside a MonoBehaviour view.
- EditMode tests should cover **Victory Facing** activation for successful accepted result.
- EditMode tests should cover **Victory Facing** not activating for failed accepted result.
- EditMode tests should cover **Victory Facing** reset on **Run Preparation**.
- EditMode tests should cover **Victory Facing** output as a presentation pose or orientation value rather than mutating gameplay transforms directly.
- PlayMode scene tests should cover that the gameplay scene has exactly one authoritative course-owned **Run Finish** contact in the finish approach.
- PlayMode scene tests should cover that any legacy scene-level duplicate **Run Finish** is gone or disabled.
- PlayMode scene tests should cover the billboard presentation object does not carry **RunContact** obstacle behavior.
- PlayMode scene tests should cover the **Finish Threshold Visual** object has no collider and no **RunContact**.
- PlayMode scene tests should cover finish presentation view references are serialized and registered in gameplay composition.
- PlayMode scene tests should cover confetti particle references are present and inactive before accepted success if they are scene-authored.
- PlayMode scene tests should cover the threshold visual is visible before crossing and reset after returning to **Run Preparation**.
- PlayMode scene tests should cover the end-to-end successful finish path reaches **Run Ended**, starts celebration, and leaves **Run End Flow** as the accepted-result authority.
- Existing composition tests for character presentation should be extended rather than duplicated where possible.
- Existing run-course scene assertions should be extended to cover finish contact authority and finish presentation authoring.
- Manual Unity smoke checks should verify camera framing of the billboard, threshold, confetti, and **Victory Facing** on desktop and representative mobile aspect ratios.
- Manual Unity smoke checks should verify the generated threshold texture has no visible chroma-key fringe once placed in the scene.
- Manual Unity smoke checks should verify confetti reads as celebration without obscuring the result UI or character victory pose.
- Compile gate must run before tests.
- Targeted tests should run only after compile is clean.

## Release and Compatibility

- Unity version assumption: current project Unity 6 setup remains unchanged.
- No package dependency, package version, or package manifest change is expected.
- No Addressables schema change is expected.
- No save-data or persistence migration is expected.
- Scene serialization changes are expected because finish hierarchy, references, and visual authoring change.
- Texture import metadata is expected for the generated transparent checkered threshold asset.
- If the billboard is converted to a prefab variant or scene override, prefab references must keep visual reuse without inheriting obstacle contact behavior.
- Removing or disabling the legacy finish trigger is behaviorally safe only if tests confirm the course-owned **Run Finish** remains authoritative.
- Backward compatibility risk is mostly scene-authoring risk: missing serialized references, duplicate contacts, or unintended colliders on visual objects.
- The generated texture is first-pass art and can be replaced later as long as the **Finish Threshold Visual** contract remains the same.

## Out of Scope

- Rebuilding the full Ladybug Rooftop Half-Tube course layout.
- Changing **Run End Flow** result priority rules.
- Changing reward grant timing, economy balance, or upgrade progression.
- Adding next-level routing, campaign completion, or new post-victory screens.
- Creating a generalized finish-line authoring framework for multiple courses.
- Introducing new particle packages or package dependencies.
- Final art polish for the billboard, texture, confetti, or victory animation.
- Haptics, audio stingers, analytics, telemetry, or camera cutscenes.
- Replacing Cinemachine run camera behavior.
- Rotating the gameplay body or changing physics behavior for the victory pose.
- Persisting any finish presentation state.
- Creating new imported character or animation assets unless existing **Victory** cannot support the facing blend.

## Further Notes

- Current inspection found the billboard named like an obstacle blocker and sourced from an obstacle billboard prefab with solid collider/contact behavior. The implementation should preserve useful visuals while separating gameplay contact roles.
- Current inspection found the authoritative course-root finish contact and a legacy scene-level finish contact. The PRD assumes the course-root contact should win.
- Current inspection found no reusable checkered finish-line texture, so a first-pass transparent texture was generated and saved for the Ladybug Rooftop Half-Tube.
- Current inspection found `IRunResultNotifier.RunResultAccepted` already exists and is used by result UI and character presentation, making it the natural source for **Finish Celebration** and **Victory Facing**.
- The first-pass generated threshold texture is intentionally accepted as a trial asset, not final art.
- A later implementation task should include a Unity scene view/camera screenshot pass because finish readability is ultimately visual and camera-dependent.
