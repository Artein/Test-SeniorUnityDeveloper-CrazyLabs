# PRD: Gameplay Config Authoring Guidance

## Problem Statement

This project is a test project where the reviewer needs to understand the gameplay tuning surface quickly. Today, many serialized gameplay config values are visible in the Unity Inspector, but their purpose, impact, units, and safe adjustment direction are not consistently explained. A reviewer can see a number, but may still need to inspect runtime code, validators, assets, and glossary docs to decide whether the value should be tweaked and what might happen if it changes.

The issue is strongest on core gameplay tuning assets that shape **Slingshot**, **Launch Impulse**, **Run Steering Control**, **Run Camera**, and **Run End Flow** behavior. Some values already have short tooltips, while nearby values have none. Existing tuned ScriptableObject assets also differ from several code defaults, so copying exact current asset values into C# tooltip text would create stale documentation risk.

The project needs **Authoring Guidance**: reviewer-facing explanations attached to tweakable serialized values so their purpose, impact, units, and safe adjustment range are understandable in the Unity Inspector. The first pass should improve scanability without changing gameplay behavior, serialization, constraints, or architecture.

## Solution

Add focused **Authoring Guidance** to the first-pass gameplay config surface using Unity Inspector metadata on existing serialized fields.

The first pass covers core gameplay tuning configs for **Slingshot**, **Launch Impulse**, **Run Steering Control**, **Run Camera**, and **Run End Flow**. Guidance should be added only to tweakable or non-obvious serialized values. Obvious object references, purely mechanical serialization helpers, and fields whose meaning is already self-evident should not receive noisy guidance.

Simple fields should receive concise one-sentence guidance. Substantive tuning settings should use labelled paragraphs:

- `Controls:` what the value directly controls
- `Impact:` what changing the value does to play, feel, safety, or economy
- `Typical:` typical units, constraints, ranges, or examples where justified

The labelled paragraphs should be separated by blank lines in Unity tooltip text. Existing tuned serialized asset values should inform examples and ranges, but exact current values should not be copied into tooltip text by default because the Inspector already displays the current value and code-level tooltip text can drift from asset tuning.

Acceptance criteria:

- The first pass is metadata-only.
- Existing serialized field names remain unchanged.
- Existing code defaults remain unchanged.
- Existing `[Min]`, `[Range]`, validators, runtime fallback behavior, and config interfaces remain unchanged.
- No new runtime modules, editor windows, custom inspectors, packages, asmdefs, or assets are introduced.
- Tooltip text uses project glossary terms such as **Authoring Guidance**, **Pull**, **Pull Strength**, **Pull Offset**, **Launch Impulse**, **Run Steering Control**, **Run Camera Anchor**, **Lost Momentum**, **Distance Bonus**, and **Air Time Bonus**.
- Multiline tooltip text stays inline on the serialized field, matching the current field-local tooltip style.
- Verification is Rider/file problem inspection plus Unity compile.
- Tests are not added unless implementation scope expands into validators, field names, defaults, custom editor behavior, or runtime behavior.

## Unity Surfaces

- Runtime assemblies and asmdefs:
  - `Game.Gameplay` contains gameplay-level config surfaces for **Launch Impulse**, **Run Steering Control**, **Run Camera**, **Run End Flow**, and run rewards.
  - `Game.Gameplay.Slingshot` contains the **Slingshot** config surface for pull interpretation and band presentation tuning.
  - No asmdef dependency changes are expected.
  - No new runtime assembly is expected.
- Editor assemblies, windows, inspectors, importers, or menu items:
  - No custom Editor assembly is required.
  - No custom inspector, property drawer, editor menu, or importer is required.
  - Unity's built-in Inspector tooltip rendering is the delivery mechanism for the first pass.
- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:
  - Existing gameplay config ScriptableObject classes are modified with Inspector metadata only.
  - Existing gameplay config ScriptableObject assets remain the current tuning source of truth.
  - No scene, prefab, package manifest, lock file, or ProjectSettings change is expected.
  - Existing tuned assets should be consulted to shape examples and typical ranges, but asset values should not be duplicated into tooltip text by default.
- RPC/helper commands, hooks, or shell wrappers:
  - Unity AI Agent Connector remains the compile gate.
  - Rider file problem inspection remains the per-file static check after edits.
  - No new helper command or hook is required.
- Package versioning, changelog, and installation/sync behavior:
  - This is project documentation/metadata work, not package behavior work.
  - No package version bump, changelog entry, install flow, or sync behavior change is expected unless the project later formalizes documentation-only release notes.

## User Stories

1. As a reviewer, I want Inspector fields to explain what they control, so that I can understand the config without jumping into code immediately.
2. As a reviewer, I want tuning fields to explain their gameplay impact, so that I know whether changing them is likely to affect feel, difficulty, safety, or rewards.
3. As a reviewer, I want units to be visible in the Inspector guidance, so that I know whether a value is pixels, meters, centimeters, seconds, degrees, impulse, coins, or normalized fraction.
4. As a reviewer, I want typical or safe ranges where justified, so that I can judge whether a current value is reasonable.
5. As a reviewer, I want guidance to avoid stale exact asset values, so that I do not trust copied numbers that may have drifted from the actual asset.
6. As a reviewer, I want the Inspector's current field value to remain the source of the current tuning value, so that I do not compare two duplicated sources.
7. As a reviewer, I want longer guidance to be split with blank lines, so that the tooltip is scannable under time pressure.
8. As a reviewer, I want simple fields to stay concise, so that every tooltip does not become a mini document.
9. As a reviewer, I want the first pass to cover the core gameplay configs first, so that the most important tuning surface is understandable quickly.
10. As a reviewer, I want **Slingshot** pull settings explained, so that I understand how **Pull** capture and limits affect launch preparation.
11. As a reviewer, I want touch target radius explained, so that I know how it affects **Slingshot** input forgiveness.
12. As a reviewer, I want minimum and maximum pull distance explained, so that I know how weak pulls and full-depth pulls are interpreted.
13. As a reviewer, I want maximum lateral pull explained, so that I understand the authored side-to-side limit before **Pull Offset** affects launch direction.
14. As a reviewer, I want **Band** contact padding explained, so that I know it is visual clearance rather than launch power.
15. As a reviewer, I want **Band** silhouette and wrap sample counts explained, so that I know they affect **Band Shape** quality and cost, not gameplay force.
16. As a reviewer, I want **Band Release Recoil** settings explained, so that I understand visual recovery timing after release.
17. As a reviewer, I want **Launch Impulse** forward settings explained, so that I understand how minimum and maximum forward push relate to **Pull Strength**.
18. As a reviewer, I want **Pull Strength** curve guidance, so that I understand how normalized pull depth maps to launch power.
19. As a reviewer, I want lateral launch angle guidance, so that I understand how **Pull Offset** bends the accepted **Launch Impulse**.
20. As a reviewer, I want upward impulse guidance, so that I know how launch lift changes arc, airtime, and run entry.
21. As a reviewer, I want total impulse clamp toggles explained, so that I know when they are defensive bounds and when they are inactive.
22. As a reviewer, I want **Run Steering Control** range guidance, so that I understand the physical thumb displacement for full steering.
23. As a reviewer, I want **Run Steering Deadzone** guidance, so that I know how neutral jitter is filtered.
24. As a reviewer, I want **Run Steering Responsiveness** guidance, so that I understand heavier versus snappier steering feel.
25. As a reviewer, I want DPI fallback guidance, so that I know those values protect physical steering range calculations.
26. As a reviewer, I want maximum turn rate guidance, so that I understand the cap on grounded steering direction change.
27. As a reviewer, I want **Run Air Steering Control** guidance, so that I know airborne steering is deliberately weaker.
28. As a reviewer, I want minimum steer speed guidance, so that I know why steering may stop near zero speed.
29. As a reviewer, I want **Run Body Speed Sanity Guard** guidance, so that I know it is not a player-facing movement cap.
30. As a reviewer, I want **Launch Landing Stabilization** guidance, so that I understand why early post-launch lift is suppressed.
31. As a reviewer, I want **Run Steering Frame** stability guidance, so that I understand support-normal smoothing and suspect normal confirmation.
32. As a reviewer, I want **Run Camera** anchor offset guidance, so that I understand how camera-facing target height is authored.
33. As a reviewer, I want **Run Camera** response-rate guidance, so that I know how anchor smoothing affects follow feel.
34. As a reviewer, I want minimum yaw speed guidance, so that I understand why camera-facing yaw is held near low speed.
35. As a reviewer, I want **Obstacle Impact** threshold guidance, so that I know what makes obstacle contact end a **Run**.
36. As a reviewer, I want **Lost Momentum** timing guidance, so that I understand sustained lack of progress versus momentary slowdown.
37. As a reviewer, I want **Lost Momentum** speed and progress thresholds explained, so that I know how stalled runs are detected.
38. As a reviewer, I want **Run Ended Acknowledge Guard** guidance, so that I understand input gating after result presentation.
39. As a reviewer, I want **Distance Bonus** guidance, so that I understand how run distance becomes currency.
40. As a reviewer, I want **Air Time Bonus** guidance, so that I understand why ramps and unsupported travel can contribute coins.
41. As a designer, I want **Authoring Guidance** beside the field being tuned, so that I do not have to cross-reference a separate document for ordinary Inspector work.
42. As a designer, I want guidance to use project terms, so that config fields match glossary language and avoid ambiguous words like rope, power, or state.
43. As a designer, I want guidance to distinguish visual settings from gameplay settings, so that I do not mistake **Band Shape** presentation for launch force.
44. As a designer, I want guidance to distinguish defensive safety settings from player-facing tuning, so that I do not over-tune guard rails.
45. As a designer, I want current tuned assets to shape examples, so that guidance reflects the project rather than invented generic ranges.
46. As a designer, I want broad ranges only when justified by existing constraints or obvious scale, so that guidance does not imply false balance authority.
47. As a designer, I want existing validators and constraints preserved, so that documentation cannot accidentally change what values are accepted.
48. As a developer, I want this pass to avoid new abstractions, so that metadata stays close to the serialized fields it explains.
49. As a developer, I want multiline tooltip text inline on the field, so that the explanation is reviewed with the serialized value declaration.
50. As a developer, I want no shared tooltip helper for the first pass, so that simple metadata does not add indirection.
51. As a developer, I want serialized field names unchanged, so that existing assets do not need migration.
52. As a developer, I want code defaults unchanged, so that runtime fallback behavior is not affected.
53. As a developer, I want interfaces unchanged, so that existing consumers and tests do not need updates.
54. As a developer, I want no asmdef changes, so that dependency direction remains unchanged.
55. As a developer, I want no custom inspector, so that there is no Editor/runtime split to maintain for this first pass.
56. As a tester, I want Unity compile to pass after metadata edits, so that attribute formatting and strings are valid C#.
57. As a tester, I want Rider/file problem inspection to pass for edited files, so that obvious syntax and analysis issues are caught.
58. As a tester, I do not want low-signal tests for static tooltip text, so that the test suite stays focused on runtime behavior and contracts.
59. As a maintainer, I want the PRD to record why exact current asset values are not copied into tooltips, so that future edits do not reintroduce staleness.
60. As a maintainer, I want second-pass assets deferred, so that the core tuning surface is improved before less central definitions are documented.
61. As a maintainer, I want obvious serialized references deferred, so that **Authoring Guidance** remains high-signal.
62. As a maintainer, I want an ADR avoided for this change, so that lightweight Inspector metadata does not create unnecessary architecture process.
63. As a future implementer, I want the first-pass scope clearly named, so that implementation can proceed without re-opening already resolved planning questions.
64. As a future implementer, I want unresolved second-pass decisions separated from first-pass acceptance, so that the first pass can ship independently.

## Implementation Decisions

- Use **Authoring Guidance** as the canonical term. "Tooltip" is the Unity delivery mechanism, not the project concept.
- Apply **Authoring Guidance** only to tweakable or non-obvious serialized values.
- Do not add guidance to every serialized field by default.
- First pass covers five core gameplay config classes: **Slingshot** config, gameplay **Launch Impulse** config, **Run Steering Control** config, **Run Camera** config, and **Run End Flow** / run reward config.
- Second pass may cover **Upgrade**, **Economy**, **Gameplay State**, and other definition assets after the core gameplay tuning surface is done.
- Obvious scene references, UI references, component references, and self-explanatory object links stay deferred unless they prove confusing.
- The first pass is metadata-only.
- Preserve existing `[Min]`, `[Range]`, validators, runtime fallback behavior, code defaults, serialized field names, config interfaces, and public behavior.
- Do not introduce new runtime modules or deep modules for this first pass. The change does not contain enough behavior to justify a new testable abstraction.
- Do not introduce a shared tooltip helper for this first pass. Keeping guidance inline makes it easier to review beside the serialized value.
- Match the existing field-local tooltip style already present in the **Run Steering Control** config.
- Use one-sentence guidance for simple fields.
- Use labelled paragraphs for substantive settings: `Controls:`, `Impact:`, and `Typical:`.
- Separate labelled paragraphs with blank lines in tooltip text for readability.
- Use `Typical:` to describe units, known constraints, stable examples, or broad ranges when they are justified.
- Use current tuned serialized assets to inform wording, examples, and ranges.
- Do not copy exact current asset values into tooltip text by default because they can drift from the asset source of truth.
- Exact numbers are appropriate in guidance when they are stable constraints, fixed units, attribute-backed bounds, or genuinely useful examples.
- Avoid invented balance ranges. If the range is not supported by an existing constraint, asset pattern, or obvious scale, phrase guidance in terms of direction and impact instead.
- Prefer project glossary terms over casual synonyms.
- Distinguish **Pull Strength** from **Launch Impulse**.
- Distinguish **Pull Offset** and lateral launch angle from **Run Steering Control**.
- Distinguish **Band Shape**, **Band Wrap**, and **Band Release Recoil** presentation settings from launch force.
- Distinguish **Run Body Speed Sanity Guard** from player-facing speed balance.
- Distinguish **Distance Bonus** and **Air Time Bonus** from **Run Distance Display** and character presentation.
- Existing ADRs are sufficient. No new ADR is needed because this is lightweight, reversible Inspector metadata rather than a hard-to-reverse architecture choice.
- If implementation discovers a field whose guidance would require changing a validator, default, or serialized structure, pause and split that decision from the metadata pass.
- If implementation discovers outdated existing tooltip text, update it to match the resolved **Authoring Guidance** structure rather than preserving inconsistent terminology.
- If implementation discovers an existing tooltip with an exact current value phrased as a baseline, replace it when appropriate with impact/range guidance that is less likely to go stale.
- The PRD intentionally does not prescribe exact tooltip copy for every field. The implementer should write field-specific guidance using the agreed structure and glossary terms.

## Testing Decisions

- Good tests verify external behavior and contracts, not implementation details.
- Static tooltip text does not create meaningful runtime behavior to test in the first pass.
- Do not add EditMode or PlayMode tests for metadata-only `[Tooltip]` or `[Header]` changes.
- Run Rider/file problem inspection on edited C# files after changes.
- Run Unity compile through Unity AI Agent Connector after changes.
- Do not run gameplay tests unless implementation changes validators, serialized field names, defaults, public interfaces, custom editor behavior, or runtime logic.
- If validators change, add or update focused EditMode validator tests.
- If serialized field names change, stop first because that is a save/asset compatibility decision and outside this PRD.
- If defaults or fallback behavior change, add focused EditMode tests covering resolved config values.
- If custom Inspector or property drawer behavior is introduced in a future pass, add Editor tests or manual Unity Inspector smoke checks as appropriate.
- If assets are edited in a future pass, inspect serialized YAML diffs and use Unity asset refresh/reserialize only when needed.
- Manual review for the metadata pass should check that tooltip text is readable in the Unity Inspector, especially multiline spacing and line length.
- Manual review should spot-check that guidance does not claim an exact current value that differs from the Inspector field value.
- Manual review should spot-check that guidance uses glossary terms consistently.
- Prior art: existing gameplay PRDs favor EditMode tests for behavior and PlayMode tests for scene/engine integration; this PRD intentionally does neither because it does not change behavior.

## Release and Compatibility

- Unity version assumption: the project targets Unity 6000.3.x.
- No package dependency changes are expected.
- No assembly definition changes are expected.
- No scene, prefab, ScriptableObject asset, package manifest, lock file, or ProjectSettings changes are expected for the first pass.
- No save data, economy storage, upgrade ownership, or gameplay state migration is expected.
- Backward compatibility risk is low if serialized field names and asset values remain unchanged.
- Main compatibility risk is accidental asset/serialization churn from unrelated Unity edits; implementation should keep diffs scoped to C# metadata.
- No package version or changelog impact is expected unless the project later tracks documentation-only changes in release notes.

## Out of Scope

- Implementing gameplay behavior changes.
- Changing serialized field names.
- Changing code defaults.
- Changing existing `[Min]`, `[Range]`, validation, clamping, or fallback behavior.
- Adding a custom Inspector, property drawer, tooltip renderer, editor window, or authoring tool.
- Adding new packages, asmdefs, assets, scenes, prefabs, or ProjectSettings.
- Adding tests for static tooltip text.
- Documenting every serialized field or reference.
- First-pass coverage for **Upgrade**, **Economy**, **Gameplay State**, pickup definitions, UI prefabs, or scene references.
- Rebalancing **Slingshot**, **Launch Impulse**, **Run Steering Control**, **Run Camera**, **Run End Flow**, **Distance Bonus**, or **Air Time Bonus** values.
- Embedding exact current asset values into every tooltip.
- Creating an ADR for this metadata-only change.
- Publishing issues or remote tracker artifacts.

## Further Notes

The current first-pass asset values are useful context for authoring copy, but the Inspector field itself remains the source of truth for the current value. The implementer should use asset-tuned values to avoid generic or misleading guidance, while phrasing the tooltip so it remains true if designers retune the asset later.

The **Run Steering Control** config already contains partial inline tooltip usage. The first pass should standardize and improve it rather than treating that file as untouched.

The most important review constraint is signal-to-noise. **Authoring Guidance** should help a reviewer decide whether to tweak a value and understand the likely impact. It should not restate the field name, repeat obvious serialization facts, or explain unrelated implementation details.
